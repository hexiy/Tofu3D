using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerAction : InspectorFieldDrawable<Action>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        Action? action = GetValue(info, componentInspectorData);
        ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.Text]);
        if (ImGui.Button($"> {info.Name} <",
                new Vector2(TofuImGui.GetContentRegionAvailWithPadding().X,
                    ImGui.GetFrameHeight())))
        {
            action?.Invoke();
        }

        ImGui.PopStyleColor(1);
    }
}