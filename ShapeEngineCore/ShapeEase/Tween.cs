
using ShapeLib;

namespace ShapeEase
{
    public interface IActionable
    {
        public bool Update(float dt);
    }

    public class Repeater : IActionable
    {
        private Action action;
        private float timer;
        private float duration;
        private int remainingRepeats;

        public Repeater(Action action, float duration, int repeats = 0)
        {
            this.action = action;
            this.duration = duration;
            this.timer = 0f;
            this.remainingRepeats = repeats;
        }
        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            
            timer += dt;
            if(timer >= duration)
            {
                action();
                if (remainingRepeats > 0)
                {
                    timer = timer - duration; //in case timer over shot
                    remainingRepeats--;
                }
            }
            return timer >= duration && remainingRepeats <= 0;
        }
    }
    public class Actionable : IActionable
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
    public class Tween : IActionable
    {
        public delegate bool TweenFunc(float f);

        private TweenFunc func;
        private float duration;
        private float timer;
        private EasingType tweenType;

        public Tween(TweenFunc tweenFunc, float duration, EasingType tweenType)
        {
            this.func = tweenFunc;
            this.duration = duration;
            this.timer = 0f;
            this.tweenType = tweenType;
        }

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            float f = SEase.Simple(t, tweenType);

            timer += dt;
            return func(f) || t >= 1f;
        }


    }
    
    //i need a better name!!!!!
    public class Tweener
    {
        private Dictionary<uint, List<IActionable>> actionableStack = new();

        private uint idCounter = 0;
        private uint NextID { get { return idCounter++; } }

        public Tweener() { }

        public uint Start(params IActionable[] actionables)
        {
            var id = NextID;
            actionableStack.Add(id, actionables.Reverse().ToList());
            return id;
        }
        public void Cancel(uint id)
        {
            if(actionableStack.ContainsKey(id)) actionableStack.Remove(id);
        }
        public void Update(float dt)
        {
            List<uint> remove = new();
            foreach(uint id in actionableStack.Keys)
            {
                var tweenList = actionableStack[id];
                if(tweenList.Count > 0)
                {
                    var tween = tweenList[tweenList.Count - 1];//list is reversed
                    var finished = tween.Update(dt);
                    if (finished) tweenList.RemoveAt(tweenList.Count - 1);
                }
                else remove.Add(id);
            }
        }
    }

}
