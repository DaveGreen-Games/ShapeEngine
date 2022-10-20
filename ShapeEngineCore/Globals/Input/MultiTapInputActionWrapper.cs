

namespace ShapeEngineCore.Globals.Input
{
    public class MultiTapInputActionWrapper
    {
        float timer = -1f;
        float interval = 1f;
        int taps = 2; //double tap
        int curTap = 0;
        string inputAction = "";
        int playerSlot = -1;
        bool pressed = false;
        bool released = false;
        public MultiTapInputActionWrapper(string inputAction, int taps = 2, float interval = 0.1f, int playerSlot = -1)
        {
            this.inputAction = inputAction;
            this.interval = interval;
            this.playerSlot = playerSlot;
            this.taps = taps;
        }


        public bool IsMultiTapFinished() { return taps == curTap; }
        public bool IsPressed() { return pressed; }
        public bool IsReleased() { return released; }


        public void Update(float dt)
        {
            pressed = InputHandler.IsPressed(playerSlot, inputAction);
            released = InputHandler.IsReleased(playerSlot, inputAction);

            if (curTap == taps)
            {
                curTap = 0;
                timer = -1f;
            }

            if (timer > 0f)
            {
                if (pressed) curTap += 1;
                else
                {
                    timer -= dt;
                    if (timer <= 0f)
                    {
                        timer = -1f;
                        curTap = 0;
                    }
                }
            }
            else
            {
                if (pressed)
                {
                    timer = interval - dt;
                    curTap += 1;
                }
            }
        }
    }
}
