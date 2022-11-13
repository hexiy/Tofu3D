using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;

namespace Tofu3D;

[ExecuteInEditMode]
public class AudioSource : Component
{
	[Show]
	public AudioClip Clip;

	[SliderF(0, 1)]
	public float Volume;

	int _processId = -1;

	public override void Awake()
	{
		base.Awake();
	}

	public async void PlaySound()
	{
		if (_processId != -1)
		{
			StopSound();
		}

		Thread tr = new Thread(new ThreadStart(() =>
		{
			StringBuilder sb = new StringBuilder();
			CancellationToken token = new CancellationToken(false);

			string path = Path.GetFullPath(Clip.Path);
			CancellationTokenSource source = new CancellationTokenSource();

			var x = " -v " + (decimal) Volume;
			x = x.Replace(',', '.');
			var command = Cli.Wrap("afplay")
			                 .WithArguments(string.Join(" ", path) + x)
			                 .ExecuteAsync(source.Token);
			_processId = command.ProcessId;
		}));
		tr.IsBackground = true;

		tr.Start();
		//tr.Start();
		// outputDevice.Play();
	}

	public void PauseSound()
	{
		// outputDevice.Stop();
	}

	public void StopSound()
	{
		if (_processId == -1)
		{
			return;
		}

		var killCommand = Cli.Wrap("kill")
		                     .WithArguments(" " + _processId)
		                     .ExecuteAsync();
		_processId = -1;
	}

	public override void Update()
	{
		base.Update();
	}
}