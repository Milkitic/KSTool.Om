using Milki.Extensions.MixPlayer.NAudioExtensions.Wave;
using NAudio.Wave;

namespace KSTool.Om.Core.Models;

public class HitsoundCache
{
    private HitsoundCache(string path, CachedSound? cachedSound)
    {
        CachedSound = cachedSound;
        SoundFile = new SoundFile
        {
            FilePath = path
        };
    }

    public CachedSound? CachedSound { get; }

    public SoundFile SoundFile { get; }

    public static async Task<HitsoundCache> CreateAsync(WaveFormat waveFormat, string path)
    {
        return new HitsoundCache(path, await CachedSoundFactory.GetOrCreateCacheSound(waveFormat, path));
    }
}