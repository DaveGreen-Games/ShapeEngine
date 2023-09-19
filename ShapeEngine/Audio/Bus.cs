namespace ShapeEngine.Audio
{
    internal class Bus
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
        public uint ID { get; protected set; }

        public Bus(uint id, float volume)
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

    /*
    public class Bus
    {
        private Bus? parent = null;
        private List<Bus> children = new();

        private Dictionary<int, Audio> audio = new();

        private float volume = 1.0f;
        private int id = -1;
        private bool paused = false;

        public Bus(int id, float volume, Bus? parent)
        {
            this.id = id;
            this.volume = volume;
            this.parent = parent;
        }

        public void AddAudio(int id, Audio a)
        {
            if (audio.ContainsKey(id)) return;
            a.ChangeCombinedVolume(GetCombinedVolume());
            audio.Add(id, a);
        }
        public float GetVolume() { return volume; }
        public void SetVolume(float volume)
        {
            this.volume = volume;
            VolumeChanged();
        }
        protected void VolumeChanged()
        {
            float combinedVolume = GetCombinedVolume();
            foreach (Audio a in audio.Values)
            {
                a.ChangeCombinedVolume(combinedVolume);
            }
            foreach (var child in children)
            {
                child.VolumeChanged();
            }
        }
        public int GetID() { return id; }
        public Bus? GetParent() { return parent; }
        public void AddChild(Bus child)
        {
            if (children.Contains(child)) return;
            children.Add(child);
        }
        public void RemoveChild(Bus child)
        {
            if (!children.Contains(child)) return;
            children.Remove(child);
        }
        public void Remove()
        {
            if (parent == null) return;
            parent.RemoveChild(this);
            foreach (Bus child in children)
            {
                parent.AddChild(child);
            }
            children.Clear();
        }
        public void Close()
        {
            foreach (Audio a in audio.Values)
            {
                a.Unload();
            }
            audio.Clear();
            parent = null;
            children.Clear();
        }

        public Audio? GetAudio(int id)
        {
            if (!audio.ContainsKey(id)) return null;
            return audio[id];
        }
        public Song? GetSong(int id)
        {
            if (!audio.ContainsKey(id)) return null;
            if (audio[id] is Song song) return song;
            return null;
        }
        public SFX? GetSFX(int id)
        {
            if (!audio.ContainsKey(id)) return null;
            if (audio[id] is SFX sfx) return sfx;
            return null;
        }
        public void PlaySFXMulti(int id, float volume = -1.0f, float pitch = -1.0f)
        {
            if (paused || !audio.ContainsKey(id)) return;
            if (volume == 0.0f) return;
            if (audio[id] is SFX)
            {
                SFX? s = audio[id] as SFX;
                if (s == null) return;
                if (volume > 0.0f) s.SetVolume(volume);
                if (pitch > 0.0f) s.SetPitch(pitch);
                PlaySoundMulti(s.GetSound());
            }
        }
        public void PlaySFX(int id, float volume = -1.0f, float pitch = -1.0f)
        {
            if (paused || !audio.ContainsKey(id)) return;
            if (volume == 0.0f) return;
            if (audio[id] is SFX)
            {
                SFX? s = audio[id] as SFX;
                if (s == null) return;
                
                if (volume > 0.0f) s.SetVolume(volume);
                if (pitch > 0.0f) s.SetPitch(pitch);
                PlaySound(s.GetSound());
            }
        }
        public void PlaySFX(int id, float volumeFactor, float volume = -1.0f, float pitch = -1.0f)
        {
            if (paused || !audio.ContainsKey(id)) return;
            if (volume == 0.0f || volumeFactor <= 0f) return;
            if (audio[id] is SFX)
            {
                SFX? s = audio[id] as SFX;
                if (s == null) return;

                if (volume > 0.0f) s.SetVolume(volume);
                
                s.AdjustVolume(volumeFactor);
                
                if (pitch > 0.0f) s.SetPitch(pitch);
                PlaySound(s.GetSound());
            }
        }
        public Song? PlaySong(int id, float volume = -1.0f, float pitch = -1.0f)
        {
            if (paused || !audio.ContainsKey(id)) return null;
            if (volume == 0.0f) return null;
            if (audio[id] is Song)
            {
                Song? s = audio[id] as Song;
                if (s == null) return null;
                if (volume > 0.0f) s.SetVolume(volume);
                if (pitch > 0.0f) s.SetPitch(pitch);
                PlayMusicStream(s.GetSong());
                return s;
            }
            return null;
        }

        public bool IsPaused() { return paused; }
        public void Stop()
        {
            foreach (Audio a in audio.Values)
            {
                a.Stop();
            }
            foreach (Bus child in children)
            {
                child.Stop();
            }
        }
        public void Pause()
        {
            if (!IsPaused())
            {
                paused = true;
                foreach (Audio a in audio.Values)
                {
                    a.Pause();
                }
            }
            foreach (Bus child in children)
            {
                child.Pause();
            }
        }
        public void Resume()
        {
            if (IsPaused())
            {
                paused = false;
                foreach (Audio a in audio.Values)
                {
                    a.Resume();
                }
            }
            foreach (Bus child in children)
            {
                child.Resume();
            }
        }
        public bool IsPlaying(int id)
        {
            if (!audio.ContainsKey(id)) return false;
            return audio[id].IsPlaying();
        }
        public bool IsSong(int id)
        {
            if (!audio.ContainsKey(id)) return false;
            return audio[id] is Song;
        }
        private float GetCombinedVolume()
        {
            Bus? parent = GetParent();
            float volume = GetVolume();
            if (parent == null) return volume;
            while (parent != null)
            {
                volume *= parent.GetVolume();
                parent = parent.GetParent();
            }
            return volume;
        }

    }
    */
}
