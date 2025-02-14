using ImGuiNET;

namespace TofuEngine;

public class TofuImGuiSetItemSpacingGuard : IDisposable
{
    private Vector2 _spacingBefore;

    public TofuImGuiSetItemSpacingGuard(Vector2 spacingBefore, Vector2 currentSpacing)
    {
        _spacingBefore = spacingBefore;
        ImGui.GetStyle().ItemSpacing = currentSpacing;
    }

    public void Dispose()
    {
        ImGui.GetStyle().ItemSpacing = _spacingBefore;
    }
}