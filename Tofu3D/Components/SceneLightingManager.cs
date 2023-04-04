// Per scene

public class SceneLightingManager
{
	List<LightBase> _lights = new List<LightBase>();

	DirectionalLight _directionalLight;
	readonly Scene _scene;

	public static SceneLightingManager I { get; private set; }

	public SceneLightingManager(Scene scene)
	{
		I = this;
		_scene = scene;
	}

	public void Update()
	{
		_lights = GetAllLightsInScene();
		_directionalLight = _scene.FindComponent<DirectionalLight>(ignoreInactive: true);


		// Debug.Log($"Light direction:{GetDirectionalLightDirection()}");
	}

	private List<LightBase> GetAllLightsInScene()
	{
		return _scene.FindComponentsInScene<LightBase>(ignoreInactive: true);
	}

	public float[] GetPointLightsPositions()
	{
		List<float> floats = new List<float>();
		for (int i = 0; i < _lights.Count; i++)
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
		List<float> floats = new List<float>();
		for (int i = 0; i < _lights.Count; i++)
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
		List<float> floats = new List<float>();
		for (int i = 0; i < _lights.Count; i++)
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
		Color col = new Color(0, 0, 0, 1);


		for (int i = 0; i < _lights.Count; i++)
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
		for (int i = 0; i < _lights.Count; i++)
		{
			if (_lights[i].GetType() == typeof(AmbientLight))
			{
				intensity += _lights[i].Intensity;
			}
		}

		return Mathf.ClampMin(intensity, 0);
	}

	public Color GetDirectionalLightColor()
	{
		return _directionalLight?.Color ?? Color.Black;
	}

	public Vector3 GetDirectionalLightDirection()
	{
		return _directionalLight?.Transform.Forward ?? Vector3.Zero;
	}

	public Vector3 GetDirectionalLightPosition()
	{
		return _directionalLight?.Transform.WorldPosition ?? Vector3.Zero;
	}

	public float GetDirectionalLightIntensity()
	{
		return _directionalLight?.Intensity ?? 0;
	}
}