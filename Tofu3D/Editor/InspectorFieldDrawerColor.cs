using System.Linq;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerColor : InspectorFieldDrawable<Color>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        System.Numerics.Vector4 fieldValue =
            ((Color)info.GetValue(componentInspectorData.Inspectable)).ToVector4();

        var hasColor3Attribute =
            info.CustomAttributes.Count(data => data.AttributeType == typeof(Color3Attrib)) > 0;
        var changed = false;
        if (hasColor3Attribute)
        {
            System.Numerics.Vector3 vec3 = Extensions.ToVector3(fieldValue);

            changed = ImGui.ColorEdit3("", ref vec3);
            fieldValue = new System.Numerics.Vector4(vec3.X, vec3.Y, vec3.Z, fieldValue.W);
        }
        else
        {
            changed = ImGui.ColorEdit4("", ref fieldValue);
        }

        if (changed)
        {
            info.SetValue(componentInspectorData.Inspectable, fieldValue.ToColor());
        }
    }
}