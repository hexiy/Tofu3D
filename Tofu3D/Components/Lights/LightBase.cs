public class LightBase : Component
{
    [SliderF(0, 20)]
    public float Intensity = 1;

    public Color Color = Color.White;
}