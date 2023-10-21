namespace Tofu3D;

[ExecuteInEditMode]
public class CurveTestComponent : Component, IComponentUpdateable
{
    [Show] private Curve _curve;
    [Show] private float p;
    public override void Awake()
    {
        _curve = new Curve();
        base.Awake();
    }

    public override void Update()
    {
        // Debug.Log(_curve.Sample(p));
        base.Update();
    }
}