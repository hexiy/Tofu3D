using System.Linq;
using System.Reflection;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerInt : InspectorFieldDrawable<int>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        int fieldValue = GetValue(info, componentInspectorData);

        info.GetCustomAttribute<Slider>(out Slider? sliderAttrib);

        if (sliderAttrib != null)
        {
            if (ImGui.SliderInt("", ref fieldValue, sliderAttrib.MinValue, sliderAttrib.MaxValue))
            {
                SetValue(info, componentInspectorData, fieldValue);
            }
        }
        else
        {
            if (ImGui.DragInt("", ref fieldValue))
            {
                SetValue(info, componentInspectorData, fieldValue);
            }
        }
    }
}