using System.Linq;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerColor : InspectorFieldDrawable<Color>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        System.Numerics.Vector4 fieldValue =
            ((Color)info.GetValue(componentInspectorData.Inspectable)).ToVector4();
        var changed = false;


        // var hasColor3Attribute =
        //     info.CustomAttributes.Count(data => data.AttributeType == typeof(Color3Attrib)) > 0;
        // if (hasColor3Attribute)
        // {
        //     System.Numerics.Vector3 vec3 = Extensions.ToVector3(fieldValue);
        //
        //     changed = ImGui.ColorEdit3("", ref vec3);
        //     fieldValue = new System.Numerics.Vector4(vec3.X, vec3.Y, vec3.Z, fieldValue.W);
        // }
        var isHDRColor =
            info.CustomAttributes.Count(data => data.AttributeType == typeof(ColorHDR)) > 0;
        ImGuiColorEditFlags flags = isHDRColor ? ImGuiColorEditFlags.HDR : ImGuiColorEditFlags.None;

        changed = ImGui.ColorEdit4("", ref fieldValue, flags);

        if (changed)
        {
            info.SetValue(componentInspectorData.Inspectable, fieldValue.ToColor());
        }
    }
}