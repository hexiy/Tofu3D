namespace TofuEngine;

public class AssetSupportedFileNameExtensions
{
    public AssetSupportedFileNameExtensions(params string[] extensions)
    {
        Extensions = extensions;
    }

    public string[] Extensions { get; init; }
}