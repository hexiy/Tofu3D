// Per scene

public class SceneFogManager
{
    private readonly Scene _scene;
    private Fog _fog;

    public SceneFogManager(Scene scene)
    {
        _scene = scene;
    }

    public bool FogEnabled => _fog?.IsActive == true;

    public Color FogColor1 => _fog.Color1;
    public Color FogColor2 => _fog.Color2;

    public float FogStartDistance => _fog.StartDistance;

    public float FogEndDistance => _fog.EndDistance;
    public float FogPositionY => _fog.PositionY;

    public float GradientSmoothness => _fog.GradientSmoothness;

    public bool IsGradient => _fog.IsGradient;
    public float Intensity => _fog.Intensity;

    public void Update()
    {
        if (_fog == null ||
            _fog?.GameObject?.Id ==
            -1) // id shenanigans for when we delete/create new component, this should be handled globally for every component that references other Components/GameObjects and the references should be nulled, i think
        {
            _fog = _scene.FindComponent<Fog>();
        }
    }
}