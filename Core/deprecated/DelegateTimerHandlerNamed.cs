/*
namespace ShapeTiming
{
    
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
    
}
*/