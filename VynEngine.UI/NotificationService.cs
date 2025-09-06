using System.Numerics;
using ImGuiNET;

namespace VynEngine.UI;

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// The notification service manages and displays transient toast notifications to the user. It supports different
/// notification types (info, success, warning, error) and allows for customization of the title, message, and duration
/// of each notification. Notifications are displayed in the top-right corner of the application window and automatically
/// fade out after a specified duration, or can be dismissed manually by the user.
/// </summary>
public sealed class NotificationService
{
    private readonly List<Toast> _toasts = [];
    private readonly Queue<Action> _uiActions = new();

    /// <summary>Adds a toast. If durationSeconds is null or &lt;= 0, it stays until dismissed.</summary>
    public void Show(string? title, string? message, NotificationType type = NotificationType.Info,
        float? durationSeconds = 4f)
    {
        Application.Instance?.InvokeOnUI(() =>
        {
            _toasts.Add(new Toast(title ?? string.Empty, message ?? string.Empty, type, durationSeconds));
        });
    }

    /// <summary>Clears all current toasts.</summary>
    public void Clear() => Application.Instance?.InvokeOnUI(() => _toasts.Clear());

    internal void Render()
    {
        if (_toasts.Count == 0) return;

        var vp = ImGui.GetMainViewport();
        var io = ImGui.GetIO();
        var dt = MathF.Max(io.DeltaTime, 1f / 240f);

        const float margin = 16f;
        const float maxWidth = 360f;
        var y = margin;

        for (var i = _toasts.Count - 1; i >= 0; i--)
        {
            var t = _toasts[i];
            t.Update(dt);

            if (t.IsFinished)
            {
                _toasts.RemoveAt(i);
                continue;
            }

            var (bg, border, text, accent) = ColorsFor(t.Type);

            ImGui.SetNextWindowViewport(vp.ID);
            ImGui.SetNextWindowPos(vp.Pos + new Vector2(vp.Size.X - margin, y), ImGuiCond.Always, new Vector2(1, 0));
            ImGui.SetNextWindowBgAlpha(0.92f * t.Alpha);
            ImGui.SetNextWindowSizeConstraints(new Vector2(0, 0), new Vector2(maxWidth, float.MaxValue));

            ImGui.PushStyleColor(ImGuiCol.WindowBg, bg with { W = 0.92f * t.Alpha });
            ImGui.PushStyleColor(ImGuiCol.Border, border with { W = 0.90f * t.Alpha });
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 6f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(12, 10));

            VynStyle.Apply();

            const ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize |
                                           ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoNav;

            ImGui.Begin($"##toast_{t.Id:N}", flags);

            var titlePos = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddText(titlePos + new Vector2(1, 1), U32(new Vector4(0, 0, 0, 0.6f)), t.Title);
            ImGui.PushStyleColor(ImGuiCol.Text, text with { W = t.Alpha });
            ImGui.TextUnformatted(t.Title);
            ImGui.PopStyleColor();

            const float btn = 14f;
            var winPos = ImGui.GetWindowPos();
            var winSz = ImGui.GetWindowSize();
            ImGui.SetCursorScreenPos(winPos + new Vector2(winSz.X - btn - 6f, 6f));
            if (ImGui.InvisibleButton($"##close_{t.Id:N}", new Vector2(btn, btn)))
                t.Dismiss();
            ImGui.GetWindowDrawList().AddText(ImGui.GetItemRectMin() - new Vector2(2, 2), 0xFFFFFFFF, "×");

            if (!string.IsNullOrEmpty(t.Message))
            {
                var msgPos = ImGui.GetCursorScreenPos();
                ImGui.GetWindowDrawList()
                    .AddText(msgPos + new Vector2(1, 1), U32(new Vector4(0, 0, 0, 0.5f)), t.Message);
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.92f, 0.97f, 0.97f, t.Alpha));
                ImGui.TextWrapped(t.Message);
                ImGui.PopStyleColor();
            }

            if (t.DurationSeconds is var dur and > 0f)
            {
                var frac = Math.Clamp(1f - (t.Elapsed / dur), 0f, 1f);
                var p0 = ImGui.GetWindowPos();
                var sz = ImGui.GetWindowSize();
                const float h = 3f;
                ImGui.GetWindowDrawList().AddRectFilled(
                    p0 + new Vector2(sz.X * (1f - frac), sz.Y - h),
                    p0 + new Vector2(sz.X, sz.Y),
                    U32(accent with { W = 0.9f * t.Alpha })
                );
            }

            var thisSize = ImGui.GetWindowSize();
            ImGui.End();

            ImGui.PopStyleVar(3);
            ImGui.PopStyleColor(2);

            y += thisSize.Y + 8f;
        }

        return;

        (Vector4 bg, Vector4 border, Vector4 text, Vector4 accent) ColorsFor(NotificationType t) => t switch
        {
            NotificationType.Success => (new Vector4(0.05f, 0.12f, 0.09f, 1f), new Vector4(0.00f, 0.85f, 0.55f, 1f),
                new Vector4(0.90f, 0.98f, 0.96f, 1f), new Vector4(0.00f, 0.95f, 0.60f, 1f)),
            NotificationType.Warning => (new Vector4(0.12f, 0.11f, 0.05f, 1f), new Vector4(0.95f, 0.85f, 0.25f, 1f),
                new Vector4(0.98f, 0.98f, 0.90f, 1f), new Vector4(0.95f, 0.85f, 0.25f, 1f)),
            NotificationType.Error => (new Vector4(0.14f, 0.06f, 0.06f, 1f), new Vector4(0.95f, 0.35f, 0.35f, 1f),
                new Vector4(0.99f, 0.94f, 0.94f, 1f), new Vector4(0.95f, 0.35f, 0.35f, 1f)),
            _ => (new Vector4(0.06f, 0.08f, 0.10f, 1f), new Vector4(0.00f, 0.80f, 0.90f, 1f), new Vector4(0.92f, 0.98f, 0.99f, 1f),
                new Vector4(0.00f, 0.85f, 0.95f, 1f)),
        };

        static uint U32(Vector4 c)
        {
            byte r = (byte)(c.X * 255), g = (byte)(c.Y * 255), b = (byte)(c.Z * 255), a = (byte)(c.W * 255);
            return (uint)(a << 24 | b << 16 | g << 8 | r);
        }
    }

    private class Toast(string title, string message, NotificationType type, float? duration)
    {
        public readonly Guid Id = Guid.NewGuid();
        public readonly string Title = title;
        public readonly string Message = message;
        public readonly NotificationType Type = type;
        public readonly float DurationSeconds = duration.GetValueOrDefault(4f);
        
        public float Elapsed;
        public float Alpha;
        
        private bool _dismissing;

        public bool IsFinished => _dismissing && Alpha <= 0f;

        public void Update(float dt)
        {
            Elapsed += dt;

            if (DurationSeconds > 0 && Elapsed >= DurationSeconds)
                _dismissing = true;

            var target = _dismissing ? 0f : 1f;
            Alpha = Lerp(Alpha, target, 1f - MathF.Pow(0.001f, dt));
            if (target == 0f && Alpha < 0.02f) Alpha = 0f;
        }


        public void Dismiss() => _dismissing = true;

        private static float Lerp(float a, float b, float t) => a + (b - a) * Math.Clamp(t, 0, 1);
    }
}