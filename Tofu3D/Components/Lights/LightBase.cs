public class LightBase : Component
{
    public Color Color = Color.White;

    [SliderF(0, 20)] public float Intensity = 1;
}