public class DemoButton : Component
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        GetComponent<Button>().OnReleasedAction += DemoManager.I.NextButtonClicked;

        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }
}