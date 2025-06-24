using Raylib_cs;

namespace ShapeEngine.Audio;

/// <summary>
/// Represents a music track (Song) for playback, supporting bus routing and playback control.
/// </summary>
internal class Song : Audio
{
    /// <summary>
    /// The Raylib music resource for this song.
    /// </summary>
    public Music Music { get; protected set; }

    /// <summary>
    /// The display name of the song.
    /// </summary>
    public string DisplayName { get; protected set; } = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="Song"/> class.
    /// </summary>
    /// <param name="id">The unique song ID.</param>
    /// <param name="displayName">The display name of the song.</param>
    /// <param name="song">The Raylib music resource.</param>
    /// <param name="buses">The buses this song is routed through.</param>
    /// <param name="volume">The base volume.</param>
    /// <param name="pitch">The base pitch.</param>
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

    /// <inheritdoc/>
    public override void Play(float volume = 1f, float pitch = 1f)
    {
        float busVolume = GetCombinedBusVolume();
        Volume = volume;
        Pitch = pitch;
        Raylib.SetMusicVolume(Music, busVolume * BaseVolume * Volume);
        Raylib.SetMusicPitch(Music, BasePitch * Pitch);
        Raylib.PlayMusicStream(Music);
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
        Raylib.SetMusicVolume(Music, newBusVolume * BaseVolume * Volume);
    }

    /// <inheritdoc/>
    public override bool IsPlaying() { return Raylib.IsMusicStreamPlaying(Music); }

    /// <summary>
    /// Updates the song playback and returns true if the song is near its end.
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>True if the song is near its end; otherwise, false.</returns>
    public bool Update(float dt)
    {
        if (!IsPlaying()) return false;
        if (Paused) return false;
        Raylib.UpdateMusicStream(Music);
        float f = GetPercentage();
        return f > 0.95f;
    }

    /// <inheritdoc/>
    public override void Stop()
    {
        if (!IsPlaying()) return;
        Raylib.StopMusicStream(Music);
        Paused = false;
    }

    /// <inheritdoc/>
    public override void Pause()
    {
        if (!IsPlaying()) return;
        Raylib.PauseMusicStream(Music);
        Paused = true;
    }

    /// <inheritdoc/>
    public override void Resume()
    {
        if (!Paused) return;
        Raylib.ResumeMusicStream(Music);
        Paused = false;
    }

    /// <inheritdoc/>
    public override void Unload()
    {
        Raylib.UnloadMusicStream(Music);
    }

    /// <summary>
    /// Gets the playback percentage of the song.
    /// </summary>
    /// <returns>A value between 0.0 and 1.0 representing the playback progress.</returns>
    public float GetPercentage()
    {
        float length = Raylib.GetMusicTimeLength(Music);
        float played = Raylib.GetMusicTimePlayed(Music);
        if (length <= 0.0f) return 0.0f;
        return played / length;
    }
}