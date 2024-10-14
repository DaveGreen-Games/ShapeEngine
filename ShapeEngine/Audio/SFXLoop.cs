using System.Numerics;
using Raylib_cs;
using ShapeEngine.Lib;

namespace ShapeEngine.Audio;

internal class SFXLoop : Audio
{
    public Sound Sound { get; protected set; }
    public bool IsLooping { get; protected set; } = false;

    public bool IsSpatial { get; set; } = false;
    public Vector2 SpatialPos { get; set; } = new();
    public float MinSpatialRange { get; set; } = 0f;
    public float MaxSpatialRange { get; set; } = 0f;
    public override bool IsPlaying() { return Raylib.IsSoundPlaying(Sound); }
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
    public override void Stop()
    {
        if (IsPlaying()) Raylib.StopSound(Sound);
        Paused = false;
        IsLooping = false;

    }
    public override void Pause()
    {
        if (IsPlaying()) Raylib.PauseSound(Sound);
        Paused = true;
    }
    public override void Resume()
    {
        if (Paused) Raylib.ResumeSound(Sound);
        Paused = false;
    }

    public override void Unload()
    {
        Raylib.UnloadSound(Sound);
    }
}