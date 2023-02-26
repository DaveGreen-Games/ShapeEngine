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

        private static Dictionary<string, string> audioBusKeys = new();
        private static Dictionary<string, Bus> buses = new();
        private static Dictionary<string, Playlist> playlists = new();
        private static Playlist? currentPlaylist = null;
        private static Song? currentSong = null;

        private static Dictionary<string, float> soundBlockers = new();

        public static GameObject? spatialTargetOverride = null;

        private static Dictionary<string, SFXLooper> loopers = new();


        
        //MAIN
        public static void Initialize()
        {
            InitAudioDevice();
            buses.Add("master", new("master", 1.0f, null));
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
        public static void PlaySFXLoop(string id, string soundName, float volume = -1f, float pitch = -1f)
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
                var looper = CreateSFXLoop(soundName, volume, pitch);
                if (looper != null)
                {
                    loopers.Add(id, looper);
                    looper.Start();
                }
            }

        }
        public static void PlaySFXLoop(string id, string soundName, Vector2 pos, float minRange, float maxRange, float volume = -1f, float pitch = -1f)
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
                var looper = CreateSFXLoop(soundName, minRange, maxRange, volume, pitch);
                if (looper != null)
                {
                    loopers.Add(id, looper);
                    looper.SpatialPos = pos;
                    looper.Start();
                }
            }

        }
        public static void UpdateSFXLoopSpatialPos(string id, Vector2 pos)
        {
            if (!loopers.ContainsKey(id)) return;
            loopers[id].SpatialPos = pos;
        }
        public static bool IsSFXLoopLooping(string id)
        {
            if (!loopers.ContainsKey(id)) return false;
            return loopers[id].IsLooping();
        }
        public static void RemoveSFXLoop(string id)
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
        private static SFXLooper? CreateSFXLoop(string soundName, float minSpatialRange, float maxSpatialRange, float volume = -1.0f, float pitch = -1.0f)
        {
            if (!audioBusKeys.ContainsKey(soundName)) return null;
            //if (sFXLoopers.ContainsKey(id)) return null;

            Bus bus = buses[audioBusKeys[soundName]];
            var sfx = bus.GetSFX(soundName);
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
        private static SFXLooper? CreateSFXLoop(string soundName, float volume = -1.0f, float pitch = -1.0f)
        {
            if (!audioBusKeys.ContainsKey(soundName)) return null;
            //if (sFXLoopers.ContainsKey(id)) return null;

            Bus bus = buses[audioBusKeys[soundName]];
            var sfx = bus.GetSFX(soundName);
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
        public static void StartPlaylist(string name)
        {
            string songName = "";
            if (currentPlaylist != null)
            {
                if (name == "" || name == currentPlaylist.GetName())
                {
                    songName = currentPlaylist.Start();
                }
                else
                {
                    if (!playlists.ContainsKey(name)) return;
                    currentPlaylist.Stop();
                    currentPlaylist = playlists[name];
                    songName = currentPlaylist.Start();
                }
            }
            else
            {
                currentPlaylist = playlists[name];
                songName = currentPlaylist.Start();
            }

            if(currentPlaylist != null)
            {
                InvokeOnPlaylistStarted(currentPlaylist, currentPlaylist.DisplayName, songName);
            }
        }
        public static Playlist? SwitchPlaylist(string name)
        {
            if (currentPlaylist == null)
            {
                StartPlaylist(name);
                return currentPlaylist;
            }
            if (!playlists.ContainsKey(name) || currentPlaylist.GetName() == name) return currentPlaylist;
            currentPlaylist.Stop();
            currentPlaylist = playlists[name];
            currentPlaylist.Start();
            
            return currentPlaylist;
        }
        public static Playlist? GetCurPlaylist()
        {
            if (currentPlaylist == null) return null;
            return currentPlaylist;
        }
        public static Playlist? GetPlaylist(string name)
        {
            if (!playlists.ContainsKey(name)) return null;
            return playlists[name];
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
        public static void AddPlaylist(string name, string displayName, List<string> songNames)
        {
            if (playlists.ContainsKey(name)) return;

            FilterSongs(songNames);
            if (songNames.Count <= 0) return;
            List<Song> songs = new();
            foreach (string songName in songNames)
            {
                Bus? bus = GetBusFromAudioName(songName);
                if (bus == null) continue;
                Song? s = bus.GetSong(songName);
                if (s == null) continue;
                songs.Add(s);
            }
            Playlist playlist = new(name, displayName, songs);
            playlists.Add(name, playlist);
        }
        public static void RemovePlaylist(string name)
        {
            if (!playlists.ContainsKey(name)) return;
            if (currentPlaylist == playlists[name])
            {
                currentPlaylist.Stop();
                currentPlaylist = null;
            }
            playlists.Remove(name);
        }
        public static string CurrentPlaylistName()
        {
            if (currentPlaylist == null) return "";
            return currentPlaylist.GetName();
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
        public static Bus? GetBusFromAudioName(string audioName)
        {
            if (!audioBusKeys.ContainsKey(audioName)) return null;
            return buses[audioBusKeys[audioName]];
        }
        public static void AddBus(string name, float volume, string parentName)
        {
            if (name == "" || buses.ContainsKey(name)) return;
            if (parentName == "" || !buses.ContainsKey(parentName)) parentName = "master";
            Bus parent = buses[parentName];
            Bus bus = new Bus(name, volume, parent);
            parent.AddChild(bus);
            buses.Add(name, bus);
        }
        public static void SetBusVolume(string name, float volume)
        {
            if (name == "" || !buses.ContainsKey(name)) return;
            volume = Clamp(volume, 0.0f, 1.0f);
            buses[name].SetVolume(volume);

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
        public static void ChangeBusVolume(string name, float amount)
        {
            if (name == "" || !buses.ContainsKey(name)) return;
            SetBusVolume(name, buses[name].GetVolume() + amount);
        }
        public static float GetBusVolume(string name)
        {
            if (name == "" || !buses.ContainsKey(name)) return 1.0f;
            return buses[name].GetVolume();
        }
        public static void Stop(string name = "master")
        {
            if (name == "" || !buses.ContainsKey(name)) return;
            buses[name].Stop();
        }
        public static bool IsPaused(string name = "master")
        {
            if (name == "" || !buses.ContainsKey(name)) return false;
            return buses[name].IsPaused();
        }
        public static void Pause(string name = "master")
        {
            if (name == "" || !buses.ContainsKey(name)) return;
            buses[name].Pause();
        }
        public static void Resume(string name = "master")
        {
            if (name == "" || !buses.ContainsKey(name)) return;
            buses[name].Resume();
        }
        public static bool IsPlaying(string name = "master")
        {
            if (!audioBusKeys.ContainsKey(name)) return false;
            return buses[audioBusKeys[name]].IsPlaying(name);
        }



        //SFX & SONGS
        public static void AddSFX(string name, Sound sound, float volume = 0.5f, string bus = "master", float pitch = 1.0f)
        {
            if (audioBusKeys.ContainsKey(name) || !buses.ContainsKey(bus)) return;
            //if (fileName == "") return;
            //Sound sound = ResourceManager.LoadSound(fileName); // LoadSound(fileName);

            SFX sfx = new SFX(name, sound, volume, bus, pitch);
            buses[bus].AddAudio(name, sfx);
            audioBusKeys.Add(name, bus);
        }
        public static void AddSong(string name, Music song, string displayName, float volume = 0.5f, string bus = "master", float pitch = 1.0f)
        {
            if (audioBusKeys.ContainsKey(name) || !buses.ContainsKey(bus)) return;
            //if (fileName == "") return;
            //Music song = ResourceManager.LoadMusic(fileName); // LoadMusicStream(fileName);
            Song s = new Song(name, displayName, song, volume, bus, pitch);
            buses[bus].AddAudio(name, s);
            audioBusKeys.Add(name, bus);

        }
        public static void AddSFX(string name, string fileName, float volume = 0.5f, string bus = "master", float pitch = 1.0f)
        {
            if (audioBusKeys.ContainsKey(name) || !buses.ContainsKey(bus)) return;
            if (fileName == "") return;
            Sound sound = ResourceManager.LoadSoundFromRaylib(fileName); // LoadSound(fileName);

            SFX sfx = new SFX(name, sound, volume, bus, pitch);
            buses[bus].AddAudio(name, sfx);
            audioBusKeys.Add(name, bus);
        }
        public static void AddSong(string name, string fileName, string displayName, float volume = 0.5f, string bus = "master", float pitch = 1.0f)
        {
            if (audioBusKeys.ContainsKey(name) || !buses.ContainsKey(bus)) return;
            if (fileName == "") return;
            Music song = ResourceManager.LoadMusicFromRaylib(fileName); // LoadMusicStream(fileName);
            Song s = new Song(name, displayName, song, volume, bus, pitch);
            buses[bus].AddAudio(name, s);
            audioBusKeys.Add(name, bus);

        }

        public static void PlaySFXMulti(string name, float volume = -1.0f, float pitch = -1.0f)
        {
            if (!audioBusKeys.ContainsKey(name)) return;
            Bus bus = buses[audioBusKeys[name]];
            bus.PlaySFXMulti(name, volume, pitch);
        }
        public static void PlaySFX(string name, float volume = -1.0f, float pitch = -1.0f, float blockDuration = 0f)
        {
            if (!audioBusKeys.ContainsKey(name)) return;
            if (soundBlockers.ContainsKey(name) && soundBlockers[name] > 0f) return;
            if (blockDuration > 0f)
            {
                if (!soundBlockers.ContainsKey(name)) soundBlockers.Add(name, blockDuration);
                else soundBlockers[name] = blockDuration;
            }
            Bus bus = buses[audioBusKeys[name]];
            bus.PlaySFX(name, volume, pitch);
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
        /// <param name="name"></param>
        /// <param name="pos"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="blockDuration"></param>
        public static void PlaySFX(string name, Vector2 pos, float volume = -1.0f, float pitch = -1.0f, float blockDuration = 0f)
        {
            if (!audioBusKeys.ContainsKey(name)) return;
            if (soundBlockers.ContainsKey(name) && soundBlockers[name] > 0f) return;

            if (!Raylib.CheckCollisionPointRec(pos, ScreenHandler.CameraArea())) return;

            if (blockDuration > 0f)
            {
                if (!soundBlockers.ContainsKey(name)) soundBlockers.Add(name, blockDuration);
                else soundBlockers[name] = blockDuration;
            }
            Bus bus = buses[audioBusKeys[name]];
            bus.PlaySFX(name, volume, pitch);
        }
        /// <summary>
        /// Play the sound. If the pos is less than minRange from the current pos of the camera (or the spatial target override) the sound is played with full volume.
        /// If the pos is further away than minRange but less than maxRange from the pos of the camera the volume is linearly interpolated.
        /// If the pos is futher aways than maxRange the sound is not played.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        /// <param name="minRange"></param>
        /// <param name="maxRange"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="blockDuration"></param>
        public static void PlaySFX(string name, Vector2 pos, float minRange, float maxRange, float volume = -1.0f, float pitch = -1.0f, float blockDuration = 0f)
        {
            if (!audioBusKeys.ContainsKey(name)) return;
            if (soundBlockers.ContainsKey(name) && soundBlockers[name] > 0f) return;
            
            Bus bus = buses[audioBusKeys[name]];

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
                if (!soundBlockers.ContainsKey(name)) soundBlockers.Add(name, blockDuration);
                else soundBlockers[name] = blockDuration;
            }

            float spatialVolumeFactor = 1f;
            if(disSq > minSquared)
            {
                spatialVolumeFactor = 1f - SUtils.LerpInverseFloat(minSquared, maxSquared, disSq);
            }

            bus.PlaySFX(name, spatialVolumeFactor, volume, pitch);
        }
        public static void PlaySong(string name, float volume = -1.0f, float pitch = -1.0f)
        {
            if (!audioBusKeys.ContainsKey(name)) return;
            Bus bus = buses[audioBusKeys[name]];
            var newSong = bus.PlaySong(name, volume, pitch);
            if (newSong != null) currentSong = newSong;
        }


        
        //PRIVATE FUNCS
        private static void FilterSongs(List<string> songs)
        {
            for (int i = songs.Count - 1; i >= 0; i--)
            {
                string name = songs[i];
                if (!audioBusKeys.ContainsKey(name)) songs.RemoveAt(i);
                if (!buses[audioBusKeys[name]].IsSong(name)) songs.RemoveAt(i);
            }
        }
        private static void InvokeOnPlaylistStarted(Playlist playlist, string playlistName, string songName)
        {
            OnPlaylistStarted?.Invoke(playlist, playlistName, songName);
        }

    }
}