using ImGuiNET;

namespace Tofu3D;

public static class TofuGUI
{
    public static Vector2 ButtonSize => new Vector2(150, 35);

    public static void CenterNextItem(Vector2 size)
    {
        Vector2 currentCursorPos = ImGui.GetCursorPos();
        ImGui.SetCursorPos(currentCursorPos - size / 2);
    }

    public static bool Button(string text, Vector2? size = null)
    {
        Vector2 sizeValue = size ?? ButtonSize;
        Vector2 currentCursorPos = ImGui.GetCursorPos();
        ImGui.SetCursorPos(currentCursorPos - sizeValue / 2);
        bool clicked = ImGui.Button(text, sizeValue);
        return clicked;
    }

    public static void Text(string text)
    {
        Vector2 size = ImGui.CalcTextSize(text);
        Vector2 currentCursorPos = ImGui.GetCursorPos();
        ImGui.SetCursorPos(currentCursorPos - size / 2);
        ImGui.Text(text);
    }
}