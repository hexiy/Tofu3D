using System.Linq;
using ImGuiNET;

namespace TofuEngine;

public class InspectorFieldDrawerColor : InspectorFieldDrawable<Color>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        System.Numerics.Vector4 fieldValue =
            ((Color)info.GetValue(componentInspectorData.Inspectable)).ToVector4();
        bool changed = false;


       

        bool hasColor3Attribute = info.GetCustomAttribute<Color3Attribute>(out Color3Attribute? color3Attrib);
        if (hasColor3Attribute)
        {
            System.Numerics.Vector3 vec3 = Extensions.ToVector3(fieldValue);
        
            changed = ImGui.ColorEdit3("", ref vec3);
            fieldValue = new System.Numerics.Vector4(vec3.X, vec3.Y, vec3.Z, fieldValue.W);
        }
        else
        {
            bool isHDRColor =
                info.GetCustomAttribute<ColorHDRAttribute>(out ColorHDRAttribute? colorHDRAttrib);
            ImGuiColorEditFlags flags = isHDRColor ? ImGuiColorEditFlags.HDR : ImGuiColorEditFlags.None;

            changed = ImGui.ColorEdit4("", ref fieldValue, flags);
        }

        if (changed)
        {
            info.SetValue(componentInspectorData.Inspectable, fieldValue.ToColor());
        }
    }
}