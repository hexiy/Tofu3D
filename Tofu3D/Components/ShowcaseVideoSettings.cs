[ExecuteInEditMode]
public class ShowcaseVideoSettings : Component
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        Camera.MainCamera.OrthographicSize = 1.6f;
        base.Start();
    }

    public void Update()
    {
    }
}