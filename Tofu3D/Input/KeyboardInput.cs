namespace Tofu3D;

public static class KeyboardInput
{
    public static bool WasKeyJustPressed(Keys key)
    {
        SwapPlatformSpecificKeys(ref key);
        return Tofu.Window.KeyboardState.IsKeyPressed((OpenTK.Windowing.GraphicsLibraryFramework.Keys)key);
    }

    public static bool WasKeyJustReleased(Keys key)
    {
        SwapPlatformSpecificKeys(ref key);
        return Tofu.Window.KeyboardState.IsKeyReleased((OpenTK.Windowing.GraphicsLibraryFramework.Keys)key);
    }

    public static bool IsKeyDown(Keys key)
    {
        SwapPlatformSpecificKeys(ref key);
        return Tofu.Window.KeyboardState.IsKeyDown((OpenTK.Windowing.GraphicsLibraryFramework.Keys)key);
    }

    public static bool IsKeyUp(Keys key)
    {
        SwapPlatformSpecificKeys(ref key);

        return IsKeyDown(key) == false;
    }

    private static void SwapPlatformSpecificKeys(ref Keys key)
    {
#if OS_WINDOWS
        return;
#endif
        if (key is Keys.LeftSuper)
            key = Keys.LeftControl;
        else if (key is Keys.RightSuper)
            key = Keys.RightControl;
        else if (key is Keys.LeftControl)
            key = Keys.LeftSuper;
        else if (key is Keys.RightControl) key = Keys.RightSuper;
    }
}