using ImGuiNET;

namespace TofuEngine;

public class TofuImGuiSetItemSpacingGuard : IDisposable
{
    private Vector2 _spacingBefore;

    public TofuImGuiSetItemSpacingGuard(float? oldSpacingX = null, float? oldSpacingY = null,
        float? newSpacingX = null,
        float? newSpacingY = null)
    {
        _spacingBefore = (oldSpacingX.HasValue == false && oldSpacingY.HasValue == false)
            ? ImGui.GetStyle().ItemSpacing
            : new Vector2(oldSpacingX ?? ImGui.GetStyle().ItemSpacing.X,
                oldSpacingY ?? ImGui.GetStyle().ItemSpacing.Y);

        ImGui.GetStyle().ItemSpacing =
            new Vector2(newSpacingX ?? _spacingBefore.X, newSpacingY ?? _spacingBefore.Y);
    }

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