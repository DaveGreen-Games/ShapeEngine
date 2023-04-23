using System.Numerics;
using ShapeLib;
using ShapeCore;

namespace ShapeEffects
{
    public abstract class Particle : EffectObject // IGameObject
    {
        //public delegate void Draw(Color color);
        //public delegate void DrawParticle(Particle p);
        //public DrawParticle? DrawParticleFunc = null;

        public Vector2 Vel { get; set; } = new(0f);

        public Particle(Vector2 pos, Vector2 size) : base(pos, size) { }
        public Particle(Vector2 pos, Vector2 size, float lifetime) : base(pos, size, lifetime) { }
        public Particle(Vector2 pos, Vector2 size, Vector2 vel, float lifetime) : base(pos, size, lifetime) { Vel = vel; }
        public Particle(Vector2 pos, Vector2 size, float angleDeg, float lifetime) : base(pos, size, lifetime) { Vel = SVec.Rotate(SVec.Right(), angleDeg * SUtils.DEGTORAD); }

        
        //public virtual bool Draw()
        //{
        //    if (IsDead()) return true;
        //    if (DrawParticleFunc != null) DrawParticleFunc(this);
        //    return false ;
        //}
    }
    

    /*

    public class LineParticle : Particle
    {
        private float size = 1f;
        private float rotRad = 0f;
        private float lineThickness = 1f;
        private Color color;

        public LineParticle(Vector2 pos, float speed, Color color, float size, float lifetime) : base(pos, lifetime)
        {
            float angle = SRNG.randF(0f, 2f * PI);
            rotRad = angle;
            Vel = SVec.Rotate(SVec.Right() * speed, angle);
            this.size = size;
            this.color = color;
        }
        public LineParticle(Vector2 pos, float speed, Color color, float size, float lifetime, float lineThickness = 1f) : base(pos, lifetime)
        {
            float angle = SRNG.randF(0f, 2f * PI);
            rotRad = angle;
            Vel = SVec.Rotate(SVec.Right() * speed, angle);
            this.size = size;
            this.color = color;
            this.lineThickness = lineThickness;
        }
        public LineParticle(Vector2 pos, float speed, Color color, float size, float lifetime, float lineThickness = 1f, float drag = 2f) : base(pos, lifetime)
        {
            float angle = SRNG.randF(0f, 2f * PI);
            rotRad = angle;
            Vel = SVec.Rotate(SVec.Right() * speed, angle);
            this.size = size;
            this.color = color;
            this.lineThickness = lineThickness;
            this.Drag = drag;
        }
        public LineParticle(Vector2 pos, float angleRad, float speed, Color color, float size, float lifetime, float lineThickness = 1f) : base(pos, lifetime)
        {
            rotRad = angleRad;
            Vel = SVec.Rotate(SVec.Right() * speed, rotRad);
            this.size = size;
            this.color = color;
            this.lineThickness = lineThickness;
        }
        public LineParticle(Vector2 pos, float angleRad, float speed, Color color, float size, float lifetime, float lineThickness = 1f, float drag = 2f) : base(pos, lifetime)
        {
            rotRad = angleRad;
            Vel = SVec.Rotate(SVec.Right() * speed, rotRad);
            this.size = size;
            this.color = color;
            this.lineThickness = lineThickness;
            this.Drag = drag;
        }
        public LineParticle(Vector2 pos, float angleRad, float accRad, float speed, Color color, float size, float lifetime, float lineThickness = 1f) : base(pos, lifetime)
        {
            rotRad = angleRad + SRNG.randF(-accRad, accRad);
            Vel = SVec.Rotate(SVec.Right() * speed, rotRad);
            this.size = size;
            this.color = color;
            this.lineThickness = lineThickness;
        }
        public override void Draw()
        {
            DrawLineEx(pos, pos + SVec.Rotate(SVec.Right() * size, rotRad), lineThickness, color);
        }
        public override Rectangle GetBoundingBox()
        {
            Vector2 end = pos + SVec.Rotate(SVec.Right() * size, rotRad);
            return new(pos.X, pos.Y, end.X - pos.X, end.Y - pos.Y);
        }
    }
    public class CircleParticle : Particle
    {
        private float r = 1f;
        private Color color;

        public CircleParticle(Vector2 pos, float speed, Color color, float radius, float lifetime) : base(pos, lifetime)
        {
            float angle = SRNG.randF(0f, 2f * PI);
            Vel = SVec.Rotate(SVec.Right() * speed, angle);
            r = radius;
            this.color = color;
        }
        public CircleParticle(Vector2 pos, float angleRad, float speed, Color color, float radius, float lifetime) : base(pos, lifetime)
        {
            Vel = SVec.Rotate(SVec.Right() * speed, angleRad);
            r = radius;
            this.color = color;
        }
        public CircleParticle(Vector2 pos, float speed, Color color, float radius, float lifetime, float drag = 2f) : base(pos, lifetime)
        {
            float angle = SRNG.randF(0f, 2f * PI);
            Vel = SVec.Rotate(SVec.Right() * speed, angle);
            r = radius;
            this.color = color;
            this.Drag = drag;
        }
        public CircleParticle(Vector2 pos, float angleRad, float speed, Color color, float radius, float lifetime, float drag = 2f) : base(pos, lifetime)
        {
            Vel = SVec.Rotate(SVec.Right() * speed, angleRad);
            r = radius;
            this.color = color;
            this.Drag = drag;
        }
        public CircleParticle(Vector2 pos, float angleRad, float accRad, float speed, Color color, float radius, float lifetime) : base(pos, lifetime)
        {
            Vel = SVec.Rotate(SVec.Right() * speed, angleRad + SRNG.randF(-accRad, accRad));
            r = radius;
            this.color = color;
        }
        public override void Draw()
        {
            DrawCircle((int)pos.X, (int)pos.Y, r, color);
        }
        public override Rectangle GetBoundingBox()
        {
            return new(pos.X - r, pos.Y - r, r * 2, r * 2);
        }
    }

    */

}


