namespace Tofu3D;

public static class KeyboardInput
{
	public static bool WasKeyJustPressed(Keys key)
	{
		return Tofu.I.Window.KeyboardState.IsKeyPressed((OpenTK.Windowing.GraphicsLibraryFramework.Keys) key);
	}

	public static bool IsKeyDown(Keys key)
	{
		return Tofu.I.Window.KeyboardState.IsKeyDown((OpenTK.Windowing.GraphicsLibraryFramework.Keys) key);
	}

	public static bool IsKeyUp(Keys key)
	{
		return Tofu.I.Window.KeyboardState.IsKeyReleased((OpenTK.Windowing.GraphicsLibraryFramework.Keys) key);
	}
}