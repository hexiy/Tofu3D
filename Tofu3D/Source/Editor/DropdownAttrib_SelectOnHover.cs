namespace TofuEngine;

[DropdownAttrib_SelectOnHover()]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public class DropdownAttrib_SelectOnHover : Attribute
{
    public DropdownAttrib_SelectOnHover()
    {
    }
}