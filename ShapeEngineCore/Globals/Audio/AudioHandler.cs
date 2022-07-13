using Raylib_CsLo;

namespace ShapeEngineCore.Globals.Audio

{
    //TODO (David):
    //figure out cross fade system for music switching
    //bus stopping/pausing/resuming also when volume of bus hits 0 stop all sfx and pause all music -> resume music if bus volume gets above 0

    public static class AudioHandler
    {
        private static Dictionary<string, string> audioBusKeys = new();
        private static Dictionary<string, Bus> buses = new();
        private static Dictionary<string, Playlist> playlists = new();
        private static Playlist? currentPlaylist = null;
        private static Song? currentSong = null;

        private static Dictionary<string, float> soundBlockers = new();
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
            CloseAudioDevice();
        }
        public static void Update(float dt)
        {
            //if(currentSong != null) UpdateMusicStream(currentSong.GetSong());
            if (currentPlaylist != null) currentPlaylist.Update(dt);
            if (currentSong != null) currentSong.Update(dt);

            foreach (var key in soundBlockers.Keys)
            {
                if (soundBlockers[key] <= 0f) continue;
                soundBlockers[key] -= dt;
                if (soundBlockers[key] <= 0f)
                {
                    soundBlockers[key] = 0f;
                }
            }
        }

        public static Bus? GetBusFromAudioName(string audioName)
        {
            if (!audioBusKeys.ContainsKey(audioName)) return null;
            return buses[audioBusKeys[audioName]];
        }

        public static void StartPlaylist(string name)
        {
            if (currentPlaylist != null)
            {
                if (name == "" || name == currentPlaylist.GetName())
                {
                    currentPlaylist.Start();
                }
                else
                {
                    if (!playlists.ContainsKey(name)) return;
                    currentPlaylist.Stop();
                    currentPlaylist = playlists[name];
                    currentPlaylist.Start();
                }
            }
            else
            {
                currentPlaylist = playlists[name];
                currentPlaylist.Start();
            }
        }
        public static void SwitchPlaylist(string name)
        {
            if (currentPlaylist == null)
            {
                StartPlaylist(name);
                return;
            }
            if (!playlists.ContainsKey(name)) return;
            if (currentPlaylist.GetName() == name) return;
            currentPlaylist.Stop();
            currentPlaylist = playlists[name];
            currentPlaylist.Start();
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
        public static void AddPlaylist(string name, List<string> songNames)
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
            Playlist playlist = new(name, songs);
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


        public static void AddSFX(string name, string path, float volume = 0.5f, string bus = "master", float pitch = 1.0f)
        {
            if (audioBusKeys.ContainsKey(name) || !buses.ContainsKey(bus)) return;
            if (path == "") return;
            Sound sound = LoadSound(path);

            SFX sfx = new SFX(name, sound, volume, bus, pitch);
            buses[bus].AddAudio(name, sfx);
            audioBusKeys.Add(name, bus);
        }
        public static void AddSong(string name, string path, float volume = 0.5f, string bus = "master", float pitch = 1.0f)
        {
            if (audioBusKeys.ContainsKey(name) || !buses.ContainsKey(bus)) return;
            if (path == "") return;
            Music song = LoadMusicStream(path);
            Song s = new Song(name, song, volume, bus, pitch);
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
        public static void PlaySong(string name, float volume = -1.0f, float pitch = -1.0f)
        {
            if (!audioBusKeys.ContainsKey(name)) return;
            Bus bus = buses[audioBusKeys[name]];
            var newSong = bus.PlaySong(name, volume, pitch);
            if (newSong != null) currentSong = newSong;
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

        private static void FilterSongs(List<string> songs)
        {
            for (int i = songs.Count - 1; i >= 0; i--)
            {
                string name = songs[i];
                if (!audioBusKeys.ContainsKey(name)) songs.RemoveAt(i);
                if (!buses[audioBusKeys[name]].IsSong(name)) songs.RemoveAt(i);
            }
        }

        public static string Debug_GetCurrentPlaylistSongName()
        {
            if (currentPlaylist == null) return "no playlist playing";
            string name = currentPlaylist.GetCurrentSongName();
            if (name == "") return "no song playing";
            return name;
        }
        public static float Debug_GetCurrentPlaylistSongPercentage()
        {
            if (currentPlaylist == null) return 0.0f;
            return currentPlaylist.GetCurrentSongPercentage();
        }

    }

}


/*
        private static void StopSFX()
        {
            foreach (SFX sfx in sounds.Values)
            {
                sfx.Stop();
            }
        }
        private static void PauseSFX()
        {
            foreach (SFX sfx in sounds.Values)
            {
                sfx.Pause();
            }
        }
        private static void ResumeSFX()
        {
            foreach (SFX sfx in sounds.Values)
            {
                sfx.Resume();
            }
        }
        private static void StopCurrentSong()
        {
            if (currentSong == null) return;
            currentSong.Stop();
        }
        private static void PauseCurrentSong()
        {
            if(currentSong == null) return;
            currentSong.Pause();
        }
        private static void ResumeCurrentSong()
        {
            if(currentSong == null) return;
            currentSong.Resume();
        }
        private static void StopSFXMulti() { StopSoundMulti(); }
        private static int GetSFXPlayingCount() { return GetSoundsPlaying(); }
        private static void SetSFXVolume(string sound, float volume)
        {
            if (!sounds.ContainsKey(sound)) return;
            SFX s = sounds[sound];
            if(volume <= 0.0f && s.IsPlaying()) StopSound(s.GetSound());
            s.SetVolume(volume);
        }
        private static void SetSFXPitch(string sound, float pitch)
        {
            if (!sounds.ContainsKey(sound)) return;
            sounds[sound].SetPitch(pitch);
        }
        private static void SetSongVolume(string song, float volume)
        {
            if (!songs.ContainsKey(song)) return;
            Song s = songs[song];
            if (volume <= 0.0f && s.IsPlaying()) StopMusicStream(s.GetSong());
            s.SetVolume(volume);
        }
        private static void SetSongPitch(string song, float pitch)
        {
            if (!songs.ContainsKey(song)) return;
            songs[song].SetPitch(pitch);
        }
        */

//public static float CalculateBusVolume(string name)
//{
//    if (name == "" || !buses.ContainsKey(name)) return 1.0f;
//    Bus bus = buses[name];
//    Bus? parent = bus.GetParent();
//    float volume = bus.GetVolume();
//    while(parent != null)
//    {
//        volume *= parent.GetVolume();
//        parent = parent.GetParent();
//    }
//    return volume;
//}


//public static void RemoveBus(string name)
//{
//    if (name == "" || name == "master" || !buses.ContainsKey(name)) return;
//    Bus bus = buses[name];
//    bus.Close();
//    buses.Remove(name);
//    List<SFX> sfx = sounds.Values.ToList();
//    foreach (SFX sound in sfx)
//    {
//        if(sound.GetBus() == name) sounds.Remove(sound.GetName());
//    }
//    List<Song> music = songs.Values.ToList();
//    foreach (Song song in music)
//    {
//        if (song.GetBus() == name) songs.Remove(song.GetName());
//    }
//}
