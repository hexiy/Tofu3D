// [ExecuteInEditMode]
// // ReSharper disable once InconsistentNaming
// public class ScrollUV : Component
// {
//     private TextureRenderer _textureRenderer;
//
//     public override void Awake()
//     {
//         _textureRenderer = GetComponent<TextureRenderer>();
//         base.Awake();
//     }
//
//     public override void Start()
//     {
//         base.Start();
//     }
//
//     public void Update()
//     {
//         _textureRenderer.Offset.Set(Time.EditorElapsedTime * 0.02f);
//     }
// }