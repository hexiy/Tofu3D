using ImGuiNET;

namespace Tofu3D;

public static class TofuImGui
{
    public static Vector2 GetCursorScreenPos() =>
        new System.Numerics.Vector2(ImGui.GetCursorScreenPos().X / Tofu.Window.MonitorScale,
            Tofu.Window.WindowSize.Y -
            ImGui.GetCursorScreenPos().Y / Tofu.Window.MonitorScale); // * new Vector2(-1, 1);

    /// <summary>
    /// Returns true only on mouse released
    /// </summary>
    /// <param name="payloadTag"></param>
    /// <returns></returns>
    public static bool PayloadHasBeenDropped(string payloadTag)
    {
        unsafe
        {
            if (ImGui.AcceptDragDropPayload(payloadTag, ImGuiDragDropFlags.None).NativePtr != (ImGuiPayloadPtr)0)
            {
                return true;
            }

            return false;
        }
    }
}