﻿[ExecuteInEditMode]
public class TorusRotator : Component
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }

    public void Update()
    {
        Transform.Rotation = Transform.Rotation.Add(-Time.EditorDeltaTime * 10);
    }
}