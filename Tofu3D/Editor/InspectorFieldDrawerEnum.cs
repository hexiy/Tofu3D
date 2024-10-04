using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerEnum : InspectorFieldDrawable<Enum>
{
    // private bool _popupOpened = false;
    private int _selectedEnumValueIndex;

    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        var fieldValue = GetValue(info, componentInspectorData);

        var enumValuesNames = Enum.GetNames(info.FieldOrPropertyType);
        var clicked = ImGui.Combo(fieldValue.ToString(), ref _selectedEnumValueIndex, enumValuesNames,
            enumValuesNames.Length);
        if (clicked)
        {
            SetValue(info, componentInspectorData,
                (Enum)Enum.ToObject(info.FieldOrPropertyType, _selectedEnumValueIndex));
            // info.SetValue(componentInspectorData.Inspectable, Enum.ToObject(info.FieldOrPropertyType, _selectedEnumValueIndex));
            EditorPanelInspector.I.QueueInspectorRefresh();
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