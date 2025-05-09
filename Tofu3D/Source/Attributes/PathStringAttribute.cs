namespace TofuEngine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class PathStringAttribute : Attribute
{
    public bool DisplayNameOnly;
    public PathStringAttribute() : this(displayNameOnly: false)
    {
        
    }
    public PathStringAttribute(bool displayNameOnly)
    {
        DisplayNameOnly = displayNameOnly;
    }
}