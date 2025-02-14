using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerMaterial : InspectorFieldDrawable<Asset_Material>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        string? materialName = "";
        Asset_Material material=null;
        if (componentInspectorData.Inspectable is Renderer)
        {
            materialName =
                Path.GetFileName((componentInspectorData.Inspectable as Renderer).Material?.AnyPath) ??
                materialName;

            material = (componentInspectorData.Inspectable as Renderer).Material;
        }
        else if (componentInspectorData.Inspectable is Asset_Material)
        {
            materialName = Path.GetFileName((componentInspectorData.Inspectable as Asset_Material).AnyPath);
            material = (componentInspectorData.Inspectable as Asset_Material);

        }

        materialName = materialName ?? "";

        if (material?.IsRuntimeCopy == true)
        {
            Vector4 headerColor = Color.DarkGoldenrod.ToVector4();
            ImGui.PushStyleColor(ImGuiCol.Text, headerColor);
        }


        bool clicked = ImGui.Button(materialName,
            new Vector2(TofuImGui.GetContentRegionAvailWithPadding().X, ImGui.GetFrameHeight()));
        if (clicked)
        {
            EditorPanelInspector.I.AddActionToActionQueue(() =>
                EditorPanelInspector.I.SelectInspectable((componentInspectorData.Inspectable as Renderer).Material,
                    (fieldName) =>
                    {
                        Asset_Material assetMaterial = (componentInspectorData.Inspectable as Asset_Material);

                        // save materials in both Library/ and Assets/ 
                        Tofu.AssetLoadManager.Save<Asset_Material>(assetMaterial.PathInAssetsFolder, assetMaterial);
                        Tofu.AssetLoadManager.Save<Asset_Material>(assetMaterial.PathInLibraryFolder, assetMaterial);
                    })
            );
        }

        // EditorPanelBrowser.I.GoToFile(materialPath);
        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload(DragDropPayloadTypes.Material, ImGuiDragDropFlags.None);
            string? payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            // if (Tofu.MouseInput.ButtonReleased(MouseButtons.Left) && payload.Length > 0)
            if (Tofu.MouseInput.ButtonReleased() && payload.Length > 0)
            {
                payload = payload;
                // var materialName = Path.GetFileName(payload);

                Asset_Material? draggedMaterial = Tofu.AssetLoadManager.Get<Asset_Material>(payload);
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