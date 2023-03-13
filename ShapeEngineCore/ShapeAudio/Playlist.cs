using ShapeLib;

namespace ShapeAudio

{
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

}
