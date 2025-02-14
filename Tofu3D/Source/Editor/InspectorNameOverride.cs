namespace TofuEngine;

[InspectorNameOverride("CustomName")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public sealed class InspectorNameOverride : Show
{
    public string Name;

    public InspectorNameOverride(string name)
    {
        Name = name;
    }
}