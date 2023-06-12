/*
namespace Audio
{

    
    public class SFXLooper
    {
        protected bool disabled = false;
        protected bool isLooping = false;
        protected SFX sound;

        public bool IsSpatial { get; set; } = false;
        public Vector2 SpatialPos { get; set; } = new();
        public float MinSpatialRange { get; set; } = 0f;
        public float MaxSpatialRange { get; set; } = 0f;

        public event Action<SFXLooper>? OnLooped;

        
        public SFXLooper(SFX sound, float minSpatialRange, float maxSpatialRange)
        {
            this.sound = sound;
            this.MinSpatialRange = minSpatialRange;
            this.MaxSpatialRange = maxSpatialRange;
            this.IsSpatial = true;
        }
        public SFXLooper(SFX sound)
        {
            this.sound = sound;
            this.IsSpatial = false;
        }

        //public void UpdateSpatialPos(Vector2 newPos) { spatialPos = newPos; }
        public bool IsDisabled() { return disabled; }
        public bool IsEnabled() { return !disabled; }
        public void Enable()
        {
            if(isLooping) PlaySound(sound.GetSound());
            disabled = false;
        }
        public void Disable()
        {
            if(isLooping) StopSound(sound.GetSound());
            disabled = true;
        }
        public bool IsLooping() { return isLooping; }
        public float GetVolume() { return sound.GetVolume(); }
        public float GetPitch() { return sound.GetPitch(); }
        public void SetVolume(float volume) { sound.SetVolume(volume); }
        public void SetPitch(float pitch) { sound.SetPitch(pitch); }
        public virtual void Start(float volume = -1, float pitch = -1)
        {
            isLooping = true;
            if (volume > 0.0f) sound.SetVolume(volume);
            if (pitch > 0.0f) sound.SetPitch(pitch);
            if(!IsDisabled()) PlaySound(sound.GetSound());
        }
        public virtual void Stop()
        {
            isLooping = false;
            StopSound(sound.GetSound());
        }
        public virtual void Toggle()
        {
            if (isLooping) Stop();
            else Start();
        }
        protected virtual void Loop()
        {
            PlaySound(sound.GetSound());
            OnLooped?.Invoke(this);
        }
        public virtual void Update(float dt)
        {
            if (disabled) return;

            bool playing = IsSoundPlaying(sound.GetSound());
            if (isLooping)
            {
                if (IsSpatial)
                {
                    Vector2 center;
                    if (AudioHandler.SpatialTargetOverride == null) center = ScreenHandler.CAMERA.RawPos;
                    else center = AudioHandler.SpatialTargetOverride.GetPosition();

                    float disSq = SVec.LengthSquared(center - SpatialPos);
                    if (MinSpatialRange < 0f) MinSpatialRange = 0f;
                    if (MaxSpatialRange < 0f || MaxSpatialRange <= MinSpatialRange) MaxSpatialRange = MinSpatialRange + 1;
                    float minSquared = MinSpatialRange * MinSpatialRange;
                    float maxSquared = MaxSpatialRange * MaxSpatialRange;
                    if (disSq >= maxSquared) return;

                    float spatialVolumeFactor = 1f;
                    if (disSq > minSquared)
                    {
                        spatialVolumeFactor = 1f - SUtils.LerpInverseFloat(minSquared, maxSquared, disSq);
                    }
                    sound.AdjustVolume(spatialVolumeFactor);
                }

                if (!playing)
                {
                    Loop();
                }
            }
        }
    }
   
}
 */