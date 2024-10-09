using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerMesh : InspectorFieldDrawable<Asset_Mesh>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        var mesh = (Asset_Mesh)info.GetValue(componentInspectorData.Inspectable);

        var assetName = Path.GetFileName(mesh?.PathToRawAsset) ?? "";

        var clicked = ImGui.Button(assetName,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));

        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload("CONTENT_BROWSER_MODEL", ImGuiDragDropFlags.None);
            var filePath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && filePath.Length > 0)
            {
                // fileName = Path.GetRelativePath("Assets", fileName);

                mesh = Tofu.AssetLoadManager.Load<Asset_Mesh>(filePath);
                // gameObject.GetComponent<Renderer>().Material.Vao = Mesh.Vao; // materials are shared
                info.SetValue(componentInspectorData.Inspectable, mesh);
            }

            ImGui.EndDragDropTarget();
        }
    }
}