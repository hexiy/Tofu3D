// using System.IO;
// using System.Linq;
//
// namespace TofuEngine;
//
// public class AssetLoader_AtlasTexture : AssetLoader<RuntimeAtlasTexture>
// {
//     public override RuntimeAtlasTexture LoadAsset(AssetLoadParameters<RuntimeAtlasTexture>? assetLoadParameters)
//     {
//         // AssetLoadParameters_Texture loadParameters = assetLoadParameters as AssetLoadParameters_Texture;
//         var loadParameters = assetLoadParameters;
//         string path = loadParameters.PathToAssetInLibrary;
//
//         if (File.Exists(AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(path)))
//         {
//             path = AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(path);
//         }
//
//         Asset_TextureAtlas assetTextureAtlas = Serializer.ReadAssetJSON<Asset_TextureAtlas>(path);
//
//         var pathOfImportParametersOfSourceAssetFile =
//             AssetPathExtensions.GetPathOfImportParametersOfSourceAssetFile(assetTextureAtlas.PathInAssetsFolder);
//         AssetImportParameters_Texture importParameters;
//
//         if (File.Exists(pathOfImportParametersOfSourceAssetFile))
//         {
//             importParameters =
//                 Serializer.ReadFileJSON<AssetImportParameters_Texture>(pathOfImportParametersOfSourceAssetFile);
//         }
//         else
//         {
//             importParameters = new AssetImportParameters_Texture();
//         }
//
//         var textureId = loadParameters.ExistingAsset?.GLTextureId ?? GL.GenTexture();
//         TextureHelper.BindTexture(textureId);
//         var textureTarget = TextureTarget.Texture2D;
//
//         var internalFormat = importParameters.IsSrgb ? PixelInternalFormat.SrgbAlpha : PixelInternalFormat.Rgba;
//
//         assetTextureAtlas.OnDeserialized(); // decompress pixels
//         GL.TexImage2D(textureTarget, 0, internalFormat, (int)assetTextureAtlas.TextureSize.X,
//             (int)assetTextureAtlas.TextureSize.Y, 0, PixelFormat.Rgba,
//             PixelType.UnsignedByte, assetTextureAtlas.Pixels);
//         
//         TextureWrapMode wrapMode = TextureWrapMode.Repeat;
//         TextureFilterMode filterMode = TextureFilterMode.Point;
//         
//         GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)wrapMode);
//         GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)wrapMode);
//         GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int)wrapMode);
//         GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)filterMode);
//         GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)filterMode);
//
//         TofuGL.CheckGlError("atlas texture load");
//
//
//         RuntimeAtlasTexture runtimeAtlasTexture = new()
//         {
//             PathInLibraryFolder = assetTextureAtlas.PathInLibraryFolder,
//             PathInAssetsFolder = assetTextureAtlas.PathInAssetsFolder,
//             GLTextureId = textureId,
//         };
//
//         return runtimeAtlasTexture;
//     }
// }