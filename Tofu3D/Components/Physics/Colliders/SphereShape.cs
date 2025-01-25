namespace Scripts;

public class SphereShape : Shape
{
    public float Radius;
    
    [Hide]
    public override ShapeType ShapeType => ShapeType.Sphere;

}