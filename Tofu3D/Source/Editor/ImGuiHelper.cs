namespace TofuEngine;

public static class ImGuiHelper
{
    public static float FlipYToGoodSpace(float y) => Tofu.Window.WindowSize.Y - y;

    public static Vector2 FlipYToGoodSpace(Vector2 v) => new Vector2(v.X, FlipYToGoodSpace(v.Y));
}