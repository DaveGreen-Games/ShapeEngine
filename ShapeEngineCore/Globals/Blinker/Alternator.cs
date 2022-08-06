using ShapeEngineCore.Globals.Timing;

namespace ShapeEngineCore.Globals.Blinker
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

        public string Name { get { return name; } }
        public float Interval { get { return interval; } }
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
            if(IsRunning)
            {
                timer -= dt;
                if(timer <= 0f)
                {
                    remainder = MathF.Abs(timer);
                    timer = 0f;
                }
            }
        }
    }

    public class Alternator
    {
        private List<AlternatorState> states = new();
        private int curState = -1;

        public Alternator(List<AlternatorState> states)
        {
            this.states = states;
            SetState(0);
        }
        public Alternator(params AlternatorState[] states)
        {
            this.states = states.ToList();
            SetState(0);
        }
        public Alternator(int startState, List<AlternatorState> states)
        {
            this.states = states;
            SetState(startState);
        }
        public Alternator(int startState, params AlternatorState[] states)
        {
            this.states = states.ToList();
            SetState(startState);
        }

        public string Cur { get { return HasStates ? states[curState].Name : ""; } }
        public AlternatorState? State { get { return HasStates ? states[curState] : null; } }
        public bool HasStates { get { return states.Count > 0; } }

        public void SetState(int newState)
        {
            if (newState < 0 || newState >= states.Count || curState == newState) return;
            curState = newState;
            Start();
        }
        public void SetState(string newState)
        {
            SetState(FindState(newState));
        }
        public void Start()
        {
            if(!HasStates) return;
            states[curState].Start();
        }
        public void Start(int state)
        {
            if (!HasStates) return;
            SetState(state);
        }
        public void Start(string state)
        {
            if (!HasStates) return;
            SetState(state);
        }

        public void Stop()
        {
            if (!HasStates) return;
            states[curState].Stop();
        }

        public void Update(float dt)
        {
            if (!HasStates) return;

            var state = states[curState];
            if (!state.IsRunning) return;

            state.Update(dt);
            if (state.IsFinished)
            {
                curState += 1;
                if (curState >= states.Count) curState = 0;
                states[curState].Start(state.Remainder);
            }
        }

        private int FindState(string name)
        {
            if (!HasStates) return -1;
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].Name == name) return i;
            }
            return -1;
        }
    }
}
