using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerTexture : InspectorFieldDrawable<RuntimeTexture>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        var texture = GetValue(info, componentInspectorData);
        var textureName = texture == null ? "" : Path.GetFileName(texture.AnyPath);

        var posX = (int)ImGui.GetCursorPosX();

        if (texture == null)
        {
            ImGui.Dummy(new Vector2(150, 150));
        }
        else
        {
            ImGui.Image(texture.AtlasGLTextureId, new Vector2(150, 150));
        }

        if (ImGui.IsItemClicked())
        {
            NavigateToFileInBrowser();
        }

        ApplyDragAndDropToLastControl();

        ImGui.SetCursorPosX(posX);


        var clicked = ImGui.Button(textureName,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
        var rightMouseClicked = ImGui.IsItemClicked(ImGuiMouseButton.Right);
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
                var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                if (payload.Length > 0)
                {
                    payload = Path.GetRelativePath(Folders.ProjectFullPath, payload);

                    textureName = payload;

                    var loadedTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(textureName);

                    SetValue(info, componentInspectorData, loadedTexture);
                }
            }
        }

        ImGui.EndDragDropTarget();
    }
}