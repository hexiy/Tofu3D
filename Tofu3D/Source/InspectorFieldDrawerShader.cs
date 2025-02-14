using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerShader : InspectorFieldDrawable<Shader>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        string? shaderPath = "";

        if (componentInspectorData.Inspectable is Asset_Material)
        {
            shaderPath = Path.GetFileName((componentInspectorData.Inspectable as Asset_Material).Shader?.Path);
        }
        else
        {
            return;
        }

        shaderPath = shaderPath ?? "";
        bool clicked = ImGui.Button(shaderPath,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));

        // EditorPanelBrowser.I.GoToFile(materialPath);
        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload(DragDropPayloadTypes.Shader, ImGuiDragDropFlags.None);
            string? payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (Tofu.MouseInput.ButtonReleased(MouseButtons.Left) && payload.Length > 0)
            {
                payload = payload;
                string shaderName = Path.GetFileName(payload);

                Shader shader = Tofu.ShaderManager.LoadShader(payload);
                (componentInspectorData.Inspectable as Asset_Material).Shader = shader;
                info.SetValue(componentInspectorData.Inspectable, shader);
                // load new material
            }

            ImGui.EndDragDropTarget();
        }
    }
}