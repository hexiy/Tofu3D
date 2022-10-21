namespace Tofu3D.Tweening;

public class TweenManager
{
	public List<Tween> ActiveTweens = new();

	public TweenManager()
	{
		I = this;
	}

	public static TweenManager I { get; private set; }

	public Tween StartTween(Tween tween)
	{
		ActiveTweens.Add(tween);
		return ActiveTweens[ActiveTweens.Count - 1];
	}

	public void Update()
	{
		for (int i = ActiveTweens.Count - 1; i >= 0; i--)
		{
			if (ActiveTweens[i].CurrentTime == 0 && ActiveTweens[i].Delay > 0)
			{
				ActiveTweens[i].CurrentTime = -ActiveTweens[i].Delay;
				ActiveTweens[i].Delay = -ActiveTweens[i].Delay;
			}

			ActiveTweens[i].CurrentTime += Time.EditorDeltaTime / ActiveTweens[i].Duration;
			bool isCompleted = ActiveTweens[i].CurrentTime > ActiveTweens[i].Duration;

			//activeTweens[i].currentTime = Mathf.Clamp(activeTweens[i].currentTime, -Math.Abs(activeTweens[i].delay), activeTweens[i].duration);
			if (ActiveTweens[i].CurrentTime >= 0)
			{
				ActiveTweens[i].OnUpdate.Invoke(ActiveTweens[i].GetValue());
			}

			if (isCompleted)
			{
				if (ActiveTweens[i].GetLoop() == Tween.LoopType.Restart)
				{
					ActiveTweens[i].CurrentTime = 0;
				}

				if (ActiveTweens[i].GetLoop() == Tween.LoopType.Yoyo)
				{
					ActiveTweens[i].OnComplete?.Invoke();
					ActiveTweens[i].CurrentTime = 0;

					float startValue = ActiveTweens[i].StartValue;
					ActiveTweens[i].StartValue = ActiveTweens[i].EndValue;
					ActiveTweens[i].EndValue = startValue;
				}

				if (ActiveTweens[i].GetLoop() == Tween.LoopType.NoLoop)
				{
					ActiveTweens[i].OnComplete?.Invoke();
					RemoveTween(i);
				}
			}
		}
	}

	public void RemoveTween(int index)
	{
		ActiveTweens.RemoveAt(index);
	}
}