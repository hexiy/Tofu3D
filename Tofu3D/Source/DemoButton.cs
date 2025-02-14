public class DemoButton : Component, IComponentUpdateable
{
    public void Update()
    {
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        GetComponent<Button>().OnReleasedAction += DemoManager.I.NextButtonClicked;

        base.Start();
    }
}