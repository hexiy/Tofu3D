using System.IO;
using Microsoft.DotNet.PlatformAbstractions;

namespace Scripts;

[Serializable]
public class Material : Asset<Material>
{
	// public bool IsValid = true;
	[Hide]
	public bool Additive = false;

	[Hide]
	public Shader Shader;

	[Hide]
	public int Vao;
	public RenderMode RenderMode = RenderMode.Opaque;

	public Texture AlbedoTexture;
	public Color AlbedoTint = Color.White;
	public Texture AoTexture;
	public Vector2 Tiling;
	public Vector2 Offset;
	public bool SpecularHighlightsEnabled;
	public float SpecularSmoothness;

	public override int GetHashCode()
	{
		HashCodeCombiner hashCodeCombiner = HashCodeCombiner.Start();
		hashCodeCombiner.Add(base.GetHashCode());
		hashCodeCombiner.Add(Additive.GetHashCode());
		hashCodeCombiner.Add(Shader?.GetHashCode());
		hashCodeCombiner.Add(AlbedoTexture?.GetHashCode());
		hashCodeCombiner.Add(AlbedoTint.GetHashCode());
		hashCodeCombiner.Add(AoTexture?.GetHashCode());
		hashCodeCombiner.Add(Tiling.GetHashCode());
		hashCodeCombiner.Add(Offset.GetHashCode());
		return hashCodeCombiner.CombinedHash;
	}

	public void LoadTextures()
	{
		if (AlbedoTexture)
		{
			AlbedoTexture = AssetManager.Load<Texture>(AlbedoTexture.AssetPath);
		}

		if (AoTexture)
		{
			AoTexture = AssetManager.Load<Texture>(AoTexture.AssetPath);
		}
	}

	public void SetShader(Shader shader)
	{
		Shader = shader;

		if (Shader.IsLoaded == false)
		{
			Shader.Load();
			BufferFactory.CreateBufferForShader(this);
		}
	}

	public void InitShader()
	{
		SetShader(Shader);
	}

	public void Dispose()
	{
		Shader.Dispose();
	}
}