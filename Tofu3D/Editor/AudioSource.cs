using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpAudio;
using SharpAudio.Codec;

namespace Tofu3D;

[ExecuteInEditMode]
public class AudioSource : Component
{
	[Show]
	public AudioClip Clip;

	[SliderF(0, 1)]
	public float Volume;

	[XmlIgnore]
	public Action PlaySoundBtn;
	[XmlIgnore]
	public Action StopSoundBtn;

	SoundStream _soundStream;
	public static AudioEngine AudioEngine;
	bool _initialized = false;
	MemoryStream _audioMemoryStream;
	string _loadedAudioFileName = "";

	public override void Awake()
	{
		AudioEngine = AudioEngine.CreateDefault(new AudioEngineOptions(44800, 2));
		LoadAudioToMemory(null);

		PlaySoundBtn += PlaySound;
		StopSoundBtn += StopSound;

		base.Awake();
	}

	private void LoadAudioToMemory(Action onLoaded)
	{
		ThreadStart threadStart = async () =>
		{
			byte[] bytes = await File.ReadAllBytesAsync(Clip.Path);
			_loadedAudioFileName = Clip.Path;
			if (_audioMemoryStream != null)
			{
				_audioMemoryStream.Close();
				await _audioMemoryStream.DisposeAsync();
			}

			_audioMemoryStream = new MemoryStream(bytes);
			_soundStream = new SoundStream(_audioMemoryStream, AudioEngine);

			onLoaded?.Invoke();
		};
		threadStart.Invoke();
	}

	public void PlaySound()
	{
		if (_soundStream?.State == SoundStreamState.Playing)
		{
			return;
		}

		if (Clip.Path != _loadedAudioFileName)
		{
			LoadAudioToMemory(onLoaded: PlaySound);
			return;
		}

		_audioMemoryStream.Position = 0;

		_initialized = true;
		_soundStream.Play();
	}

	public void PauseSound()
	{
		if (_initialized == false)
		{
			return;
		}

		_soundStream.State = SoundStreamState.Paused;
	}

	public void StopSound()
	{
		if (_initialized == false)
		{
			return;
		}

		_soundStream.State = SoundStreamState.Paused;
	}

	public override void Update()
	{
		if (_soundStream == null)
		{
			return;
		}

		_soundStream.Volume = Volume;
		base.Update();
	}
}