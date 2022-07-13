using ShapeEngineCore.Globals;

namespace ShapeEngineCore.Globals.Audio

{
    public class Playlist
    {
        private List<Song> mixtape = new();
        private List<Song> queue = new();
        private string name = "";
        private Song? currentSong = null;

        private bool paused = false;
        public Playlist(string name, List<Song> songs)
        {
            this.name = name;
            AddSongs(songs);
        }

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
        public void Start()
        {
            if (IsPaused())
            {
                Resume();
            }

            Stop();
            currentSong = PopNext();
            PlayMusicStream(currentSong.GetSong());
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
            int index = RNG.randI(0, queue.Count);
            Song next = queue[index];
            queue.RemoveAt(index);
            return next;
        }
        public void Refill()
        {
            queue.Clear();
            queue.AddRange(mixtape);
        }
        public string GetName() { return name; }
        public string GetCurrentSongName()
        {
            if (currentSong == null) return "";
            return currentSong.GetName();
        }
        public float GetCurrentSongPercentage()
        {
            if (currentSong == null) return 0.0f;
            return currentSong.GetPercentage();
        }
    }

}
