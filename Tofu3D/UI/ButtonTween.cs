namespace Tofu3D.UI;

public class ButtonTween : Component
{
	bool _clicked;
	public float ScaleSpeed = 20;
	public float ScaleTarget = 0.9f;

	public override void Awake()
	{
		base.Awake();
	}

	public override void Update()
	{
		//if (needToScale == false) { return; }
		bool mouseInside = MouseInput.WorldPosition.In(GetComponent<BoxShape>());
		if (MouseInput.ButtonPressed() && mouseInside)
		{
			Transform.LocalScale = Vector3.One;

			_clicked = true;
		}
		else if (MouseInput.ButtonReleased())
		{
			_clicked = false;
		}

		if (_clicked)
		{
			Transform.LocalScale = Vector3.Lerp(Transform.LocalScale, Vector3.One * ScaleTarget, Time.DeltaTime * ScaleSpeed);
		}
		else
		{
			Transform.LocalScale = Vector3.Lerp(Transform.LocalScale, Vector3.One, Time.DeltaTime * ScaleSpeed);
		}
	}
}