using System.Numerics;
using ImGuiNET;

namespace VynEngine.UI;

internal static class VynStyle
{
    internal static void Apply()
    {
        var style = ImGui.GetStyle();
        var colors = style.Colors;

        // Grundpalette
        Vector4 bg = new(0.06f, 0.07f, 0.08f, 1.00f); // fast schwarz
        Vector4 bgDark = new(0.04f, 0.05f, 0.06f, 1.00f);
        Vector4 neon = new(0.0f, 0.95f, 0.6f, 1.0f); // Vyn-Grün
        Vector4 neonDim = new(0.0f, 0.75f, 0.5f, 1.0f); // etwas dunkler
        Vector4 accent = new(0.0f, 0.85f, 0.95f, 1.0f); // Cyan für Hover

        // Fenster
        colors[(int)ImGuiCol.WindowBg] = bg;
        colors[(int)ImGuiCol.ChildBg] = bgDark;
        colors[(int)ImGuiCol.PopupBg] = bgDark with { W = 0.98f };

        // Rahmen
        colors[(int)ImGuiCol.Border] = neonDim with { W = 0.3f };
        colors[(int)ImGuiCol.BorderShadow] = Vector4.Zero;

        // Text
        colors[(int)ImGuiCol.Text] = new Vector4(0.90f, 0.95f, 0.95f, 1.0f);
        colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.55f, 0.55f, 1.0f);

        // Header (CollapsingHeader, TreeNode, Selectable)
        colors[(int)ImGuiCol.Header] = neonDim with { W = 0.25f };
        colors[(int)ImGuiCol.HeaderHovered] = accent with { W = 0.6f };
        colors[(int)ImGuiCol.HeaderActive] = neon;

        // Buttons
        colors[(int)ImGuiCol.Button] = neonDim with { W = 0.25f };
        colors[(int)ImGuiCol.ButtonHovered] = accent with { W = 0.8f };
        colors[(int)ImGuiCol.ButtonActive] = neon;

        // Frame (InputText, Slider, Checkbox)
        colors[(int)ImGuiCol.FrameBg] = bgDark with { W = 0.9f };
        colors[(int)ImGuiCol.FrameBgHovered] = accent with { W = 0.25f };
        colors[(int)ImGuiCol.FrameBgActive] = neonDim with { W = 0.5f };

        // Tabs
        colors[(int)ImGuiCol.Tab] = neonDim with { W = 0.2f };
        colors[(int)ImGuiCol.TabHovered] = accent with { W = 0.7f };
        colors[(int)ImGuiCol.TabSelected] = neon with { W = 0.9f };
        colors[(int)ImGuiCol.TabDimmed] = bgDark;
        colors[(int)ImGuiCol.TabDimmedSelected] = neonDim with { W = 0.4f };

        // Title
        colors[(int)ImGuiCol.TitleBg] = bgDark;
        colors[(int)ImGuiCol.TitleBgActive] = neonDim with { W = 0.6f };
        colors[(int)ImGuiCol.TitleBgCollapsed] = bg;

        // Slider/Scrollbar/ResizeGrip
        colors[(int)ImGuiCol.SliderGrab] = neonDim;
        colors[(int)ImGuiCol.SliderGrabActive] = neon;
        colors[(int)ImGuiCol.ScrollbarGrab] = neonDim with { W = 0.4f };
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = accent with { W = 0.7f };
        colors[(int)ImGuiCol.ScrollbarGrabActive] = neon;

        colors[(int)ImGuiCol.ResizeGrip] = neonDim with { W = 0.3f };
        colors[(int)ImGuiCol.ResizeGripHovered] = accent with { W = 0.8f };
        colors[(int)ImGuiCol.ResizeGripActive] = neon;

        // Tabellen
        colors[(int)ImGuiCol.TableHeaderBg] = bgDark;
        colors[(int)ImGuiCol.TableBorderStrong] = neonDim with { W = 0.3f };
        colors[(int)ImGuiCol.TableBorderLight] = neonDim with { W = 0.1f };
        colors[(int)ImGuiCol.TableRowBg] = bg with { W = 0.0f };
        colors[(int)ImGuiCol.TableRowBgAlt] = bg with { W = 0.06f };

        // Style Tweaks
        style.WindowRounding = 6.0f;
        style.FrameRounding = 4.0f;
        style.ScrollbarRounding = 9.0f;
        style.GrabRounding = 4.0f;
        style.TabRounding = 4.0f;

        style.WindowBorderSize = 1.0f;
        style.FrameBorderSize = 1.0f;

        style.WindowPadding = new Vector2(8, 6);
        style.FramePadding = new Vector2(6, 4);
        style.ItemSpacing = new Vector2(6, 6);
    }
}