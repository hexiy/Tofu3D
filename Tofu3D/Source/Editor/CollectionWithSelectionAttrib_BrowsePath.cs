namespace Tofu3D;

[CollectionWithSelectionAttrib_BrowsePath("")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public sealed class CollectionWithSelectionAttrib_BrowsePath : Attribute
{
    public string FileFilter;

    public CollectionWithSelectionAttrib_BrowsePath(string fileFilter = "")
    {
        FileFilter = fileFilter;
    }
}