using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using ShapeEngine.Timing;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Effects
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
    public abstract class EffectObject : IGameObject
    {
        public Vector2 Pos { get; set; }
        public Vector2 Size { get; protected set; }
        public TweenType TweenType { get; set; } = TweenType.LINEAR;
        public float LifetimeF { get { return 1f - lifetimeTimer.F; } }
        protected BasicTimer lifetimeTimer = new();

        public int Layer { get; set; } = 0;

        public EffectObject(Vector2 pos, Vector2 size) { this.Pos = pos; this.Size = size; }
        public EffectObject(Vector2 pos, Vector2 size, float lifeTime) { this.Pos = pos; this.Size = size; lifetimeTimer.Start(lifeTime); }

        public bool Kill()
        {
            if (IsDead()) return false;
            lifetimeTimer.Stop();
            return true;
        }
        public bool IsDead() { return lifetimeTimer.IsFinished; }

        public virtual Vector2 GetPosition() { return Pos; }
        public virtual Rect GetBoundingBox() { return new(Pos, Size, new(0.5f)); }
        protected float GetTweenFloat(float start, float end) { return ShapeTween.Tween(start, end, LifetimeF, TweenType); }
        protected Vector2 GetTweenVector2(Vector2 start, Vector2 end) { return start.Tween(end, LifetimeF, TweenType); }
        protected ShapeColor GetTweenColor(ShapeColor startColor, ShapeColor endColor) { return startColor.Tween(endColor, LifetimeF, TweenType); }

        /*
        public virtual bool HasBehaviors() { return false; }
        public virtual bool AddBehavior(IBehavior behavior) { return false; }
        public virtual bool RemoveBehavior(IBehavior behavior) { return false; }
        */
        public void AddedToHandler(GameObjectHandler gameObjectHandler)     {}
        public void RemovedFromHandler(GameObjectHandler gameObjectHandler) {}
        
        //public Vector2 GetCameraFollowPosition(Vector2 camPos) { return GetPosition(); }

        public virtual void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            lifetimeTimer.Update(dt);
        }
        public abstract void DrawGame(ScreenInfo game);
        public virtual void DrawGameUI(ScreenInfo ui) { }


        public virtual bool CheckHandlerBounds()
        {
            return false;
        }

        public virtual void LeftHandlerBounds(Vector2 safePosition, CollisionPoints collisionPoints)
        {
        }

        public virtual void DeltaFactorApplied(float f)
        {
            
        }

        
        public virtual bool DrawToGame(Rect gameArea)
        {
            return true;
        }
        public virtual bool DrawToGameUI(Rect uiArea)
        {
            return false;
        }
    }

    

}
