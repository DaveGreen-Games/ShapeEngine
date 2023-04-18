/*
using ShapeLib;

namespace ShapeEase
{
    public class EaseOrder
    {
        dynamic change = 0f;
        float duration = 0f;
        float timer = 0f;
        float f = 0f;
        EasingType easingType = EasingType.LINEAR_NONE;

        public EaseOrder(float duration, dynamic change, EasingType easingType = EasingType.LINEAR_NONE)
        {
            this.change = change;
            this.duration = duration;
            timer = duration;
            this.easingType = easingType;
        }

        public dynamic GetValue(dynamic start) { return SUtils.LerpDynamic(start, start + change, f); }
        public bool IsFinished() { return duration > 0 && timer <= 0; }
        public void Update(float dt)
        {
            if (timer > 0f && duration > 0f)
            {
                timer -= MathF.Min(dt, timer);
                f = SEase.Simple(1.0f - timer / duration, easingType);
            }
        }
    }

}
*/