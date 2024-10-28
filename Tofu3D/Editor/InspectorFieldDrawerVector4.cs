using System.Linq;
using System.Reflection;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerVector4 : InspectorFieldDrawable<Vector4>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {

        float cursorPosX = ImGui.GetCursorPosX();
        float width = ImGui.GetContentRegionAvail().X;
        Vector4 v4 = (Vector4)info.GetValue(componentInspectorData.Inspectable);
        var changed = false;

        System.Numerics.Vector3 vec3 = v4.ToVector3();

        ColorHDR colorHdr = info.GetCustomAttribute<ColorHDR>();
        var isHDRColor = colorHdr != null;
        if (isHDRColor)
        {
            ImGui.SetNextItemWidth(width);

            changed = ImGui.ColorEdit3("", ref vec3, ImGuiColorEditFlags.None);
            
            ImGui.SetCursorPosX(cursorPosX);
            ImGui.SetNextItemWidth(width);
            changed = changed || ImGui.SliderFloat("", ref v4.W, colorHdr.MinIntensity, colorHdr.MaxIntensity);
        }
        else
        {
        }

        if (changed)
        {
            v4 = new Vector4(vec3.X, vec3.Y, vec3.Z, v4.W);

            info.SetValue(componentInspectorData.Inspectable, v4.ToNumerics().ToVector4());
        }
    }
}