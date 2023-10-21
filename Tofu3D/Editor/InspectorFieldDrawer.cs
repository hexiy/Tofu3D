namespace Tofu3D;

public abstract class InspectorFieldDrawable<T> : IInspectorFieldDrawable
{
    internal T GetValue(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        T v = (T)info.GetValue(componentInspectorData.Inspectable);
        return v;
    }

    internal void SetValue(FieldOrPropertyInfo info, InspectableData componentInspectorData, T value)
    {
        info.SetValue(componentInspectorData.Inspectable, value);
    }

    public abstract void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData);
}