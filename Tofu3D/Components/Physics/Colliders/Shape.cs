namespace Scripts;

[ExecuteInEditMode]
public abstract class Shape : Component
{
    [Hide]
    public abstract ShapeType ShapeType { get; }

    public Vector3 Pivot = Vector3.Half;

    public bool PhysicsEnabled = true;
}