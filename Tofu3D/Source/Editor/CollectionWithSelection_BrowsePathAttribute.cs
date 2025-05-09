namespace TofuEngine;

[CollectionWithSelection_BrowsePath("")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public sealed class CollectionWithSelection_BrowsePathAttribute : Attribute
{
    public string FileFilter;

    public CollectionWithSelection_BrowsePathAttribute(string fileFilter = "")
    {
        FileFilter = fileFilter;
    }
}