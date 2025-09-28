using IOPath = System.IO.Path;

namespace VynEngine.Editor.Data;

/// <summary>
/// The data folder class is a utility class giving access to VynEngine's data folder in a platform-independent way.
/// Most data written to the data folder are user preferences. It uses a TOML format for easy editing.
/// </summary>
internal static class DataFolder
{
    /// <summary>
    /// The data folder path for the current user.
    /// </summary>
    internal static string Path => IOPath.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Vyn");

    static DataFolder()
    {
        Directory.CreateDirectory(Path);
    }

    /// <summary>
    /// Returns the full path to a subdirectory in the data folder, creating it if it doesn't exist.
    /// </summary>
    /// <param name="parts">The parts of the subdirectory path.</param>
    /// <returns>The full path to the subdirectory.</returns>
    internal static string GetSubDirectory(params string[] parts)
    {
        var path = IOPath.Join(new[] { Path }.Concat(parts).ToArray());
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// Returns the full path to a file in the data folder, creating any necessary subdirectories.
    /// </summary>
    /// <param name="parts">The parts of the file path, with the last part being the filename.</param>
    /// <returns>The full path to the file.</returns>
    /// <exception cref="ArgumentException">Thrown if the filename is empty or whitespace.</exception>
    internal static string GetFilePath(params string[] parts)
    {
        var filename = parts.Last();
        if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentException("Filename cannot be empty", nameof(parts));
        var dirParts = parts.Length > 1 ? parts[..^1] : [];
        var dir = GetSubDirectory(dirParts);
        return IOPath.Join(dir, filename);
    }
}