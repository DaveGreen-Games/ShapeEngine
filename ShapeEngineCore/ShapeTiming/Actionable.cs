

namespace ShapeTiming
{
    public class Actionable : ISequenceable
    {
        public delegate void ActionableFunc(float timeF, float dt);
        private ActionableFunc action;
        private float duration;
        private float timer;

        public Actionable(ActionableFunc action, float duration)
        {
            this.action = action;
            this.duration = duration;
            this.timer = 0f;
        }

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);

            timer += dt;
            action(t, dt);
            return t >= 1f;
        }

    }

}
