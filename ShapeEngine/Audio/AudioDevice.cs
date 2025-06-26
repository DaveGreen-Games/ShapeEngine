using Raylib_cs;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Audio;

    
/// <summary>
/// Central manager for all audio operations in the engine, including initialization, playback, spatial audio,
/// bus and playlist management, and resource cleanup.
/// Provides a high-level API for controlling sound effects,
/// music, and audio buses, as well as advanced features like spatialization and sound blocking.
/// </summary>
public class AudioDevice
{
    #region Members

    /// <summary>
    /// Tracks whether the audio device has been initialized to prevent redundant initialization.
    /// </summary>
    private static bool initialized;
    
    /// <summary>
    /// Provides unique, incrementing IDs for audio resources managed by this device.
    /// </summary>
    public readonly IdCounter IdCounter = new();

    /// <summary>
    /// Occurs when a playlist is started via this audio device.
    /// </summary>
    public event Action<string>? PlaylistStarted;

    /// <summary>
    /// Occurs when a new song begins playing in the current playlist.
    /// </summary>
    public event Action<string, string>? PlaylistSongStarted;

    /// <summary>
    /// The unique ID of the master audio bus, which all other buses and audio sources can be routed through.
    /// </summary>
    public static readonly uint BUS_MASTER = ShapeID.NextID;

    private readonly Dictionary<uint, Bus> buses = new();
    private readonly Dictionary<uint, SFX> sounds = new();
    private readonly Dictionary<uint, SFXLoop> loops = new();
    private readonly Dictionary<uint, Song> songs = new();
    
    private Playlist? currentPlaylist;
    private readonly Dictionary<uint, Playlist> playlists = new();
    private readonly Dictionary<uint, float> soundBlockers = new();

    /// <summary>
    /// Optional override for the spatial audio listener target. If set, spatial sounds will be centered on this object.
    /// </summary>
    public GameObject? SpatialTargetOverride { get; set; }

    private Rect cameraRect;

    #endregion
    
    #region Constructors

    /// <summary>
    /// Initializes a new <see cref="AudioDevice"/> instance, setting up the master bus and initializing the audio system if needed.
    /// </summary>
    public AudioDevice()
    {
        if (!initialized)
        {
            Raylib.InitAudioDevice();
            initialized = true;
        }
        BusAdd(BUS_MASTER, 1f);
    }

    /// <summary>
    /// Initializes a new <see cref="AudioDevice"/> instance, setting up the master bus and any additional buses provided.
    /// Initializes the audio system if needed.
    /// </summary>
    /// <param name="busIds">An array of bus IDs to create and add to the device.</param>
    public AudioDevice(params uint[] busIds)
    {
        if (!initialized)
        {
            Raylib.InitAudioDevice();
            initialized = true;
        }
        BusAdd(BUS_MASTER, 1f);
        foreach (var id in busIds)
        {
            BusAdd(id, 1f);
        }
    }
    #endregion
    
    #region Public

    /// <summary>
    /// Shuts down the audio device, releasing all resources, stopping all playback, and closing the underlying audio system.
    /// </summary>
    public void Close()
    {
        //StopBus
        currentPlaylist = null;
        foreach (var playlist in playlists.Values)
        {
            playlist.Close();
        }
        buses.Clear();
        playlists.Clear();
        soundBlockers.Clear();
        Raylib.CloseAudioDevice();
    }

    /// <summary>
    /// Updates the state of the audio device, including spatial audio calculations, playlist progression,
    /// and sound blocker timers. Should be called once per frame.
    /// </summary>
    /// <param name="dt">Elapsed time in seconds since the last update.</param>
    /// <param name="camera">The current camera, used for spatial audio calculations.</param>
    public void Update(float dt, ShapeCamera camera)
    {
        cameraRect = camera.Area;
        if (currentPlaylist != null) currentPlaylist.Update(dt);

        if (SpatialTargetOverride != null && SpatialTargetOverride.IsDead) SpatialTargetOverride = null;

        foreach (var key in soundBlockers.Keys)
        {
            if (soundBlockers[key] <= 0f) continue;
            soundBlockers[key] -= dt;
            if (soundBlockers[key] <= 0f)
            {
                soundBlockers[key] = 0f;
            }
        }

        foreach(var loop in loops.Values)
        {
            Vector2 center;
            if (SpatialTargetOverride == null) center = cameraRect.Center;
            else center = SpatialTargetOverride.Transform.Position;
            
            loop.Update(dt, center);
        }
    }
    
    #endregion

    #region Playlist

    /// <summary>
    /// Handles a playlist's request for the next song by delivering the corresponding <see cref="Song"/> instance.
    /// </summary>
    /// <param name="id">The song ID requested.</param>
    /// <param name="playlist">The playlist requesting the song.</param>
    private void OnPlaylistRequestSong(uint id, Playlist playlist)
    {
        if (songs.ContainsKey(id)) playlist.DeliverNextSong(songs[id]);
    }

    /// <summary>
    /// Invokes the <see cref="PlaylistSongStarted"/> event when a new song starts in a playlist.
    /// </summary>
    /// <param name="songName">The name of the song that started.</param>
    /// <param name="playlistName">The name of the playlist.</param>
    private void OnPlaylistSongStarted(string songName, string playlistName)
    {
        PlaylistSongStarted?.Invoke(songName, playlistName);
    }
    
    /// <summary>
    /// Adds a new playlist with a specific ID and a set of song IDs.
    /// </summary>
    /// <param name="id">The unique ID for the playlist.</param>
    /// <param name="displayName">The display name for the playlist.</param>
    /// <param name="songIDs">IDs of songs to include in the playlist.</param>
    /// <returns>True if the playlist was added; false if the ID already exists.</returns>
    public bool PlaylistAdd(uint id, string displayName, params uint[] songIDs)
    {
        if (playlists.ContainsKey(id)) return false;
        IdCounter.AdvanceTo(id);
        Playlist playlist = new(id, displayName, songIDs.ToHashSet());
        playlist.RequestNextSong += OnPlaylistRequestSong;
        playlist.SongStarted += OnPlaylistSongStarted;
        playlists.Add(id, playlist);
        return true;
    }

    /// <summary>
    /// Adds a new playlist and returns its generated unique ID.
    /// </summary>
    /// <param name="displayName">The display name for the playlist.</param>
    /// <param name="songIDs">IDs of songs to include in the playlist.</param>
    /// <returns>The generated playlist ID.</returns>
    public uint PlaylistAdd(string displayName, params uint[] songIDs)
    {
        var id = IdCounter.NextId;
        if (playlists.ContainsKey(id))
        {
            var maxId = playlists.Keys.Max();
            IdCounter.AdvanceTo(maxId);
            id = IdCounter.NextId;
        }
        
        Playlist playlist = new(id, displayName, songIDs.ToHashSet());
        playlist.RequestNextSong += OnPlaylistRequestSong;
        playlist.SongStarted += OnPlaylistSongStarted;
        playlists.Add(id, playlist);
        return id;
    }
    
    /// <summary>
    /// Starts playback of the current playlist, if one is selected.
    /// Triggers the <see cref="PlaylistStarted"/> event.
    /// </summary>
    /// <returns>True if a playlist was started; otherwise, false.</returns>
    public bool PlaylistStart()
    {
        if (currentPlaylist == null) return false;
        currentPlaylist.Start();
        PlaylistStarted?.Invoke(currentPlaylist.DisplayName);
        
        return true;
    }

    /// <summary>
    /// Stops playback of the current playlist, if one is selected.
    /// </summary>
    /// <returns>True if a playlist was stopped; otherwise, false.</returns>
    public bool PlaylistStop()
    {
        if (currentPlaylist == null) return false;
        currentPlaylist.Stop();
        
        return true;
    }

    /// <summary>
    /// Pauses playback of the current playlist, if one is selected.
    /// </summary>
    /// <returns>True if a playlist was paused; otherwise, false.</returns>
    public bool PlaylistPause()
    {
        if (currentPlaylist == null) return false;
        currentPlaylist.Pause();
        
        return true;
    }

    /// <summary>
    /// Resumes playback of the current playlist, if one is selected.
    /// </summary>
    /// <returns>True if a playlist was resumed; otherwise, false.</returns>
    public bool PlaylistResume()
    {
        if (currentPlaylist == null) return false;
        currentPlaylist.Resume();
        
        return true;
    }

    /// <summary>
    /// Switches playback to a different playlist by ID, stopping the current playlist if necessary.
    /// Triggers the <see cref="PlaylistStarted"/> event.
    /// </summary>
    /// <param name="id">The ID of the playlist to switch to.</param>
    /// <returns>True if the switch was successful; otherwise, false.</returns>
    public bool PlaylistSwitch(uint id)
    {
        if (!playlists.ContainsKey(id)) return false;
        
        if(currentPlaylist == null) currentPlaylist = playlists[id];
        else
        {
            currentPlaylist.Stop();
            currentPlaylist = playlists[id];
        }
        currentPlaylist.Start();
        PlaylistStarted?.Invoke(currentPlaylist.DisplayName);
        return true;
    }

    /// <summary>
    /// Gets the display name of the current playlist, or an empty string if none is selected.
    /// </summary>
    public string PlaylistGetName() { return currentPlaylist != null ? currentPlaylist.DisplayName : ""; }

    /// <summary>
    /// Gets the display name of the current song in the playlist, or an empty string if none is playing.
    /// </summary>
    public string PlaylistGetSongName()
    {
        if (currentPlaylist == null) return "";
        if (currentPlaylist.CurrentSong == null) return "";
        return currentPlaylist.CurrentSong.DisplayName;
    }

    /// <summary>
    /// Gets the playback progress of the current song in the playlist as a percentage (0.0 to 1.0).
    /// Returns -1 if no playlist or song is active.
    /// </summary>
    public float PlaylistGetSongPercentage()
    {
        if (currentPlaylist == null) return -1f;
        if (currentPlaylist.CurrentSong == null) return -1f;
        return currentPlaylist.CurrentSong.GetPercentage();
    }
    #endregion
    
    #region Bus

    /// <summary>
    /// Adds a new audio bus with a specific ID and initial volume.
    /// </summary>
    /// <param name="busId">The unique bus ID.</param>
    /// <param name="volume">Initial volume for the bus (0.0 to 1.0).</param>
    /// <returns>True if the bus was added; false if the ID already exists.</returns>
    public bool BusAdd(uint busId, float volume)
    {
        if (buses.ContainsKey(busId)) return false;
        var bus = new Bus(busId, volume);
        buses.Add(busId, bus);
        return true;
    }

    /// <summary>
    /// Adds a new audio bus and returns its generated unique ID.
    /// </summary>
    /// <param name="volume">Initial volume for the bus (default: 1.0).</param>
    /// <returns>The generated bus ID.</returns>
    public uint BusAdd(float volume = 1f)
    {
        var id = IdCounter.NextId;
        if (buses.ContainsKey(id))
        {
            var maxId = buses.Keys.Max();
            IdCounter.AdvanceTo(maxId);
            id = IdCounter.NextId;
        }
        var bus = new Bus(id, volume);
        buses.Add(id, bus);
        return id;
    }

    /// <summary>
    /// Sets the volume of a specific bus.
    /// </summary>
    /// <param name="busId">The bus ID.</param>
    /// <param name="volume">The new volume (0.0 to 1.0).</param>
    /// <returns>True if the volume was set; otherwise, false.</returns>
    public bool BusSetVolume(uint busId, float volume)
    {
        if (!buses.TryGetValue(busId, out var bus)) return false;
        volume = ShapeMath.Clamp(volume, 0.0f, 1.0f);
        bus.Volume = volume;
        
        return true;

    }

    /// <summary>
    /// Adjusts the volume of a specific bus by a given amount.
    /// </summary>
    /// <param name="busId">The bus ID.</param>
    /// <param name="amount">The amount to change the volume by (can be negative).</param>
    /// <returns>True if the volume was changed; otherwise, false.</returns>
    public bool BusChangeVolume(uint busId, float amount)
    {
        if (!buses.TryGetValue(busId, out var bus)) return false;
        float newVolume = ShapeMath.Clamp(bus.Volume + amount, 0.0f, 1.0f);
        buses[busId].Volume = newVolume;
        
        return true;
    }

    /// <summary>
    /// Gets the current volume of a specific bus.
    /// </summary>
    /// <param name="busId">The bus ID.</param>
    /// <returns>The bus volume (0.0 to 1.0), or 1.0 if the bus does not exist.</returns>
    public float BusGetVolume(uint busId)
    {
        if (!buses.TryGetValue(busId, out var bus)) return 1.0f;
        return bus.Volume;
    }

    /// <summary>
    /// Stops all audio routed through a specific bus.
    /// </summary>
    /// <param name="busId">The bus ID.</param>
    /// <returns>True if the bus was found and stopped; otherwise, false.</returns>
    public bool BusStop(uint busId)
    {
        if (!buses.TryGetValue(busId, out var bus)) return false;
        bus.Stop();
        
        return true;
    }
    #endregion

    #region Sound

    /// <summary>
    /// Adds a new sound effect (SFX) to a specific bus and returns its generated unique ID.
    /// </summary>
    /// <param name="sound">The sound resource to add.</param>
    /// <param name="busId">The bus ID to assign the SFX to.</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <returns>The generated SFX ID.</returns>
    public uint SFXAdd(Sound sound, uint busId, float volume = 0.5f, float pitch = 1.0f)
    {
        var id = IdCounter.NextId;
        if (sounds.ContainsKey(id))
        {
            var maxId = sounds.Keys.Max();
            IdCounter.AdvanceTo(maxId);
            id = IdCounter.NextId;
        }
        List<Bus> b = [];
        if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        var sfx = new SFX(id, sound, b.ToArray(), volume, pitch);

        sounds.Add(id, sfx);

        return id;
    }

    /// <summary>
    /// Adds a new sound effect (SFX) to multiple buses and returns its generated unique ID.
    /// </summary>
    /// <param name="sound">The sound resource to add.</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <param name="busIDs">IDs of buses to assign the SFX to.</param>
    /// <returns>The generated SFX ID.</returns>
    public uint SFXAdd(Sound sound, float volume = 0.5f, float pitch = 1.0f, params uint[] busIDs)
    {
        var id = IdCounter.NextId;
        if (sounds.ContainsKey(id))
        {
            var maxId = sounds.Keys.Max();
            IdCounter.AdvanceTo(maxId);
            id = IdCounter.NextId;
        }
        List<Bus> b = [];
        foreach (uint busId in busIDs)
        {
            if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        }
        var sfx = new SFX(id, sound, b.ToArray(), volume, pitch);

        sounds.Add(id, sfx);

        return id;
    }

    /// <summary>
    /// Adds a new sound effect (SFX) with a specific ID to a bus.
    /// </summary>
    /// <param name="id">The unique SFX ID.</param>
    /// <param name="sound">The sound resource to add.</param>
    /// <param name="busId">The bus ID to assign the SFX to.</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <returns>True if the SFX was added; otherwise, false.</returns>
    public bool SFXAdd(uint id, Sound sound, uint busId, float volume = 0.5f, float pitch = 1.0f)
    {
        if (sounds.ContainsKey(id)) return false;
        List<Bus> b = [];
        if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        var sfx = new SFX(id, sound, b.ToArray(), volume, pitch);

        sounds.Add(id, sfx);

        return true;
    }

    /// <summary>
    /// Adds a new sound effect (SFX) with a specific ID to multiple buses.
    /// </summary>
    /// <param name="id">The unique SFX ID.</param>
    /// <param name="sound">The sound resource to add.</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <param name="busIDs">IDs of buses to assign the SFX to.</param>
    /// <returns>True if the SFX was added; otherwise, false.</returns>
    public bool SFXAdd(uint id, Sound sound, float volume = 0.5f, float pitch = 1.0f, params uint[] busIDs)
    {
        if (sounds.ContainsKey(id)) return false;
        List<Bus> b = [];
        foreach (uint busId in busIDs)
        {
            if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        }
        var sfx = new SFX(id, sound, b.ToArray(), volume, pitch);

        sounds.Add(id, sfx);

        return true;
    }
    
    /// <summary>
    /// Adds a new looping sound effect (SFXLoop) to multiple buses and returns its generated unique ID.
    /// </summary>
    /// <param name="sound">The sound resource to loop.</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <param name="busIDs">IDs of buses to assign the loop to.</param>
    /// <returns>The generated SFXLoop ID.</returns>
    public uint SFXLoopAdd(Sound sound, float volume = 0.5f, float pitch = 1.0f, params uint[] busIDs)
    {
        var id = IdCounter.NextId;
        if (sounds.ContainsKey(id))
        {
            var maxId = sounds.Keys.Max();
            IdCounter.AdvanceTo(maxId);
            id = IdCounter.NextId;
        }
        List<Bus> b = new();
        foreach (var busID in busIDs)
        {
            if (buses.ContainsKey(busID)) b.Add(buses[busID]);
        }
        SFXLoop loop = new SFXLoop(id, sound, b.ToArray(), volume, pitch);

        loops.Add(id, loop);
        return id;
    }

    /// <summary>
    /// Adds a new spatial looping sound effect (SFXLoop) to multiple buses and returns its generated unique ID.
    /// </summary>
    /// <param name="sound">The sound resource to loop.</param>
    /// <param name="minSpatialRange">Minimum distance for full volume.</param>
    /// <param name="maxSpatialRange">Maximum distance for attenuation (beyond this, sound is inaudible).</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <param name="busIDs">IDs of buses to assign the loop to.</param>
    /// <returns>The generated SFXLoop ID.</returns>
    public uint SFXLoopAdd(Sound sound, float minSpatialRange, float maxSpatialRange, float volume = 0.5f, float pitch = 1.0f, params uint[] busIDs)
    {
        var id = IdCounter.NextId;
        if (sounds.ContainsKey(id))
        {
            var maxId = sounds.Keys.Max();
            IdCounter.AdvanceTo(maxId);
            id = IdCounter.NextId;
        }
        List<Bus> b = new();
        foreach (var busID in busIDs)
        {
            if (buses.ContainsKey(busID)) b.Add(buses[busID]);
        }
        SFXLoop loop = new SFXLoop(id, sound, minSpatialRange, maxSpatialRange, b.ToArray(), volume, pitch);

        loops.Add(id, loop);
        return id;
    }

    /// <summary>
    /// Adds a new looping sound effect (SFXLoop) to a specific bus and returns its generated unique ID.
    /// </summary>
    /// <param name="sound">The sound resource to loop.</param>
    /// <param name="busId">The bus ID to assign the loop to.</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <returns>The generated SFXLoop ID.</returns>
    public uint SFXLoopAdd(Sound sound, uint busId, float volume = 0.5f, float pitch = 1.0f)
    {
        var id = IdCounter.NextId;
        if (sounds.ContainsKey(id))
        {
            var maxId = sounds.Keys.Max();
            IdCounter.AdvanceTo(maxId);
            id = IdCounter.NextId;
        }
        List<Bus> b = [];
        if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        SFXLoop loop = new SFXLoop(id, sound, b.ToArray(), volume, pitch);

        loops.Add(id, loop);

        return id;
    }
    
    /// <summary>
    /// Adds a new spatial looping sound effect (SFXLoop) to a specific bus and returns its generated unique ID.
    /// </summary>
    /// <param name="sound">The sound resource to loop.</param>
    /// <param name="busId">The bus ID to assign the loop to.</param>
    /// <param name="minSpatialRange">Minimum distance for full volume.</param>
    /// <param name="maxSpatialRange">Maximum distance for attenuation (beyond this, sound is inaudible).</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <returns>The generated SFXLoop ID.</returns>
    public uint SFXLoopAdd(Sound sound, uint busId, float minSpatialRange, float maxSpatialRange, float volume, float pitch)
    {
        var id = IdCounter.NextId;
        if (sounds.ContainsKey(id))
        {
            var maxId = sounds.Keys.Max();
            IdCounter.AdvanceTo(maxId);
            id = IdCounter.NextId;
        }
        List<Bus> b = [];
        if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        SFXLoop loop = new SFXLoop(id, sound, minSpatialRange, maxSpatialRange, b.ToArray(), volume, pitch);

        loops.Add(id, loop);

        return id;
    }

    /// <summary>
    /// Adds a new looping sound effect (SFXLoop) with a specific ID to a bus.
    /// </summary>
    /// <param name="id">The unique SFXLoop ID.</param>
    /// <param name="sound">The sound resource to loop.</param>
    /// <param name="busId">The bus ID to assign the loop to.</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <returns>True if the SFXLoop was added; otherwise, false.</returns>
    public bool SFXLoopAdd(uint id, Sound sound, uint busId, float volume = 0.5f, float pitch = 1.0f)
    {
        if (sounds.ContainsKey(id)) return false;
        List<Bus> b = [];
        if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        SFXLoop loop = new SFXLoop(id, sound, b.ToArray(), volume, pitch);

        loops.Add(id, loop);

        return true;
    }

    /// <summary>
    /// Adds a new spatial looping sound effect (SFXLoop) with a specific ID to a bus.
    /// </summary>
    /// <param name="id">The unique SFXLoop ID.</param>
    /// <param name="sound">The sound resource to loop.</param>
    /// <param name="busId">The bus ID to assign the loop to.</param>
    /// <param name="minSpatialRange">Minimum distance for full volume.</param>
    /// <param name="maxSpatialRange">Maximum distance for attenuation (beyond this, sound is inaudible).</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <returns>True if the SFXLoop was added; otherwise, false.</returns>
    public bool SFXLoopAdd(uint id, Sound sound, uint busId, float minSpatialRange, float maxSpatialRange, float volume, float pitch)
    {
        if (sounds.ContainsKey(id)) return false;
        List<Bus> b = [];
        if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        SFXLoop loop = new SFXLoop(id, sound, minSpatialRange, maxSpatialRange, b.ToArray(), volume, pitch);

        loops.Add(id, loop);

        return true;
    }
    
    /// <summary>
    /// Adds a new looping sound effect (SFXLoop) with a specific ID to multiple buses.
    /// </summary>
    /// <param name="id">The unique SFXLoop ID.</param>
    /// <param name="sound">The sound resource to loop.</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <param name="busIDs">IDs of buses to assign the loop to.</param>
    /// <returns>True if the SFXLoop was added; otherwise, false.</returns>
    public bool SFXLoopAdd(uint id, Sound sound, float volume = 0.5f, float pitch = 1.0f, params uint[] busIDs)
    {
        if (sounds.ContainsKey(id)) return false;
        List<Bus> b = [];
        foreach (uint busId in busIDs)
        {
            if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        }
        var loop = new SFXLoop(id, sound, b.ToArray(), volume, pitch);

        loops.Add(id, loop);

        return true;
    }

    /// <summary>
    /// Adds a new spatial looping sound effect (SFXLoop) with a specific ID to multiple buses.
    /// </summary>
    /// <param name="id">The unique SFXLoop ID.</param>
    /// <param name="sound">The sound resource to loop.</param>
    /// <param name="minSpatialRange">Minimum distance for full volume.</param>
    /// <param name="maxSpatialRange">Maximum distance for attenuation (beyond this, sound is inaudible).</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <param name="busIDs">IDs of buses to assign the loop to.</param>
    /// <returns>True if the SFXLoop was added; otherwise, false.</returns>
    public bool SFXLoopAdd(uint id, Sound sound, float minSpatialRange, float maxSpatialRange, float volume = 0.5f, float pitch = 1.0f, params uint[] busIDs)
    {
        if (sounds.ContainsKey(id)) return false;
        List<Bus> b = [];
        foreach (var busId in busIDs)
        {
            if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        }
        var loop = new SFXLoop(id, sound, minSpatialRange, maxSpatialRange, b.ToArray(), volume, pitch);

        loops.Add(id, loop);

        return true;
    }
    
    /// <summary>
    /// Adds a new song to a specific bus and returns its generated unique ID.
    /// </summary>
    /// <param name="song">The music resource to add.</param>
    /// <param name="busId">The bus ID to assign the song to.</param>
    /// <param name="displayName">The display name for the song.</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <returns>The generated song ID.</returns>
    public uint SongAdd(Music song, uint busId, string displayName, float volume = 0.5f, float pitch = 1.0f)
    {
        var id = IdCounter.NextId;
        if (songs.ContainsKey(id))
        {
            var maxId = songs.Keys.Max();
            IdCounter.AdvanceTo(maxId);
            id = IdCounter.NextId;
        }
        List<Bus> b = [];
        if (buses.TryGetValue(busId, out var bus)) b.Add(bus);

        var s = new Song(id, displayName, song, b.ToArray(), volume, pitch);

        songs.Add(id, s);

        return id;
    }

    /// <summary>
    /// Adds a new song to multiple buses and returns its generated unique ID.
    /// </summary>
    /// <param name="song">The music resource to add.</param>
    /// <param name="displayName">The display name for the song.</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <param name="busIDs">IDs of buses to assign the song to.</param>
    /// <returns>The generated song ID.</returns>
    public uint SongAdd(Music song, string displayName, float volume = 0.5f, float pitch = 1.0f, params uint[] busIDs)
    {
        var id = IdCounter.NextId;
        if (songs.ContainsKey(id))
        {
            var maxId = songs.Keys.Max();
            IdCounter.AdvanceTo(maxId);
            id = IdCounter.NextId;
        }
        List<Bus> b = [];
        foreach (var busId in busIDs)
        {
            if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        }

        var s = new Song(id, displayName, song, b.ToArray(), volume, pitch);

        songs.Add(id, s);

        return id;
    }
    
    /// <summary>
    /// Adds a new song with a specific ID to a bus.
    /// </summary>
    /// <param name="id">The unique song ID.</param>
    /// <param name="song">The music resource to add.</param>
    /// <param name="busId">The bus ID to assign the song to.</param>
    /// <param name="displayName">The display name for the song.</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <returns>True if the song was added; otherwise, false.</returns>
    public bool SongAdd(uint id, Music song, uint busId, string displayName, float volume = 0.5f, float pitch = 1.0f)
    {
        if(songs.ContainsKey(id)) return false;
        List<Bus> b = [];
        if (buses.TryGetValue(busId, out var bus)) b.Add(bus);

        var s = new Song(id, displayName, song, b.ToArray(), volume, pitch);

        songs.Add(id, s);

        return true;
    }

    /// <summary>
    /// Adds a new song with a specific ID to multiple buses.
    /// </summary>
    /// <param name="id">The unique song ID.</param>
    /// <param name="song">The music resource to add.</param>
    /// <param name="displayName">The display name for the song.</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <param name="busIDs">IDs of buses to assign the song to.</param>
    /// <returns>True if the song was added; otherwise, false.</returns>
    public bool SongAdd(uint id, Music song, string displayName, float volume = 0.5f, float pitch = 1.0f, params uint[] busIDs)
    {
        if(songs.ContainsKey(id)) return false;
        List<Bus> b = [];
        foreach (var busId in busIDs)
        {
            if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        }

        var s = new Song(id, displayName, song, b.ToArray(), volume, pitch);

        songs.Add(id, s);

        return true;
    }
    

    /// <summary>
    /// Plays a sound effect (SFX) by ID, with optional volume, pitch, and cooldown blocking.
    /// </summary>
    /// <param name="id">The SFX ID.</param>
    /// <param name="volume">Playback volume multiplier (default: 1.0f).</param>
    /// <param name="pitch">Playback pitch multiplier (default: 1.0f).</param>
    /// <param name="blockDuration">Optional cooldown in seconds before this SFX can play again (default: 0f).</param>
    /// <returns>True if the SFX was played; otherwise, false.</returns>
    public bool SFXPlay(uint id, float volume = 1f, float pitch = 1f, float blockDuration = 0f)
    {
        if (!sounds.ContainsKey(id)) return false;
        if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return false;
        if (blockDuration > 0f)
        {
            soundBlockers[id] = blockDuration;
        }

        sounds[id].Play(volume, pitch);
        return true;
    }

    /// <summary>
    /// Plays a sound effect (SFX) at a specific world position if inside the camera area.
    /// </summary>
    /// <param name="id">The SFX ID.</param>
    /// <param name="pos">World position to play the sound at.</param>
    /// <param name="volume">Playback volume multiplier (default: 1.0f).</param>
    /// <param name="pitch">Playback pitch multiplier (default: 1.0f).</param>
    /// <param name="blockDuration">Optional cooldown in seconds before this SFX can play again (default: 0f).</param>
    /// <returns>True if the SFX was played; otherwise, false.</returns>
    public bool SFXPlay(uint id, Vector2 pos, float volume = 1.0f, float pitch = 1.0f, float blockDuration = 0f)
    {
        if (!sounds.ContainsKey(id)) return false;
        if (!Raylib.CheckCollisionPointRec(pos, cameraRect.Rectangle)) return false;

        if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return false;

        if (blockDuration > 0f)
        {
            soundBlockers[id] = blockDuration;
        }
        sounds[id].Play(volume, pitch);
        
        return true;
    }

    /// <summary>
    /// Plays a sound effect (SFX) at a given position with spatial volume attenuation.
    /// </summary>
    /// <param name="id">The SFX ID.</param>
    /// <param name="pos">World position to play the sound at.</param>
    /// <param name="minRange">Minimum distance for full volume.</param>
    /// <param name="maxRange">Maximum distance for attenuation (beyond this, sound is inaudible).</param>
    /// <param name="volume">Base volume (default: 0.5f).</param>
    /// <param name="pitch">Playback pitch (default: 1.0f).</param>
    /// <param name="blockDuration">Optional cooldown in seconds before this SFX can play again.</param>
    /// <returns>True if the SFX was played; otherwise, false.</returns>
    public bool SFXPlay(uint id, Vector2 pos, float minRange, float maxRange, float volume, float pitch, float blockDuration)
    {
        if (!sounds.ContainsKey(id)) return false;
        if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return false;
        

        Vector2 center;
        if (SpatialTargetOverride == null) center = cameraRect.Center;// GraphicsDevice.CAMERA.RawPos;
        else center = SpatialTargetOverride.Transform.Position;

        float disSq = (center - pos).LengthSquared();
        
        
        if (minRange < 0f) minRange = 0f;
        if (maxRange < 0f || maxRange <= minRange) maxRange = minRange + 1;
        float minSquared = minRange * minRange;
        float maxSquared = maxRange * maxRange;
        if (disSq >= maxSquared) return false;

        if (blockDuration > 0f)
        {
            soundBlockers[id] = blockDuration;
        }

        var spatialVolumeFactor = 1f;
        if(disSq > minSquared)
        {
            spatialVolumeFactor = 1f - ShapeMath.LerpInverseFloat(minSquared, maxSquared, disSq);
        }

        sounds[id].Play(volume * spatialVolumeFactor, pitch);
        
        return true;
    }

    /// <summary>
    /// Plays a looping sound effect (SFXLoop) by ID, with optional volume, pitch, and cooldown blocking.
    /// </summary>
    /// <param name="id">The SFXLoop ID.</param>
    /// <param name="volume">Playback volume multiplier (default: 1.0f).</param>
    /// <param name="pitch">Playback pitch multiplier (default: 1.0f).</param>
    /// <param name="blockDuration">Optional cooldown in seconds before this SFXLoop can play again (default: 0f).</param>
    /// <returns>True if the SFXLoop was played; otherwise, false.</returns>
    public bool SFXLoopPlay(uint id, float volume = 1f, float pitch = 1f, float blockDuration = 0f)
    {
        if (!loops.ContainsKey(id)) return false;
        if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return false;
        if (blockDuration > 0f)
        {
            soundBlockers[id] = blockDuration;
        }

        loops[id].Play(volume, pitch);
        
        return true;
    }

    /// <summary>
    /// Plays a looping sound effect (SFXLoop) at a specific world position.
    /// </summary>
    /// <param name="id">The SFXLoop ID.</param>
    /// <param name="pos">World position to play the loop at.</param>
    /// <param name="volume">Playback volume multiplier (default: 1.0f).</param>
    /// <param name="pitch">Playback pitch multiplier (default: 1.0f).</param>
    /// <param name="blockDuration">Optional cooldown in seconds before this SFXLoop can play again (default: 0f).</param>
    /// <returns>True if the SFXLoop was played; otherwise, false.</returns>
    public bool SFXLoopPlay(uint id, Vector2 pos, float volume = 1f, float pitch = 1f, float blockDuration = 0f)
    {
        if (!loops.ContainsKey(id)) return false;
        if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return false;
        if (blockDuration > 0f)
        {
            soundBlockers[id] = blockDuration;
        }
        loops[id].SpatialPos = pos;
        loops[id].Play(volume, pitch);
        
        return true;
    }
    
    /// <summary>
    /// Sets the spatial position of a looping sound effect (SFXLoop) by ID. (Deprecated)
    /// </summary>
    /// <param name="id">The SFXLoop ID.</param>
    /// <param name="pos">The new spatial position.</param>
    public void SFXLoopUpdateSpatialPos(uint id, Vector2 pos)
    {
        if(loops.TryGetValue(id, out var loop)) loop.SpatialPos = pos;
    }

    /// <summary>
    /// Stops a looping sound effect (SFXLoop) by ID.
    /// </summary>
    /// <param name="id">The SFXLoop ID.</param>
    /// <returns>True if the SFXLoop was stopped; otherwise, false.</returns>
    public bool SFXLoopStop(uint id)
    {
        if (!loops.TryGetValue(id, out var loop)) return false;
        loop.Stop();
        
        return true;
    }
    #endregion
    
}