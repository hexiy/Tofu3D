namespace Tofu3D;

public class RenderingBlendingHelper
{
    public static void SetBlendMode(BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                GL.Disable(EnableCap.Blend);
                break;
            case BlendMode.Cutout:
                GL.Disable(EnableCap.Blend);
                // discard in shader
                break;
            case BlendMode.Fade:
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                break;
            case BlendMode.Additive:
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                break;
            case BlendMode.PremultipliedAlpha:
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
                break;
            default:
                GL.Disable(EnableCap.Blend);
                break;
        }
    }
}