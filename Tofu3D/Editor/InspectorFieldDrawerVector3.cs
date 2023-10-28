using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerVector3 : InspectorFieldDrawable<Vector3>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        System.Numerics.Vector3 value = GetValue(info, componentInspectorData);

        if (ImGui.DragFloat3("", ref value, 0.01f)) SetValue(info, componentInspectorData, value);
    }
}