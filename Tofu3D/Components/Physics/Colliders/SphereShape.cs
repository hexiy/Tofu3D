namespace Scripts;

public class SphereShape : Shape
{
    public float Radius = 1;

    [Hide]
    public override ShapeType ShapeType => ShapeType.Sphere;
}