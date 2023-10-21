using System.Linq;
using System.Reflection;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerFloat : InspectorFieldDrawable<float>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        float fieldValue = GetValue(info, componentInspectorData);

        SliderF sliderAttrib = null;
        List<CustomAttributeData> a = info.FieldOrPropertyType.CustomAttributes.ToList();
        for (int i = 0; i < info.CustomAttributes.Count(); i++)
        {
            if (info.CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(SliderF))
            {
                FieldInfo fieldType = componentInspectorData.Inspectable.GetType().GetField(info.Name);
                if (fieldType != null)
                {
                    sliderAttrib = fieldType.GetCustomAttribute<SliderF>();
                }
                else
                {
                    PropertyInfo propertyType =
                        componentInspectorData.Inspectable.GetType().GetProperty(info.Name);
                    sliderAttrib = propertyType.GetCustomAttribute<SliderF>();
                }
            }
        }

        if (sliderAttrib != null)
        {
            if (ImGui.SliderFloat("", ref fieldValue, sliderAttrib.MinValue, sliderAttrib.MaxValue))
            {
                SetValue(info,componentInspectorData,fieldValue);
            }
        }
        else
        {
            if (ImGui.DragFloat("", ref fieldValue, 0.01f, float.NegativeInfinity, float.PositiveInfinity,
                    "%.05f"))
            {
                SetValue(info,componentInspectorData,fieldValue);
            }
        }
    }
}