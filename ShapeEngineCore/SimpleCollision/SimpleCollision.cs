using Raylib_CsLo;
using System.Numerics;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.UI;

namespace ShapeEngineCore.SimpleCollision
{
    public enum ColliderType
    {
        STATIC = 0,
        DYNAMIC = 1
    }
    public enum ColliderClass
    {
        COLLIDER = 0,
        AREA = 1
    }



    public interface ICollidable
    {
        public string GetID();
        public Collider GetCollider();
        public ColliderType GetColliderType();
        public ColliderClass GetColliderClass();
        public void Collide(CastInfo info);
        public void Overlap(OverlapInfo info);
        public Vector2 GetPos();
        public string GetCollisionLayer();//on what collision layer is the ICollidable
        public string[] GetCollisionMask();//with what layers does this ICollidable collide

        public bool HasDynamicBoundingBox();// { return false; }
    }




    public struct IntersectionInfo
    {
        public bool intersected;
        public Vector2 point;
        public float time;
        public float remaining;
    }
    public struct OverlapInfo
    {
        public bool overlapping;
        public ICollidable? self;//area
        public ICollidable? other;//entity
        public Vector2 selfVel;
        public Vector2 otherVel;
        //some time add closest point to have some extra info
        public OverlapInfo(bool overlapping, ICollidable other, ICollidable self, Vector2 otherVel, Vector2 selfVel)
        {
            this.overlapping = overlapping;
            this.other = other;
            this.self = self;
            this.otherVel = otherVel;
            this.selfVel = selfVel;
        }
    }
    public struct CastInfo
    {
        public bool overlapping = false;
        public bool collided = false;
        public float time = 0.0f;
        public Vector2 intersectionPoint = new();
        public Vector2 collisionPoint = new();
        public Vector2 reflectVector = new();
        public Vector2 normal = new();
        public ICollidable? self = null;
        public ICollidable? other = null;
        public Vector2 selfVel = new();
        public Vector2 otherVel = new();

        public CastInfo() { overlapping = false; collided = false; }
        public CastInfo(bool overlapping) { this.overlapping = overlapping; collided = false; }
        public CastInfo(bool overlapping, bool collided) { this.overlapping = overlapping; this.collided = collided; }
        public CastInfo(bool overlapping, bool collided, float time, Vector2 intersectionPoint, Vector2 collisionPoint, Vector2 reflectVector, Vector2 normal, Vector2 selfVel, Vector2 otherVel)
        {
            this.overlapping = overlapping;
            this.collided = collided;
            this.time = time;
            this.intersectionPoint = intersectionPoint;
            this.collisionPoint = collisionPoint;
            this.reflectVector = reflectVector;
            this.normal = normal;
            this.selfVel = selfVel;
            this.otherVel = otherVel;
        }
    }



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
        public virtual Rectangle GetDynamicBoundingRect() { return Utils.ScaleRectangle(GetBoundingRect(), MathF.Max(Vel.Length() * GAMELOOP.GAME_DELTA, 1f)); }
        public virtual void DebugDrawShape(Color color) { DrawCircle((int)Pos.X, (int)Pos.Y, 5.0f, color); }
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
        //public virtual Rectangle GetDynamicBoundingRect() { return Utils.ScaleRectangle(GetBoundingRect(), Vel.Length() * GAMELOOP.DELTA); }
        public override void DebugDrawShape(Color color) { DrawCircle((int)Pos.X, (int)Pos.Y, Radius, color); }

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
        //public Vector2 Displacement { get { return End - Pos; } private set { } }
        //public Vector2 End { get { return Pos + Dir * Length; } private set { } }

        public Vector2 GetEnd() { return Pos + Dir * Length; }
        public Vector2 GetDisplacement() { return GetEnd() - Pos; }

        public override Rectangle GetBoundingRect()
        {
            Vector2 end = GetEnd();
            float topLeftX = MathF.Min(Pos.X, end.X);
            float topLeftY = MathF.Min(Pos.Y, end.Y);
            float bottomRightX = MathF.Max(Pos.X, end.X);
            float bottomRightY = MathF.Max(Pos.Y, end.Y);
            return new(topLeftX, topLeftY, bottomRightX - topLeftX, bottomRightY - topLeftY);
        }
        public override void DebugDrawShape(Color color)
        {
            DrawCircle((int)Pos.X, (int)Pos.Y, 10.0f, color);
            DrawLineEx(Pos, GetEnd(), 5.0f, color);
        }
    }
    public class RectCollider : Collider
    {
        public RectCollider() { }
        public RectCollider(float x, float y, float w, float h)
        {
            Rectangle r = GetCorrectRect(x, y, w, h);
            Pos = new(r.x, r.y);
            Size = new(r.width, r.height);
        }
        public RectCollider(Vector2 pos, Vector2 size)
        {
            Rectangle r = GetCorrectRect(pos, size);
            Pos = new(r.x, r.y);
            Size = new(r.width, r.height);
            Vel = new(0.0f, 0.0f);
        }
        public RectCollider(Vector2 pos, Vector2 vel, Vector2 size)
        {
            Rectangle r = GetCorrectRect(pos, size);
            Pos = new(r.x, r.y);
            Size = new(r.width, r.height);
            Vel = vel;
        }

        public Vector2 Size { get; set; }
        //public Vector2 BottomRight { get { return Pos + Size; } private set { } }
        public Vector2 GetBottomRight() { return Pos + Size; }
        public Rectangle GetRectangle() { return new(Pos.X, Pos.Y, Size.X, Size.Y); }
        public override Rectangle GetBoundingRect() { return GetRectangle(); }
        public override void DebugDrawShape(Color color)
        {
            DrawRectangleRec(GetRectangle(), color);
        }

        public static Rectangle GetCorrectRect(float x, float y, float w, float h)
        {
            float p1x = x;
            float p1y = y;
            float p2x = x + w;
            float p2y = y + h;

            float topLeftX = MathF.Min(p1x, p2x);
            float topLeftY = MathF.Min(p1y, p2y);
            float bottomRightX = MathF.Max(p1x, p2x);
            float bottomRightY = MathF.Max(p1y, p2y);
            return new(topLeftX, topLeftY, bottomRightX - topLeftX, bottomRightY - topLeftY);
        }
        public static Rectangle GetCorrectRect(Vector2 pos, Vector2 size)
        {
            return GetCorrectRect(pos.X, pos.Y, size.X, size.Y);
        }

    }
    public class OrientedRectCollider : Collider
    {
        public OrientedRectCollider() { }
        public OrientedRectCollider(RectCollider rect, float rotation)
        {
            HalfSize = rect.Size * 0.5f;//half size
            Pos = rect.Pos + HalfSize;//centered
            Rotation = rotation;
            Vel = new(0.0f, 0.0f);
        }
        public OrientedRectCollider(Vector2 center, Vector2 halfSize, float rotation) : base(center, new Vector2(0.0f, 0.0f)) { HalfSize = halfSize; Rotation = rotation; }
        public OrientedRectCollider(Vector2 center, Vector2 vel, Vector2 halfSize, float rotation) : base(center, vel) { HalfSize = halfSize; Rotation = rotation; }

        public Vector2 HalfSize { get; set; }
        public float Rotation { get; set; }

        public Rectangle GetRectangle() { return new(Pos.X, Pos.Y, HalfSize.X * 2.0f, HalfSize.Y * 2.0f); }
        public override Rectangle GetBoundingRect()
        {
            (Vector2 pos, Vector2 size) hull = Overlap.OrientedRectHull(Pos, HalfSize, Rotation);
            Rectangle rect = new(hull.pos.X, hull.pos.Y, hull.size.X, hull.size.Y);
            return rect;
        }
        public override void DebugDrawShape(Color color)
        {
            DrawRectanglePro(GetRectangle(), HalfSize, Rotation, color);
        }
    }



    internal class Range
    {
        public float min;
        public float max;
        public Range(float min, float max)
        {
            this.min = min;
            this.max = max;
            Sort();
        }

        public void Sort()
        {
            if (min > max)
            {
                float temp = max;
                max = min;
                min = temp;
            }
        }
    }
    public class CollisionHandler
    {
        protected bool advanced = false;
        protected List<ICollidable> collidables = new();
        protected List<ICollidable> tempHolding = new();
        protected List<ICollidable> tempRemoving = new();
        protected SpatialHash spatialHash;

        protected List<OverlapInfo> overlapInfos = new();
        protected List<CastInfo> castInfos = new();

        //advanced section
        protected List<ICollidable> disabled = new();
        protected List<ICollidable> staticCollidables = new();


        //advanced automatically removes disabled shapes and readds them when they are enabled again
        public CollisionHandler(float x, float y, float w, float h, int rows, int cols, bool advanced = false)
        {
            spatialHash = new(x, y, w, h, rows, cols);
            this.advanced = advanced;
        }
        public void UpdateArea(Rectangle newArea)
        {
            int rows = spatialHash.GetRows();
            int cols = spatialHash.GetCols();
            spatialHash.Close();
            spatialHash = new(newArea.x, newArea.y, newArea.width, newArea.height, rows, cols);
            foreach (var c in staticCollidables)
            {
                spatialHash.Add(c, false);
            }
        }
        public SpatialHash GetSpatialHash() { return spatialHash; }
        public void Add(ICollidable collider)
        {
            if (collidables.Contains(collider)) return;
            tempHolding.Add(collider);
        }
        public void AddRange(List<ICollidable> colliders)
        {
            foreach (ICollidable collider in colliders)
            {
                Add(collider);
            }
        }
        public void Remove(ICollidable collider)
        {
            tempRemoving.Add(collider);
        }
        public void RemoveRange(List<ICollidable> colliders)
        {
            tempRemoving.AddRange(colliders);
        }
        public void Clear()
        {
            collidables.Clear();
            tempHolding.Clear();
            tempRemoving.Clear();
            overlapInfos.Clear();
            castInfos.Clear();
        }
        public void Close()
        {
            Clear();
            spatialHash.Close();
        }
        public virtual void Update(float dt)
        {
            spatialHash.ClearDynamic();

            if (advanced)
            {
                for (int i = staticCollidables.Count - 1; i >= 0; i--)
                {
                    var collider = staticCollidables[i];
                    if (!collider.GetCollider().IsEnabled())
                    {
                        disabled.Add(collider);
                        spatialHash.Remove(collider, false);
                        staticCollidables.RemoveAt(i);
                    }
                }
            }

            for (int i = collidables.Count - 1; i >= 0; i--)
            {
                var collider = collidables[i];
                if (!collider.GetCollider().IsEnabled())
                {
                    if (advanced)
                    {
                        disabled.Add(collider);
                        collidables.RemoveAt(i);
                    }
                    continue;
                }
                if (collider.GetColliderType() == ColliderType.DYNAMIC)
                {
                    spatialHash.Add(collider, true);
                }
            }

            for (int i = 0; i < collidables.Count; i++)
            {
                ICollidable collider = collidables[i];
                if (!collider.GetCollider().IsEnabled()) continue;
                string[] collisionMask = collider.GetCollisionMask();

                //if (collider.GetColliderType() == ColliderType.STATIC || collider.GetColliderClass() == ColliderClass.AREA) continue;

                List<ICollidable> others = spatialHash.GetObjects(collider);
                foreach (ICollidable other in others)
                {
                    string otherLayer = other.GetCollisionLayer();
                    if (collisionMask.Length > 0)
                    {
                        //if (otherLayer == "") continue; //do i need that?
                        if (!collisionMask.Contains(otherLayer)) continue;
                    }//else collide with everything

                    if (collider.GetColliderClass() == ColliderClass.COLLIDER)
                    {
                        if (other.GetColliderClass() == ColliderClass.COLLIDER)
                        {
                            CastInfo info = Collision.CheckCast(collider, other, dt);
                            if (info.collided || info.overlapping)
                            {
                                castInfos.Add(info);
                            }
                        }
                        //else//area is being checked by collider
                        //{
                        //    bool overlap = Overlap.Check(collider, other);
                        //    if (overlap)
                        //    {
                        //
                        //        overlapInfos.Add(new(true, collider, other));
                        //    }
                        //}
                    }
                    else//area checks other things
                    {
                        bool overlap = Overlap.Check(collider, other);
                        if (overlap)
                        {
                            overlapInfos.Add(new(true, other, collider, other.GetCollider().Vel, collider.GetCollider().Vel));
                        }
                    }

                }

            }
        }
        public virtual void Resolve()
        {
            //collidables.AddRange(tempHolding);
            foreach (var collider in tempHolding)
            {
                if (collider.GetColliderType() == ColliderType.STATIC)
                {
                    if (collider.GetColliderClass() == ColliderClass.AREA)
                    {
                        if (advanced)
                        {
                            if (!collider.GetCollider().IsEnabled())
                            {
                                disabled.Add(collider);
                            }
                            else
                            {
                                collidables.Add(collider);
                            }
                        }
                        else
                        {
                            collidables.Add(collider);
                        }
                    }
                    else
                    {
                        if (advanced)
                        {
                            if (!collider.GetCollider().IsEnabled())
                            {
                                disabled.Add(collider);
                            }
                            else
                            {
                                spatialHash.Add(collider, false);
                                staticCollidables.Add(collider);
                            }
                        }
                        else
                        {
                            spatialHash.Add(collider, false);
                        }
                    }
                }
                else
                {
                    if (advanced)
                    {
                        if (!collider.GetCollider().IsEnabled())
                        {
                            disabled.Add(collider);
                        }
                        else
                        {
                            collidables.Add(collider);
                        }
                    }
                    else
                    {
                        collidables.Add(collider);
                    }
                }
            }
            tempHolding.Clear();

            foreach (var collider in tempRemoving)
            {
                if (collider.GetColliderType() == ColliderType.STATIC)
                {
                    if (advanced)
                    {
                        if (!collider.GetCollider().IsEnabled())
                        {
                            disabled.Remove(collider);
                        }
                        else
                        {
                            spatialHash.Remove(collider, false);
                            staticCollidables.Remove(collider);
                        }
                    }
                    else
                    {
                        spatialHash.Remove(collider, false);
                    }
                }
                else
                {
                    if (advanced)
                    {
                        if (!collider.GetCollider().IsEnabled())
                        {
                            disabled.Remove(collider);
                        }
                        else
                        {
                            collidables.Remove(collider);
                        }
                    }
                    else
                    {
                        collidables.Remove(collider);
                    }
                }
            }
            tempRemoving.Clear();

            foreach (CastInfo info in castInfos)
            {
                if (info.self == null) continue;
                info.self.Collide(info);
            }
            castInfos.Clear();

            foreach (OverlapInfo info in overlapInfos)
            {
                if (info.other == null || info.self == null) continue;
                info.other.Overlap(info);
                info.self.Overlap(info);
            }
            overlapInfos.Clear();
            if (advanced)
            {
                for (int i = disabled.Count - 1; i >= 0; i--)
                {
                    var collider = disabled[i];
                    if (collider.GetCollider().IsEnabled())
                    {
                        if (collider.GetColliderType() == ColliderType.STATIC)
                        {
                            spatialHash.Add(collider, false);
                            staticCollidables.Add(collider);
                        }
                        else
                        {
                            collidables.Add(collider);
                        }
                        disabled.RemoveAt(i);
                    }
                }
            }
        }


        public List<ICollidable> CastSpace(ICollidable caster)
        {
            List<ICollidable> bodies = new();
            List<ICollidable> objects = spatialHash.GetObjects(caster);
            foreach (ICollidable obj in objects)
            {
                if (caster.GetCollisionMask().Length <= 0)
                {
                    if (Overlap.Check(caster.GetCollider(), obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (Overlap.Check(caster.GetCollider(), obj.GetCollider()))
                    {
                        if (caster.GetCollisionMask().Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }
            }
            return bodies;
        }
        public List<ICollidable> CastSpace(Collider collider, string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }
            }
            return bodies;
        }
        public List<ICollidable> CastSpace(Rectangle rect, string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            RectCollider collider = new(new(rect.x, rect.y), new(rect.width, rect.height));
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }

            }
            return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 pos, float r, string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            CircleCollider collider = new(pos, r);
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }

            }
            return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 pos, Vector2 size, string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            RectCollider collider = new(pos, size);
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }

            }
            return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 pos, Vector2 dir, float length, string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            SegmentCollider collider = new(pos, dir, length);
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }

            }
            return bodies;
        }


        public CastInfo RayCastSpace(Vector2 pos, Vector2 dir, float length, string[] collisionMask)
        {
            throw new NotImplementedException();
        }
        public CastInfo RayCastSpace(SegmentCollider ray, string[] collisionMask)
        {
            throw new NotImplementedException();
        }

        public void DebugDrawGrid(Color border, Color fill)
        {
            spatialHash.DebugDrawGrid(border, fill);
        }
    }



    public static class Overlap
    {
        //exact point line, point segment and point point overlap calculations are used if <= 0
        public static readonly float POINT_OVERLAP_EPSILON = 5.0f; //point line and point segment overlap makes more sense when the point is a circle (epsilon = radius)
        public static bool Check(ICollidable a, ICollidable b)
        {
            if (a == b) return false;
            if (a == null || b == null) return false;
            return Check(a.GetCollider(), b.GetCollider());
        }
        public static bool Check(Collider shapeA, Collider shapeB)
        {
            if (shapeA == shapeB) return false;
            if (shapeA == null || shapeB == null) return false;
            if (!shapeA.IsEnabled() || !shapeB.IsEnabled()) return false;
            if (shapeA is CircleCollider)
            {
                if (shapeB is CircleCollider)
                {
                    return Simple(shapeA as CircleCollider, shapeB as CircleCollider);
                }
                /*else if (shapeB is RayShape)
                {
                    return Simple(shapeA as CircleCollider, shapeB as RayShape);
                }*/
                else if (shapeB is SegmentCollider)
                {
                    return Simple(shapeA as CircleCollider, shapeB as SegmentCollider);
                }
                /*else if (shapeB is LineShape)
                {
                    return Simple(shapeA as CircleCollider, shapeB as LineShape);
                }*/
                else if (shapeB is RectCollider)
                {
                    return Simple(shapeA as CircleCollider, shapeB as RectCollider);
                }
                else if (shapeB is OrientedRectCollider)
                {
                    return Simple(shapeA as CircleCollider, shapeB as OrientedRectCollider);
                }
                else
                {
                    return Simple(shapeA as CircleCollider, shapeB);
                }
            }
            /*else if (shapeA is RayShape)
            {
                if (shapeB is CircleCollider)
                {
                    return Simple(shapeA as RayShape, shapeB as CircleCollider);
                }
                else if (shapeB is RayShape)
                {
                    return Simple(shapeA as RayShape, shapeB as RayShape);
                }
                else if (shapeB is SegmentCollider)
                {
                    return Simple(shapeA as RayShape, shapeB as SegmentCollider);
                }
                else if (shapeB is LineShape)
                {
                    return Simple(shapeA as RayShape, shapeB as LineShape);
                }
                else if (shapeB is RectCollider)
                {
                    return Simple(shapeA as RayShape, shapeB as RectCollider);
                }
                else if (shapeB is OrientedRectCollider)
                {
                    return Simple(shapeA as RayShape, shapeB as OrientedRectCollider);
                }
                else
                {
                    return Simple(shapeA as RayShape, shapeB);
                }
            }*/
            else if (shapeA is SegmentCollider)
            {
                if (shapeB is CircleCollider)
                {
                    return Simple(shapeA as SegmentCollider, shapeB as CircleCollider);
                }
                /*else if (shapeB is RayShape)
                {
                    return Simple(shapeA as SegmentCollider, shapeB as RayShape);
                }*/
                else if (shapeB is SegmentCollider)
                {
                    return Simple(shapeA as SegmentCollider, shapeB as SegmentCollider);
                }
                /*else if (shapeB is LineShape)
                {
                    return Simple(shapeA as SegmentCollider, shapeB as LineShape);
                }*/
                else if (shapeB is RectCollider)
                {
                    return Simple(shapeA as SegmentCollider, shapeB as RectCollider);
                }
                else if (shapeB is OrientedRectCollider)
                {
                    return Simple(shapeA as SegmentCollider, shapeB as OrientedRectCollider);
                }
                else
                {
                    return Simple(shapeA as SegmentCollider, shapeB);
                }
            }
            /*else if (shapeA is LineShape)
            {
                if (shapeB is CircleCollider)
                {
                    return Simple(shapeA as LineShape, shapeB as CircleCollider);
                }
                else if (shapeB is RayShape)
                {
                    return Simple(shapeA as LineShape, shapeB as RayShape);
                }
                else if (shapeB is SegmentCollider)
                {
                    return Simple(shapeA as LineShape, shapeB as SegmentCollider);
                }
                else if (shapeB is LineShape)
                {
                    return Simple(shapeA as LineShape, shapeB as LineShape);
                }
                else if (shapeB is RectCollider)
                {
                    return Simple(shapeA as LineShape, shapeB as RectCollider);
                }
                else if (shapeB is OrientedRectCollider)
                {
                    return Simple(shapeA as LineShape, shapeB as OrientedRectCollider);
                }
                else
                {
                    return Simple(shapeA as LineShape, shapeB);
                }
            }*/
            else if (shapeA is RectCollider)
            {
                if (shapeB is CircleCollider)
                {
                    return Simple(shapeA as RectCollider, shapeB as CircleCollider);
                }
                /*else if (shapeB is RayShape)
                {
                    return Simple(shapeA as RectCollider, shapeB as RayShape);
                }*/
                else if (shapeB is SegmentCollider)
                {
                    return Simple(shapeA as RectCollider, shapeB as SegmentCollider);
                }
                /*else if (shapeB is LineShape)
                {
                    return Simple(shapeA as RectCollider, shapeB as LineShape);
                }*/
                else if (shapeB is RectCollider)
                {
                    return Simple(shapeA as RectCollider, shapeB as RectCollider);
                }
                else if (shapeB is OrientedRectCollider)
                {
                    return Simple(shapeA as RectCollider, shapeB as OrientedRectCollider);
                }
                else
                {
                    return Simple(shapeA as RectCollider, shapeB);
                }
            }
            else if (shapeA is OrientedRectCollider)
            {
                if (shapeB is CircleCollider)
                {
                    return Simple(shapeA as OrientedRectCollider, shapeB as CircleCollider);
                }
                /*else if (shapeB is RayShape)
                {
                    return Simple(shapeA as OrientedRectCollider, shapeB as RayShape);
                }*/
                else if (shapeB is SegmentCollider)
                {
                    return Simple(shapeA as OrientedRectCollider, shapeB as SegmentCollider);
                }
                /*else if (shapeB is LineShape)
                {
                    return Simple(shapeA as OrientedRectCollider, shapeB as LineShape);
                }*/
                else if (shapeB is RectCollider)
                {
                    return Simple(shapeA as OrientedRectCollider, shapeB as RectCollider);
                }
                else if (shapeB is OrientedRectCollider)
                {
                    return Simple(shapeA as OrientedRectCollider, shapeB as OrientedRectCollider);
                }
                else
                {
                    return Simple(shapeA as OrientedRectCollider, shapeB);
                }
            }
            else
            {
                if (shapeB is CircleCollider)
                {
                    return Simple(shapeA, shapeB as CircleCollider);
                }
                /*else if (shapeB is RayShape)
                {
                    return Simple(shapeA, shapeB as RayShape);
                }*/
                else if (shapeB is SegmentCollider)
                {
                    return Simple(shapeA, shapeB as SegmentCollider);
                }
                /*else if (shapeB is LineShape)
                {
                    return Simple(shapeA, shapeB as LineShape);
                }*/
                else if (shapeB is RectCollider)
                {
                    return Simple(shapeA, shapeB as RectCollider);
                }
                else if (shapeB is OrientedRectCollider)
                {
                    return Simple(shapeA, shapeB as OrientedRectCollider);
                }
                else
                {
                    return Simple(shapeA, shapeB);
                }
            }
        }
        public static bool Check(Rectangle rect, Collider shape)
        {
            if (shape == null) return false;
            if (!shape.IsEnabled()) return false;

            if (shape is CircleCollider)
            {
                return Simple(rect, shape as CircleCollider);
            }
            else if (shape is SegmentCollider)
            {
                return Simple(rect, shape as SegmentCollider);
            }
            else if (shape is RectCollider)
            {
                return Simple(rect, shape as RectCollider);
            }
            else if (shape is OrientedRectCollider)
            {
                return Simple(rect, shape as OrientedRectCollider);
            }
            else
            {
                return Simple(rect, shape);
            }
        }

        //OVERLAP with different implementation
        public static bool Simple(Collider a, Collider b) { return OverlapPointPoint(a.Pos, b.Pos); }
        public static bool Simple(Collider p, CircleCollider c) { return OverlapPointCircle(p.Pos, c.Pos, c.Radius); }
        //public static bool Simple(Collider p, LineShape l) { return OverlapPointLine(p.Pos, l.Pos, l.Dir); }
        public static bool Simple(Collider p, SegmentCollider s) { return OverlapPointSegment(p.Pos, s.Pos, s.GetEnd()); }
        public static bool Simple(Collider p, RectCollider r) { return OverlapPointRect(p.Pos, r.Pos, r.Size); }
        public static bool Simple(Collider p, OrientedRectCollider or) { return OverlapPointOrientedRect(p.Pos, or.Pos, or.HalfSize, or.Rotation); }
        public static bool Simple(CircleCollider a, CircleCollider b) { return OverlapCircleCircle(a.Pos, a.Radius, b.Pos, b.Radius); }
        public static bool Simple(CircleCollider c, Collider p) { return OverlapCirclePoint(c.Pos, c.Radius, p.Pos); }
        //public static bool Simple(CircleCollider c, LineShape l) { return OverlapCircleLine(c.Pos, c.Radius, l.Pos, l.Dir); }
        public static bool Simple(CircleCollider c, SegmentCollider s) { return OverlapCircleSegment(c.Pos, c.Radius, s.Pos, s.GetEnd()); }
        public static bool Simple(CircleCollider c, RectCollider r) { return OverlapCircleRect(c.Pos, c.Radius, r.Pos, r.Size); }
        public static bool Simple(CircleCollider c, OrientedRectCollider or) { return OverlapCircleOrientedRect(c.Pos, c.Radius, or.Pos, or.HalfSize, or.Rotation); }
        //public static bool Simple(LineShape a, LineShape b) { return OverlapLineLine(a.Pos, a.Dir, b.Pos, b.Dir); }
        //public static bool Simple(LineShape l, Collider p) { return OverlapLinePoint(l.Pos, l.Dir, p.Pos); }
        //public static bool Simple(LineShape l, CircleCollider c) { return OverlapLineCircle(l.Pos, l.Dir, c.Pos, c.Radius); }
        //public static bool Simple(LineShape l, SegmentCollider s) { return OverlapLineSegment(l.Pos, l.Dir, s.Pos, s.End); }
        //public static bool Simple(LineShape l, RectCollider r) { return OverlapLineRect(l.Pos, l.Dir, r.Pos, r.Size); }
        //public static bool Simple(LineShape l, OrientedRectCollider or) { return OverlapLineOrientedRect(l.Pos, l.Dir, or.Pos, or.HalfSize, or.Rotation); }
        public static bool Simple(SegmentCollider a, SegmentCollider b) { return OverlapSegmentSegment(a.Pos, a.GetEnd(), b.Pos, b.GetEnd()); }
        public static bool Simple(SegmentCollider s, Collider p) { return OverlapSegmentPoint(s.Pos, s.GetEnd(), p.Pos); }
        public static bool Simple(SegmentCollider s, CircleCollider c) { return OverlapSegmentCircle(s.Pos, s.GetEnd(), c.Pos, c.Radius); }
        //public static bool Simple(SegmentCollider s, LineShape l) { return OverlapSegmentLine(s.Pos, s.End, l.Pos, l.Dir); }
        public static bool Simple(SegmentCollider s, RectCollider r) { return OverlapSegmentRect(s.Pos, s.GetEnd(), r.Pos, r.Size); }
        public static bool Simple(SegmentCollider s, OrientedRectCollider or) { return OverlapSegmentOrientedRect(s.Pos, s.GetEnd(), or.Pos, or.HalfSize, or.Rotation); }
        public static bool Simple(RectCollider a, RectCollider b) { return OverlapRectRect(a.Pos, a.Size, b.Pos, b.Size); }
        public static bool Simple(RectCollider r, Collider p) { return OverlapRectPoint(r.Pos, r.Size, p.Pos); }
        public static bool Simple(RectCollider r, CircleCollider c) { return OverlapRectCircle(r.Pos, r.Size, c.Pos, c.Radius); }
        //public static bool Simple(RectCollider r, LineShape l) { return OverlapRectLine(r.Pos, r.Size, l.Pos, l.Dir); }
        public static bool Simple(RectCollider r, SegmentCollider s) { return OverlapRectSegment(r.Pos, r.Size, s.Pos, s.GetEnd()); }
        public static bool Simple(RectCollider r, OrientedRectCollider or) { return OverlapRectOrientedRect(r.Pos, r.Size, or.Pos, or.HalfSize, or.Rotation); }
        public static bool Simple(OrientedRectCollider a, OrientedRectCollider b) { return OverlapOrientedRectOrientedRect(a.Pos, a.HalfSize, a.Rotation, b.Pos, b.HalfSize, b.Rotation); }
        public static bool Simple(OrientedRectCollider or, Collider p) { return OverlapOrientedRectPoint(or.Pos, or.HalfSize, or.Rotation, p.Pos); }
        public static bool Simple(OrientedRectCollider or, CircleCollider c) { return OverlapOrientedRectCircle(or.Pos, or.HalfSize, or.Rotation, c.Pos, c.Radius); }
        //public static bool Simple(OrientedRectCollider or, LineShape l) { return OverlapOrientedRectLine(or.Pos, or.HalfSize, or.Rotation, l.Pos, l.Dir); }
        public static bool Simple(OrientedRectCollider or, SegmentCollider s) { return OverlapOrientedRectSegment(or.Pos, or.HalfSize, or.Rotation, s.Pos, s.GetEnd()); }
        public static bool Simple(OrientedRectCollider or, RectCollider r) { return OverlapOrientedRectRect(or.Pos, or.HalfSize, or.Rotation, r.Pos, r.Size); }

        public static bool Simple(Rectangle rect, Collider p)
        {
            return OverlapRectPoint(new(rect.x, rect.y), new(rect.width, rect.height), p.Pos);
        }
        public static bool Simple(Rectangle rect, Vector2 pos)
        {
            return OverlapRectPoint(new(rect.x, rect.y), new(rect.width, rect.height), pos);
        }
        public static bool Simple(Rectangle rect, CircleCollider c)
        {
            return OverlapRectCircle(new(rect.x, rect.y), new(rect.width, rect.height), c.Pos, c.Radius);
        }
        public static bool Simple(Rectangle rect, SegmentCollider s)
        {
            return OverlapRectSegment(new(rect.x, rect.y), new(rect.width, rect.height), s.Pos, s.GetEnd());
        }
        public static bool Simple(Rectangle rect, RectCollider r)
        {
            return OverlapRectRect(new(rect.x, rect.y), new(rect.width, rect.height), r.Pos, r.Size);
        }
        public static bool Simple(Rectangle a, Rectangle b)
        {
            return OverlapRectRect(new(a.X, a.Y), new(a.width, a.height), new(b.X, b.Y), new(b.width, b.height));
        }
        public static bool Simple(Rectangle rect, OrientedRectCollider or)
        {
            return OverlapRectOrientedRect(new(rect.x, rect.y), new(rect.width, rect.height), or.Pos, or.HalfSize, or.Rotation);
        }

        public static bool OverlapPointPoint(Vector2 pointA, Vector2 pointB)
        {
            if (POINT_OVERLAP_EPSILON > 0.0f) { return OverlapCircleCircle(pointA, POINT_OVERLAP_EPSILON, pointB, POINT_OVERLAP_EPSILON); }
            else return (int)pointA.X == (int)pointB.X && (int)pointA.Y == (int)pointB.Y;
        }
        public static bool OverlapPointCircle(Vector2 point, Vector2 circlePos, float circleRadius)
        {
            if (circleRadius <= 0.0f) return OverlapPointPoint(point, circlePos);
            return (circlePos - point).LengthSquared() <= circleRadius * circleRadius;
        }
        public static bool OverlapPointLine(Vector2 point, Vector2 linePos, Vector2 lineDir)
        {
            if (POINT_OVERLAP_EPSILON > 0.0f) return OverlapCircleLine(point, POINT_OVERLAP_EPSILON, linePos, lineDir);
            if (OverlapPointPoint(point, linePos)) return true;
            Vector2 displacement = point - linePos;

            return Vec.Parallel(displacement, lineDir);
        }
        public static bool OverlapPointRay(Vector2 point, Vector2 rayPos, Vector2 rayDir)
        {
            Vector2 displacement = point - rayPos;
            float p = rayDir.Y * displacement.X - rayDir.X * displacement.Y;
            if (p != 0.0f) return false;
            float d = displacement.X * rayDir.X + displacement.Y * rayDir.Y;
            return d >= 0;
        }
        public static bool OverlapRayPoint(Vector2 point, Vector2 rayPos, Vector2 rayDir)
        {
            return OverlapPointRay(point, rayPos, rayDir);
        }
        public static bool OverlapPointSegment(Vector2 point, Vector2 segmentPos, Vector2 segmentDir, float segmentlength)
        {
            return OverlapPointSegment(point, segmentPos, segmentPos + segmentDir * segmentlength);
        }
        public static bool OverlapPointSegment(Vector2 point, Vector2 segmentPos, Vector2 segmentEnd)
        {
            if (POINT_OVERLAP_EPSILON > 0.0f) return OverlapCircleSegment(point, POINT_OVERLAP_EPSILON, segmentPos, segmentEnd);
            Vector2 d = segmentEnd - segmentPos;
            Vector2 lp = point - segmentPos;
            Vector2 p = Vec.Project(lp, d);
            return lp == p && p.LengthSquared() <= d.LengthSquared() && Vector2.Dot(p, d) >= 0.0f;
        }
        public static bool OverlapPointRect(Vector2 point, Vector2 rectPos, Vector2 rectSize)
        {
            float left = rectPos.X;
            float top = rectPos.Y;
            float right = rectPos.X + rectSize.X;
            float bottom = rectPos.Y + rectSize.Y;

            return left <= point.X && right >= point.X && top <= point.Y && bottom >= point.Y;
        }
        public static bool OverlapPointOrientedRect(Vector2 point, Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation)
        {
            Vector2 rectPos = new(0.0f, 0.0f);
            Vector2 size = rectHalfSize * 2.0f;

            Vector2 lp = Vec.Rotate(point - rectCenter, -rectRotation) + rectHalfSize;

            return OverlapPointRect(lp, rectPos, size);
        }
        public static bool OverlapCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius)
        {
            if (aRadius <= 0.0f && bRadius > 0.0f) return OverlapPointCircle(aPos, bPos, bRadius);
            else if (bRadius <= 0.0f && aRadius > 0.0f) return OverlapPointCircle(bPos, aPos, aRadius);
            else if (aRadius <= 0.0f && bRadius <= 0.0f) return OverlapPointPoint(aPos, bPos);
            float rSum = aRadius + bRadius;

            return (aPos - bPos).LengthSquared() < rSum * rSum;
        }
        public static bool OverlapCirclePoint(Vector2 circlePos, float circleRadius, Vector2 point)
        {
            return OverlapPointCircle(point, circlePos, circleRadius);
        }
        public static bool OverlapCircleLine(Vector2 circlePos, float circleRadius, Vector2 linePos, Vector2 lineDir)
        {
            Vector2 lc = circlePos - linePos;
            Vector2 p = Vec.Project(lc, lineDir);
            Vector2 nearest = linePos + p;
            return OverlapPointCircle(nearest, circlePos, circleRadius);
        }

        public static bool OverlapCircleRay(Vector2 circlePos, float circleRadius, Vector2 rayPos, Vector2 rayDir)
        {
            return OverlapRayCircle(rayPos, rayDir, circlePos, circleRadius);
        }
        public static bool OverlapRayCircle(Vector2 rayPos, Vector2 rayDir, Vector2 circlePos, float circleRadius)
        {
            Vector2 w = circlePos - rayPos;
            float p = w.X * rayDir.Y - w.Y * rayDir.X;
            if (p < -circleRadius || p > circleRadius) return false;
            float t = w.X * rayDir.X + w.Y * rayDir.Y;
            if (t < 0.0f)
            {
                float d = w.LengthSquared();
                if (d > circleRadius * circleRadius) return false;
            }
            return true;
        }
        public static bool OverlapCircleSegment(Vector2 circlePos, float circleRadius, Vector2 segmentPos, Vector2 segmentDir, float segmentLength)
        {
            return OverlapCircleSegment(circlePos, circleRadius, segmentPos, segmentPos + segmentDir * segmentLength);
        }
        public static bool OverlapCircleSegment(Vector2 circlePos, float circleRadius, Vector2 segmentPos, Vector2 segmentEnd)
        {
            if (circleRadius <= 0.0f) return OverlapPointSegment(circlePos, segmentPos, segmentEnd);
            if (OverlapPointCircle(segmentPos, circlePos, circleRadius)) return true;
            if (OverlapPointCircle(segmentEnd, circlePos, circleRadius)) return true;

            Vector2 d = segmentEnd - segmentPos;
            Vector2 lc = circlePos - segmentPos;
            Vector2 p = Vec.Project(lc, d);
            Vector2 nearest = segmentPos + p;

            //bool nearestInside = OverlapPointCircle(nearest, circlePos, circleRadius);
            //bool smaller = p.LengthSquared() <= d.LengthSquared();
            //float dot = Vector2.Dot(p, d);

            return
                OverlapPointCircle(nearest, circlePos, circleRadius) &&
                p.LengthSquared() <= d.LengthSquared() &&
                Vector2.Dot(p, d) >= 0.0f;
        }
        public static bool OverlapCircleRect(Vector2 circlePos, float circleRadius, Vector2 rectPos, Vector2 rectSize)
        {
            if (circleRadius <= 0.0f) return OverlapPointRect(circlePos, rectPos, rectSize);
            return OverlapPointCircle(ClampOnRect(circlePos, rectPos, rectSize), circlePos, circleRadius);
        }
        public static bool OverlapCircleOrientedRect(Vector2 circlePos, float circleRadius, Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation)
        {
            if (circleRadius < 0.0f) return OverlapPointOrientedRect(circlePos, rectCenter, rectHalfSize, rectRotation);
            //local rect
            Vector2 lrPos = new(0.0f, 0.0f);
            Vector2 lrSize = rectHalfSize * 2.0f;

            //local circle
            Vector2 dis = Vec.Rotate(circlePos - rectCenter, -rectRotation);
            Vector2 lcPos = dis + rectHalfSize;
            return OverlapCircleRect(lcPos, circleRadius, lrPos, lrSize);
        }
        public static bool OverlapLineLine(Vector2 aPos, Vector2 aDir, Vector2 bPos, Vector2 bDir)
        {
            if (Vec.Parallel(aDir, bDir))
            {
                Vector2 displacement = aPos - bPos;
                return Vec.Parallel(displacement, aDir);
            }
            return true;
        }
        public static bool OverlapLinePoint(Vector2 linePos, Vector2 lineDir, Vector2 point)
        {
            return OverlapPointLine(point, linePos, lineDir);
        }
        public static bool OverlapLineCircle(Vector2 linePos, Vector2 lineDir, Vector2 circlePos, float circleRadius)
        {
            return OverlapCircleLine(circlePos, circleRadius, linePos, lineDir);
        }
        public static bool OverlapLineSegment(Vector2 linePos, Vector2 lineDir, Vector2 segmentPos, Vector2 segmentDir, float segmentLength)
        {
            return OverlapLineSegment(linePos, lineDir, segmentPos, segmentPos + segmentDir * segmentLength);
        }
        public static bool OverlapLineSegment(Vector2 linePos, Vector2 lineDir, Vector2 segmentPos, Vector2 segmentEnd)
        {
            return !SegmentOnOneSide(linePos, lineDir, segmentPos, segmentEnd);
        }
        public static bool OverlapLineRect(Vector2 linePos, Vector2 lineDir, Vector2 rectPos, Vector2 rectSize)
        {
            Vector2 n = Vec.Rotate90CCW(lineDir);

            Vector2 c1 = rectPos;
            Vector2 c2 = rectPos + rectSize;
            Vector2 c3 = new(c2.X, c1.Y);
            Vector2 c4 = new(c1.X, c2.Y);

            c1 -= linePos;
            c2 -= linePos;
            c3 -= linePos;
            c4 -= linePos;

            float dp1 = Vector2.Dot(n, c1);
            float dp2 = Vector2.Dot(n, c2);
            float dp3 = Vector2.Dot(n, c3);
            float dp4 = Vector2.Dot(n, c4);

            return dp1 * dp2 <= 0.0f || dp2 * dp3 <= 0.0f || dp3 * dp4 <= 0.0f;
        }
        public static bool OverlapLineOrientedRect(Vector2 linePos, Vector2 lineDir, Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation)
        {
            Vector2 rPos = new(0.0f, 0.0f);
            Vector2 rSize = rectHalfSize * 2.0f;
            Vector2 lPos = Vec.Rotate(linePos - rectCenter, -rectRotation) + rectHalfSize;
            Vector2 lDir = Vec.Rotate(lineDir, -rectRotation);
            return OverlapLineRect(lPos, lDir, rPos, rSize);
        }
        public static bool OverlapSegmentSegment(Vector2 aPos, Vector2 aDir, float aLength, Vector2 bPos, Vector2 bDir, float bLength)
        {
            return OverlapSegmentSegment(aPos, aPos + aDir * aLength, bPos, bPos + bDir * bLength);
        }
        public static bool OverlapSegmentSegment(Vector2 aPos, Vector2 aEnd, Vector2 bPos, Vector2 bEnd)
        {
            Vector2 axisAPos = aPos;
            Vector2 axisADir = aEnd - aPos;
            if (SegmentOnOneSide(axisAPos, axisADir, bPos, bEnd)) return false;

            Vector2 axisBPos = bPos;
            Vector2 axisBDir = bEnd - bPos;
            if (SegmentOnOneSide(axisBPos, axisBDir, aPos, aEnd)) return false;

            if (Vec.Parallel(axisADir, axisBDir))
            {
                Range rangeA = ProjectSegment(aPos, aEnd, axisADir);
                Range rangeB = ProjectSegment(bPos, bEnd, axisADir);
                return OverlappingRange(rangeA, rangeB);
            }
            return true;
        }
        public static bool OverlapSegmentPoint(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 point)
        {
            return OverlapPointSegment(point, segmentPos, segmentPos + segmentDir * segmentLength);
        }
        public static bool OverlapSegmentPoint(Vector2 segmentPos, Vector2 segmentEnd, Vector2 point)
        {
            return OverlapPointSegment(point, segmentPos, segmentEnd);
        }
        public static bool OverlapSegmentCircle(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 circlePos, float circleRadius)
        {
            return OverlapCircleSegment(circlePos, circleRadius, segmentPos, segmentPos + segmentDir * segmentLength);
        }
        public static bool OverlapSegmentCircle(Vector2 segmentPos, Vector2 segmentEnd, Vector2 circlePos, float circleRadius)
        {
            return OverlapCircleSegment(circlePos, circleRadius, segmentPos, segmentEnd);
        }
        public static bool OverlapSegmentLine(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 linePos, Vector2 lineDir)
        {
            return OverlapLineSegment(linePos, lineDir, segmentPos, segmentPos + segmentDir * segmentLength);
        }
        public static bool OverlapSegmentLine(Vector2 segmentPos, Vector2 segmentEnd, Vector2 linePos, Vector2 lineDir)
        {
            return OverlapLineSegment(linePos, lineDir, segmentPos, segmentEnd);
        }
        public static bool OverlapSegmentRect(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 rectPos, Vector2 rectSize)
        {
            return OverlapSegmentRect(segmentPos, segmentPos + segmentDir * segmentLength, rectPos, rectSize);
        }
        public static bool OverlapSegmentRect(Vector2 segmentPos, Vector2 segmentEnd, Vector2 rectPos, Vector2 rectSize)
        {
            if (!OverlapLineRect(segmentPos, segmentEnd - segmentPos, rectPos, rectSize)) return false;
            Range rectRange = new
                (
                    rectPos.X,
                    rectPos.X + rectSize.X
                );
            Range segmentRange = new
                (
                    segmentPos.X,
                    segmentEnd.X
                );

            if (!OverlappingRange(rectRange, segmentRange)) return false;

            rectRange.min = rectPos.Y;
            rectRange.max = rectPos.Y + rectSize.Y;
            rectRange.Sort();

            segmentRange.min = segmentPos.Y;
            segmentRange.max = segmentEnd.Y;
            segmentRange.Sort();

            return OverlappingRange(rectRange, segmentRange);
        }
        public static bool OverlapSegmentOrientedRect(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation)
        {
            return OverlapSegmentOrientedRect(segmentPos, segmentPos + segmentDir * segmentLength, rectCenter, rectHalfSize, rectRotation);
        }
        public static bool OverlapSegmentOrientedRect(Vector2 segmentPos, Vector2 segmentEnd, Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation)
        {
            Vector2 rPos = new(0.0f, 0.0f);
            Vector2 rSize = rectHalfSize * 2.0f;

            Vector2 sStart = Vec.Rotate(segmentPos - rectCenter, -rectRotation) + rectHalfSize;
            Vector2 sEnd = Vec.Rotate(segmentEnd - rectCenter, -rectRotation) + rectHalfSize;

            return OverlapSegmentRect(sStart, sEnd, rPos, rSize);
        }
        public static bool OverlapRectRect(Rectangle a, Rectangle b)
        {
            Vector2 aTopLeft = new(a.x, a.y);
            Vector2 aBottomRight = aTopLeft + new Vector2(a.width, a.height);
            Vector2 bTopLeft = new(b.x, b.y);
            Vector2 bBottomRight = bTopLeft + new Vector2(b.width, b.height);
            return
                OverlappingRange(aTopLeft.X, aBottomRight.X, bTopLeft.X, bBottomRight.X) &&
                OverlappingRange(aTopLeft.Y, aBottomRight.Y, bTopLeft.Y, bBottomRight.Y);
        }
        public static bool OverlapRectRect(Vector2 aPos, Vector2 aSize, Vector2 bPos, Vector2 bSize)
        {
            Vector2 aTopLeft = aPos;
            Vector2 aBottomRight = aPos + aSize;
            Vector2 bTopLeft = bPos;
            Vector2 bBottomRight = bPos + bSize;
            return
                OverlappingRange(aTopLeft.X, aBottomRight.X, bTopLeft.X, bBottomRight.X) &&
                OverlappingRange(aTopLeft.Y, aBottomRight.Y, bTopLeft.Y, bBottomRight.Y);
        }
        public static bool OverlapRectPoint(Vector2 rectPos, Vector2 rectSize, Vector2 point)
        {
            return OverlapPointRect(point, rectPos, rectSize);
        }
        public static bool OverlapRectCircle(Vector2 rectPos, Vector2 rectSize, Vector2 circlePos, float circleRadius)
        {
            return OverlapCircleRect(circlePos, circleRadius, rectPos, rectSize);
        }
        public static bool OverlapRectLine(Vector2 rectPos, Vector2 rectSize, Vector2 linePos, Vector2 lineDir)
        {
            return OverlapLineRect(linePos, lineDir, rectPos, rectSize);
        }
        public static bool OverlapRectSegment(Vector2 rectPos, Vector2 rectSize, Vector2 segmentPos, Vector2 segmentDir, float segmentLength)
        {
            return OverlapSegmentRect(segmentPos, segmentDir, segmentLength, rectPos, rectSize);
        }
        public static bool OverlapRectSegment(Vector2 rectPos, Vector2 rectSize, Vector2 segmentPos, Vector2 segmentEnd)
        {
            return OverlapSegmentRect(segmentPos, segmentEnd, rectPos, rectSize);
        }
        public static bool OverlapRectOrientedRect(Vector2 aPos, Vector2 aSize, Vector2 bCenter, Vector2 bHalfSize, float bRotation)
        {
            (Vector2 pos, Vector2 size) orientedHull = OrientedRectHull(bCenter, bHalfSize, bRotation);
            if (!OverlapRectRect(orientedHull.pos, orientedHull.size, aPos, aSize)) return false;

            (Vector2 start, Vector2 end) edge = OrientedRectEdge(bCenter, bHalfSize, bRotation, 0);
            if (SeperateAxisRect(edge.start, edge.end, aPos, aSize)) return false;

            edge = OrientedRectEdge(bCenter, bHalfSize, bRotation, 1);
            return !SeperateAxisRect(edge.start, edge.end, aPos, aSize);
        }
        public static bool OverlapOrientedRectOrientedRect(Vector2 aCenter, Vector2 aHalfSize, float aRotation, Vector2 bCenter, Vector2 bHalfSize, float bRotation)
        {
            (Vector2 start, Vector2 end) edge = OrientedRectEdge(aCenter, aHalfSize, aRotation, 0);
            if (SeperateAxisOrientedRect(edge.start, edge.end, bCenter, bHalfSize, bRotation)) return false;

            edge = OrientedRectEdge(aCenter, aHalfSize, aRotation, 1);
            if (SeperateAxisOrientedRect(edge.start, edge.end, bCenter, bHalfSize, bRotation)) return false;

            edge = OrientedRectEdge(bCenter, bHalfSize, bRotation, 0);
            if (SeperateAxisOrientedRect(edge.start, edge.end, aCenter, aHalfSize, aRotation)) return false;

            edge = OrientedRectEdge(bCenter, bHalfSize, bRotation, 1);
            return !SeperateAxisOrientedRect(edge.start, edge.end, aCenter, aHalfSize, aRotation);
        }
        public static bool OverlapOrientedRectPoint(Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation, Vector2 point)
        {
            return OverlapPointOrientedRect(point, rectCenter, rectHalfSize, rectRotation);
        }
        public static bool OverlapOrientedRectCircle(Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation, Vector2 circlePos, float circleRadius)
        {
            return OverlapCircleOrientedRect(circlePos, circleRadius, rectCenter, rectHalfSize, rectRotation);
        }
        public static bool OverlapOrientedRectLine(Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation, Vector2 linePos, Vector2 lineDir)
        {
            return OverlapLineOrientedRect(linePos, lineDir, rectCenter, rectHalfSize, rectRotation);
        }
        public static bool OverlapOrientedRectSegment(Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation, Vector2 segmentPos, Vector2 segmentDir, float segmentLength)
        {
            return OverlapSegmentOrientedRect(segmentPos, segmentDir, segmentLength, rectCenter, rectHalfSize, rectRotation);
        }
        public static bool OverlapOrientedRectSegment(Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation, Vector2 segmentPos, Vector2 segmentEnd)
        {
            return OverlapSegmentOrientedRect(segmentPos, segmentEnd, rectCenter, rectHalfSize, rectRotation);
        }
        public static bool OverlapOrientedRectRect(Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation, Vector2 rectPos, Vector2 rectSize)
        {
            return OverlapRectOrientedRect(rectPos, rectSize, rectCenter, rectHalfSize, rectRotation);
        }




        //HELPER FUNCTIONS
        private static bool OverlappingRange(float minA, float maxA, float minB, float maxB)
        {
            if (maxA < minA)
            {
                float temp = minA;
                minA = maxA;
                maxA = temp;
            }
            if (maxB < minB)
            {
                float temp = minB;
                minB = maxB;
                maxB = temp;
            }
            //return minA < maxB && maxA > minB;
            return minB <= maxA && minA <= maxB;
        }
        private static bool OverlappingRange(Range a, Range b)
        {
            return OverlappingRange(a.min, a.max, b.min, b.max);
        }
        private static Range RangeHull(Range a, Range b)
        {
            return new
                (
                    a.min < b.min ? a.min : b.min,
                    a.max > b.max ? a.max : b.max
                );
        }
        private static bool SegmentOnOneSide(Vector2 axisPos, Vector2 axisDir, Vector2 segmentPos, Vector2 segmentEnd)
        {
            Vector2 d1 = segmentPos - axisPos;
            Vector2 d2 = segmentEnd - axisPos;
            Vector2 n = Vec.Rotate90CCW(axisDir);// new(-axisDir.Y, axisDir.X);
            return Vector2.Dot(n, d1) * Vector2.Dot(n, d2) > 0.0f;
        }
        private static Range ProjectSegment(Vector2 aPos, Vector2 aEnd, Vector2 onto)
        {
            Vector2 unitOnto = Vector2.Normalize(onto);
            Range r = new(Vector2.Dot(unitOnto, aPos), Vector2.Dot(unitOnto, aEnd));
            return r;
        }
        private static (Vector2 start, Vector2 end) OrientedRectEdge(Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation, int edge)
        {
            Vector2 a = rectHalfSize;
            Vector2 b = rectHalfSize;

            switch (edge)
            {
                case 0: a.X = -a.X; break;
                case 1: b.Y = -b.Y; break;
                case 2:
                    a.Y = -a.Y;
                    b = Vector2.Negate(b);
                    break;
                default:
                    a = Vector2.Negate(a);
                    b.X = -b.X;
                    break;
            }

            a = Vec.Rotate(a, rectRotation) + rectCenter;
            b = Vec.Rotate(b, rectRotation) + rectCenter;
            return (a, b);
        }
        private static Vector2 RectCorner(Vector2 rectPos, Vector2 rectSize, int corner)
        {
            Vector2 v = rectPos;
            switch (corner % 4)
            {
                case 0: v.X += rectSize.X; break;
                case 1: v += rectSize; break;
                case 2: v.Y += rectSize.Y; break;
                default:
                    break;
            }
            return v;
        }
        private static Vector2 OrientedRectCorner(Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation, int corner)
        {
            Vector2 c = rectHalfSize;
            switch (corner % 4)
            {
                case 0: c.X = -c.X; break;
                case 1: break;
                case 2: c.Y = -c.Y; break;
                default: c = Vector2.Negate(c); break;
            }
            return Vec.Rotate(c, rectRotation) + rectCenter;
        }
        private static bool SeperateAxisOrientedRect(Vector2 axisStart, Vector2 axisEnd, Vector2 rectCenter, Vector2 rectHalfSize, float rectRotation)
        {
            (Vector2 start, Vector2 end) edge0 = OrientedRectEdge(rectCenter, rectHalfSize, rectRotation, 0);
            (Vector2 start, Vector2 end) edge2 = OrientedRectEdge(rectCenter, rectHalfSize, rectRotation, 2);
            Vector2 n = axisStart - axisEnd;

            Range axisRange = ProjectSegment(axisStart, axisEnd, n);
            Range r0 = ProjectSegment(edge0.start, edge0.end, n);
            Range r2 = ProjectSegment(edge2.start, edge2.end, n);
            Range projection = RangeHull(r0, r2);
            return !OverlappingRange(axisRange, projection);
        }
        private static bool SeperateAxisRect(Vector2 axisStart, Vector2 axisEnd, Vector2 rectPos, Vector2 rectSize)
        {
            Vector2 n = axisStart - axisEnd;
            Vector2 edgeAStart = RectCorner(rectPos, rectSize, 0);
            Vector2 edgeAEnd = RectCorner(rectPos, rectSize, 1);
            Vector2 edgeBStart = RectCorner(rectPos, rectSize, 2);
            Vector2 edgeBEnd = RectCorner(rectPos, rectSize, 3);

            Range edgeARange = ProjectSegment(edgeAStart, edgeAEnd, n);
            Range edgeBRange = ProjectSegment(edgeBStart, edgeBEnd, n);
            Range rProjection = RangeHull(edgeARange, edgeBRange);

            Range axisRange = ProjectSegment(axisStart, axisEnd, n);
            return !OverlappingRange(axisRange, rProjection);
        }
        private static (Vector2 rectPos, Vector2 rectSize) EnlargeRectangle(Vector2 rectPos, Vector2 rectSize, Vector2 point)
        {
            Vector2 newPos = new
                (
                    MathF.Min(rectPos.X, point.X),
                    MathF.Min(rectPos.Y, point.Y)
                );
            Vector2 newSize = new
                (
                    MathF.Max(rectPos.X + rectSize.X, point.X),
                    MathF.Max(rectPos.Y + rectSize.Y, point.Y)
                );

            return (newPos, newSize - newPos);
        }
        public static (Vector2 rectPos, Vector2 rectSize) OrientedRectHull(Vector2 center, Vector2 halfSize, float rotation)
        {
            (Vector2 pos, Vector2 size) rect = (center, new(0.0f, 0.0f));
            Vector2 corner;
            for (int i = 0; i < 4; i++)
            {
                corner = OrientedRectCorner(center, halfSize, rotation, i);
                rect = EnlargeRectangle(rect.pos, rect.size, corner);
            }
            return rect;
        }
        private static float ClampFloat(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            else return v;
        }
        private static Vector2 ClampOnRect(Vector2 p, Vector2 rectPos, Vector2 rectSize)
        {
            return new
                (
                    ClampFloat(p.X, rectPos.X, rectPos.X + rectSize.X),
                    ClampFloat(p.Y, rectPos.Y, rectPos.Y + rectSize.Y)
                );
        }

        /*
        function pointInPolygon(point, shape)

	local firstPoint = true
	local lastPoint = 0
	local rotatedPoint = 0
	local onRight = 0
	local onLeft = 0
	local xCrossing = 0
	
	for index,shapePoint in ipairs(shape.points) do
	
		rotatedPoint = rotatePoint(shapePoint,shape.rotation)
	
		if firstPoint then
			lastPoint = rotatedPoint
			firstPoint = false
		else
			startPoint = {
				x = lastPoint.x + shape.position.x,
			 y = lastPoint.y + shape.position.y
			}
			endPoint = {
				x = rotatedPoint.x + shape.position.x,
				y = rotatedPoint.y + shape.position.y
			}
			if ((startPoint.y >= point.y) and (endPoint.y < point.y))
				or ((startPoint.y < point.y) and (endPoint.y >= point.y)) then
				-- line crosses ray
				if (startPoint.x <= point.x) and (endPoint.x <= point.x) then
					-- line is to left
					onLeft = onLeft + 1
				elseif (startPoint.x >= point.x) and (endPoint.x >= point.x) then
					-- line is to right
					onRight = onRight + 1
				else
					-- need to calculate crossing x coordinate
					if (startPoint.y ~= endPoint.y) then
						-- filter out horizontal line
						xCrossing = startPoint.x +
							((point.y - startPoint.y)
							* (endPoint.x - startPoint.x)
							/ (endPoint.y - startPoint.y))
						if (xCrossing >= point.x) then
							onRight = onRight + 1
						else
							onLeft = onLeft + 1
						end -- if
					end -- if horizontal
				end -- if
			end -- if crosses ray
				
			lastPoint = rotatedPoint
		end
	
	end -- for

	-- only need to check on side
	if (onRight % 2) == 1 then
		-- odd = inside
		return true
	else
		return false
	end

end -- pointInPolygon

        */
    }
    public static class Collision
    {
        public static CastInfo CheckCast(ICollidable a, ICollidable b, float dt)
        {
            if (a == b) return new();
            if (a == null || b == null) return new();
            CastInfo castInfo = CheckCast(a.GetCollider(), b.GetCollider(), dt);
            castInfo.self = a;
            castInfo.other = b;
            return castInfo;
        }
        public static CastInfo CheckCast(Collider shapeA, Collider shapeB, float dt)
        {
            if (shapeA is RectCollider || shapeA is OrientedRectCollider || shapeB is RectCollider || shapeB is OrientedRectCollider) return new();
            if (shapeA == null || shapeB == null) return new();
            if (!shapeA.IsEnabled() || !shapeB.IsEnabled()) return new();

            if (shapeA is CircleCollider)
            {

                if (shapeB is CircleCollider)
                {
                    return Cast(shapeA as CircleCollider, shapeB as CircleCollider, dt);
                }
                else if (shapeB is SegmentCollider)
                {
                    return Cast(shapeA as CircleCollider, shapeB as SegmentCollider, dt);
                }
                /*else if (shapeB is RayShape)
                {
                    return Cast(shapeA as CircleCollider, shapeB as RayShape, dt);
                }
                else if (shapeB is LineShape)
                {
                    return Cast(shapeA as CircleCollider, shapeB as LineShape, dt);
                }*/
                else if (shapeB is Collider)
                {
                    return Cast(shapeA as CircleCollider, shapeB as Collider, dt);
                }
            }
            else if (shapeA is Collider)
            {
                if (shapeB is CircleCollider)
                {
                    return Cast(shapeA as Collider, shapeB as CircleCollider, dt);
                }
                else if (shapeB is SegmentCollider)
                {
                    return Cast(shapeA as Collider, shapeB as SegmentCollider, dt);
                }
                /*else if (shapeB is RayShape)
                {
                    return Cast(shapeA as Collider, shapeB as RayShape, dt);
                }
                else if (shapeB is LineShape)
                {
                    return Cast(shapeA as Collider, shapeB as LineShape, dt);
                }*/
                else if (shapeB is Collider)
                {
                    return Cast(shapeA as Collider, shapeB as Collider, dt);
                }
            }

            return new();
        }


        //CONTAINS
        //Circle - Point/Circle/Rect/Line
        public static bool Contains(Vector2 circlePos, float r, Vector2 point)
        {
            Vector2 dif = circlePos - point;
            return dif.LengthSquared() < r * r;
        }
        public static bool Contains(CircleCollider circle, Collider point)
        {
            return Contains(circle.Pos, circle.Radius, point.Pos);
        }
        public static bool Contains(CircleCollider circle, Vector2 point)
        {
            return Contains(circle.Pos, circle.Radius, point);
        }
        public static bool Contains(Vector2 aPos, float aR, Vector2 bPos, float bR)
        {
            if (aR <= bR) return false;
            Vector2 dif = bPos - aPos;

            return dif.LengthSquared() + bR < aR;
        }
        public static bool Contains(CircleCollider self, CircleCollider other)
        {
            return Contains(self.Pos, self.Radius, other.Pos, other.Radius);
        }
        public static bool Contains(CircleCollider circle, Vector2 pos, float radius)
        {
            return Contains(circle.Pos, circle.Radius, pos, radius);
        }
        public static bool Contains(CircleCollider circle, RectCollider Rect)
        {
            if (!Contains(circle, Rect.Pos)) return false;
            if (!Contains(circle, Rect.GetBottomRight())) return false;
            return true;
        }
        public static bool Contains(CircleCollider circle, SegmentCollider segment)
        {
            if (!Contains(circle, segment.Pos)) return false;
            if (!Contains(circle, segment.GetEnd())) return false;
            return true;
        }

        //Rect - Rect/Point/Circle/Line
        public static bool Contains(RectCollider a, RectCollider b)
        {
            return a.Pos.X < b.Pos.X && a.Pos.Y < b.Pos.Y && a.GetBottomRight().X > b.GetBottomRight().X && a.GetBottomRight().Y > b.GetBottomRight().Y;
        }
        public static bool Contains(Rectangle a, Rectangle b)
        {
            return a.X < b.X && a.Y < b.Y && a.X + a.width > b.X + b.width && a.Y + a.height > b.Y + b.height;
        }
        public static bool Contains(Rectangle a, RectCollider b)
        {
            return a.X < b.Pos.X && a.Y < b.Pos.Y && a.X + a.width > b.GetBottomRight().X && a.Y + a.height > b.GetBottomRight().Y;
        }
        public static bool Contains(RectCollider rect, Collider point)
        {
            return rect.Pos.X <= point.Pos.X && rect.Pos.Y <= point.Pos.Y && rect.GetBottomRight().X >= point.Pos.X && rect.GetBottomRight().Y >= point.Pos.Y;
        }
        public static bool Contains(RectCollider rect, Vector2 point)
        {
            return rect.Pos.X <= point.X && rect.Pos.Y <= point.Y && rect.GetBottomRight().X >= point.X && rect.GetBottomRight().Y >= point.Y;
        }
        public static bool Contains(Vector2 topLeft, Vector2 size, Vector2 point)
        {
            return topLeft.X <= point.X && topLeft.Y <= point.Y && topLeft.X + size.X >= point.X && topLeft.Y + size.Y >= point.Y;
        }
        public static bool Contains(Rectangle rect, Vector2 point)
        {
            return rect.X <= point.X && rect.Y <= point.Y && rect.X + rect.width >= point.X && rect.Y + rect.height >= point.Y;
        }
        public static bool Contains(Rectangle rect, Collider point)
        {
            return Contains(rect, point.Pos);
        }
        public static bool Contains(RectCollider rect, CircleCollider circle)
        {
            return
                rect.Pos.X <= circle.Pos.X - circle.Radius &&
                rect.Pos.Y <= circle.Pos.Y - circle.Radius &&
                rect.GetBottomRight().X >= circle.Pos.X + circle.Radius &&
                rect.GetBottomRight().Y >= circle.Pos.Y + circle.Radius;
        }
        public static bool Contains(Rectangle rect, CircleCollider circle)
        {
            return
                rect.X <= circle.Pos.X - circle.Radius &&
                rect.Y <= circle.Pos.Y - circle.Radius &&
                rect.X + rect.width >= circle.Pos.X + circle.Radius &&
                rect.Y + rect.height >= circle.Pos.Y + circle.Radius;
        }
        public static bool Contains(RectCollider rect, Vector2 circlePos, float circleRadius)
        {
            return
                rect.Pos.X <= circlePos.X - circleRadius &&
                rect.Pos.Y <= circlePos.Y - circleRadius &&
                rect.GetBottomRight().X >= circlePos.X + circleRadius &&
                rect.GetBottomRight().Y >= circlePos.Y + circleRadius;
        }
        public static bool Contains(Rectangle rect, Vector2 circlePos, float circleRadius)
        {
            return
                rect.X <= circlePos.X - circleRadius &&
                rect.Y <= circlePos.Y - circleRadius &&
                rect.X + rect.width >= circlePos.X + circleRadius &&
                rect.Y + rect.height >= circlePos.Y + circleRadius;
        }
        public static bool Contains(RectCollider rect, SegmentCollider segment)
        {
            if (!Contains(rect, segment.Pos)) return false;
            if (!Contains(rect, segment.GetEnd())) return false;
            return true;
        }
        public static bool Contains(Rectangle rect, SegmentCollider segment)
        {
            if (!Contains(rect, segment.Pos)) return false;
            if (!Contains(rect, segment.GetEnd())) return false;
            return true;
        }
        public static bool Contains(Rectangle rect, Vector2 start, Vector2 end)
        {
            if (!Contains(rect, start)) return false;
            if (!Contains(rect, end)) return false;
            return true;
        }

        //CLOSEST POINT
        //circle - circle
        public static Vector2 ClosestPointCircleCircle(CircleCollider self, CircleCollider other)
        {
            return Vec.Normalize(other.Pos - self.Pos) * self.Radius;
        }
        public static Vector2 ClosestPointCircleCircle(CircleCollider self, Vector2 otherPos)
        {
            return Vec.Normalize(otherPos - self.Pos) * self.Radius;
        }
        public static Vector2 ClosestPointCircleCircle(Vector2 pos, float r, CircleCollider other)
        {
            return Vec.Normalize(other.Pos - pos) * r;
        }
        public static Vector2 ClosestPointCircleCircle(Vector2 selfPos, float selfR, Vector2 otherPos)
        {
            return Vec.Normalize(otherPos - selfPos) * selfR;
        }

        //point - line
        public static Vector2 ClosestPointLineCircle(Vector2 linePos, Vector2 lineDir, Vector2 circlePos, float circleRadius)
        {
            Vector2 w = circlePos - linePos;
            float p = w.X * lineDir.Y - w.Y * lineDir.X;
            if (p < -circleRadius || p > circleRadius)
            {
                float t = lineDir.X * w.X + lineDir.Y * w.Y;
                return linePos + lineDir * t;
            }
            else
            {
                float qb = w.X * lineDir.X + w.Y * lineDir.Y;
                float qc = w.LengthSquared() - circleRadius * circleRadius;
                float qd = qb * qb - qc;
                float t = qb - MathF.Sqrt(qd);
                return linePos + lineDir * t;
            }
        }
        public static Vector2 ClosestPointLineCircle(Vector2 linePos, Vector2 lineDir, CircleCollider circle)
        {
            return ClosestPointLineCircle(linePos, lineDir, circle.Pos, circle.Radius);
        }
        public static Vector2 ClosestPointPointLine(Vector2 point, Vector2 linePos, Vector2 lineDir)
        {
            Vector2 displacement = point - linePos;
            float t = lineDir.X * displacement.X + lineDir.Y * displacement.Y;
            return Vec.ProjectionPoint(linePos, lineDir, t);
        }
        public static Vector2 ClosestPointPointLine(Collider point, Vector2 linePos, Vector2 lineDir)
        {
            return ClosestPointPointLine(point.Pos, linePos, lineDir);
        }

        //point - ray
        public static Vector2 ClosestPointRayCircle(Vector2 rayPos, Vector2 rayDir, CircleCollider circle)
        {
            return ClosestPointRayCircle(rayPos, rayDir, circle.Pos, circle.Radius);
        }
        public static Vector2 ClosestPointRayCircle(Vector2 rayPos, Vector2 rayDir, Vector2 circlePos, float circleRadius)
        {
            Vector2 w = circlePos - rayPos;
            float p = w.X * rayDir.Y - w.Y * rayDir.X;
            float t1 = w.X * rayDir.X + w.Y * rayDir.Y;
            if (p < -circleRadius || p > circleRadius)
            {
                if (t1 < 0.0f) return rayPos;
                else return rayPos + rayDir * t1;
            }
            else
            {
                if (t1 < -circleRadius) return rayPos;
                else if (t1 < circleRadius)
                {
                    float qb = w.X * rayDir.X + w.Y * rayDir.Y;
                    float qc = w.LengthSquared() - circleRadius * circleRadius;
                    float qd = qb * qb - qc;
                    float t2 = qb - MathF.Sqrt(qd);
                    return rayPos + rayDir * t2;
                }
                else return rayPos + rayDir * t1;
            }
        }
        public static Vector2 ClosestPointPointRay(Vector2 point, Vector2 rayPos, Vector2 rayDir)
        {
            Vector2 displacement = point - rayPos;
            float t = rayDir.X * displacement.X + rayDir.Y * displacement.Y;
            return t < 0 ? rayPos : Vec.ProjectionPoint(rayPos, rayDir, t);
        }
        public static Vector2 ClosestPointPointRay(Collider point, Vector2 rayPos, Vector2 rayDir)
        {
            return ClosestPointPointRay(point.Pos, rayPos, rayDir);
        }

        //point - circle
        public static Vector2 ClosestPoint(Vector2 circlaAPos, float circlaARadius, Vector2 circleBPos, float circleBRadius)
        {
            return ClosestPoint(circlaAPos, circleBPos, circleBRadius);
        }
        public static Vector2 ClosestPoint(Vector2 point, Vector2 circlePos, float circleRadius)
        {
            Vector2 w = point - circlePos;
            float t = circleRadius / w.Length();
            return circlePos + w * t;
        }
        public static Vector2 ClosestPoint(CircleCollider a, Vector2 circlePos, float circleRadius)
        {
            return ClosestPoint(a.Pos, circlePos, circleRadius);
        }
        public static Vector2 ClosestPoint(CircleCollider a, CircleCollider b)
        {
            return ClosestPoint(a.Pos, b.Pos, b.Radius);
        }
        public static Vector2 ClosestPoint(Collider point, CircleCollider circle)
        {
            return ClosestPoint(point.Pos, circle.Pos, circle.Radius);
        }
        public static Vector2 ClosestPoint(Vector2 point, CircleCollider circle)
        {
            return ClosestPoint(point, circle.Pos, circle.Radius);
        }

        //point - segment
        public static Vector2 ClosestPoint(Vector2 point, Vector2 segmentPos, Vector2 segmentDir, float segmentLength)
        {
            Vector2 displacment = point - segmentPos;
            float t = Vec.ProjectionTime(displacment, segmentDir);
            if (t < 0.0f) return segmentPos;
            else if (t > 1.0f) return segmentPos + segmentDir * segmentLength;
            else return Vec.ProjectionPoint(segmentPos, segmentDir, t);
        }
        public static Vector2 ClosestPoint(Vector2 point, SegmentCollider segment)
        {
            return ClosestPoint(point, segment.Pos, segment.Dir, segment.Length);
        }
        public static Vector2 ClosestPoint(Collider point, Vector2 segmentPos, Vector2 segmentDir, float segmentLength)
        {
            return ClosestPoint(point.Pos, segmentPos, segmentDir, segmentLength);
        }
        public static Vector2 ClosestPoint(Collider point, SegmentCollider segment)
        {
            return ClosestPoint(point.Pos, segment.Pos, segment.Dir, segment.Length);
        }

        //segment - circle
        public static Vector2 ClosestPoint(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 circlePos, float circleRadius)
        {
            Vector2 sv = segmentDir * segmentLength;
            Vector2 w = circlePos - segmentPos;
            float p = w.X * segmentDir.Y - w.Y * segmentDir.X;
            float qa = sv.LengthSquared();
            float t1 = (w.X * sv.X + w.Y * sv.Y) / qa;
            if (p < -circleRadius || p > circleRadius)
            {
                if (t1 < 0.0f) return segmentPos;
                else if (t1 > 1.0f) return segmentPos + sv;
                else return segmentPos + sv * t1;
            }
            else
            {
                float qb = w.X * sv.X + w.Y * sv.Y;
                float qc = w.LengthSquared() - circleRadius * circleRadius;
                float qd = qb * qb - qc * qa;
                float t2 = (qb + MathF.Sqrt(qd)) / qa;
                if (t2 < 0.0f) return segmentPos;
                else if (t2 < 1.0f) return segmentPos + sv * t2;
                else
                {
                    float t3 = (qb - MathF.Sqrt(qd)) / qa;
                    if (t3 < 1.0f) return segmentPos + sv * t3;
                    else return segmentPos + sv;
                }
            }
        }
        public static Vector2 ClosestPoint(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, CircleCollider circle)
        {
            return ClosestPoint(segmentPos, segmentDir, segmentLength, circle.Pos, circle.Radius);
        }
        public static Vector2 ClosestPoint(SegmentCollider segment, Vector2 circlePos, float circleRadius)
        {
            return ClosestPoint(segment.Pos, segment.Dir, segment.Length, circlePos, circleRadius);
        }
        public static Vector2 ClosestPoint(SegmentCollider segment, CircleCollider circle)
        {
            return ClosestPoint(segment.Pos, segment.Dir, segment.Length, circle.Pos, circle.Radius);
        }



        //INTERSECTION
        public static IntersectionInfo PointIntersectCircle(Vector2 point, Vector2 vel, Vector2 circlePos, float radius)
        {
            Vector2 w = circlePos - point;
            float qa = vel.LengthSquared();
            float qb = vel.X * w.X + vel.Y * w.Y;
            float qc = w.LengthSquared() - radius * radius;

            float qd = qb * qb - qa * qc;
            if (qd < 0.0f) return new IntersectionInfo { intersected = false };
            float t = (qb - MathF.Sqrt(qd)) / qa;
            if (t < 0.0f || t > 1.0f) return new IntersectionInfo { intersected = false };

            Vector2 intersectionPoint = point + vel * t; // new(point.X + vel.X * t, point.Y + vel.Y * t);
            return new IntersectionInfo
            {
                intersected = true,
                point = intersectionPoint,
                time = t,
                remaining = 1.0f - t
            };
        }


        //CAST (SemiDynamic - Get Collision Response only for first object - second object can have vel as well)
        public static CastInfo Cast(Collider point, CircleCollider circle, float dt)
        {
            bool overlapping = Overlap.Simple(point, circle);// Contains(circle.Pos, circle.Radius, point.Pos);
            Vector2 vel = point.Vel - circle.Vel; //-> simple way of making sure second object is static and first is dynamic
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            Vector2 w = circle.Pos - point.Pos;
            float qa = vel.LengthSquared();
            float qb = vel.X * w.X + vel.Y * w.Y;
            float qc = w.LengthSquared() - circle.RadiusSquared;
            float qd = qb * qb - qa * qc;

            if (qd < 0.0f) return new();
            float t = (qb - MathF.Sqrt(qd)) / qa;
            if (t < 0.0f || t > 1.0f) return new();

            Vector2 intersectPoint = point.Pos + vel * t;
            Vector2 collisionPoint = intersectPoint;
            Vector2 normal = (intersectPoint - circle.Pos) / circle.Radius;
            float remaining = 1.0f - t;
            float d = 2.0f * (vel.X * normal.X + vel.Y * normal.Y);
            Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
            //Vector2 reflectPoint = intersectPoint + reflectVector;

            return new(false, true, t, intersectPoint, collisionPoint, reflectVector, normal, point.Vel, circle.Vel);
        }
        public static CastInfo Cast(CircleCollider circle, Collider point, float dt)
        {
            bool overlapping = Overlap.Simple(circle, point);// Contains(circle.Pos, circle.Radius, point.Pos);
            Vector2 vel = circle.Vel - point.Vel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            Vector2 w = point.Pos - circle.Pos;

            float qa = vel.LengthSquared();
            float qb = vel.X * w.X + vel.Y * w.Y;
            float qc = w.LengthSquared() - circle.RadiusSquared;
            float qd = qb * qb - qa * qc;
            if (qd < 0) return new();
            float t = (qb - MathF.Sqrt(qd)) / qa;
            if (t < 0 || t > 1) return new();

            Vector2 intersectionPoint = circle.Pos + vel * t;
            Vector2 collisionPoint = point.Pos;
            Vector2 normal = (intersectionPoint - point.Pos) / circle.Radius;
            float remaining = 1.0f - t;
            float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
            Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
            //Vector2 reflectPoint = intersectionPoint + reflectVector;
            return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, point.Vel);
        }
        public static CastInfo Cast(CircleCollider self, CircleCollider other, float dt)
        {
            bool overlapping = Overlap.Simple(self, other);
            Vector2 vel = self.Vel - other.Vel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            float r = self.Radius + other.Radius;
            IntersectionInfo intersectionInfo = PointIntersectCircle(self.Pos, vel, other.Pos, r);
            if (!intersectionInfo.intersected) return new();

            Vector2 normal = (intersectionInfo.point - other.Pos) / r;
            Vector2 collisionPoint = other.Pos + normal * other.Radius;
            //Vector2 reflectVector = Utils.ElasticCollision2D(self.Pos, self.Vel, self.Mass, other.Pos, other.Vel, other.Mass, 1f);
            if (Vec.Dot(vel, normal) > 0f) vel *= -1;
            float dot = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
            Vector2 reflectVector = new(intersectionInfo.remaining * (vel.X - dot * normal.X), intersectionInfo.remaining * (vel.Y - dot * normal.Y));
            //Vector2 reflectPoint = intersectionInfo.point + reflectVector;
            return new(false, true, intersectionInfo.time, intersectionInfo.point, collisionPoint, reflectVector, normal, self.Vel, other.Vel);
        }
        public static CastInfo Cast(Collider self, Collider other, float dt)
        {
            //REAL Point - Point collision basically never happens.... so this is the point - circle cast code!!!
            CircleCollider circle = new(other.Pos, other.Vel, Overlap.POINT_OVERLAP_EPSILON);

            bool overlapping = Overlap.Simple(self, circle);// Contains(circle.Pos, circle.Radius, point.Pos);
            Vector2 vel = self.Vel - circle.Vel; //-> simple way of making sure second object is static and first is dynamic
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            Vector2 w = circle.Pos - self.Pos;
            float qa = vel.LengthSquared();
            float qb = vel.X * w.X + vel.Y * w.Y;
            float qc = w.LengthSquared() - circle.RadiusSquared;
            float qd = qb * qb - qa * qc;

            if (qd < 0.0f) return new();
            float t = (qb - MathF.Sqrt(qd)) / qa;
            if (t < 0.0f || t > 1.0f) return new();

            Vector2 intersectPoint = self.Pos + vel * t;
            Vector2 collisionPoint = intersectPoint;
            Vector2 normal = (intersectPoint - circle.Pos) / circle.Radius;
            float remaining = 1.0f - t;
            float d = 2.0f * (vel.X * normal.X + vel.Y * normal.Y);
            Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
            //Vector2 reflectPoint = intersectPoint + reflectVector;
            return new(false, true, t, intersectPoint, collisionPoint, reflectVector, normal, self.Vel, circle.Vel);
            /*
            bool overlapping = Overlap.Simple(self, other);
            Vector2 vel = self.Vel - other.Vel;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new CollisionInfo() { collided = false, overlapping = overlapping };

            Vector2 w = Vec.Floor(other.Pos) - Vec.Floor(self.Pos); //displacement
            float p = w.X * vel.Y - w.Y * vel.X; //perpendicular product
            if(p != 0.0f) return new CollisionInfo { overlapping = false, collided = false };
            float t = (w.X * vel.X + w.Y * vel.Y) / vel.LengthSquared();
            if(t < 0.0f || t > 1.0f) return new CollisionInfo() { overlapping = false, collided=false };

            Vector2 intersectionPoint = other.Pos;
            Vector2 collisionPoint = intersectionPoint;
            float len = vel.Length();
            Vector2 normal = -vel / len;
            float remaining = 1.0f - t;
            float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
            Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (self.Vel.Y - normal.Y * d));
            Vector2 reflectPoint = intersectionPoint + reflectVector;
            
            return new CollisionInfo 
            { 
                overlapping = false, 
                collided = true,
                self = new CollisionResponse { shape = self, available = true, normal = normal, intersectPoint = intersectionPoint, reflectVector = reflectVector, reflectPoint = reflectPoint },
                other = new CollisionResponse { shape = other, available = false},
                intersectPoint = intersectionPoint,
                collisionPoint = collisionPoint,
                time = t,
                remaining = remaining
            };
            */
        }
        public static CastInfo Cast(Collider point, SegmentCollider segment, float dt)
        {
            //bool overlapping = Overlap.Simple(point, segment);
            Vector2 vel = point.Vel - segment.Vel;
            vel *= dt;
            if (vel.LengthSquared() <= 0.0f) return new();
            Vector2 sv = segment.Dir * segment.Length;
            Vector2 w = segment.Pos - point.Pos;
            float projectionTime = -(w.X * sv.X + w.Y * sv.Y) / sv.LengthSquared();
            if (projectionTime < 0.0f)//behind
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f)//parallel
                {
                    float c = w.X * segment.Dir.Y - w.Y * segment.Dir.X;
                    if (c != 0.0f) return new();
                    float t;
                    if (vel.X == 0.0f) t = w.Y / vel.Y;
                    else t = w.X / vel.X;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segment.Pos;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segment.Dir * -1.0f;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                }
                else //not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return new();
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();
                    if (ts == 0.0f)
                    {
                        Vector2 intersectionPoint = segment.Pos;
                        Vector2 collisionPoint = intersectionPoint;
                        Vector2 normal = segment.Dir * -1.0f;
                        float remaining = 1.0f - t;
                        float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                        Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                        //Vector2 reflectPoint = intersectionPoint + reflectVector;
                        return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                    }
                    else
                    {
                        Vector2 intersectionPoint = point.Pos + vel * t;
                        Vector2 collisionPoint = intersectionPoint;
                        Vector2 normal;
                        if (p < 0) normal = new(-segment.Dir.Y, segment.Dir.X);
                        else normal = new(segment.Dir.Y, -segment.Dir.X);
                        float remaining = 1.0f - t;
                        float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                        Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                        //Vector2 reflectPoint = intersectionPoint + reflectVector;
                        return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                    }
                }
            }
            else if (projectionTime > 1.0f)//ahead
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) //parallel
                {
                    float c = w.X * segment.Dir.Y - w.Y * segment.Dir.X;
                    if (c != 0.0f) return new();
                    float t = vel.X == 0.0f ? w.Y / vel.Y - 1.0f : w.X / vel.X - 1.0f;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segment.GetEnd();
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segment.Dir;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                }
                else // not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return new();
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segment.GetEnd();
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segment.Dir;

                    if (ts != 1.0f)
                    {
                        intersectionPoint = point.Pos + vel * t;
                        collisionPoint = intersectionPoint;
                        normal = p < 0.0f ? new(-segment.Dir.Y, segment.Dir.X) : new(segment.Dir.Y, -segment.Dir.X);
                    }

                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                }
            }
            else//on
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) return new();
                float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                if (ts < 0.0f || ts > 1.0f) return new();
                float t = (sv.X * w.Y - sv.Y * w.X) / p;
                if (t < 0.0f || t > 1.0f) return new();

                Vector2 intersectionPoint = point.Pos + vel * t;
                Vector2 collisionPoint = intersectionPoint;
                Vector2 normal = p < 0.0f ? new(-segment.Dir.Y, segment.Dir.X) : new(segment.Dir.Y, -segment.Dir.X);

                float remaining = 1.0f - t;
                float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                //Vector2 reflectPoint = intersectionPoint + reflectVector;
                return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
            }
        }
        public static CastInfo Cast(CircleCollider circle, SegmentCollider segment, float dt)
        {
            bool overlapping = Overlap.Simple(circle, segment);
            Vector2 vel = circle.Vel - segment.Vel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);
            Vector2 sv = segment.Dir * segment.Length;
            float p = sv.X * vel.Y - sv.Y * vel.X;
            if (p < 0.0f)
            {

                Vector2 point = new(segment.Pos.X - segment.Dir.Y * circle.Radius, segment.Pos.Y + segment.Dir.X * circle.Radius);// segment.Pos - segment.Dir * circle.Radius;
                Vector2 w1 = point - circle.Pos;
                float ts = (vel.X * w1.Y - vel.Y * w1.X) / p;
                if (ts < 0.0f)
                {
                    Vector2 w2 = segment.Pos - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = segment.Pos;
                    Vector2 normal = (intersectionPoint - segment.Pos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
                else if (ts > 1.0f)
                {
                    Vector2 end = segment.Pos + sv;
                    Vector2 w2 = end - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = end;
                    Vector2 normal = (intersectionPoint - end) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
                else
                {
                    float t = (sv.X * w1.Y - sv.Y * w1.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = new(intersectionPoint.X + segment.Dir.Y * circle.Radius, intersectionPoint.Y - segment.Dir.X * circle.Radius);
                    Vector2 normal = new(-segment.Dir.Y, segment.Dir.X);
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
            }
            else if (p > 0.0f)
            {
                Vector2 p1 = new(segment.Pos.X + segment.Dir.Y * circle.Radius, segment.Pos.Y - segment.Dir.X * circle.Radius);// segment.Pos + segment.Dir * circle.Radius;
                Vector2 w1 = p1 - circle.Pos;
                float ts = (vel.X * w1.Y - vel.Y * w1.X) / p;
                if (ts < 0.0f)
                {
                    Vector2 w2 = segment.Pos - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = segment.Pos;
                    Vector2 normal = (intersectionPoint - segment.Pos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
                else if (ts > 1.0f)
                {
                    Vector2 end = segment.Pos + sv;// segment.End;
                    Vector2 w2 = end - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = end;
                    Vector2 normal = (intersectionPoint - end) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
                else
                {
                    float t = (sv.X * w1.Y - sv.Y * w1.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = new(intersectionPoint.X - segment.Dir.Y * circle.Radius, intersectionPoint.Y + segment.Dir.X * circle.Radius); // intersectionPoint - segment.Dir * circle.Radius;
                    Vector2 normal = new(segment.Dir.Y, -segment.Dir.X);
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
            }
            else
            {
                return new(true);
            }
        }


        //--- --- --- --- --- --- NOT IMPLEMENTED --- --- --- --- --- ---
        public static CastInfo Cast(Collider point, RectCollider rect, float dt)
        {
            return new();
        }
        public static CastInfo Cast(RectCollider rect, Collider point, float dt)
        {
            return new();
        }
        public static CastInfo Cast(Collider point, OrientedRectCollider oRect, float dt)
        {
            return new();
        }
        public static CastInfo Cast(OrientedRectCollider oRect, Collider point, float dt)
        {
            return new();
        }
        public static CastInfo Cast(CircleCollider circle, RectCollider rect, float dt)
        {
            return new();
        }
        public static CastInfo Cast(RectCollider rect, CircleCollider circle, float dt)
        {
            return new();
        }
        public static CastInfo Cast(CircleCollider circle, OrientedRectCollider oRect, float dt)
        {
            return new();
        }
        public static CastInfo Cast(OrientedRectCollider oRect, CircleCollider circle, float dt)
        {
            return new();
        }
        public static CastInfo Cast(RectCollider self, RectCollider other, float dt)
        {
            return new();
        }
        public static CastInfo Cast(OrientedRectCollider self, OrientedRectCollider other, float dt)
        {
            return new();
        }
        public static CastInfo Cast(RectCollider self, SegmentCollider segment, float dt)
        {
            return new();
        }
        public static CastInfo Cast(OrientedRectCollider self, SegmentCollider segment, float dt)
        {
            return new();
        }
        //--- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---

        
        public static List<Vector2> IntersectLineRect(Vector2 start, Vector2 end, Rectangle rect)
        {
            Vector2 tl = new(rect.x, rect.y);
            Vector2 tr = new(rect.x + rect.width, rect.y);
            Vector2 bl = new(rect.x, rect.y + rect.height);
            Vector2 br = new(rect.x + rect.width, rect.y + rect.height);
            return IntersectLineRect(start, end, tl, tr, br, bl);
        }
        public static List<Vector2> IntersectLineRect(Vector2 start, Vector2 end, Vector2 rectPos, Vector2 rectSize, Alignement rectAlignement)
        {
            var rect = Utils.ConstructRectangle(rectPos, rectSize, rectAlignement);
            return IntersectLineRect(start, end, rect);
            
        }
        public static List<Vector2> IntersectLineRect(Vector2 start, Vector2 end, Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl)
        {
            List<(Vector2 start, Vector2 end)> segments = new()
            {
                (tl, tr), (bl, br), (tl, bl), (tr, br)
            };

            List<Vector2> intersections = new();
            foreach (var seg in segments)
            {
                var result = IntersectLineSegment(start, end, seg.start, seg.end);
                if (result.intersection)
                {
                    intersections.Add(result.intersectPoint);
                }
                if (intersections.Count >= 2) return intersections;
            }
            return intersections;
        }
        


        public static (bool intersection, Vector2 intersectPoint) IntersectLineSegment(Vector2 start, Vector2 end, Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 segmentVel)
        {
            Vector2 pointPos = start;
            Vector2 pointVel = end - start;
            Vector2 vel = pointVel - segmentVel;
            if (vel.LengthSquared() <= 0.0f) return (false, new(0f));
            Vector2 sv = segmentDir * segmentLength;
            Vector2 w = segmentPos - pointPos;
            float projectionTime = -(w.X * sv.X + w.Y * sv.Y) / sv.LengthSquared();
            if (projectionTime < 0.0f)//behind
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f)//parallel
                {
                    float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
                    if (c != 0.0f) return (false, new(0f));
                    float t;
                    if (vel.X == 0.0f) t = w.Y / vel.Y;
                    else t = w.X / vel.X;
                    if (t < 0.0f || t > 1.0f) return (false, new(0f));

                    Vector2 intersectionPoint = segmentPos;
                    return (true, intersectionPoint);
                }
                else //not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return (false, new(0f));
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return (false, new(0f));
                    if (ts == 0.0f)
                    {
                        Vector2 intersectionPoint = segmentPos;
                        return (true, intersectionPoint);
                    }
                    else
                    {
                        Vector2 intersectionPoint = pointPos + vel * t;
                        return (true, intersectionPoint);
                    }
                }
            }
            else if (projectionTime > 1.0f)//ahead
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) //parallel
                {
                    float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
                    if (c != 0.0f) return (false, new(0f));
                    float t = vel.X == 0.0f ? w.Y / vel.Y - 1.0f : w.X / vel.X - 1.0f;
                    if (t < 0.0f || t > 1.0f) return (false, new(0f));

                    Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength;
                    return (true, intersectionPoint);
                }
                else // not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return (false, new(0f));
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return (false, new(0f));

                    Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength;

                    if (ts != 1.0f)
                    {
                        intersectionPoint = pointPos + vel * t;
                    }
                    return (true, intersectionPoint);
                }
            }
            else//on
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) return(false, new(0f));
                float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                if (ts < 0.0f || ts > 1.0f) return (false, new(0f));
                float t = (sv.X * w.Y - sv.Y * w.X) / p;
                if (t < 0.0f || t > 1.0f) return (false, new(0f));

                Vector2 intersectionPoint = pointPos + vel * t;
                return (true, intersectionPoint);
            }
        }
        public static (bool intersection, Vector2 intersectPoint) IntersectLineSegment(Vector2 start, Vector2 end, Vector2 segmentStart, Vector2 segmentEnd)
        {
            Vector2 segmentPos = segmentStart;
            Vector2 segmentDir = segmentEnd - segmentStart;
            float segmentLength = segmentDir.Length();
            segmentDir /= segmentLength;
            Vector2 segmentVel = new(0f);
            Vector2 pointPos = start;
            Vector2 pointVel = end - start;
            Vector2 vel = pointVel - segmentVel;
            if (vel.LengthSquared() <= 0.0f) return (false, new(0f));
            Vector2 sv = segmentDir * segmentLength;
            Vector2 w = segmentPos - pointPos;
            float projectionTime = -(w.X * sv.X + w.Y * sv.Y) / sv.LengthSquared();
            if (projectionTime < 0.0f)//behind
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f)//parallel
                {
                    float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
                    if (c != 0.0f) return (false, new(0f));
                    float t;
                    if (vel.X == 0.0f) t = w.Y / vel.Y;
                    else t = w.X / vel.X;
                    if (t < 0.0f || t > 1.0f) return (false, new(0f));

                    Vector2 intersectionPoint = segmentPos;
                    return (true, intersectionPoint);
                }
                else //not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return (false, new(0f));
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return (false, new(0f));
                    if (ts == 0.0f)
                    {
                        Vector2 intersectionPoint = segmentPos;
                        return (true, intersectionPoint);
                    }
                    else
                    {
                        Vector2 intersectionPoint = pointPos + vel * t;
                        return (true, intersectionPoint);
                    }
                }
            }
            else if (projectionTime > 1.0f)//ahead
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) //parallel
                {
                    float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
                    if (c != 0.0f) return (false, new(0f));
                    float t = vel.X == 0.0f ? w.Y / vel.Y - 1.0f : w.X / vel.X - 1.0f;
                    if (t < 0.0f || t > 1.0f) return (false, new(0f));

                    Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength;
                    return (true, intersectionPoint);
                }
                else // not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return (false, new(0f));
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return (false, new(0f));

                    Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength;

                    if (ts != 1.0f)
                    {
                        intersectionPoint = pointPos + vel * t;
                    }
                    return (true, intersectionPoint);
                }
            }
            else//on
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) return (false, new(0f));
                float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                if (ts < 0.0f || ts > 1.0f) return (false, new(0f));
                float t = (sv.X * w.Y - sv.Y * w.X) / p;
                if (t < 0.0f || t > 1.0f) return (false, new(0f));

                Vector2 intersectionPoint = pointPos + vel * t;
                return (true, intersectionPoint);
            }
        }

        
        
        public static CastInfo Cast(Vector2 pointPos, Vector2 pointVel, Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 segmentVel, float dt)
        {
            //bool overlapping = Overlap.Simple(point, segment);
            Vector2 vel = pointVel - segmentVel;
            vel *= dt;
            if (vel.LengthSquared() <= 0.0f) return new();
            Vector2 sv = segmentDir * segmentLength;
            Vector2 w = segmentPos - pointPos;
            float projectionTime = -(w.X * sv.X + w.Y * sv.Y) / sv.LengthSquared();
            if (projectionTime < 0.0f)//behind
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f)//parallel
                {
                    float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
                    if (c != 0.0f) return new();
                    float t;
                    if (vel.X == 0.0f) t = w.Y / vel.Y;
                    else t = w.X / vel.X;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segmentPos;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segmentDir * -1.0f;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                }
                else //not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return new();
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();
                    if (ts == 0.0f)
                    {
                        Vector2 intersectionPoint = segmentPos;
                        Vector2 collisionPoint = intersectionPoint;
                        Vector2 normal = segmentDir * -1.0f;
                        float remaining = 1.0f - t;
                        float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                        Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                        //Vector2 reflectPoint = intersectionPoint + reflectVector;
                        return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                    }
                    else
                    {
                        Vector2 intersectionPoint = pointPos + vel * t;
                        Vector2 collisionPoint = intersectionPoint;
                        Vector2 normal;
                        if (p < 0) normal = new(-segmentDir.Y, segmentDir.X);
                        else normal = new(segmentDir.Y, -segmentDir.X);
                        float remaining = 1.0f - t;
                        float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                        Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                        //Vector2 reflectPoint = intersectionPoint + reflectVector;
                        return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                    }
                }
            }
            else if (projectionTime > 1.0f)//ahead
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) //parallel
                {
                    float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
                    if (c != 0.0f) return new();
                    float t = vel.X == 0.0f ? w.Y / vel.Y - 1.0f : w.X / vel.X - 1.0f;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segmentDir;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                }
                else // not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return new();
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength; ;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segmentDir;

                    if (ts != 1.0f)
                    {
                        intersectionPoint = pointPos + vel * t;
                        collisionPoint = intersectionPoint;
                        normal = p < 0.0f ? new(-segmentDir.Y, segmentDir.X) : new(segmentDir.Y, -segmentDir.X);
                    }

                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                }
            }
            else//on
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) return new();
                float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                if (ts < 0.0f || ts > 1.0f) return new();
                float t = (sv.X * w.Y - sv.Y * w.X) / p;
                if (t < 0.0f || t > 1.0f) return new();

                Vector2 intersectionPoint = pointPos + vel * t;
                Vector2 collisionPoint = intersectionPoint;
                Vector2 normal = p < 0.0f ? new(-segmentDir.Y, segmentDir.X) : new(segmentDir.Y, -segmentDir.X);

                float remaining = 1.0f - t;
                float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                //Vector2 reflectPoint = intersectionPoint + reflectVector;
                return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
            }
        }
        public static CastInfo CastPointLine(Collider point, Vector2 linePos, Vector2 lineDir, Vector2 lineVel, float dt)
        {
            //bool overlapping = Overlap.Simple(point, line);
            Vector2 vel = point.Vel - lineVel;
            vel *= dt;
            if (vel.LengthSquared() <= 0.0f) return new();

            Vector2 w = linePos - point.Pos;
            float p = lineDir.X * point.Vel.Y - lineDir.Y * point.Vel.X;
            if (p == 0.0f) return new();
            float t = (lineDir.X * w.Y - lineDir.Y * w.X) / p;
            if (t < 0.0f || t > 1.0f) return new();

            Vector2 intersectionPoint = point.Pos + point.Vel * t;
            Vector2 collisionPoint = intersectionPoint;
            Vector2 n = p < 0.0f ? new(-lineDir.Y, lineDir.X) : new(lineDir.Y, -lineDir.X);
            float remaining = 1.0f - t;
            float d = 2.0f * (point.Vel.X * n.X + point.Vel.Y * n.Y);
            Vector2 reflectVector = new(remaining * (point.Vel.X - n.X * d), remaining * (point.Vel.Y - n.Y * d));
            //Vector2 reflectPoint = intersectionPoint + reflectVector;
            return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, n, point.Vel, lineVel);
        }
        public static CastInfo CastPointRay(Collider point, Vector2 rayPos, Vector2 rayDir, Vector2 rayVel, float dt)
        {
            //bool overlapping = Overlap.Simple(point, ray);
            Vector2 vel = point.Vel - rayVel;
            vel *= dt;
            if (vel.LengthSquared() <= 0.0f) return new();

            Vector2 w = rayPos - point.Pos;
            float p = rayDir.X * vel.Y - rayDir.Y * vel.X;
            if (p == 0.0f)
            {
                float c = w.X * rayDir.Y - w.Y * rayDir.X;
                if (c != 0.0f) return new();

                float t;
                if (vel.X == 0.0f) t = w.Y / vel.Y;
                else t = w.X / vel.X;

                if (t < 0.0f || t > 1.0f) return new();

                Vector2 intersectionPoint = rayPos;
                Vector2 collisionPoint = intersectionPoint;
                Vector2 normal = rayDir * -1;
                float remaining = 1.0f - t;
                float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                //Vector2 reflectPoint = intersectionPoint + reflectVector;
                return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, rayVel);
            }
            else
            {
                float t = (rayDir.X * w.Y - rayDir.Y * w.X) / p;
                if (t < 0.0f || t > 1.0f) return new();
                float tr = (vel.X * w.Y - vel.Y * w.X) / p;
                if (tr < 0.0f) return new();

                Vector2 intersectionPoint = point.Pos + vel * t;
                Vector2 collisionPoint = intersectionPoint;
                Vector2 normal;
                if (p < 0) normal = new(-rayDir.Y, rayDir.X);
                else normal = new(rayDir.Y, -rayDir.X);
                float remaining = 1.0f - t;
                float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                //Vector2 reflectPoint = intersectionPoint + reflectVector;
                return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, rayVel);
            }
        }
        public static CastInfo CastCircleLine(CircleCollider circle, Vector2 linePos, Vector2 lineDir, Vector2 lineVel, float dt)
        {
            bool overlapping = Overlap.OverlapCircleLine(circle.Pos, circle.Radius, linePos, lineDir);
            Vector2 vel = circle.Vel - lineVel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            Vector2 intersectionPoint, normal;
            float t;
            float p = lineDir.X * vel.Y - lineDir.Y * vel.X;
            if (p < 0.0f)
            {
                Vector2 w = linePos - circle.Pos;
                t = (lineDir.X * w.Y - lineDir.Y * w.X + circle.Radius) / p;
                if (t < 0.0f || t > 1.0f) return new();
                intersectionPoint = circle.Pos + vel * t;
                normal = new(-lineDir.Y, lineDir.X);

            }
            else if (p > 0.0f)
            {
                Vector2 w = linePos - circle.Pos;
                t = (lineDir.X * w.Y - lineDir.Y * w.X - circle.Radius) / p;
                if (t < 0.0f || t > 1.0f) return new();
                intersectionPoint = circle.Pos + vel * t;
                normal = new(lineDir.Y, -lineDir.X);
            }
            else
            {
                return new(true);
            }

            Vector2 collisionPoint = intersectionPoint - circle.Radius * normal;
            float remaining = 1.0f - t;
            float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
            Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
            //Vector2 reflectPoint = intersectionPoint + reflectVector;
            return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, lineVel);
        }
        public static CastInfo CastCircleRay(CircleCollider circle, Vector2 rayPos, Vector2 rayDir, Vector2 rayVel, float dt)
        {
            bool overlapping = Overlap.OverlapCircleRay(circle.Pos, circle.Radius, rayPos, rayDir);
            Vector2 vel = circle.Vel - rayVel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            float p = rayDir.X * vel.Y - rayDir.Y * vel.X;
            if (p < 0.0f)
            {
                Vector2 point = new(rayPos.X - rayDir.Y * circle.Radius, rayPos.Y + rayDir.X * circle.Radius);
                Vector2 w1 = point - circle.Pos;
                float tr = (vel.X * w1.Y - vel.Y * w1.X) / p;
                if (tr < 0.0f)
                {
                    Vector2 w2 = rayPos - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = rayPos;
                    Vector2 normal = (intersectionPoint - rayPos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, rayVel);
                }
                else
                {
                    float t = (rayDir.X * w1.Y - rayDir.Y * w1.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = new(intersectionPoint.X + rayDir.Y * circle.Radius, intersectionPoint.Y - rayDir.X * circle.Radius); // intersectionPoint + ray.Dir * circle.Radius;
                    Vector2 normal = new(-rayDir.Y, rayDir.X);
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, rayVel);
                }
            }
            else if (p > 0.0f)
            {
                Vector2 point = new(rayPos.X + rayDir.Y * circle.Radius, rayPos.Y - rayDir.X * circle.Radius);
                Vector2 w1 = point - circle.Pos;
                float tr = (vel.X * w1.Y - vel.Y * w1.X) / p;
                if (tr < 0.0f)
                {
                    Vector2 w2 = rayPos - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = rayPos;
                    Vector2 normal = (intersectionPoint - rayPos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, rayVel);
                }
                else
                {
                    float t = (rayDir.X * w1.Y - rayDir.Y * w1.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = new(intersectionPoint.X - rayDir.Y * circle.Radius, intersectionPoint.Y + rayDir.X * circle.Radius); ;
                    Vector2 normal = new(rayDir.Y, -rayDir.X);// (intersectionPoint - ray.Pos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, rayVel);
                }
            }
            else//p == 0
            {
                return new(true);
            }
        }
    }
}

/*
    public struct CollisionResponse
    {
        public Collider shape;
        public Vector2 intersectPoint;
        public Vector2 reflectVector;
        public Vector2 normal;
    }
    public struct CollisionInfo
    {
        public bool overlapping;
        public bool collided;
        public CollisionResponse self;
        public CollisionResponse other;
        public Vector2 collisionPoint;
        public float time;
    }
    */

/*//COLLIDE (Dynamic - get collision response for both objects)
public static CollisionInfo Dynamic(Collider point, CircleCollider circle)
{
    Vector2 v = point.Vel - circle.Vel;
    bool overlapping = Overlap.Simple(point, circle); // Contains(circle.Pos, circle.Radius, point.Pos);
    if (overlapping || (v.X == 0.0f && v.X == 0.0f)) return new CollisionInfo() { collided = false, overlapping = overlapping};

    Vector2 w = circle.Pos - point.Pos;
    float qa = v.LengthSquared();
    float qb = w.X * v.X + w.Y * v.Y;
    float qc = w.LengthSquared() - circle.RadiusSquared;
    float qd = qb * qb - qa * qc;
    if (qd < 0.0f) return new CollisionInfo { collided = false, overlapping = false};
    float t = (qb - MathF.Sqrt(qd)) / qa;
    if (t < 0.0f || t > 1.0f) return new CollisionInfo { collided = false, overlapping = false};


    //Vector2 intersectPoint = point.Pos + v * t; // new Vector2(point.Pos.X + v.X * t, point.Pos.Y + v.Y * t);
    Vector2 intersectPointPoint = point.Pos + point.Vel * t; // new Vector2(point.Pos.X + point.Vel.X * t, point.Pos.Y + point.Vel.Y * t);
    Vector2 intersectPointCircle = circle.Pos + circle.Vel * t; // new Vector2(circle.Pos.X + circle.Vel.X * t, circle.Pos.Y + circle.Vel.Y * t);

    Vector2 collisionPoint = intersectPointPoint;
    Vector2 normalPoint = (intersectPointPoint - intersectPointCircle) / circle.Radius; // new Vector2((intersectPoint.X - intersectPointCircle.X) / circle.Radius, (intersectPoint.Y - intersectPointCircle.Y) / circle.Radius);
    Vector2 normalCircle = new Vector2(-normalPoint.X, -normalPoint.Y);

    float remaining = 1.0f - t;

    float p = 2.0f * (normalCircle.X * v.X + normalCircle.Y * v.Y);
    Vector2 reflectVectorPoint = (point.Vel - normalCircle * p) * remaining;
    Vector2 reflectVectorCircle = (circle.Vel + normalCircle * p) * remaining;

    //float dp = 2.0f * (point.Vel.X * normalPoint.X + point.Vel.Y * normalPoint.Y);
    //Vector2 reflectVectorPoint = (point.Vel - normalPoint * dp) * remaining;// new Vector2(remaining * (point.Vel.X - dp * normalPoint.X), remaining * (point.Vel.Y - dp * normalPoint.Y));
    //Vector2 reflectPointPoint = intersectPointPoint + reflectVectorPoint;// new Vector2(intersectPointPoint.X + reflectVectorPoint.X, intersectPointPoint.Y + reflectVectorPoint.Y);

    //float dc = 2.0f * (circle.Vel.X * normalCircle.X + circle.Vel.Y * normalCircle.Y);
    //Vector2 reflectVectorCircle = (circle.Vel - normalCircle * dc) * remaining; //new Vector2(remaining * (circle.Vel.X - dc * normalCircle.X), remaining * (circle.Vel.Y - dc * normalCircle.Y));
    //Vector2 reflectPointCircle = intersectPointCircle + reflectVectorCircle;// new Vector2(intersectPointCircle.X + reflectVectorCircle.X, intersectPointCircle.Y + reflectVectorCircle.Y);


    return
        new CollisionInfo
        {
            collided = true,
            overlapping = false,
            self = new CollisionResponse { shape = point, intersectPoint = intersectPointPoint, normal = normalPoint, reflectVector = reflectVectorPoint },
            other = new CollisionResponse { shape = circle, intersectPoint = intersectPointCircle, normal = normalCircle, reflectVector = reflectVectorCircle },
            collisionPoint = collisionPoint,
            time = t
        };
}
public static CollisionInfo Dynamic(CircleCollider circle, Collider point)
{
    Vector2 v = circle.Vel - point.Vel;
    bool overlapping = Overlap.Simple(circle, point); // Contains(circle.Pos, circle.Radius, point.Pos);
    if (overlapping || (v.X == 0.0f && v.X == 0.0f)) return new CollisionInfo() { collided = false, overlapping = overlapping };
    //if (v.X == 0.0f && v.X == 0.0f) return new CollisionInfo { collided = false, overlapping = false };

    Vector2 w = point.Pos - circle.Pos;
    float qa = v.LengthSquared();
    float qb = w.X * v.X + w.Y * v.Y;
    float qc = w.LengthSquared() - circle.RadiusSquared;
    float qd = qb * qb - qa * qc;
    if (qd < 0.0f) return new CollisionInfo { collided = false , overlapping = false};
    float t = (qb - MathF.Sqrt(qd)) / qa;
    if (t < 0.0f || t > 1.0f) return new CollisionInfo { collided = false, overlapping = false };


    //Vector2 intersectPoint = circle.Pos + v * t; // new Vector2(point.Pos.X + v.X * t, point.Pos.Y + v.Y * t);
    Vector2 intersectPointCircle = circle.Pos + circle.Vel * t; // new Vector2(circle.Pos.X + circle.Vel.X * t, circle.Pos.Y + circle.Vel.Y * t);
    Vector2 intersectPointPoint = point.Pos + point.Vel * t; // new Vector2(point.Pos.X + point.Vel.X * t, point.Pos.Y + point.Vel.Y * t);

    Vector2 collisionPoint = intersectPointPoint;
    Vector2 normalCircle = (intersectPointCircle - intersectPointPoint) / circle.Radius; //new Vector2(-normalPoint.X, -normalPoint.Y);
    Vector2 normalPoint = new(-normalCircle.X, -normalCircle.Y); // new Vector2((intersectPoint.X - intersectPointCircle.X) / circle.Radius, (intersectPoint.Y - intersectPointCircle.Y) / circle.Radius);

    float remaining = 1.0f - t;

    float p = 2.0f * (normalPoint.X * v.X + normalPoint.Y * v.Y);
    Vector2 reflectVectorCircle = (circle.Vel - normalPoint * p) * remaining;
    Vector2 reflectVectorPoint = (point.Vel + normalPoint * p) * remaining;

    //float dc = 2.0f * (circle.Vel.X * normalCircle.X + circle.Vel.Y * normalCircle.Y);
    //Vector2 reflectVectorCircle = (circle.Vel - normalCircle * dc) * remaining; //new Vector2(remaining * (circle.Vel.X - dc * normalCircle.X), remaining * (circle.Vel.Y - dc * normalCircle.Y));
    //Vector2 reflectPointCircle = intersectPointCircle + reflectVectorCircle; // new Vector2(intersectPointCircle.X + reflectVectorCircle.X, intersectPointCircle.Y + reflectVectorCircle.Y);

    //float dp = 2.0f * (point.Vel.X * normalPoint.X + point.Vel.Y * normalPoint.Y);
    //Vector2 reflectVectorPoint = (point.Vel - normalPoint * dp) * remaining; // new Vector2(remaining * (point.Vel.X - dp * normalPoint.X), remaining * (point.Vel.Y - dp * normalPoint.Y));
    //Vector2 reflectPointPoint = intersectPointPoint + reflectVectorPoint; // new Vector2(intersectPointPoint.X + reflectVectorPoint.X, intersectPointPoint.Y + reflectVectorPoint.Y);

    return
        new CollisionInfo
        {
            collided = true,
            overlapping = false,
            self = new CollisionResponse { shape = circle, intersectPoint = intersectPointCircle, normal = normalCircle, reflectVector = reflectVectorCircle },
            other = new CollisionResponse { shape = point, intersectPoint = intersectPointPoint, normal = normalPoint, reflectVector = reflectVectorPoint },
            collisionPoint = collisionPoint,
            time = t
        };
}
public static CollisionInfo Dynamic(CircleCollider self, CircleCollider other)
{
    Vector2 v = self.Vel - other.Vel;
    bool overlapping = Overlap.Simple(self, other);
    if (overlapping || (v.X == 0.0f && v.Y == 0.0f)) return new CollisionInfo() { collided = false, overlapping = overlapping };

    float r = self.Radius + other.Radius;
    IntersectionInfo intersect = PointIntersectCircle(self.Pos, v, other.Pos, r);
    if (!intersect.intersected) return new CollisionInfo { collided = false, overlapping = false };
    Vector2 intersectionPointSelf = self.Pos + self.Vel * intersect.time;
    Vector2 intersectionPointOther = other.Pos + other.Vel * intersect.time;
    Vector2 normalSelf = (intersectionPointSelf - intersectionPointOther) / r;
    Vector2 normalOther = normalSelf * -1.0f;
    Vector2 collisionPoint = intersectionPointSelf + normalOther * self.Radius;

    float p = 2.0f * (normalOther.X * v.X + normalOther.Y * v.Y);
    Vector2 reflectVectorSelf = (self.Vel - normalOther * p) * intersect.remaining;
    Vector2 reflectVectorOther = (other.Vel + normalOther * p) * intersect.remaining;

    return new CollisionInfo
    {
        collided = true,
        overlapping = false,
        self = new CollisionResponse { shape = self, intersectPoint = intersectionPointSelf, normal = normalSelf, reflectVector = reflectVectorSelf },
        other = new CollisionResponse { shape = other, intersectPoint = intersectionPointOther, normal = normalOther, reflectVector = reflectVectorOther },
        collisionPoint = collisionPoint,
        time = intersect.time,
    };
}
public static CollisionInfo Dynamic(Collider self, Collider other)
{
    //point - point collision basically never happens so point - circle collision code is used
    CircleCollider circle = new(other.Pos, other.Vel, Overlap.POINT_OVERLAP_EPSILON);
    Vector2 v = self.Vel - circle.Vel;
    bool overlapping = Overlap.Simple(self, circle);
    if (overlapping || (v.X == 0.0f && v.X == 0.0f)) return new CollisionInfo() { collided = false, overlapping = overlapping };

    Vector2 w = circle.Pos - self.Pos;
    float qa = v.LengthSquared();
    float qb = w.X * v.X + w.Y * v.Y;
    float qc = w.LengthSquared() - circle.RadiusSquared;
    float qd = qb * qb - qa * qc;
    if (qd < 0.0f) return new CollisionInfo { collided = false, overlapping = false };
    float t = (qb - MathF.Sqrt(qd)) / qa;
    if (t < 0.0f || t > 1.0f) return new CollisionInfo { collided = false, overlapping = false };

    Vector2 intersectPointPoint = self.Pos + self.Vel * t;
    Vector2 intersectPointCircle = circle.Pos + circle.Vel * t;

    Vector2 collisionPoint = intersectPointPoint;
    Vector2 normalPoint = (intersectPointPoint - intersectPointCircle) / circle.Radius;
    Vector2 normalCircle = new Vector2(-normalPoint.X, -normalPoint.Y);

    float remaining = 1.0f - t;

    float p = 2.0f * (normalCircle.X * v.X + normalCircle.Y * v.Y);
    Vector2 reflectVectorPoint = (self.Vel - normalCircle * p) * remaining;
    Vector2 reflectVectorCircle = (circle.Vel + normalCircle * p) * remaining;
    return
        new CollisionInfo
        {
            collided = true,
            overlapping = false,
            self = new CollisionResponse { shape = self, intersectPoint = intersectPointPoint, normal = normalPoint, reflectVector = reflectVectorPoint },
            other = new CollisionResponse { shape = other, intersectPoint = intersectPointCircle, normal = normalCircle, reflectVector = reflectVectorCircle },
            collisionPoint = collisionPoint,
            time = t
        };

    //Vector2 vel = self.Vel - other.Vel;
    //Vector2 w = other.Pos - self.Pos;
    //bool overlapping = w.LengthSquared() == 0.0f;
    //if (overlapping || vel.LengthSquared() <= 0.0f) return new CollisionInfo() { collided = false, overlapping = overlapping };
    //
    //float p = w.X * vel.Y - w.Y * vel.X;
    //if (p != 0.0f) return new CollisionInfo() { collided = false, overlapping = false };
    //float t = vel.X == 0.0f ? w.Y / vel.Y : w.X / vel.X;
    //if(t < 0.0f || t > 1.0f) return new CollisionInfo() { collided = false, overlapping = false };
    //
    //Vector2 intersectionPoint = self.Pos + vel * t;
    //Vector2 intersectionPointSelf = self.Pos + self.Vel * t;
    //Vector2 intersectionPointOther = other.Pos + other.Vel * t;
    //float l = w.Length();
    //Vector2 normalOther = w / l;
    //Vector2 normalSelf = Vector2.Negate(normalOther);
    //Vector2 collisionPoint = intersectionPointSelf;
    //float remaining = 1.0f - t;
    //float dSelf = (self.Vel.X * normalSelf.X + self.Vel.Y * normalSelf.Y) * 2.0f;
    //Vector2 reflectVectorSelf = new(remaining * (self.Vel.X - dSelf * normalSelf.X), remaining * (self.Vel.Y - dSelf * normalSelf.Y));
    //Vector2 reflectPointSelf = intersectionPointSelf + reflectVectorSelf;
    //
    //float dOther = (other.Vel.X * normalOther.X + other.Vel.Y * normalOther.Y) * 2.0f;
    //Vector2 reflectVectorOther = new(remaining * (other.Vel.X - dOther * normalOther.X), remaining * (other.Vel.Y - dOther * normalOther.Y));
    //Vector2 reflectPointOther = intersectionPointOther + reflectVectorOther;
    //
    //return new CollisionInfo
    //{
    //    overlapping = false,
    //    collided = true,
    //    self = new CollisionResponse { shape = self, available = true, intersectPoint = intersectionPointSelf, normal = normalSelf, reflectPoint = reflectPointSelf, reflectVector = reflectVectorSelf },
    //    other = new CollisionResponse { shape = other, available = true, intersectPoint = intersectionPointOther, normal = normalOther, reflectPoint = intersectionPointOther, reflectVector = reflectVectorOther },
    //    collisionPoint = collisionPoint,
    //    intersectPoint = intersectionPoint,
    //    time = t,
    //    remaining = remaining
    //};
}
*/

/*public class LineShape : Collider
    {
        public LineShape() { }
        public LineShape(float x, float y, float dx, float dy) : base(x, y) { Dir = new(dx, dy); }
        public LineShape(Vector2 pos, Vector2 dir) : base(pos, new(0.0f, 0.0f)) { Dir = dir; }
        public LineShape(Vector2 pos, Vector2 dir, Vector2 offset) : base(pos, new(0.0f, 0.0f), offset) { Dir = dir; }
        
        public Vector2 Dir { get; set; }

        //public override Rectangle GetBoundingRect() { return new(Pos.X - radius, Pos.Y - radius, Pos.X + radius, Pos.X + radius); }
        public override void DebugDrawShape(Color color) 
        {
            Raylib.DrawCircle((int)Pos.X, (int)Pos.Y, 10.0f, color);
            Raylib.DrawLineEx(Pos, Pos + Dir * 5000.0f, 5.0f, color);
            Raylib.DrawLineEx(Pos, Pos - Dir * 5000.0f, 5.0f, color);
        }
    }
    public class RayShape : LineShape
    {
        public RayShape() { }
        public RayShape(float x, float y, float dx, float dy) : base(x, y, dx, dy) {}
        public RayShape(Vector2 pos, Vector2 dir) : base(pos, dir) {}
        public RayShape(Vector2 pos, Vector2 dir, Vector2 offset) : base(pos, dir, offset) {}

        //public override Rectangle GetBoundingRect() { return new(Pos.X - radius, Pos.Y - radius, Pos.X + radius, Pos.X + radius); }

        public override void DebugDrawShape(Color color)
        {
            Raylib.DrawCircle((int)Pos.X, (int)Pos.Y, 10.0f, color);
            Raylib.DrawLineEx(Pos, Pos + Dir * 5000.0f, 5.0f, color);
        }
    }*/

/*public enum AttractionType
    {
        REALISTIC = 0,
        LINEAR = 1,
        CONSTANT = 2
    }
    public static void Attract(Collider self, Collider other, float attractionStrength = 5.0f, AttractionType attractionType = AttractionType.LINEAR)
    {
        Vector2 w = self.Pos - other.Pos;
        float disSq = MathF.Max(w.LengthSquared(), 1.0f);

        if (attractionType == AttractionType.REALISTIC)
        {
            float strength = attractionStrength * ((self.Mass * other.Mass) / disSq);
            Vector2 force = Vec.Normalize(w) * strength;
            other.AccumulateForce(force);
        }
        else if (attractionType == AttractionType.LINEAR)
        {
            float strength = attractionStrength * (self.Mass / MathF.Sqrt(disSq));
            Vector2 force = Vec.Normalize(w) * strength;
            other.AccumulateForce(force);
        }
        else
        {
            float strength = attractionStrength * self.Mass;
            Vector2 force = Vec.Normalize(w) * strength;
            other.AccumulateForce(force);
        }
    }
    public static void Attract(ICollidable self, ICollidable other, float attractionStrength = 5.0f, AttractionType attractionType = AttractionType.LINEAR)
    {
        Collider selfCol = self.GetCollider();
        Collider otherCol = other.GetCollider();
        Vector2 w = selfCol.Pos - otherCol.Pos;
        float disSq = MathF.Max(w.LengthSquared(), 1.0f);
        if (attractionType == AttractionType.REALISTIC)
        {
            float strength = attractionStrength * ((selfCol.Mass * otherCol.Mass) / disSq);
            Vector2 force = Vec.Normalize(w) * strength;
            otherCol.AccumulateForce(force);
        }
        else if (attractionType == AttractionType.LINEAR)
        {
            float strength = attractionStrength * (selfCol.Mass / MathF.Sqrt(disSq));
            Vector2 force = Vec.Normalize(w) * strength;
            otherCol.AccumulateForce(force);
        }
        else
        {
            float strength = attractionStrength * selfCol.Mass;
            Vector2 force = Vec.Normalize(w) * strength;
            otherCol.AccumulateForce(force);
        }
    }
    public static void Attract(Collider self, ICollidable other, float attractionStrength = 5.0f, AttractionType attractionType = AttractionType.LINEAR)
    {
        Collider col = other.GetCollider();
        Vector2 w = self.Pos - col.Pos;
        float disSq = MathF.Max(w.LengthSquared(), 1.0f);

        if (attractionType == AttractionType.REALISTIC)
        {
            float strength = attractionStrength * ((self.Mass * col.Mass) / disSq);
            Vector2 force = Vec.Normalize(w) * strength;
            col.AccumulateForce(force);
        }
        else if (attractionType == AttractionType.LINEAR)
        {
            float strength = attractionStrength * (self.Mass / MathF.Sqrt(disSq));
            Vector2 force = Vec.Normalize(w) * strength;
            col.AccumulateForce(force);
        }
        else
        {
            float strength = attractionStrength * self.Mass;
            Vector2 force = Vec.Normalize(w) * strength;
            col.AccumulateForce(force);
        }
    }
    public static void Attract(Vector2 pos, float mass, ICollidable other, float attractionStrength = 5.0f, AttractionType attractionType = AttractionType.LINEAR)
    {
        Collider col = other.GetCollider();
        Vector2 w = pos - col.Pos;
        float disSq = MathF.Max(w.LengthSquared(), 1.0f);

        if (attractionType == AttractionType.REALISTIC)
        {
            float strength = attractionStrength * ((mass * col.Mass) / disSq);
            Vector2 force = Vec.Normalize(w) * strength;
            col.AccumulateForce(force);

        }
        else if (attractionType == AttractionType.LINEAR)
        {
            float strength = attractionStrength * (mass / MathF.Sqrt(disSq));
            Vector2 force = Vec.Normalize(w) * strength;
            col.AccumulateForce(force);
        }
        else
        {
            float strength = attractionStrength * mass;
            Vector2 force = Vec.Normalize(w) * strength;
            col.AccumulateForce(force);
        }
    }
    public static void Attract(Vector2 pos, float mass, Collider other, float attractionStrength = 5.0f, AttractionType attractionType = AttractionType.LINEAR)
    {
        Vector2 w = pos - other.Pos;
        float disSq = MathF.Max(w.LengthSquared(), 1.0f);

        if (attractionType == AttractionType.REALISTIC)
        {
            float strength = attractionStrength * ((mass * other.Mass) / disSq);
            Vector2 force = Vec.Normalize(w) * strength;
            other.AccumulateForce(force);
        }
        else if (attractionType == AttractionType.LINEAR)
        {
            float strength = attractionStrength * (mass / MathF.Sqrt(disSq));
            Vector2 force = Vec.Normalize(w) * strength;
            other.AccumulateForce(force);
        }
        else
        {
            float strength = attractionStrength * mass;
            Vector2 force = Vec.Normalize(w) * strength;
            other.AccumulateForce(force);
        }
    }
    public static Vector2 Attract(Vector2 selfPos, float selfMass, Vector2 otherPos, float otherMass, float attractionStrength = 5.0f, AttractionType attractionType = AttractionType.LINEAR)
    {
        Vector2 w = selfPos - otherPos;
        float disSq = MathF.Max(w.LengthSquared(), 1.0f);

        if (attractionType == AttractionType.REALISTIC)
        {
            float strength = attractionStrength * ((selfMass * otherMass) / disSq);
            return Vec.Normalize(w) * strength;
        }
        else if (attractionType == AttractionType.LINEAR)
        {
            float strength = attractionStrength * (selfMass / MathF.Sqrt(disSq));
            return Vec.Normalize(w) * strength;
        }
        else
        {
            float strength = attractionStrength * selfMass;
            return Vec.Normalize(w) * strength;
        }
    }
    */

