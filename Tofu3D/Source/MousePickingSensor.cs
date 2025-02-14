namespace TofuEngine;

public class MousePickingSensor : Component, IComponentUpdateable
{
    Renderer _renderer;

    public override void Awake()
    {
        _renderer = GetComponent<Renderer>();
        base.Awake();
    }

    public void Update()
    {
        if (_renderer == MousePickingSystem.HoveredRenderer)
        {
            Transform.Rotation = new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100));
        }
    }
}