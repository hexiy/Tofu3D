using System.IO;
using System.Threading;
using SharpAudio;
using SharpAudio.Codec;

namespace Tofu3D;

[ExecuteInEditMode]
public class AudioSource : Component, IComponentUpdateable
{
    public static AudioEngine AudioEngine = AudioEngine.CreateDefault(new AudioEngineOptions(48000, 2));
    private MemoryStream _audioMemoryStream;
    private bool _initialized;
    private string _loadedAudioFileName = "";

    private SoundStream _soundStream;
    private ThreadStart _threadStart;

    [Show] public AudioClip Clip;

    [XmlIgnore] public Action PlaySoundBtn;

    [XmlIgnore] public Action StopSoundBtn;

    [SliderF(0, 1)] public float Volume;

    public void Update()
    {
        if (_soundStream == null)
        {
            return;
        }

        _soundStream.Volume = Volume;
    }

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
            var bytes = File.ReadAllBytes(Clip.PathToRawAsset);
            _loadedAudioFileName = Clip.PathToRawAsset;
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

        if (Clip.PathToRawAsset != _loadedAudioFileName)
        {
            LoadAudioToMemory(PlaySound);
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
}