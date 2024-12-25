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
        LogCategoryErrorIcon = Tofu.AssetLoadManager.Load<RuntimeTexture>("Resources/Console/error.png");
        LogCategoryInfoIcon = Tofu.AssetLoadManager.Load<RuntimeTexture>("Resources/Console/info.png");
        LogCategoryTimerIcon = Tofu.AssetLoadManager.Load<RuntimeTexture>("Resources/Console/timer.png");
        LogCategoryWarningIcon = Tofu.AssetLoadManager.Load<RuntimeTexture>("Resources/Console/warning.png");
        WhitePixel = Tofu.AssetLoadManager.Load<RuntimeTexture>("Resources/whitePixel.png");
    }
}