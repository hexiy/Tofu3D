namespace TofuEngine;

[RequireComponent(typeof(RectShape))]
[ExecuteInEditMode]
public class RectTransform : Transform
{
    [LinkableComponent]
    private RectShape _rectShape;

    public override void Awake()
    {
        ReplaceNormalTransformComponent();
        base.Awake();
    }

    private void ReplaceNormalTransformComponent()
    {
        if (Transform == this)
        {
            return;
        }

        GameObject.RemoveComponent<Transform>();
        Transform = this;


        // Move this so it's first
        int index = GameObject.Components.IndexOf(this);
        if (index > 0)
        {
            GameObject.Components.RemoveAt(index);
            GameObject.Components.Insert(0, this);
        }
    }
}