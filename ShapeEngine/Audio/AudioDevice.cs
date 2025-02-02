using Raylib_cs;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Audio;

    
public class AudioDevice
{
    #region Members
    
    private static bool initialized = false;
    
    public readonly IdCounter IdCounter = new();
    public event Action<string>? PlaylistStarted;
    public event Action<string, string>? PlaylistSongStarted;
    public static readonly uint BUS_MASTER = ShapeID.NextID;

    private readonly Dictionary<uint, Bus> buses = new();
    private readonly Dictionary<uint, SFX> sounds = new();
    private readonly Dictionary<uint, SFXLoop> loops = new();
    private readonly Dictionary<uint, Song> songs = new();
    
    private Playlist? currentPlaylist = null;
    private readonly Dictionary<uint, Playlist> playlists = new();
    private readonly Dictionary<uint, float> soundBlockers = new();
    public  GameObject? SpatialTargetOverride { get; set; } = null;

    private Rect cameraRect = new Rect();

    #endregion
    
    #region Constructors
    
    public AudioDevice()
    {
        if (!initialized)
        {
            Raylib.InitAudioDevice();
            initialized = true;
        }
        BusAdd(BUS_MASTER, 1f);
    }
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
    public void Update(float dt, ShapeCamera camera)// Vector2 cameraPos)
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
    private void OnPlaylistRequestSong(uint id, Playlist playlist)
    {
        if (songs.ContainsKey(id)) playlist.DeliverNextSong(songs[id]);
    }
    private void OnPlaylistSongStarted(string songName, string playlistName)
    {
        PlaylistSongStarted?.Invoke(songName, playlistName);
    }
    
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
    
    public bool PlaylistStart()
    {
        if (currentPlaylist == null) return false;
        currentPlaylist.Start();
        PlaylistStarted?.Invoke(currentPlaylist.DisplayName);
        
        return true;
    }
    public bool PlaylistStop()
    {
        if (currentPlaylist == null) return false;
        currentPlaylist.Stop();
        
        return true;
    }
    public bool PlaylistPause()
    {
        if (currentPlaylist == null) return false;
        currentPlaylist.Pause();
        
        return true;
    }
    public bool PlaylistResume()
    {
        if (currentPlaylist == null) return false;
        currentPlaylist.Resume();
        
        return true;
    }
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
    public string PlaylistGetName() { return currentPlaylist != null ? currentPlaylist.DisplayName : ""; }
    public string PlaylistGetSongName()
    {
        if (currentPlaylist == null) return "";
        if (currentPlaylist.CurrentSong == null) return "";
        return currentPlaylist.CurrentSong.DisplayName;
    }
    public float PlaylistGetSongPercentage()
    {
        if (currentPlaylist == null) return -1f;
        if (currentPlaylist.CurrentSong == null) return -1f;
        return currentPlaylist.CurrentSong.GetPercentage();
    }
    #endregion
    
    #region Bus
    public bool BusAdd(uint busId, float volume)
    {
        if (buses.ContainsKey(busId)) return false;
        var bus = new Bus(busId, volume);
        buses.Add(busId, bus);
        return true;
    }
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
    public bool BusSetVolume(uint busId, float volume)
    {
        if (!buses.TryGetValue(busId, out var bus)) return false;
        volume = ShapeMath.Clamp(volume, 0.0f, 1.0f);
        bus.Volume = volume;
        
        return true;

    }
    public bool BusChangeVolume(uint busId, float amount)
    {
        if (!buses.TryGetValue(busId, out var bus)) return false;
        float newVolume = ShapeMath.Clamp(bus.Volume + amount, 0.0f, 1.0f);
        buses[busId].Volume = newVolume;
        
        return true;
    }
    public float BusGetVolume(uint busId)
    {
        if (!buses.TryGetValue(busId, out var bus)) return 1.0f;
        return bus.Volume;
    }
    public bool BusStop(uint busId)
    {
        if (!buses.TryGetValue(busId, out var bus)) return false;
        bus.Stop();
        
        return true;
    }
    #endregion

    #region Sound
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
    public bool SFXAdd(uint id, Sound sound, uint busId, float volume = 0.5f, float pitch = 1.0f)
    {
        if (sounds.ContainsKey(id)) return false;
        List<Bus> b = [];
        if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        var sfx = new SFX(id, sound, b.ToArray(), volume, pitch);

        sounds.Add(id, sfx);

        return true;
    }
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
    public uint SFXLoopAdd(Sound sound, uint busId, float minSpatialRange, float maxSpatialRange, float volume = 0.5f, float pitch = 1.0f)
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

    
    public bool SFXLoopAdd(uint id, Sound sound, uint busId, float volume = 0.5f, float pitch = 1.0f)
    {
        if (sounds.ContainsKey(id)) return false;
        List<Bus> b = [];
        if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        SFXLoop loop = new SFXLoop(id, sound, b.ToArray(), volume, pitch);

        loops.Add(id, loop);

        return true;
    }
    public bool SFXLoopAdd(uint id, Sound sound, uint busId, float minSpatialRange, float maxSpatialRange, float volume = 0.5f, float pitch = 1.0f)
    {
        if (sounds.ContainsKey(id)) return false;
        List<Bus> b = [];
        if (buses.TryGetValue(busId, out var bus)) b.Add(bus);
        SFXLoop loop = new SFXLoop(id, sound, minSpatialRange, maxSpatialRange, b.ToArray(), volume, pitch);

        loops.Add(id, loop);

        return true;
    }
    
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
    
    public bool SongAdd(uint id, Music song, uint busId, string displayName, float volume = 0.5f, float pitch = 1.0f)
    {
        if(songs.ContainsKey(id)) return false;
        List<Bus> b = [];
        if (buses.TryGetValue(busId, out var bus)) b.Add(bus);

        var s = new Song(id, displayName, song, b.ToArray(), volume, pitch);

        songs.Add(id, s);

        return true;
    }
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
    /// Play a sound. If pos is not inside the current camera area the sound is NOT played.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pos"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <param name="blockDuration"></param>
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
    /// Play the sound. If the pos is less than minRange from the current pos of the camera (or the spatial target override) the sound is played with full volume.
    /// If the pos is further away than minRange but less than maxRange from the pos of the camera the volume is linearly interpolated.
    /// If the pos is futher aways than maxRange the sound is not played.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pos"></param>
    /// <param name="minRange"></param>
    /// <param name="maxRange"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <param name="blockDuration"></param>
    public bool SFXPlay(uint id, Vector2 pos, float minRange, float maxRange, float volume = 1.0f, float pitch = 1.0f, float blockDuration = 0f)
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
    /// Deprecated. Do not use, will be removed in a future updated!
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pos"></param>
    public void SFXLoopUpdateSpatialPos(uint id, Vector2 pos)
    {
        if(loops.TryGetValue(id, out var loop)) loop.SpatialPos = pos;
    }
    public bool SFXLoopStop(uint id)
    {
        if (!loops.TryGetValue(id, out var loop)) return false;
        loop.Stop();
        
        return true;
    }
    #endregion
    
}

