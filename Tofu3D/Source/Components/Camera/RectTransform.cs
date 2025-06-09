namespace TofuEngine;

[ExecuteInEditMode]
public class RectTransform : Transform
{
    public RectTransform? ParentRectTransform => Parent as RectTransform;
    public Canvas? Canvas => GetComponentInParents<Canvas>();

    [PositiveNumber]
    public Vector2 Size;

    public Vector2 AnchorMin = new Vector2(0, 0);
    public Vector2 AnchorMax = new Vector2(1, 1);

    [Space]
    public Vector2 Pivot = new Vector2(0.5f, 0.5f);


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


        Parent = Transform.Parent;

        if (Parent != null)
        {
            Parent.RemoveChild(Transform.GameObjectId);
        }


        ParentId = Transform.ParentId;

        Children = Transform.Children;
        ChildrenIDs = Transform.ChildrenIDs;

        GameObject.RemoveComponent<Transform>();
        Transform = this;
        GameObject.Transform = this;

        if (Parent != null)
        {
            Parent.AddChild(this);
            // this.SetParent(Parent);
        }


        // Move this so it's first
        int index = GameObject.Components.IndexOf(this);
        if (index > 0)
        {
            GameObject.Components.RemoveAt(index);
            GameObject.Components.Insert(0, this);
        }
    }

    public override void EditorUpdate()
    {
        ClampAnchors();

        base.EditorUpdate();
    }

    private void ClampAnchors()
    {
        if (AnchorMax.X < AnchorMin.X)
        {
            AnchorMax.X = AnchorMin.X;
        }

        if (AnchorMax.Y < AnchorMin.Y)
        {
            AnchorMax.Y = AnchorMin.Y;
        }
    }
}