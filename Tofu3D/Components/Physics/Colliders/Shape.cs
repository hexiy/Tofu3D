namespace Scripts;

[ExecuteInEditMode]
public abstract class Shape : Component
{
    [Hide]
    public abstract ShapeType ShapeType { get; }
}