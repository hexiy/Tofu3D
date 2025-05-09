namespace TofuEngine;

[ShowIf(null)]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public sealed class ShowIfAttribute : ShowAttribute
{
    public string FieldName;

    public ShowIfAttribute(string fieldName)
    {
        FieldName = fieldName;
    }
}