using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerShader : InspectorFieldDrawable<Shader>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        var shaderPath = "";

        if (componentInspectorData.Inspectable is Asset_Material)
        {
            shaderPath = Path.GetFileName((componentInspectorData.Inspectable as Asset_Material).Shader?.Path);
        }
        else
        {
            return;
        }

        shaderPath = shaderPath ?? "";
        var clicked = ImGui.Button(shaderPath,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));

        // EditorPanelBrowser.I.GoToFile(materialPath);
        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload(DragDropPayloadTypes.Shader, ImGuiDragDropFlags.None);
            var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (Tofu.MouseInput.ButtonReleased(MouseButtons.Left) && payload.Length > 0)
            {
                payload = payload;
                var shaderName = Path.GetFileName(payload);

                Shader shader = Tofu3D.Tofu.ShaderManager.LoadShader(payload);
                (componentInspectorData.Inspectable as Asset_Material).Shader = shader;
                info.SetValue(componentInspectorData.Inspectable, shader);
                // load new material
            }

            ImGui.EndDragDropTarget();
        }
    }
}