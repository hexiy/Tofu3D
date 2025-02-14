using ImGuiNET;

namespace TofuEngine;

public class TofuImGuiSetFramePaddingGuard : IDisposable
{
    private Vector2 _paddingBefore;

    public TofuImGuiSetFramePaddingGuard(float newPadding) : this(new Vector2(newPadding))
    {
    }

    public TofuImGuiSetFramePaddingGuard(Vector2 newPadding)
    {
        _paddingBefore = ImGui.GetStyle().FramePadding;
        ImGui.GetStyle().FramePadding = newPadding;
    }

    public void Dispose()
    {
        ImGui.GetStyle().FramePadding = _paddingBefore;
    }
}