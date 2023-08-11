namespace Tofu3D;

public static class Screen
{
    public static Vector2 Center => Tofu.I.Window.WindowSize /2*Scale;
    public static Vector2 Size => Tofu.I.Window.WindowSize * Scale;
    public static float Scale => Tofu.I.Window.MonitorScale;
}