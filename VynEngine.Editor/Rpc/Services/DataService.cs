using VynEngine.Editor.Data;

namespace VynEngine.Editor.Rpc.Services;

/// <summary>
/// The data service is responsible for managing application data, such as user preferences and settings.
/// </summary>
[RpcService("data")]
internal sealed class DataService
{
    /// <summary>
    /// Returns the full path to a file in the data folder, creating any necessary subdirectories.
    /// </summary>
    [RpcMethod]
    public string GetFile(string[] parts)
    {
        return DataFolder.GetFilePath(parts);
    }

    /// <summary>
    /// Returns true if the specified file exists in the data folder, false otherwise.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    [RpcMethod]
    public bool Exists(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// Returns the contents of the specified file as a string. If the file does not exist, returns an empty string.
    /// If the file cannot be read, null is returned.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    [RpcMethod]
    public string? Read(string path)
    {
        try
        {
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }
        catch (Exception ex)
        {
            Program.Log.Error(ex, "Failed to read file: {Path}", path);
            return null;
        }
    }

    /// <summary>
    /// Writes the specified content to the file at the given path, creating any necessary directories.
    /// If the file already exists, it will be overwritten. Returns true if the write operation was successful,
    /// false otherwise.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <param name="content">The content to write to the file.</param>
    [RpcMethod]
    public bool Write(string path, string content)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (dir is not null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(path, content);
            return true;
        }
        catch (Exception ex)
        {
            Program.Log.Error(ex, "Failed to write file: {Path}", path);
            return false;
        }
    }
}