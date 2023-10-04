

namespace ShapeEngine.Timing
{
    public class Repeater : ISequenceable
    {
        /// <summary>
        /// Delegate that is called after duration for every repeat. Takes the specified duration and return a duration as well.
        /// </summary>
        /// <param name="duration">Takes in the specified duration for modification.</param>
        /// <returns>Returns the duration for the next cycle.</returns>
        public delegate float RepeaterFunc(float duration);

        private RepeaterFunc repeaterFunc;
        private float timer;
        private float duration;
        private int remainingRepeats;

        public Repeater(RepeaterFunc repeaterFunc, float duration, int repeats = 0)
        {
            this.repeaterFunc = repeaterFunc;
            this.duration = duration;
            this.timer = 0f;
            this.remainingRepeats = repeats;
        }
        public Repeater(Repeater repeater)
        {
            this.repeaterFunc = repeater.repeaterFunc;
            this.duration = repeater.duration;
            this.timer = repeater.duration;
            this.remainingRepeats = repeater.remainingRepeats;
        }

        public ISequenceable Copy() => new Repeater(this);
        public bool Update(float dt)
        {
            if (duration <= 0f) return true;

            timer += dt;
            if (timer >= duration)
            {
                float dur = repeaterFunc(duration);
                if (remainingRepeats > 0)
                {
                    timer = 0f;// timer - duration; //in case timer over shot
                    duration = dur;
                    remainingRepeats--;
                }
            }
            return timer >= duration && remainingRepeats <= 0;
        }
    }

}
