using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerMaterial : InspectorFieldDrawable<Asset_Material>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        var materialPath = "";

        if (componentInspectorData.Inspectable is Renderer)
        {
            materialPath = Path.GetFileName((componentInspectorData.Inspectable as Renderer).Material.Path);
        }
        else if (componentInspectorData.Inspectable is Asset_Material)
        {
            materialPath = Path.GetFileName((componentInspectorData.Inspectable as Asset_Material).Path);
        }

        materialPath = materialPath ?? "";
        var clicked = ImGui.Button(materialPath,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
        if (clicked)
        {
            EditorPanelInspector.I.AddActionToActionQueue(() =>
                EditorPanelInspector.I.SelectInspectable((componentInspectorData.Inspectable as Renderer).Material)
            );
        }

        // EditorPanelBrowser.I.GoToFile(materialPath);
        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload("CONTENT_BROWSER_MATERIAL", ImGuiDragDropFlags.None);
            var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
            {
                payload = payload;
                var materialName = Path.GetFileName(payload);

                var draggedMaterial = Tofu.AssetManager.Load<Asset_Material>(payload);
                if (draggedMaterial.Shader == null)
                {
                    Debug.Log("No Shader attached to material.");
                }
                else
                {
                    (componentInspectorData.Inspectable as Renderer).Material = draggedMaterial;
                }
                // load new material
            }

            ImGui.EndDragDropTarget();
        }
    }
}