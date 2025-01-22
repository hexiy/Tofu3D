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
        AssetLoadParameters_Texture loadParametersTexture = new AssetLoadParameters_Texture()
            { LoadType = TextureLoadType.InAtlas };
        LogCategoryErrorIcon = Tofu.AssetLoadManager.Load<RuntimeTexture>("Resources/Console/error.png",loadParametersTexture);
        LogCategoryInfoIcon = Tofu.AssetLoadManager.Load<RuntimeTexture>("Resources/Console/info.png",loadParametersTexture);
        LogCategoryTimerIcon = Tofu.AssetLoadManager.Load<RuntimeTexture>("Resources/Console/timer.png",loadParametersTexture);
        LogCategoryWarningIcon = Tofu.AssetLoadManager.Load<RuntimeTexture>("Resources/Console/warning.png",loadParametersTexture);
        WhitePixel = Tofu.AssetLoadManager.Load<RuntimeTexture>("Resources/whitePixel.png",loadParametersTexture);
    }
}