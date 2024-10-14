using Raylib_cs;

namespace ShapeEngine.Audio;

internal class SFX : Audio
{
    public Sound Sound { get; protected set; }

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
    public override bool IsPlaying() { return Raylib.IsSoundPlaying(Sound); }
    public override void Play(float volume = 1f, float pitch = 1f)
    {
        float busVolume = GetCombinedBusVolume();
        Volume = volume;
        Pitch = pitch;
        Raylib.SetSoundVolume(Sound, busVolume * BaseVolume * Volume);
        Raylib.SetSoundPitch(Sound, BasePitch * Pitch);
        Raylib.PlaySound(Sound);
    }
    protected override void UpdateBusVolume(float newBusVolume)
    {
        Raylib.SetSoundVolume(Sound, newBusVolume * BaseVolume * Volume);
    }
    public override void Stop()
    {
        if (!IsPlaying()) return;
        Raylib.StopSound(Sound);
        Paused = false;
    }
    public override void Pause()
    {
        if (!IsPlaying()) return;
        Raylib.PauseSound(Sound);
        Paused = true;
    }
    public override void Resume()
    {
        if (!Paused) return;
        Raylib.ResumeSound(Sound);
        Paused = false;
    }

    public override void Unload()
    {
        Raylib.UnloadSound(Sound);
    }
}