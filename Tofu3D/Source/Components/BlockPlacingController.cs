namespace TofuEngine;

[ExecuteInEditMode]
public class BlockPlacingController : Component, IComponentUpdateable
{
    [Show] public GameObject MovingCube;

    public void Update()
    {
        if (MovingCube != null)
        {
            Vector3 blockPosition = Camera.GameViewCamera.Transform.WorldPosition +
                                    Camera.GameViewCamera.Transform
                                        .TransformVectorToWorldSpaceVector(new Vector3(1, 1, 10));
            blockPosition = blockPosition.TranslateToGrid(2);
            MovingCube.Transform.WorldPosition = blockPosition;
        }
    }
}