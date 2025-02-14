namespace Tofu3D;

[ShowIfNot(null)]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public sealed class ShowIfNot : Show
{
    public string FieldName;

    public ShowIfNot(string fieldName)
    {
        FieldName = fieldName;
    }
}