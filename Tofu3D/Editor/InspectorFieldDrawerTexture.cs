using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerTexture : InspectorFieldDrawable<Asset_Texture>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        var texture = GetValue(info, componentInspectorData);
        var textureName = texture == null ? "" : Path.GetFileName(texture.Path);

        var posX = (int)ImGui.GetCursorPosX();

        if (texture == null)
        {
            ImGui.Dummy(new Vector2(150, 150));
        }
        else
        {
            ImGui.Image(texture.TextureId, new Vector2(150, 150));
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
            EditorPanelInspector.I.AddActionToActionQueue(() => { EditorPanelBrowser.I.GoToFile(texture.Path); });
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
                ImGui.AcceptDragDropPayload("CONTENT_BROWSER_TEXTURE", ImGuiDragDropFlags.None);
                var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
                {
                    payload = Path.GetRelativePath(Folders.EngineFolderPath, payload);

                    textureName = payload;

                    var loadedTexture = Tofu.AssetManager.Load<Asset_Texture>(textureName);

                    SetValue(info, componentInspectorData, loadedTexture);

                    if (componentInspectorData.Inspectable is Asset_Material)
                    {
                        // EditorPanelInspector.I.AddActionToActionQueue(() =>
                            // Tofu.AssetManager.Save<Asset_Material>(componentInspectorData.Inspectable as Asset_Material));
                    }
                }

                ImGui.EndDragDropTarget();
            }
        }
    }
}