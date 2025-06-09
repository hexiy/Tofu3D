using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerVector2 : InspectorFieldDrawable<Vector2>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        System.Numerics.Vector2 value = GetValue(info, componentInspectorData);

        info.GetCustomAttribute<NumberRangeLimiterAttribute>(out NumberRangeLimiterAttribute? rangeLimiterAttribute);

        if (rangeLimiterAttribute != null)
        {
            float min = (rangeLimiterAttribute.MinValueInt ?? rangeLimiterAttribute.MinValueFloat) ?? float.MinValue;
            float max = (rangeLimiterAttribute.MaxValueInt ?? rangeLimiterAttribute.MaxValueFloat) ?? float.MaxValue;

            value.X = Mathf.Clamp(value.X, min, max);
            value.Y = Mathf.Clamp(value.Y, min, max);
        }

        if (ImGui.DragFloat2("", ref value, 0.01f))
        {
            SetValue(info, componentInspectorData, value);
        }
    }
}