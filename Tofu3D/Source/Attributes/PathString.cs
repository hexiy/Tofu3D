namespace TofuEngine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class PathString : Attribute
{
    public bool DisplayNameOnly;
    public PathString() : this(displayNameOnly: false)
    {
        
    }
    public PathString(bool displayNameOnly)
    {
        DisplayNameOnly = displayNameOnly;
    }
}