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

        Asset_Mesh assetMesh = QuickSerializer.ReadFileBinary<Asset_Mesh>(meshAssetPath);

        RuntimeMesh runtimeMesh = new RuntimeMesh()
        {
            MeshAssetPath = meshAssetPath,
            VertexBufferDataLength = assetMesh.VertexBufferData.Length,
            VerticesCount = assetMesh.VerticesCount
        };
    
        BufferFactory.CreateGenericBuffer(ref runtimeMesh.Vao, assetMesh.VertexBufferData, assetMesh.CountsOfElements);
        
        runtimeMesh.InitAssetRuntimeHandle(runtimeMesh.Vao);
        
        return runtimeMesh;
    }
}