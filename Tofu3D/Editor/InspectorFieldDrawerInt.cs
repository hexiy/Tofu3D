using System.Linq;
using System.Reflection;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerInt : InspectorFieldDrawable<int>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        int fieldValue = GetValue(info, componentInspectorData);

        Slider? sliderAttrib = null;
        for (int i = 0; i < info.CustomAttributes.Count(); i++)
        {
            if (info.CustomAttributes.ElementAtOrDefault(i)?.AttributeType == typeof(Slider))
            {
                FieldInfo? fieldType = componentInspectorData.Inspectable.GetType().GetField(info.Name);
                if (fieldType != null)
                {
                    sliderAttrib = fieldType.GetCustomAttribute<Slider>();
                }
                else
                {
                    PropertyInfo? propertyType =
                        componentInspectorData.Inspectable.GetType().GetProperty(info.Name);
                    if (propertyType != null)
                    {
                        sliderAttrib = propertyType.GetCustomAttribute<Slider>();
                    }
                }
            }
        }

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