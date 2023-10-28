using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerAction : InspectorFieldDrawable<Action>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        Action action = GetValue(info, componentInspectorData);
        ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.Text]);
        if (ImGui.Button($"> {info.Name} <",
                new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight())))
            action?.Invoke();

        ImGui.PopStyleColor(1);
    }
}