/*
namespace ShapeTiming
{
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
        public bool IsRunning { get { if(!HasStates) return false; return states[curState].IsRunning; } }
        public void AddState(AlternatorState state, bool set = false)
        {
            states.Add(state);
            if (set) SetState(states.Count - 1);
        }
        public void AddState(string name, float interval, bool set = false)
        {
            AddState(new(name, interval), set);
        }
        public void ChangeState(string name, string newName, float newInterval, bool set = false)
        {
            int index = FindState(name);
            if (index != -1)
            {
                states[index].Name = newName;
                states[index].Interval = newInterval;
                if (set) SetState(index);
            }
        }
        public void ChangeStateInterval(string name, float newInterval, bool set = false)
        {
            int index = FindState(name);
            if(index != -1)
            {
                states[index].Interval = newInterval;
                if (set) SetState(index);
            }
        }
        public void ChangeStateName(string name, string newName, bool set = false)
        {
            int index = FindState(name);
            if (index != -1)
            {
                states[index].Name = newName;
                if (set) SetState(index);
            }
        }
        public void SetState(int newState)
        {
            if (curState == newState) return;
            if (newState < 0 || newState >= states.Count) curState = 0;
            else curState = newState;
            //Start();
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
*/