using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;

namespace Tofu3D;

public class AssetLoader_RuntimeMesh : AssetLoader<Asset_Mesh, RuntimeMesh>
{
    public override RuntimeMesh LoadAsset(AssetLoadParameters<RuntimeMesh>? assetLoadParameters)
    {
        string meshAssetPath = assetLoadParameters.PathToAssetInLibrary;
        Asset_Mesh assetMesh = Serializer.ReadAssetJSON<Asset_Mesh>(meshAssetPath);


        RuntimeMesh runtimeMesh = new RuntimeMesh()
        {
            MeshAssetPath = meshAssetPath,
            VertexBufferDataLength = assetMesh.VertexBufferData.Length,
            VerticesCount = assetMesh.VerticesCount,
            Vao = -1,
            Ebo = -1,
            IndicesCount = assetMesh.Indices.Length
        };

        // if mesh is already loaded, we take its vao!! problem is on model import we unload the runtime meshes so we wont find anything here...
        if (assetLoadParameters.ExistingAsset != null)
        {
            runtimeMesh.Vao = assetLoadParameters.ExistingAsset.Vao;
        }

        BufferFactory.CreateGenericBuffer(ref runtimeMesh.Vao, ref runtimeMesh.Ebo, assetMesh.VertexBufferData,
            assetMesh.CountsOfElements,
            indices: assetMesh.Indices);

        runtimeMesh.InitAssetRuntimeHandle(runtimeMesh.Vao);


        return runtimeMesh;
    }

    public RuntimeMesh LoadAsset(Asset_Mesh assetMesh, AssetLoadParameters<RuntimeMesh>? assetLoadParameters)
    {
        RuntimeMesh runtimeMesh;
        if (assetLoadParameters.ExistingAsset != null)
        {
            runtimeMesh = assetLoadParameters.ExistingAsset;
        }
        else
        {
            runtimeMesh = new RuntimeMesh()
            {
                MeshAssetPath = assetMesh.PathToAssetInLibrary,
                VertexBufferDataLength = assetMesh.VertexBufferData.Length,
                VerticesCount = assetMesh.VerticesCount,
                Vao = -1,
                Ebo = -1,
                IndicesCount = assetMesh.Indices.Length
            };
        }

        // if mesh is already loaded, we take its vao!! problem is on model import we unload the runtime meshes so we wont find anything here...
        if (assetLoadParameters.ExistingAsset != null)
        {
            runtimeMesh.Vao = assetLoadParameters.ExistingAsset.Vao;
            runtimeMesh.Ebo = assetLoadParameters.ExistingAsset.Ebo;
        }

        BufferFactory.CreateGenericBuffer(ref runtimeMesh.Vao, ref runtimeMesh.Ebo, assetMesh.VertexBufferData,
            assetMesh.CountsOfElements,
            indices: assetMesh.Indices);

        runtimeMesh.InitAssetRuntimeHandle(runtimeMesh.Vao);


        return runtimeMesh;
    }
}