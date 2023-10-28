namespace Tofu3D;

public class RenderableComponentQueue : IComponentQueue
{
    // bool _renderQueueChanged;
    private List<IComponentRenderable> _components = new();

    // public List<Renderer> RenderQueueWorld { get; private set; } = new();
    // public List<Renderer> RenderQueueUI { get; private set; } = new();

    public RenderableComponentQueue()
    {
        Scene.ComponentEnabled += OnComponentEnabled;
        Scene.ComponentDisabled += OnComponentDisabled;

        Scene.SceneStartedDisposing += OnSceneStartedDisposing;
        Scene.SceneDisposed += OnSceneDisposed;
    }

    private void OnSceneDisposed()
    {
        Scene.ComponentEnabled += OnComponentEnabled;
        Scene.ComponentDisabled += OnComponentDisabled;
    }

    private void OnSceneStartedDisposing()
    {
        Scene.ComponentEnabled -= OnComponentEnabled;
        Scene.ComponentDisabled -= OnComponentDisabled;
        ClearList();
    }

    private void ClearList()
    {
        _components.Clear();
    }

    public void AddComponent(IComponentRenderable component)
    {
        _components.Add(component);
    }

    public void RemoveComponent(IComponentRenderable component)
    {
        _components.Remove(component);
    }

    public void OnComponentEnabled(Component component)
    {
        if (component is IComponentRenderable componentRenderable) _components.Add(componentRenderable);
    }

    public void OnComponentDisabled(Component component)
    {
        if (component is IComponentRenderable componentRenderable) _components.Remove(componentRenderable);
    }

    public void RenderWorld()
    {
        for (int i = 0; i < _components.Count; i++)
            // RenderQueueWorld[i].UpdateMvp();
            _components[i].Render();
    }
// public void Update()
// {
// 	if (_renderQueueChanged)
// 	{
// 		RebuildRenderQueue();
// 		_renderQueueChanged = false;
// 	}
// 	else if (Time.EditorElapsedTicks % 20 == 0)
// 	{
// 		// SortRenderQueue();
// 	}
// }
//
// public void RenderQueueChanged()
// {
// 	_renderQueueChanged = true;
// }

// in the future just add the added component to queue, no need to rebuild the whole thing
// private void RebuildRenderQueue()
// {
// 	RenderQueueWorld = new List<Renderer>();
// 	for (int i = 0; i < _scene.GameObjects.Count; i++)
// 	{
// 		if (_scene.GameObjects[i].GetComponent<Renderer>())
// 		{
// 			if (_scene.GameObjects[i].Transform.Parent?.GetComponent<Canvas>() != null)
// 			{
// 				RenderQueueUI.AddRange(_scene.GameObjects[i].GetComponents<Renderer>());
// 			}
// 			else
// 			{
// 				if (_scene.GameObjects[i] == TransformHandle.I.GameObject)
// 				{
// 					continue;
// 				}
//
// 				RenderQueueWorld.AddRange(_scene.GameObjects[i].GetComponents<Renderer>());
// 			}
// 		}
// 	}
//
// 	SortRenderQueue();
// }
//
// public void SortRenderQueue()
// {
// 	return;
// 	RenderQueueWorld.Sort();
// 	RenderQueueUI.Sort();
// }

// public void RenderUI()
// {
// 	for (int i = 0; i < RenderQueueUI.Count; i++)
// 	{
// 		if (RenderQueueUI[i].CanRender)
// 		{
// 			// RenderQueueUI[i].UpdateMvp();
// 			RenderQueueUI[i].Render();
// 		}
// 	}
// }
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