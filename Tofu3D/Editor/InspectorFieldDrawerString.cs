using ImGuiNET;
using NativeFileDialogSharp;

namespace Tofu3D;

public class InspectorFieldDrawerString : InspectorFieldDrawable<string>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        string? fieldValue = GetValue(info, componentInspectorData);


        if (ImGui.InputTextMultiline("", ref fieldValue, 10_000,
                new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, 200)))
        {
            SetValue(info, componentInspectorData, fieldValue);
        }

        info.GetCustomAttribute<PathString>(out PathString? pathStringAttrib);
        if (pathStringAttrib != null)
        {
            bool clicked = ImGui.IsItemClicked();
            if (clicked)
            {
                DialogResult dialogResult = Dialog.FolderPicker(null);
                if (dialogResult.IsOk)
                {
                    fieldValue = dialogResult.Path;
                    SetValue(info, componentInspectorData, fieldValue);
                }
            }
        }
    }
}