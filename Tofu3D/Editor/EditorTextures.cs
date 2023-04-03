namespace Tofu3D;

public class EditorTextures
{
	public static EditorTextures I { get; private set; }

	public EditorTextures()
	{
		I = this;

		LogCategoryInfoIcon = AssetManager.Load<Texture>("Resources/Console/info.png");
		LogCategoryWarningIcon = AssetManager.Load<Texture>("Resources/Console/warning.png");
		LogCategoryErrorIcon = AssetManager.Load<Texture>("Resources/Console/error.png");
		LogCategoryTimerIcon = AssetManager.Load<Texture>("Resources/Console/timer.png");
	}

	public Texture LogCategoryInfoIcon;
	public Texture LogCategoryWarningIcon;
	public Texture LogCategoryErrorIcon;
	public Texture LogCategoryTimerIcon;
}