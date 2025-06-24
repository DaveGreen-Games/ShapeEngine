namespace ShapeEngine.Audio;

/// <summary>
/// Represents an audio bus for grouping and controlling the volume of multiple audio sources.
/// </summary>
internal class Bus
{
    /// <summary>
    /// Event triggered when the bus volume changes.
    /// </summary>
    public event Action<float>? VolumeChanged;

    /// <summary>
    /// Event triggered when the bus is stopped.
    /// </summary>
    public event Action? Stopped;

    private float volume;

    /// <summary>
    /// Gets or sets the volume of the bus.
    /// </summary>
    public float Volume
    {
        get
        {
            return volume;
        }
        set
        {
            volume = value;
            VolumeChanged?.Invoke(volume);
        }
    }

    /// <summary>
    /// The unique ID of the bus.
    /// </summary>
    public uint ID { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bus"/> class.
    /// </summary>
    /// <param name="id">The unique bus ID.</param>
    /// <param name="volume">The initial volume.</param>
    public Bus(uint id, float volume)
    {
        this.ID = id;
        this.volume = volume;
    }

    /// <summary>
    /// Stops all audio routed through this bus.
    /// </summary>
    public void Stop()
    {
        Stopped?.Invoke();
    }
}