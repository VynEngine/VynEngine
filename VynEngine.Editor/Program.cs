using VynEngine.UI;

namespace VynEngine.Editor;

internal class Program
{
    private static void Main(string[] args)
    {
        var app = new Application();
        app.Chrome.Enabled = true;
        var main = new TestWindow();
        app.AddWindow(main);
        app.Start();
    }
}