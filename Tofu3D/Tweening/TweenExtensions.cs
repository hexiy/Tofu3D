namespace Tofu3D.Tweening;

public static class Tweener
{
	public static Tween Tween(float startValue, float endValue, float duration, Action<float> onUpdate)
	{
		Tween tween = new()
		              {Duration = duration, EndValue = endValue, StartValue = startValue, CurrentTime = 0, OnUpdate = onUpdate};
		return TweenManager.I.StartTween(tween);
	}

	public static void Kill(object target)
	{
		for (int i = 0; i < TweenManager.I.ActiveTweens.Count; i++)
		{
			if (TweenManager.I.ActiveTweens[i].Target == target)
			{
				TweenManager.I.RemoveTween(i);
				return;
			}
		}
	}
}