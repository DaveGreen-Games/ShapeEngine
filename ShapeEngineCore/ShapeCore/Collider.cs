
using System.Numerics;
using Raylib_CsLo;
using ShapeLib;

namespace ShapeCore
{
    public interface ICollider : IPhysicsObject
    {
        public bool Enabled { get; set; }
        public bool ComputeCollision { get; set; }
        public bool ComputeIntersections { get; set; }
        
        public Rect GetBoundingBox();
        public void DrawDebugShape(Color color);
        public (bool valid, bool overlap) CheckOverlap(ICollider other);
        public (bool valid, Intersection i) CheckIntersection(ICollider other);
        public bool CheckOverlapRect(Rect rect);
    }
    
    public abstract class Collider : ICollider
    {
        public Collider() { }
        public Collider(float x, float y) { Pos = new(x, y); }
        public Collider(Vector2 pos, Vector2 vel) { Pos = pos; Vel = vel; }

        public float Mass { get; set; } = 1.0f;
        public Vector2 Vel { get; set; }
        public Vector2 Pos { get; set; }
        public Vector2 ConstAcceleration { get; set; } = new(0f);
        public float Drag { get; set; } = 0f;
        public bool Enabled { get; set; } = true;
        public bool ComputeCollision { get; set; } = true;
        public bool ComputeIntersections { get; set; } = false;
        public abstract Rect GetBoundingBox();
        public abstract void DrawDebugShape(Color color);
        public abstract (bool valid, bool overlap) CheckOverlap(ICollider other);
        public abstract (bool valid, Intersection i) CheckIntersection(ICollider other);
        public abstract bool CheckOverlapRect(Rect other);

        
        protected Vector2 accumulatedForce = new(0f);
        public Vector2 GetAccumulatedForce() { return accumulatedForce; }
        public void ClearAccumulatedForce() { accumulatedForce = new(0f); }
        public void AddForce(Vector2 force) { accumulatedForce = SPhysics.AddForce(this, force); }
        public void AddImpulse(Vector2 force) { SPhysics.AddImpuls(this, force); }
        public void UpdateState(float dt) { SPhysics.UpdateState(this, dt); }

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
        public override void DrawDebugShape(Color color) 
        { 
            //Raylib.DrawCircleV(Pos, Radius, color);
            SDrawing.DrawCircleLines(Pos, Radius, 5f, color, 4f);
        }

        public override (bool valid, bool overlap) CheckOverlap(ICollider other)
        {
            
            if (other is CircleCollider c)
            {
                return (true, this.OverlapCircleCircle(c));
            }
            else if (other is SegmentCollider s)
            {
                return (true, this.OverlapCircleSegment(s));
            }
            else if (other is RectCollider r)
            {
                return (true, this.OverlapCircleRect(r));
            }
            else if (other is PolyCollider p)
            {
                return (true, this.OverlapCirclePoly(p));
            }
            return (false, false);
        }
        public override (bool valid, Intersection i) CheckIntersection(ICollider other)
        {

            if (other is CircleCollider c)
            {
                return (true, this.IntersectionCircleCircle(c));
            }
            else if (other is SegmentCollider s)
            {
                return (true, this.IntersectionCircleSegment(s));
            }
            else if (other is RectCollider r)
            {
                return (true, this.IntersectionCircleRect(r));
            }
            else if (other is PolyCollider p)
            {
                return (true, this.IntersectionCirclePoly(p));
            }
            return (false, new());
        }
        public override bool CheckOverlapRect(Rect rect) { return rect.OverlapRectCircle(this); }
    
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
        public override (bool valid, bool overlap) CheckOverlap(ICollider other)
        {
            if (other is CircleCollider c)
            {
                return (true, this.OverlapSegmentCircle(c));
            }
            else if (other is SegmentCollider s)
            {
                return (true, this.OverlapSegmentSegment(s));
            }
            else if (other is RectCollider r)
            {
                return (true, this.OverlapSegmentRect(r));
            }
            else if (other is PolyCollider p)
            {
                return (true, this.OverlapSegmentPoly(p));
            }
            return (false, false);
        }
        public override (bool valid, Intersection i) CheckIntersection(ICollider other)
        {
            if (other is CircleCollider c)
            {
                return (true, this.IntersectionSegmentCircle(c));
            }
            else if (other is SegmentCollider s)
            {
                return (true, this.IntersectionSegmentSegment(s));
            }
            else if (other is RectCollider r)
            {
                return (true, this.IntersectionSegmentRect(r));
            }
            else if (other is PolyCollider p)
            {
                return (true, this.IntersectionSegmentPoly(p));
            }
            return (false, new());
        }
        public override bool CheckOverlapRect(Rect rect) { return rect.OverlapRectSegment(this); }

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
        public RectCollider(Rect rect)
        {
            this.Pos = new(rect.x, rect.y);
            this.Size = new(rect.width, rect.height);
            this.Vel = new(0f);
            this.Alignement = new(0f);
        }
        public RectCollider(Rect rect, Vector2 vel)
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
            get { return new(Pos, Size, Alignement); }
        }
        //public List<Vector2> GetCornersList() { return SRect.GetRectCornersList(Rect); }
        //public (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) GetCorners() { return SRect.GetRectCorners(Rect); }
        public override Rect GetBoundingBox() { return Rect; }
        public override void DrawDebugShape(Color color)
        {
            Raylib.DrawRectangleLinesEx(Rect.Rectangle, 5f, color);
        }
        public override (bool valid, bool overlap) CheckOverlap(ICollider other)
        {
            if (other is CircleCollider c)
            {
                return (true, this.OverlapRectCircle(c));
            }
            else if (other is SegmentCollider s) 
            {
                return (true, this.OverlapRectSegment(s));
            }
            else if (other is RectCollider r)
            {
                return (true, this.OverlapRectRect(r));
            }
            else if (other is PolyCollider p)
            {
                return (true, this.OverlapRectPoly(p));
            }
            return (false, false);
        }
        public override (bool valid, Intersection i) CheckIntersection(ICollider other)
        {

            if (other is CircleCollider c)
            {
                return (true, this.IntersectionRectCircle(c));
            }
            else if (other is SegmentCollider s)
            {
                return (true, this.IntersectionRectSegment(s));
            }
            else if (other is RectCollider r)
            {
                return (true, this.IntersectionRectRect(r));
            }
            else if (other is PolyCollider p)
            {
                return (true, this.IntersectionRectPoly(p));
            }
            return (false, new());
        }
        public override bool CheckOverlapRect(Rect rect) { return  rect.OverlapRectRect(this); }

    }
    public class PolyCollider : Collider
    {
        List<Vector2> points;
        public float RotRad { get; set; }
        public float Scale { get; set; } = 1f;
        public List<Vector2> Shape { get { return SPoly.TransformPoly(points, Pos, RotRad, new(Scale)); } }
        //{
        //    get
        //    {
        //        List<Vector2> shape = new();
        //        for (int i = 0; i < points.Count; i++)
        //        {
        //            shape.Add(Pos + SVec.Rotate(points[i], RotRad));
        //        }
        //        return shape;
        //    }
        //}
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
        public PolyCollider(Rect rect, float rotRad = 0f) : base(rect.Center, new(0f))
        {
            this.points = rect.GetRectCornersListRelative(rect.Center);
            this.RotRad = rotRad;
        }
        public PolyCollider(Rect rect, Vector2 vel, float rotRad = 0f) : base(rect.Center, vel)
        {
            this.points = rect.GetRectCornersListRelative(rect.Center);
            this.RotRad = RotRad;
            this.Vel = vel;
        }
        public override Rect GetBoundingBox()
        {
            return SPoly.GetPolyBoundingBox(Shape);
        }
        public override void DrawDebugShape(Color color)
        {
            SDrawing.DrawPolygon(Shape, 5f, color);
        }
        
        public override (bool valid, bool overlap) CheckOverlap(ICollider other)
        {
            if (other is CircleCollider c)
            {
                return (true, this.OverlapPolyCircle(c));
            }
            else if (other is SegmentCollider s)
            {
                return (true, this.OverlapPolySegment(s));
            }
            else if (other is RectCollider r)
            {
                return (true, this.OverlapPolyRect(r));
            }   
            else if (other is PolyCollider p)
            {
                return (true, this.OverlapPolyPoly(p));
            }
            return (false, false);
        }
        public override (bool valid, Intersection i) CheckIntersection(ICollider other)
        {

            if (other is CircleCollider c)
            {
                return (true, this.IntersectionPolyCircle(c));
            }
            else if (other is SegmentCollider s)
            {
                return (true, this.IntersectionPolySegment(s));
            }
            else if (other is RectCollider r)
            {
                return (true, this.IntersectionPolyRect(r));
            }
            else if (other is PolyCollider p)
            {
                return (true, this.IntersectionPolyPoly(p));
            }
            return (false, new());
        }
        public override bool CheckOverlapRect(Rect rect) { return rect.OverlapRectPoly(this); }

    }

}
