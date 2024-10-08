namespace Tofu3D;

public abstract class InspectorFieldDrawable<T> : IInspectorFieldDrawable
{
    public abstract void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData);

    internal T GetValue(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        var v = (T)info.GetValue(componentInspectorData.Inspectable);
        return v;
    }

    internal void SetValue(FieldOrPropertyInfo info, InspectableData componentInspectorData, T value)
    {
        info.SetValue(componentInspectorData.Inspectable, value);
    }
}