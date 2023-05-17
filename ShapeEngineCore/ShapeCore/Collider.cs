
using System.Numerics;
using ShapeLib;

namespace ShapeCore
{
    public abstract class Collider : ICollider
    {
        public Collider() { }
        public Collider(float x, float y) { Pos = new(x, y); }
        public Collider(Vector2 pos, Vector2 vel) { Pos = pos; Vel = vel; }

        public float Mass { get; set; } = 1.0f;
        public Vector2 Vel { get; set; }
        public virtual Vector2 Pos { get; set; }
        public Vector2 ConstAcceleration { get; set; } = new(0f);
        public float Drag { get; set; } = 0f;
        public bool Enabled { get; set; } = true;
        public bool ComputeCollision { get; set; } = true;
        public bool ComputeIntersections { get; set; } = false;

        public abstract IShape GetShape();
        //public abstract Rect GetBoundingBox();
        //public abstract void DrawDebugShape(Color color);
        public abstract bool CheckOverlap(ICollider other);
        public abstract Intersection CheckIntersection(ICollider other);
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

        //public float GetArea() { return MathF.PI * RadiusSquared; }
        //public float GetCircumference() { return MathF.PI * Radius * 2.0f; }
        //public override void DrawDebugShape(Color color) { new Circle(Pos, radius).dr SDrawing.DrawCircleLines(Pos, Radius, 5f, color, 4f); }
        //public override Rect GetBoundingBox() { return new Circle(Pos, radius).GetBoundingBox(); }
        public override IShape GetShape() { return new Circle(Pos, radius); }
        public Circle GetCircleShape() { return new Circle(Pos, radius); }
        public override bool CheckOverlap(ICollider other)
        {
            Circle shape = new(Pos, radius);
            return shape.Overlap(other.GetShape());
            //var otherShape = other.GetShape();
            //if(otherShape is Circle c) return (true, shape.OverlapShape(c));
            //else if (otherShape is Segment s) return (true, shape.OverlapShape(s));
            //else if (otherShape is Triangle t) return (true, shape.OverlapShape(t));
            //else if (otherShape is Rect r) return (true, shape.OverlapShape(r));
            //else if (otherShape is Polygon p) return (true, shape.OverlapShape(p));
            //return (false, false);
        }
        public override Intersection CheckIntersection(ICollider other)
        {
            Circle shape = new(Pos, radius);
            return shape.Intersect(other.GetShape());
            //var otherShape = other.GetShape();
            //if (otherShape is Circle c) return (true, shape.IntersectShape(c));
            //else if (otherShape is Segment s) return (true, shape.IntersectShape(s));
            //else if (otherShape is Triangle t) return (true, shape.IntersectShape(t));
            //else if (otherShape is Rect r) return (true, shape.IntersectShape(r));
            //else if (otherShape is Polygon p) return (true, shape.IntersectShape(p));
            //return (false, new());
        }
        public override bool CheckOverlapRect(Rect rect) { return rect.OverlapShape(new Circle(Pos, radius)); }
    
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


        public override IShape GetShape() { return new Segment(Pos, End); }
        public Segment GetSegmentShape() { return new Segment(Pos, End); }
        public override bool CheckOverlap(ICollider other)
        {
            Segment shape = GetSegmentShape();
            return shape.Overlap(other.GetShape());
            //var otherShape = other.GetShape();
            //if (otherShape is Circle c) return (true, shape.OverlapShape(c));
            //else if (otherShape is Segment s) return (true, shape.OverlapShape(s));
            //else if (otherShape is Triangle t) return (true, shape.OverlapShape(t));
            //else if (otherShape is Rect r) return (true, shape.OverlapShape(r));
            //else if (otherShape is Polygon p) return (true, shape.OverlapShape(p));
            //return (false, false);
        }
        public override Intersection CheckIntersection(ICollider other)
        {
            Segment shape = GetSegmentShape();
            return shape.Intersect(other.GetShape());
            //var otherShape = other.GetShape();
            //if (otherShape is Circle c) return (true, shape.IntersectShape(c));
            //else if (otherShape is Segment s) return (true, shape.IntersectShape(s));
            //else if (otherShape is Triangle t) return (true, shape.IntersectShape(t));
            //else if (otherShape is Rect r) return (true, shape.IntersectShape(r));
            //else if (otherShape is Polygon p) return (true, shape.IntersectShape(p));
            //return (false, new());
        }
        public override bool CheckOverlapRect(Rect rect) { return rect.OverlapShape( GetSegmentShape() ); }

        //public override Rect GetBoundingBox()
        //{
        //    Vector2 end = End;
        //    float topLeftX = MathF.Min(Pos.X, end.X);
        //    float topLeftY = MathF.Min(Pos.Y, end.Y);
        //    float bottomRightX = MathF.Max(Pos.X, end.X);
        //    float bottomRightY = MathF.Max(Pos.Y, end.Y);
        //    return new(topLeftX, topLeftY, bottomRightX - topLeftX, bottomRightY - topLeftY);
        //}
        //public override void DrawDebugShape(Color color)
        //{
        //    Raylib.DrawCircleV(Pos, 5.0f, color);
        //    Raylib.DrawLineEx(Pos, End, 5f, color);
        //}
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
        
        //public Rect Rect
        //{
        //    get { return new(Pos, Size, Alignement); }
        //}
        //public List<Vector2> GetCornersList() { return SRect.GetRectCornersList(Rect); }
        //public (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) GetCorners() { return SRect.GetRectCorners(Rect); }
        //public override Rect GetBoundingBox() { return Rect; }
        //public override void DrawDebugShape(Color color)
        //{
        //    Raylib.DrawRectangleLinesEx(Rect.Rectangle, 5f, color);
        //}

        public override IShape GetShape() { return new Rect(Pos, Size, Alignement); }
        public Rect GetRectShape() { return new Rect(Pos, Size, Alignement); }
        public override bool CheckOverlap(ICollider other)
        {
            Rect shape = GetRectShape();
            return shape.Overlap(other.GetShape());
            //var otherShape = other.GetShape();
            //if (otherShape is Circle c) return (true, shape.OverlapShape(c));
            //else if (otherShape is Segment s) return (true, shape.OverlapShape(s));
            //else if (otherShape is Triangle t) return (true, shape.OverlapShape(t));
            //else if (otherShape is Rect r) return (true, shape.OverlapShape(r));
            //else if (otherShape is Polygon p) return (true, shape.OverlapShape(p));
            //return (false, false);
        }
        public override Intersection CheckIntersection(ICollider other)
        {
            Rect shape = GetRectShape();
            return shape.Intersect(other.GetShape());
            //var otherShape = other.GetShape();
            //if (otherShape is Circle c) return (true, shape.IntersectShape(c));
            //else if (otherShape is Segment s) return (true, shape.IntersectShape(s));
            //else if (otherShape is Triangle t) return (true, shape.IntersectShape(t));
            //else if (otherShape is Rect r) return (true, shape.IntersectShape(r));
            //else if (otherShape is Polygon p) return (true, shape.IntersectShape(p));
            //return (false, new());
        }
        public override bool CheckOverlapRect(Rect rect) { return rect.OverlapShape(GetRectShape()); }

    }
    public class PolyCollider : Collider
    {
        private Polygon points;
        private Polygon shape;
        private Vector2 pos;
        private float rotRad = 0f;
        private float scale = 1f;
        private bool dirty = false;

        public override Vector2 Pos 
        {
            get { return pos; }
            set { pos = value; dirty = true; } 
        }
        public float RotRad 
        {
            get { return rotRad; } 
            set { rotRad = value; dirty = true; }
        }
        public float Scale 
        {
            get { return scale; }
            set { scale = value; dirty = true; } 
        }
        public List<Vector2> Shape 
        { 
            get 
            {
                if (dirty) GenerateShape();
                return shape; 
            } 
        }

        public PolyCollider(Polygon shape)
        {
            this.pos = shape.GetCentroid(); // SPoly.GetCentroid(shape);
            points = GenerateDisplacements(shape, this.pos);
            this.shape = shape;
            this.Vel = new(0f);
        }
        public PolyCollider(Polygon shape, Vector2 vel)
        {
            this.pos = shape.GetCentroid();
            points = GenerateDisplacements(shape, this.pos);
            this.shape = shape;
            this.Vel = vel;
        }
        public PolyCollider(Vector2 vel, params Vector2[] shape)
        {
            this.shape = new(shape);// shape.ToList();
            this.pos = this.shape.GetCentroid(); // SPoly.GetCentroid(this.shape);
            points = GenerateDisplacements(this.shape, this.pos);
            this.Vel = vel;
        }
        public PolyCollider(Polygon shape, Vector2 pos, Vector2 vel)
        {
            this.pos = pos;
            points = GenerateDisplacements(shape, this.pos);
            this.shape = shape;
            this.Vel = vel;
        }
        public PolyCollider(Vector2 pos, Vector2 vel, params Vector2[] shape)
        {
            this.pos = pos;
            this.shape = new(shape); // shape.ToList();
            points = GenerateDisplacements(this.shape, this.pos);
            this.Vel = vel;
        }
        public PolyCollider(Polygon relativePoints, Vector2 pos, float rotRad = 0f, float scale = 1f)
        {
            this.points = relativePoints;
            this.shape = new();
            this.pos = pos;
            this.rotRad = rotRad;
            this.scale = scale;
            this.dirty = true;
            this.Vel = new(0f);
        }
        public PolyCollider(Polygon relativePoints, Vector2 pos, Vector2 vel, float rotRad = 0f, float scale = 1f)
        {
            this.points = relativePoints;
            this.shape = new();
            this.pos = pos;
            this.rotRad = rotRad;
            this.scale = scale;
            this.dirty = true;
            this.Vel = vel;
        }
        public PolyCollider(Vector2 pos, Vector2 vel, float rotRad = 0f, float scale = 1f, params Vector2[] relativePoints)
        {
            this.points = new(relativePoints); // relativePoints.ToList();
            this.shape = new();
            this.pos = pos;
            this.rotRad = rotRad;
            this.scale = scale;
            this.dirty = true;
            this.Vel = vel;
        }

        public override IShape GetShape() { return new Polygon(Shape); } // new Polygon(Shape, Pos); }
        public Polygon GetPolygonShape() { return new Polygon(Shape); }

        public override bool CheckOverlap(ICollider other)
        {
            Polygon shape = GetPolygonShape();
            return shape.Overlap(other.GetShape());
            //var otherShape = other.GetShape();
            //if (otherShape is Circle c) return (true, shape.OverlapShape(c));
            //else if (otherShape is Segment s) return (true, shape.OverlapShape(s));
            //else if (otherShape is Triangle t) return (true, shape.OverlapShape(t));
            //else if (otherShape is Rect r) return (true, shape.OverlapShape(r));
            //else if (otherShape is Polygon p) return (true, shape.OverlapShape(p));
            //return (false, false);
        }
        public override Intersection CheckIntersection(ICollider other)
        {
            Polygon shape = GetPolygonShape();
            return shape.Intersect(other.GetShape());
            //var otherShape = other.GetShape();
            //if (otherShape is Circle c) return (true, shape.IntersectShape(c));
            //else if (otherShape is Segment s) return (true, shape.IntersectShape(s));
            //else if (otherShape is Triangle t) return (true, shape.IntersectShape(t));
            //else if (otherShape is Rect r) return (true, shape.IntersectShape(r));
            //else if (otherShape is Polygon p) return (true, shape.IntersectShape(p));
            //return (false, new());
        }
        public override bool CheckOverlapRect(Rect rect) { return rect.OverlapShape(GetPolygonShape()); }

        private void GenerateShape()
        {
            shape = SPoly.GetShape(points, Pos, RotRad, new(Scale));
            dirty = false;
        }
        private static Polygon GenerateDisplacements(Polygon shape, Vector2 center)
        {
            Polygon displacements = new();
            for (int i = 0; i < shape.Count; i++)
            {
                displacements.Add(shape[i] - center);
            }
            return displacements;
        }
        
        
        /*
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
            this.points = rect.GetPointsRelative(rect.Center);
            this.RotRad = rotRad;
        }
        public PolyCollider(Rect rect, Vector2 vel, float rotRad = 0f) : base(rect.Center, vel)
        {
            this.points = rect.GetPointsRelative(rect.Center);
            this.RotRad = RotRad;
            this.Vel = vel;
        }
        */
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
        //public override Rect GetBoundingBox()
        //{
        //    return SPoly.GetBoundingBox(Shape);
        //}
        //public override void DrawDebugShape(Color color)
        //{
        //    SDrawing.DrawPolygonLines(Shape, 5f, color);
        //}
    }

}


/*
public interface ICollider : IPhysicsObject
    {
        public bool Enabled { get; set; }
        public bool ComputeCollision { get; set; }
        public bool ComputeIntersections { get; set; }

        public IShape GetShape();
        //public Rect GetBoundingBox();
        //public void DrawDebugShape(Color color);
        public (bool valid, bool overlap) CheckOverlap(ICollider other);
        public (bool valid, Intersection i) CheckIntersection(ICollider other);
        public bool CheckOverlapRect(Rect rect);

        //todo getsegments intersection/overlap
    }
    
    public abstract class Collider : ICollider
    {
        //public Collider() { }
        //public Collider(float x, float y) { Pos = new(x, y); }
        //public Collider(Vector2 pos, Vector2 vel) { Pos = pos; Vel = vel; }

        public float Mass { get; set; } = 1.0f;
        public Vector2 Vel { get; set; }
        public virtual Vector2 Pos { get; set; }
        public Vector2 ConstAcceleration { get; set; } = new(0f);
        public float Drag { get; set; } = 0f;
        public bool Enabled { get; set; } = true;
        public bool ComputeCollision { get; set; } = true;
        public bool ComputeIntersections { get; set; } = false;
        //public abstract Rect GetBoundingBox();
        //public abstract void DrawDebugShape(Color color);
        public abstract IShape GetShape();
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
        private Circle circle;

        public CircleCollider(Circle c) { this.circle = c; }

        public override Vector2 Pos { get => circle.center; set => circle.center = value; }

        public override IShape GetShape() { return circle; }
        public override (bool valid, bool overlap) CheckOverlap(ICollider other)
        {
            IShape otherShape = other.GetShape();
            if (otherShape is Circle c)         return (true, circle.OverlapShape(c));
            else if (otherShape is Segment s)   return (true, circle.OverlapShape(s));
            else if(otherShape is Triangle t)   return (true, circle.OverlapShape(t));
            else if (otherShape is Rect r)      return (true, circle.OverlapShape(r));
            else if (otherShape is Polygon p)   return (true, circle.OverlapShape(p));
            return (false, false);
        }
        public override (bool valid, Intersection i) CheckIntersection(ICollider other)
        {
            IShape otherShape = other.GetShape();
            if (otherShape is Circle c) return (true, circle.IntersectShape(c));
            else if (otherShape is Segment s) return (true, circle.IntersectShape(s));
            else if (otherShape is Triangle t) return (true, circle.IntersectShape(t));
            else if (otherShape is Rect r) return (true, circle.IntersectShape(r));
            else if (otherShape is Polygon p) return (true, circle.IntersectShape(p));
            return (false, new());
        }
        public override bool CheckOverlapRect(Rect rect) { return rect.OverlapShape(circle); }
       
    
    }
    public class SegmentCollider : Collider
{
    private Segment segment;

    public SegmentCollider(Segment s) { this.segment = s; }

    public override Vector2 Pos { get => segment.start; set => segment.SetPosition(value); }
    public override IShape GetShape() { return segment; }
    public override (bool valid, bool overlap) CheckOverlap(ICollider other)
    {
        IShape otherShape = other.GetShape();
        if (otherShape is Circle c) return (true, segment.OverlapShape(c));
        else if (otherShape is Segment s) return (true, segment.OverlapShape(s));
        else if (otherShape is Triangle t) return (true, segment.OverlapShape(t));
        else if (otherShape is Rect r) return (true, segment.OverlapShape(r));
        else if (otherShape is Polygon p) return (true, segment.OverlapShape(p));
        return (false, false);
    }
    public override (bool valid, Intersection i) CheckIntersection(ICollider other)
    {
        IShape otherShape = other.GetShape();
        if (otherShape is Circle c) return (true, segment.IntersectShape(c));
        else if (otherShape is Segment s) return (true, segment.IntersectShape(s));
        else if (otherShape is Triangle t) return (true, segment.IntersectShape(t));
        else if (otherShape is Rect r) return (true, segment.IntersectShape(r));
        else if (otherShape is Polygon p) return (true, segment.IntersectShape(p));
        return (false, new());
    }
    public override bool CheckOverlapRect(Rect rect) { return rect.OverlapShape(segment); }

    
}
public class RectCollider : Collider
{
    private Rect rect;//rect needs alignement?

    public RectCollider(Rect r) { this.rect = r; }

    public override Vector2 Pos { get => rect.Center; set => rect.SetPosition(value); }
    public override IShape GetShape() { return rect; }
    public override (bool valid, bool overlap) CheckOverlap(ICollider other)
    {
        IShape otherShape = other.GetShape();
        if (otherShape is Circle c) return (true, rect.OverlapShape(c));
        else if (otherShape is Segment s) return (true, rect.OverlapShape(s));
        else if (otherShape is Triangle t) return (true, rect.OverlapShape(t));
        else if (otherShape is Rect r) return (true, rect.OverlapShape(r));
        else if (otherShape is Polygon p) return (true, rect.OverlapShape(p));
        return (false, false);
    }
    public override (bool valid, Intersection i) CheckIntersection(ICollider other)
    {
        IShape otherShape = other.GetShape();
        if (otherShape is Circle c) return (true, rect.IntersectShape(c));
        else if (otherShape is Segment s) return (true, rect.IntersectShape(s));
        else if (otherShape is Triangle t) return (true, rect.IntersectShape(t));
        else if (otherShape is Rect r) return (true, rect.IntersectShape(r));
        else if (otherShape is Polygon p) return (true, rect.IntersectShape(p));
        return (false, new());
    }
    public override bool CheckOverlapRect(Rect rect) { return rect.OverlapShape(this.rect); }




}
public class PolyCollider : Collider
{
    List<Vector2> points;
    public float RotRad { get; set; }
    public float Scale { get; set; } = 1f;
    public List<Vector2> Shape { get { return SPoly.GetShape(points, Pos, RotRad, new(Scale)); } }
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
        this.points = rect.GetPointsRelative(rect.Center);
        this.RotRad = rotRad;
    }
    public PolyCollider(Rect rect, Vector2 vel, float rotRad = 0f) : base(rect.Center, vel)
    {
        this.points = rect.GetPointsRelative(rect.Center);
        this.RotRad = RotRad;
        this.Vel = vel;
    }
    public override Rect GetBoundingBox()
    {
        return SPoly.GetBoundingBox(Shape);
    }
    public override void DrawDebugShape(Color color)
    {
        SDrawing.DrawPolygonLines(Shape, 5f, color);
    }

    public override (bool valid, bool overlap) CheckOverlap(ICollider other)
    {
        if (other is CircleCollider c)
        {
            return (true, this.OverlapCollider(c));
        }
        else if (other is SegmentCollider s)
        {
            return (true, this.OverlapCollider(s));
        }
        else if (other is RectCollider r)
        {
            return (true, this.OverlapCollider(r));
        }
        else if (other is PolyCollider p)
        {
            return (true, this.OverlapCollider(p));
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


*/