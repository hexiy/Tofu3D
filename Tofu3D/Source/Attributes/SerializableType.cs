namespace TofuEngine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Delegate,
    Inherited = false)]
public sealed class SerializableType : Attribute
{
}