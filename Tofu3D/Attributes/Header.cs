namespace Tofu3D;

[Header("Header")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public sealed class Header : Show
{
    public string Text;

    public Header(string text)
    {
        Text = text;
    }
}