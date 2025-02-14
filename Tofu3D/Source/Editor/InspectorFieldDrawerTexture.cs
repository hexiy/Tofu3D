using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerTexture : InspectorFieldDrawable<RuntimeTexture>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        RuntimeTexture? texture = GetValue(info, componentInspectorData);
        string? textureName = texture == null ? "" : Path.GetFileName(texture.AnyPath);

        int posX = (int)ImGui.GetCursorPosX();

        if (texture == null)
        {
            ImGui.Dummy(new Vector2(150, 150));
        }
        else
        {
            TofuImGui.ImageTexture2DArray(texture, size: new Vector2(150, 150));
        }

        if (ImGui.IsItemClicked())
        {
            NavigateToFileInBrowser();
        }

        ApplyDragAndDropToLastControl();

        ImGui.SetCursorPosX(posX);


        bool clicked = ImGui.Button(textureName,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
        bool rightMouseClicked = ImGui.IsItemClicked(ImGuiMouseButton.Right);
        //ImiGui.Text(textureName);
        if (clicked)
        {
            NavigateToFileInBrowser();
        }

        void NavigateToFileInBrowser()
        {
            EditorPanelInspector.I.AddActionToActionQueue(() =>
            {
                EditorPanelBrowser.I.GoToFile(texture.PathInAssetsFolder);
            });
        }

        if (rightMouseClicked)
        {
            SetValue(info, componentInspectorData, null);
        }

        ApplyDragAndDropToLastControl();

        void ApplyDragAndDropToLastControl()
        {
            if (ImGui.BeginDragDropTarget())
            {
                ImGui.AcceptDragDropPayload(DragDropPayloadTypes.Texture, ImGuiDragDropFlags.None);
                string? payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                if (payload.Length > 0)
                {
                    payload = Path.GetRelativePath(Folders.ProjectFullPath, payload);

                    textureName = payload;

                    RuntimeTexture? loadedTexture = Tofu.AssetLoadManager.Get<RuntimeTexture>(textureName);

                    SetValue(info, componentInspectorData, loadedTexture);
                }
            }
        }

        ImGui.EndDragDropTarget();
    }
}