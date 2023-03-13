using Raylib_CsLo;

namespace ShapeAudio
{
    public class Audio
    {
        protected float volume = 0.5f;
        protected float pitch = 1.0f;
        protected int id = -1;
        protected int bus = 0;
        protected float combinedVolume = 1.0f;
        protected bool paused = false;

        public float GetVolume() { return volume; }
        public float GetPitch() { return pitch; }
        public virtual void SetPitch(float pitch) { }

        public void AdjustVolume(float factor) { volume *= factor; }
        public void SetVolume(float volume)
        {
            this.volume = volume;
            UpdateVolume(combinedVolume);
        }
        public void ChangeCombinedVolume(float newVolume)
        {
            combinedVolume = newVolume;
            UpdateVolume(combinedVolume);
        }
        protected virtual void UpdateVolume(float volume) { }
        public int GetBusID() { return bus; }
        public int GetID() { return id; }
        public virtual bool IsPlaying() { return false; }
        public bool IsPaused() { return paused; }
        public virtual void Stop() { }
        public virtual void Pause() { }
        public virtual void Resume() { }
        public virtual void Unload() { }
    }
    public class SFX : Audio
    {
        private Sound sound;

        public SFX(int id, Sound sound, float volume = 0.5f, int bus = AudioHandler.BUS_MASTER, float pitch = 1.0f)
        {
            this.id = id;
            this.sound = sound;
            this.bus = bus;
            this.pitch = pitch;
            this.volume = volume;
            //combinedVolume = AudioHandler.CalculateBusVolume(this.bus);
            //SetVolume(volume);
        }

        public override void SetPitch(float pitch)
        {
            this.pitch = pitch;
            SetSoundPitch(sound, pitch);
        }
        public override bool IsPlaying() { return IsSoundPlaying(sound); }
        public Sound GetSound() { return sound; }
        public override void Stop()
        {
            if (!IsPlaying()) return;
            StopSound(sound);
            paused = false;
        }
        public override void Pause()
        {
            if (!IsPlaying()) return;
            PauseSound(sound);
            paused = true;
        }
        public override void Resume()
        {
            if (!paused) return;
            ResumeSound(sound);
            paused = false;
        }

        public override void Unload()
        {
            UnloadSound(sound);
        }

        protected override void UpdateVolume(float volume)
        {
            SetSoundVolume(sound, volume * this.volume);
        }
    }
    public class Song : Audio
    {
        private Music song;
        private string displayName = "";
        public Song(int id, string displayName, Music song, float volume = 0.5f, int bus = AudioHandler.BUS_MASTER, float pitch = 1.0f)
        {
            this.id = id;
            this.displayName = displayName;
            this.song = song;
            this.bus = bus;
            this.pitch = pitch;
            this.volume = volume;
            //combinedVolume = AudioHandler.CalculateBusVolume(this.bus);
            //SetVolume(volume);
        }

        public string DisplayName { get { return displayName; } }
        public override void SetPitch(float pitch)
        {
            this.pitch = pitch;
            SetMusicPitch(song, pitch);
        }
        public override bool IsPlaying() { return IsMusicStreamPlaying(song); }
        public Music GetSong() { return song; }
        public override void Stop()
        {
            if (!IsPlaying()) return;
            StopMusicStream(song);
            paused = false;
        }
        public override void Pause()
        {
            if (!IsPlaying()) return;
            PauseMusicStream(song);
            paused = true;
        }
        public override void Resume()
        {
            if (!paused) return;
            ResumeMusicStream(song);
            paused = false;
        }

        public override void Unload()
        {
            UnloadMusicStream(song);
        }
        protected override void UpdateVolume(float volume)
        {
            SetMusicVolume(song, volume * this.volume);
        }

        public float GetPercentage()
        {
            float length = GetMusicTimeLength(song);
            float played = GetMusicTimePlayed(song);
            if (length <= 0.0f) return 0.0f;
            return played / length;
        }
        public bool Update(float dt)
        {
            if (!IsPlaying()) return false;
            if (IsPaused()) return false;
            UpdateMusicStream(song);
            float f = GetPercentage();
            return f > 0.95f;
            //float length = GetMusicTimeLength(song);
            //float played = GetMusicTimePlayed(song);
            //return played >= length;
        }
    }

}
