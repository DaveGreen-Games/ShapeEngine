namespace ShapeEngine.Audio;

/// <summary>
/// Abstract base class for all audio types (SFX, SFXLoop, Song).
/// Provides common properties and methods for audio playback and control.
/// </summary>
internal abstract class Audio
{
    /// <summary>
    /// The base volume for this audio instance.
    /// </summary>
    public float BaseVolume { get; protected set; } = 0.5f;

    /// <summary>
    /// The current volume multiplier for this audio instance.
    /// </summary>
    public float Volume { get; set; } = 1f;

    /// <summary>
    /// The base pitch for this audio instance.
    /// </summary>
    public float BasePitch { get; protected set; } = 1.0f;

    /// <summary>
    /// The current pitch multiplier for this audio instance.
    /// </summary>
    public float Pitch { get; set; } = 1f;

    /// <summary>
    /// The unique ID of this audio instance.
    /// </summary>
    public uint ID { get; protected set; }

    /// <summary>
    /// Indicates whether this audio instance is paused.
    /// </summary>
    public bool Paused { get; protected set; }

    /// <summary>
    /// The buses this audio instance is routed through.
    /// </summary>
    protected Bus[] buses = new Bus[0];

    /// <summary>
    /// Returns whether the audio is currently playing.
    /// </summary>
    public virtual bool IsPlaying() { return false; }

    /// <summary>
    /// Plays the audio with the specified volume and pitch.
    /// </summary>
    public virtual void Play(float volume = 1f, float pitch = 1f) { }

    /// <summary>
    /// Stops the audio playback.
    /// </summary>
    public virtual void Stop() { }

    /// <summary>
    /// Pauses the audio playback.
    /// </summary>
    public virtual void Pause() { }

    /// <summary>
    /// Resumes the audio playback if paused.
    /// </summary>
    public virtual void Resume() { }

    /// <summary>
    /// Unloads the audio resource from memory.
    /// </summary>
    public virtual void Unload() { }

    /// <summary>
    /// Updates the audio's volume based on the bus volume.
    /// </summary>
    protected virtual void UpdateBusVolume(float newBusVolume) { }

    /// <summary>
    /// Gets the combined volume from all buses this audio is routed through.
    /// </summary>
    protected float GetCombinedBusVolume()
    {
        float v = 1f;
        foreach (var bus in buses)
        {
            if (bus.Volume <= 0f) return 0f;
            v *= bus.Volume;
        }
        return v;
    }
}
