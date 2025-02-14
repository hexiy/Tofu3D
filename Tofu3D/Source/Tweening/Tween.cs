namespace Tofu3D.Tweening;

public class Tween
{
    public enum LoopType
    {
        NoLoop,
        Yoyo,
        Restart
    }

    private LoopType _loopType;

    public float CurrentTime;
    public float Delay;
    public float Duration;
    public float EndValue;
    public Action OnComplete;
    public Action<float> OnUpdate;
    public float StartValue;
    public object Target;

    public float GetValue() => Mathf.Lerp(StartValue, EndValue, Mathf.Clamp(CurrentTime / Duration, 0, 1));

    public Tween SetLoop(LoopType lt)
    {
        _loopType = lt;
        return this;
    }

    public Tween SetDelay(float dl)
    {
        Delay = dl;
        return this;
    }

    public Tween SetOnComplete(Action onComplete)
    {
        OnComplete = onComplete;
        return this;
    }

    public LoopType GetLoop() => _loopType;

    public Tween SetTarget(object obj)
    {
        Target = obj;
        return this;
    }
}