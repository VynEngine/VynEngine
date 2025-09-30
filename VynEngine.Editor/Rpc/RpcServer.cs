using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Photino.NET;

namespace VynEngine.Editor.Rpc;

/// <summary>
/// A simple RPC server that allows calling C# methods from JavaScript in a Photino.NET application.
/// Services and methods are registered via attributes, and communication is done using JSON messages.
/// </summary>
// ReSharper disable NotAccessedPositionalProperty.Local
internal static class RpcServer
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        WriteIndented = false
    };

    private sealed record RpcCall(string Type, string Dir, string Id, string Svc, string Method, JsonElement? Args);
    private sealed record RpcResult(string Type, string Dir, string Id, bool Ok, object? Result, RpcError? Error);
    private sealed record RpcError(string Code, string Message, object? Data);

    private sealed class Endpoint
    {
        public object Target { get; init; } = null!;
        public MethodInfo Method { get; init; } = null!;
        public ParameterInfo[] Params { get; init; } = [];
        public bool ReturnsTask { get; init; }
        public bool ReturnsTaskOfT { get; init; }
        public Type? TaskResultType { get; init; }
    }

    private static readonly ConcurrentDictionary<(string svc, string method), Endpoint> _map = new(new IgnoreCaseMapComparer());

    /// <summary>
    /// Registers all RPC services and their methods from the specified assembly.
    /// The services are identified by the [RpcService] attribute and methods by the [RpcMethod] attribute.
    /// </summary>
    /// <param name="asm">The assembly to scan for RPC services.</param>
    public static void RegisterServicesFromAssembly(Assembly asm)
    {
        var svcTypes = asm.GetTypes()
            .Where(t => t.GetCustomAttribute<RpcServiceAttribute>() is not null && !t.IsAbstract);

        foreach (var t in svcTypes)
        {
            var svcAttr = t.GetCustomAttribute<RpcServiceAttribute>()!;
            var target = Activator.CreateInstance(t)!;

            foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                var mm = m.GetCustomAttribute<RpcMethodAttribute>();
                if (mm is null) continue;
                var name = string.IsNullOrWhiteSpace(mm.Name) ? m.Name : mm.Name!;
                var (retTask, retTaskT, taskT) = AnalyzeReturn(m.ReturnType);

                _map[(svcAttr.Name, name)] = new Endpoint
                {
                    Target = target,
                    Method = m,
                    Params = m.GetParameters(),
                    ReturnsTask = retTask,
                    ReturnsTaskOfT = retTaskT,
                    TaskResultType = taskT
                };
            }
        }
    }

    /// <summary>
    /// Attaches the RPC server to a Photino window, enabling it to handle incoming RPC calls from the JavaScript side.
    /// </summary>
    /// <param name="window">The Photino window to attach the RPC server to.</param>
    /// <exception cref="InvalidOperationException">Thrown if the method invocation fails.</exception>
    public static void Attach(PhotinoWindow window)
    {
        window.RegisterWebMessageReceivedHandler(async void (sender, json) =>
        {
            try
            {
                var call = JsonSerializer.Deserialize<RpcCall>(json, JsonOpts);
                if (call is null || call.Type != "vyn.rpc" || call.Dir != "js->cs") return;

                var win = (PhotinoWindow)sender!;
                var key = (call.Svc, call.Method);
                if (!_map.TryGetValue(key, out var ep))
                {
                    await Send(win, new RpcResult("vyn.rpc", "cs->js", call.Id, false, null,
                        new RpcError("method_not_found", $"RPC {call.Svc}.{call.Method} not found", null)));
                    return;
                }

                var args = BindArgs(ep.Params, call.Args);
                object? result;

                if (ep.ReturnsTask)
                {
                    if (ep.Method.Invoke(ep.Target, args) is not Task taskObj) 
                        throw new InvalidOperationException("Method returned null Task");

                    await taskObj.ConfigureAwait(false);

                    if (ep.ReturnsTaskOfT)
                    {
                        var resProp = taskObj.GetType().GetProperty("Result");
                        result = resProp?.GetValue(taskObj);
                    }
                    else result = null;
                }
                else
                {
                    result = ep.Method.Invoke(ep.Target, args);
                }

                await Send(win, new RpcResult("vyn.rpc", "cs->js", call.Id, true, result, null));
            }
            catch (TargetInvocationException tex)
            {
                await SafeSendError((PhotinoWindow)sender!, tex.InnerException ?? tex, "invoke_failed");

#if DEBUG
                throw;
#endif
            }
            catch (Exception ex)
            {
                await SafeSendError((PhotinoWindow)sender!, ex, "server_error");
                
#if DEBUG
                throw;
#endif
            }
        });
    }

    /// <summary>
    /// Emits a custom event to the JavaScript side. This is a one-way notification and does not expect a response.
    /// </summary>
    /// <param name="w">The Photino window to send the event to.</param>
    /// <param name="svc">The service name.</param>
    /// <param name="name">The event name.</param>
    /// <param name="data">The event data (optional).</param>
    public static void Emit(PhotinoWindow w, string svc, string name, object? data = null)
    {
        var evt = new
        {
            type = "vyn.evt",
            dir  = "cs->js",
            svc,
            name,
            data
        };
        w.SendWebMessage(JsonSerializer.Serialize(evt, JsonOpts));
    }

    private static (bool isTask, bool isTaskT, Type? taskT) AnalyzeReturn(Type rt)
    {
        if (rt == typeof(Task)) return (true, false, null);
        if (rt.IsGenericType && rt.GetGenericTypeDefinition() == typeof(Task<>))
            return (true, true, rt.GetGenericArguments()[0]);
        return (false, false, null);
    }

    private static object?[] BindArgs(ParameterInfo[] ps, JsonElement? argsEl)
    {
        if (ps.Length == 0) return [];
        if (argsEl is null || argsEl.Value.ValueKind == JsonValueKind.Null)
            return ps.Select(_ => Type.Missing).ToArray();

        if (argsEl.Value.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("RPC args must be an array");

        var arr = argsEl.Value.EnumerateArray().ToArray();
        var res = new object?[ps.Length];

        for (var i = 0; i < ps.Length; i++)
        {
            var pi = ps[i];
            if (i >= arr.Length || arr[i].ValueKind == JsonValueKind.Null)
            {
                res[i] = pi.HasDefaultValue ? pi.DefaultValue : (pi.ParameterType.IsValueType ? Activator.CreateInstance(pi.ParameterType) : null);
                continue;
            }
            res[i] = JsonSerializer.Deserialize(arr[i].GetRawText(), pi.ParameterType, JsonOpts);
        }
        return res;
    }

    private static async Task Send(PhotinoWindow w, RpcResult msg)
    {
        var json = JsonSerializer.Serialize(msg, JsonOpts);
        await Task.Yield();
        await w.SendWebMessageAsync(json);
    }

    private static Task SafeSendError(PhotinoWindow w, Exception ex, string code)
    {
        var err = new RpcResult("vyn.rpc", "cs->js", Guid.NewGuid().ToString("n"), false, null,
            new RpcError(code, ex.Message, new { ex.GetType().FullName, ex.StackTrace }));
        w.SendWebMessage(JsonSerializer.Serialize(err, JsonOpts));
        return Task.CompletedTask;
    }
    
    private class IgnoreCaseMapComparer : IEqualityComparer<(string svc, string method)>
    {
        public bool Equals((string svc, string method) x, (string svc, string method) y)
            => string.Equals(x.svc, y.svc, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.method, y.method, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string svc, string method) obj)
            => HashCode.Combine(obj.svc.ToLowerInvariant(), obj.method.ToLowerInvariant());
    }
}