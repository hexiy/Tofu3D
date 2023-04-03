// namespace Tofu3D;
//
// public class AssetsCache
// {
// 	static Dictionary<int, Asset> _assets = new();
//
// 	public static Asset GetAsset(string assetPath)
// 	{
// 		int hash = GetHash(textureLoadSettings.Paths[0]);
// 		if (_cachedTextures.ContainsKey(hash) == false)
// 		{
// 			Texture texture = TextureLoader.LoadTexture(textureLoadSettings);
// 			_cachedTextures.Add(hash, texture);
// 			return texture;
// 		}
//
// 		return _cachedTextures[hash];
// 	}
//
// 	public static void DisposeAsset(string assetPath)
// 	{
// 		if (_cachedTextures.ContainsKey(GetHash(texturePath)))
// 		{
// 			GL.DeleteTexture(_cachedTextures[GetHash(texturePath)].Id);
//
// 			_cachedTextures.Remove(GetHash(texturePath));
// 		}
// 	}
// }