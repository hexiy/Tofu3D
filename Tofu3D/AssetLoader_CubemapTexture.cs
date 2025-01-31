namespace Tofu3D;

public class AssetLoader_CubemapTexture : AssetLoader<RuntimeCubemapTexture>
{
    public override RuntimeCubemapTexture LoadAsset(AssetLoadParameters<RuntimeCubemapTexture>? assetLoadParameters)
    {
        AssetLoadParameters_CubemapTexture loadParameters = assetLoadParameters as AssetLoadParameters_CubemapTexture;
        string[] pathsToSourceTextures = loadParameters.PathsToSourceTextures;


        Vector2 imageSize = Vector2.Zero;


        int textureId = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(textureId, TextureType.Cubemap);

        for (int textureIndex = 0; textureIndex < pathsToSourceTextures.Length; textureIndex++)
        {
            string path = AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(pathsToSourceTextures[textureIndex]);
            // Asset_Texture assetTexture = Tofu.AssetLoadManager.Load<Asset_Texture>(path);
            Asset_Texture assetTexture = Serializer.ReadAssetJSON<Asset_Texture>(path);


            // path = loadSettings.Paths[textureIndex];
            // var image = Image.Load<Rgba32>(assetTexture.PathToRawAsset);
            //
            // var pixels = new byte[4 * image.Width * image.Height];
            // image.Frames[0].CopyPixelDataTo(pixels);
            // image.Dispose();

            imageSize = new Vector2(assetTexture.TextureSize.X, assetTexture.TextureSize.Y);


            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + textureIndex, 0, PixelInternalFormat.Rgba,
                (int)imageSize.X, (int)imageSize.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte,
                assetTexture.Pixels);

            TextureTarget textureTarget = TextureTarget.TextureCubeMap;
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)loadParameters.WrapMode);
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)loadParameters.WrapMode);
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int)loadParameters.WrapMode);
            GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)loadParameters.FilterMode);
            GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)loadParameters.FilterMode);
            // GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter,
            //     (int)TextureMinFilter.LinearMipmapLinear);
            // GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        // GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
        // int maxMipLevels = (int)Math.Floor(Math.Log2(imageSize.X));
        // GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMaxLevel,
        //     maxMipLevels); // Generate mipmaps for the cubemap texture


        TofuGL.CheckGlError("cubemap texture load");

        RuntimeCubemapTexture runtimeCubemapTexture = new RuntimeCubemapTexture
        {
            Size = imageSize,
            Loaded = true,
            TextureId = textureId
        };

        return runtimeCubemapTexture;
    }
}