using ImGuiNET;

namespace TofuEngine;

public class TofuImGuiSetWindowPaddingGuard : IDisposable
{
    private Vector2 _paddingBefore;

    public TofuImGuiSetWindowPaddingGuard(Vector2 paddingBefore, Vector2 currentPadding)
    {
        _paddingBefore = paddingBefore;
        ImGui.GetStyle().WindowPadding = currentPadding;
    }

    public void Dispose()
    {
        ImGui.GetStyle().WindowPadding = _paddingBefore;
    }
}