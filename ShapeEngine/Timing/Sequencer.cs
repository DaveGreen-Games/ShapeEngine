
namespace ShapeEngine.Timing
{
   
    public sealed class SequencerState<T> where T : ISequenceable
    {
        internal readonly Sequences<T> state = new();

        internal SequencerState(Sequences<T> sequences)
        {
            state = sequences.Copy();
        }
    }

    internal sealed class Sequences<T> : Dictionary<uint, List<T>> where T : ISequenceable
    {
        public Sequences<T> Copy()
        {
            Sequences<T> returnValue = new();
            foreach (var key in this.Keys)
            {
                var copy = new List<T>();
                foreach (var item in this[key])
                {
                    copy.Add((T)item.Copy());
                }
                if(returnValue.ContainsKey(key)) returnValue[key].AddRange(copy);
                else returnValue.Add(key, copy);
            }

            return returnValue;
        }
    }
    
    public class Sequencer <T>  where T : ISequenceable
    {
        private static uint idCounter = 0;
        private static uint NextID { get { return idCounter++; } }
        
        public event Action<uint>? OnSequenceFinished;
        public event Action<T>? OnItemUpdated;

        private Sequences<T> sequences = new();

        public Sequencer() { }

        public SequencerState<T> GetState() => new(sequences);
        public SequencerState<T> Clear()
        {
            var state = new SequencerState<T>(sequences);
            sequences.Clear();
            return state;
        }

        public void ApplyState(SequencerState<T> state)
        {
            sequences = state.state;
        }
        
        public bool HasSequences() => sequences.Count > 0;
        public bool HasSequence(uint id) => sequences.ContainsKey(id);
        public uint StartSequence(params T[] items)
        {
            var id = NextID;
            sequences.Add(id, items.Reverse().ToList());
            return id;
        }
        public void CancelSequence(uint id)
        {
            if (sequences.ContainsKey(id)) sequences.Remove(id);
        }
        public void Stop() => sequences.Clear();

        public void Update(float dt)
        {
            StartUpdate();
            List<uint> remove = new();
            foreach (uint id in sequences.Keys)
            {
                var sequenceList = sequences[id];
                if (sequenceList.Count > 0)
                {
                    var sequence = sequenceList[sequenceList.Count - 1];//list is reversed
                    var finished = UpdateSequence(sequence, dt);
                    OnItemUpdated?.Invoke(sequence);
                    if (finished) sequenceList.RemoveAt(sequenceList.Count - 1);
                }
                else
                {
                    remove.Add(id);
                    OnSequenceFinished?.Invoke(id);
                }
            }

            foreach (uint id in remove) sequences.Remove(id);
            EndUpdate();
        }
        
        protected virtual bool UpdateSequence(T sequence, float dt) { return sequence.Update(dt); }
        protected virtual void StartUpdate() { }
        protected virtual void EndUpdate() { }
    }


    
}