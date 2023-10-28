namespace Tofu3D;

public abstract class AssetLoadSettingsBase
{
    private string _path = "";

    public string Path
    {
        get => _path;
        internal set => _path = value;
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