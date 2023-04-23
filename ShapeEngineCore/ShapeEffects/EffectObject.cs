using ShapeCore;
using ShapeLib;
using ShapeTiming;
using System.Numerics;
using Raylib_CsLo;

namespace ShapeEffects
{
    /*
    //experiment----------------------------
    public abstract class DrawFuncBasic
    {
        public delegate void DrawFunc<T>(T t) where T : DrawFuncBasic;
    }
    public class DrawFuncExecuter<T> : DrawFuncBasic where T : DrawFuncBasic
    {
        public DrawFunc<T>? Func = null;
        public virtual bool Draw()
        {
            if (this is T t) Func?.Invoke(t);
            return false;
        }

    }
    //-------------------------------------
    */
    public abstract class EffectObject: IGameObject
    {
        public Vector2 Pos { get; set; }
        public Vector2 Size { get; protected set; }
        public TweenType TweenType { get; set; } = TweenType.LINEAR;
        public float LifetimeF { get { return 1f - lifetimeTimer.F; } }
        protected BasicTimer lifetimeTimer = new();

        public float DrawOrder { get; set; } = 0;
        public int AreaLayer { get; set; } = 0;
        public bool DrawToUI { get; set; } = false;
        public Vector2 ParallaxeOffset { get; set; } = new(0f);
        public float UpdateSlowFactor { get; set; } = 1f;
        public float UpdateSlowResistance { get; set; } = 1f;

        public EffectObject(Vector2 pos, Vector2 size) { this.Pos = pos; this.Size = size; }
        public EffectObject(Vector2 pos, Vector2 size, float lifeTime) { this.Pos = pos; this.Size = size; lifetimeTimer.Start(lifeTime); }


        public virtual bool Update(float dt)
        {
            if (IsDead()) return true;
            lifetimeTimer.Update(dt);
            return false;
        }
        public bool IsDead() { return lifetimeTimer.IsFinished; }

        public virtual Vector2 GetPosition() { return Pos; }
        public virtual Rect GetBoundingBox() { return new(Pos, Size, new(0.5f)); }
        protected float GetTweenFloat(float start, float end) { return STween.Tween(start, end, LifetimeF, TweenType); }
        protected Vector2 GetTweenVector2(Vector2 start, Vector2 end) { return start.Tween(end, LifetimeF, TweenType); }
        protected Color GetTweenColor(Color startColor, Color endColor) { return startColor.Tween(endColor, LifetimeF, TweenType); }
    }

    

}
