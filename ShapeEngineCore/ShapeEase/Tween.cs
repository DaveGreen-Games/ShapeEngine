
using Raylib_CsLo;
using ShapeCore;
using System.Numerics;

namespace ShapeEase
{
    public interface ISequenceable
    {
        public bool Update(float dt);
    }

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
        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            
            timer += dt;
            if(timer >= duration)
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
    
    //alternator class?


    public class Tween : ISequenceable
    {
        public delegate bool TweenFunc(float f);

        private TweenFunc func;
        private float duration;
        private float timer;
        private TweenType tweenType;

        public Tween(TweenFunc tweenFunc, float duration, TweenType tweenType)
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
            float f = STween.Tween(t, tweenType);

            timer += dt;
            return func(f) || t >= 1f;
        }


    }
    public class TweenVector2 : ISequenceable
    {
        public delegate bool TweenFunc(Vector2 result);

        private TweenFunc func;
        private float duration;
        private float timer;
        private TweenType tweenType;
        private Vector2 from;
        private Vector2 to;

        public TweenVector2(TweenFunc tweenFunc, Vector2 from, Vector2 to, float duration, TweenType tweenType)
        {
            this.func = tweenFunc;
            this.duration = duration;
            this.timer = 0f;
            this.tweenType = tweenType;
            this.from = from;
            this.to = to;
        }

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            timer += dt;
            Vector2 result = STween.Tween(from, to, t, tweenType);

            return func(result) || t >= 1f;
        }
    }
    public class TweenInt : ISequenceable
    {
        public delegate bool TweenFunc(int result);

        private TweenFunc func;
        private float duration;
        private float timer;
        private TweenType tweenType;
        private int from;
        private int to;

        public TweenInt(TweenFunc tweenFunc, int from, int to, float duration, TweenType tweenType)
        {
            this.func = tweenFunc;
            this.duration = duration;
            this.timer = 0f;
            this.tweenType = tweenType;
            this.from = from;
            this.to = to;
        }

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            timer += dt;
            int result = STween.Tween(from, to, t, tweenType);

            return func(result) || t >= 1f;
        }
    }
    public class TweenFloat : ISequenceable
    {
        public delegate bool TweenFunc(float result);

        private TweenFunc func;
        private float duration;
        private float timer;
        private TweenType tweenType;
        private float from;
        private float to;

        public TweenFloat(TweenFunc tweenFunc, float from, float to, float duration, TweenType tweenType)
        {
            this.func = tweenFunc;
            this.duration = duration;
            this.timer = 0f;
            this.tweenType = tweenType;
            this.from = from;
            this.to = to;
        }

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            timer += dt;
            float result = STween.Tween(from, to, t, tweenType);

            return func(result) || t >= 1f;
        }
    }
    public class TweenColor
    {
        public delegate bool TweenFunc(Color result);

        private TweenFunc func;
        private float duration;
        private float timer;
        private TweenType tweenType;
        private Color from;
        private Color to;

        public TweenColor(TweenFunc tweenFunc, Color from, Color to, float duration, TweenType tweenType)
        {
            this.func = tweenFunc;
            this.duration = duration;
            this.timer = 0f;
            this.tweenType = tweenType;
            this.from = from;
            this.to = to;
        }

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            timer += dt;
            Color result = STween.Tween(from, to, t, tweenType);

            return func(result) || t >= 1f;
        }
    }
    public class TweenRect
    {
        public delegate bool TweenFunc(Rect result);

        private TweenFunc func;
        private float duration;
        private float timer;
        private TweenType tweenType;
        private Rect from;
        private Rect to;

        public TweenRect(TweenFunc tweenFunc, Rect from, Rect to, float duration, TweenType tweenType)
        {
            this.func = tweenFunc;
            this.duration = duration;
            this.timer = 0f;
            this.tweenType = tweenType;
            this.from = from;
            this.to = to;
        }

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            timer += dt;
            Rect result = STween.Tween(from, to, t, tweenType);

            return func(result) || t >= 1f;
        }
    }


    public class Sequencer
    {
        public event Action<uint>? OnSequenceFinished;

        private Dictionary<uint, List<ISequenceable>> sequences = new();

        private static uint idCounter = 0;
        private static uint NextID { get { return idCounter++; } }

        public Sequencer() { }

        public uint StartSequence(params ISequenceable[] actionables)
        {
            var id = NextID;
            sequences.Add(id, actionables.Reverse().ToList());
            return id;
        }
        public void CancelSequence(uint id)
        {
            if(sequences.ContainsKey(id)) sequences.Remove(id);
        }
        public void Stop() { sequences.Clear(); }
        public void Update(float dt)
        {
            List<uint> remove = new();
            foreach(uint id in sequences.Keys)
            {
                var tweenList = sequences[id];
                if (tweenList.Count > 0)
                {
                    var tween = tweenList[tweenList.Count - 1];//list is reversed
                    var finished = tween.Update(dt);
                    if (finished) tweenList.RemoveAt(tweenList.Count - 1);
                }
                else
                {
                    remove.Add(id);
                    OnSequenceFinished?.Invoke(id);
                }
            }

            foreach(uint id in remove) sequences.Remove(id);
        }
    }

}


