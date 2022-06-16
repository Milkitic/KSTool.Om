using Milki.Extensions.MixPlayer.Devices;
using Milki.Extensions.MixPlayer.NAudioExtensions;
using Milki.Extensions.MixPlayer.NAudioExtensions.Wave;

namespace KSTool.Om.Core;

public class AudioManager
{
    private CachedSoundSampleProvider? _currentCachedSoundSampleProvider;
    private AudioManager()
    {
        Engine = new AudioPlaybackEngine(new DeviceDescription
        {
            WavePlayerType = WavePlayerType.WASAPI,
            Latency = 1
        });
    }

    public AudioPlaybackEngine Engine { get; }

    public static AudioManager Instance { get; } = new();
    public void TryPlaySound(CachedSound sound)
    {
        TryRemoveCurrentSound();

        _currentCachedSoundSampleProvider = new CachedSoundSampleProvider(sound);
        Engine.RootMixer.AddMixerInput(_currentCachedSoundSampleProvider);
    }

    public void TryRemoveCurrentSound()
    {
        if (_currentCachedSoundSampleProvider == null) return;

        Engine.RootMixer.RemoveMixerInput(_currentCachedSoundSampleProvider);
        _currentCachedSoundSampleProvider = null;
    }
}