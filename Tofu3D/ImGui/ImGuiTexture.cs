using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Dear_ImGui_Sample;

public enum TextureCoordinate
{
	S = TextureParameterName.TextureWrapS,
	T = TextureParameterName.TextureWrapT,
	R = TextureParameterName.TextureWrapR
}

class ImGuiTexture : IDisposable
{
	public const SizedInternalFormat Srgb8Alpha8 = (SizedInternalFormat) All.Srgb8Alpha8;
	public const SizedInternalFormat Rgb32F = (SizedInternalFormat) All.Rgb32f;

	public const GetPName MaxTextureMaxAnisotropy = (GetPName) 0x84FF;

	public static readonly float MaxAniso;
	public readonly int GlTexture;
	public readonly SizedInternalFormat InternalFormat;
	public readonly int MipmapLevels;

	public readonly string Name;
	public readonly int Width, Height;

	static ImGuiTexture()
	{
		MaxAniso = GL.GetFloat(MaxTextureMaxAnisotropy);
	}

	public ImGuiTexture(string name, Bitmap image, bool generateMipmaps, bool srgb)
	{
		Name = name;
		Width = image.Width;
		Height = image.Height;
		InternalFormat = srgb ? Srgb8Alpha8 : SizedInternalFormat.Rgba8;

		if (generateMipmaps)
			// Calculate how many levels to generate for this texture
		{
			MipmapLevels = (int) Math.Floor(Math.Log(Math.Max(Width, Height), 2));
		}
		else
			// There is only one level
		{
			MipmapLevels = 1;
		}

		Util.CheckGlError("Clear");

		Util.CreateTexture(TextureTarget.Texture2D, Name, out GlTexture);
		GL.TextureStorage2D(GlTexture, MipmapLevels, InternalFormat, Width, Height);
		Util.CheckGlError("Storage2d");

		BitmapData data = image.LockBits(new Rectangle(0, 0, Width, Height),
		                                 ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

		GL.TextureSubImage2D(GlTexture, 0, 0, 0, Width, Height, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
		Util.CheckGlError("SubImage");

		image.UnlockBits(data);

		if (generateMipmaps)
		{
			GL.GenerateTextureMipmap(GlTexture);
		}

		GL.TextureParameter(GlTexture, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
		Util.CheckGlError("WrapS");
		GL.TextureParameter(GlTexture, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
		Util.CheckGlError("WrapT");

		GL.TextureParameter(GlTexture, TextureParameterName.TextureMinFilter, (int) (generateMipmaps ? TextureMinFilter.Linear : TextureMinFilter.LinearMipmapLinear));
		GL.TextureParameter(GlTexture, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
		Util.CheckGlError("Filtering");

		GL.TextureParameter(GlTexture, TextureParameterName.TextureMaxLevel, MipmapLevels - 1);

		// This is a bit weird to do here
		image.Dispose();
	}

	public ImGuiTexture(string name, int glTex, int width, int height, int mipmaplevels, SizedInternalFormat internalFormat)
	{
		Name = name;
		GlTexture = glTex;
		Width = width;
		Height = height;
		MipmapLevels = mipmaplevels;
		InternalFormat = internalFormat;
	}

	public ImGuiTexture(string name, int width, int height, IntPtr data, bool generateMipmaps = false, bool srgb = false)
	{
		Name = name;
		Width = width;
		Height = height;
		InternalFormat = srgb ? Srgb8Alpha8 : SizedInternalFormat.Rgba8;
		MipmapLevels = generateMipmaps == false ? 1 : (int) Math.Floor(Math.Log(Math.Max(Width, Height), 2));

		Util.CreateTexture(TextureTarget.Texture2D, Name, out GlTexture);
		GL.TextureStorage2D(GlTexture, MipmapLevels, InternalFormat, Width, Height);

		GL.TextureSubImage2D(GlTexture, 0, 0, 0, Width, Height, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data);

		if (generateMipmaps)
		{
			GL.GenerateTextureMipmap(GlTexture);
		}

		SetWrap(TextureCoordinate.S, TextureWrapMode.Repeat);
		SetWrap(TextureCoordinate.T, TextureWrapMode.Repeat);

		GL.TextureParameter(GlTexture, TextureParameterName.TextureMaxLevel, MipmapLevels - 1);
	}

	public void Dispose()
	{
		GL.DeleteTexture(GlTexture);
	}

	public void SetMinFilter(TextureMinFilter filter)
	{
		GL.TextureParameter(GlTexture, TextureParameterName.TextureMinFilter, (int) filter);
	}

	public void SetMagFilter(TextureMagFilter filter)
	{
		GL.TextureParameter(GlTexture, TextureParameterName.TextureMagFilter, (int) filter);
	}

	public void SetAnisotropy(float level)
	{
		const TextureParameterName textureMaxAnisotropy = (TextureParameterName) 0x84FE;
		GL.TextureParameter(GlTexture, textureMaxAnisotropy, Util.Clamp(level, 1, MaxAniso));
	}

	public void SetLod(int @base, int min, int max)
	{
		GL.TextureParameter(GlTexture, TextureParameterName.TextureLodBias, @base);
		GL.TextureParameter(GlTexture, TextureParameterName.TextureMinLod, min);
		GL.TextureParameter(GlTexture, TextureParameterName.TextureMaxLod, max);
	}

	public void SetWrap(TextureCoordinate coord, TextureWrapMode mode)
	{
		GL.TextureParameter(GlTexture, (TextureParameterName) coord, (int) mode);
	}
}