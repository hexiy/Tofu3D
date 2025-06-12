[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class Canvas : Component, IComponentUpdateable
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

        RectTransform.Size = Camera.GameViewCamera.Size;
        RectTransform.Pivot = Vector3.Zero;

        base.Awake();
    }

    public void Update()
    {
        Transform.Rotation = new Vector3(90, 0, 0);
        RectTransform.CalculateLayoutForSelfAndChildren();
    }
}