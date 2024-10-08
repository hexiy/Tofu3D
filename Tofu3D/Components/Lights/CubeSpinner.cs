[ExecuteInEditMode]
public class CubeSpinner : Component, IComponentUpdateable
{
    public void Update()
    {
        Transform.Rotation = Transform.Rotation.Add(y: Time.EditorDeltaTime * 40);
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }
}