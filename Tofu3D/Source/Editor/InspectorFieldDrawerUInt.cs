using System.Linq;
using System.Reflection;
using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerUInt : InspectorFieldDrawable<uint>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        uint fieldValue = GetValue(info, componentInspectorData);
        int fieldValueForImGui = (int)fieldValue;
        info.GetCustomAttribute<SliderAttribute>(out SliderAttribute? sliderAttrib);
        info.GetCustomAttribute<NumberRangeLimiterAttribute>(out NumberRangeLimiterAttribute? rangeLimiterAttribute);

        if (rangeLimiterAttribute != null)
        {
            fieldValue = (uint)Mathf.Clamp(fieldValue, 0,
                rangeLimiterAttribute.MaxValueInt != null ? (uint)rangeLimiterAttribute.MaxValueInt : uint.MaxValue);
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
                // when custom value, dont show the slider


                bool isCustomValue = fieldValue < sliderAttrib.MinValue || fieldValue > sliderAttrib.MaxValue;
                if (isCustomValue)
                {
                    TofuImGui.PushStyleColor(ImGuiCol.SliderGrab, Color.Teal.ToVector4());
                    TofuImGui.PushStyleColor(ImGuiCol.Text, Color.Teal.ToVector4());
                }

                bool sliderValueChanged =
                    ImGui.SliderInt("", ref fieldValueForImGui, sliderAttrib.MinValue, sliderAttrib.MaxValue);

                fieldValue = (uint)fieldValueForImGui;
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
                    ImGui.SetKeyboardFocusHere();
                    data.FirstTimeShowingEditing = false;
                }

                bool inputFieldValueChanged =
                    ImGui.InputInt("", ref fieldValueForImGui, 1, 5, ImGuiInputTextFlags.AutoSelectAll);

                fieldValue = (uint)fieldValueForImGui;
                if (inputFieldValueChanged)
                {
                    SetValue(info, componentInspectorData, fieldValue);
                }

                // enter or we lose focus by clicking elsewhere...
                if (ImGui.IsItemEdited() && KeyboardInput.WasKeyJustPressed(Keys.Enter))
                {
                    data.IsEditing = false;
                }

                if (ImGui.IsItemFocused() == false)
                {
                    data.IsEditing = false;
                }
            }
        }
        else
        {
            if (ImGui.DragInt("", ref fieldValueForImGui))
            {
                fieldValue = (uint)fieldValueForImGui;
                SetValue(info, componentInspectorData, fieldValue);
            }
        }
    }
}