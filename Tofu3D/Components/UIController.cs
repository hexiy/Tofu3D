using Tofu3D.Tweening;

namespace Tofu3D.Components;

public class UiController : Component
{
	public GameObject Bg;
	[Show]
	public GameObject PlayBtn;

	public override void Start()
	{
		PlayBtn.GetComponent<Button>().OnReleasedAction += PlayClicked;
		base.Start();
	}

	void PlayClicked()
	{
		Tweener.Tween(1, 0, 0.7f, f =>
		{
			Debug.Log($"Tweening alpha progress:{f}");

			PlayBtn.GetComponent<Renderer>().Color = PlayBtn.GetComponent<Renderer>().Color.SetA(f);
			Bg.GetComponent<Renderer>().Color = Bg.GetComponent<Renderer>().Color.SetA(f);
		});
		//playBtn.GetComponent<Renderer>();
	}
}