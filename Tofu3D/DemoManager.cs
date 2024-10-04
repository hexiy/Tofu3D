public class DemoManager : Component
{
    private int _currentIndex;

    public List<GameObject> DemoGameObjects = new();
    public static DemoManager I { get; private set; }

    public override void Awake()
    {
        I = this;

        base.Awake();
    }

    public void NextButtonClicked()
    {
        MoveToNextDemo();
    }

    private void MoveToNextDemo()
    {
        _currentIndex++;
        if (_currentIndex >= DemoGameObjects.Count)
        {
            _currentIndex = 0;
            return;
        }

        Tofu.SceneViewController.MoveToGameObject(DemoGameObjects[_currentIndex]);
        //Camera.I.Transform.LocalPosition = DemoGameObjects[_currentIndex].Transform.LocalPosition;
    }
}