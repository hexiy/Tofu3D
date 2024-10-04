namespace Tofu3D;

public class RenderSettings
{
    public ViewRenderModeSettings CurrentRenderModeSettings;
    public WireframeRenderSettings CurrentWireframeRenderSettings;


    public void SaveData()
    {
        PersistentData.Set("RenderSettings.Wireframe", CurrentWireframeRenderSettings);
        PersistentData.Set("RenderSettings.ViewRenderMode", CurrentRenderModeSettings);
    }

    public void LoadSavedData()
    {
        CurrentWireframeRenderSettings =
            PersistentData.Get("RenderSettings.Wireframe", new WireframeRenderSettings());
        CurrentRenderModeSettings =
            PersistentData.Get("RenderSettings.ViewRenderMode", new ViewRenderModeSettings());
    }
}