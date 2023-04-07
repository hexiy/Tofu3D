using System.IO;

[ExecuteInEditMode]
public class Grid : Component
{
	BoxShape _boxShape;
	SpriteRenderer _spriteRenderer;
	
	public override void Awake()
	{
		_boxShape = AddComponent<BoxShape>();
		_spriteRenderer = AddComponent<SpriteRenderer>();

		_spriteRenderer.LoadTexture(Path.Combine(Folders.Textures, "grid.png"));
		_spriteRenderer.Color = new Color(0, 0, 0, 29);
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
		float clampedOrthoSize = Mathf.ClampMin(Camera.I.OrthographicSize, 1);
		_boxShape.Size = Vector3.One * 17 * (clampedOrthoSize);
		_spriteRenderer.Tiling = _boxShape.Size;
		Transform.LocalScale = Vector3.One * (float) Math.Ceiling(clampedOrthoSize / 2);
		Transform.LocalPosition = Extensions.TranslateToGrid(Camera.I.Transform.LocalPosition, Transform.LocalScale.X) + Vector2.One * 1.5f * Transform.LocalScale.X;
		base.Update();
	}
}