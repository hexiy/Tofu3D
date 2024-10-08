﻿namespace Tofu3D;

[ExecuteInEditMode]
public class BlockPlacingController : Component, IComponentUpdateable
{
    [Show] public GameObject MovingCube;

    public void Update()
    {
        if (MovingCube != null)
        {
            var blockPosition = Camera.MainCamera.Transform.WorldPosition +
                                Camera.MainCamera.Transform
                                    .TransformVectorToWorldSpaceVector(new Vector3(1, 1, 10));
            blockPosition = blockPosition.TranslateToGrid(2);
            MovingCube.Transform.WorldPosition = blockPosition;
        }
    }
}