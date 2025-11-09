namespace TofuEngine;

[ExecuteInEditMode]
public class RectTransform : Transform, IComponentUpdateable
{
    public RectTransform? ParentRectTransform => Parent as RectTransform;
    public Canvas? Canvas => GetComponentInParents<Canvas>();

    [Hide]
    public Vector2 CalculatedSize = new Vector2(100, 100);

    [Hide]
    public Vector2 CalculatedPosition = new Vector2(0, 0);

    [PositiveNumber]
    public Vector2 Size = new Vector2(100, 100);

    [PositiveNumber]
    public Vector2 AnchorMin = new Vector2(0.5f, 0.5f);

    [PositiveNumber]
    public Vector2 AnchorMax = new Vector2(0.5f, 0.5f);

    [Space]
    public Vector2 Pivot = new Vector2(0.5f, 0.5f);

    [Hide]
    public float? AspectRatio = null;


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
        ChildrenIds = Transform.ChildrenIds;

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
    }

    internal void CalculateLayoutForSelfAndChildren()
    {
        CalculateLayout();

        foreach (Transform transform in Transform.Children)
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null)
            {
                continue;
            }

            rectTransform.CalculateLayoutForSelfAndChildren();
        }
    }

    private void CalculateLayout()
    {
        if (ParentRectTransform == null)
        {
            CalculatedSize = Size;
            CalculatedPosition = WorldPosition;
            return;
        }

        Vector2 parentSize = ParentRectTransform.CalculatedSize;
        Vector2 parentWorldBottomLeft =
            (Vector2)ParentRectTransform.CalculatedPosition - (parentSize * ParentRectTransform.Pivot);

        Vector2 anchorBoxWorldSize = (AnchorMax - AnchorMin) * parentSize;
        CalculatedSize.X = AnchorMin.X == AnchorMax.X ? Size.X : anchorBoxWorldSize.X;
        CalculatedSize.Y = AnchorMin.Y == AnchorMax.Y ? Size.Y : anchorBoxWorldSize.Y;
        CalculatedSize = CalculatedSize / Screen.Scale;
        if (AspectRatio != null)
        {
            float currentAspectRatio = CalculatedSize.X / CalculatedSize.Y;
            if (currentAspectRatio != AspectRatio.Value)
            {
                if (currentAspectRatio > AspectRatio.Value) // current is too wide
                {
                    CalculatedSize.X = CalculatedSize.Y * AspectRatio.Value;
                    CalculatedSize.Y = CalculatedSize.X / AspectRatio.Value;
                }
                else
                {
                    CalculatedSize.Y = CalculatedSize.X / AspectRatio.Value;
                    CalculatedSize.X = CalculatedSize.Y / AspectRatio.Value;
                }
            }
        }

        Vector2 anchorMinWorldPoint = parentWorldBottomLeft + (parentSize * AnchorMin);

        Vector2 pivotReferencePoint = anchorMinWorldPoint + (anchorBoxWorldSize * Pivot);

        CalculatedPosition = pivotReferencePoint + (Vector2)LocalPosition;
    }
}