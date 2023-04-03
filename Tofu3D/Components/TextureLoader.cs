namespace Tofu3D.Components;

public class TextureLoader : AssetLoader<Texture>
{
	public TextureLoader()
	{
		AssetManager.AddAssetLoader(this);
	}

	public override Asset<Texture> LoadAsset(AssetLoadSettings<Texture> loadSettings)
	{
		throw new NotImplementedException();
	}

	public override void DisposeAsset(Asset<Texture> asset)
	{
		throw new NotImplementedException();
	}
}

