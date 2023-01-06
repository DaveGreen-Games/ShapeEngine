namespace ShapeTiming
{
    //execute delegates every frame until true(finished) is returned

    public delegate bool Step(float dt);
    public delegate bool StepTimer(float dt, float f);
    public delegate bool StepFrame(float dt, int frame, int maxFrames); //variant for using easing lib (uses frames instead of timer)
    public class StepTimerContainer
    {
        private StepTimer step;
        private float timer;
        private float maxTimer;
        public StepTimerContainer(StepTimer step, float time)
        {
            timer = time;
            maxTimer = time;
            this.step = step;
        }
        public bool Update(float dt)
        {
            timer -= dt;
            return step(dt, timer / maxTimer);
        }

        public bool Contains(StepTimer step) { return step == this.step; }
    }
    public class StepFrameContainer
    {
        private StepFrame step;
        private int frame;
        private int maxFrames;
        public StepFrameContainer(StepFrame step, int maxFrames)
        {
            frame = 0;
            this.maxFrames = maxFrames;
            this.step = step;
        }
        public bool Update(float dt)
        {
            frame++;
            return step(dt, frame, maxFrames);
        }
        public bool Contains(StepFrame step) { return step == this.step; }
    }

    public static class StepHandler
    {
        private static List<Step> steps = new();
        private static List<StepTimerContainer> stepTimers = new();
        private static List<StepFrameContainer> stepFrames = new();

        public static void Add(Step step)
        {
            if (step == null) return;
            steps.Add(step);
        }
        public static void Add(StepTimer step, float timer)
        {
            if (step == null || timer <= 0.0f) return;
            stepTimers.Add(new(step, timer));
        }
        public static void Add(StepFrame step, int maxFrames)
        {
            if (step == null || maxFrames <= 0) return;
            stepFrames.Add(new(step, maxFrames));
        }

        public static void Remove(Step step)
        {
            if (step == null || steps.Count <= 0) return;
            steps.Remove(step);
        }
        public static void Remove(StepTimer step)
        {
            if (step == null || stepTimers.Count <= 0) return;
            for (int i = stepTimers.Count - 1; i >= 0; i--)
            {
                if (stepTimers[i].Contains(step)) { stepTimers.RemoveAt(i); return; }
            }
        }
        public static void Remove(StepFrame step)
        {
            if (step == null || stepFrames.Count <= 0) return;
            for (int i = stepFrames.Count - 1; i >= 0; i--)
            {
                if (stepFrames[i].Contains(step)) { stepFrames.RemoveAt(i); return; }
            }
        }

        public static void ClearSteps() { steps.Clear(); }
        public static void ClearStepTimers() { stepTimers.Clear(); }
        public static void ClearStepFrames() { stepFrames.Clear(); }
        public static void Clear() { steps.Clear(); stepTimers.Clear(); stepFrames.Clear(); }

        public static void Update(float dt)
        {
            for (int i = steps.Count - 1; i >= 0; i--)
            {
                if (steps[i](dt)) { steps.RemoveAt(i); }
            }

            for (int i = stepTimers.Count - 1; i >= 0; i--)
            {
                if (stepTimers[i].Update(dt)) { stepTimers.RemoveAt(i); }
            }

            for (int i = stepFrames.Count - 1; i >= 0; i--)
            {
                if (stepFrames[i].Update(dt)) { stepFrames.RemoveAt(i); }
            }
        }

        public static void Close()
        {
            Clear();
        }
    }

}
