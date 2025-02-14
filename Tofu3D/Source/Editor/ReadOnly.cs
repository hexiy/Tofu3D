namespace TofuEngine;

[ReadOnly]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public class ReadOnly : Attribute
{
}