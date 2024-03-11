namespace Tofu3D;

public static class ImGuiHelper
{
    public static float FlipYToGoodSpace(float y)
    {
        return Tofu.Window.WindowSize.Y - y;
    }

    public static Vector2 FlipYToGoodSpace(Vector2 v)
    {
        return new Vector2(v.X, FlipYToGoodSpace(v.Y));
    }
}