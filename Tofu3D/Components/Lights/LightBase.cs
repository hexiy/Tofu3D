public class LightBase : Component
{
	[SliderF(0,3)]
	public float Intensity = 1;
	public Color Color = Tofu3D.Color.White;
}