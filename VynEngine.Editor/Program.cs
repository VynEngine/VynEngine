using Silk.NET.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VynEngine.UI;

namespace VynEngine.Editor;

internal class Program
{
    internal static MainWindow MainWindow { get; private set; } = null!;
    
    private static void Main(string[] args)
    {
        var icon = LoadFromEmbeddedResource("logo.png");
        var app = new Application();
        app.Icon = icon;
        app.Chrome.Enabled = true;
        app.AddWindow(MainWindow = new MainWindow());
        app.Start();
    }
    
    private static RawImage? LoadFromEmbeddedResource(string resourceName)
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var bytes = ms.ToArray();
        var img = Image.Load<Rgba32>(bytes);
        var bytes2 = new byte[4 * img.Width * img.Height];
        img.CopyPixelDataTo(bytes2);
        return new RawImage(img.Width, img.Height, bytes2);
    }
}