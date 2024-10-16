﻿[ExecuteInEditMode]
public class MouseTracker : Component, IComponentUpdateable
{
    private bool _clicked;
    public float ScaleSpeed = 20;
    public float ScaleTarget = 0.4f;

    public void Update()
    {
        Transform.WorldPosition = Tofu.MouseInput.PositionInView;
        Transform.Rotation = Transform.Rotation.Set(z: Transform.Rotation.Z + Time.EditorDeltaTime * 150);
        if (Tofu.MouseInput.ButtonPressed() || Tofu.MouseInput.ButtonPressed(MouseButtons.Right))
        {
            Transform.LocalScale = Vector3.One;

            _clicked = true;
        }
        else if (Tofu.MouseInput.ButtonReleased())
        {
            _clicked = false;
        }

        if (_clicked)
        {
            Transform.LocalScale = Vector3.Lerp(Transform.LocalScale, Vector3.One * ScaleTarget,
                Time.EditorDeltaTime * ScaleSpeed);
        }
        else
        {
            Transform.LocalScale = Vector3.Lerp(Transform.LocalScale, Vector3.One, Time.EditorDeltaTime * ScaleSpeed);
        }
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }
}