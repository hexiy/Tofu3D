public struct AssetHandle<T>
{
	required public uint Id { get; set; }
	public Type AssetType
	{
		get { return typeof(T); }
	}
}