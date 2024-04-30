[ExecuteInEditMode]
public class CubeSpinner : Component, IComponentUpdateable
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }

    public void Update()
    {
        Transform.Rotation = Transform.Rotation.Add(y: Time.EditorDeltaTime * 20);
    }
}