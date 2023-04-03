namespace Tofu3D;

public class EditorTextures
{
	public static EditorTextures I { get; private set; }

	public EditorTextures()
	{
		I = this;
	}

	public Texture LogCategoryInfoIcon = AssetManager.Load<Texture>("Resources/Console/info.png");
	public Texture LogCategoryWarningIcon = AssetManager.Load<Texture>("Resources/Console/warning.png");
	public Texture LogCategoryErrorIcon = AssetManager.Load<Texture>("Resources/Console/error.png");
	public Texture LogCategoryTimerIcon = AssetManager.Load<Texture>("Resources/Console/timer.png");
}