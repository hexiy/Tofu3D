namespace Tofu3D.Tweening;

public static class Tweener
{
	public static Tween Tween(float startValue, float endValue, float duration, Action<float> onUpdate)
	{
		Tween tween = new()
		              {Duration = duration, EndValue = endValue, StartValue = startValue, CurrentTime = 0, OnUpdate = onUpdate};
		return Tofu.TweenManager.StartTween(tween);
	}

	public static void Kill(object target)
	{
		for (int i = 0; i < Tofu.TweenManager.ActiveTweens.Count; i++)
		{
			if (Tofu.TweenManager.ActiveTweens[i].Target == target)
			{
				Tofu.TweenManager.RemoveTween(i);
				return;
			}
		}
	}
}