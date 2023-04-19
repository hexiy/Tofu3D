public abstract class AssetLoader<T> : IAssetLoader where T : Asset<T>
{
	public abstract Asset<T> LoadAsset(AssetLoadSettingsBase loadSettings);

	public abstract void SaveAsset(T asset, AssetLoadSettingsBase loadSettings);

	public abstract void UnloadAsset(Asset<T> asset);
}