[ExecuteInEditMode]
// ReSharper disable once InconsistentNaming
public class ScrollUV : Component
{
	TextureRenderer _textureRenderer;
	public override void Awake()
	{
		_textureRenderer= GetComponent<TextureRenderer>();
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		_textureRenderer.Offset.Set(x: Time.EditorElapsedTime * 0.02f);
		base.Update();
	}
}