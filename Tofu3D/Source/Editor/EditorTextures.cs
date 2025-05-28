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
        LogCategoryErrorIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EditorResourcesTextures,"Console/error.png"));
        LogCategoryInfoIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EditorResourcesTextures,"Console/info.png"));
        LogCategoryTimerIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EditorResourcesTextures,"Console/timer.png"));
        LogCategoryWarningIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EditorResourcesTextures,"Console/warning.png"));
        PlayIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EditorResourcesTextures,"play.png"));
        PauseIcon = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EditorResourcesTextures,"pause.png"));
        
        WhitePixel = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EditorResourcesTextures,"whitePixel.png"));
        TransparentPixel = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EditorResourcesTextures,"transparent.png"));
        Checkerboard = Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.EditorResourcesTextures,"checkerboard.png"));
    }
}