﻿
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using ShapeEngine.Color;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Lib;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

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

    public enum ShapeType
    {
        None = 0,
        Circle = 1,
        Segment = 2,
        Triangle = 3,
        Rect = 4,
        Poly = 5,
        PolyLine = 6
    }
    
    //TODO IShape removal
    //Remove ICollider interface -> just use abstract collider class
    //implement a collider for each shape (segment, triangle, circle, rect, poly, polyline)
    //collider uses shape type and GetSegmentShape(), GetCircleShape(), GetTriangleShape(), etc functions.
    
    //ICollidable get physics stuff
    //Collider has an offset -> every time state is updated relativePos & vel is updated based on ICollidable parent state (using offset)
    //Collider is basically just a wrapper class for shape structs

    public abstract class Collidable2
    {
        public Collidable2(Vector2 position, BitFlag collisionMask, uint collisionLayer)
        {
            Position = position;
            CollisionMask = collisionMask;
            CollisionLayer = collisionLayer;
        }

        public bool Enabled { get; set; } = true;
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; } = new(0f);
        public Vector2 ConstAcceleration { get; set; } = new(0f);
        public Vector2 AccumulatedForce { get; private set; } = new(0f);
        public float Mass { get; set; } = 1.0f;
        public float Drag { get; set; } = 0f;
        

        public BitFlag CollisionMask { get; private set; }
        public uint CollisionLayer { get; private set; }
        
        public List<Collider2> Colliders { get; private set; } = new();
        public bool HasColliders => Colliders.Count > 1;

        public Collider2? GetSingleCollider() => Colliders.Count != 1 ? null : Colliders[0];


        /// <summary>
        /// If false this shape does not take part in collision detection.
        /// </summary>
        public bool ComputeCollision { get; set; } = true;
        /// <summary>
        /// If false only overlaps will be reported but no further details on the intersection.
        /// </summary>
        public bool ComputeIntersections { get; set; } = false;

        
        public void ClearAccumulatedForce() => AccumulatedForce = new(0f);

        public void AddForce(Vector2 force) => AccumulatedForce = ShapePhysics.AddForce(force, AccumulatedForce, Mass);

        public void AddImpulse(Vector2 force) => Velocity = ShapePhysics.AddImpulse(force, Velocity, Mass);

        public void UpdateState(float dt) 
        {
            foreach (var collider in Colliders)
            {
                collider.UpdateState(dt, Position, Velocity);
            }
            OnUpdateState(dt);
        }

        public abstract void Overlap(CollisionInformation info);
        public abstract void OverlapEnded(ICollidable other);
        
        protected virtual void OnUpdateState(float dt)
        {
            
        }
    }
    public abstract class Collider2
    {
        public Vector2 Offset { get; set; }
        
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public Vector2 PrevPosition { get; private set; }
        public bool FlippedNormals { get; set; } = false;

        public Collider2(Vector2 offset)
        {
            this.Offset = offset;
        }

        public void UpdateState(float dt, Vector2 parentPosition, Vector2 parentVelocity)
        {
            PrevPosition = Position;
            Velocity = parentVelocity;
            Position = parentPosition + Offset;
            OnUpdateState(dt);
        }

        protected virtual void OnUpdateState(float dt)
        {
            
        }

        public abstract ShapeType GetShapeType();
        public virtual Circle? GetCircleShape() => null;
        public virtual Segment? GetSegmentShape() => null;
        public virtual Triangle? GetTriangleShape() => null;
        public virtual Rect? GetRectShape() => null;
        public virtual Polygon? GetPolygonShape() => null;
        public virtual Polyline? GetPolylineShape() => null;

    }

    public class SegmentCollider2 : Collider2
    {
        private Vector2 dir;
        private float length;
        private float originOffset = 0f;
        
        
        /// <summary>
        /// 0 Start = Position / 0.5 Center = Position / 1 End = Position
        /// </summary>
        public float OriginOffset
        {
            get => originOffset;
            set
            {
                originOffset = ShapeMath.Clamp(value, 0f, 1f);
                Recalculate();
            } 
        }
        public Vector2 Dir
        {
            get => dir;
            set
            {
                if (dir.LengthSquared() <= 0f) return;
                dir = value;
                Recalculate();
            }
        }
        public float Length
        {
            get => length;
            set
            {
                if (value <= 0) return;
                length = value;
                Recalculate();
            }
        }

        
        public Vector2 Start { get; private set; }  //  => Position;
        public Vector2 End { get; private set; }  // => Position + Dir * Length;
        
        public Vector2 Center => (Start + End) * 0.5f; // Position + Dir * Length / 2;
        
        public Vector2 Displacement => End - Start;

        private void Recalculate()
        {
            Start = Position - Dir * OriginOffset * Length;
            End = Position + Dir * (1f - OriginOffset) * Length;
        }

        public SegmentCollider2(Vector2 offset, Vector2 dir, float length, float originOffset = 0f) : base(offset)
        {
            this.dir = dir;
            this.length = length;
            this.originOffset = originOffset;
        }

        public override ShapeType GetShapeType() => ShapeType.Segment;
        public override Segment? GetSegmentShape() => new Segment(Start, End, FlippedNormals);
    }
    public class CircleCollider2 : Collider2
    {
        public float Radius { get; set; }
        
        
        public CircleCollider2(Vector2 offset, float radius) : base(offset)
        {
            this.Radius = radius;
        }

        public override ShapeType GetShapeType() => ShapeType.Circle;
        public override Circle? GetCircleShape() => new Circle(Position, Radius);
    }
    public class TriangleCollider2 : Collider2
    {
        public TriangleCollider2(Vector2 offset) : base(offset)
        {
        }

        public override ShapeType GetShapeType() => ShapeType.Triangle;
        public override Triangle? GetTriangleShape()
        {
            return base.GetTriangleShape();
        }
    }
    public class RectCollider2 : Collider2
    {
        public RectCollider2(Vector2 offset) : base(offset)
        {
        }

        public override ShapeType GetShapeType() => ShapeType.Rect;
        public override Rect? GetRectShape()
        {
            return base.GetRectShape();
        }
    }
    public class PolyCollider2 : Collider2
    {
        public PolyCollider2(Vector2 offset) : base(offset)
        {
        }

        public override ShapeType GetShapeType() => ShapeType.Poly;
        public override Polygon? GetPolygonShape()
        {
            return base.GetPolygonShape();
        }
    }
    public class PolyLineCollider2 : Collider2
    {
        public PolyLineCollider2(Vector2 offset) : base(offset)
        {
        }

        public override ShapeType GetShapeType() => ShapeType.PolyLine;
        public override Polyline? GetPolylineShape()
        {
            return base.GetPolylineShape();
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
        public void AddForce(Vector2 force) { accumulatedForce = ShapePhysics.AddForce(this, force); }
        public void AddImpulse(Vector2 force) { ShapePhysics.AddImpuls(this, force); }
        public virtual void UpdateState(float dt) 
        {
            UpdatePreviousPosition(dt);
            ShapePhysics.UpdateState(this, dt);
        }

        public Vector2 GetPreviousPosition()
        {
            return prevPos;
        }

        public void UpdatePreviousPosition(float dt)
        {
            prevPos = Pos;
        }
        
        public abstract void DrawShape(float lineThickness, ColorRgba colorRgba);
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

        public override void DrawShape(float lineThickness, ColorRgba colorRgba)
        {
            var shape = GetCircleShape();
            shape.DrawLines(lineThickness, colorRgba);
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
        public override void DrawShape(float lineThickness, ColorRgba colorRgba)
        {
            var shape = GetSegmentShape();
            shape.Draw(lineThickness, colorRgba);
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
        public override void DrawShape(float lineThickness, ColorRgba colorRgba)
        {
            var shape = GetRectShape();
            shape.DrawLines(lineThickness, colorRgba);
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
            this.shape = ShapeClipper.Inflate(pl, inflation, Clipper2Lib.JoinType.Square, Clipper2Lib.EndType.Square, 2, 2).ToPolygons()[0];
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
            var p = new Polygon(shape)
            {
                FlippedNormals = FlippedNormals
            };
            return p;
        }
        public override void DrawShape(float lineThickness, ColorRgba colorRgba)
        {
            var shape = GetPolygonShape();
            shape.DrawLines(lineThickness, colorRgba);
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
        public override void DrawShape(float lineThickness, ColorRgba colorRgba)
        {
            var shape = GetPolylineShape();
            shape.Draw(lineThickness, colorRgba);
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