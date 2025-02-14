namespace TofuEngine;

public interface IInspectorFieldDrawable
{
    public void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData);
}