

namespace ShapeTiming
{
    public interface ISequenceable
    {
        public bool Update(float dt);
    }

    
    public class Sequencer <T> where T : ISequenceable
    {
        public event Action<uint>? OnSequenceFinished;
        public event Action<T>? OnItemUpdated;

        protected Dictionary<uint, List<T>> sequences = new();

        private static uint idCounter = 0;
        private static uint NextID { get { return idCounter++; } }

        public Sequencer() { }

        public uint StartSequence(params T[] actionables)
        {
            var id = NextID;
            sequences.Add(id, actionables.Reverse().ToList());
            return id;
        }
        public void CancelSequence(uint id)
        {
            if (sequences.ContainsKey(id)) sequences.Remove(id);
        }
        public void Stop() { sequences.Clear(); }
        public void Update(float dt)
        {
            List<uint> remove = new();
            foreach (uint id in sequences.Keys)
            {
                var tweenList = sequences[id];
                if (tweenList.Count > 0)
                {
                    var tween = tweenList[tweenList.Count - 1];//list is reversed
                    var finished = tween.Update(dt);
                    OnItemUpdated?.Invoke(tween);
                    if (finished) tweenList.RemoveAt(tweenList.Count - 1);
                }
                else
                {
                    remove.Add(id);
                    OnSequenceFinished?.Invoke(id);
                }
            }

            foreach (uint id in remove) sequences.Remove(id);
        }
    }

}
