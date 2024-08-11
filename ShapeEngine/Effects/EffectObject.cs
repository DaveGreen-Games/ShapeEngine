using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Timing;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Effects
{
    public abstract class EffectObject : GameObject
    {
        public TweenType TweenType { get; set; } = TweenType.LINEAR;
        public float LifetimeF { get { return 1f - lifetimeTimer.F; } }
        protected BasicTimer lifetimeTimer = new();


        public EffectObject(Vector2 pos, Size size)
        {
            Transform = new(pos, 0f, size);
        }

        public EffectObject(Vector2 pos, Size size, float lifeTime)
        {
            Transform = new(pos, 0f, size);
            lifetimeTimer.Start(lifeTime);
        }

        protected override bool TryKill(string? killMessage = null, GameObject? killer = null)
        {
            lifetimeTimer.Stop();
            return true;
        }
        
        public override Rect GetBoundingBox() { return new(Transform.Position, Transform.ScaledSize, new(0.5f)); }
        protected float GetTweenFloat(float start, float end) { return ShapeTween.Tween(start, end, LifetimeF, TweenType); }
        protected Vector2 GetTweenVector2(Vector2 start, Vector2 end) { return start.Tween(end, LifetimeF, TweenType); }
        protected ColorRgba GetTweenColor(ColorRgba startColorRgba, ColorRgba endColorRgba) { return startColorRgba.Tween(endColorRgba, LifetimeF, TweenType); }

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            lifetimeTimer.Update(time.Delta);
            if (lifetimeTimer.IsFinished) Kill();
        }

    }

    

}
