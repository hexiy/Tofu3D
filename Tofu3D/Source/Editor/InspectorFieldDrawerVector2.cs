using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerVector2 : InspectorFieldDrawable<Vector2>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        System.Numerics.Vector2 value = GetValue(info, componentInspectorData);

        if (ImGui.DragFloat2("", ref value, 0.01f))
        {
            SetValue(info, componentInspectorData, value);
        }
    }
}