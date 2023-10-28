using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerInt : InspectorFieldDrawable<int>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        int fieldValue = GetValue(info, componentInspectorData);


        if (ImGui.DragInt("", ref fieldValue)) SetValue(info, componentInspectorData, fieldValue);
    }
}