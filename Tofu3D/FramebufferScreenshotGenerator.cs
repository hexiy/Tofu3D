using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Tofu3D;

public static class FramebufferScreenshotGenerator
{
    public static void TakeScreenshot(Framebuffer framebuffer)
    {
        framebuffer.Bind();
        byte[] framebufferData = new byte[(int)framebuffer.Size.X * (int)framebuffer.Size.Y * 4];
        GL.ReadPixels(0, 0, framebuffer.Size.Xi, framebuffer.Size.Yi, PixelFormat.Rgba, PixelType.UnsignedByte,
            ref framebufferData[0]);
        using (var image = Image.LoadPixelData<Rgba32>(framebufferData, framebuffer.Size.Xi, framebuffer.Size.Yi))
        {
            image.Mutate(x => x.Flip(FlipMode.Vertical));
            byte[] pixels = new byte[framebufferData.Length];
            image.CopyPixelDataTo(pixels);

            Asset_Texture texture = new Asset_Texture() { Pixels = pixels, TextureSize = framebuffer.Size };

            string path = Path.Combine(Folders.ThumbnailsInLibrary,
                "framebufferCapture" + Random.Range(0, 100) + ".png");
            string tofuTexturePath = path + ".tofutexture";
            QuickSerializer.SaveAssetJSON<Asset_Texture>(tofuTexturePath, texture);
            image.SaveAsPng(Path.Combine(Folders.ThumbnailsInLibrary,
                "framebufferCapture" + Random.Range(0, 100) + ".png"));
        }

        framebuffer.Unbind();
    }
}