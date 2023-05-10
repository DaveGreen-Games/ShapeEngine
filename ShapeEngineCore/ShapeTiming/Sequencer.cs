

using System.Runtime.CompilerServices;

namespace ShapeTiming
{
    public interface ISequenceable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>Returns if finished.</returns>
        public bool Update(float dt);
    }



    public class Sequencer <T>  where T : ISequenceable
    {
        private static uint idCounter = 0;
        private static uint NextID { get { return idCounter++; } }
        
        public event Action<uint>? OnSequenceFinished;
        public event Action<T>? OnItemUpdated;

        protected Dictionary<uint, List<T>> sequences = new();

        public Sequencer() { }

        public bool HasSequences() { return sequences.Count > 0; }
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
        public void Stop() { sequences.Clear(); }
        public void Update(float dt)
        {
            StartUpdate();
            List<uint> remove = new();
            foreach (uint id in sequences.Keys)
            {
                var sequenceList = sequences[id];
                if (sequenceList.Count > 0)
                {
                    var seqence = sequenceList[sequenceList.Count - 1];//list is reversed
                    var finished = UpdateSequence(seqence, dt); // seqence.Update(dt);
                    OnItemUpdated?.Invoke(seqence);
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

//doesnt work
    //public interface ISequencer<T> where T : ISequenceable
    //{
    //    public event Action<uint>? OnSequenceFinished;
    //    public event Action<T>? OnItemUpdated;
    //
    //
    //    protected static uint idCounter = 0;
    //    protected static uint NextID { get { return idCounter++; } }
    //
    //
    //    public bool HasSequences();
    //    public uint StartSequence(params T[] actionables);
    //    public void CancelSequence(uint id);
    //    public void Stop();
    //    public void Update(float dt);
    //    //protected virtual bool UpdateSequence(T sequence, float dt) { return sequence.Update(dt); }
    //    //protected virtual void StartUpdate() { }
    //    //protected virtual void EndUpdate() { }
    //}