// Per scene

public class SceneFogManager
{
    private readonly Scene _scene;
    private Fog _fog;

    public SceneFogManager(Scene scene)
    {
        _scene = scene;

        Scene.ComponentAwoken += OnComponentAwoken;
    }

    private void OnComponentAwoken(Component component)
    {
        if (component is Fog)
        {
            _fog = component as Fog;
        }
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
}