namespace Tofu3D;

public class AssetLoader_MeshFile : AssetLoader<MeshFile>
{
    public override MeshFile LoadAsset(AssetLoadParameters<MeshFile>? assetLoadParameters)
    {
        string meshAssetPath = assetLoadParameters.PathToAssetInLibrary;
        MeshFile meshFile = Serializer.ReadAssetJSON<MeshFile>(meshAssetPath);

        return meshFile;
    }
}