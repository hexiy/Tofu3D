namespace Tofu3D;

public static class RenderSettings
{
	public static WireframeRenderSettings WireframeRenderSettings;

	public static void SaveData()
	{
		PersistentData.Set("RenderSettings.Wireframe", WireframeRenderSettings);
	}

	public static void LoadSavedData()
	{
		WireframeRenderSettings = PersistentData.Get<WireframeRenderSettings>("RenderSettings.Wireframe", new WireframeRenderSettings());
	}
}