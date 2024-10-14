using Raylib_cs;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Audio;

    
public class AudioDevice
{
    #region Members
    
    private static bool initialized = false;
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
        //if(currentSong != null) UpdateMusicStream(currentSong.GetSong());
        //if (currentSong != null) currentSong.Update(dt);

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
            if (SpatialTargetOverride == null) center = cameraRect.Center;// cameraPos; // ScreenHandler.CAMERA.RawPos;
            else center = SpatialTargetOverride.Transform.Position;
            //if (SpatialTargetOverride != null) loop.SpatialPos = SpatialTargetOverride.GetPosition();
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
    public void PlaylistAdd(uint id, string displayName, params uint[] songIDs)
    {
        if (playlists.ContainsKey(id)) return;
        Playlist playlist = new(id, displayName, songIDs.ToHashSet());
        playlist.RequestNextSong += OnPlaylistRequestSong;
        playlist.SongStarted += OnPlaylistSongStarted;
        playlists.Add(id, playlist);
    }
    public void PlaylistStart()
    {
        if (currentPlaylist == null) return;
        currentPlaylist.Start();
        PlaylistStarted?.Invoke(currentPlaylist.DisplayName);
    }
    public void PlaylistStop()
    {
        if (currentPlaylist == null) return;
        currentPlaylist.Stop();
    }
    public void PlaylistPause()
    {
        if (currentPlaylist == null) return;
        currentPlaylist.Pause();
    }
    public void PlaylistResume()
    {
        if (currentPlaylist == null) return;
        currentPlaylist.Resume();
    }
    public void PlaylistSwitch(uint id)
    {
        if (!playlists.ContainsKey(id)) return;
        
        if(currentPlaylist == null) currentPlaylist = playlists[id];
        else
        {
            currentPlaylist.Stop();
            currentPlaylist = playlists[id];
        }
        currentPlaylist.Start();
        PlaylistStarted?.Invoke(currentPlaylist.DisplayName);
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
    public void BusAdd(uint busID, float volume)
    {
        if (buses.ContainsKey(busID)) return;
        Bus bus = new Bus(busID, volume);
        buses.Add(busID, bus);
    }
    public void BusSetVolume(uint busID, float volume)
    {
        if (!buses.ContainsKey(busID)) return;
        volume = ShapeMath.Clamp(volume, 0.0f, 1.0f);
        buses[busID].Volume = volume;

    }
    public void BusChangeVolume(uint busID, float amount)
    {
        if (!buses.ContainsKey(busID)) return;
        float newVolume = ShapeMath.Clamp(buses[busID].Volume + amount, 0.0f, 1.0f);
        buses[busID].Volume = newVolume;
    }
    public float BusGetVolume(uint busID)
    {
        if (!buses.ContainsKey(busID)) return 1.0f;
        return buses[busID].Volume;
    }
    public void BusStop(uint busID)
    {
        if (!buses.ContainsKey(busID)) return;
        buses[busID].Stop();
    }
    #endregion

    #region Sound
    public void SFXAdd(uint id, Sound sound, float volume = 0.5f, float pitch = 1.0f, params uint[] busIDs)
    {
        if (sounds.ContainsKey(id)) return;
        List<Bus> b = new();
        foreach (var busID in busIDs)
        {
            if (buses.ContainsKey(busID)) b.Add(buses[busID]);
        }
        SFX sfx = new SFX(id, sound, b.ToArray(), volume, pitch);

        sounds.Add(id, sfx);
    }
    public void SFXLoopAdd(uint id, Sound sound, float volume = 0.5f, float pitch = 1.0f, params uint[] busIDs)
    {
        if (sounds.ContainsKey(id)) return;
        List<Bus> b = new();
        foreach (var busID in busIDs)
        {
            if (buses.ContainsKey(busID)) b.Add(buses[busID]);
        }
        SFXLoop loop = new SFXLoop(id, sound, b.ToArray(), volume, pitch);

        loops.Add(id, loop);
    }
    public void SFXLoopAdd(uint id, Sound sound, float minSpatialRange, float maxSpatialRange, float volume = 0.5f, float pitch = 1.0f, params uint[] busIDs)
    {
        if (sounds.ContainsKey(id)) return;
        List<Bus> b = new();
        foreach (var busID in busIDs)
        {
            if (buses.ContainsKey(busID)) b.Add(buses[busID]);
        }
        SFXLoop loop = new SFXLoop(id, sound, minSpatialRange, maxSpatialRange, b.ToArray(), volume, pitch);

        loops.Add(id, loop);
    }
    public void SongAdd(uint id, Music song, string displayName, float volume = 0.5f, float pitch = 1.0f, params uint[] busIDs)
    {
        if(songs.ContainsKey(id)) return;
        List<Bus> b = new();
        foreach (var busID in busIDs)
        {
            if (buses.ContainsKey(busID)) b.Add(buses[busID]);
        }

        Song s = new Song(id, displayName, song, b.ToArray(), volume, pitch);

        songs.Add(id, s);
    }
    

    public void SFXPlay(uint id, float volume = 1f, float pitch = 1f, float blockDuration = 0f)
    {
        if(!sounds.ContainsKey(id)) return;
        if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return;
        if (blockDuration > 0f)
        {
            if (!soundBlockers.ContainsKey(id)) soundBlockers.Add(id, blockDuration);
            else soundBlockers[id] = blockDuration;
        }

        sounds[id].Play(volume, pitch);
    }
    /// <summary>
    /// Play a sound. If pos is not inside the current camera area the sound is NOT played.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pos"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <param name="blockDuration"></param>
    public void SFXPlay(uint id, Vector2 pos, float volume = 1.0f, float pitch = 1.0f, float blockDuration = 0f)
    {
        if (!sounds.ContainsKey(id)) return;
        if (!Raylib.CheckCollisionPointRec(pos, cameraRect.Rectangle)) return;

        if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return;

        if (blockDuration > 0f)
        {
            if (!soundBlockers.ContainsKey(id)) soundBlockers.Add(id, blockDuration);
            else soundBlockers[id] = blockDuration;
        }
        sounds[id].Play(volume, pitch);
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
    public void SFXPlay(uint id, Vector2 pos, float minRange, float maxRange, float volume = 1.0f, float pitch = 1.0f, float blockDuration = 0f)
    {
        if (!sounds.ContainsKey(id)) return;
        if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return;
        

        Vector2 center;
        if (SpatialTargetOverride == null) center = cameraRect.Center;// GraphicsDevice.CAMERA.RawPos;
        else center = SpatialTargetOverride.Transform.Position;

        float disSq = (center - pos).LengthSquared();
        
        
        if (minRange < 0f) minRange = 0f;
        if (maxRange < 0f || maxRange <= minRange) maxRange = minRange + 1;
        float minSquared = minRange * minRange;
        float maxSquared = maxRange * maxRange;
        if (disSq >= maxSquared) return;

        if (blockDuration > 0f)
        {
            if (!soundBlockers.ContainsKey(id)) soundBlockers.Add(id, blockDuration);
            else soundBlockers[id] = blockDuration;
        }

        float spatialVolumeFactor = 1f;
        if(disSq > minSquared)
        {
            spatialVolumeFactor = 1f - ShapeMath.LerpInverseFloat(minSquared, maxSquared, disSq);
        }

        sounds[id].Play(volume * spatialVolumeFactor, pitch);
    }

    public void SFXLoopPlay(uint id, float volume = 1f, float pitch = 1f, float blockDuration = 0f)
    {
        if (!loops.ContainsKey(id)) return;
        if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return;
        if (blockDuration > 0f)
        {
            if (!soundBlockers.ContainsKey(id)) soundBlockers.Add(id, blockDuration);
            else soundBlockers[id] = blockDuration;
        }

        loops[id].Play(volume, pitch);
    }
    public void SFXLoopPlay(uint id, Vector2 pos, float volume = 1f, float pitch = 1f, float blockDuration = 0f)
    {
        if (!loops.ContainsKey(id)) return;
        if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return;
        if (blockDuration > 0f)
        {
            if (!soundBlockers.ContainsKey(id)) soundBlockers.Add(id, blockDuration);
            else soundBlockers[id] = blockDuration;
        }
        loops[id].SpatialPos = pos;
        loops[id].Play(volume, pitch);
    }
    public void SFXLoopUpdateSpatialPos(uint id, Vector2 pos)
    {
        if(loops.ContainsKey(id)) loops[id].SpatialPos = pos;
    }
    public void SFXLoopStop(uint id)
    {
        if (!loops.ContainsKey(id)) return;
        loops[id].Stop();
    }
    #endregion
    
}

