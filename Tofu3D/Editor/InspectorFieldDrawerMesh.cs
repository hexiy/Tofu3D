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
            ImGui.AcceptDragDropPayload("CONTENT_BROWSER_MODEL", ImGuiDragDropFlags.None);
            var filePath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && filePath.Length > 0)
            {
                // fileName = Path.GetRelativePath("Assets", fileName);

                Debug.Log("todo after we have expandable .obj files in browser");
                // once we have obj expander in browser we can drag individual meshes
                Asset_Model modelAssetTemporary = Tofu.AssetLoadManager.Load<Asset_Model>(filePath);
                mesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(modelAssetTemporary.PathsToMeshAssets[0]);
                // gameObject.GetComponent<Renderer>().Material.Vao = Mesh.Vao; // materials are shared
                info.SetValue(componentInspectorData.Inspectable, mesh);
            }

            ImGui.EndDragDropTarget();
        }
    }
}