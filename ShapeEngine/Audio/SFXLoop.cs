using System.Numerics;
using Raylib_cs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Audio;

/// <summary>
/// Represents a looping sound effect (SFXLoop), optionally with spatial audio support.
/// </summary>
internal class SFXLoop : Audio
{
    /// <summary>
    /// The Raylib sound resource for this SFXLoop.
    /// </summary>
    public Sound Sound { get; protected set; }

    /// <summary>
    /// Indicates whether the sound is currently looping.
    /// </summary>
    public bool IsLooping { get; protected set; }

    /// <summary>
    /// Indicates whether this SFXLoop uses spatial audio.
    /// </summary>
    public bool IsSpatial { get; set; }

    /// <summary>
    /// The spatial position for this SFXLoop.
    /// </summary>
    public Vector2 SpatialPos { get; set; }

    /// <summary>
    /// The minimum spatial range for full volume.
    /// </summary>
    public float MinSpatialRange { get; set; }

    /// <summary>
    /// The maximum spatial range for attenuation.
    /// </summary>
    public float MaxSpatialRange { get; set; }

    /// <inheritdoc/>
    public override bool IsPlaying() { return Raylib.IsSoundPlaying(Sound); }

    /// <summary>
    /// Initializes a new instance of the <see cref="SFXLoop"/> class.
    /// </summary>
    /// <param name="id">The unique SFXLoop ID.</param>
    /// <param name="sound">The Raylib sound resource.</param>
    /// <param name="buses">The buses this SFXLoop is routed through.</param>
    /// <param name="volume">The base volume.</param>
    /// <param name="pitch">The base pitch.</param>
    public SFXLoop(uint id, Sound sound, Bus[] buses, float volume = 0.5f, float pitch = 1.0f)
    {
        this.ID = id;
        this.Sound = sound;
        this.buses = buses;
        this.BasePitch = pitch;
        this.BaseVolume = volume;
        this.IsSpatial = false;
        foreach (var bus in this.buses)
        {
            bus.VolumeChanged += UpdateBusVolume;
            bus.Stopped += Stop;
        }
    }

    /// <summary>
    /// Initializes a new spatial instance of the <see cref="SFXLoop"/> class.
    /// </summary>
    /// <param name="id">The unique SFXLoop ID.</param>
    /// <param name="sound">The Raylib sound resource.</param>
    /// <param name="minSpatialRange">Minimum spatial range for full volume.</param>
    /// <param name="maxSpatialRange">Maximum spatial range for attenuation.</param>
    /// <param name="buses">The buses this SFXLoop is routed through.</param>
    /// <param name="volume">The base volume.</param>
    /// <param name="pitch">The base pitch.</param>
    public SFXLoop(uint id, Sound sound, float minSpatialRange, float maxSpatialRange, Bus[] buses, float volume = 0.5f, float pitch = 1.0f)
    {
        this.ID = id;
        this.Sound = sound;
        this.buses = buses;
        this.BasePitch = pitch;
        this.BaseVolume = volume;
        this.IsSpatial = true;
        this.MinSpatialRange = minSpatialRange;
        this.MaxSpatialRange = maxSpatialRange;
        foreach (var bus in this.buses)
        {
            bus.VolumeChanged += UpdateBusVolume;
            bus.Stopped += Stop;
        }
    }

    /// <summary>
    /// Updates the SFXLoop, handling spatial audio and looping logic.
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
    /// <param name="spatialCenter">The spatial center for attenuation calculations.</param>
    public virtual void Update(float dt, Vector2 spatialCenter)
    {
        if (Paused) return;

        bool playing = IsPlaying();
        if (IsLooping)
        {
            if (IsSpatial)
            {
                //Vector2 center;
                float disSq = (spatialCenter - SpatialPos).LengthSquared();
                if (MinSpatialRange < 0f) MinSpatialRange = 0f;
                if (MaxSpatialRange < 0f || MaxSpatialRange <= MinSpatialRange) MaxSpatialRange = MinSpatialRange + 1;
                float minSquared = MinSpatialRange * MinSpatialRange;
                float maxSquared = MaxSpatialRange * MaxSpatialRange;
                if (disSq >= maxSquared) return;

                float spatialVolumeFactor = 1f;
                if (disSq > minSquared)
                {
                    spatialVolumeFactor = 1f - ShapeMath.LerpInverseFloat(minSquared, maxSquared, disSq);
                }
                Raylib.SetSoundVolume(Sound, BaseVolume * spatialVolumeFactor);
            }

            if (!playing)
            {
                Raylib.PlaySound(Sound);
            }
        }
    }

    /// <inheritdoc/>
    protected override void UpdateBusVolume(float newBusVolume)
    {
        if (newBusVolume <= 0f)
        {
            if (IsPlaying() && !Paused) Pause();
        }
        else
        {
            if (Paused) Resume();
        }
        Raylib.SetSoundVolume(Sound, newBusVolume * BaseVolume * Volume);
    }

    /// <inheritdoc/>
    public override void Play(float volume = 1f, float pitch = 1f)
    {
        float busVolume = GetCombinedBusVolume();
        Volume = volume;
        Pitch = pitch;
        Raylib.SetSoundVolume(Sound, busVolume * BaseVolume * Volume);
        Raylib.SetSoundPitch(Sound, BasePitch * Pitch);
        Raylib.PlaySound(Sound);
        IsLooping = true;
    }

    /// <inheritdoc/>
    public override void Stop()
    {
        if (IsPlaying()) Raylib.StopSound(Sound);
        Paused = false;
        IsLooping = false;

    }

    /// <inheritdoc/>
    public override void Pause()
    {
        if (IsPlaying()) Raylib.PauseSound(Sound);
        Paused = true;
    }

    /// <inheritdoc/>
    public override void Resume()
    {
        if (Paused) Raylib.ResumeSound(Sound);
        Paused = false;
    }

    /// <inheritdoc/>
    public override void Unload()
    {
        Raylib.UnloadSound(Sound);
    }
}