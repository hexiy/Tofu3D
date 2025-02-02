using ImGuiNET;
using NativeFileDialogSharp;

namespace Tofu3D;

public class InspectorFieldDrawerString : InspectorFieldDrawable<string>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        string? fieldValue = GetValue(info, componentInspectorData);

        info.GetCustomAttribute<PathString>(out PathString? pathStringAttrib);
        if (pathStringAttrib != null)
        {
            string buttonLabel = fieldValue.Length > 0 ? fieldValue : "<path>";
            bool clicked = ImGui.Button(buttonLabel, new Vector2(ImGui.GetContentRegionAvail().X, 30));
            
            // ImGui.SameLine();
            if (clicked)
            {
                DialogResult dialogResult = Dialog.FolderPicker(null);
                if (dialogResult.IsOk)
                {
                    fieldValue = dialogResult.Path;

                    SetValue(info, componentInspectorData, fieldValue);
                }
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                SetValue(info, componentInspectorData, "");
            }

            return;
        }

        float widthAvailable = ImGui.GetContentRegionAvail().X;
        Vector2 textSize =
            ImGui.CalcTextSize($"{fieldValue} ",
                wrapWidth: widthAvailable); // add 1 space so new line changes the height
        textSize.Y = Mathf.ClampMin(textSize.Y, 35);
        if (ImGui.InputTextMultiline("", ref fieldValue, 10_000,
                new System.Numerics.Vector2(widthAvailable, textSize.Y + 10)))
        {
            SetValue(info, componentInspectorData, fieldValue);
        }
    }
}