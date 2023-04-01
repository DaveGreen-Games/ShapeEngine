using Raylib_CsLo;
using ShapeScreen;
using System.Numerics;
using ShapeCore;
using ShapeLib;

namespace ShapeAudio
{
    public class AudioHandler
    {
        public event Action<string>? PlaylistStarted;
        public event Action<string, string>? PlaylistSongStarted;
        public const int BUS_MASTER = 0;

        private Dictionary<int, Bus> buses = new();
        private Dictionary<int, SFX> sounds = new();
        private Dictionary<int, SFXLoop> loops = new();
        private Dictionary<int, Song> songs = new();
        
        private Dictionary<int, Playlist> playlists = new();
        private Playlist? currentPlaylist = null;
        private Dictionary<int, float> soundBlockers = new();
        public  GameObject? SpatialTargetOverride { get; set; } = null;

        public AudioHandler()
        {
            BusAdd(BUS_MASTER, 1f);
        }
        //MAIN
        //public static void Initialize()
        //{
        //    InitAudioDevice();
        //    BusAdd(BUS_MASTER, 1f);
        //}
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
            CloseAudioDevice();
        }
        public void Update(float dt, Vector2 cameraPos)
        {
            if (currentPlaylist != null) currentPlaylist.Update(dt);
            //if(currentSong != null) UpdateMusicStream(currentSong.GetSong());
            //if (currentSong != null) currentSong.Update(dt);

            if (SpatialTargetOverride != null && SpatialTargetOverride.IsDead()) SpatialTargetOverride = null;

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
                if (SpatialTargetOverride == null) center = cameraPos; // ScreenHandler.CAMERA.RawPos;
                else center = SpatialTargetOverride.GetPosition();
                //if (SpatialTargetOverride != null) loop.SpatialPos = SpatialTargetOverride.GetPosition();
                loop.Update(dt, center);
            }
        }


        //PLAYLISTS
        private void OnPlaylistRequestSong(int id, Playlist playlist)
        {
            if (songs.ContainsKey(id)) playlist.DeliverNextSong(songs[id]);
        }
        private void OnPlaylistSongStarted(string songName, string playlistName)
        {
            PlaylistSongStarted?.Invoke(songName, playlistName);
        }
        public void PlaylistAdd(int id, string displayName, params int[] songIDs)
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
        public void PlaylistSwitch(int id)
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
        
        
        
        //BUS
        public void BusAdd(int busID, float volume)
        {
            if (buses.ContainsKey(busID)) return;
            Bus bus = new Bus(busID, volume);
            buses.Add(busID, bus);
        }
        public void BusSetVolume(int busID, float volume)
        {
            if (!buses.ContainsKey(busID)) return;
            volume = Clamp(volume, 0.0f, 1.0f);
            buses[busID].Volume = volume;

        }
        public void BusChangeVolume(int busID, float amount)
        {
            if (!buses.ContainsKey(busID)) return;
            float newVolume = Clamp(buses[busID].Volume + amount, 0.0f, 1.0f);
            buses[busID].Volume = newVolume;
        }
        public float BusGetVolume(int busID)
        {
            if (!buses.ContainsKey(busID)) return 1.0f;
            return buses[busID].Volume;
        }
        public void BusStop(int busID)
        {
            if (!buses.ContainsKey(busID)) return;
            buses[busID].Stop();
        }
        

        // SFX - LOOPS - SONGS
        public void SFXAdd(int id, Sound sound, float volume = 0.5f, float pitch = 1.0f, params int[] busIDs)
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
        public void SFXLoopAdd(int id, Sound sound, float volume = 0.5f, float pitch = 1.0f, params int[] busIDs)
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
        public void SFXLoopAdd(int id, Sound sound, float minSpatialRange, float maxSpatialRange, float volume = 0.5f, float pitch = 1.0f, params int[] busIDs)
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
        public void SongAdd(int id, Music song, string displayName, float volume = 0.5f, float pitch = 1.0f, params int[] busIDs)
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
        

        public void SFXPlay(int id, float volume = 1f, float pitch = 1f, float blockDuration = 0f)
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
        public void SFXPlay(int id, Vector2 pos, float volume = 1.0f, float pitch = 1.0f, float blockDuration = 0f)
        {
            if (!sounds.ContainsKey(id)) return;
            if (!Raylib.CheckCollisionPointRec(pos, ScreenHandler.CameraArea())) return;

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
        public void SFXPlay(int id, Vector2 pos, float minRange, float maxRange, float volume = 1.0f, float pitch = 1.0f, float blockDuration = 0f)
        {
            if (!sounds.ContainsKey(id)) return;
            if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return;
            

            Vector2 center;
            if (SpatialTargetOverride == null) center = ScreenHandler.CAMERA.RawPos;
            else center = SpatialTargetOverride.GetPosition();

            float disSq = SVec.LengthSquared(center - pos);
            
            
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
                spatialVolumeFactor = 1f - SUtils.LerpInverseFloat(minSquared, maxSquared, disSq);
            }

            sounds[id].Play(volume * spatialVolumeFactor, pitch);
        }

        public void SFXLoopPlay(int id, float volume = 1f, float pitch = 1f, float blockDuration = 0f)
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
        public void SFXLoopPlay(int id, Vector2 pos, float volume = 1f, float pitch = 1f, float blockDuration = 0f)
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
        public void SFXLoopUpdateSpatialPos(int id, Vector2 pos)
        {
            if(loops.ContainsKey(id)) loops[id].SpatialPos = pos;
        }
        public void SFXLoopStop(int id)
        {
            if (!loops.ContainsKey(id)) return;
            loops[id].Stop();
        }

        
    }
}


//public static void PlaySong(int id, float volume = -1.0f, float pitch = -1.0f)
//{
//    if (!audioBusKeys.ContainsKey(id)) return;
//    Bus bus = buses[audioBusKeys[id]];
//    var newSong = bus.PlaySong(id, volume, pitch);
//    if (newSong != null) currentSong = newSong;
//}



//PRIVATE FUNCS
//private static void FilterSongs(List<int> songIDs)
//{
//    for (int i = songIDs.Count - 1; i >= 0; i--)
//    {
//        var id = songIDs[i];
//        if (!audioBusKeys.ContainsKey(id)) songIDs.RemoveAt(i);
//        if (!buses[audioBusKeys[id]].IsSong(id)) songIDs.RemoveAt(i);
//    }
//}
//private static void InvokeOnPlaylistStarted(Playlist playlist, string playlistName, string songName)
//{
//    OnPlaylistStarted?.Invoke(playlist, playlistName, songName);
//}


//LOOPING SFX OLD
/*
public static void PlaySFXLoop(int id, int soundID, float volume = -1f, float pitch = -1f)
{
    if (loopers.ContainsKey(id))
    {
        var looper = loopers[id];
        if (!looper.IsLooping())
        {
            looper.Start(volume, pitch);
        }
        else
        {
            looper.SetPitch(pitch);
            looper.SetVolume(volume);
        }
    }
    else
    {
        var looper = CreateSFXLoop(soundID, volume, pitch);
        if (looper != null)
        {
            loopers.Add(id, looper);
            looper.Start();
        }
    }

}
public static void PlaySFXLoop(int id, int soundID, Vector2 pos, float minRange, float maxRange, float volume = -1f, float pitch = -1f)
{
    if (loopers.ContainsKey(id))
    {
        var looper = loopers[id];

        looper.IsSpatial = true;
        looper.MinSpatialRange = minRange;
        looper.MaxSpatialRange = maxRange;
        looper.SpatialPos = pos;

        if (!looper.IsLooping())
        {
            looper.Start(volume, pitch);
        }
        else
        {
            looper.SetPitch(pitch);
            looper.SetVolume(volume);

        }
    }
    else
    {
        var looper = CreateSFXLoop(soundID, minRange, maxRange, volume, pitch);
        if (looper != null)
        {
            loopers.Add(id, looper);
            looper.SpatialPos = pos;
            looper.Start();
        }
    }

}
public static void UpdateSFXLoopSpatialPos(int id, Vector2 pos)
{
    if (!loopers.ContainsKey(id)) return;
    loopers[id].SpatialPos = pos;
}
public static bool IsSFXLoopLooping(int id)
{
    if (!loopers.ContainsKey(id)) return false;
    return loopers[id].IsLooping();
}
public static void RemoveSFXLoop(int id)
{
    if (!loopers.ContainsKey(id)) return;
    loopers[id].Disable();
    loopers.Remove(id);
}
public static void ClearSFXLoops()
{
    foreach (var looper in loopers.Values)
    {
        looper.Disable();
    }
    loopers.Clear();
}
public static void EnableSFXLoops()
{
    foreach (var looper in loopers.Values)
    {
        looper.Enable();
    }
}
public static void DisableSFXLoops()
{
    foreach (var looper in loopers.Values)
    {
        looper.Disable();
    }
}
//public static void EnableSFXLoop(string id)
//{
//    if (!loopers.ContainsKey(id)) return;
//    loopers[id].Enable();
//}
//public static void DisableSFXLoop(string id)
//{
//    if (!loopers.ContainsKey(id)) return;
//    loopers[id].Disable();
//}
//public static void StartSFXLoop(string id, float volume = -1f, float pitch = -1f)
//{
//    if (!loopers.ContainsKey(id)) return;
//    loopers[id].Start(volume, pitch);
//}
//public static void StopSFXLoop(string id)
//{
//    if (!loopers.ContainsKey(id)) return;
//    loopers[id].Stop();
//}
private static SFXLooper? CreateSFXLoop(int soundID, float minSpatialRange, float maxSpatialRange, float volume = -1.0f, float pitch = -1.0f)
{
    if (!audioBusKeys.ContainsKey(soundID)) return null;
    //if (sFXLoopers.ContainsKey(id)) return null;

    Bus bus = buses[audioBusKeys[soundID]];
    var sfx = bus.GetSFX(soundID);
    if (sfx != null)
    {
        SFXLooper looper = new(sfx, minSpatialRange, maxSpatialRange);
        looper.SetVolume(volume);
        looper.SetPitch(pitch);
        //sFXLoopers.Add(id, looper);
        return looper;
    }
    return null;
}
private static SFXLooper? CreateSFXLoop(int soundID, float volume = -1.0f, float pitch = -1.0f)
{
    if (!audioBusKeys.ContainsKey(soundID)) return null;
    //if (sFXLoopers.ContainsKey(id)) return null;

    Bus bus = buses[audioBusKeys[soundID]];
    var sfx = bus.GetSFX(soundID);
    if (sfx != null)
    {
        SFXLooper looper = new(sfx);
        looper.SetVolume(volume);
        looper.SetPitch(pitch);
        //sFXLoopers.Add(id, looper);
        return looper;
    }
    return null;
}
*/


//PLAYLISTS OLD
/*
public static void StartPlaylist(int id)
{
    string songName = "";
    if (currentPlaylist != null)
    {
        if (id == currentPlaylist.GetID())
        {
            songName = currentPlaylist.Start();
        }
        else
        {
            if (!playlists.ContainsKey(id)) return;
            currentPlaylist.Stop();
            currentPlaylist = playlists[id];
            songName = currentPlaylist.Start();
        }
    }
    else
    {
        currentPlaylist = playlists[id];
        songName = currentPlaylist.Start();
    }

    if(currentPlaylist != null)
    {
        InvokeOnPlaylistStarted(currentPlaylist, currentPlaylist.DisplayName, songName);
    }
}
public static Playlist? SwitchPlaylist(int id)
{
    if (currentPlaylist == null)
    {
        StartPlaylist(id);
        return currentPlaylist;
    }
    if (!playlists.ContainsKey(id) || currentPlaylist.GetID() == id) return currentPlaylist;
    currentPlaylist.Stop();
    currentPlaylist = playlists[id];
    currentPlaylist.Start();

    return currentPlaylist;
}
public static Playlist? GetCurPlaylist()
{
    if (currentPlaylist == null) return null;
    return currentPlaylist;
}
public static Playlist? GetPlaylist(int id)
{
    if (!playlists.ContainsKey(id)) return null;
    return playlists[id];
}
public static void StopPlaylist()
{
    if (currentPlaylist == null) return;
    currentPlaylist.Stop();
}
public static void PausePlaylist()
{
    if (currentPlaylist == null) return;
    currentPlaylist.Pause();
}
public static void ResumePlaylist()
{
    if (currentPlaylist == null) return;
    currentPlaylist.Resume();
}
public static bool IsPlaylistPaused()
{
    if (currentPlaylist == null) return false;
    return currentPlaylist.IsPaused();
}
public static bool IsPlaylistPlaying()
{
    if (currentPlaylist == null) return false;
    return currentPlaylist.IsPlaying();
}
public static void AddPlaylist(int id, string displayName, List<int> songIDs)
{
    if (playlists.ContainsKey(id)) return;

    FilterSongs(songIDs);
    if (songIDs.Count <= 0) return;
    List<Song> songs = new();
    foreach (var songName in songIDs)
    {
        Bus? bus = GetBusFromAudioName(songName);
        if (bus == null) continue;
        Song? s = bus.GetSong(songName);
        if (s == null) continue;
        songs.Add(s);
    }
    Playlist playlist = new(id, displayName, songs);
    playlists.Add(id, playlist);
}
public static void RemovePlaylist(int id)
{
    if (!playlists.ContainsKey(id)) return;
    if (currentPlaylist == playlists[id])
    {
        currentPlaylist.Stop();
        currentPlaylist = null;
    }
    playlists.Remove(id);
}
public static int CurrentPlaylistID()
{
    if (currentPlaylist == null) return -1;
    return currentPlaylist.GetID();
}
public static (string playlistName, string songName, float songPercentage) GetCurrentPlaylistInfo()
{
    if (currentPlaylist == null) return new("", "", 0f);
    else return new
        (
            currentPlaylist.DisplayName,
            currentPlaylist.GetCurrentSongDisplayName(),
            currentPlaylist.GetCurrentSongPercentage()
        );
}
public static string GetCurrentPlaylistSongDisplayName()
{
    if (currentPlaylist == null) return "no playlist playing";
    string name = currentPlaylist.GetCurrentSongDisplayName();
    if (name == "") return "no song playing";
    return name;
}
public static float GetCurrentPlaylistSongPercentage()
{
    if (currentPlaylist == null) return 0.0f;
    return currentPlaylist.GetCurrentSongPercentage();
}
*/
