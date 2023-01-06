
namespace ShapeInput
{
    public class InputActionWrapper
    {
        float holdTimer = -1f;
        float holdInterval = 1f;
        float tapTimer = -1f;
        float tapInterval = 1f;
        int taps = 2; //double tap
        int tapCount = 0;
        bool pressed = false;
        bool released = false;
        public InputActionWrapper(int taps = 2, float tapInterval = 0.1f, float holdInterval = 1f)
        {
            this.tapInterval = tapInterval;
            this.taps = taps;
            this.holdInterval = holdInterval;
        }
        public InputActionWrapper(int taps = 2, float tapInterval = 0.1f)
        {
            this.tapInterval = tapInterval;
            this.taps = taps;
        }
        public InputActionWrapper(float holdInterval = 0f)
        {
            this.holdInterval = holdInterval;
        }

        public void Reset()
        {
            holdTimer = -1f;
            tapTimer = -1f;
            tapCount = 0;
        }
        public void SetTapInterval(float newInterval) { tapInterval = newInterval; }
        public void SetTaps(int newTapCount) { taps = newTapCount; }
        public void SetHoldInterval(float newInterval) { holdInterval = newInterval; }

        public float HoldF
        {
            get
            {
                if (holdInterval <= 0f) return -1f;
                return 1.0f - (holdTimer / holdInterval);
            }

        }
        public bool HasHolding() { return holdInterval > 0f; }
        public bool HasTaps() { return tapInterval > 0f && taps > 1; }
        public bool IsHolding() { return HoldF > 0f; }
        public bool IsHoldFinished() { return HoldF == 1f; }
        public bool IsMultiTapFinished() { return taps == tapCount; }
        public bool IsPressed() { return pressed; }
        public bool IsReleased() { return released; }


        public (bool holdFinished, bool tapFinished) Update(float dt, bool pressed, bool released)
        {
            this.pressed = pressed;
            this.released = released;

            bool holdFinished = false;
            bool tapFinished = false;

            if(HasHolding())
            {
                if (holdTimer > 0f)
                {
                    if (released) holdTimer = -1f;
                    else
                    {
                        holdTimer -= dt;
                        if (holdTimer <= 0f)
                        {
                            holdTimer = 0f;
                            holdFinished = true;
                        }
                    }
                }
                else
                {
                    if (pressed)
                    {
                        holdTimer = holdInterval - dt;
                    }
                }
            }

            if (HasTaps())
            {
                if (tapCount == taps)
                {
                    tapCount = 0;
                    tapTimer = -1f;
                }

                if (tapTimer > 0f)
                {
                    if (pressed) tapCount += 1;
                    else
                    {
                        tapTimer -= dt;
                        if (tapTimer <= 0f)
                        {
                            tapTimer = -1f;
                            tapCount = 0;
                        }
                    }
                }
                else
                {
                    if (pressed)
                    {
                        tapTimer = tapInterval - dt;
                        tapCount += 1;
                    }
                }

                if (tapCount == taps) tapFinished = true;
            }

            return (holdFinished, tapFinished);
        }

        /*
        public void Update(float dt)
        {
            pressed = InputHandler.IsPressed(playerSlot, inputAction);
            released = InputHandler.IsReleased(playerSlot, inputAction);

            
        }
        public void Update(float dt)
        {
            pressed = InputHandler.IsPressed(playerSlot, inputAction);
            released = InputHandler.IsReleased(playerSlot, inputAction);

            
        }
        */
    }
}
