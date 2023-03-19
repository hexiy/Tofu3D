namespace Tofu3D;

public class EditorTextures
{
	public static EditorTextures I { get; private set; }

	public EditorTextures()
	{
		I = this;
	}

	public Texture LogCategoryInfoIcon = new Texture().Load("Resources/Console/info.png", false, true);
	public Texture LogCategoryWarningIcon = new Texture().Load("Resources/Console/warning.png", false, true);
	public Texture LogCategoryErrorIcon = new Texture().Load("Resources/Console/error.png", false, true);
	public Texture LogCategoryTimerIcon = new Texture().Load("Resources/Console/timer.png", false, true);
}