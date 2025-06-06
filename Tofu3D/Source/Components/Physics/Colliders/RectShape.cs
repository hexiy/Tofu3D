public class RectShape : Shape
{
    public Vector2 Offset = Vector3.Zero;
    public Vector2 Size = Vector3.One;

    public Vector2 GetMinPos() => Transform.WorldPosition;

    public Vector2 GetMaxPos() => Transform.WorldPosition + Size;

    [Hide]
    public override ShapeType ShapeType => ShapeType.Rect;
}