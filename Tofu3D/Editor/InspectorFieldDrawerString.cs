using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerString : InspectorFieldDrawable<string>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        string fieldValue = GetValue(info, componentInspectorData);

        if (ImGui.InputTextMultiline("", ref fieldValue, 100,
                new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, 200)))
        {
            SetValue(info,componentInspectorData,fieldValue);
        }
    }
}