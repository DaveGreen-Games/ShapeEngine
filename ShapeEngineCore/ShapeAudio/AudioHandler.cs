using Raylib_CsLo;
using ShapePersistent;
using ShapeScreen;
using System.Numerics;
using ShapeCore;
using ShapeLib;

namespace ShapeAudio
{
    //TODO (David):
    //figure out cross fade system for music switching
    //bus stopping/pausing/resuming also when volume of bus hits 0 stop all sfx and pause all music -> resume music if bus volume gets above 0

    public static class AudioHandler
    {
        public delegate void PlaylistStarted(Playlist playlist, string playlistName, string songName);
        public static event PlaylistStarted? OnPlaylistStarted;

        private static Dictionary<int, int> audioBusKeys = new();
        private static Dictionary<int, Bus> buses = new();
        private static Dictionary<int, Playlist> playlists = new();
        private static Playlist? currentPlaylist = null;
        private static Song? currentSong = null;

        private static Dictionary<int, float> soundBlockers = new();

        public static GameObject? spatialTargetOverride = null;

        private static Dictionary<int, SFXLooper> loopers = new();

        public const int BUS_MASTER = 0;
        
        //MAIN
        public static void Initialize()
        {
            InitAudioDevice();
            buses.Add(BUS_MASTER, new(BUS_MASTER, 1.0f, null));
        }
        public static void Close()
        {
            Stop();
            StopSoundMulti();
            currentPlaylist = null;
            foreach (Playlist playlist in playlists.Values)
            {
                playlist.Close();
            }
            //if (currentPlaylist != null) currentPlaylist.Close();
            //currentSong = null;
            foreach (Bus bus in buses.Values)
            {
                bus.Close();
            }
            audioBusKeys.Clear();
            buses.Clear();
            playlists.Clear();
            soundBlockers.Clear();
            ClearSFXLoops();
            CloseAudioDevice();
        }
        public static void Update(float dt)
        {
            //if(currentSong != null) UpdateMusicStream(currentSong.GetSong());
            if (currentPlaylist != null) currentPlaylist.Update(dt);
            if (currentSong != null) currentSong.Update(dt);

            if (spatialTargetOverride != null && spatialTargetOverride.IsDead()) spatialTargetOverride = null;

            foreach (var key in soundBlockers.Keys)
            {
                if (soundBlockers[key] <= 0f) continue;
                soundBlockers[key] -= dt;
                if (soundBlockers[key] <= 0f)
                {
                    soundBlockers[key] = 0f;
                }
            }

            foreach(var looper in loopers.Values)
            {
                looper.Update(dt);
            }
        }



        //LOOPING SFX
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



        //PLAYLISTS
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



        //BUS
        public static Bus? GetBusFromAudioName(int id)
        {
            if (!audioBusKeys.ContainsKey(id)) return null;
            return buses[audioBusKeys[id]];
        }
        public static void AddBus(int id, float volume, int parentID)
        {
            if (buses.ContainsKey(id)) return;
            if (!buses.ContainsKey(parentID)) parentID = BUS_MASTER;
            Bus parent = buses[parentID];
            Bus bus = new Bus(id, volume, parent);
            parent.AddChild(bus);
            buses.Add(id, bus);
        }
        public static void SetBusVolume(int id, float volume)
        {
            if (buses.ContainsKey(id)) return;
            volume = Clamp(volume, 0.0f, 1.0f);
            buses[id].SetVolume(volume);

            //float combinedVolume = CalculateBusVolume(name);
            //
            //List<SFX> sfx = sounds.Values.ToList();
            //foreach (SFX sound in sfx)
            //{
            //    if (sound.GetBus() == name) sound.ChangeCombinedVolume(combinedVolume);
            //}
            //List<Song> music = songs.Values.ToList();
            //foreach (Song song in music)
            //{
            //    if (song.GetBus() == name) song.ChangeCombinedVolume(combinedVolume);
            //}
        }
        public static void ChangeBusVolume(int id, float amount)
        {
            if (!buses.ContainsKey(id)) return;
            SetBusVolume(id, buses[id].GetVolume() + amount);
        }
        public static float GetBusVolume(int id)
        {
            if (!buses.ContainsKey(id)) return 1.0f;
            return buses[id].GetVolume();
        }
        public static void Stop(int id = BUS_MASTER)
        {
            if (!buses.ContainsKey(id)) return;
            buses[id].Stop();
        }
        public static bool IsPaused(int id = BUS_MASTER)
        {
            if (!buses.ContainsKey(id)) return false;
            return buses[id].IsPaused();
        }
        public static void Pause(int id = BUS_MASTER)
        {
            if (!buses.ContainsKey(id)) return;
            buses[id].Pause();
        }
        public static void Resume(int id = BUS_MASTER)
        {
            if (!buses.ContainsKey(id)) return;
            buses[id].Resume();
        }
        public static bool IsPlaying(int id = BUS_MASTER)
        {
            if (!audioBusKeys.ContainsKey(id)) return false;
            return buses[audioBusKeys[id]].IsPlaying(id);
        }



        //SFX & SONGS
        public static void AddSFX(int id, Sound sound, float volume = 0.5f, int bus = BUS_MASTER, float pitch = 1.0f)
        {
            if (audioBusKeys.ContainsKey(id) || !buses.ContainsKey(bus)) return;
            //if (fileName == "") return;
            //Sound sound = ResourceManager.LoadSound(fileName); // LoadSound(fileName);

            SFX sfx = new SFX(id, sound, volume, bus, pitch);
            buses[bus].AddAudio(id, sfx);
            audioBusKeys.Add(id, bus);
        }
        public static void AddSong(int id, Music song, string displayName, float volume = 0.5f, int bus = BUS_MASTER, float pitch = 1.0f)
        {
            if (audioBusKeys.ContainsKey(id) || !buses.ContainsKey(bus)) return;
            //if (fileName == "") return;
            //Music song = ResourceManager.LoadMusic(fileName); // LoadMusicStream(fileName);
            Song s = new Song(id, displayName, song, volume, bus, pitch);
            buses[bus].AddAudio(id, s);
            audioBusKeys.Add(id, bus);

        }
        public static void AddSFX(int id, string fileName, float volume = 0.5f, int bus = BUS_MASTER, float pitch = 1.0f)
        {
            if (audioBusKeys.ContainsKey(id) || !buses.ContainsKey(bus)) return;
            if (fileName == "") return;
            Sound sound = ResourceManager.LoadSoundFromRaylib(fileName); // LoadSound(fileName);

            SFX sfx = new SFX(id, sound, volume, bus, pitch);
            buses[bus].AddAudio(id, sfx);
            audioBusKeys.Add(id, bus);
        }
        public static void AddSong(int id, string fileName, string displayName, float volume = 0.5f, int bus = BUS_MASTER, float pitch = 1.0f)
        {
            if (audioBusKeys.ContainsKey(id) || !buses.ContainsKey(bus)) return;
            if (fileName == "") return;
            Music song = ResourceManager.LoadMusicFromRaylib(fileName); // LoadMusicStream(fileName);
            Song s = new Song(id, displayName, song, volume, bus, pitch);
            buses[bus].AddAudio(id, s);
            audioBusKeys.Add(id, bus);

        }

        public static void PlaySFXMulti(int id, float volume = -1.0f, float pitch = -1.0f)
        {
            if (!audioBusKeys.ContainsKey(id)) return;
            Bus bus = buses[audioBusKeys[id]];
            bus.PlaySFXMulti(id, volume, pitch);
        }
        public static void PlaySFX(int id, float volume = -1.0f, float pitch = -1.0f, float blockDuration = 0f)
        {
            if (!audioBusKeys.ContainsKey(id)) return;
            if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return;
            if (blockDuration > 0f)
            {
                if (!soundBlockers.ContainsKey(id)) soundBlockers.Add(id, blockDuration);
                else soundBlockers[id] = blockDuration;
            }
            Bus bus = buses[audioBusKeys[id]];
            bus.PlaySFX(id, volume, pitch);
        }
        
        //public static bool PlaySFXLoop(string id, float volume = -1.0f, float pitch = -1.0f)
        //{
        //    if (sFXLoopers.ContainsKey(id))
        //    {
        //        sFXLoopers[id].Start(volume, pitch);
        //        return true;
        //    }
        //    return false;
        //}
        //public static bool StopSFXLoop(string id)
        //{
        //    if (sFXLoopers.ContainsKey(id))
        //    {
        //        sFXLoopers[id].Stop();
        //        return true;
        //    }
        //    return false;
        //}
        //public static SFXLooper? GetSFXLoop(string id)
        //{
        //    if(sFXLoopers.ContainsKey(id)) return sFXLoopers[id];
        //    return null;
        //}
        //public static void RemoveSFXLoop(string id)
        //{
        //    if (sFXLoopers.ContainsKey(id))
        //    {
        //        sFXLoopers[id].Stop();
        //        sFXLoopers.Remove(id);
        //    }
        //}
        /// <summary>
        /// Play a sound. If pos is not inside the current camera area the sound is NOT played.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pos"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="blockDuration"></param>
        public static void PlaySFX(int id, Vector2 pos, float volume = -1.0f, float pitch = -1.0f, float blockDuration = 0f)
        {
            if (!audioBusKeys.ContainsKey(id)) return;
            if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return;

            if (!Raylib.CheckCollisionPointRec(pos, ScreenHandler.CameraArea())) return;

            if (blockDuration > 0f)
            {
                if (!soundBlockers.ContainsKey(id)) soundBlockers.Add(id, blockDuration);
                else soundBlockers[id] = blockDuration;
            }
            Bus bus = buses[audioBusKeys[id]];
            bus.PlaySFX(id, volume, pitch);
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
        public static void PlaySFX(int id, Vector2 pos, float minRange, float maxRange, float volume = -1.0f, float pitch = -1.0f, float blockDuration = 0f)
        {
            if (!audioBusKeys.ContainsKey(id)) return;
            if (soundBlockers.ContainsKey(id) && soundBlockers[id] > 0f) return;
            
            Bus bus = buses[audioBusKeys[id]];

            Vector2 center;
            if (spatialTargetOverride == null) center = ScreenHandler.CAMERA.RawPos;
            else center = spatialTargetOverride.GetPosition();

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

            bus.PlaySFX(id, spatialVolumeFactor, volume, pitch);
        }
        public static void PlaySong(int id, float volume = -1.0f, float pitch = -1.0f)
        {
            if (!audioBusKeys.ContainsKey(id)) return;
            Bus bus = buses[audioBusKeys[id]];
            var newSong = bus.PlaySong(id, volume, pitch);
            if (newSong != null) currentSong = newSong;
        }


        
        //PRIVATE FUNCS
        private static void FilterSongs(List<int> songIDs)
        {
            for (int i = songIDs.Count - 1; i >= 0; i--)
            {
                var id = songIDs[i];
                if (!audioBusKeys.ContainsKey(id)) songIDs.RemoveAt(i);
                if (!buses[audioBusKeys[id]].IsSong(id)) songIDs.RemoveAt(i);
            }
        }
        private static void InvokeOnPlaylistStarted(Playlist playlist, string playlistName, string songName)
        {
            OnPlaylistStarted?.Invoke(playlist, playlistName, songName);
        }

    }
}