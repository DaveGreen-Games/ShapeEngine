using Raylib_cs;

namespace ShapeEngine.Audio;

internal class Song : Audio
{
    public Music Music { get; protected set; }
    public string DisplayName { get; protected set; } = "";
    public Song(uint id, string displayName, Music song, Bus[] buses, float volume = 0.5f, float pitch = 1.0f)
    {
        this.ID = id;
        this.DisplayName = displayName;
        this.Music = song;
        this.buses = buses;
        this.BasePitch = pitch;
        this.BaseVolume = volume;
        foreach (var bus in this.buses)
        {
            bus.VolumeChanged += UpdateBusVolume;
            bus.Stopped += Stop;
        }
    }
    public override void Play(float volume = 1f, float pitch = 1f)
    {
        float busVolume = GetCombinedBusVolume();
        Volume = volume;
        Pitch = pitch;
        Raylib.SetMusicVolume(Music, busVolume * BaseVolume * Volume);
        Raylib.SetMusicPitch(Music, BasePitch * Pitch);
        Raylib.PlayMusicStream(Music);
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
        Raylib.SetMusicVolume(Music, newBusVolume * BaseVolume * Volume);
    }
    public override bool IsPlaying() { return Raylib.IsMusicStreamPlaying(Music); }
    public bool Update(float dt)
    {
        if (!IsPlaying()) return false;
        if (Paused) return false;
        Raylib.UpdateMusicStream(Music);
        float f = GetPercentage();
        return f > 0.95f;
    }
    public override void Stop()
    {
        if (!IsPlaying()) return;
        Raylib.StopMusicStream(Music);
        Paused = false;
    }
    public override void Pause()
    {
        if (!IsPlaying()) return;
        Raylib.PauseMusicStream(Music);
        Paused = true;
    }
    public override void Resume()
    {
        if (!Paused) return;
        Raylib.ResumeMusicStream(Music);
        Paused = false;
    }
    public override void Unload()
    {
        Raylib.UnloadMusicStream(Music);
    }
    public float GetPercentage()
    {
        float length = Raylib.GetMusicTimeLength(Music);
        float played = Raylib.GetMusicTimePlayed(Music);
        if (length <= 0.0f) return 0.0f;
        return played / length;
    }
}