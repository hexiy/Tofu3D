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
		List<uint> indices = new List<uint>();

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
				int indice1 = int.Parse(lineSplit[1][0].ToString());
				int indice2 = int.Parse(lineSplit[2][0].ToString());
				int indice3 = int.Parse(lineSplit[3][0].ToString());
				indices.Add((uint) indice1-1);
				indices.Add((uint) indice2-1);
				indices.Add((uint) indice3-1);
			}
		}

		Model model = new Model() {Vertices = vertices.ToArray(), Normals = normals.ToArray(), UVs = uvs.ToArray(), Indices = indices.ToArray()};
		BufferFactory.CreateModelBuffers(model);
		model.InitAssetHandle(model.Vao);
		model.AssetPath = loadSettings.Path;

		return model;
	}
}