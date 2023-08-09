namespace Tofu3D;

public class RenderSettings
{
	public WireframeRenderSettings CurrentWireframeRenderSettings;
	public ViewRenderModeSettings CurrentRenderModeSettings;
		
	

	public void SaveData()
	{
		PersistentData.Set("RenderSettings.Wireframe", CurrentWireframeRenderSettings);
		PersistentData.Set("RenderSettings.ViewRenderMode", CurrentRenderModeSettings);
	}

	public void LoadSavedData()
	{
		CurrentWireframeRenderSettings = PersistentData.Get<WireframeRenderSettings>("RenderSettings.Wireframe", new WireframeRenderSettings());
		CurrentRenderModeSettings = PersistentData.Get<ViewRenderModeSettings>("RenderSettings.ViewRenderMode", new ViewRenderModeSettings());
	}
}