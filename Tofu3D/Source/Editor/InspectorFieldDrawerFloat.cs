using System.Linq;
using System.Reflection;
using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerFloat : InspectorFieldDrawable<float>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        float fieldValue = GetValue(info, componentInspectorData);

        info.GetCustomAttribute<SliderFAttribute>(out SliderFAttribute? sliderAttrib);
        info.GetCustomAttribute<NumberRangeLimiterAttribute>(out NumberRangeLimiterAttribute? rangeLimiterAttribute);

        if (rangeLimiterAttribute != null)
        {
            fieldValue = Mathf.Clamp(fieldValue,
                rangeLimiterAttribute.MinValueFloat ?? rangeLimiterAttribute.MinValueInt ?? float.MinValue,
                rangeLimiterAttribute.MaxValueFloat ?? rangeLimiterAttribute.MaxValueInt ?? float.MaxValue);
        }

        if (info.AdditionalData == null)
        {
            info.AdditionalData = new InspectorFieldDrawerSliderData();
        }

        InspectorFieldDrawerSliderData data = info.AdditionalData as InspectorFieldDrawerSliderData;

        if (sliderAttrib != null)
        {
            if (data.IsEditing == false)
            {
                bool isCustomValue = fieldValue < sliderAttrib.MinValue || fieldValue > sliderAttrib.MaxValue;
                if (isCustomValue)
                {
                    TofuImGui.PushStyleColor(ImGuiCol.SliderGrab, Color.Teal.ToVector4());
                    TofuImGui.PushStyleColor(ImGuiCol.Text, Color.Teal.ToVector4());
                }

                bool sliderValueChanged =
                    ImGui.SliderFloat("", ref fieldValue, sliderAttrib.MinValue, sliderAttrib.MaxValue);

                if (sliderValueChanged)
                {
                    SetValue(info, componentInspectorData, fieldValue);
                }

                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && ImGui.IsItemHovered())
                {
                    data.IsEditing = true;
                    data.FirstTimeShowingEditing = true;
                }

                if (isCustomValue)
                {
                    TofuImGui.PopStyleColor(2);
                }
            }
            else
            {
                if (data.FirstTimeShowingEditing)
                {
                    // ImGui.SetKeyboardFocusHere();
                    data.FirstTimeShowingEditing = false;
                }

                bool inputFieldValueChanged =
                    ImGui.InputFloat(string.Empty, ref fieldValue, 1, 5, string.Empty,
                        ImGuiInputTextFlags.AutoSelectAll);
                if (inputFieldValueChanged)
                {
                    SetValue(info, componentInspectorData, fieldValue);
                }

                // enter or we lose focus by clicking elsewhere...
                if (data.IsEditing && KeyboardInput.WasKeyJustPressed(Keys.Enter))
                {
                    data.IsEditing = false;
                }

                if (ImGui.IsItemFocused() == false)
                {
                    // data.IsEditing = false;
                }
            }

            // if (ImGui.SliderFloat("", ref fieldValue, sliderAttrib.MinValue, sliderAttrib.MaxValue))
            // {
            // SetValue(info, componentInspectorData, fieldValue);
            // }
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