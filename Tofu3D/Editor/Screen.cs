namespace Tofu3D;

public static class Screen
{
    public static Vector2 Center => Tofu.Window.WindowSize / 2 * Scale;
    public static Vector2 WindowSize => Tofu.Window.WindowSize * Scale;
    public static float Scale => Tofu.Window.MonitorScale;
}