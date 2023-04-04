namespace Tofu3D;

public class AssetLoadSettings<T>:IAssetLoadSettings where T : Asset<T>
{
	public string Path { get; internal set; }
}