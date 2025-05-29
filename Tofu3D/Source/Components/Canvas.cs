[ExecuteInEditMode]
public class Canvas : Component, IComponentUpdateable
{
    public void Update()
    {
        return;
    }

    public override void Awake()
    {
        foreach (Transform transformChild in Transform.Children)
        {
            if (transformChild.GetComponent<Renderer>(out var renderer))
            {
                renderer.Material.RenderMode = RenderMode.UI;
            }
        }

        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }
}