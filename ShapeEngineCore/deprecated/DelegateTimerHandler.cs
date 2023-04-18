
/*
namespace ShapeTiming
{
    public class DelegateTimerHandler
    {
        private List<DelegateTimer> timers = new List<DelegateTimer>();
        private bool paused = false;


        public DelegateTimerHandler()
        {

        }

        public void Add(float duration, Action action, int repeats = 0)
        {
            if (duration <= 0.0f) return;
            timers.Add(new DelegateTimer(duration, action, repeats));
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
        public void Clear()
        {
            timers.Clear();
        }

        public void Update(float dt)
        {
            if (paused) return;

            for (int i = timers.Count() - 1; i >= 0; i--)
            {
                var timer = timers[i];
                timer.Update(dt);
                if (timer.IsFinished()) { timers.RemoveAt(i); }

            }
        }

        public void Close()
        {
            timers.Clear();
        }

    }


}
*/