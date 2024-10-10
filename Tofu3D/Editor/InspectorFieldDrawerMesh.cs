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
            {
                if (ImGui.AcceptDragDropPayload("MODEL", ImGuiDragDropFlags.None).DataSize > 0)
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
        }

        if (ImGui.BeginDragDropTarget())
        {
            if (ImGui.AcceptDragDropPayload("MESH", ImGuiDragDropFlags.None).DataSize > 0)
            {
                var filePath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && filePath.Length > 0)
                {
                    try
                    {
                        Asset_Mesh assetMesh = Tofu.AssetLoadManager.Load<Asset_Mesh>(filePath);
                        mesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(assetMesh.PathToRawAsset);
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
    }
}