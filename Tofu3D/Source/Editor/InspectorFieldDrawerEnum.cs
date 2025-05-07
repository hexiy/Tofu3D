using System.Linq;
using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerEnum : InspectorFieldDrawable<Enum>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        if (info.AdditionalData == null)
        {
            info.AdditionalData = new InspectorFieldDrawerEnumData();
        }

        InspectorFieldDrawerEnumData data = info.AdditionalData as InspectorFieldDrawerEnumData;


        Enum fieldValue = GetValue(info, componentInspectorData);

        string[] enumValuesNames = Enum.GetNames(info.FieldOrPropertyType);
        int[] enumValues = Enum.GetValues(info.FieldOrPropertyType).Cast<int>().ToArray();

        data.SelectedEnumValueIndex = Array.IndexOf(enumValues, Convert.ToInt32(fieldValue));

        bool hasSelectOnHoverAttrib = info.HasCustomAttribute<DropdownAttrib_SelectOnHover>();
        int oldIndex = data.SelectedEnumValueIndex;

        // bool clicked = ImGui.Combo(string.Empty, ref _selectedEnumValueIndex, enumValuesNames,
        // enumValuesNames.Length);

        bool anyValueSelected = false;
        bool anyItemClicked = false;
        ////////////////////////////////////////////

        bool buttonClicked = ImGui.Button(enumValuesNames[data.SelectedEnumValueIndex]);


        if (buttonClicked)
        {
            data.PopupOpened = !data.PopupOpened;
            if (data.PopupOpened)
            {
                ImGui.OpenPopup("enumPopup");
            }
        }

        if (data.PopupOpened)
        {
            if (ImGui.BeginPopupContextWindow("enumPopup"))
            {
                // ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - TofuImGui.DefaultWindowPadding.X);

                for (var i = 0; i < enumValuesNames.Length; i++)
                {
                    var val = enumValuesNames[i];
                    TofuImGui.SetItemWidthToFullSpan(); //doesnt work ;)
                    bool clicked = ImGui.Button(val);
                    if (ImGui.IsItemClicked())
                    {
                        data.SelectedEnumValueIndex = i;
                        anyValueSelected = true;
                        anyItemClicked = true;
                    }
                    else if (ImGui.IsItemHovered() && hasSelectOnHoverAttrib)
                    {
                        data.SelectedEnumValueIndex = i;
                        anyValueSelected = true;
                    }
                }


                ImGui.EndPopup();
            }

            if (ImGui.IsPopupOpen("enumPopup") == false && data.PopupOpened)
                // clicked away
            {
                data.PopupOpened = false;
                // _popupOpened = false;
            }

            if (anyItemClicked)
            {
                ImGui.CloseCurrentPopup();
                data.PopupOpened = false;
            }
        }


        ////////////////////////////////////////////
        if (anyValueSelected)
        {
            object selectedEnumValue = Enum.ToObject(info.FieldOrPropertyType, enumValues[data.SelectedEnumValueIndex]);
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