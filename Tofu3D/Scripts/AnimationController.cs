namespace Scripts;

public class AnimationController : Component, IComponentUpdateable
{
    public float AnimationSpeed = 1;

    public Vector2 AnimRangeIdle = new(0, 0);
    public Vector2 AnimRangeJump = new(0, 0);
    public Vector2 AnimRangeMeeleeAttack = new(0, 0);
    public Vector2 AnimRangeRun = new(0, 0);
    public Vector2 CurrentAnimRange = new(0, 0);
    private Vector2? _forcedAnimation;

    private Action _onAnimationFinished = () => { };
    private SpriteSheetRenderer _spriteSheetRenderer;
    private float _timeOnCurrentFrame;

    public override void Awake()
    {
        _spriteSheetRenderer = GetComponent<SpriteSheetRenderer>();
        base.Awake();
    }

    public override void Start()
    {
        //SetAnimation(animRange_Idle);

        base.Start();
    }

    public override void Update()
    {
        if (AnimationSpeed == 0) return;

        _timeOnCurrentFrame += Time.DeltaTime * AnimationSpeed;
        while (_timeOnCurrentFrame > 1 / AnimationSpeed)
        {
            _timeOnCurrentFrame -= 1 / AnimationSpeed;
            if (_spriteSheetRenderer.CurrentSpriteIndex + 1 >= CurrentAnimRange.Y)
            {
                _spriteSheetRenderer.CurrentSpriteIndex = (int)CurrentAnimRange.X;
                _onAnimationFinished?.Invoke();
            }
            else
            {
                _spriteSheetRenderer.CurrentSpriteIndex++;
            }
        }

        base.Update();
    }

    public void ResetCurrentAnimation()
    {
        _timeOnCurrentFrame = 0;
        _spriteSheetRenderer.CurrentSpriteIndex = (int)CurrentAnimRange.X;
    }

    public void Turn(Vector2 direction)
    {
        if (direction == Vector2.Right)
            Transform.Rotation = new Vector3(Transform.Rotation.X, 0, Transform.Rotation.Z);
        else
            Transform.Rotation = new Vector3(Transform.Rotation.X, 180, Transform.Rotation.Z);
    }

    public void Jump()
    {
        _forcedAnimation = AnimRangeJump;
        SetAnimation(_forcedAnimation.Value);
        AnimationSpeed = 4.5f;
        _onAnimationFinished += () =>
        {
            _forcedAnimation = null;

            SetAnimation(AnimRangeIdle);
            AnimationSpeed = 3;

            _onAnimationFinished = () => { };
        };
    }

    public void MeleeAttack()
    {
        _forcedAnimation = AnimRangeMeeleeAttack;
        SetAnimation(_forcedAnimation.Value);
        AnimationSpeed = 3f;
        _onAnimationFinished += () =>
        {
            _forcedAnimation = null;

            SetAnimation(AnimRangeIdle);
            AnimationSpeed = 3;

            _onAnimationFinished = () => { };
        };
    }

    public void SetAnimation(Vector2 animRange)
    {
        if (_forcedAnimation != null && _forcedAnimation != animRange) return;

        Vector2 oldAnim = CurrentAnimRange;

        CurrentAnimRange = animRange;

        if (oldAnim != CurrentAnimRange) ResetCurrentAnimation();
    }
}