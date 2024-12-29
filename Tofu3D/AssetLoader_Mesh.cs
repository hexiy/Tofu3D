using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;

namespace Tofu3D;

public class AssetLoader_Mesh : AssetLoader<Asset_Mesh, RuntimeMesh>
{
    public override RuntimeMesh LoadAsset(AssetLoadParameters<RuntimeMesh>? assetLoadParameters)
    {
        string meshAssetPath = assetLoadParameters.PathToAsset;
        Asset_Mesh assetMesh = Serializer.ReadAssetJSON<Asset_Mesh>(meshAssetPath);


        RuntimeMesh runtimeMesh = new RuntimeMesh()
        {
            MeshAssetPath = meshAssetPath,
            VertexBufferDataLength = assetMesh.VertexBufferData.Length,
            VerticesCount = assetMesh.VerticesCount,
            Vao = -1,
            Indices = assetMesh.GetIndices()
        };

        // if mesh is already loaded, we take its vao!! problem is on model import we unload the runtime meshes so we wont find anything here...
        RuntimeMesh alreadyLoadedMesh = Tofu.AssetLoadManager.GetLoadedAsset<RuntimeMesh>(meshAssetPath);
        if (alreadyLoadedMesh != null)
        {
            runtimeMesh.Vao = alreadyLoadedMesh.Vao;
        }

        BufferFactory.CreateGenericBuffer(ref runtimeMesh.Vao, ref runtimeMesh.Ebo, assetMesh.VertexBufferData, assetMesh.CountsOfElements,
            indices: assetMesh.GetIndices());

        runtimeMesh.InitAssetRuntimeHandle(runtimeMesh.Vao);
        

        return runtimeMesh;
    }
}