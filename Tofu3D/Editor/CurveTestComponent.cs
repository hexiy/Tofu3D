namespace Tofu3D;

[ExecuteInEditMode]
public class CurveTestComponent : Component
{
    [Show] private Curve _curve;
    public override void Awake()
    {
        _curve = new Curve();
        base.Awake();
    }
}