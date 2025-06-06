using Raylib_cs;

namespace ShapeEngine.Audio;


/// <summary>
/// Represents a single sound effect (SFX) audio instance.
/// </summary>
internal class SFX : Audio
{
    /// <summary>
    /// The Raylib sound resource for this SFX.
    /// </summary>
    public Sound Sound { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SFX"/> class.
    /// </summary>
    /// <param name="id">The unique SFX ID.</param>
    /// <param name="sound">The Raylib sound resource.</param>
    /// <param name="buses">The buses this SFX is routed through.</param>
    /// <param name="volume">The base volume.</param>
    /// <param name="pitch">The base pitch.</param>
    public SFX(uint id, Sound sound, Bus[] buses, float volume = 0.5f, float pitch = 1.0f)
    {
        this.ID = id;
        this.Sound = sound;
        this.buses = buses;
        this.BasePitch = pitch;
        this.BaseVolume = volume;
        foreach (var bus in this.buses)
        {
            //bus.VolumeChanged += UpdateBusVolume;
            bus.Stopped += Stop;
        }
    }

    /// <inheritdoc/>
    public override bool IsPlaying() { return Raylib.IsSoundPlaying(Sound); }

    /// <inheritdoc/>
    public override void Play(float volume = 1f, float pitch = 1f)
    {
        float busVolume = GetCombinedBusVolume();
        Volume = volume;
        Pitch = pitch;
        Raylib.SetSoundVolume(Sound, busVolume * BaseVolume * Volume);
        Raylib.SetSoundPitch(Sound, BasePitch * Pitch);
        Raylib.PlaySound(Sound);
    }

    /// <inheritdoc/>
    protected override void UpdateBusVolume(float newBusVolume)
    {
        Raylib.SetSoundVolume(Sound, newBusVolume * BaseVolume * Volume);
    }

    /// <inheritdoc/>
    public override void Stop()
    {
        if (!IsPlaying()) return;
        Raylib.StopSound(Sound);
        Paused = false;
    }

    /// <inheritdoc/>
    public override void Pause()
    {
        if (!IsPlaying()) return;
        Raylib.PauseSound(Sound);
        Paused = true;
    }

    /// <inheritdoc/>
    public override void Resume()
    {
        if (!Paused) return;
        Raylib.ResumeSound(Sound);
        Paused = false;
    }

    /// <inheritdoc/>
    public override void Unload()
    {
        Raylib.UnloadSound(Sound);
    }
}