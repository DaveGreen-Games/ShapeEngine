using ShapeEngine.Random;
namespace ShapeEngine.Audio;

/// <summary>
/// Represents a playlist of songs, supporting random playback and queue management.
/// </summary>
internal class Playlist
{
    /// <summary>
    /// Event triggered when the playlist requests the next song.
    /// </summary>
    public event Action<uint, Playlist>? RequestNextSong;

    /// <summary>
    /// Event triggered when a song starts playing in the playlist.
    /// </summary>
    public event Action<string, string>? SongStarted;

    private HashSet<uint> mixtape;
    private List<uint> queue = new();

    /// <summary>
    /// The unique ID of the playlist.
    /// </summary>
    public uint ID { get; private set; }

    /// <summary>
    /// The currently playing song.
    /// </summary>
    public Song? CurrentSong { get; private set; }

    /// <summary>
    /// The display name of the playlist.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Indicates whether the playlist is paused.
    /// </summary>
    public bool Paused { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Playlist"/> class.
    /// </summary>
    /// <param name="id">The unique playlist ID.</param>
    /// <param name="displayName">The display name of the playlist.</param>
    /// <param name="songIDs">The set of song IDs in the playlist.</param>
    public Playlist(uint id, string displayName, HashSet<uint> songIDs)
    {
        this.ID = id;
        this.DisplayName = displayName;
        this.mixtape = songIDs;
        this.Refill();
    }

    /// <summary>
    /// Updates the playlist and advances to the next song if needed.
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
    public void Update(float dt)
    {
        if (Paused) return;

        if (CurrentSong != null && CurrentSong.Update(dt))
        {
            CurrentSong.Stop();
            uint id = PopNextID();
            RequestNextSong?.Invoke(id, this);
        }
    }

    /// <summary>
    /// Closes the playlist and stops playback.
    /// </summary>
    public void Close()
    {
        if (CurrentSong != null) CurrentSong.Stop();
        CurrentSong = null;
        mixtape.Clear();
        queue.Clear();
    }

    /// <summary>
    /// Returns whether the playlist is currently playing a song.
    /// </summary>
    public bool IsPlaying()
    {
        return CurrentSong != null;
    }

    /// <summary>
    /// Starts playback of the playlist.
    /// </summary>
    public void Start()
    {
        if (IsPlaying()) return;
        uint id = PopNextID();
        RequestNextSong?.Invoke(id, this);
    }

    /// <summary>
    /// Stops playback of the playlist.
    /// </summary>
    public void Stop()
    {
        if (CurrentSong == null) return;
        CurrentSong.Stop();
        CurrentSong = null;
    }

    /// <summary>
    /// Pauses playback of the playlist.
    /// </summary>
    public void Pause()
    {
        Paused = true;
        if (CurrentSong == null) return;
        CurrentSong.Pause();
    }

    /// <summary>
    /// Resumes playback of the playlist.
    /// </summary>
    public void Resume()
    {
        Paused = false;
        if (CurrentSong == null) return;
        CurrentSong.Resume();
    }

    /// <summary>
    /// Delivers the next song to be played in the playlist.
    /// </summary>
    /// <param name="song">The song to play.</param>
    public void DeliverNextSong(Song song)
    {
        CurrentSong = song;
        SongStarted?.Invoke(song.DisplayName, DisplayName);
        song.Play();
    }

    /// <summary>
    /// Adds a song ID to the playlist.
    /// </summary>
    public void AddSongID(uint id)
    {
        if (mixtape.Add(id))
        {
            queue.Add(id);
        }
    }

    /// <summary>
    /// Adds multiple song IDs to the playlist.
    /// </summary>
    public void AddSongIDs(params uint[] ids)
    {
        foreach (var id in ids)
        {
            if (mixtape.Add(id))
            {
                queue.Add(id);
            }
        }
    }

    private uint PopNextID()
    {
        if (queue.Count <= 0) Refill();
        int index = Rng.Instance.RandI(0, queue.Count);
        uint nextID = queue[index];
        queue.RemoveAt(index);
        return nextID;
    }
    private void Refill()
    {
        queue.Clear();
        queue.AddRange(mixtape);
    }

}
