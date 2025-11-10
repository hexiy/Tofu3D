using System.Linq;

namespace TofuEngine;

public class UpdateableComponentQueue : IComponentQueue
{
    private readonly HashSet<IComponentUpdateable> _components = new HashSet<IComponentUpdateable>();
    private readonly HashSet<IComponentUpdateable> _componentsToRemove = new HashSet<IComponentUpdateable>();
    private readonly object _componentsLock = new();

    public UpdateableComponentQueue()
    {
        Scene.ComponentEnabled += OnComponentEnabled;
        Scene.ComponentDisabled += OnComponentDisabled;
        Scene.SceneStartedDisposing += OnSceneStartedDisposing;
        Scene.SceneLoaded += OnSceneLoaded;
    }

    public void OnComponentEnabled(Component component)
    {
        if (component is IComponentUpdateable componentUpdateable)
        {
            AddComponent(componentUpdateable);
        }
    }

    public void OnComponentDisabled(Component component)
    {
        if (component is IComponentUpdateable componentUpdateable)
        {
            QueueRemove(componentUpdateable);
        }
    }

    private void OnSceneLoaded()
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
        lock (_components)
        {
            _components.Clear();
        }
    }

    public void UpdateComponents()
    {
        Debug.StatSetValue("Update queue components", $"Update queue components: {_components.Count}");

        IComponentUpdateable[] snapshot;
        lock (_componentsLock)
        {
            snapshot = _components.ToArray();
        }

        foreach (IComponentUpdateable updateable in snapshot)
        {
            updateable.Update();
        }
        
        lock (_componentsLock)
        {
            if (_componentsToRemove.Count > 0)
            {
                foreach (var r in _componentsToRemove)
                    _components.Remove(r);

                _componentsToRemove.Clear();
            }
        }
    }

    public void AddComponent(IComponentUpdateable component)
    {
        lock (_componentsLock)
        {
            _components.Add(component);
        }
    }
    

    public void QueueRemove(IComponentUpdateable component)
    {
        _componentsToRemove.Add(component);
    }
}