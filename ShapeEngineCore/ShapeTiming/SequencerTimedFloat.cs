

namespace ShapeTiming
{
    public interface ISequenceableTimedFloat : ISequenceable
    {
        public float ApplyValue(float total);
    }
    public class TimedFloat : ISequenceableTimedFloat
    {
        private float timer = 0f;
        private float value = 0f;

        public TimedFloat(float duration, float value)
        {
            this.timer = duration;
            this.value = value;
        }

        public float ApplyValue(float total) { return total * value; }
        public bool Update(float dt)
        {
            if (timer <= 0f) return true;
            timer -= dt;
            return timer <= 0f;
        }
    }
    public class SequencerTimedFloat<T> : Sequencer<T> where T : ISequenceableTimedFloat
    {
        public float Total { get; protected set; } = 1f;
        private float accumulated = 1f;

        protected override void StartUpdate()
        {
            accumulated = 1f;
        }
        protected override void EndUpdate()
        {
            Total = accumulated;
        }
        protected override bool UpdateSequence(T sequence, float dt)
        {
            accumulated = sequence.ApplyValue(accumulated);
            return base.UpdateSequence(sequence, dt);
        }
    }

}
