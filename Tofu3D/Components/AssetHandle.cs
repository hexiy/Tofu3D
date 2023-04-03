public struct AssetHandle<T>
{
	public uint Id { get; private set; }
	public Type AssetType
	{
		get { return typeof(T); }
	}

	/*private AssetHandle()
	{
		
	}*/
	public AssetHandle(uint id)
	{
		Id = id;
	}
}