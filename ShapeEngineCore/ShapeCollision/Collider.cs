
using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals;
using ShapeLib;

namespace ShapeCollision
{
    //collider class now holds everything physics related (force, vel, drag, pos, etc)
    //new shape class will be added that contains everything related to the shape for collision detection
    //collider has a list of shapes (compound collider) with offset per shape -> figure out how to have accurate world position
    //for each shape (collider.Pos + shape.Offset) when used in collision detection

    public class Collider
    {
        public Collider() { }
        public Collider(float x, float y) { Pos = new(x, y); }
        public Collider(Vector2 pos, Vector2 vel) { Pos = pos; Vel = vel; }


        public float Mass { get; set; } = 1.0f;
        public Vector2 Vel { get; set; }
        public Vector2 Pos { get; set; }

        protected Vector2 accumulatedForce = Vector2.Zero;
        protected bool enabled = true;
        public bool CheckCollision = true;
        public bool CheckIntersections = false;
        public bool CheckClosestPoint = false;
        public bool HasAccumulatedForce() { return accumulatedForce.X != 0f || accumulatedForce.Y != 0f; }
        public Vector2 GetAccumulatedForce() { return accumulatedForce; }
        public void AccumulateForce(Vector2 force)
        {
            accumulatedForce += force / Mass;
        }
        public void ApplyAccumulatedForce(float dt)
        {
            Vel += accumulatedForce * dt;
            accumulatedForce = Vector2.Zero;
        }
        public void AddImpulse(Vector2 force)
        {
            if (Mass <= 0.0f)
            {
                Vel += force;
            }
            else
            {
                Vel = Vel + force / Mass;
            }
        }
        public bool IsEnabled() { return enabled; }
        public void Enable() { enabled = true; }
        public void Disable() { enabled = false; }
        public bool IsStatic() { return Vel.X == 0.0f && Vel.Y == 0.0f; }

        public virtual Rectangle GetBoundingRect() { return new(Pos.X, Pos.Y, 1.0f, 1.0f); }
        public virtual void DebugDrawShape(Color color) { Raylib.DrawCircleV(Pos, 5.0f, color); }
    }
    public class CircleCollider : Collider
    {
        public CircleCollider() { }
        public CircleCollider(float x, float y, float r) : base(x, y) { Radius = r; }
        public CircleCollider(Vector2 pos, float r) : base(pos, new(0.0f, 0.0f)) { Radius = r; }
        public CircleCollider(Vector2 pos, Vector2 vel, float r) : base(pos, vel) { Radius = r; }

        private float radius = 0.0f;
        public float Radius { get { return radius; } set { radius = value; RadiusSquared = value * value; } }
        public float RadiusSquared { get; private set; }

        public float GetArea() { return MathF.PI * RadiusSquared; }
        public float GetCircumference() { return MathF.PI * Radius * 2.0f; }

        public override Rectangle GetBoundingRect() { return new(Pos.X - radius, Pos.Y - radius, radius * 2.0f, radius * 2.0f); }
        public override void DebugDrawShape(Color color) 
        { 
            //Raylib.DrawCircleV(Pos, Radius, color);
            Drawing.DrawCircleLines(Pos, Radius, 5f, color, 4f);
        }

    }
    public class SegmentCollider : Collider
    {
        public SegmentCollider() { }
        public SegmentCollider(Vector2 start, Vector2 end) : base(start, new(0.0f, 0.0f))
        {
            Vector2 v = end - start;
            Dir = Vector2.Normalize(v);
            Length = v.Length();
        }
        public SegmentCollider(Vector2 start, Vector2 dir, float length) : base(start, new(0.0f, 0.0f)) { Dir = dir; Length = length; }

        public Vector2 Dir { get; set; }
        public float Length { get; set; }
        public Vector2 Start { get { return Pos; } }
        public Vector2 End { get { return Pos + Dir * Length; } }
        public Vector2 Displacement { get { return End - Pos; } }

        public override Rectangle GetBoundingRect()
        {
            Vector2 end = End;
            float topLeftX = MathF.Min(Pos.X, end.X);
            float topLeftY = MathF.Min(Pos.Y, end.Y);
            float bottomRightX = MathF.Max(Pos.X, end.X);
            float bottomRightY = MathF.Max(Pos.Y, end.Y);
            return new(topLeftX, topLeftY, bottomRightX - topLeftX, bottomRightY - topLeftY);
        }
        public override void DebugDrawShape(Color color)
        {
            Raylib.DrawCircleV(Pos, 5.0f, color);
            Raylib.DrawLineEx(Pos, End, 5f, color);
        }
    }
    public class RectCollider : Collider
    {
        public RectCollider() { }
        public RectCollider(float x, float y, float w, float h, Vector2 alignement)
        {
            this.Pos = new(x, y);
            this.Size = new(w, h);
            this.Alignement = alignement;
        }
        public RectCollider(Vector2 pos, Vector2 size, Vector2 alignement)
        {
            this.Pos = pos;
            this.Size = size;
            this.Vel = new(0.0f, 0.0f);
            this.Alignement = alignement;
        }
        public RectCollider(Vector2 pos, Vector2 vel, Vector2 size, Vector2 alignement)
        {
            this.Pos = pos;
            this.Size = size;
            this.Vel = vel;
            this.Alignement = alignement;
        }
        public RectCollider(Rectangle rect)
        {
            this.Pos = new(rect.x, rect.y);
            this.Size = new(rect.width, rect.height);
            this.Vel = new(0f);
            this.Alignement = new(0f);
        }
        public RectCollider(Rectangle rect, Vector2 vel)
        {
            this.Pos = new(rect.x, rect.y);
            this.Size = new(rect.width, rect.height);
            this.Vel = vel;
            this.Alignement = new(0f);
        }
        public Vector2 Alignement { get; set; }
        public Vector2 Size { get; set; }
        public Rectangle Rect 
        { 
            get
            {
                return SRect.ConstructRect(Pos, Size, Alignement);
            } 
        }
        public List<Vector2> GetCornersList() { return SRect.GetRectCornersList(Rect); }
        public (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) GetCorners() { return SRect.GetRectCorners(Rect); }
        public override Rectangle GetBoundingRect() { return Rect; }
        public override void DebugDrawShape(Color color)
        {
            //Raylib.DrawRectangleRec(Rect, color);
            Raylib.DrawRectangleLinesEx(Rect, 5f, color);
        }

        //public static Rectangle GetCorrectRect(float x, float y, float w, float h)
        //{
        //    float p1x = x;
        //    float p1y = y;
        //    float p2x = x + w;
        //    float p2y = y + h;
        //
        //    float topLeftX = MathF.Min(p1x, p2x);
        //    float topLeftY = MathF.Min(p1y, p2y);
        //    float bottomRightX = MathF.Max(p1x, p2x);
        //    float bottomRightY = MathF.Max(p1y, p2y);
        //    return new(topLeftX, topLeftY, bottomRightX - topLeftX, bottomRightY - topLeftY);
        //}

    }
    public class PolyCollider : Collider
    {
        List<Vector2> points;
        public float RotRad { get; set; }
        public List<Vector2> Shape
        {
            get
            {
                List<Vector2> shape = new();
                for (int i = 0; i < points.Count; i++)
                {
                    shape.Add(Pos + SVec.Rotate(points[i], RotRad));
                }
                return shape;
            }
        }
        public PolyCollider(float x, float y, List<Vector2> points, float rotRad = 0f) : base(x, y)
        {
            this.RotRad = rotRad;
            this.points = points;
            
        }
        public PolyCollider(Vector2 pos, List<Vector2> points, float rotRad = 0f) : base(pos, new(0f))
        {
            this.RotRad = rotRad;
            this.points = points;
        }
        public PolyCollider(Vector2 pos, Vector2 vel, List<Vector2> points, float rotRad = 0f) : base(pos, vel)
        {
            this.RotRad = rotRad;
            this.points = points;
        }

        public override Rectangle GetBoundingRect()
        {
            return SPoly.GetPolyBoundingBox(Shape);
        }

        public override void DebugDrawShape(Color color)
        {
            Drawing.DrawPolygon(Shape, 5f, color);
        }
    }

}
