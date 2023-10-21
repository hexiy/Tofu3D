using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerMesh : InspectorFieldDrawable<Mesh>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        Mesh mesh = (Mesh)info.GetValue(componentInspectorData.Inspectable);

        string assetName = Path.GetFileName(mesh?.Path) ?? "";

        bool clicked = ImGui.Button(assetName,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));

        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload("CONTENT_BROWSER_MODEL", ImGuiDragDropFlags.None);
            string filePath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && filePath.Length > 0)
            {
                // fileName = Path.GetRelativePath("Assets", fileName);

                mesh = Tofu.AssetManager.Load<Mesh>(filePath);
                // gameObject.GetComponent<Renderer>().Material.Vao = Mesh.Vao; // materials are shared
                info.SetValue(componentInspectorData.Inspectable, mesh);
            }

            ImGui.EndDragDropTarget();
        }
    }
}