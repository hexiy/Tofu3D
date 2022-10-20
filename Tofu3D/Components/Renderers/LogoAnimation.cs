using Tofu3D.Tweening;

public class LogoAnimation : Component
{
	public override void Awake()
	{
		base.Awake();
	}

	public override void Start()
	{
		Vector2 originalPosition = transform.position;
		Tweener.Tween(0, 0.4f, 1f, (f) => { transform.position = originalPosition + new Vector2(0, f); }).SetLoop(Tween.LoopType.Yoyo);
		base.Start();
	}

	public override void Update()
	{
		base.Update();
	}
}