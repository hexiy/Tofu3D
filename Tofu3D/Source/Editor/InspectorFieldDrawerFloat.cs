using System.Linq;
using System.Reflection;
using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerFloat : InspectorFieldDrawable<float>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        float fieldValue = GetValue(info, componentInspectorData);

        info.GetCustomAttribute<SliderF>(out SliderF? sliderAttrib);

        if (sliderAttrib != null)
        {
            if (ImGui.SliderFloat("", ref fieldValue, sliderAttrib.MinValue, sliderAttrib.MaxValue))
            {
                SetValue(info, componentInspectorData, fieldValue);
            }
        }
        else
        {
            if (ImGui.DragFloat("", ref fieldValue, 0.01f, float.NegativeInfinity, float.PositiveInfinity,
                    "%.05f"))
            {
                SetValue(info, componentInspectorData, fieldValue);
            }
        }
    }
}