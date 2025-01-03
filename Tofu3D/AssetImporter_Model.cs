using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Tofu3D;

// Imports .obj, creates .asset in /Library/ and
public class AssetImporter_Model : AssetImporter<Asset_Model>
{
    public override Asset_Model ImportAsset(AssetImportParameters<Asset_Model> assetImportParameters)
    {
        AssetImportParameters_Model importParameters = assetImportParameters as AssetImportParameters_Model;

        string objInAssetsFolderPath = importParameters.PathToSourceAsset;

        // read .mtl file
        ObjMaterialFileDefinition objMaterialFileDefinition = null;

        if (File.Exists(objInAssetsFolderPath) == false)
        {
            Debug.LogError($"importing obj failed, file doesnt exist:{objInAssetsFolderPath}");
        }

        var data = File.ReadAllText(objInAssetsFolderPath).Split("\n");

        List<float> vertices = new();
        List<float> uvs = new();
        List<float> normals = new();

        foreach (var l in data)
        {
            string line = l.Trim();
            var lineSplits = line.Split(' ').ToList();
            lineSplits.Remove("");

            if (line.Contains("mtllib"))
            {
                string materialPath =
                    TofuPath.Combine(Folders.GetParentFolder(objInAssetsFolderPath), lineSplits[1]);
                LoadObjMaterial(materialPath, out objMaterialFileDefinition);
            }

            if (line.StartsWith("v ")) // positions
            {
                var x = float.Parse(lineSplits[1]);
                var y = float.Parse(lineSplits[2]);
                var z = float.Parse(lineSplits[3]);
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);
            }
            else if (line.StartsWith("vt ")) // UVs
            {
                var x = float.Parse(lineSplits[1]);
                var y = float.Parse(lineSplits[2]);
                uvs.Add(x);
                uvs.Add(y);
            }
            else if (line.StartsWith("vn ")) // normals
            {
                var x = float.Parse(lineSplits[1]);
                var y = float.Parse(lineSplits[2]);
                var z = float.Parse(lineSplits[3]);
                normals.Add(x);
                normals.Add(y);
                normals.Add(z);
            }
        }

        Asset_Model model = new Asset_Model();

        MeshFile meshFile = null; //= new MeshFile() { UsesIndices = RenderingSettings.USE_INDICES};
        int lineStartIndex = 0;
        while (lineStartIndex != -1)
        {
            int indxTemp = lineStartIndex;

            // save meshes too
            meshFile = CreateMeshFileFromData(data: data, vertices: vertices, uvs: uvs, normals: normals,
                lineStartIndex: ref lineStartIndex, singleMesh: importParameters.ImportAsSingleMesh,
                smoothNormals: importParameters.SmoothNormals, objMaterialFileDefinition);
            int meshIndex = model.PathsToMeshAssets.Count;

            string meshFileName = AssetPathExtensions.ModelToMeshFileName(objInAssetsFolderPath, meshIndex);
            string meshPath = AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(meshFileName);

            meshFile.Mesh.Name = Path.GetFileNameWithoutExtension(meshPath);
            meshFile.Mesh.PathInLibraryFolder = meshPath;

            Serializer.SaveAssetJSON<MeshFile>(meshPath, meshFile);

            model.PathsToMeshAssets.Add(meshPath);
            if (indxTemp == lineStartIndex)
            {
                break; // final mesh
            }
        }

        model.PathInAssetsFolder = objInAssetsFolderPath;
        string modelPath = AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(objInAssetsFolderPath);
        model.PathInLibraryFolder = objInAssetsFolderPath;

        Serializer.SaveAssetJSON<Asset_Model>(modelPath, model);

        return model;
    }

    private void LoadObjMaterial(string objMaterialPath, out ObjMaterialFileDefinition objMaterialFileDefinition)
    {
        if (File.Exists(objMaterialPath) == false)
        {
            objMaterialFileDefinition = null;
            return;
        }

        objMaterialFileDefinition = new ObjMaterialFileDefinition();
        string objMaterialDirectory = Folders.GetParentFolder(objMaterialPath);

        string materialFileText = File.ReadAllText(objMaterialPath);
        materialFileText =
            materialFileText.Replace('\\', Path.DirectorySeparatorChar); // change \ to directory separator(/ or \)
        string[] materialFileLines = materialFileText.Split("\n");
        ObjMaterialDefinition? currentObjMaterialDefinition = null;

        foreach (string l in materialFileLines)
        {
            string line = l.Trim();
            var lineSplits = line.Split(' ').ToList();


            // at the beginning of new material definition
            if (line.Contains("newmtl"))
            {
                if (currentObjMaterialDefinition != null)
                {
                    objMaterialFileDefinition.Materials.Add(currentObjMaterialDefinition);
                }

                currentObjMaterialDefinition = new ObjMaterialDefinition();
                currentObjMaterialDefinition.MaterialName = lineSplits[1];
            }

            if (lineSplits[0].Equals("Kd", StringComparison.OrdinalIgnoreCase)) // diffuse/albedo color
            {
                float r = float.Parse(lineSplits[1]);
                float g = float.Parse(lineSplits[2]);
                float b = float.Parse(lineSplits[3]);
                Color albedoColor = new Color(r, g, b, 1);
                currentObjMaterialDefinition.AlbedoTint = albedoColor;
            }

            if (lineSplits[0].Equals("map_Kd", StringComparison.OrdinalIgnoreCase)) // diffuse/albedo texture
            {
                string albedoTextureName = TofuPath.Combine(objMaterialDirectory, lineSplits[1]);
                currentObjMaterialDefinition.AlbedoTexturePath = albedoTextureName;
                Tofu.AssetImportManager
                    .ImportAsset(
                        albedoTextureName); // we need to import it because this is called on model import, so textures are not guaranteed to be imported yet
            }

            if (lineSplits[0].Equals("map_bump", StringComparison.OrdinalIgnoreCase) ||
                lineSplits[0].Equals("bump", StringComparison.OrdinalIgnoreCase)) // diffuse/albedo texture
            {
                string normalTextureName = TofuPath.Combine(objMaterialDirectory, lineSplits[1]);
                currentObjMaterialDefinition.PathInAssetsFolder = normalTextureName;
                Tofu.AssetImportManager
                    .ImportAsset(
                        normalTextureName); // we need to import it because this is called on model import, so textures are not guaranteed to be imported yet
            }
        }
    }


    private MeshFile CreateMeshFileFromData(string[] data, List<float> vertices, List<float> uvs, List<float> normals,
        ref int lineStartIndex, bool singleMesh = false, bool smoothNormals = true,
        ObjMaterialFileDefinition objMaterialFileDefinition = null)
    {
        List<uint> indices = new();
        ObjMaterialDefinition? objMaterialDefinition = objMaterialFileDefinition?.Materials.LastOrDefault() ?? null;
        Dictionary<Vector3, uint> uniqueVertices = new Dictionary<Vector3, uint>();
        uint currentUniqueVertexIndex = 0;
        List<float> everything = new();
        var numberOfIndicesPerLine = 0;
        var totalVerticesCount = 0;

        int lineIndexRelativeForThisMesh = -1;
        for (int lineIndex = lineStartIndex; lineIndex < data.Length; lineIndex++)
        {
            lineIndexRelativeForThisMesh++;

            var line = data[lineIndex].Trim();
            line = line.Replace("\r", "");

            var lineSplits = line.Split(' ').ToList();
            if (line.StartsWith("f ")) // indices
            {
                numberOfIndicesPerLine = lineSplits.Count - 1;
                bool isQuad = numberOfIndicesPerLine == 4;
                int[] indicesSequenceForFirstTriangle = new int[] { 0, 1, 2 };
                int[] indicesSequenceForSecondTriangle = new int[] { 0, 2, 3 };

                for (var k = 0; k < 3; k++)
                {
                    int indiceIndex = indicesSequenceForFirstTriangle[k];
                    totalVerticesCount++;

                    var group = lineSplits[indiceIndex + 1].Split('/');
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

                        var group = lineSplits[indiceIndex + 1].Split('/');
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
            // else if ((line.StartsWith("g") || line.StartsWith("usemtl")) && singleMesh == false ||
            else if (((line.StartsWith("g") || line.StartsWith("o ")) && singleMesh == false) ||
                     (line.StartsWith("# object") && lineStartIndex != 0))
            {
                // new mesh
                lineStartIndex = lineIndex + 1;
                break;
            }

            if (line.StartsWith("usemtl") && objMaterialFileDefinition != null)
            {
                objMaterialDefinition =
                    objMaterialFileDefinition.Materials.FirstOrDefault(d => d.MaterialName == lineSplits[1]) ?? null;
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

        if (smoothNormals)
        {
            SmoothNormals(everything, floatsPerTriangle, floatsPerVertex);
        }

        List<float> vertexBufferData = new List<float>();

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


            float[] vertex1 =
            {
                position1.X, position1.Y, position1.Z, uv1.X, uv1.Y, nm1.X, nm1.Y, nm1.Z, tangent1.X, tangent1.Y,
                tangent1.Z, bitangent1.X, bitangent1.Y, bitangent1.Z
            };
            float[] vertex2 =
            {
                position2.X, position2.Y, position2.Z, uv2.X, uv2.Y, nm2.X, nm2.Y, nm2.Z, tangent1.X, tangent1.Y,
                tangent1.Z, bitangent1.X, bitangent1.Y, bitangent1.Z,
            };
            float[] vertex3 =
            {
                position3.X, position3.Y, position3.Z, uv3.X, uv3.Y, nm3.X, nm3.Y, nm3.Z, tangent1.X, tangent1.Y,
                tangent1.Z, bitangent1.X, bitangent1.Y, bitangent1.Z,
            };
            // float[] triangleVertices =
            // {
            //     // positions                           // uvs        // normals        // tangent                          // bitangent
            //     position1.X, position1.Y, position1.Z, uv1.X, uv1.Y, nm1.X, nm1.Y, nm1.Z, tangent1.X, tangent1.Y,
            //     tangent1.Z, bitangent1.X, bitangent1.Y, bitangent1.Z,
            //     position2.X, position2.Y, position2.Z, uv2.X, uv2.Y, nm2.X, nm2.Y, nm2.Z, tangent1.X, tangent1.Y,
            //     tangent1.Z, bitangent1.X, bitangent1.Y, bitangent1.Z,
            //     position3.X, position3.Y, position3.Z, uv3.X, uv3.Y, nm3.X, nm3.Y, nm3.Z, tangent1.X, tangent1.Y,
            //     tangent1.Z, bitangent1.X, bitangent1.Y, bitangent1.Z,
            // };
            if (RenderingSettings.USE_INDICES == false)
            {
                vertexBufferData.AddRange(vertex1);
                vertexBufferData.AddRange(vertex2);
                vertexBufferData.AddRange(vertex3);
            }
            else
            {
                if (uniqueVertices.ContainsKey(position1))
                {
                    uint index = uniqueVertices[position1];
                    indices.Add(index);
                }
                else
                {
                    vertexBufferData.AddRange(vertex1);
                    uint index = (uint)currentUniqueVertexIndex;
                    indices.Add(index);
                    currentUniqueVertexIndex++;
                    uniqueVertices.Add(position1, index);
                }

                if (uniqueVertices.ContainsKey(position2))
                {
                    uint index = uniqueVertices[position2];
                    indices.Add(index);
                }
                else
                {
                    vertexBufferData.AddRange(vertex2);
                    uint index = (uint)currentUniqueVertexIndex;
                    indices.Add(index);
                    currentUniqueVertexIndex++;
                    uniqueVertices.Add(position2, index);
                }

                if (uniqueVertices.ContainsKey(position3))
                {
                    uint index = uniqueVertices[position3];
                    indices.Add(index);
                }
                else
                {
                    vertexBufferData.AddRange(vertex3);
                    uint index = (uint)currentUniqueVertexIndex;
                    indices.Add(index);
                    currentUniqueVertexIndex++;
                    uniqueVertices.Add(position3, index);
                }
            }
        }

        // unique vertex list
        // once we have unique vertices in an array, thats our new vertex data, and indices we just find indexes of
        //     them there
        //     because right now we have all vertices in the array wasting time and its wrong too.
        // so our indice will be pointing to [vertex1, vertex2, vertex3]

        Mesh mesh = new Mesh();
        mesh.CountsOfElements = countsOfElements;
        mesh.VertexBufferData = vertexBufferData.ToArray();
        mesh.VerticesCount = (int)(vertexBufferData.Count / 14);
        mesh.Indices = indices.ToArray();

        if (objMaterialDefinition != null)
        {
            if (objMaterialDefinition.GeneratedMaterialFilePath == null)
            {
                objMaterialDefinition.GeneratedMaterialFilePath =
                    CreateMaterialFromObjMaterialDefinition(objMaterialDefinition);
            }

            mesh.PathToObjMaterial = objMaterialDefinition.GeneratedMaterialFilePath;
        }

        // mesh.paths are set in ImportAsset


        MeshFile meshFile = new MeshFile()
        {
            Mesh = mesh,
            UsesIndices = RenderingSettings.USE_INDICES,
        };
        return meshFile;
    }

    private static string? CreateMaterialFromObjMaterialDefinition(ObjMaterialDefinition? materialDefinition)
    {
        if (materialDefinition == null)
        {
            return null;
        }

        Asset_Material material = new Asset_Material()
            { Shader = new Shader(TofuPath.Combine(Folders.ShadersInAssets, "ModelRendererInstanced.glsl")) };

        material.SmoothShadows = true;

        material.AlbedoTint = materialDefinition.AlbedoTint;

        if (materialDefinition.AlbedoTexturePath != null)
        {
            string pathOfAlbedoTexture =
                AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(materialDefinition
                    .AlbedoTexturePath);
            RuntimeTexture texture =
                Tofu.AssetLoadManager.Load<RuntimeTexture>(pathOfAlbedoTexture);
            material.AlbedoTexture = texture;
        }

        if (materialDefinition.PathInAssetsFolder != null)
        {
            string pathOfNormalTexture =
                AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(materialDefinition
                    .PathInAssetsFolder);
            RuntimeTexture texture =
                Tofu.AssetLoadManager.Load<RuntimeTexture>(pathOfNormalTexture);
            material.NormalTexture = texture;
        }

        string materialPath =
            Folders.GetPathRelativeToProjectFolder(TofuPath.Combine(Folders.MaterialsInLibrary,
                materialDefinition.MaterialName + ".tofumaterial"));

        material.PathInLibraryFolder = materialPath;
        Tofu.AssetLoadManager.Save<Asset_Material>(materialPath, material);

        return materialPath;
    }

    private static void SmoothNormals(List<float> everything, int floatsPerTriangle, int floatsPerVertex)
    {
        // smooth normals
        ConcurrentDictionary<Vector3, Vector3> smoothedNormals = new ConcurrentDictionary<Vector3, Vector3>();
        // Dictionary<vertexPosition, accumulatedNormals>
        // and at the end we just find those vertex positions again, and assign them new normal, the accumulatedNormal but normalized

        ParallelOptions opt = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
        Parallel.For(0, (int)MathF.Floor((float)everything.Count / (float)floatsPerTriangle), parallelOptions: opt,
            i =>
            {
                int triangle1StartIndex = i * floatsPerTriangle;
                Vector3 t1position1 = new Vector3(
                    everything[triangle1StartIndex + 0],
                    everything[triangle1StartIndex + 1],
                    everything[triangle1StartIndex + 2]);
                Vector3 t1position2 = new Vector3(
                    everything[triangle1StartIndex + floatsPerVertex + 0],
                    everything[triangle1StartIndex + floatsPerVertex + 1],
                    everything[triangle1StartIndex + floatsPerVertex + 2]);
                Vector3 t1position3 = new Vector3(
                    everything[triangle1StartIndex + floatsPerVertex + floatsPerVertex + 0],
                    everything[triangle1StartIndex + floatsPerVertex + floatsPerVertex + 1],
                    everything[triangle1StartIndex + floatsPerVertex + floatsPerVertex + 2]);

                int offset = 5; // pos.x,pos.y,pos.z, uv.x,uv.y
                Vector3 t1nm1 = new Vector3(
                    everything[triangle1StartIndex + offset + 0],
                    everything[triangle1StartIndex + offset + 1],
                    everything[triangle1StartIndex + offset + 2]);
                Vector3 t1nm2 = new Vector3(
                    everything[triangle1StartIndex + offset + floatsPerVertex + 0],
                    everything[triangle1StartIndex + offset + floatsPerVertex + 1],
                    everything[triangle1StartIndex + offset + floatsPerVertex + 2]);
                Vector3 t1nm3 = new Vector3(
                    everything[triangle1StartIndex + offset + floatsPerVertex + floatsPerVertex + 0],
                    everything[triangle1StartIndex + offset + floatsPerVertex + floatsPerVertex + 1],
                    everything[triangle1StartIndex + offset + floatsPerVertex + floatsPerVertex + 2]);

                if (smoothedNormals.ContainsKey(t1position1) == false)
                {
                    smoothedNormals.TryAdd(t1position1, t1nm1);
                }
                else
                {
                    smoothedNormals[t1position1] = smoothedNormals[t1position1] + t1nm1;
                }

                if (smoothedNormals.ContainsKey(t1position2) == false)
                {
                    smoothedNormals.TryAdd(t1position2, t1nm2);
                }
                else
                {
                    smoothedNormals[t1position2] = smoothedNormals[t1position2] + t1nm2;
                }

                if (smoothedNormals.ContainsKey(t1position3) == false)
                {
                    smoothedNormals.TryAdd(t1position3, t1nm3);
                }
                else
                {
                    smoothedNormals[t1position3] = smoothedNormals[t1position3] + t1nm3;
                }
            });
        foreach (KeyValuePair<Vector3, Vector3> pair in smoothedNormals)
        {
            smoothedNormals[pair.Key] = pair.Value.Normalized();
        }

        Parallel.For(0, (int)MathF.Floor((float)everything.Count / (float)floatsPerTriangle), parallelOptions: opt,
            i =>
            {
                int triangle1StartIndex = i * floatsPerTriangle;
                Vector3 t1position1 = new Vector3(
                    everything[triangle1StartIndex + 0],
                    everything[triangle1StartIndex + 1],
                    everything[triangle1StartIndex + 2]);
                Vector3 t1position2 = new Vector3(
                    everything[triangle1StartIndex + floatsPerVertex + 0],
                    everything[triangle1StartIndex + floatsPerVertex + 1],
                    everything[triangle1StartIndex + floatsPerVertex + 2]);
                Vector3 t1position3 = new Vector3(
                    everything[triangle1StartIndex + floatsPerVertex + floatsPerVertex + 0],
                    everything[triangle1StartIndex + floatsPerVertex + floatsPerVertex + 1],
                    everything[triangle1StartIndex + floatsPerVertex + floatsPerVertex + 2]);

                int offset = 5; // pos.x,pos.y,pos.z, uv.x,uv.y


                if (smoothedNormals.TryGetValue(t1position1, out var normal1))
                {
                    everything[triangle1StartIndex + offset + 0] = normal1.X;
                    everything[triangle1StartIndex + offset + 1] = normal1.Y;
                    everything[triangle1StartIndex + offset + 2] = normal1.Z;
                }

                if (smoothedNormals.TryGetValue(t1position2, out var normal2))
                {
                    everything[triangle1StartIndex + offset + floatsPerVertex + 0] = normal2.X;
                    everything[triangle1StartIndex + offset + floatsPerVertex + 1] = normal2.Y;
                    everything[triangle1StartIndex + offset + floatsPerVertex + 2] = normal2.Z;
                }

                if (smoothedNormals.TryGetValue(t1position3, out var normal3))
                {
                    everything[triangle1StartIndex + offset + floatsPerVertex + floatsPerVertex + 0] = normal3.X;
                    everything[triangle1StartIndex + offset + floatsPerVertex + floatsPerVertex + 1] = normal3.Y;
                    everything[triangle1StartIndex + offset + floatsPerVertex + floatsPerVertex + 2] = normal3.Z;
                }
            });
    }
}