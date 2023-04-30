using System.IO;
using System.Threading;
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
	public static AudioEngine AudioEngine = AudioEngine.CreateDefault(new AudioEngineOptions(48000, 2));
	bool _initialized = false;
	MemoryStream _audioMemoryStream;
	string _loadedAudioFileName = "";
	ThreadStart _threadStart;

	public override void Awake()
	{
		LoadAudioToMemory();

		PlaySoundBtn += PlaySound;
		StopSoundBtn += StopSound;

		base.Awake();
	}

	private void LoadAudioToMemory(Action onLoaded = null)
	{
		_threadStart = () =>
		{
			byte[] bytes = File.ReadAllBytes(Clip.AssetPath);
			_loadedAudioFileName = Clip.AssetPath;
			if (_audioMemoryStream != null)
			{
				_audioMemoryStream.Close();
				_audioMemoryStream.Dispose();
			}

			_audioMemoryStream = new MemoryStream(bytes);

			_soundStream = new SoundStream(_audioMemoryStream, AudioEngine);


			onLoaded?.Invoke();
		};
		_threadStart.Invoke();
	}

	public override void OnDestroyed()
	{
		base.OnDestroyed();
	}

	public void PlaySound()
	{
		if (_soundStream?.State == SoundStreamState.Playing)
		{
			return;
		}

		if (Clip.AssetPath != _loadedAudioFileName)
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