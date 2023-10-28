namespace Tofu3D;

[ExecuteInEditMode]
public class EnumInspectorTest : Component
{
    [Show]
    public ParticleColorType ParticleColorType1;

    [ShowIf(nameof(CanShow))]
    public int BBBBB = 1;

    private bool CanShow => ParticleColorType1 is ParticleColorType.Random;
}