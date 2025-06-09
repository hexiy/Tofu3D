[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class Canvas : Component
{
    public override void Awake()
    {
        foreach (Transform transformChild in Transform.Children)
        {
            if (transformChild.GetComponent<Renderer>(out var renderer))
            {
                renderer.Material.RenderMode = RenderMode.UI;
                renderer.NeedsToSetupMaterial = false;
            }
        }

        base.Awake();
    }
}