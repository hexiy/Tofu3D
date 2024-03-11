using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerMaterial : InspectorFieldDrawable<Material>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        string materialPath = "";

        if (componentInspectorData.Inspectable is Renderer)
        {
            materialPath = Path.GetFileName((componentInspectorData.Inspectable as Renderer).Material.Path);
        }
        else if (componentInspectorData.Inspectable is Material)
        {
            materialPath = Path.GetFileName((componentInspectorData.Inspectable as Material).Path);
        }
        materialPath = materialPath ?? "";
        bool clicked = ImGui.Button(materialPath,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
        if (clicked)
            EditorPanelInspector.I.AddActionToActionQueue(() =>
                EditorPanelInspector.I.SelectInspectable((componentInspectorData.Inspectable as Renderer).Material)
            );
        // EditorPanelBrowser.I.GoToFile(materialPath);
        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload("CONTENT_BROWSER_MATERIAL", ImGuiDragDropFlags.None);
            string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
            {
                payload = payload;
                string materialName = Path.GetFileName(payload);

                Material draggedMaterial = Tofu.AssetManager.Load<Material>(payload);
                if (draggedMaterial.Shader == null)
                    Debug.Log("No Shader attached to material.");
                else
                    (componentInspectorData.Inspectable as Renderer).Material = draggedMaterial;
                // load new material
            }

            ImGui.EndDragDropTarget();
        }
    }
}