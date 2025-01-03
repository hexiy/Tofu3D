namespace Tofu3D;

public class ObjMaterialDefinition
{
    public string MaterialName;
    public string? AlbedoTexturePath;
    public string? PathInAssetsFolder;
    public Color AlbedoTint = Color.White;
    public string? GeneratedMaterialFilePath = null;
}