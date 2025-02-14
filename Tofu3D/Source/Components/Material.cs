// using Microsoft.DotNet.PlatformAbstractions;
//
// namespace Scripts;
//
// [Serializable]
// public class Material : Asset<Material>
// {
//     public Shader Shader;
//     // public bool IsValid = true;
//     [Hide] public bool Additive = false;
//
//     public Texture AlbedoTexture;
//     public Color AlbedoTint = Color.White;
//     public Texture AmbientOcclusionTexture;
//     public Texture NormalTexture;
//     public Vector2 Offset;
//
//     public RenderMode RenderMode = RenderMode.Opaque;
//
//
//     public bool SpecularHighlightsEnabled;
//     public float SpecularSmoothness;
//     public Vector2 Tiling;
//
//     [XmlIgnore] [Hide] public int Vao;
//
//     public override int GetHashCode()
//     {
//         var hashCodeCombiner = HashCodeCombiner.Start();
//         hashCodeCombiner.Add(base.GetHashCode());
//         hashCodeCombiner.Add(Additive.GetHashCode());
//         hashCodeCombiner.Add(Shader?.GetHashCode());
//         hashCodeCombiner.Add(AlbedoTexture?.GetHashCode());
//         hashCodeCombiner.Add(AlbedoTint.GetHashCode());
//         hashCodeCombiner.Add(AmbientOcclusionTexture?.GetHashCode());
//         hashCodeCombiner.Add(NormalTexture?.GetHashCode());
//         hashCodeCombiner.Add(Tiling.GetHashCode());
//         hashCodeCombiner.Add(Offset.GetHashCode());
//         return hashCodeCombiner.CombinedHash;
//     }
//
//     public void LoadTextures()
//     {
//         if (AlbedoTexture?.Path.Length > 2)
//         {
//             AlbedoTexture = Tofu.AssetManager.Load<Texture>(AlbedoTexture.Path);
//         }
//
//         if (AmbientOcclusionTexture?.Path.Length > 2)
//         {
//             AmbientOcclusionTexture = Tofu.AssetManager.Load<Texture>(AmbientOcclusionTexture.Path);
//         }
//         
//         if (NormalTexture?.Path.Length > 2)
//         {
//             NormalTexture = Tofu.AssetManager.Load<Texture>(NormalTexture.Path);
//         }
//     }
//
//     public void SetShader(Shader shader)
//     {
//         Shader = shader;
//
//         if (Shader.IsLoaded == false)
//         {
//             Shader.Load();
//             BufferFactory.CreateBufferForShader(this);
//         }
//     }
//
//     public void InitShader()
//     {
//         SetShader(Shader);
//     }
//
//     public void Dispose()
//     {
//         Shader.Dispose();
//     }
// }