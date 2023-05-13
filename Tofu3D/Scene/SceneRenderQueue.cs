namespace Tofu3D;

public class SceneRenderQueue
{
	bool _renderQueueChanged;
	public List<Renderer> RenderQueueWorld { get; private set; } = new();
	public List<Renderer> RenderQueueUI { get; private set; } = new();
	readonly Scene _scene;

	public SceneRenderQueue(Scene scene)
	{
		_scene = scene;
		Scene.ComponentAdded += component => RenderQueueChanged();
		Scene.SceneModified += RenderQueueChanged;
	}

	public void Update()
	{
		if (_renderQueueChanged)
		{
			RebuildRenderQueue();
			_renderQueueChanged = false;
		}
		else if (Time.EditorElapsedTicks % 20 == 0)
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
		RenderQueueWorld = new List<Renderer>();
		for (int i = 0; i < _scene.GameObjects.Count; i++)
		{
			if (_scene.GameObjects[i].GetComponent<Renderer>())
			{
				if (_scene.GameObjects[i].Transform.Parent?.GetComponent<Canvas>() != null)
				{
					RenderQueueUI.AddRange(_scene.GameObjects[i].GetComponents<Renderer>());
				}
				else
				{
					if (_scene.GameObjects[i] == TransformHandle.I.GameObject)
					{
						continue;
					}

					RenderQueueWorld.AddRange(_scene.GameObjects[i].GetComponents<Renderer>());
				}
			}
		}

		SortRenderQueue();
	}

	public void SortRenderQueue()
	{
		RenderQueueWorld.Sort();
		RenderQueueUI.Sort();
	}

	public void RenderWorld()
	{
		for (int i = 0; i < RenderQueueWorld.Count; i++)
		{
			if (RenderQueueWorld[i].CanRender)
			{
				// RenderQueueWorld[i].UpdateMvp();
				RenderQueueWorld[i].Render();
			}
		}
	}

	public void RenderUI()
	{
		for (int i = 0; i < RenderQueueUI.Count; i++)
		{
			if (RenderQueueUI[i].CanRender)
			{
				// RenderQueueUI[i].UpdateMvp();
				RenderQueueUI[i].Render();
			}
		}
	}
	// public void RenderOpaques()
	// {
	// 	// Debug.ClearLogs();
	//
	// 	for (int i = 0; i < RenderQueue.Count; i++)
	// 	{
	// 		if (RenderQueue[i].Enabled && RenderQueue[i].GameObject.Awoken && RenderQueue[i].GameObject.ActiveInHierarchy && RenderQueue[i].RenderMode== RenderMode.Opaque)
	// 		{
	// 			// Debug.Log($"Rendering {RenderQueue[i].GameObject.Name}");
	// 			RenderQueue[i].UpdateMvp();
	// 			RenderQueue[i].Render();
	// 		}
	// 	}
	// }
	//
	// public void RenderTransparent()
	// {
	// 	for (int i = 0; i < RenderQueue.Count; i++)
	// 	{
	// 		if (RenderQueue[i].Enabled && RenderQueue[i].GameObject.Awoken && RenderQueue[i].GameObject.ActiveInHierarchy && RenderQueue[i].RenderMode == RenderMode.Transparent)
	// 		{
	// 			// Debug.Log($"Rendering {RenderQueue[i].GameObject.Name}");
	// 			RenderQueue[i].UpdateMvp();
	// 			RenderQueue[i].Render();
	// 		}
	// 	}
	// }
}