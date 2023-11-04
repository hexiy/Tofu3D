namespace Tofu3D;

[ExecuteInEditMode]
public class CurveTestComponent : Component, IComponentUpdateable
{
    [Show]
    private Curve _curve;

    [Header("1")]
    public float AA { get; } = 1f;
    [Show]
    private float p;

    [Space]
    public float x;

    [Header("2")]
    public float BB { get; } = 1f;
    
    
    
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