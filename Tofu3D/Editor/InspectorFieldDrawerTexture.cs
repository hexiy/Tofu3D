using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerTexture : InspectorFieldDrawable<Texture>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        Texture texture = GetValue(info, componentInspectorData);
        string textureName = texture == null ? "" : Path.GetFileName(texture.Path);

        int posX = (int)ImGui.GetCursorPosX();

        if (texture == null)
            ImGui.Dummy(new Vector2(150, 150));
        else
            ImGui.Image(texture.TextureId, new Vector2(150, 150));

        ImGui.SetCursorPosX(posX);
        bool clicked = ImGui.Button(textureName,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
        bool rightMouseClicked = ImGui.IsItemClicked(ImGuiMouseButton.Right);
        //ImiGui.Text(textureName);
        if (clicked)
            // Debug.Log("TODO");
            EditorPanelInspector.I.AddActionToActionQueue(() => { EditorPanelBrowser.I.GoToFile(texture.Path); });

        if (rightMouseClicked) SetValue(info, componentInspectorData, null);

        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload("CONTENT_BROWSER_TEXTURE", ImGuiDragDropFlags.None);
            string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
            {
                payload = Path.GetRelativePath(Folders.EngineFolderPath, payload);

                textureName = payload;

                Texture loadedTexture = Tofu.AssetManager.Load<Texture>(textureName);

                SetValue(info, componentInspectorData, loadedTexture);
            }

            ImGui.EndDragDropTarget();
        }
    }
}