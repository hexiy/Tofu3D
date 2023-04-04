namespace Tofu3D;

public class AssetLoadSettings<T> : IAssetLoadSettings where T : Asset<T>
{
	string _path;
	public string Path
	{
		get { return _path; }
		internal set
		{
			_path = Folders.GetPathRelativeToEngineFolder(value);
		}
	}
}