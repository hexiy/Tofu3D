namespace TofuEngine;

public enum RenderPassType
{
    Skybox,
    DirectionalLightShadowDepth,
    PointLightShadowDepth,

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