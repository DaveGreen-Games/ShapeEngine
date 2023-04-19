namespace ShapeTiming
{
    public class BasicTimer
    {
        public float timer = 0.0f;
        public float Remaining { get { return timer; } }
        public float Elapsed { get; protected set; } = 0.0f;
        public float Duration { get; protected set; } = 0.0f;
        public bool Paused { get; protected set; } = false;
        public float F { get; protected set; } = 0.0f;

        public BasicTimer()
        {
        }

        public bool IsRunning { get { return timer > 0.0f; } }
        public bool IsFinished { get { return WasStarted && !IsRunning; } }
        public bool WasStarted { get { return Duration > 0f; } }


        public void Update(float dt)
        {
            if (Paused) return;

            if (timer > 0.0f)
            {
                timer -= dt;
                if (timer <= 0.0f)
                {
                    timer = 0.0f;
                    Elapsed = Duration;
                    F = 0.0f;
                    return;
                }
                Elapsed = Duration - timer;
                F = timer / Duration;
            }
        }

        public void Start(float duration)
        {
            if (duration <= 0.0f) return;

            Paused = false;
            timer = duration;
            this.Duration = duration;
            Elapsed = 0.0f;
            F = 1.0f;
        }
        public void Add(float amount)
        {
            if (amount <= 0f) return;
            Paused = false;
            Duration += amount;
            timer += amount;

            F = timer / Duration;
        }
        public void Pause()
        {
            if (Paused) return;
            Paused = true;
        }
        public void Resume()
        {
            if (!Paused) return;
            Paused = false;
        }
        public void Stop()
        {
            Paused = false;
            timer = 0.0f;
            F = 0.0f;
        }
        public void Cancel()
        {
            Paused = false;
            timer = 0f;
            F = 0f;
            Duration = 0f;
            Elapsed = 0f;
        }
        public void Restart()
        {
            if (Duration <= 0.0f) return;
            Paused = false;
            timer = Duration;
            F = 1.0f;
        }
    }

}
