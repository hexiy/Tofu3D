using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TofuEngine;

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

        // string[] data = File.ReadAllText(objInAssetsFolderPath).Split("\n");
        string[] data = File.ReadAllLines(objInAssetsFolderPath);

        List<float> positions = new List<float>();
        List<float> uvs = new List<float>();
        List<float> normals = new List<float>();

        foreach (string l in data)
        {
            string line = l.Trim();
            List<string> lineSplits = line.Split(' ').ToList();
            lineSplits.Remove("");

            if (line.Contains("mtllib"))
            {
                string materialPath =
                    TofuPath.Combine(Folders.GetParentFolder(objInAssetsFolderPath), lineSplits[1]);
                LoadObjMaterial(materialPath, out objMaterialFileDefinition);
            }

            if (line.StartsWith("v ")) // positions
            {
                float x = float.Parse(lineSplits[1]);
                float y = float.Parse(lineSplits[2]);
                float z = float.Parse(lineSplits[3]);
                if (lineSplits[1].Length == 0 || lineSplits[1] == " ")
                {
                    int a = 0;
                }

                positions.Add(x);
                positions.Add(y);
                positions.Add(z);
            }
            else if (line.StartsWith("vt ")) // UVs
            {
                float x = float.Parse(lineSplits[1]);
                float y = float.Parse(lineSplits[2]);
                uvs.Add(x);
                uvs.Add(y);
            }
            else if (line.StartsWith("vn ")) // normals
            {
                float x = float.Parse(lineSplits[1]);
                float y = float.Parse(lineSplits[2]);
                float z = float.Parse(lineSplits[3]);
                normals.Add(x);
                normals.Add(y);
                normals.Add(z);
            }
        }

        Asset_Model model = new Asset_Model();

        MeshFile meshFile; //= new MeshFile() { UsesIndices = RenderingSettings.USE_INDICES};

        int meshIndex = 0;
        int lineStartIndex = 0;
        while (true)
        {
            int lineStartIndexBefore = lineStartIndex;
            meshFile = CreateMeshFileFromObj(
                modelFilePath: objInAssetsFolderPath,
                meshIndex: meshIndex,
                data: data,
                positions: positions,
                uvs: uvs,
                normals: normals,
                lineStartIndex: ref lineStartIndex,
                singleMesh: importParameters.ImportAsSingleMesh,
                smoothNormals: importParameters.SmoothNormals,
                objMaterialFileDefinition);

            if (meshFile != null)
            {
                model.PathsToMeshAssets.Add(meshFile.PathInLibraryFolder);
                meshIndex++;
            }

            if (meshFile == null && lineStartIndexBefore != 0)
            {
                break;
            }

            if (lineStartIndex >= data.Length - 1)
            {
                importParameters.ImportAsSingleMesh = true;
                break;
            }

            if (lineStartIndex == lineStartIndexBefore)
            {
                lineStartIndex++;
                // break;
            }

            if (importParameters.ImportAsSingleMesh)
            {
                break;
            }
        }

        model.PathInAssetsFolder = objInAssetsFolderPath;
        string modelPath = AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(objInAssetsFolderPath);
        model.PathInLibraryFolder = objInAssetsFolderPath;
        model.AssetImportParameters = importParameters;

        Serializer.SaveAssetJSON<Asset_Model>(modelPath, model);
        // Debug.Log($"Imported model {Path.GetFileName(modelPath)} with {model.PathsToMeshAssets.Count} meshes");
        return model;
    }

    private Dictionary<string, ObjMaterialFileDefinition> _objMaterialFiles =
        new Dictionary<string, ObjMaterialFileDefinition>();

    private void LoadObjMaterial(string objMaterialPath, out ObjMaterialFileDefinition objMaterialFileDefinition)
    {
        if (File.Exists(objMaterialPath) == false)
        {
            objMaterialFileDefinition = null;
            return;
        }

        if (_objMaterialFiles.TryGetValue(objMaterialPath, out ObjMaterialFileDefinition? materialFile))
        {
            objMaterialFileDefinition = materialFile;
            return;
        }

        objMaterialFileDefinition = new ObjMaterialFileDefinition();
        string objMaterialDirectory = Folders.GetParentFolder(objMaterialPath);

        string materialFileText = File.ReadAllText(objMaterialPath);
        materialFileText =
            materialFileText.Replace('\\', Path.DirectorySeparatorChar); // change \ to directory separator(/ or \)
        materialFileText = materialFileText.Replace("# d", "d");
        materialFileText = materialFileText.Replace("# Tr", "Tr");
        string[] materialFileLines = materialFileText.Split("\n");
        ObjMaterialDefinition? currentObjMaterialDefinition = null;

        foreach (string l in materialFileLines)
        {
            string line = l.Trim();
            List<string> lineSplits = line.Split(' ').ToList();


            // at the beginning of new material definition
            if (line.Contains("newmtl"))
            {
                if (currentObjMaterialDefinition != null)
                {
                    objMaterialFileDefinition.Materials.Add(currentObjMaterialDefinition);
                }

                currentObjMaterialDefinition = new ObjMaterialDefinition
                {
                    MaterialName = lineSplits[1]
                };
            }

            if (lineSplits[0].Equals("Kd", StringComparison.OrdinalIgnoreCase)) // diffuse/albedo color
            {
                float r = float.Parse(lineSplits[1]);
                float g = float.Parse(lineSplits[2]);
                float b = float.Parse(lineSplits[3]);
                currentObjMaterialDefinition.AlbedoColor.GetColorWithRGB(r, g, b);
            }

            if (lineSplits[0].Equals("d", StringComparison.OrdinalIgnoreCase)) // opacity
            {
                float a = 1 - float.Parse(lineSplits[1]);
                currentObjMaterialDefinition.AlbedoColor.SetAlpha(a);
            }

            if (lineSplits[0].Equals("Tr", StringComparison.OrdinalIgnoreCase)) // transparency
            {
                float a = float.Parse(lineSplits[1]);
                currentObjMaterialDefinition.AlbedoColor.SetAlpha(a);
            }

            if (lineSplits[0].Equals("map_Kd", StringComparison.OrdinalIgnoreCase) ||
                lineSplits[0].Equals("map_Ka", StringComparison.OrdinalIgnoreCase)) // diffuse/albedo texture
            {
                string albedoTextureName = TofuPath.Combine(objMaterialDirectory, lineSplits[1]);
                currentObjMaterialDefinition.AlbedoTexturePath = albedoTextureName;
                Tofu.AssetImportManager
                    .ImportAsset(
                        albedoTextureName); // we need to import it because this is called on model import, so textures are not guaranteed to be imported yet
            }

            if (lineSplits[0].Equals("map_d", StringComparison.OrdinalIgnoreCase)) // diffuse/albedo texture
            {
                string alphaMaskTextureName = TofuPath.Combine(objMaterialDirectory, lineSplits[1]);
                currentObjMaterialDefinition.AlphaMaskTexturePath = alphaMaskTextureName;
                Tofu.AssetImportManager
                    .ImportAsset(
                        alphaMaskTextureName); // we need to import it because this is called on model import, so textures are not guaranteed to be imported yet
            }

            if (lineSplits[0].Equals("map_bump", StringComparison.OrdinalIgnoreCase) ||
                lineSplits[0].Equals("bump", StringComparison.OrdinalIgnoreCase)) // diffuse/albedo texture
            {
                string normalTextureName = TofuPath.Combine(objMaterialDirectory, lineSplits[1]);
                currentObjMaterialDefinition.NormalTexturePath = normalTextureName;
                Tofu.AssetImportManager
                    .ImportAsset(
                        normalTextureName); // we need to import it because this is called on model import, so textures are not guaranteed to be imported yet
            }
        }

        _objMaterialFiles[objMaterialPath] = objMaterialFileDefinition;
    }

    private MeshFile? CreateMeshFileFromObj(string modelFilePath,
        int meshIndex,
        string[] data,
        List<float> positions,
        List<float> uvs,
        List<float> normals,
        ref int lineStartIndex,
        bool singleMesh = false,
        bool smoothNormals = true,
        ObjMaterialFileDefinition objMaterialFileDefinition = null)
    {
        List<uint> indices = new List<uint>();
        ObjMaterialDefinition? objMaterialDefinition = objMaterialFileDefinition?.Materials.LastOrDefault() ?? null;
        Dictionary<VertexDataKey, uint> uniqueVertices = new Dictionary<VertexDataKey, uint>();
        uint currentUniqueVertexIndex = 0;
        List<float> everything = new List<float>();
        int numberOfIndicesPerLine = 0;
        int totalVerticesCount = 0;

        string? foundMeshName = null;
        bool isInMesh = false;
        if (singleMesh)
        {
            isInMesh = true;
        }

        if (isInMesh == false)
        {
            bool hasG = false;
            for (int lineIndex = lineStartIndex; lineIndex < data.Length; lineIndex++)
            {
                if (data[lineIndex].StartsWith("g ", StringComparison.OrdinalIgnoreCase) ||
                    data[lineIndex].StartsWith("o ", StringComparison.OrdinalIgnoreCase))
                {
                    hasG = true;
                }
            }

            if (hasG == false)
            {
                isInMesh = true;
            }
        }

        for (int lineIndex = lineStartIndex; lineIndex < data.Length; lineIndex++)
        {
            string line = data[lineIndex].Trim();
            line = line.Replace("\r", "");

            string[] lineSplits = line.Split(' ');
            if (line.StartsWith("f ")) // indices
            {
                numberOfIndicesPerLine = lineSplits.Length - 1;
                bool isQuad = numberOfIndicesPerLine == 4;

                int[] indicesSequence = new int[] { 0, 1, 2 };
                if (isQuad)
                {
                    indicesSequence = new int[]
                    {
                        0, 1, 2,
                        0, 2, 3
                    };
                }


                for (int k = 0; k < indicesSequence.Length; k++)
                {
                    int indiceIndex = indicesSequence[k];
                    totalVerticesCount++;


                    string[] group = lineSplits[indiceIndex + 1].Split('/');
                    for (int i = 0; i < group.Length; i++)
                    {
                        if (group[i].Length == 0)
                        {
                            // throw new Exception("group[i].Length == 0");
                            group[i] = "1"; // 1 because we will do -1 few lines down
                        }
                    }

                    int positionIndex = int.Parse(group[0]) - 1;
                    int uvIndex = int.Parse(group[1]) - 1;
                    int normalIndex = int.Parse(group[2]) - 1;

                    // negative means its from the end of the vertex list
                    if (positionIndex < 0)
                    {
                        positionIndex = positions.Count / 3 + positionIndex + 1;
                    }

                    if (uvIndex < 0)
                    {
                        uvIndex = uvs.Count / 2 + uvIndex + 1;
                    }

                    if (normalIndex < 0)
                    {
                        normalIndex = normals.Count / 3 + normalIndex + 1;
                    }


                    everything.Add(positions[positionIndex * 3 + 0]);
                    everything.Add(positions[positionIndex * 3 + 1]);
                    everything.Add(positions[positionIndex * 3 + 2]);

                    if (uvs.Count == 0)
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


            if (
                ((line.StartsWith("g") || line.StartsWith("o ")) && singleMesh == false))
            {
                // if we're already in mesh
                if (isInMesh)
                {
                    break;
                }

                isInMesh = true;
            }

            if (line.StartsWith("g ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("o ", StringComparison.OrdinalIgnoreCase))
            {
                if (isInMesh)
                {
                    foundMeshName = lineSplits[1];
                }
            }

            if (line.StartsWith("usemtl") && objMaterialFileDefinition != null)
            {
                objMaterialDefinition =
                    objMaterialFileDefinition.Materials.FirstOrDefault(d => d.MaterialName == lineSplits[1]) ?? null;
            }

            lineStartIndex = lineIndex;
        }

        if (everything.Count == 0 || isInMesh == false)
        {
            return null;
        }

        int[] countsOfElements = { 3, 2, 3, 3, 3 }; // position, uv, normal, tangent, bitangent

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

        List<float> geometryBufferData = new List<float>();

        int trianglesCount = everything.Count / floatsPerTriangle;
        for (int triangleIndex = 0; triangleIndex < trianglesCount; triangleIndex++)
        {
            int baseIndex = triangleIndex * floatsPerTriangle;

            int v0 = baseIndex;
            int v1 = baseIndex + floatsPerVertex;
            int v2 = baseIndex + 2 * floatsPerVertex;

            Vector3 pos1 = new Vector3(everything[v0 + 0], everything[v0 + 1], everything[v0 + 2]);
            Vector2 uv1 = new Vector2(everything[v0 + 3], everything[v0 + 4]);
            Vector3 norm1 = new Vector3(everything[v0 + 5], everything[v0 + 6], everything[v0 + 7]);

            Vector3 pos2 = new Vector3(everything[v1 + 0], everything[v1 + 1], everything[v1 + 2]);
            Vector2 uv2 = new Vector2(everything[v1 + 3], everything[v1 + 4]);
            Vector3 norm2 = new Vector3(everything[v1 + 5], everything[v1 + 6], everything[v1 + 7]);

            Vector3 pos3 = new Vector3(everything[v2 + 0], everything[v2 + 1], everything[v2 + 2]);
            Vector2 uv3 = new Vector2(everything[v2 + 3], everything[v2 + 4]);
            Vector3 norm3 = new Vector3(everything[v2 + 5], everything[v2 + 6], everything[v2 + 7]);

            // Compute edges and UV differences
            Vector3 edge1 = pos2 - pos1;
            Vector3 edge2 = pos3 - pos1;
            Vector2 deltaUV1 = uv2 - uv1;
            Vector2 deltaUV2 = uv3 - uv1;

            float f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);
            Vector3 tangent = new Vector3
            (
                f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X),
                f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y),
                f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z)
            );
            Vector3 bitangent = new Vector3
            (
                f * (-deltaUV2.X * edge1.X + deltaUV1.X * edge2.X),
                f * (-deltaUV2.X * edge1.Y + deltaUV1.X * edge2.Y),
                f * (-deltaUV2.X * edge1.Z + deltaUV1.X * edge2.Z)
            );

            float[] vertex1 = new float[]
            {
                pos1.X, pos1.Y, pos1.Z,
                uv1.X, uv1.Y,
                norm1.X, norm1.Y, norm1.Z,
                tangent.X, tangent.Y, tangent.Z,
                bitangent.X, bitangent.Y, bitangent.Z
            };
            float[] vertex2 = new float[]
            {
                pos2.X, pos2.Y, pos2.Z,
                uv2.X, uv2.Y,
                norm2.X, norm2.Y, norm2.Z,
                tangent.X, tangent.Y, tangent.Z,
                bitangent.X, bitangent.Y, bitangent.Z
            };
            float[] vertex3 = new float[]
            {
                pos3.X, pos3.Y, pos3.Z,
                uv3.X, uv3.Y,
                norm3.X, norm3.Y, norm3.Z,
                tangent.X, tangent.Y, tangent.Z,
                bitangent.X, bitangent.Y, bitangent.Z
            };

            if (RenderingSettings.USE_INDICES == false)
            {
                geometryBufferData.AddRange(vertex1);
                geometryBufferData.AddRange(vertex2);
                geometryBufferData.AddRange(vertex3);
            }
            else
            {
                // Otherwise check for unique vertices (using position, normal, and UV as the key)
                VertexDataKey key1 = new VertexDataKey { Position = pos1, Normal = norm1, UV = uv1 };
                if (uniqueVertices.ContainsKey(key1))
                {
                    indices.Add(uniqueVertices[key1]);
                }
                else
                {
                    geometryBufferData.AddRange(vertex1);
                    uint index = currentUniqueVertexIndex++;
                    indices.Add(index);
                    uniqueVertices.Add(key1, index);
                }

                VertexDataKey key2 = new VertexDataKey { Position = pos2, Normal = norm2, UV = uv2 };
                if (uniqueVertices.ContainsKey(key2))
                {
                    indices.Add(uniqueVertices[key2]);
                }
                else
                {
                    geometryBufferData.AddRange(vertex2);
                    uint index = currentUniqueVertexIndex++;
                    indices.Add(index);
                    uniqueVertices.Add(key2, index);
                }

                VertexDataKey key3 = new VertexDataKey { Position = pos3, Normal = norm3, UV = uv3 };
                if (uniqueVertices.ContainsKey(key3))
                {
                    indices.Add(uniqueVertices[key3]);
                }
                else
                {
                    geometryBufferData.AddRange(vertex3);
                    uint index = currentUniqueVertexIndex++;
                    indices.Add(index);
                    uniqueVertices.Add(key3, index);
                }
            }
        }

        Mesh mesh = new Mesh
        {
            CountsOfElements = countsOfElements,
            GeometryBufferData = geometryBufferData.ToArray(),
            VerticesCount = (int)(geometryBufferData.Count / 14),
            Indices = indices.ToArray(),
            IndicesLength = indices.Count()
        };

        if (objMaterialDefinition != null)
        {
            if (objMaterialDefinition.GeneratedMaterialFilePath == null)
            {
                objMaterialDefinition.GeneratedMaterialFilePath =
                    CreateMaterialFromObjMaterialDefinition(objMaterialDefinition);
            }

            mesh.PathToObjMaterial = objMaterialDefinition.GeneratedMaterialFilePath;
        }


        MeshFile meshFile = new MeshFile()
        {
            Mesh = mesh,
            UsesIndices = RenderingSettings.USE_INDICES,
        };


        string meshFileName = singleMesh
            ? modelFilePath + ".tofumesh"
            : AssetPathExtensions.ModelToMeshFileName(modelFilePath, meshIndex);
        string meshPath = AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(meshFileName);

        meshFile.Mesh.Name = foundMeshName ?? Path.GetFileNameWithoutExtension(meshPath);
        meshFile.Mesh.PathInLibraryFolder = meshPath;
        meshFile.PathInLibraryFolder = meshPath;

        Serializer.SaveAssetJSON<MeshFile>(meshPath, meshFile);

        return meshFile;
    }

    private static string? CreateMaterialFromObjMaterialDefinition(ObjMaterialDefinition? materialDefinition)
    {
        if (materialDefinition == null)
        {
            return null;
        }

        Asset_Material material = new Asset_Material
        {
            Shader = Tofu.ShaderManager.LoadShader(TofuPath.Combine(Folders.EngineResourcesShaders,
                "ModelRendererInstanced.glsl")),
            SmoothShadows = true,
            AlbedoColor = materialDefinition.AlbedoColor
        };
        if (materialDefinition.AlbedoColor.A < 255)
        {
            material.RenderMode = RenderMode.Transparent;
        }

        if (materialDefinition.AlbedoTexturePath != null)
        {
            string pathOfAlbedoTexture =
                AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(materialDefinition
                    .AlbedoTexturePath);
            RuntimeTexture texture =
                Tofu.AssetLoadManager.Get<RuntimeTexture>(pathOfAlbedoTexture);
            material.AlbedoTexture = texture;
        }

        if (materialDefinition.AlphaMaskTexturePath != null)
        {
            string pathOfAlphaMaskTexture =
                AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(materialDefinition
                    .AlphaMaskTexturePath);
            RuntimeTexture texture =
                Tofu.AssetLoadManager.Get<RuntimeTexture>(pathOfAlphaMaskTexture);
            material.AlphaMaskTexture = texture;

            material.RenderMode = RenderMode.Transparent;
        }

        if (materialDefinition.NormalTexturePath != null)
        {
            string pathOfNormalTexture =
                AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(materialDefinition
                    .NormalTexturePath);
            RuntimeTexture texture =
                Tofu.AssetLoadManager.Get<RuntimeTexture>(pathOfNormalTexture);
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

        ParallelOptions opt = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
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


                if (smoothedNormals.TryGetValue(t1position1, out Vector3 normal1))
                {
                    everything[triangle1StartIndex + offset + 0] = normal1.X;
                    everything[triangle1StartIndex + offset + 1] = normal1.Y;
                    everything[triangle1StartIndex + offset + 2] = normal1.Z;
                }

                if (smoothedNormals.TryGetValue(t1position2, out Vector3 normal2))
                {
                    everything[triangle1StartIndex + offset + floatsPerVertex + 0] = normal2.X;
                    everything[triangle1StartIndex + offset + floatsPerVertex + 1] = normal2.Y;
                    everything[triangle1StartIndex + offset + floatsPerVertex + 2] = normal2.Z;
                }

                if (smoothedNormals.TryGetValue(t1position3, out Vector3 normal3))
                {
                    everything[triangle1StartIndex + offset + floatsPerVertex + floatsPerVertex + 0] = normal3.X;
                    everything[triangle1StartIndex + offset + floatsPerVertex + floatsPerVertex + 1] = normal3.Y;
                    everything[triangle1StartIndex + offset + floatsPerVertex + floatsPerVertex + 2] = normal3.Z;
                }
            });
    }
}