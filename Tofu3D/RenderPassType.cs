namespace Tofu3D;

public enum RenderPassType
{
    Skybox,
    DirectionalLightShadowDepth,

    // MousePicking,
    //GeometryDepth,
    ZPrePass,
    Opaques,
    MousePicking,

    // Transparency,
    BloomThreshold,
    BloomPostProcess,

    PostProcess,
    UI
}