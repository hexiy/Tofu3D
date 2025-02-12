namespace Tofu3D;

public class AssetLoader_MeshFile : AssetLoader<MeshFile>
{
    public override MeshFile LoadAsset(AssetLoadParameters<MeshFile>? assetLoadParameters)
    {
        string meshAssetPath = assetLoadParameters.PathToAssetInLibrary;
        MeshFile meshFile = Serializer.ReadAssetJSON<MeshFile>(meshAssetPath);
        meshFile.AssetLoadParameters = assetLoadParameters as AssetLoadParameters_MeshFile;
        return meshFile;
    }
}