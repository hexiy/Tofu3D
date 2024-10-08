[ExecuteInEditMode]
public class Fog : Component, IComponentUpdateable
{
    public Color Color1 = Color.White;

    [ShowIf(nameof(IsGradient))] public Color Color2 = Color.Black;

    public float EndDistance = 20;

    // [SliderF(0.001f, 500)]
    [ShowIf(nameof(IsGradient))] public float GradientSmoothness = 0.001f;

    [SliderF(0, 1)] public float Intensity = 1;

    public bool IsGradient = false;
    public float PositionY = 0;
    public float StartDistance = 10;

    public void Update()
    {
        EndDistance = Mathf.ClampMin(EndDistance, StartDistance + 0.001f);
    }
}