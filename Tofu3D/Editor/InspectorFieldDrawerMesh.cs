using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerMesh : InspectorFieldDrawable<RuntimeMesh>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        var mesh = (RuntimeMesh)info.GetValue(componentInspectorData.Inspectable);

        var assetName = Path.GetFileName(mesh?.MeshAssetPath) ?? "";

        var clicked = ImGui.Button(assetName,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));

        if (ImGui.BeginDragDropTarget())
        {
            if (TofuImGui.AcceptDragDropPayload("MODEL"))
            {
                var filePath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && filePath.Length > 0)
                {
                    try
                    {
                        Asset_Model modelAsset = Tofu.AssetLoadManager.Load<Asset_Model>(filePath);
                        mesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(modelAsset.PathsToMeshAssets[0]);
                        info.SetValue(componentInspectorData.Inspectable, mesh);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                    }
                }
            }

            ImGui.EndDragDropTarget();
        }

        if (ImGui.BeginDragDropTarget())
        {
            if (TofuImGui.AcceptDragDropPayload("MESH"))
            {
                var filePath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && filePath.Length > 0)
                {
                    try
                    {
                        RuntimeMesh runtimeMesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(filePath);
                        info.SetValue(componentInspectorData.Inspectable, runtimeMesh);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                    }
                }
            }

            ImGui.EndDragDropTarget();
        }
    }
}