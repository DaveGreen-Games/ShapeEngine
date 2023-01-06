namespace ShapeTiming
{
    public class BasicTimer
    {
        private float timer = 0.0f;
        private float elapsed = 0.0f;
        private float duration = 0.0f;
        private bool paused = false;
        private float f = 0.0f;

        public BasicTimer()
        {
        }

        public bool IsRunning() { return timer > 0.0f; }
        public bool IsPaused() { return paused; }
        public bool IsFinished() { return WasStarted() && !IsRunning(); }
        public bool WasStarted() { return duration > 0f; }
        public float GetF() { return f; }
        public float GetDuration() { return duration; }
        public float GetElapsed() { return elapsed; }
        public float GetRemaining() { return timer; }


        public void Update(float dt)
        {
            if (paused) return;

            if (timer > 0.0f)
            {
                timer -= dt;
                if (timer <= 0.0f)
                {
                    timer = 0.0f;
                    elapsed = duration;
                    f = 0.0f;
                    return;
                }
                elapsed = duration - timer;
                f = timer / duration;
            }
        }

        public void Start(float duration)
        {
            if (duration <= 0.0f) return;

            paused = false;
            timer = duration;
            this.duration = duration;
            elapsed = 0.0f;
            f = 1.0f;
        }
        public void Add(float amount)
        {
            if (amount <= 0f) return;
            paused = false;
            duration += amount;
            timer += amount;

            f = timer / duration;
        }
        public void Pause()
        {
            if (paused) return;
            paused = true;
        }

        public void Resume()
        {
            if (!paused) return;
            paused = false;
        }

        public void Stop()
        {
            paused = false;
            timer = 0.0f;
            f = 0.0f;
        }
        public void Cancel()
        {
            paused = false;
            timer = 0f;
            f = 0f;
            duration = 0f;
            elapsed = 0f;
        }
        public void Restart()
        {
            if (duration <= 0.0f) return;
            paused = false;
            timer = duration;
            f = 1.0f;
        }
    }

}
