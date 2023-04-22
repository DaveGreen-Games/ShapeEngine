using System.Numerics;
using Raylib_CsLo;
using ShapeTiming;
using ShapeLib;
using System.Reflection.Metadata.Ecma335;

namespace ShapeCore
{
    public class Effect : IGameObject
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

        public Effect(Vector2 pos, Vector2 size) { this.Pos = pos; this.Size = size; }
        public Effect(Vector2 pos, Vector2 size, float lifeTime) { this.Pos = pos; this.Size = size; lifetimeTimer.Start(lifeTime); }


        public virtual void Update(float dt)
        {
            if (IsDead()) return;
            lifetimeTimer.Update(dt);
        }
        public bool IsDead()
        {
            return lifetimeTimer.IsFinished;
        }
        
        public virtual Vector2 GetPosition() { return Pos; }
        protected virtual Vector2 GetCurSize() { return Size.Tween(new Vector2(0f), LifetimeF, TweenType); }
        public virtual Rect GetBoundingBox() { return new(Pos, Size, new(0.5f)); }
    }
}
/*
    public class ShapeEffect : Effect
    {
        protected float rotRad = 0f;//radians
        protected float rotSpeed = 0f;
        
        public Color Color { get; set; } = WHITE;
        public ShapeEffect(Vector2 pos, Vector2 size, float duration, float rotSpeed = 0f) : base(pos, size, duration)
        {
            rotRad = SRNG.randF(2f * PI);
            this.rotSpeed = rotSpeed * SRNG.randF() < 0.5f ? 1f : -1f;
        }
        public ShapeEffect(Vector2 pos, Vector2 size, float duration, float rot = 0f, float rotSpeed = 0f) : base(pos, size, duration)
        {
            rotRad = rot;
            this.rotSpeed = rotSpeed * SRNG.randF() < 0.5f ? 1f : -1f;
        }
        public override void Update(float dt)
        {
            if (IsDead()) return;
            base.Update(dt);
            rotRad += rotSpeed * dt;
        }
         // return STween. return Size.Lerp(new(0f), 1f - lifetimeTimer.F); }// SUtils.LerpFloat(Size, 0f, 1.0f - lifetimeTimer.F); }
        
    }
    
    public class SquareEffect : ShapeEffect
    {
        public SquareEffect(Vector2 pos, Vector2 size, float duration, Color color, float rotSpeed = 0f) : base(pos, duration, size, color, rotSpeed) { }
        public void Draw()
        {
            if (IsDead()) return;
            var curSize = GetCurSize();
            Rect r = GetBoundingBox();
            DrawRectanglePro(r.Rectangle, curSize, rotRad * RAD2DEG, Color);
        }
    }
    public class CircleEffect : ShapeEffect
    {
        public CircleEffect(Vector2 pos, float duration, float radius, Color color) : base(pos, duration, radius, color, 0f) { }
        public override void Draw()
        {
            if (IsDead()) return;
            DrawCircleV(Pos, GetCurSize(), Color);
        }
    }
    public class LineEffect : ShapeEffect
    {
        float lineThickness = 4f;
        public LineEffect(Vector2 pos, float duration, float size, float thickness, Color color, float rot = 0f, float rotSpeed = 0f) : base(pos, duration, size, color, rot, rotSpeed)
        {
            lineThickness = thickness;
        }
        public LineEffect(Vector2 pos, float duration, float size, float thickness, Color color, float rotSpeed = 0f) : base(pos, duration, size, color, rotSpeed)
        {
            lineThickness = thickness;
        }
        public LineEffect(Vector2 pos, float duration, float size, float thickness, Color color) : base(pos, duration, size, color, 0f)
        {
            lineThickness = thickness;
        }
        public override void Draw()
        {
            if (IsDead()) return;
            DrawLineEx(Pos, Pos + SVec.Rotate(SVec.Right(), rotRad) * GetCurSize(), lineThickness, Color);
        }
        public override Rectangle GetBoundingBox()
        {
            Vector2 end = Pos + SVec.Rotate(SVec.Right() * size, rotRad);
            return new(Pos.X, Pos.Y, end.X - Pos.X, end.Y - Pos.Y);
        }
    }
    */

/*
public class SquareEffect : Effect
    {
        float size = 0f;
        float curSize = 0f;
        float rotation = 0f;
        bool second = false;
        float duration = 0f;
        Color color = WHITE;
        public SquareEffect(Vector2 pos, float duration, float size, Color color) : base(pos, duration * 0.8f)
        {
            this.duration = duration;
            this.size = size;
            this.curSize = size;
            this.color = color;
            this.rotation = RNG.randF(360f);
        }
        public override void Update(float dt)
        {
            base.Update(dt);

            if (lifetimeTimer.IsFinished() && !second)
            {
                lifetimeTimer.Start(duration * 0.2f);
                second = true;
            }
            if (second) curSize = Utils.LerpFloat(size, 0, 1.0f - lifetimeTimer.GetF());
        }

        public override void Draw()
        {
            Rectangle rect = new(pos.X, pos.Y, curSize * 2f, curSize * 2f);
            DrawRectanglePro(rect, new(curSize, curSize), rotation, color);
        }
        public override bool IsDead() { return second && lifetimeTimer.IsFinished(); }
    }
*/

/*
class AsteroidDeathEffect : GameObject
{
    private Color color = WHITE;
    private BasicTimer timer = new();
    private float duration = 0f;
    private Vector2 pos = new();
    private float size = 0f;
    private float curSize = 0f;
    private float rotation = 0f;
    private bool second = false;
    public AsteroidDeathEffect(Vector2 pos, float duration, float size, Color color)
    {
        drawOrder = 30;
        timer.Start(duration * 0.8f);
        this.duration = duration;
        this.size = size;
        this.curSize = size;
        this.pos = pos;
        this.color = color;
        this.rotation = RNG.randF(360f);
    }

    public override void Update(float dt)
    {
        timer.Update(dt);

        if (timer.IsFinished() && !second)
        {
            timer.Start(duration * 0.2f);
            second = true;
        }
        if (second) curSize = Utils.LerpFloat(size, 0, 1.0f - timer.GetF());
        //else rotation += dt * 450;
    }

    public override void Draw()
    {
        Rectangle rect = new(pos.X, pos.Y, curSize * 2f, curSize * 2f);
        //DrawRectangleRec(rect, color);
        DrawRectanglePro(rect, new(curSize, curSize), rotation, color);
    }
    public override bool IsDead() { return second && timer.IsFinished(); }
}
*/