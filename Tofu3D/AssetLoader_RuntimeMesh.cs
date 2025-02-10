namespace Tofu3D;

public class AssetLoader_RuntimeMesh : AssetLoader<RuntimeMesh>
{
    public override RuntimeMesh LoadAsset(AssetLoadParameters<RuntimeMesh>? assetLoadParameters)
    {
        string meshAssetPath = assetLoadParameters.PathToAssetInLibrary;

        
        Tofu.AssetFileCache.GetAsset<MeshFile>(meshAssetPath, out MeshFile meshFile);

        RuntimeMesh runtimeMesh = LoadAsset(meshFile, assetLoadParameters);

        return runtimeMesh;
    }

    public RuntimeMesh LoadAsset(MeshFile meshFile, AssetLoadParameters<RuntimeMesh>? assetLoadParameters)
    {
        Mesh mesh = meshFile.Mesh;
        RuntimeMesh runtimeMesh;
        if (assetLoadParameters.ExistingAsset != null)
        {
            runtimeMesh = assetLoadParameters.ExistingAsset;
        }
        else
        {
            runtimeMesh = new RuntimeMesh()
            {
                Mesh = mesh,
                Vao = -1,
                Ebo = -1,
            };
        }

        // if mesh is already loaded, we take its vao!! problem is on model import we unload the runtime meshes so we wont find anything here...
        if (assetLoadParameters.ExistingAsset != null)
        {
            runtimeMesh.Vao = assetLoadParameters.ExistingAsset.Vao;
            runtimeMesh.Ebo = assetLoadParameters.ExistingAsset.Ebo;
        }

        BufferFactory.CreateGeometryBuffer(ref runtimeMesh.Vao, ref runtimeMesh.Ebo, mesh.GeometryBufferData,
            mesh.CountsOfElements, indices: mesh.Indices);

        return runtimeMesh;
    }
}