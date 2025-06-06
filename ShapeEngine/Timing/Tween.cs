using ShapeEngine.Core;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Timing
{
    /// <summary>
    /// Represents a generic tween operation that interpolates a value over time.
    /// </summary>
    public class Tween : ISequenceable
    {
        /// <summary>
        /// Delegate for the tween function, called with the interpolated value.
        /// </summary>
        /// <param name="f">The interpolated value (0 to 1).</param>
        /// <returns>True if the tween should finish, otherwise false.</returns>
        public delegate bool TweenFunc(float f);

        private readonly TweenFunc func;
        private readonly float duration;
        private readonly TweenType tweenType;
        private float timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tween"/> class.
        /// </summary>
        /// <param name="tweenFunc">The function to call with the tweened value.</param>
        /// <param name="duration">The duration of the tween in seconds.</param>
        /// <param name="tweenType">The type of tweening to use.</param>
        public Tween(TweenFunc tweenFunc, float duration, TweenType tweenType)
        {
            this.func = tweenFunc;
            this.duration = duration;
            this.timer = 0f;
            this.tweenType = tweenType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tween"/> class by copying another instance.
        /// </summary>
        /// <param name="tween">The tween to copy.</param>
        public Tween(Tween tween)
        {
            this.func = tween.func;
            this.duration = tween.duration;
            this.timer = tween.timer;
            this.tweenType = tween.tweenType;
        }

        /// <summary>
        /// Creates a copy of this tween.
        /// </summary>
        /// <returns>A new <see cref="Tween"/> instance with the same parameters.</returns>
        public ISequenceable Copy() => new Tween(this);

        /// <summary>
        /// Updates the tween by the given delta time.
        /// </summary>
        /// <param name="dt">The time in seconds since the last update.</param>
        /// <returns>True if the tween is finished or if the timer exceeds the duration, otherwise false.</returns>
        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = ShapeMath.Clamp(timer / duration, 0f, 1f);
            float f = ShapeTween.Tween(t, tweenType);

            timer += dt;
            return func(f) || t >= 1f;
        }

        /// <summary>
        /// Gets the tween function delegate.
        /// </summary>
        public TweenFunc Func => func;

        /// <summary>
        /// Gets the duration of the tween in seconds.
        /// </summary>
        public float Duration => duration;

        /// <summary>
        /// Gets the tween type.
        /// </summary>
        public TweenType TweenType => tweenType;

        /// <summary>
        /// Gets the current timer value.
        /// </summary>
        public float Timer => timer;
    }
}