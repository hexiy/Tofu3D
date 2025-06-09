namespace Tofu3D.Components;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScaler : Component
{
    public Vector2 ReferenceResolution;

    [NumberRangeLimiter(0, 1)]
    public float MatchWidthOrHeight = 0.5f;
}