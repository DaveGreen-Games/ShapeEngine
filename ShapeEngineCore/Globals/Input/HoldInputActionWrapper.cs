

namespace ShapeEngineCore.Globals.Input
{
    public class HoldInputActionWrapper
    {
        float timer = -1f;
        float interval = 1f;
        string inputAction = "";
        int playerSlot = -1;
        bool pressed = false;
        bool released = false;
        public HoldInputActionWrapper(string inputAction, float holdInterval = 1f, int playerSlot = -1)
        {
            this.inputAction = inputAction;
            this.interval = holdInterval;
            this.playerSlot = playerSlot;
        }

        public float HoldF
        {
            get
            {
                if (interval <= 0f) return -1f;
                return 1.0f - (timer / interval);
            }
            
        }
        public bool IsHolding() { return HoldF > 0f; }
        public bool IsHoldFinished() { return HoldF == 1f; }
        public bool IsPressed() { return pressed; }
        public bool IsReleased() { return released; }


        public void Update(float dt)
        {
            pressed = InputHandler.IsPressed(playerSlot, inputAction);
            released = InputHandler.IsReleased(playerSlot, inputAction);

            if(timer > 0f)
            {
                if (released) timer = -1f;
                else
                {
                    timer -= dt;
                    if (timer <= 0f) timer = 0f;
                }
            }
            else
            {
                if (pressed)
                {
                    timer = interval - dt;
                }
            }
        }
    }

    
}
