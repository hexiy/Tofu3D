using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;

namespace Tofu3D;

// Imports .obj, creates .asset in /Library/ and
public class AssetImporter_Model : AssetImporter<Asset_Model>
{
    private readonly XmlSerializer _xmlSerializer;


    public override Asset_Model ImportAsset(AssetImportParameters<Asset_Model> assetImportParameters)
    {
        AssetImportParameters_Model importParameters = assetImportParameters as AssetImportParameters_Model;

        string objPath = importParameters.PathToSourceAsset;
        // string assetPath = Folders.Library

        var data = File.ReadAllText(objPath).Split("\n");

        List<float> vertices = new();
        List<float> uvs = new();
        List<float> normals = new();


        foreach (var line in data)
        {
            var lineSplit = line.Split(' ');

            if (line.StartsWith("v ")) // positions
            {
                var x = float.Parse(lineSplit[1]);
                var y = float.Parse(lineSplit[2]);
                var z = float.Parse(lineSplit[3]);
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);
            }
            else if (line.StartsWith("vt ")) // UVs
            {
                var x = float.Parse(lineSplit[1]);
                var y = float.Parse(lineSplit[2]);
                uvs.Add(x);
                uvs.Add(y);
            }
            else if (line.StartsWith("vn ")) // normals
            {
                var x = float.Parse(lineSplit[1]);
                var y = float.Parse(lineSplit[2]);
                var z = float.Parse(lineSplit[3]);
                normals.Add(x);
                normals.Add(y);
                normals.Add(z);
            }
        }

        Asset_Model model = new Asset_Model();

        Asset_Mesh assetMesh = new Asset_Mesh();
        int lineStartIndex = 0;
        while (lineStartIndex != -1)
        {
            int indxTemp = lineStartIndex;

            // save meshes too
            assetMesh = LoadMeshFromData(data: data, vertices: vertices, uvs: uvs, normals: normals,
                lineStartIndex: ref lineStartIndex);
            string meshPath = (objPath + $"_mesh_{model.PathsToMeshAssets.Count}")
                .FromRawAssetFileNameToPathOfAssetInLibrary();
            QuickSerializer.SaveFileBinary<Asset_Mesh>(meshPath, assetMesh);
            model.PathsToMeshAssets.Add(meshPath);
            if (indxTemp == lineStartIndex)
            {
                break; // final mesh
            }
        }

        model.PathToRawAsset = objPath;
        string modelPath = objPath.FromRawAssetFileNameToPathOfAssetInLibrary();

        QuickSerializer.SaveFileBinary<Asset_Model>(modelPath, model);
        
        return model;
    }


    private Asset_Mesh LoadMeshFromData(string[] data, List<float> vertices, List<float> uvs, List<float> normals,
        ref int lineStartIndex)
    {
        List<float> everything = new();
        var numberOfIndicesPerLine = 0;
        var totalVerticesCount = 0;

        for (int lineIndex = lineStartIndex; lineIndex < data.Length; lineIndex++)
        {
            var line = data[lineIndex];
            var lineSplit = line.Split(' ');

            if (line.StartsWith("f ")) // indices
            {
                numberOfIndicesPerLine = lineSplit.Length - 1;
                bool isQuad = numberOfIndicesPerLine == 4;
                int[] indicesSequenceForFirstTriangle = new int[] { 0, 1, 2 };
                int[] indicesSequenceForSecondTriangle = new int[] { 0, 2, 3 };

                for (var k = 0; k < 3; k++)
                {
                    int indiceIndex = indicesSequenceForFirstTriangle[k];
                    totalVerticesCount++;

                    var group = lineSplit[indiceIndex + 1].Split('/');
                    for (var i = 0; i < group.Length; i++)
                    {
                        if (group[i].Length == 0)
                        {
                            group[i] = "0";
                        }
                    }

                    var positionIndex = int.Parse(group[0]) - 1;
                    var uvIndex = int.Parse(group[1]) - 1;
                    var normalIndex = int.Parse(group[2]) - 1;

                    everything.Add(vertices[positionIndex * 3 + 0]);
                    everything.Add(vertices[positionIndex * 3 + 1]);
                    everything.Add(vertices[positionIndex * 3 + 2]);

                    if (uvIndex == -1)
                    {
                        everything.Add(0);
                        everything.Add(0);
                    }
                    else
                    {
                        everything.Add(uvs[uvIndex * 2 + 0]);
                        everything.Add(uvs[uvIndex * 2 + 1]);
                    }

                    everything.Add(normals[normalIndex * 3 + 0]);
                    everything.Add(normals[normalIndex * 3 + 1]);
                    everything.Add(normals[normalIndex * 3 + 2]);
                }

                if (isQuad)
                {
                    for (var k = 0; k < 3; k++)
                    {
                        int indiceIndex = indicesSequenceForSecondTriangle[k];
                        totalVerticesCount++;

                        var group = lineSplit[indiceIndex + 1].Split('/');
                        for (var i = 0; i < group.Length; i++)
                        {
                            if (group[i].Length == 0)
                            {
                                group[i] = "0";
                            }
                        }

                        var positionIndex = int.Parse(group[0]) - 1;
                        var uvIndex = int.Parse(group[1]) - 1;
                        var normalIndex = int.Parse(group[2]) - 1;

                        everything.Add(vertices[positionIndex * 3 + 0]);
                        everything.Add(vertices[positionIndex * 3 + 1]);
                        everything.Add(vertices[positionIndex * 3 + 2]);

                        if (uvIndex == -1)
                        {
                            everything.Add(0);
                            everything.Add(0);
                        }
                        else
                        {
                            everything.Add(uvs[uvIndex * 2 + 0]);
                            everything.Add(uvs[uvIndex * 2 + 1]);
                        }

                        everything.Add(normals[normalIndex * 3 + 0]);
                        everything.Add(normals[normalIndex * 3 + 1]);
                        everything.Add(normals[normalIndex * 3 + 2]);
                    }
                }
            }
            else if (line.StartsWith("g") || line.StartsWith("usemtl"))
            {
                // new mesh
                lineStartIndex = lineIndex + 1;
                break;
            }
        }

        int[] countsOfElements = { 3, 2, 3, 3, 3 }; // position, uv, normal, tangent, bitangent

        // now we need to calculate tangents and bitangents per triangle

        int floatsOfPosition = 9; // pos(3x vector3)
        int floatsOfUv = 6; // uv(3x vector2)
        int floatsOfNormal = 9; // norm(3x vector3)


        int floatsPerTriangle =
            floatsOfPosition + floatsOfUv + floatsOfNormal; // (9+6+9) = 24
        int floatsPerVertex = floatsPerTriangle / 3; // 24/3 = 8 to get to another vertex

        List<float> newEverything = new List<float>();

        for (int indexOfVertex1Start = 0;
             indexOfVertex1Start < everything.Count;
             indexOfVertex1Start += floatsPerTriangle)
        {
            Vector3 position1 = new Vector3(
                everything[indexOfVertex1Start + 0],
                everything[indexOfVertex1Start + 1],
                everything[indexOfVertex1Start + 2]);
            Vector3 position2 = new Vector3(
                everything[indexOfVertex1Start + floatsPerVertex + 0],
                everything[indexOfVertex1Start + floatsPerVertex + 1],
                everything[indexOfVertex1Start + floatsPerVertex + 2]);
            Vector3 position3 = new Vector3(
                everything[indexOfVertex1Start + floatsPerVertex + floatsPerVertex + 0],
                everything[indexOfVertex1Start + floatsPerVertex + floatsPerVertex + 1],
                everything[indexOfVertex1Start + floatsPerVertex + floatsPerVertex + 2]);


            int offset = 3; // pos.x,pos.y,pos.z
            Vector2 uv1 = new Vector2(
                everything[indexOfVertex1Start + offset + 0],
                everything[indexOfVertex1Start + offset + 1]);
            Vector2 uv2 = new Vector2(
                everything[indexOfVertex1Start + offset + floatsPerVertex + 0],
                everything[indexOfVertex1Start + offset + floatsPerVertex + 1]);
            Vector2 uv3 = new Vector2(
                everything[indexOfVertex1Start + offset + floatsPerVertex + floatsPerVertex + 0],
                everything[indexOfVertex1Start + offset + floatsPerVertex + floatsPerVertex + 1]);


            offset = 5; // pos.x,pos.y,pos.z, uv.x,uv.y

            // average out normals

            Vector3 nm1 = new Vector3(
                everything[indexOfVertex1Start + offset + 0],
                everything[indexOfVertex1Start + offset + 1],
                everything[indexOfVertex1Start + offset + 2]);
            Vector3 nm2 = new Vector3(
                everything[indexOfVertex1Start + offset + floatsPerVertex + 0],
                everything[indexOfVertex1Start + offset + floatsPerVertex + 1],
                everything[indexOfVertex1Start + offset + floatsPerVertex + 2]);
            Vector3 nm3 = new Vector3(
                everything[indexOfVertex1Start + offset + floatsPerVertex + floatsPerVertex + 0],
                everything[indexOfVertex1Start + offset + floatsPerVertex + floatsPerVertex + 1],
                everything[indexOfVertex1Start + offset + floatsPerVertex + floatsPerVertex + 2]);

            Vector3 nm = (nm1 + nm2 + nm3) / 3f;
            // Vector3 nm = nm1;
            // Vector3 nm = new Vector3(
            //     everything[indexOfVertex1Start + offset + 0],
            //     everything[indexOfVertex1Start + offset + 1],
            //     everything[indexOfVertex1Start + offset + 2]);

            Vector3 tangent1;
            Vector3 bitangent1;
            Vector3 tangent2;
            Vector3 bitangent2;

            Vector3 edge1 = position2 - position1;
            Vector3 edge2 = position3 - position1;
            Vector2 deltaUV1 = uv2 - uv1;
            Vector2 deltaUV2 = uv3 - uv1;

            float f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);


            tangent1.X = f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X);
            tangent1.Y = f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y);
            tangent1.Z = f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z);

            bitangent1.X = f * (-deltaUV2.X * edge1.X + deltaUV1.X * edge2.X);
            bitangent1.Y = f * (-deltaUV2.X * edge1.Y + deltaUV1.X * edge2.Y);
            bitangent1.Z = f * (-deltaUV2.X * edge1.Z + deltaUV1.X * edge2.Z);

            // triangle 2
            // ----------
            edge1 = position2 - position1;
            edge2 = position3 - position1;
            deltaUV1 = uv2 - uv1;
            deltaUV2 = uv3 - uv1;

            f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

            tangent2.X = f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X);
            tangent2.Y = f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y);
            tangent2.Z = f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z);


            bitangent2.X = f * (-deltaUV2.X * edge1.X + deltaUV1.X * edge2.X);
            bitangent2.Y = f * (-deltaUV2.X * edge1.Y + deltaUV1.X * edge2.Y);
            bitangent2.Z = f * (-deltaUV2.X * edge1.Z + deltaUV1.X * edge2.Z);


            float[] triangleVertices =
            {
                // positions                           // uvs        // normals        // tangent                          // bitangent
                position1.X, position1.Y, position1.Z, uv1.X, uv1.Y, nm.X, nm.Y, nm.Z, tangent1.X, tangent1.Y,
                tangent1.Z, bitangent1.X, bitangent1.Y, bitangent1.Z,
                position2.X, position2.Y, position2.Z, uv2.X, uv2.Y, nm.X, nm.Y, nm.Z, tangent1.X, tangent1.Y,
                tangent1.Z, bitangent1.X, bitangent1.Y, bitangent1.Z,
                position3.X, position3.Y, position3.Z, uv3.X, uv3.Y, nm.X, nm.Y, nm.Z, tangent1.X, tangent1.Y,
                tangent1.Z, bitangent1.X, bitangent1.Y, bitangent1.Z,
            };
            newEverything.AddRange(triangleVertices);
        }

        Asset_Mesh mesh = new Asset_Mesh();
        mesh.CountsOfElements = countsOfElements;
        mesh.VertexBufferData = newEverything.ToArray();

        // BufferFactory.CreateGenericBuffer(ref runtimeMesh.Vao, newEverything.ToArray(), countsOfElements);
        //
        // runtimeMesh.InitAssetRuntimeHandle(runtimeMesh.Vao);

        return mesh;
    }
}