public abstract class AssetLoader<T> : IAssetLoader where T : Asset<T>
{
	public abstract Asset<T> LoadAsset(IAssetLoadSettings loadSettings);

	public abstract void UnloadAsset(Asset<T> asset);
}