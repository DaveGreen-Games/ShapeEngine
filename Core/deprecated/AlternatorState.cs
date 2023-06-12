
/*
namespace ShapeTiming
{
    public class AlternatorState
    {
        private string name = "";
        private float interval = 0f;
        private float timer = -1f;
        private float remainder = 0f;

        public AlternatorState(string name, float interval)
        {
            this.name = name;
            this.interval = interval;
        }

        public string Name
        {
            get { return name; }
            set { if (value != "") name = value; }
        }
        public float Interval
        {
            get { return interval; }
            set { if (value > 0) interval = value; }
        }
        public bool IsFinished { get { return timer == 0f; } }
        public bool IsRunning { get { return timer > 0f; } }
        public float Remainder { get { return remainder; } }
        public void Start(float remainder = 0f)
        {
            timer = interval - remainder;
            this.remainder = 0f;
        }
        public void Stop() { timer = -1f; remainder = 0f; }
        public void Update(float dt)
        {
            if (IsRunning)
            {
                timer -= dt;
                if (timer <= 0f)
                {
                    remainder = MathF.Abs(timer);
                    timer = 0f;
                }
            }
        }
    }

}
*/