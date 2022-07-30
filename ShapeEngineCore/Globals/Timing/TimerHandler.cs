namespace ShapeEngineCore.Globals.Timing
{
    public class DelegateTimer
    {
        private Action a;
        private BasicTimer timer = new BasicTimer();
        private int repeats = -1;
        private bool finished = false;
        public DelegateTimer(float duration, Action action, int repeats = -1)
        {
            timer.Start(duration);
            a = action;
            this.repeats = repeats;
        }

        public void Update(float dt)
        {
            if (finished) return;
            timer.Update(dt);
            if (timer.IsFinished())
            {
                a();
                if (repeats == 0) { finished = true; }
                if (repeats > 0)
                {
                    repeats--;
                    timer.Restart();
                }
                else if (repeats < 0)
                {
                    timer.Restart();
                }
            }
        }

        public bool IsFinished() { return finished; }

    } //execute action after a certain amount of time has passed with possible repeats




    public class DelegateTimerHandlerNamed
    {
        private Dictionary<string, DelegateTimer> timers = new();
        private bool paused = false;


        public DelegateTimerHandlerNamed()
        {

        }

        public bool Has(string name) { return timers.ContainsKey(name); }
        public void Add(string name, float duration, Action action, int repeats = 0)
        {
            if (duration <= 0.0f) return;
            var timer = new DelegateTimer(duration, action, repeats);
            if (timers.ContainsKey(name))
            {
                timers[name] = timer;
            }
            else timers.Add(name, timer);
        }
        public void Remove(string name)
        {
            if (timers.ContainsKey(name)) timers.Remove(name);
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
            var keys = timers.Keys.ToList();
            for (int i = keys.Count() - 1; i >= 0; i--)
            {
                var key = keys[i];
                var timer = timers[key];
                timer.Update(dt);
                if (timer.IsFinished()) { timers.Remove(key); }
            }
        }

        public void Close()
        {
            timers.Clear();
        }

    }
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

    public static class TimerHandler
    {
        private static DelegateTimerHandler delegateTimerHandler = new DelegateTimerHandler();

        public static void Add(float duration, Action action, int repeats = 0)
        {
            delegateTimerHandler.Add(duration, action, repeats);
        }
        public static void Pause()
        {
            delegateTimerHandler.Pause();
        }
        public static void Resume()
        {
            delegateTimerHandler.Resume();
        }
        public static void Clear()
        {
            delegateTimerHandler.Clear();
        }
        public static void Update(float dt)
        {
            delegateTimerHandler.Update(dt);
        }
        public static void Close()
        {
            delegateTimerHandler.Close();
        }
    }

}
