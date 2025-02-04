using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerMesh : InspectorFieldDrawable<RuntimeMesh>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        RuntimeMesh? mesh = (RuntimeMesh)info.GetValue(componentInspectorData.Inspectable);

        string assetName = mesh?.Mesh?.Name ?? "";

        bool clicked = ImGui.Button(assetName,
            new Vector2(TofuImGui.GetContentRegionAvailWithPadding().X, ImGui.GetFrameHeight()));

        if (ImGui.BeginDragDropTarget())
        {
            if (TofuImGui.PayloadHasBeenDropped(DragDropPayloadTypes.Model))
            {
                string? filePath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                if (filePath.Length > 0 &&
                    AssetPathExtensions.IsFileModel(filePath))
                {
                    // try
                    // {
                        Asset_Model modelAsset = Tofu.AssetLoadManager.Load<Asset_Model>(filePath);
                        mesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(modelAsset.PathsToMeshAssets[0]);
                        info.SetValue(componentInspectorData.Inspectable, mesh);
                    // }
                    // catch (Exception ex)
                    // {
                        // Debug.LogError(ex.Message);
                    // }
                }
            }

            ImGui.EndDragDropTarget();
        }

        if (ImGui.BeginDragDropTarget())
        {
            if (TofuImGui.PayloadHasBeenDropped(DragDropPayloadTypes.Mesh))
            {
                string? filePath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                if (filePath.Length > 0 &&
                    AssetPathExtensions.IsFileMesh(filePath))
                {
                    // try
                    // {
                        RuntimeMesh runtimeMesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(filePath);
                        info.SetValue(componentInspectorData.Inspectable, runtimeMesh);
                    // }
                    // catch (Exception ex)
                    // {
                        // Debug.LogError(ex.Message);
                    // }
                }
            }

            ImGui.EndDragDropTarget();
        }
    }
}