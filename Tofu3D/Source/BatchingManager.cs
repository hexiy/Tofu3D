// using System.Linq;
//
// namespace Tofu3D;
//
// public class BatchingManager
// {
// 	private static Dictionary<int, Batcher> _batchers = new(); // textureID
// 	private static float[] _attribsSkeletonSprite = {0, 0, 0, 0};
// 	private static float[] _attribsSkeletonSpriteSheet = {0, 0, 0, 0};
//
// 	private static void CreateBatcherForTexture(Material material, Texture texture, Batcher.BatcherType batcherType)
// 	{
// 		Batcher batcher;
// 		// if (batcherType == Batcher.BatcherType.Sprite)
// 		// {
// 		batcher = new SpriteBatcher(1000, material, texture);
// 		// }
//
// 		_batchers.Add(texture.Id, batcher);
// 	}
//
// 	public static void AddObjectToBatcher(int textureId, SpriteRenderer renderer, int instanceIndex = 0)
// 	{
// 		if (_batchers.ContainsKey(textureId) == false)
// 		{
// 			CreateBatcherForTexture(renderer.Material, renderer.Texture, Batcher.BatcherType.Sprite);
// 		}
//
// 		_batchers[textureId].AddGameObject(renderer.GameObjectId, instanceIndex);
// 	}
//
// 	/*public static void AddObjectToBatcher(int textureID, SpriteSheetRenderer renderer, int instanceIndex = 0)
// 	{
// 		if (batchers.ContainsKey(textureID) == false)
// 		{
// 			CreateBatcherForTexture(renderer.material, renderer.texture, Batcher.BatcherType.SpriteSheet);
// 		}
//
// 		batchers[textureID].AddGameObject(renderer.gameObjectID, instanceIndex);
// 	}*/
//
// 	public static void UpdateAttribs(int textureId, int gameObjectId, Vector2 position, Vector2 size, float rotation, Color color, int instanceIndex = 0) //  use instanceIndex for particles-when we use single gameObject
// 	{
// 		if (_batchers.ContainsKey(textureId) == false)
// 		{
// 			return;
// 		}
//
// 		_attribsSkeletonSprite[0] = position.X;
// 		_attribsSkeletonSprite[1] = position.Y;
// 		_attribsSkeletonSprite[2] = size.X;
// 		_attribsSkeletonSprite[3] = size.Y;
//
// 		_batchers[textureId].SetAttribs(gameObjectId, _attribsSkeletonSprite, instanceIndex);
// 	}
//
// 	public static void RemoveAttribs(int textureId, int gameObjectId, int instanceIndex = 0)
// 	{
// 		if (_batchers.ContainsKey(textureId) == false)
// 		{
// 			return;
// 		}
//
// 		_attribsSkeletonSprite[0] = 0;
// 		_attribsSkeletonSprite[1] = 0;
// 		_attribsSkeletonSprite[2] = 0;
// 		_attribsSkeletonSprite[3] = 0;
//
// 		_batchers[textureId].SetAttribs(gameObjectId, _attribsSkeletonSprite, instanceIndex);
// 	}
//
// 	public static void RenderAllBatchers()
// 	{
// 		for (int i = 0; i < _batchers.Count; i++)
// 		{
// 			_batchers.ElementAt(i).Value.Render();
// 		}
// 	}
// }

