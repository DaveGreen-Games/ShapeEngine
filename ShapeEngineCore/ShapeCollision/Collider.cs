
using System.Numerics;
using Raylib_CsLo;
using ShapeCore;
using ShapeLib;

namespace ShapeCollision
{
    //collider class now holds everything physics related (force, vel, drag, pos, etc)
    //new shape class will be added that contains everything related to the shape for collision detection
    //collider has a list of shapes (compound collider) with offset per shape -> figure out how to have accurate world position
    //for each shape (collider.Pos + shape.Offset) when used in collision detection

    //collider has list of shapes
    //rename circle/segment/rect/poly collider to shape

    
    public interface ICollider
    {
        public float Mass { get; set; }
        public Vector2 Vel { get; set; }
        public Vector2 Pos { get; set; }
        public bool Enabled { get; set; }
        public bool CheckCollision { get; set; }
        public bool CheckIntersections { get; set; }
        public sealed bool IsStatic() { return Vel.X == 0.0f && Vel.Y == 0.0f; }
        public Vector2 GetAccumulatedForce();
        public void AccumulateForce(Vector2 force);
        public void ApplyAccumulatedForce(float dt);
        public void AddImpulse(Vector2 force);
        public Rect GetBoundingBox();
        public void DrawDebugShape(Color color);
        public bool Overlap(ICollider other);
        public bool OverlapRect(Rect other);
        public Intersection Intersect(ICollider other);


        //public sealed Intersection Intersect(Collider other)
        //{
        //    return CheckIntersections ? CheckIntersection(other) : new();
        //}

    }
    
    public abstract class Collider : ICollider
    {
        public Collider() { }
        public Collider(float x, float y) { Pos = new(x, y); }
        public Collider(Vector2 pos, Vector2 vel) { Pos = pos; Vel = vel; }


        public float Mass { get; set; } = 1.0f;
        public Vector2 Vel { get; set; }
        public Vector2 Pos { get; set; }

        protected Vector2 accumulatedForce = new(0f);
        public bool Enabled { get; set; } = true;
        public bool CheckCollision { get; set; } = true;
        public bool CheckIntersections { get; set; } = false;
        public bool IsStatic() { return Vel.X == 0.0f && Vel.Y == 0.0f; }
        public bool HasAccumulatedForce() { return accumulatedForce.X != 0f || accumulatedForce.Y != 0f; }
        public Vector2 GetAccumulatedForce() { return accumulatedForce; }
        public void AccumulateForce(Vector2 force)
        {
            if (Mass <= 0) accumulatedForce += force;
            else accumulatedForce += force / Mass;
        }
        public void ApplyAccumulatedForce(float dt)
        {
            Vel += accumulatedForce * dt;
            accumulatedForce = Vector2.Zero;
        }
        public void AddImpulse(Vector2 force)
        {
            if (Mass <= 0.0f) Vel += force;
            else Vel = Vel + force / Mass;
        }
        



        //public Intersection Intersect(Collider other)
        //{
        //    return CheckIntersections ? CheckIntersection(other) : new();
        //}

        public abstract Rect GetBoundingBox();
        public abstract void DrawDebugShape(Color color);
        public abstract bool Overlap(ICollider other);
        public abstract bool OverlapRect(Rect other);
        public abstract Intersection Intersect(ICollider other);

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

        public override Rect GetBoundingBox() { return new(Pos.X - radius, Pos.Y - radius, radius * 2.0f, radius * 2.0f); }
        //public override float GetBoundingRadius()
        //{
        //    return Radius;
        //}
        public override void DrawDebugShape(Color color) 
        { 
            //Raylib.DrawCircleV(Pos, Radius, color);
            SDrawing.DrawCircleLines(Pos, Radius, 5f, color, 4f);
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
        public Vector2 Center { get { return Pos + Dir * Length / 2; } }
        public Vector2 End { get { return Pos + Dir * Length; } }
        public Vector2 Displacement { get { return End - Pos; } }

        public override Rect GetBoundingBox()
        {
            Vector2 end = End;
            float topLeftX = MathF.Min(Pos.X, end.X);
            float topLeftY = MathF.Min(Pos.Y, end.Y);
            float bottomRightX = MathF.Max(Pos.X, end.X);
            float bottomRightY = MathF.Max(Pos.Y, end.Y);
            return new(topLeftX, topLeftY, bottomRightX - topLeftX, bottomRightY - topLeftY);
        }
        public override void DrawDebugShape(Color color)
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
        public Rect Rect 
        { 
            get
            {
                return new(Pos, Size, Alignement);
            } 
        }
        public List<Vector2> GetCornersList() { return SRect.GetRectCornersList(Rect); }
        public (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) GetCorners() { return SRect.GetRectCorners(Rect); }
        public override Rect GetBoundingBox() { return Rect; }
       
        public override void DrawDebugShape(Color color)
        {
            Raylib.DrawRectangleLinesEx(Rect.Rectangle, 5f, color);
        }

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

        public override Rect GetBoundingBox()
        {
            return SPoly.GetPolyBoundingBox(Shape);
        }
        public override void DrawDebugShape(Color color)
        {
            SDrawing.DrawPolygon(Shape, 5f, color);
        }
    }

}
