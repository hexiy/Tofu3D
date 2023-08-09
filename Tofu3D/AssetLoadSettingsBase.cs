using Microsoft.DotNet.PlatformAbstractions;

namespace Tofu3D;

public abstract class AssetLoadSettingsBase
{
	string _path = "";
	public string Path
	{
		get { return _path; }
		internal set { _path = value; }
	}

	public void ValidatePath()
	{
		AssetUtils.ValidateAssetPath(ref _path);
	}

	public override int GetHashCode()
	{
		return (_path ?? "").GetHashCode();
	}
}