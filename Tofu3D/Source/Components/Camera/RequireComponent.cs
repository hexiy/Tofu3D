namespace TofuEngine;

[RequireComponent(typeof(Component))]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public class RequireComponentAttribute : Attribute
{
    public Type RequiredComponentType { get; init; }

    public RequireComponentAttribute(Type t)
    {
        RequiredComponentType =t;
    }
}