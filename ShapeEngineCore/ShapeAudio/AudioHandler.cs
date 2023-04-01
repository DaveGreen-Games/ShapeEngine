using Raylib_CsLo;
using ShapeScreen;
using System.Numerics;
using ShapeCore;
using ShapeLib;

namespace ShapeAudio
{

    internal class Bus2
    {
        public event Action<float>? VolumeChanged;
        public event Action? Stopped;
        private float volume = 1f;
        public float Volume 
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
                VolumeChanged?.Invoke(volume);
            } 
        } 
        public int ID { get; protected set; } = -1;

        public Bus2(int id, float volume)
        {
            this.ID = id;
            this.volume = volume;
        }
        public void Stop()
        {
            Stopped?.Invoke();
        }
        //public float GetCombinedVolume()
        //{
        //    if (Parent == null) return Volume;
        //
        //    float v = 1f;
        //
        //    Bus2? current = Parent;
        //    while (current != null)
        //    {
        //        v *= current.Volume;
        //        current = current.Parent;
        //    }
        //
        //    return v;
        //}
    }
    internal class Playlist2
    {
        public event Action<int, Playlist2>? RequestNextSong;
        public event Action<string, string>? SongStarted;

        private HashSet<int> mixtape = new();
        private List<int> queue = new();
        
        public int ID { get; private set; } = -1;
        public Song2? CurrentSong { get; private set; } = null;
        public string DisplayName { get; private set; } = "";
        public bool Paused { get; private set; } = false;
        public Playlist2(int id, string displayName, HashSet<int> songIDs)
        {
            this.ID = id;
            this.DisplayName = displayName;
            this.mixtape = songIDs;
            this.Refill();
        }

        public void Update(float dt)
        {
            if (Paused) return;

            if (CurrentSong != null && CurrentSong.Update(dt))
            {
                CurrentSong.Stop();
                int id = PopNextID();
                RequestNextSong?.Invoke(id, this);
            }
        }
        public void Close()
        {
            if (CurrentSong != null) CurrentSong.Stop();
            CurrentSong = null;
            mixtape.Clear();
            queue.Clear();
        }
        public bool IsPlaying()
        {
            return CurrentSong != null;
        }
        public void Start()
        {
            if (IsPlaying()) return;
            int id = PopNextID();
            RequestNextSong?.Invoke(id, this);
        }
        public void Stop()
        {
            if (CurrentSong == null) return;
            CurrentSong.Stop();
            CurrentSong = null;
        }
        public void Pause()
        {
            Paused = true;
            if (CurrentSong == null) return;
            CurrentSong.Pause();
        }
        public void Resume()
        {
            Paused = false;
            if (CurrentSong == null) return;
            CurrentSong.Resume();
        }

        public void DeliverNextSong(Song2 song)
        {
            CurrentSong = song;
            SongStarted?.Invoke(song.DisplayName, DisplayName);
            song.Play();
        }
        public void AddSongID(int id)
        {
            if (mixtape.Add(id))
            {
                queue.Add(id);
            }
        }
       
        private int PopNextID()
        {
            if (queue.Count <= 0) Refill();
            int index = SRNG.randI(0, queue.Count);
            int nextID = queue[index];
            queue.RemoveAt(index);
            return nextID;
        }
        private void Refill()
        {
            queue.Clear();
            queue.AddRange(mixtape);
        }

    }

    internal class Audio2
    {
        public float BaseVolume {get; protected set; } = 0.5f;
        public float Volume { get; set; } = 1f;
        public float BasePitch { get; protected set; } = 1.0f;
        public float Pitch { get; set; } = 1f;
        public int ID { get; protected set; } = -1;
        public bool Paused { get; protected set; } = false;
        protected Bus2[] buses = new Bus2[0];
        public virtual bool IsPlaying() { return false; }
        public virtual void Play(float volume = 1f, float pitch = 1f) { }
        public virtual void Stop() { }
        public virtual void Pause() { }
        public virtual void Resume() { }
        public virtual void Unload() { }
        protected virtual void UpdateBusVolume(float newBusVolume) { }
        protected float GetCombinedBusVolume()
        {
            float v = 1f;
            foreach (var bus in buses)
            {
                if (bus.Volume <= 0f) return 0f;
                v *= bus.Volume;
            }
            return v;
        }
    }
    internal class SFX2 : Audio2
    {
        public Sound Sound { get; protected set; }

        public SFX2(int id, Sound sound, Bus2[] buses, float volume = 0.5f, float pitch = 1.0f)
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
        public override bool IsPlaying() { return IsSoundPlaying(Sound); }
        public override void Play(float volume = 1f, float pitch = 1f)
        {
            float busVolume = GetCombinedBusVolume();
            Volume = volume;
            Pitch = pitch;
            SetSoundVolume(Sound, busVolume * BaseVolume * Volume);
            SetSoundPitch(Sound, BasePitch * Pitch);
            PlaySound(Sound);
        }
        protected override void UpdateBusVolume(float newBusVolume)
        {
            SetSoundVolume(Sound, newBusVolume * BaseVolume * Volume);
        }
        public override void Stop()
        {
            if (!IsPlaying()) return;
            StopSound(Sound);
            Paused = false;
        }
        public override void Pause()
        {
            if (!IsPlaying()) return;
            PauseSound(Sound);
            Paused = true;
        }
        public override void Resume()
        {
            if (!Paused) return;
            ResumeSound(Sound);
            Paused = false;
        }

        public override void Unload()
        {
            UnloadSound(Sound);
        }
    }
    internal class SFXLoop : Audio2
    {
        public Sound Sound { get; protected set; }
        public bool IsLooping { get; protected set; } = false;

        public bool IsSpatial { get; set; } = false;
        public Vector2 SpatialPos { get; set; } = new();
        public float MinSpatialRange { get; set; } = 0f;
        public float MaxSpatialRange { get; set; } = 0f;
        public override bool IsPlaying() { return IsSoundPlaying(Sound); }
        public SFXLoop(int id, Sound sound, Bus2[] buses, float volume = 0.5f, float pitch = 1.0f)
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
        public SFXLoop(int id, Sound sound, float minSpatialRange, float maxSpatialRange, Bus2[] buses, float volume = 0.5f, float pitch = 1.0f)
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
        public virtual void Update(float dt)
        {
            if (Paused) return;

            bool playing = IsPlaying();
            if (IsLooping)
            {
                if (IsSpatial)
                {
                    Vector2 center;
                    if (AudioHandler.SpatialTargetOverride == null) center = ScreenHandler.CAMERA.RawPos;
                    else center = AudioHandler.SpatialTargetOverride.GetPosition();

                    float disSq = SVec.LengthSquared(center - SpatialPos);
                    if (MinSpatialRange < 0f) MinSpatialRange = 0f;
                    if (MaxSpatialRange < 0f || MaxSpatialRange <= MinSpatialRange) MaxSpatialRange = MinSpatialRange + 1;
                    float minSquared = MinSpatialRange * MinSpatialRange;
                    float maxSquared = MaxSpatialRange * MaxSpatialRange;
                    if (disSq >= maxSquared) return;

                    float spatialVolumeFactor = 1f;
                    if (disSq > minSquared)
                    {
                        spatialVolumeFactor = 1f - SUtils.LerpInverseFloat(minSquared, maxSquared, disSq);
                    }
                    SetSoundVolume(Sound, BaseVolume * spatialVolumeFactor);
                }

                if (!playing)
                {
                    PlaySound(Sound);
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
            SetSoundVolume(Sound, newBusVolume * BaseVolume * Volume);
        }
        public override void Play(float volume = 1f, float pitch = 1f)
        {
            float busVolume = GetCombinedBusVolume();
            Volume = volume;
            Pitch = pitch;
            SetSoundVolume(Sound, busVolume * BaseVolume * Volume);
            SetSoundPitch(Sound, BasePitch * Pitch);
            PlaySound(Sound);
            IsLooping = true;
        }
        public override void Stop()
        {
            if(IsPlaying()) StopSound(Sound);
            Paused = false;
            IsLooping = false;
            
        }
        public override void Pause()
        {
            if(IsPlaying()) PauseSound(Sound);
            Paused = true;
        }
        public override void Resume()
        {
            if(Paused) ResumeSound(Sound);
            Paused = false;
        }

        public override void Unload()
        {
            UnloadSound(Sound);
        }
    }
    internal class Song2 : Audio2
    {
        public Music Music { get; protected set; }
        public string DisplayName { get; protected set; } = "";
        public Song2(int id, string displayName, Music song, Bus2[] buses, float volume = 0.5f, float pitch = 1.0f)
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
            SetMusicVolume(Music, busVolume * BaseVolume * Volume);
            SetMusicPitch(Music, BasePitch * Pitch);
            PlayMusicStream(Music);
        }
        protected override void UpdateBusVolume(float newBusVolume)
        {
            if(newBusVolume <= 0f)
            {
                if (IsPlaying() && !Paused) Pause();
            }
            else
            {
                if (Paused) Resume();
            }
            SetMusicVolume(Music, newBusVolume * BaseVolume * Volume);
        }
        public override bool IsPlaying() { return IsMusicStreamPlaying(Music); }
        public bool Update(float dt)
        {
            if (!IsPlaying()) return false;
            if (Paused) return false;
            UpdateMusicStream(Music);
            float f = GetPercentage();
            return f > 0.95f;
        }
        public override void Stop()
        {
            if (!IsPlaying()) return;
            StopMusicStream(Music);
            Paused = false;
        }
        public override void Pause()
        {
            if (!IsPlaying()) return;
            PauseMusicStream(Music);
            Paused = true;
        }
        public override void Resume()
        {
            if (!Paused) return;
            ResumeMusicStream(Music);
            Paused = false;
        }
        public override void Unload()
        {
            UnloadMusicStream(Music);
        }
        public float GetPercentage()
        {
            float length = GetMusicTimeLength(Music);
            float played = GetMusicTimePlayed(Music);
            if (length <= 0.0f) return 0.0f;
            return played / length;
        }
    }



    public static class AudioHandler
    {
        public static event Action<string>? PlaylistStarted;
        public static event Action<string, string>? PlaylistSongStarted;
        public const int BUS_MASTER = 0;

        private static Dictionary<int, Bus2> buses = new();
        private static Dictionary<int, SFX2> sounds = new();
        private static Dictionary<int, SFXLoop> loops = new();
        private static Dictionary<int, Song2> songs = new();
        
        private static Dictionary<int, Playlist2> playlists = new();
        private static Playlist2? currentPlaylist = null;
        private static Dictionary<int, float> soundBlockers = new();
        public static GameObject? SpatialTargetOverride { get; set; } = null;


        //MAIN
        public static void Initialize()
        {
            InitAudioDevice();
            BusAdd(BUS_MASTER, 1f);
        }
        public static void Close()
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
        public static void Update(float dt)
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
                if(SpatialTargetOverride != null) loop.SpatialPos = SpatialTargetOverride.GetPosition();
                loop.Update(dt);
            }
        }


        //PLAYLISTS
        private static void OnPlaylistRequestSong(int id, Playlist2 playlist)
        {
            if (songs.ContainsKey(id)) playlist.DeliverNextSong(songs[id]);
        }
        private static void OnPlaylistSongStarted(string songName, string playlistName)
        {
            PlaylistSongStarted?.Invoke(songName, playlistName);
        }
        public static void PlaylistAdd(int id, string displayName, params int[] songIDs)
        {
            if (playlists.ContainsKey(id)) return;
            Playlist2 playlist = new(id, displayName, songIDs.ToHashSet());
            playlist.RequestNextSong += OnPlaylistRequestSong;
            playlist.SongStarted += OnPlaylistSongStarted;
            playlists.Add(id, playlist);
        }
        public static void PlaylistStart()
        {
            if (currentPlaylist == null) return;
            currentPlaylist.Start();
            PlaylistStarted?.Invoke(currentPlaylist.DisplayName);
        }
        public static void PlaylistStop()
        {
            if (currentPlaylist == null) return;
            currentPlaylist.Stop();
        }
        public static void PlaylistPause()
        {
            if (currentPlaylist == null) return;
            currentPlaylist.Pause();
        }
        public static void PlaylistResume()
        {
            if (currentPlaylist == null) return;
            currentPlaylist.Resume();
        }
        public static void PlaylistSwitch(int id)
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
        public static string PlaylistGetName() { return currentPlaylist != null ? currentPlaylist.DisplayName : ""; }
        public static string PlaylistGetSongName()
        {
            if (currentPlaylist == null) return "";
            if (currentPlaylist.CurrentSong == null) return "";
            return currentPlaylist.CurrentSong.DisplayName;
        }
        public static float PlaylistGetSongPercentage()
        {
            if (currentPlaylist == null) return -1f;
            if (currentPlaylist.CurrentSong == null) return -1f;
            return currentPlaylist.CurrentSong.GetPercentage();
        }
        
        
        
        //BUS
        public static void BusAdd(int busID, float volume)
        {
            if (buses.ContainsKey(busID)) return;
            Bus2 bus = new Bus2(busID, volume);
            buses.Add(busID, bus);
        }
        public static void BusSetVolume(int busID, float volume)
        {
            if (!buses.ContainsKey(busID)) return;
            volume = Clamp(volume, 0.0f, 1.0f);
            buses[busID].Volume = volume;

        }
        public static void BusChangeVolume(int busID, float amount)
        {
            if (!buses.ContainsKey(busID)) return;
            float newVolume = Clamp(buses[busID].Volume + amount, 0.0f, 1.0f);
            buses[busID].Volume = newVolume;
        }
        public static float BusGetVolume(int busID)
        {
            if (!buses.ContainsKey(busID)) return 1.0f;
            return buses[busID].Volume;
        }
        public static void BusStop(int busID)
        {
            if (!buses.ContainsKey(busID)) return;
            buses[busID].Stop();
        }
        

        // SFX - LOOPS - SONGS
        public static void SFXAdd(int id, Sound sound, float volume = 0.5f, float pitch = 1.0f, params int[] busIDs)
        {
            if (sounds.ContainsKey(id)) return;
            List<Bus2> b = new();
            foreach (var busID in busIDs)
            {
                if (buses.ContainsKey(busID)) b.Add(buses[busID]);
            }
            SFX2 sfx = new SFX2(id, sound, b.ToArray(), volume, pitch);

            sounds.Add(id, sfx);
        }
        public static void SFXLoopAdd(int id, Sound sound, float volume = 0.5f, float pitch = 1.0f, params int[] busIDs)
        {
            if (sounds.ContainsKey(id)) return;
            List<Bus2> b = new();
            foreach (var busID in busIDs)
            {
                if (buses.ContainsKey(busID)) b.Add(buses[busID]);
            }
            SFXLoop loop = new SFXLoop(id, sound, b.ToArray(), volume, pitch);

            loops.Add(id, loop);
        }
        public static void SFXLoopAdd(int id, Sound sound, float minSpatialRange, float maxSpatialRange, float volume = 0.5f, float pitch = 1.0f, params int[] busIDs)
        {
            if (sounds.ContainsKey(id)) return;
            List<Bus2> b = new();
            foreach (var busID in busIDs)
            {
                if (buses.ContainsKey(busID)) b.Add(buses[busID]);
            }
            SFXLoop loop = new SFXLoop(id, sound, minSpatialRange, maxSpatialRange, b.ToArray(), volume, pitch);

            loops.Add(id, loop);
        }
        public static void SongAdd(int id, Music song, string displayName, float volume = 0.5f, float pitch = 1.0f, params int[] busIDs)
        {
            if(songs.ContainsKey(id)) return;
            List<Bus2> b = new();
            foreach (var busID in busIDs)
            {
                if (buses.ContainsKey(busID)) b.Add(buses[busID]);
            }

            Song2 s = new Song2(id, displayName, song, b.ToArray(), volume, pitch);

            songs.Add(id, s);
        }
        

        public static void SFXPlay(int id, float volume = 1f, float pitch = 1f, float blockDuration = 0f)
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
        public static void SFXPlay(int id, Vector2 pos, float volume = 1.0f, float pitch = 1.0f, float blockDuration = 0f)
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
        public static void SFXPlay(int id, Vector2 pos, float minRange, float maxRange, float volume = 1.0f, float pitch = 1.0f, float blockDuration = 0f)
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

        public static void SFXLoopPlay(int id, float volume = 1f, float pitch = 1f, float blockDuration = 0f)
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
        public static void SFXLoopStop(int id)
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
