using System.IO;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerCubemapTexture : InspectorFieldDrawable<CubemapTexture>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        // CubemapTexture cubemapTexture = info.ListElement as CubemapTexture;
        CubemapTexture cubemapTexture = GetValue(info, componentInspectorData);
        string textureName = Path.GetFileName(cubemapTexture.Path);

        bool clicked = ImGui.Button(textureName,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
        if (clicked) EditorPanelBrowser.I.GoToFile(cubemapTexture.Path);
    }
}