namespace Tofu3D;

public class ObjMaterialDefinition
{
    public string MaterialName;
    public string? AlbedoTexturePath;
    public string? AlphaMaskTexturePath;
    public string? NormalTexturePath;
    
    
    public string? PathInAssetsFolder;
    public Color AlbedoColor = new Color(1,1,1,1);
    public string? GeneratedMaterialFilePath = null;
}