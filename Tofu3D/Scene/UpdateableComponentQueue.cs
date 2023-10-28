namespace Tofu3D;

public class UpdateableComponentQueue : IComponentQueue
{
    private List<IComponentUpdateable> _components = new();

    public UpdateableComponentQueue()
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

    public void UpdateComponents()
    {
        Debug.StatSetValue($"Update queue components", $"Update queue components: {_components.Count}");
        for (int i = 0; i < _components.Count; i++) _components[i].Update();
    }

    public void AddComponent(IComponentUpdateable component)
    {
        _components.Add(component);
    }

    public void RemoveComponent(IComponentUpdateable component)
    {
        _components.Remove(component);
    }

    public void OnComponentEnabled(Component component)
    {
        if (component is IComponentUpdateable componentUpdateable) AddComponent(componentUpdateable);
    }

    public void OnComponentDisabled(Component component)
    {
        if (component is IComponentUpdateable componentUpdateable) RemoveComponent(componentUpdateable);
    }
}