using System.IO;

[ExecuteInEditMode]
public class Grid : Component, IComponentUpdateable
{
	BoxShape _boxShape;
	SpriteRenderer _spriteRenderer;
	public Vector2 PanSpeed = Vector2.Zero;

	public override void Awake()
	{
		_boxShape = GetComponent<BoxShape>() ?? AddComponent<BoxShape>();
		_spriteRenderer = GetComponent<SpriteRenderer>() ?? AddComponent<SpriteRenderer>();


		_spriteRenderer.Texture = Tofu.I.AssetManager.Load<Texture>(Path.Combine(Folders.Textures, "gridX.png"), TextureLoadSettings.DefaultSettingsSpritePixelArt);
		_spriteRenderer.Color = new Color(255, 255, 255, 255);
		_spriteRenderer.Layer = -10;
		_spriteRenderer.Batched = false;
		Transform.Pivot = new Vector3(0.5f, 0.5f, 0.5f);
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		// float clampedOrthoSize = Mathf.ClampMin(Camera.I.OrthographicSize, 1);
		// _boxShape.Size = Camera.I.Size;
		// _spriteRenderer.Tiling = _boxShape.Size / 100f / (10 / Camera.I.OrthographicSize);
		_spriteRenderer.Offset = Camera.MainCamera.Transform.WorldPosition * PanSpeed / _spriteRenderer.Tiling;
		Transform.LocalScale = Vector3.One;
		Transform.LocalPosition = Vector3.Zero;
		base.Update();
	}
}