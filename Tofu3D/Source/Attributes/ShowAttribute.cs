namespace TofuEngine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class ShowAttribute : Attribute
{
}