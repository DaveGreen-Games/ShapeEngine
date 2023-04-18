
/*
namespace ShapeTiming
{
    public class DelegateTimer
    {
        private Action a;
        private BasicTimer timer = new BasicTimer();
        private int repeats = -1;
        private bool finished = false;
        public DelegateTimer(float duration, Action action, int repeats = -1)
        {
            timer.Start(duration);
            a = action;
            this.repeats = repeats;
        }

        public void Update(float dt)
        {
            if (finished) return;
            timer.Update(dt);
            if (timer.IsFinished())
            {
                a();
                if (repeats == 0) { finished = true; }
                if (repeats > 0)
                {
                    repeats--;
                    timer.Restart();
                }
                else if (repeats < 0)
                {
                    timer.Restart();
                }
            }
        }

        public bool IsFinished() { return finished; }

    } //execute action after a certain amount of time has passed with possible repeats

}
*/