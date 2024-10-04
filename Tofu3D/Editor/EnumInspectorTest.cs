namespace Tofu3D;

[ExecuteInEditMode]
public class EnumInspectorTest : Component
{
    [ShowIf(nameof(CanShow))] public int BBBBB = 1;

    [Show] public ParticleColorType ParticleColorType1;

    private bool CanShow => ParticleColorType1 is ParticleColorType.Random;
}