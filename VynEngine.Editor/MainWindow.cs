using ImGuiNET;
using VynEngine.UI;

namespace VynEngine.Editor;

public class MainWindow : Window
{
    public MainWindow()
    {
        WindowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove |
                      ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoBringToFrontOnFocus |
                      ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoSavedSettings |
                      ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse |
                      ImGuiWindowFlags.MenuBar;
    }

    protected override void OnUI()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Exit"))
                {
                    Environment.Exit(0);
                }

                if (ImGui.MenuItem("SUCCESS"))
                {
                    Notifications.Show("Success", "This is a success notification.", NotificationType.Success);
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}