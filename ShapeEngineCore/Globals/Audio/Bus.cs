namespace ShapeEngineCore.Globals.Audio

{
    public class Bus
    {
        private Bus? parent = null;
        private List<Bus> children = new();

        private Dictionary<string, Audio> audio = new();

        private float volume = 1.0f;
        private string name = "";
        private bool paused = false;

        public Bus(string name, float volume, Bus? parent)
        {
            this.name = name;
            this.volume = volume;
            this.parent = parent;
        }

        public void AddAudio(string name, Audio a)
        {
            if (audio.ContainsKey(name)) return;
            a.ChangeCombinedVolume(GetCombinedVolume());
            audio.Add(name, a);
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
        public string GetName() { return name; }
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

        public Audio? GetAudio(string name)
        {
            if (!audio.ContainsKey(name)) return null;
            return audio[name];
        }
        public Song? GetSong(string name)
        {
            if (!audio.ContainsKey(name)) return null;
            if (!IsSong(name)) return null;
            return audio[name] as Song;
        }

        public void PlaySFXMulti(string name, float volume = -1.0f, float pitch = -1.0f)
        {
            if (paused || name == "" || !audio.ContainsKey(name)) return;
            if (volume == 0.0f) return;
            if (audio[name] is SFX)
            {
                SFX? s = audio[name] as SFX;
                if (s == null) return;
                if (volume > 0.0f) s.SetVolume(volume);
                if (pitch > 0.0f) s.SetPitch(pitch);
                PlaySoundMulti(s.GetSound());
            }
        }
        public void PlaySFX(string name, float volume = -1.0f, float pitch = -1.0f)
        {
            if (paused || name == "" || !audio.ContainsKey(name)) return;
            if (volume == 0.0f) return;
            if (audio[name] is SFX)
            {
                SFX? s = audio[name] as SFX;
                if (s == null) return;
                if (volume > 0.0f) s.SetVolume(volume);
                if (pitch > 0.0f) s.SetPitch(pitch);
                PlaySound(s.GetSound());
            }
        }
        public Song? PlaySong(string name, float volume = -1.0f, float pitch = -1.0f)
        {
            if (paused || name == "" || !audio.ContainsKey(name)) return null;
            if (volume == 0.0f) return null;
            if (audio[name] is Song)
            {
                Song? s = audio[name] as Song;
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
        public bool IsPlaying(string name)
        {
            if (!audio.ContainsKey(name)) return false;
            return audio[name].IsPlaying();
        }
        public bool IsSong(string name)
        {
            if (!audio.ContainsKey(name)) return false;
            return audio[name] is Song;
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

}
