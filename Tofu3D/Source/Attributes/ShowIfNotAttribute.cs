namespace TofuEngine;

[ShowIfNot(null)]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public sealed class ShowIfNotAttribute : ShowAttribute
{
    public string FieldName;

    public ShowIfNotAttribute(string fieldName)
    {
        FieldName = fieldName;
    }
}