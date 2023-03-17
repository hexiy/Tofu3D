namespace Tofu3D;

public class SceneRenderQueue
{
	bool _renderQueueChanged;
	public List<Renderer> RenderQueue { get; private set; } = new();
	readonly Scene _scene;

	public SceneRenderQueue(Scene scene)
	{
		_scene = scene;
		Scene.AnyComponentAddedToScene += component => RenderQueueChanged();
	}

	public void Update()
	{
		if (_renderQueueChanged)
		{
			RebuildRenderQueue();
			_renderQueueChanged = false;
		}
		else if (Time.ElapsedTicks % 20 == 0)
		{
			SortRenderQueue();
		}
	}

	public void RenderQueueChanged()
	{
		_renderQueueChanged = true;
	}

	// in the future just add the added component to queue, no need to rebuild the whole thing
	private void RebuildRenderQueue()
	{
		RenderQueue = new List<Renderer>();
		for (int i = 0; i < _scene.GameObjects.Count; i++)
		{
			if (_scene.GameObjects[i].GetComponent<Renderer>())
			{
				RenderQueue.AddRange(_scene.GameObjects[i].GetComponents<Renderer>());
			}
		}

		SortRenderQueue();
	}

	public void SortRenderQueue()
	{
		RenderQueue.Sort();
	}
}