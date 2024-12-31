using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerMaterial : InspectorFieldDrawable<Asset_Material>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        var materialPath = "";
        Asset_Material material=null;
        if (componentInspectorData.Inspectable is Renderer)
        {
            materialPath =
                Path.GetFileName((componentInspectorData.Inspectable as Renderer).Material?.Path) ??
                materialPath;

            material = (componentInspectorData.Inspectable as Renderer).Material;
        }
        else if (componentInspectorData.Inspectable is Asset_Material)
        {
            materialPath = Path.GetFileName((componentInspectorData.Inspectable as Asset_Material).Path);
            material = (componentInspectorData.Inspectable as Asset_Material);

        }

        materialPath = materialPath ?? "";

        if (material?.IsRuntimeCopy == true)
        {
            Vector4 headerColor = Color.DarkGoldenrod.ToVector4();
            ImGui.PushStyleColor(ImGuiCol.Text, headerColor);
        }


        var clicked = ImGui.Button(materialPath,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
        if (clicked)
        {
            EditorPanelInspector.I.AddActionToActionQueue(() =>
                EditorPanelInspector.I.SelectInspectable((componentInspectorData.Inspectable as Renderer).Material,
                    () =>
                    {
                        Asset_Material assetMaterial = (componentInspectorData.Inspectable as Asset_Material);

                        // save materials in both Library/ and Assets/ 
                        Tofu.AssetLoadManager.Save<Asset_Material>(assetMaterial.Path.GetPathOfAssetInLibrayFromSourceAssetPathOrName(), assetMaterial);
                        Tofu.AssetLoadManager.Save<Asset_Material>(assetMaterial.Path, assetMaterial);
                    })
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

                var draggedMaterial = Tofu.AssetLoadManager.Load<Asset_Material>(payload);
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

        if (material?.IsRuntimeCopy == true)
        {
            ImGui.PopStyleColor();
        }
    }
}