namespace Tofu3D;

public enum RenderPassType
{
    Skybox,
    DirectionalLightShadowDepth,

    // MousePicking,
    //GeometryDepth,
    ZPrePass,
    Opaques,
    Transparency,
    MousePicking,

    BloomThreshold,
    BloomPostProcess,

    PostProcess,
    UI
}