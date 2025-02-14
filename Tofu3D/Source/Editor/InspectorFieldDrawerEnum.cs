using System.Linq;
using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerEnum : InspectorFieldDrawable<Enum>
{
    // private bool _popupOpened = false;
    private int _selectedEnumValueIndex;

    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        Enum fieldValue = GetValue(info, componentInspectorData);

        string[] enumValuesNames = Enum.GetNames(info.FieldOrPropertyType);
        int[] enumValues = Enum.GetValues(info.FieldOrPropertyType).Cast<int>().ToArray();

        _selectedEnumValueIndex = Array.IndexOf(enumValues, Convert.ToInt32(fieldValue));

        bool clicked = ImGui.Combo(string.Empty, ref _selectedEnumValueIndex, enumValuesNames,
            enumValuesNames.Length);
        if (clicked)
        {
            object selectedEnumValue = Enum.ToObject(info.FieldOrPropertyType, enumValues[_selectedEnumValueIndex]);
            SetValue(info, componentInspectorData,
                (Enum)Enum.ToObject(info.FieldOrPropertyType, (Enum)selectedEnumValue));
            // info.SetValue(componentInspectorData.Inspectable, Enum.ToObject(info.FieldOrPropertyType, _selectedEnumValueIndex));

            componentInspectorData.Inspector.QueueRefresh(componentInspectorData);
        }
        // bool enumClicked = ImGui.IsItemClicked();
        //
        // if (enumClicked)
        // {
        //     ImGui.OpenPopupOnItemClick();
        // }
        //
        //
        // if (enumClicked)
        // {
        //     _popupOpened = !_popupOpened;
        //     if (_popupOpened)
        //     {
        //         ImGui.OpenPopup("Enum");
        //     }
        // }
        //
        // if (_popupOpened)
        // {
        //     if (ImGui.BeginPopupContextWindow("Enum"))
        //     {var enumNames = Enum.GetNames(info.FieldOrPropertyType);
        //         
        //         foreach (string enumName in enumNames)
        //         {
        //           
        //            ImGui.Text(enumName);
        //
        //             if (ImGui.IsItemClicked())
        //             {
        //                 info.SetValue(componentInspectorData.Inspectable, Enum.Parse(info.FieldOrPropertyType, enumName));
        //                 _popupOpened = false;
        //             }
        //         }
        //
        //         ImGui.EndPopup();
        //     }
        //
        //     if (ImGui.IsPopupOpen("Enum") == false && _popupOpened)
        //     {
        //         // clicked away
        //         _popupOpened = false;
        //     }
        // }
    }
}