using Milki.Extensions.MixPlayer.Devices;
using Milki.Extensions.MixPlayer.NAudioExtensions;

namespace KSTool.Om.Core;

public class AudioManager
{
    private AudioManager()
    {
        Engine = new AudioPlaybackEngine(DeviceDescription.WasapiDefault);
    }

    public AudioPlaybackEngine Engine { get; }

    public static AudioManager Instance { get; } = new();
}