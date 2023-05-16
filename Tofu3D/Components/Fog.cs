[ExecuteInEditMode]
public class Fog : Component, IComponentUpdateable
{
	public float StartDistance = 10;
	public float EndDistance = 20;
	public float PositionY = 0;
	public bool IsGradient = false;

	// [SliderF(0.001f, 500)]
	[ShowIf(nameof(IsGradient))]
	public float GradientSmoothness = 0.001f;

	public Color Color1 = Color.White;

	[ShowIf(nameof(IsGradient))]
	public Color Color2 = Color.Black;

	[SliderF(0, 1)]
	public float Intensity = 1;

	public override void Update()
	{
		EndDistance = Mathf.ClampMin(EndDistance, StartDistance + 0.001f);

		base.Update();
	}
}