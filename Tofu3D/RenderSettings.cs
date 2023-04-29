namespace Tofu3D;

public static class RenderSettings
{
	public static WireframeRenderSettings CurrentWireframeRenderSettings;
	public static ViewRenderModeSettings CurrentRenderModeSettings;
		
	

	public static void SaveData()
	{
		PersistentData.Set("RenderSettings.Wireframe", CurrentWireframeRenderSettings);
		PersistentData.Set("RenderSettings.ViewRenderMode", CurrentRenderModeSettings);
	}

	public static void LoadSavedData()
	{
		CurrentWireframeRenderSettings = PersistentData.Get<WireframeRenderSettings>("RenderSettings.Wireframe", new WireframeRenderSettings());
		CurrentRenderModeSettings = PersistentData.Get<ViewRenderModeSettings>("RenderSettings.ViewRenderMode", new ViewRenderModeSettings());
	}
}