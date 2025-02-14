using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerBool : InspectorFieldDrawable<bool>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        ImGui.SameLine(ImGui.GetWindowWidth() - ImGui.GetContentRegionAvail().X / 2);

        bool fieldValue = (bool)info.GetValue(componentInspectorData.Inspectable);

        if (ImGui.Checkbox("", ref fieldValue))
        {
            info.SetValue(componentInspectorData.Inspectable, fieldValue);
            // .QueueRefresh(componentInspectorData);
        }
    }
}