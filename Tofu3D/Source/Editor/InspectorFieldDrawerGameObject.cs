using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerGameObject : InspectorFieldDrawable<GameObject>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        GameObject? goObject = info.GetValue(componentInspectorData.Inspectable) as GameObject;
        string fieldGoName = goObject?.Name ?? "";
        bool clicked = ImGui.Button(fieldGoName,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
        if (clicked && goObject != null)
            // todo
            // EditorPanelHierarchy.I.SelectGameObject(goObject.Id);
        {
            return;
        }

        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload(DragDropPayloadTypes.PrefabPath, ImGuiDragDropFlags.None);
            string? payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            string dataType = ImGui.GetDragDropPayload().DataType.GetStringASCII()
                .Replace("\0", string.Empty);
            if (dataType == DragDropPayloadTypes.PrefabPath)
            {
                if (Tofu.MouseInput.ButtonReleased(MouseButtons.Left) && payload.Length > 0)
                {
                    GameObject loadedGo = Tofu.SceneSerializer.LoadPrefab(payload, true);
                    info.SetValue(componentInspectorData.Inspectable, loadedGo);
                }
            }

            ImGui.EndDragDropTarget();
        }

        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload(DragDropPayloadTypes.GameObject, ImGuiDragDropFlags.None);
            string? payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            string dataType = ImGui.GetDragDropPayload().DataType.GetStringASCII()
                .Replace("\0", string.Empty);

            if (dataType == DragDropPayloadTypes.GameObject)
                //	string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            {
                if (Tofu.MouseInput.ButtonReleased(MouseButtons.Left) && payload.Length > 0)
                {
                    GameObject foundGo = Tofu.SceneManager.CurrentScene.GetGameObjectByID(int.Parse(payload));
                    info.SetValue(componentInspectorData.Inspectable, foundGo);
                }
            }

            ImGui.EndDragDropTarget();
        }
    }
}