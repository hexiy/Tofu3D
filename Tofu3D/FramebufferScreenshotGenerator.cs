using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Tofu3D;

public static class FramebufferScreenshotGenerator
{
    public static void TakeScreenshot(Framebuffer framebuffer, string fileName)
    {
        if (fileName.Contains(".png") == false)
        {
            fileName += ".png";
        }

        fileName = fileName.Replace(".tofutexture", "");

        framebuffer.Bind();
        byte[] framebufferData = new byte[framebuffer.Size.Xi * framebuffer.Size.Yi * 4];
        GL.ReadPixels(0, 0, framebuffer.Size.Xi, framebuffer.Size.Yi, PixelFormat.Rgba, PixelType.UnsignedByte,
            ref framebufferData[0]);
        
        using var image = Image.LoadPixelData<Rgba32>(framebufferData, framebuffer.Size.Xi, framebuffer.Size.Yi);
        Vector2 newSize = framebuffer.Size / 5f;

        image.Mutate(x =>
        {
            x.Flip(FlipMode.Vertical);
            x.Resize(newSize.Xi, newSize.Yi);
        });
        byte[] pixels = new byte[newSize.Xi*newSize.Yi * 4];
        image.CopyPixelDataTo(pixels);

        Asset_Texture texture = new Asset_Texture() { Pixels = pixels, TextureSize = newSize };

        string path = Path.Combine(Folders.SceneThumbnailsInLibrary,
            fileName);
        string tofuTexturePath = path + ".tofutexture";
        Serializer.SaveAssetJSON<Asset_Texture>(tofuTexturePath, texture);
        image.SaveAsPng(path);


        framebuffer.Unbind();
    }
}