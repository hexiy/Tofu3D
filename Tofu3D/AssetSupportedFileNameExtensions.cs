namespace Tofu3D;

public class AssetSupportedFileNameExtensions
{
    public string[] Extensions { get; init; }

    public AssetSupportedFileNameExtensions(params string[] extensions)
    {
        Extensions = extensions;
    }
}