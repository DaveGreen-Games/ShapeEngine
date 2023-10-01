using ShapeEngine.Lib;

namespace ShapeEngine.Audio
{
    internal class Playlist
    {
        public event Action<uint, Playlist>? RequestNextSong;
        public event Action<string, string>? SongStarted;

        private HashSet<uint> mixtape = new();
        private List<uint> queue = new();

        public uint ID { get; private set; }
        public Song? CurrentSong { get; private set; } = null;
        public string DisplayName { get; private set; } = "";
        public bool Paused { get; private set; } = false;
        public Playlist(uint id, string displayName, HashSet<uint> songIDs)
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
                uint id = PopNextID();
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
            uint id = PopNextID();
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

        public void DeliverNextSong(Song song)
        {
            CurrentSong = song;
            SongStarted?.Invoke(song.DisplayName, DisplayName);
            song.Play();
        }
        public void AddSongID(uint id)
        {
            if (mixtape.Add(id))
            {
                queue.Add(id);
            }
        }

        private uint PopNextID()
        {
            if (queue.Count <= 0) Refill();
            int index = ShapeRandom.randI(0, queue.Count);
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

    /*
    public class Playlist
    {
        public delegate void SongStarted(string songName, string playlistName);
        public event SongStarted? OnSongStarted;

        private List<Song> mixtape = new();
        private List<Song> queue = new();
        private int id = -1;
        private Song? currentSong = null;

        private bool paused = false;
        private string displayName = "";
        public Playlist(int id, string displayName, List<Song> songs)
        {
            this.id = id;
            this.displayName = displayName;
            AddSongs(songs);
        }

        public string DisplayName { get { return displayName; } }
        public void Close()
        {
            if (currentSong != null) currentSong.Stop();
            currentSong = null;
            mixtape.Clear();
            queue.Clear();
        }
        public bool IsPlaying()
        {
            return currentSong != null;
        }
        public bool IsPaused()
        {
            if (currentSong == null) return paused;
            return currentSong.IsPaused() || paused;
        }
        public string Start()
        {
            if (IsPaused())
            {
                Resume();
            }

            Stop();
            currentSong = PopNext();
            PlayMusicStream(currentSong.GetSong());
            return GetCurrentSongDisplayName();
        }
        public void Stop()
        {
            if (!IsPlaying()) return;
            currentSong.Stop();
            currentSong = null;
        }
        public void Pause()
        {
            paused = true;
            if (!IsPlaying()) return;
            currentSong.Pause();
        }
        public void Resume()
        {
            paused = false;
            if (!IsPlaying()) return;
            currentSong.Resume();
        }


        public void Update(float dt)
        {
            if (IsPaused()) return;
            if (!IsPlaying()) return;

            if (currentSong.Update(dt))
            {
                currentSong.Stop();
                currentSong = PopNext();
                PlayMusicStream(currentSong.GetSong());
                InvokeOnSongStarted(currentSong.DisplayName);
            }
        }

        public void AddSong(Song song)
        {
            if (mixtape.Contains(song)) return;
            mixtape.Add(song);
        }
        public void AddSongs(List<Song> songs)
        {
            if (songs != null && songs.Count > 0)
            {
                foreach (Song song in songs)
                {
                    AddSong(song);
                }
            }
        }
        public void RemoveSong(Song song)
        {
            if (!mixtape.Contains(song)) return;
            mixtape.Remove(song);
            queue.Remove(song);
        }
        public Song PopNext()
        {
            if (queue.Count <= 0) Refill();
            int index = SRNG.randI(0, queue.Count);
            Song next = queue[index];
            queue.RemoveAt(index);
            return next;
        }
        public void Refill()
        {
            queue.Clear();
            queue.AddRange(mixtape);
        }
        public int GetID() { return id; }
        public string GetCurrentSongDisplayName()
        {
            if (currentSong == null) return "";
            return currentSong.DisplayName;
        }
        public string GetCurrentSongName()
        {
            if (currentSong == null) return "";
            return currentSong.DisplayName;
        }
        public float GetCurrentSongPercentage()
        {
            if (currentSong == null) return 0.0f;
            return currentSong.GetPercentage();
        }


        private void InvokeOnSongStarted(string songName)
        {
            OnSongStarted?.Invoke(songName, displayName);
        }
    }
    */
}
