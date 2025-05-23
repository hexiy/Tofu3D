using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerTexture : InspectorFieldDrawable<RuntimeTexture>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        RuntimeTexture? texture = GetValue(info, componentInspectorData);
        string textureName = texture == null ? "None" : Path.GetFileName(texture.AnyPath);

        int posX = (int)ImGui.GetCursorPosX();

        if (texture == null)
        {
            // TofuImGui.ImageTexture2DArray(Tofu.Editor.EditorTextures.TransparentPixel, size: new Vector2(150, 150),
            // border_col: new Color(0.3f, 0.3f, 0.3f, 1));
            TofuImGui.Button("-", new Vector2(150, 150));
        }
        else
        {
            Vector2 pos = ImGui.GetCursorPos();
            TofuImGui.ImageTexture2DArray(Tofu.Editor.EditorTextures.Checkerboard, size: new Vector2(150, 150));
            ImGui.SetCursorPos(pos);
            TofuImGui.ImageTexture2DArray(texture, size: new Vector2(150, 150),
                border_col: new Color(0.3f, 0.3f, 0.3f, 1));
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
            if (texture == null)
            {
                return;
            }

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

                    if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) == false)
                    {
                        textureName = payload;

                        RuntimeTexture? loadedTexture = Tofu.AssetLoadManager.Get<RuntimeTexture>(textureName);

                        SetValue(info, componentInspectorData, loadedTexture);
                    }
                }
            }
        }

        ImGui.EndDragDropTarget();
    }
}