using System.IO;

namespace Tofu3D;

public class ModelLoader : AssetLoader<Model>
{
	public override void UnloadAsset(Asset<Model> asset)
	{
		GL.DeleteTexture(asset.AssetHandle.Id);
	}

	public override Asset<Model> LoadAsset(IAssetLoadSettings assetLoadSettings)
	{
		ModelLoadSettings loadSettings = assetLoadSettings as ModelLoadSettings;

		string modelPath = loadSettings.Path;
		string[] data = File.ReadAllText(modelPath).Split("\n");

		List<float> vertices = new List<float>();
		List<float> uvs = new List<float>();
		List<float> normals = new List<float>();

		List<float> everything = new List<float>();
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
				
				for (int indiceIndex = 1; indiceIndex < 4; indiceIndex++)
				{
					string[] nums = lineSplit[indiceIndex].Split('/');
					int positionIndex = int.Parse(nums[0].ToString()) - 1;
					int uvIndex = int.Parse(nums[1].ToString()) - 1;
					int normalIndex = int.Parse(nums[2].ToString()) - 1;

					everything.Add(vertices[positionIndex * 3]);
					everything.Add(vertices[positionIndex * 3 + 1]);
					everything.Add(vertices[positionIndex * 3 + 2]);

					everything.Add(uvs[uvIndex * 2]);
					everything.Add(uvs[uvIndex * 2 + 1]);

					everything.Add(normals[normalIndex * 3]);
					everything.Add(normals[normalIndex * 3 + 1]);
					everything.Add(normals[normalIndex * 3 + 2]);
				}
			}
		}

		Model model = new Model() {VertexBufferData = everything.ToArray()};
		BufferFactory.CreateModelBuffers(model);
		model.InitAssetHandle(model.Vao);
		model.AssetPath = loadSettings.Path;

		return model;
	}
}