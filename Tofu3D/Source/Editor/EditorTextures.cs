using Microsoft.Build.Experimental.BuildCheck;

namespace TofuEngine;

public class EditorTextures
{
    public readonly RuntimeTexture LogCategoryErrorIcon;
    public readonly RuntimeTexture LogCategoryInfoIcon;
    public readonly RuntimeTexture LogCategoryTimerIcon;
    public readonly RuntimeTexture LogCategoryWarningIcon;
    public readonly RuntimeTexture PlayIcon;
    public readonly RuntimeTexture PauseIcon;
    public readonly RuntimeTexture WhitePixel;
    public readonly RuntimeTexture TransparentPixel;
    public readonly RuntimeTexture Checkerboard;

    public EditorTextures()
    {
        LogCategoryErrorIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EngineResourcesTextures,"Console/error.png"));
        LogCategoryInfoIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EngineResourcesTextures,"Console/info.png"));
        LogCategoryTimerIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EngineResourcesTextures,"Console/timer.png"));
        LogCategoryWarningIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EngineResourcesTextures,"Console/warning.png"));
        PlayIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EngineResourcesTextures,"play.png"));
        PauseIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EngineResourcesTextures,"pause.png"));
        
        WhitePixel = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EngineResourcesTextures,"whitePixel.png"));
        TransparentPixel = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EngineResourcesTextures,"transparent.png"));
        Checkerboard = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EngineResourcesTextures,"checkerboard.png"));
    }
}