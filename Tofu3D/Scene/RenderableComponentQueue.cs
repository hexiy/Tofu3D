namespace Tofu3D;

public class RenderableComponentQueue : IComponentQueue
{
    // bool _renderQueueChanged;
    private readonly List<IComponentRenderable> _opaqueRenderables = new();
    private readonly List<IComponentRenderable> _transparentRenderables = new();
    private readonly List<IComponentRenderable> _opaqueRenderablesToRemove = new();
    private readonly List<IComponentRenderable> _transparentRenderablesToRemove = new();

    // public List<Renderer> RenderQueueWorld { get; private set; } = new();
    // public List<Renderer> RenderQueueUI { get; private set; } = new();

    public RenderableComponentQueue()
    {
        Scene.ComponentEnabled += OnComponentEnabled;
        Scene.ComponentDisabled += OnComponentDisabled;

        Scene.SceneStartedDisposing += OnSceneStartedDisposing;
    }

    public void OnComponentEnabled(Component component)
    {
        if (component is IComponentRenderable componentRenderable)
        {
            if (componentRenderable.RenderMode == RenderMode.Opaque)
            {
                _opaqueRenderables.Add(componentRenderable);
            }
            else
            {
                _transparentRenderables.Add(componentRenderable);
            }
        }
    }

    public void OnComponentDisabled(Component component)
    {
        if (component is IComponentRenderable componentRenderable)
        {
            if (componentRenderable.RenderMode == RenderMode.Opaque)
            {
                _opaqueRenderables.Remove(componentRenderable);
            }
            else
            {
                _transparentRenderables.Remove(componentRenderable);
            }
        }
    }

    private void OnSceneStartedDisposing()
    {
        Scene.ComponentEnabled -= OnComponentEnabled;
        Scene.ComponentDisabled -= OnComponentDisabled;
        ClearList();
    }

    private void ClearList()
    {
        _opaqueRenderables.Clear();
        _transparentRenderables.Clear();
    }

    public void AddComponent(IComponentRenderable component)
    {
        if (component.RenderMode == RenderMode.Opaque)
        {
            if (_opaqueRenderables.Contains(component))
            {
                return;
            }

            _opaqueRenderables.Add(component);
        }
        else
        {
            _transparentRenderables.Add(component);
        }
    }

    public void QueueRemove(IComponentRenderable component)
    {
        if (component.RenderMode == RenderMode.Opaque)
        {
            _opaqueRenderablesToRemove.Add(component);
        }
        else
        {
            _transparentRenderablesToRemove.Add(component);
        }
    }

    // public void RenderAll()
    // {
    //     RenderOpaques();
    //     RenderTransparency();
    // }

    public void UploadRenderDataOpaques()
    {
        Debug.StatSetValue("Renderable queue components",
            $"Renderable queue components: {_opaqueRenderables.Count + _transparentRenderables.Count}");

        // _opaqueRenderables.Sort();

        for (int i = 0; i < _opaqueRenderables.Count; i++)
        {
            _opaqueRenderables[i].UploadRenderData();
        }

        for (int i = 0; i < _opaqueRenderablesToRemove.Count; i++)
        {
            _opaqueRenderables.Remove(_opaqueRenderablesToRemove[i]);
        }

        if (_opaqueRenderablesToRemove.Count > 0)
        {
            _opaqueRenderablesToRemove.Clear();
        }
    }

    public void UploadRenderDataTransparency()
    {
        // _transparentRenderables.Sort();
        for (int i = 0; i < _transparentRenderables.Count; i++)
        {
            _transparentRenderables[i].UploadRenderData();
        }

        for (int i = 0; i < _transparentRenderablesToRemove.Count; i++)
        {
            _transparentRenderables.Remove(_transparentRenderablesToRemove[i]);
        }

        if (_transparentRenderablesToRemove.Count > 0)
        {
            _transparentRenderablesToRemove.Clear();
        }
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