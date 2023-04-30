// Per scene

public class SceneFogManager
{
	Fog _fog;
	readonly Scene _scene;

	public SceneFogManager(Scene scene)
	{
		_scene = scene;
	}

	public void Update()
	{
		if (_fog == null || _fog?.GameObject?.Id == -1) // id shenanigans for when we delete/create new component, this should be handled globally for every component that references other Components/GameObjects and the references should be nulled, i think
		{
			_fog = _scene.FindComponent<Fog>();
		}
	}

	public bool FogEnabled
	{
		get { return _fog?.IsActive == true; }
	}
	public Color FogColor1
	{
		get { return _fog.Color1; }
	}
	public Color FogColor2
	{
		get { return _fog.Color2; }
	}
	public float FogStartDistance
	{
		get { return _fog.StartDistance; }
	}
	public float FogEndDistance
	{
		get { return _fog.EndDistance; }
	}
	public float FogPositionY
	{
		get { return _fog.PositionY; }
	}
	public float GradientSmoothness
	{
		get { return _fog.GradientSmoothness; }
	}
}