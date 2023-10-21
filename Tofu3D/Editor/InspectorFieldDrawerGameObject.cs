using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerGameObject : InspectorFieldDrawable<GameObject>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        GameObject goObject = info.GetValue(componentInspectorData.Inspectable) as GameObject;
        string fieldGoName = goObject?.Name ?? "";
        bool clicked = ImGui.Button(fieldGoName,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
        if (clicked && goObject != null)
        {
            // todo
            // EditorPanelHierarchy.I.SelectGameObject(goObject.Id);
            return;
        }

        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload("PREFAB_PATH", ImGuiDragDropFlags.None);
            string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            string dataType = ImGui.GetDragDropPayload().DataType.GetStringASCII()
                .Replace("\0", string.Empty);
            if (dataType == "PREFAB_PATH")
            {
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
                {
                    GameObject loadedGo = Tofu.SceneSerializer.LoadPrefab(payload, true);
                    info.SetValue(componentInspectorData.Inspectable, loadedGo);
                }
            }

            ImGui.EndDragDropTarget();
        }

        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload("GAMEOBJECT", ImGuiDragDropFlags.None);
            string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            string dataType = ImGui.GetDragDropPayload().DataType.GetStringASCII()
                .Replace("\0", string.Empty);

            if (dataType == "GAMEOBJECT")
            {
                //	string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
                {
                    GameObject foundGo = Tofu.SceneManager.CurrentScene.GetGameObject(int.Parse(payload));
                    info.SetValue(componentInspectorData.Inspectable, foundGo);
                }
            }

            ImGui.EndDragDropTarget();
        }
    }
}