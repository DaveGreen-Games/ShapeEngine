using ShapeEngine;
using Demo;
using Timing;
using System.Numerics;

namespace ShapeEngineCore
{
    public class Skill
    {

        Vector2 location = new();
        bool active = false;
        StatSimple duration;
        StatSimple cooldown;
        StatSimple maxCharges;
        StatSimple rechargeTime;
        float rechargeTimer = 0f;
        int curCharges = 0;
        BasicTimer timer = new();
        SkillDisplay? skillDisplay;
        public event GameLoop.Triggered? OnTriggered;

        protected void OnActivated()
        {
            OnTriggered?.Invoke("activated");
        }
        protected void OnEnded()
        {
            OnTriggered?.Invoke("ended");
        }
        protected void OnRecharged()
        {
            OnTriggered?.Invoke("recharged");
        }

        public Skill(float duration, float cooldown, float rechargeTime, int charges = 1, SkillDisplay? skillDisplay = null)
        {
            this.duration = new(duration);
            this.cooldown = new(cooldown);
            maxCharges = new(charges);
            curCharges = (int)maxCharges.Cur;
            this.rechargeTime = new(rechargeTime);
            this.skillDisplay = skillDisplay;
        }

        public float F { get { return 1.0f - timer.GetF(); } }
        public float RechargeF { get { return 1.0f - rechargeTimer / rechargeTime.Cur; } }
        public float CurCharges { get { return curCharges; } }
        public bool HasCharges() { return curCharges > 0; }
        public bool AreChargesFull() { return curCharges >= maxCharges.Cur; }
        public bool IsActive() { return active; }
        public bool IsCooldownActive() { return !IsActive() && timer.IsRunning(); }
        public Vector2 Location { get { return location; } }
        public bool Activate(Vector2 pos)
        {
            if (IsActive()) return false;
            if (IsCooldownActive()) return false;
            if (!HasCharges()) return false;
            active = true;
            curCharges -= 1;
            StartRechargeTimer();
            timer.Start(duration.Cur);
            location = pos;
            return true;
        }
        public bool End()
        {
            if (IsActive())
            {
                active = false;
                timer.Start(cooldown.Cur * F);
                return true;
            }
            return false;
        }
        public void Update(float dt)
        {
            timer.Update(dt);
            if (timer.IsFinished())
            {
                if (IsActive())
                {
                    active = false;
                    timer.Start(cooldown.Cur);
                }
            }

            if (rechargeTimer > 0f)
            {
                rechargeTimer -= dt;
                if (rechargeTimer <= 0f)
                {
                    curCharges += 1;
                    if (!AreChargesFull())
                    {
                        StartRechargeTimer();
                    }
                }
            }
        }
        public void Draw()
        {

        }
        public void DrawUI()
        {

        }

        private void StartRechargeTimer()
        {
            if (rechargeTimer < 0)
            {
                rechargeTimer = rechargeTime.Cur - MathF.Abs(rechargeTimer);
            }
            else
            {
                rechargeTimer = rechargeTime.Cur;
            }
        }
    }
}
