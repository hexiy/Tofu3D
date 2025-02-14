namespace Tofu3D;

public static class Screen
{
    public static Vector2 Center => Tofu.Window.WindowSize / 2 * Scale;
    
    /// <summary>
    /// Scaled screen size
    /// </summary>
    public static Vector2 Size => Tofu.Window.WindowSize * Scale;
    public static float Scale => Tofu.Window.MonitorScale;
    public static int ScaleI => (int)Tofu.Window.MonitorScale;
}