using System.Linq;
using System.Reflection;
using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerInt : InspectorFieldDrawable<int>
{
    // private bool _isEditing = false;
    // private bool _firstTimeShowingEditing = false;

    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        int fieldValue = GetValue(info, componentInspectorData);

        info.GetCustomAttribute<Slider>(out Slider? sliderAttrib);

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
                    ImGui.PushStyleColor(ImGuiCol.SliderGrab, Color.Teal.ToVector4());
                    ImGui.PushStyleColor(ImGuiCol.Text, Color.Teal.ToVector4());
                }

                bool sliderValueChanged =
                    ImGui.SliderInt("", ref fieldValue, sliderAttrib.MinValue, sliderAttrib.MaxValue);

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
                    ImGui.PopStyleColor(2);
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
                    ImGui.InputInt("", ref fieldValue, 1, 5, ImGuiInputTextFlags.AutoSelectAll);
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
            if (ImGui.DragInt("", ref fieldValue))
            {
                SetValue(info, componentInspectorData, fieldValue);
            }
        }
    }
}