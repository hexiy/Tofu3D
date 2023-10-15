public class DemoManager : Component
{
	public static DemoManager I { get; private set; }

	public List<GameObject> DemoGameObjects = new List<GameObject>();
	private int _currentIndex = 0;

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