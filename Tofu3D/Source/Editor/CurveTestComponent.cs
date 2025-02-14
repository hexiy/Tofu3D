namespace Tofu3D;

[ExecuteInEditMode]
public class CurveTestComponent : Component, IComponentUpdateable
{
    [Show] private Curve _curve;

    [Show] private float p;

    [Space] public float x;

    [Header("1")] public float AA { get; } = 1f;

    [Header("2")] public float BB { get; } = 1f;

    public void Update()
    {
        // Debug.Log(_curve.Sample(p));
    }


    public override void Awake()
    {
        _curve = new Curve();
        base.Awake();
    }
}