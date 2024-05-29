using Raylib_cs;
using ShapeEngine.Lib;
using System.Numerics;

namespace ShapeEngine.Audio
{

    internal abstract class Audio
    {
        public float BaseVolume { get; protected set; } = 0.5f;
        public float Volume { get; set; } = 1f;
        public float BasePitch { get; protected set; } = 1.0f;
        public float Pitch { get; set; } = 1f;
        public uint ID { get; protected set; }
        public bool Paused { get; protected set; } = false;
        protected Bus[] buses = new Bus[0];
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
    internal class SFX : Audio
    {
        public Sound Sound { get; protected set; }

        public SFX(uint id, Sound sound, Bus[] buses, float volume = 0.5f, float pitch = 1.0f)
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
        public override bool IsPlaying() { return Raylib.IsSoundPlaying(Sound); }
        public override void Play(float volume = 1f, float pitch = 1f)
        {
            float busVolume = GetCombinedBusVolume();
            Volume = volume;
            Pitch = pitch;
            Raylib.SetSoundVolume(Sound, busVolume * BaseVolume * Volume);
            Raylib.SetSoundPitch(Sound, BasePitch * Pitch);
            Raylib.PlaySound(Sound);
        }
        protected override void UpdateBusVolume(float newBusVolume)
        {
            Raylib.SetSoundVolume(Sound, newBusVolume * BaseVolume * Volume);
        }
        public override void Stop()
        {
            if (!IsPlaying()) return;
            Raylib.StopSound(Sound);
            Paused = false;
        }
        public override void Pause()
        {
            if (!IsPlaying()) return;
            Raylib.PauseSound(Sound);
            Paused = true;
        }
        public override void Resume()
        {
            if (!Paused) return;
            Raylib.ResumeSound(Sound);
            Paused = false;
        }

        public override void Unload()
        {
            Raylib.UnloadSound(Sound);
        }
    }
    internal class SFXLoop : Audio
    {
        public Sound Sound { get; protected set; }
        public bool IsLooping { get; protected set; } = false;

        public bool IsSpatial { get; set; } = false;
        public Vector2 SpatialPos { get; set; } = new();
        public float MinSpatialRange { get; set; } = 0f;
        public float MaxSpatialRange { get; set; } = 0f;
        public override bool IsPlaying() { return Raylib.IsSoundPlaying(Sound); }
        public SFXLoop(uint id, Sound sound, Bus[] buses, float volume = 0.5f, float pitch = 1.0f)
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
        public SFXLoop(uint id, Sound sound, float minSpatialRange, float maxSpatialRange, Bus[] buses, float volume = 0.5f, float pitch = 1.0f)
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
        public virtual void Update(float dt, Vector2 spatialCenter)
        {
            if (Paused) return;

            bool playing = IsPlaying();
            if (IsLooping)
            {
                if (IsSpatial)
                {
                    //Vector2 center;
                    float disSq = (spatialCenter - SpatialPos).LengthSquared();
                    if (MinSpatialRange < 0f) MinSpatialRange = 0f;
                    if (MaxSpatialRange < 0f || MaxSpatialRange <= MinSpatialRange) MaxSpatialRange = MinSpatialRange + 1;
                    float minSquared = MinSpatialRange * MinSpatialRange;
                    float maxSquared = MaxSpatialRange * MaxSpatialRange;
                    if (disSq >= maxSquared) return;

                    float spatialVolumeFactor = 1f;
                    if (disSq > minSquared)
                    {
                        spatialVolumeFactor = 1f - ShapeMath.LerpInverseFloat(minSquared, maxSquared, disSq);
                    }
                    Raylib.SetSoundVolume(Sound, BaseVolume * spatialVolumeFactor);
                }

                if (!playing)
                {
                    Raylib.PlaySound(Sound);
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
            Raylib.SetSoundVolume(Sound, newBusVolume * BaseVolume * Volume);
        }
        public override void Play(float volume = 1f, float pitch = 1f)
        {
            float busVolume = GetCombinedBusVolume();
            Volume = volume;
            Pitch = pitch;
            Raylib.SetSoundVolume(Sound, busVolume * BaseVolume * Volume);
            Raylib.SetSoundPitch(Sound, BasePitch * Pitch);
            Raylib.PlaySound(Sound);
            IsLooping = true;
        }
        public override void Stop()
        {
            if (IsPlaying()) Raylib.StopSound(Sound);
            Paused = false;
            IsLooping = false;

        }
        public override void Pause()
        {
            if (IsPlaying()) Raylib.PauseSound(Sound);
            Paused = true;
        }
        public override void Resume()
        {
            if (Paused) Raylib.ResumeSound(Sound);
            Paused = false;
        }

        public override void Unload()
        {
            Raylib.UnloadSound(Sound);
        }
    }
    internal class Song : Audio
    {
        public Music Music { get; protected set; }
        public string DisplayName { get; protected set; } = "";
        public Song(uint id, string displayName, Music song, Bus[] buses, float volume = 0.5f, float pitch = 1.0f)
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
            Raylib.SetMusicVolume(Music, busVolume * BaseVolume * Volume);
            Raylib.SetMusicPitch(Music, BasePitch * Pitch);
            Raylib.PlayMusicStream(Music);
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
            Raylib.SetMusicVolume(Music, newBusVolume * BaseVolume * Volume);
        }
        public override bool IsPlaying() { return Raylib.IsMusicStreamPlaying(Music); }
        public bool Update(float dt)
        {
            if (!IsPlaying()) return false;
            if (Paused) return false;
            Raylib.UpdateMusicStream(Music);
            float f = GetPercentage();
            return f > 0.95f;
        }
        public override void Stop()
        {
            if (!IsPlaying()) return;
            Raylib.StopMusicStream(Music);
            Paused = false;
        }
        public override void Pause()
        {
            if (!IsPlaying()) return;
            Raylib.PauseMusicStream(Music);
            Paused = true;
        }
        public override void Resume()
        {
            if (!Paused) return;
            Raylib.ResumeMusicStream(Music);
            Paused = false;
        }
        public override void Unload()
        {
            Raylib.UnloadMusicStream(Music);
        }
        public float GetPercentage()
        {
            float length = Raylib.GetMusicTimeLength(Music);
            float played = Raylib.GetMusicTimePlayed(Music);
            if (length <= 0.0f) return 0.0f;
            return played / length;
        }
    }

    /*
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
    */
}
