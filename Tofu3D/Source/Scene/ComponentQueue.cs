namespace TofuEngine;

public interface IComponentQueue
{
    // public void AddComponent(Component component);
    //
    // public void RemoveComponent(Component component);

    public void OnComponentEnabled(Component component);

    public void OnComponentDisabled(Component component);
}