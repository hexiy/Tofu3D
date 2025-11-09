using System.Linq;

namespace TofuEngine;

public class RenderableComponentQueue : IComponentQueue
{
    // bool _renderQueueChanged;
    private readonly HashSet<IComponentRenderable> _opaqueRenderables = new HashSet<IComponentRenderable>();
    private readonly HashSet<IComponentRenderable> _transparentRenderables = new HashSet<IComponentRenderable>();
    private readonly HashSet<IComponentRenderable> _uiRenderables = new HashSet<IComponentRenderable>();
    private readonly HashSet<IComponentRenderable> _opaqueRenderablesToRemove = new HashSet<IComponentRenderable>();

    private readonly HashSet<IComponentRenderable>
        _transparentRenderablesToRemove = new HashSet<IComponentRenderable>();

    private readonly HashSet<IComponentRenderable> _uiRenderablesToRemove = new HashSet<IComponentRenderable>();

    private readonly object _transparentLock = new();
    private readonly object _opaqueLock = new();

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
                lock (_opaqueLock)
                {
                    _opaqueRenderables.Add(componentRenderable);
                }
            }
            else if (componentRenderable.RenderMode == RenderMode.Transparent)
            {
                lock (_transparentLock)
                {
                    _transparentRenderables.Add(componentRenderable);
                }
            }
            else if (componentRenderable.RenderMode == RenderMode.UI)
            {
                _uiRenderables.Add(componentRenderable);
            }
        }
    }

    public void OnComponentDisabled(Component component)
    {
        if (component is IComponentRenderable componentRenderable)
        {
            if (componentRenderable.RenderMode == RenderMode.Opaque)
            {
                lock (_opaqueLock)
                {
                    _opaqueRenderables.Remove(componentRenderable);
                }
            }
            else if (componentRenderable.RenderMode == RenderMode.Transparent)
            {
                lock (_transparentLock)
                {
                    _transparentRenderables.Remove(componentRenderable);
                }
            }
            else if (componentRenderable.RenderMode == RenderMode.UI)
            {
                _uiRenderables.Remove(componentRenderable);
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
        _uiRenderables.Clear();
    }

    public void AddComponent(IComponentRenderable component)
    {
        if (component.RenderMode == RenderMode.Opaque)
        {
            lock (_opaqueLock)
            {
                if (_opaqueRenderables.Contains(component))
                {
                    return;
                }

                _opaqueRenderables.Add(component);
            }
        }
        else if (component.RenderMode == RenderMode.Transparent)
        {
            lock (_transparentLock)
            {
                _transparentRenderables.Add(component);
            }
        }
        else if (component.RenderMode == RenderMode.UI)
        {
            _uiRenderables.Add(component);
        }
    }

    public void QueueRemove(IComponentRenderable component)
    {
        if (component.RenderMode == RenderMode.Opaque)
        {
            _opaqueRenderablesToRemove.Add(component);
        }
        else if (component.RenderMode == RenderMode.Transparent)
        {
            _transparentRenderablesToRemove.Add(component);
        }
        else if (component.RenderMode == RenderMode.UI)
        {
            _uiRenderablesToRemove.Add(component);
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

        IComponentRenderable[] snapshot;
        lock (_opaqueLock)
        {
            snapshot = _opaqueRenderables.ToArray();
        }

        foreach (IComponentRenderable renderable in snapshot)
        {
            renderable.UploadRenderData();
        }


        lock (_opaqueLock)
        {
            if (_opaqueRenderablesToRemove.Count > 0)
            {
                foreach (var r in _opaqueRenderablesToRemove)
                    _opaqueRenderables.Remove(r);

                _opaqueRenderablesToRemove.Clear();
            }
        }
    }

    public void UploadRenderDataTransparency()
    {
        // _transparentRenderables.Sort();

        IComponentRenderable[] snapshot;
        lock (_transparentLock)
        {
            snapshot = _transparentRenderables.ToArray();
        }

        foreach (IComponentRenderable renderable in snapshot)
        {
            renderable.UploadRenderData();
        }


        lock (_transparentLock)
        {
            if (_transparentRenderablesToRemove.Count > 0)
            {
                foreach (var r in _transparentRenderablesToRemove)
                    _transparentRenderables.Remove(r);

                _transparentRenderablesToRemove.Clear();
            }
        }
    }

    public void UploadRenderDataUI()
    {
        // _transparentRenderables.Sort();

        foreach (IComponentRenderable renderable in _uiRenderables.ToArray())
        {
            renderable.UploadRenderData();
        }

        foreach (IComponentRenderable renderableToRemove in _uiRenderablesToRemove)
        {
            _uiRenderables.Remove(renderableToRemove);
        }

        _uiRenderablesToRemove.Clear();

        if (_uiRenderablesToRemove.Count > 0)
        {
            _uiRenderablesToRemove.Clear();
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