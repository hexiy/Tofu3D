namespace TofuEngine;

[ExecuteInEditMode]
public class RectTransform : Transform, IComponentUpdateable
{
    public RectTransform? ParentRectTransform => Parent as RectTransform;
    public Canvas? Canvas => GetComponentInParents<Canvas>();

    [Hide]
    public Vector2 SizeForRendering = new Vector2(100, 100);
    [Hide]
    public Vector2 PositionForRendering = new Vector2(0,0);

    [PositiveNumber]
    public Vector2 Size = new Vector2(100, 100);

    public Vector2 AnchorMin = new Vector2(0.5f, 0.5f);
    public Vector2 AnchorMax = new Vector2(0.5f, 0.5f);

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

    public void Update()
    {
        ClampAnchors();
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (ParentRectTransform == null)
        {
            SizeForRendering = Size;
            PositionForRendering = WorldPosition;
            return;
        }

        Vector2 parentSize = ParentRectTransform.SizeForRendering;
        Vector2 parentWorldBottomLeft = (Vector2)ParentRectTransform.WorldPosition - (parentSize * ParentRectTransform.Pivot);

        Vector2 anchorBoxWorldSize = (AnchorMax - AnchorMin) * parentSize;
        SizeForRendering.X = AnchorMin.X == AnchorMax.X ? Size.X : anchorBoxWorldSize.X;
        SizeForRendering.Y = AnchorMin.Y == AnchorMax.Y ? Size.Y : anchorBoxWorldSize.Y;

        Vector2 anchorMinWorldPoint = parentWorldBottomLeft + (parentSize * AnchorMin);

        Vector2 pivotReferencePoint = anchorMinWorldPoint + (anchorBoxWorldSize * Pivot);

        PositionForRendering = pivotReferencePoint + (Vector2)LocalPosition;
    }
}