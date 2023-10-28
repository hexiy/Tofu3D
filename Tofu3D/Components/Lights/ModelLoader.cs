﻿using System.IO;

namespace Tofu3D;

public class ModelLoader : AssetLoader<Mesh>
{
    public override Mesh SaveAsset(ref Mesh asset, AssetLoadSettingsBase loadSettings)
    {
        throw new NotImplementedException();
    }

    public override void UnloadAsset(Asset<Mesh> asset)
    {
        GL.DeleteTexture(asset.Handle.Id);
    }

    public override Asset<Mesh> LoadAsset(AssetLoadSettingsBase assetLoadSettings)
    {
        ModelLoadSettings loadSettings = assetLoadSettings as ModelLoadSettings;

        string modelPath = loadSettings.Path;
        string[] data = File.ReadAllText(modelPath).Split("\n");

        List<float> vertices = new();
        List<float> uvs = new();
        List<float> normals = new();

        List<float> everything = new();
        int numberOfIndicesPerLine = 0;
        int totalVerticesCount = 0;
        foreach (string line in data)
        {
            string[] lineSplit = line.Split(' ');

            if (line.StartsWith("v ")) // positions
            {
                float x = float.Parse(lineSplit[1]);
                float y = float.Parse(lineSplit[2]);
                float z = float.Parse(lineSplit[3]);
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);
            }
            else if (line.StartsWith("vn ")) // normals
            {
                float x = float.Parse(lineSplit[1]);
                float y = float.Parse(lineSplit[2]);
                float z = float.Parse(lineSplit[3]);
                normals.Add(x);
                normals.Add(y);
                normals.Add(z);
            }
            else if (line.StartsWith("vt ")) // UVs
            {
                float x = float.Parse(lineSplit[1]);
                float y = float.Parse(lineSplit[2]);
                uvs.Add(x);
                uvs.Add(y);
            }
            else if (line.StartsWith("f ")) // indices
            {
                numberOfIndicesPerLine = lineSplit.Length - 1;
                for (int indiceIndex = 0; indiceIndex < numberOfIndicesPerLine; indiceIndex++)
                {
                    totalVerticesCount++;

                    string[] nums = lineSplit[indiceIndex + 1].Split('/');
                    for (int i = 0; i < nums.Length; i++)
                        if (nums[i].Length == 0)
                            nums[i] = "0";

                    int positionIndex = int.Parse(nums[0].ToString()) - 1;
                    int uvIndex = int.Parse(nums[1].ToString()) - 1;
                    int normalIndex = int.Parse(nums[2].ToString()) - 1;

                    everything.Add(vertices[positionIndex * 3]);
                    everything.Add(vertices[positionIndex * 3 + 1]);
                    everything.Add(vertices[positionIndex * 3 + 2]);

                    if (uvIndex == -1)
                    {
                        everything.Add(0);
                        everything.Add(0);
                    }
                    else
                    {
                        everything.Add(uvs[uvIndex * 2]);
                        everything.Add(uvs[uvIndex * 2 + 1]);
                    }

                    everything.Add(normals[normalIndex * 3]);
                    everything.Add(normals[normalIndex * 3 + 1]);
                    everything.Add(normals[normalIndex * 3 + 2]);
                }
            }
        }

        Mesh mesh = new();
        mesh.VertexBufferDataLength = everything.Count;
        mesh.VerticesCount = totalVerticesCount;
        int[] countsOfElements = new[] { 3, 2, 3 };

        BufferFactory.CreateModelBuffers(ref mesh.Vao, everything.ToArray(), countsOfElements);

        mesh.InitAssetRuntimeHandle(mesh.Vao);
        mesh.Path = loadSettings.Path;

        return mesh;
    }
}