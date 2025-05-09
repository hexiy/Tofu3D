namespace TofuEngine;

[Dropdown_SelectOnHover()]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public class Dropdown_SelectOnHoverAttribute : Attribute
{
    public Dropdown_SelectOnHoverAttribute()
    {
    }
}