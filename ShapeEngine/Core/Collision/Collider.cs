
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Lib;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Interfaces;

namespace ShapeEngine.Core.Collision
{
    public struct Transform2D
    {
        public Vector2 pos = new();
        public float rotRad = 0f;
        public float scale = 1;

        public Transform2D() { }
        public Transform2D(Vector2 pos) { this.pos = pos; }
        public Transform2D(Vector2 pos, float rotRad) { this.pos = pos; this.rotRad = rotRad; }
        public Transform2D(Vector2 pos, float rotRad, float scale) { this.pos = pos; this.rotRad = rotRad; this.scale = scale; }

        public Transform2D Subtract(Transform2D other)
        {
            return new
            (
                pos - other.pos,
                rotRad - other.rotRad,
                scale - other.scale
            );
        }
        public Transform2D Add(Transform2D other)
        {
            return new
            (
                pos + other.pos,
                rotRad + other.rotRad,
                scale + other.scale
            );
        }
        public Transform2D Multiply(float factor)
        {
            return new
            (
                pos * factor,
                rotRad * factor,
                scale * factor
            );
        }
        public Transform2D Divide(float divisor)
        {
            return new
            (
                pos / divisor,
                rotRad / divisor,
                scale / divisor
            );
        }
        
    }

    public abstract class Collider : ICollider
    {
        public Collider() { }
        public Collider(float x, float y) { Pos = new(x, y); }
        public Collider(Vector2 pos, Vector2 vel) { Pos = pos; Vel = vel; }

        private Vector2 prevPos;

        public bool FlippedNormals { get; set; } = false;
        public float Mass { get; set; } = 1.0f;
        public Vector2 Vel { get; set; }
        public virtual Vector2 Pos { get; set; }
        public Vector2 ConstAcceleration { get; set; } = new(0f);
        public float Drag { get; set; } = 0f;
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// If false this shape does not take part in collision detection.
        /// </summary>
        public bool ComputeCollision { get; set; } = true;
        /// <summary>
        /// If false only overlaps will be reported but no further details on the intersection.
        /// </summary>
        public bool ComputeIntersections { get; set; } = false;
        public bool SimplifyCollision{ get; set; } = false;
        
        public abstract IShape GetShape();
        public abstract IShape GetSimplifiedShape();

        protected Vector2 accumulatedForce = new(0f);
        public Vector2 GetAccumulatedForce() { return accumulatedForce; }
        public void ClearAccumulatedForce() { accumulatedForce = new(0f); }
        public void AddForce(Vector2 force) { accumulatedForce = SPhysics.AddForce(this, force); }
        public void AddImpulse(Vector2 force) { SPhysics.AddImpuls(this, force); }
        public virtual void UpdateState(float dt) 
        {
            UpdatePreviousPosition(dt);
            SPhysics.UpdateState(this, dt);
        }

        public Vector2 GetPreviousPosition()
        {
            return prevPos;
        }

        public void UpdatePreviousPosition(float dt)
        {
            prevPos = Pos;
        }
        
        public abstract void DrawShape(float lineThickness, Raylib_CsLo.Color color);
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

        public override IShape GetShape() 
        {
            return GetCircleShape();
        }
        public override IShape GetSimplifiedShape()
        {
            return GetCircleShape();
        }
        public Circle GetCircleShape() 
        {
            var c = new Circle(Pos, radius);
            c.FlippedNormals = FlippedNormals;
            return c;
        }

        public override void DrawShape(float lineThickness, Raylib_CsLo.Color color)
        {
            var shape = GetCircleShape();
            shape.DrawLines(lineThickness, color);
        }
    }
    public class SegmentCollider : Collider
    {
        //private enum NormalFacingDirection { Automatic, Right, Left};
        //private NormalFacingDirection normalFacingDirection = NormalFacingDirection.Automatic;

        public SegmentCollider() { }
        public SegmentCollider(Vector2 start, Vector2 end) : base(start, new(0.0f, 0.0f))
        {
            Vector2 v = end - start;
            Dir = Vector2.Normalize(v);
            Length = v.Length();
        }
        /// <summary>
        /// Create a segment collider with a fixed normal direction. The normal direction is based on the direction of the segment.
        /// A right facing normal faces right in the direction of the segment and left facing normal faces left in the direction of the segment.
        /// </summary>
        /// <param name="start">The start point of the segment.</param>
        /// <param name="end">The end point of the segment.</param>
        /// <param name="normalAlwaysFacesRight">Sets the direction the normal faces.</param>
        public SegmentCollider(Vector2 start, Vector2 end, bool flippedNormals = false) : base(start, new(0.0f, 0.0f))
        {
            Vector2 v = end - start;
            Dir = Vector2.Normalize(v);
            Length = v.Length();
            this.FlippedNormals = flippedNormals;
            //normalFacingDirection = normalAlwaysFacesRight ? NormalFacingDirection.Right : NormalFacingDirection.Left;
        }
        public SegmentCollider(Vector2 start, Vector2 dir, float length) : base(start, new(0.0f, 0.0f)) 
        { 
            Dir = dir; 
            Length = length; 
        }

        public Vector2 Dir { get; set; }
        public float Length { get; set; }
        public Vector2 Start { get { return Pos; } }
        public Vector2 Center { get { return Pos + Dir * Length / 2; } }
        public Vector2 End { get { return Pos + Dir * Length; } }
        public Vector2 Displacement { get { return End - Pos; } }


        public override IShape GetShape() 
        {
            return GetSegmentShape();
        }
        public override IShape GetSimplifiedShape()
        {
            return GetSegmentShape();
        }
        public Segment GetSegmentShape()
        {
            //if (normalFacingDirection == NormalFacingDirection.Automatic) return new Segment(Pos, End);
            //else
            //{
            //    
            //    Vector2 n = (End - Pos);
            //    if (normalFacingDirection == NormalFacingDirection.Right) n = n.GetPerpendicularRight().Normalize();
            //    else n = n.GetPerpendicularLeft().Normalize();
            //    return new Segment(Pos, End, n);
            //}

            return new Segment(Pos, End, FlippedNormals);
        }
        public override void DrawShape(float lineThickness, Raylib_CsLo.Color color)
        {
            var shape = GetSegmentShape();
            shape.Draw(lineThickness, color);
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
        public RectCollider(Rect rect)
        {
            this.Pos = new(rect.X, rect.Y);
            this.Size = new(rect.Width, rect.Height);
            this.Vel = new(0f);
            this.Alignement = new(0f);
        }
        public RectCollider(Rect rect, Vector2 vel)
        {
            this.Pos = new(rect.X, rect.Y);
            this.Size = new(rect.Width, rect.Height);
            this.Vel = vel;
            this.Alignement = new(0f);
        }
        public Vector2 Alignement { get; set; }
        public Vector2 Size { get; set; }
        
        public override IShape GetShape() 
        {
            return GetRectShape();  
        }
        

        public override IShape GetSimplifiedShape()
        {
            return GetRectShape().GetBoundingCircle();
        }
        public Rect GetRectShape()
        {
            var r = new Rect(Pos, Size, Alignement);
            r.FlippedNormals = FlippedNormals;
            return r;
        }
        public override void DrawShape(float lineThickness, Raylib_CsLo.Color color)
        {
            var shape = GetRectShape();
            shape.DrawLines(lineThickness, color);
        }
    }
    public class PolyCollider : Collider
    {
        private Polygon shape;
        public override Vector2 Pos
        {
            get { return cur.pos; }
            set
            {
                dirty = true;
                cur.pos = value;
            }
        }
        public float Rot
        {
            get { return cur.rotRad; }
            set
            {
                dirty = true;
                cur.rotRad = value;
            }
        }
        public float Scale
        {
            get { return cur.scale; }
            set
            {
                dirty = true;
                cur.scale = value;
            }
        }

        private bool dirty = false;
        private Transform2D cur;
        private Transform2D prev;

        public PolyCollider(Polygon shape) 
        { 
            this.shape = shape;
            var pos = this.shape.GetCentroid();
            this.cur = new(pos);
            this.prev = new(pos);
        }
        public PolyCollider(Polygon shape, Vector2 vel)
        {
            this.shape = shape;
            this.Vel = vel;
            var pos = this.shape.GetCentroid();
            this.cur = new(pos);
            this.prev = new(pos);
        }
        public PolyCollider(Vector2 vel, params Vector2[] shape)
        {
            this.shape = new(shape);
            this.Vel = vel;
            var pos = this.shape.GetCentroid();
            this.cur = new(pos);
            this.prev = new(pos);
        }
        public PolyCollider(Polygon shape, Vector2 pos, Vector2 vel)
        {
            this.shape = shape;
            this.Vel = vel;
            this.cur = new(pos);
            this.prev = new(pos);
        }
        public PolyCollider(Vector2 pos, Vector2 vel, params Vector2[] shape)
        {
            this.shape = new(shape);
            this.Vel = vel;
            this.cur = new(pos);
            this.prev = new(pos);
        }
        public PolyCollider(Polygon shape, Vector2 pos, float rotRad, float scale)
        {
            this.shape = shape;
            this.cur = new(pos, rotRad, scale);
            this.prev = new(pos, rotRad, scale);
        }
        public PolyCollider(Polygon shape, Vector2 pos, float rotRad, float scale, Vector2 vel)
        {
            this.shape = shape;
            this.cur = new(pos, rotRad, scale);
            this.prev = new(pos, rotRad, scale);
            this.Vel = vel;
        }
        
        public PolyCollider(Segment s, float inflation, float alignement = 0.5f)
        {
            this.shape = s.Inflate(inflation, alignement).ToPolygon();
            var pos = this.shape.GetCentroid();
            this.cur = new(pos);
            this.prev = new(pos);
        }
        public PolyCollider(Polyline pl, float inflation)
        {
            this.shape = SClipper.Inflate(pl, inflation, Clipper2Lib.JoinType.Square, Clipper2Lib.EndType.Square, 2, 2).ToPolygons()[0];
        }
        public void SetNewShape(Polygon newShape) { this.shape = newShape; }
        public override IShape GetShape() 
        {
            return GetPolygonShape();
        }

        public override IShape GetSimplifiedShape()
        {
            return GetPolygonShape().GetBoundingCircle();
        }
        public Polygon GetPolygonShape() 
        { 
            if(dirty) UpdateShape();
            var p = new Polygon(shape);
            p.FlippedNormals = FlippedNormals;
            return p;
        }
        public override void DrawShape(float lineThickness, Raylib_CsLo.Color color)
        {
            var shape = GetPolygonShape();
            shape.DrawLines(lineThickness, color);
        }
        private void UpdateShape()
        {
            dirty = false;
            var difference = cur.Subtract(prev);
            prev = cur;

            for (int i = 0; i < shape.Count; i++)
            {
                Vector2 newPos = shape[i] + difference.pos;//translation
                Vector2 w = newPos - cur.pos;
                shape[i] = cur.pos + w.Rotate(difference.rotRad) * cur.scale;
            }

        }

    }
    public class PolylineCollider : Collider
    {
        private Polyline shape;
        public override Vector2 Pos
        {
            get { return cur.pos; } 
            set 
            {
                dirty = true;
                cur.pos = value; 
            }
        }
        public float Rot
        {
            get { return cur.rotRad; }
            set
            {
                dirty = true;
                cur.rotRad = value;
            }
        }
        public float Scale
        {
            get { return cur.scale; }
            set
            {
                dirty = true;
                cur.scale = value;
            }
        }

        private bool dirty = false;
        private Transform2D cur = new();
        private Transform2D prev = new();

        public PolylineCollider(Polyline shape) { this.shape = shape; }
        public PolylineCollider(Polyline shape, Vector2 vel)
        {
            this.shape = shape;
            this.Vel = vel;
        }
        public PolylineCollider(Vector2 vel, params Vector2[] shape)
        {
            this.shape = new(shape);
            this.Vel = vel;
        }
        public PolylineCollider(Polyline shape, Vector2 pos, Vector2 vel)
        {
            this.shape = shape;
            this.cur = new(pos);
            this.prev = new(pos);
            this.Vel = vel;
        }
        public PolylineCollider(Vector2 pos, Vector2 vel, params Vector2[] shape)
        {
            this.cur = new(pos);
            this.prev = new(pos);
            this.Vel = vel;
            this.shape = new(shape);
        }
        public PolylineCollider(Polyline shape, Vector2 pos, float rotRad, float scale, Vector2 vel)
        {
            this.shape = shape;
            this.Vel = vel;

            this.cur = new(pos, rotRad, scale);
            this.prev = new(pos, rotRad, scale);
        }
        public PolylineCollider(Vector2 pos, float rotRad, float scale, Vector2 vel, params Vector2[] shape)
        {
            this.Vel = vel;
            this.shape = new(shape);

            this.cur = new(pos, rotRad, scale);
            this.prev = new(pos, rotRad, scale);
        }


        public override IShape GetShape()
        {
            if(dirty) UpdateShape();

            var pl = new Polyline(shape);
            pl.FlippedNormals = FlippedNormals;
            return pl;
        }
        public override IShape GetSimplifiedShape()
        {
            return GetPolylineShape().GetBoundingCircle();
        }
        public Polyline GetPolylineShape() 
        {
            if (dirty) UpdateShape();
            var p = new Polyline(shape);
            p.FlippedNormals = FlippedNormals;
            return p; 
        }
        public override void DrawShape(float lineThickness, Raylib_CsLo.Color color)
        {
            var shape = GetPolylineShape();
            shape.Draw(lineThickness, color);
        }
        private void UpdateShape()
        {
            dirty = false;
            var difference = cur.Subtract(prev);
            prev = cur;

            for (int i = 0; i < shape.Count; i++)
            {
                Vector2 newPos = shape[i] + difference.pos;//translation
                Vector2 w = newPos - cur.pos;
                shape[i] = cur.pos + w.Rotate(difference.rotRad) * cur.scale;
            }

        }
    }
}