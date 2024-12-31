using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;

namespace Tofu3D;

public class AssetLoader_AssetMesh : AssetLoader<Asset_Mesh, Asset_Mesh>
{
    public override Asset_Mesh LoadAsset(AssetLoadParameters<Asset_Mesh>? assetLoadParameters)
    {
        string meshAssetPath = assetLoadParameters.PathToAsset;
        Asset_Mesh assetMesh = Serializer.ReadAssetJSON<Asset_Mesh>(meshAssetPath);


        // RuntimeMesh runtimeMesh = new RuntimeMesh()
        // {
        //     MeshAssetPath = meshAssetPath,
        //     VertexBufferDataLength = assetMesh.VertexBufferData.Length,
        //     VerticesCount = assetMesh.VerticesCount,
        //     Vao = -1,
        //     IndicesCount = assetMesh.Indices.Length
        // };
        //
        // // if mesh is already loaded, we take its vao!! problem is on model import we unload the runtime meshes so we wont find anything here...
        // if (assetLoadParameters.ExistingAsset != null)
        // {
        //     runtimeMesh.Vao = assetLoadParameters.ExistingAsset.Vao;
        // }
        //
        // BufferFactory.CreateGenericBuffer(ref runtimeMesh.Vao, ref runtimeMesh.Ebo, assetMesh.VertexBufferData,
        //     assetMesh.CountsOfElements,
        //     indices: assetMesh.Indices);
        //
        // runtimeMesh.InitAssetRuntimeHandle(runtimeMesh.Vao);
        //

        return assetMesh;
    }
}