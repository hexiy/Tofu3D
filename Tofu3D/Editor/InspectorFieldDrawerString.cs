using ImGuiNET;
using NativeFileDialogSharp;

namespace Tofu3D;

public class InspectorFieldDrawerString : InspectorFieldDrawable<string>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        string? fieldValue = GetValue(info, componentInspectorData);

        bool changed = false;


        info.GetCustomAttribute<PathString>(out PathString? pathStringAttrib);
        if (pathStringAttrib != null)
        {
            bool clicked = ImGui.Button("$");
            ImGui.SameLine();
            if (clicked)
            {
                DialogResult dialogResult = Dialog.FolderPicker(null);
                if (dialogResult.IsOk)
                {
                    fieldValue = dialogResult.Path;
                    changed = true;
                }
            }
        }

        float widthAvailable = ImGui.GetContentRegionAvail().X;
        Vector2 textSize = ImGui.CalcTextSize($"{fieldValue} ", wrapWidth: widthAvailable); // add 1 space so new line changes the height
        textSize.Y = Mathf.ClampMin(textSize.Y, 35);
        if (ImGui.InputTextMultiline("", ref fieldValue, 10_000,
                new System.Numerics.Vector2(widthAvailable, textSize.Y + 10)))
        {
            changed = true;
        }


        if (changed)
        {
            SetValue(info, componentInspectorData, fieldValue);
        }
    }
}