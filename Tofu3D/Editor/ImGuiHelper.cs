namespace Tofu3D;

public static class ImGuiHelper
{
    public static float FlipYToGoodSpace(float y) => Tofu.Window.WindowSize.Y - y;

    public static Vector2 FlipYToGoodSpace(Vector2 v) => new(v.X, FlipYToGoodSpace(v.Y));
}