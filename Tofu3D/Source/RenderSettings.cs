namespace TofuEngine;

public class RenderSettings
{
    public ViewRenderModeSettings RenderModeSettings;
    public WireframeRenderSettings WireframeRenderSettings;

    public RenderSettings()
    {
        RenderModeSettings = new ViewRenderModeSettings();
        WireframeRenderSettings = new WireframeRenderSettings();
    }
    // public void SaveData()
    // {
    //     PersistentData.Set("RenderSettings.Wireframe", CurrentWireframeRenderSettings);
    //     PersistentData.Set("RenderSettings.ViewRenderMode", CurrentRenderModeSettings);
    // }

    // public void LoadSavedData()
    // {
    //     CurrentWireframeRenderSettings =
    //         PersistentData.Get("RenderSettings.Wireframe", () => new WireframeRenderSettings()) as
    //             WireframeRenderSettings;
    //     CurrentRenderModeSettings =
    //         PersistentData.Get("RenderSettings.ViewRenderMode", () => new ViewRenderModeSettings()) as
    //             ViewRenderModeSettings;
    // }
}