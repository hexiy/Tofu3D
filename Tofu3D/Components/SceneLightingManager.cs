// Per scene

public class SceneLightingManager
{
    private readonly Scene _scene;

    private DirectionalLight _directionalLight;
    private List<LightBase> _lights = new();

    public SceneLightingManager(Scene scene)
    {
        I = this;
        _scene = scene;
        Scene.ComponentAwoken += OnSceneComponentAdded;
    }

    public static SceneLightingManager I { get; private set; }

    public void Update()
    {
    }

    private void OnSceneComponentAdded(Component obj)
    {
        if (_directionalLight == null)
        {
            _lights = _scene.FindComponentsInScene<LightBase>(true);

            _directionalLight = _scene.FindComponent<DirectionalLight>(true);
        }
    }

    public float[] GetPointLightsPositions()
    {
        List<float> floats = new();
        for (var i = 0; i < _lights.Count; i++)
        {
            if (_lights[i] is PointLight)
            {
                floats.Add(_lights[i].Transform.WorldPosition.X);
                floats.Add(_lights[i].Transform.WorldPosition.Y);
                floats.Add(_lights[i].Transform.WorldPosition.Z);
            }
        }


        return floats.ToArray();
    }

    public float[] GetPointLightsColors()
    {
        List<float> floats = new();
        for (var i = 0; i < _lights.Count; i++)
        {
            if (_lights[i] is PointLight)
            {
                floats.Add(_lights[i].Color.R / 255f);
                floats.Add(_lights[i].Color.G / 255f);
                floats.Add(_lights[i].Color.B / 255f);
            }
        }


        return floats.ToArray();
    }

    public float[] GetPointLightsIntensities()
    {
        List<float> floats = new();
        for (var i = 0; i < _lights.Count; i++)
        {
            if (_lights[i] is PointLight)
            {
                floats.Add(_lights[i].Intensity);
            }
        }

        return floats.ToArray();
    }

    public Color GetAmbientLightsColor()
    {
        Color col = new(0, 0, 0, 1);


        for (var i = 0; i < _lights.Count; i++)
        {
            if (_lights[i].GetType() == typeof(AmbientLight))
            {
                col += _lights[i].Color;
            }
        }

        return col;
    }

    public float GetAmbientLightsIntensity()
    {
        float intensity = 0;
        for (var i = 0; i < _lights.Count; i++)
        {
            if (_lights[i].GetType() == typeof(AmbientLight))
            {
                intensity += _lights[i].Intensity * (_lights[i].IsActive ? 1 : 0);
            }
        }

        return Mathf.ClampMin(intensity, 0);
    }

    public Color GetDirectionalLightColor() => _directionalLight?.Color ?? Color.Black;

    public Vector3 GetDirectionalLightDirection() => _directionalLight?.Transform.Forward ?? Vector3.Zero;

    public Vector3 GetDirectionalLightPosition() => _directionalLight?.Transform.WorldPosition ?? Vector3.Zero;

    public float GetDirectionalLightIntensity() =>
        (_directionalLight?.Intensity ?? 0) * (_directionalLight?.IsActive == true ? 1 : 0);
}