namespace Tofu3D;

public class EditorTextures
{
    public readonly RuntimeTexture LogCategoryErrorIcon;
    public readonly RuntimeTexture LogCategoryInfoIcon;
    public readonly RuntimeTexture LogCategoryTimerIcon;
    public readonly RuntimeTexture LogCategoryWarningIcon;
    public readonly RuntimeTexture WhitePixel;

    public EditorTextures()
    {
        LogCategoryErrorIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>("Resources/Console/error.png");
        LogCategoryInfoIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>("Resources/Console/info.png");
        LogCategoryTimerIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>("Resources/Console/timer.png");
        LogCategoryWarningIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>("Resources/Console/warning.png");
        WhitePixel = Tofu.AssetLoadManager.Get<RuntimeTexture>("Resources/whitePixel.png");
    }
}