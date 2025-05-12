using System.IO;
using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerCubemapTexture : InspectorFieldDrawable<RuntimeCubemapTexture>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        // CubemapTexture cubemapTexture = info.ListElement as CubemapTexture;
        RuntimeCubemapTexture cubemapTexture = GetValue(info, componentInspectorData);
        string textureName = Path.GetFileName(cubemapTexture.PathInAssetsFolder) ?? "";

        bool clicked = ImGui.Button(textureName,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
        if (clicked)
        {
            EditorPanelBrowser.I.GoToFile(cubemapTexture.PathInAssetsFolder);
        }
    }
}