using ShapeEngine.Lib;
using System.Collections.Generic;
using System.Numerics;

namespace ShapeEngine.Core
{
    public struct QueryInfo
    {
        public ICollidable collidable;
        public Intersection intersection;
        public QueryInfo(ICollidable collidable)
        {
            this.collidable = collidable;
            this.intersection = new();
        }
        public QueryInfo(ICollidable collidable, CollisionPoints points)
        {
            this.collidable = collidable;
            this.intersection = new(points);
        }

    }

    public class CollisionHandler
    {
        internal class OverlapRegister : Dictionary<ICollidable, HashSet<ICollidable>>
        {
            public bool AddEntry(ICollidable self, ICollidable other)
            {
                if (ContainsKey(self))
                {
                    return this[self].Add(other);
                }

                Add(self, new() { other });
                return true;
            }
            public bool RemoveEntry(ICollidable self, ICollidable other)
            {
                if (ContainsKey(self))
                {
                    return this[self].Remove(other);
                }

                return false;
            }
            public bool HasEntry(ICollidable self, ICollidable other)
            {
                if (ContainsKey(self))
                {
                    return this[self].Contains(other);
                }
                return false;
            }
            public List<(ICollidable self, ICollidable other)> GetPairs()
            {
                List<(ICollidable self, ICollidable other)> pairs = new();
                foreach (var kvp in this)
                {
                    var self = kvp.Key;
                    foreach (var other in kvp.Value)
                    {
                        pairs.Add((self, other));
                    }
                }
                return pairs;
            }
        }

       /// <summary>
       /// Bucket First algorithm iterates over all buckets and checks collision within each indiviual bucket.
       /// Does not scale well with increase bucket count. If you use large areas with a lot of rows/columns this algorithm might perform worse!
       /// If false the default algorithm is used. The default algorithm iterates over all collidables and checks for other collidables that are 
       /// in the surounding buckets. 
       /// </summary>
        public bool BucketFirstAlgorithm = false;

        public int IterationsPerFrame = 0;
        public int CollisionChecksPerFrame = 0;
        public int ClosestPointChecksPerFrame = 0;
        
        protected List<ICollidable> collidables = new();
        protected List<ICollidable> tempHolding = new();
        protected List<ICollidable> tempRemoving = new();
        protected SpatialHash spatialHash;
        protected Dictionary<ICollidable, List<Collision>> collisionStack = new();
        private OverlapRegister activeRegister = new();
        private OverlapRegister oldRegister = new();


        public int Count { get { return collidables.Count; } }

        public CollisionHandler(float x, float y, float w, float h, int rows, int cols) { spatialHash = new(x, y, w, h, rows, cols); }
        public CollisionHandler(Rect bounds, int rows, int cols) { spatialHash = new(bounds.x, bounds.y, bounds.width, bounds.height, rows, cols); }
        
        
        public void UpdateBounds(Rect newBounds) { spatialHash = spatialHash.Resize(newBounds); }
        
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
            //collisionInfos.Clear();
            collisionStack.Clear();
        }
        public void Close()
        {
            Clear();
            spatialHash.Close();
        }
        
        public virtual void Update(float dt)
        {
            IterationsPerFrame = 0;
            CollisionChecksPerFrame = 0;
            ClosestPointChecksPerFrame = 0;
            FillSpatialHash();
            
            if (BucketFirstAlgorithm) ProcessCollisionsBucketFirst();
            else ProcessCollisionsCollidableFirst();
            
            Resolve();
        }

        protected virtual void FillSpatialHash()
        {
            spatialHash.Clear();

            //fill spatial hash and filter out all disabled colliders
            for (int i = collidables.Count - 1; i >= 0; i--)
            {
                var collidable = collidables[i];
                var collider = collidable.GetCollider();
                if (collider.Enabled)
                {
                    spatialHash.Add(collidable);
                }
                IterationsPerFrame++;

            }
        }
        protected virtual void ProcessCollisionsBucketFirst()
        {
            int bucketCount = spatialHash.GetBucketCount();
            for (int i = 0; i < bucketCount; i++)
            {
                var bucketInfo = spatialHash.GetBucketInfo(i);
                if (!bucketInfo.Valid) continue;

                foreach (var collidable in bucketInfo.Active)
                {
                    List<ICollidable> others = bucketInfo.GetOthers(collidable);

                    bool computeIntersections = collidable.GetCollider().ComputeIntersections;
                    List<Collision> cols = new();
                    foreach (var other in others)
                    {
                        IterationsPerFrame++;
                        if (activeRegister.HasEntry(collidable, other)) continue;

                        bool overlap = SGeometry.Overlap(collidable, other);
                        if (overlap)
                        {
                            bool firstContact = !oldRegister.RemoveEntry(collidable, other);
                            activeRegister.AddEntry(collidable, other);
                            if (computeIntersections)
                            {
                                CollisionChecksPerFrame++;
                                var collisionPoints = SGeometry.Intersect(collidable, other);

                                //shapes overlap but no collision points means collidable is completely inside other
                                //closest point on bounds of other are now used for collision point
                                if (collisionPoints.Count <= 0)
                                {
                                    Vector2 refPoint = collidable.GetCollider().GetPreviousPosition();
                                    var shape = other.GetCollider().GetShape();
                                    if (!shape.IsPointInside(refPoint))
                                    {
                                        CollisionPoint closest = shape.GetClosestPoint(refPoint);
                                        collisionPoints.Add(closest);
                                        ClosestPointChecksPerFrame++;
                                    }

                                }

                                Collision c = new(collidable, other, firstContact, collisionPoints);
                                cols.Add(c);
                            }
                            else
                            {
                                Collision c = new(collidable, other, firstContact);
                                cols.Add(c);
                            }
                        }
                    }

                    if (cols.Count > 0)
                    {
                        if (collisionStack.ContainsKey(collidable))
                        {
                            collisionStack[collidable].AddRange(cols);
                        }
                        else collisionStack.Add(collidable, cols);
                    }
                }
                //IterationsPerFrame++;
            }

        }
        protected virtual void ProcessCollisionsCollidableFirst()
        {
            foreach (var collidable in collidables)
            {
                var others = spatialHash.GetObjects(collidable);
                bool computeIntersections = collidable.GetCollider().ComputeIntersections;
                List<Collision> cols = new();
                foreach (var other in others)
                {
                    IterationsPerFrame++;

                    bool overlap = SGeometry.Overlap(collidable, other);
                    if (overlap)
                    {
                        bool firstContact = !oldRegister.RemoveEntry(collidable, other);
                        activeRegister.AddEntry(collidable, other);
                        if (computeIntersections)
                        {
                            CollisionChecksPerFrame++;
                            var collisionPoints = SGeometry.Intersect(collidable, other);

                            //shapes overlap but no collision points means collidable is completely inside other
                            //closest point on bounds of other are now used for collision point
                            if (collisionPoints.Count <= 0)
                            {
                                Vector2 refPoint = collidable.GetCollider().GetPreviousPosition();
                                var shape = other.GetCollider().GetShape();
                                if (!shape.IsPointInside(refPoint))
                                {
                                    CollisionPoint closest = shape.GetClosestPoint(refPoint);
                                    collisionPoints.Add(closest);
                                    ClosestPointChecksPerFrame++;
                                }

                            }

                            Collision c = new(collidable, other, firstContact, collisionPoints);
                            cols.Add(c);
                        }
                        else
                        {
                            Collision c = new(collidable, other, firstContact);
                            cols.Add(c);
                        }
                    }
                }

                if (cols.Count > 0)
                {
                    if (collisionStack.ContainsKey(collidable))
                    {
                        collisionStack[collidable].AddRange(cols);
                    }
                    else collisionStack.Add(collidable, cols);
                }
            }
        }

        protected virtual void Resolve()
        {
            foreach (var collidable in tempHolding)
            {
                collidables.Add(collidable);
            }
            tempHolding.Clear();

            foreach (var collidable in tempRemoving)
            {
                collidables.Remove(collidable);
            }
            tempRemoving.Clear();


            foreach (var kvp in collisionStack)
            {
                var collidable = kvp.Key;
                var cols = kvp.Value;
                if(cols.Count > 0)
                {
                    CollisionInformation collisionInfo = new(cols, collidable.GetCollider().ComputeIntersections);
                    collidable.Overlap(collisionInfo);
                }
            }
            collisionStack.Clear();

            List<(ICollidable self, ICollidable other)> overlapEndedPairs = oldRegister.GetPairs();
            foreach (var pair in overlapEndedPairs)
            {
                pair.self.OverlapEnded(pair.other);
            }

            oldRegister = activeRegister;
            activeRegister = new();
        }

        
        public List<QueryInfo> QuerySpace(ICollidable caster, bool sorted = false)
        {
            return QuerySpace(caster.GetCollider(), sorted, caster.GetCollisionMask());
        }
        public List<QueryInfo> QuerySpace(ICollider caster, bool sorted = false, params uint[] collisionMask)
        {
            List<QueryInfo> infos = new();
            List<ICollidable> objects = spatialHash.GetObjects(caster, collisionMask);
            foreach (ICollidable obj in objects)
            {
                if (obj.GetCollider() == caster) continue;
                var collisionPoints = SGeometry.Intersect(caster, obj.GetCollider());
                if (collisionPoints.Valid) infos.Add(new(obj, collisionPoints));
            }

            if (sorted && infos.Count > 1)
            {
                infos.Sort
                (
                    (a, b) =>
                    {
                        Vector2 pos = caster.Pos;
                        float la = (pos - a.intersection.CollisionSurface.Point).LengthSquared();
                        float lb = (pos - b.intersection.CollisionSurface.Point).LengthSquared();

                        if (la > lb) return 1;
                        else if (la == lb) return 0;
                        else return -1;
                    }
                );
            }

            return infos;
        }
        public List<QueryInfo> QuerySpace(IShape shape, bool sorted = false, params uint[] collisionMask)
        {
            List<QueryInfo> infos = new();
            List<ICollidable> objects = spatialHash.GetObjects(shape, collisionMask);
            foreach (ICollidable obj in objects)
            {
                var collisionPoints = SGeometry.Intersect(shape, obj.GetCollider().GetShape());
                if (collisionPoints.Valid) infos.Add(new(obj, collisionPoints));
            }

            if (sorted && infos.Count > 1)
            {
                infos.Sort
                (
                    (a, b) =>
                    {
                        Vector2 pos = shape.GetCentroid();
                        float la = (pos - a.intersection.CollisionSurface.Point).LengthSquared();
                        float lb = (pos - b.intersection.CollisionSurface.Point).LengthSquared();

                        if (la > lb) return 1;
                        else if (la == lb) return 0;
                        else return -1;
                    }
                );
            }

            return infos;
        }
        public List<QueryInfo> QuerySpace(IShape shape, ICollidable[] exceptions, bool sorted = false, params uint[] collisionMask)
        {
            List<QueryInfo> infos = new();
            List<ICollidable> objects = spatialHash.GetObjects(shape, collisionMask);
            foreach (ICollidable obj in objects)
            {
                if(exceptions.Contains(obj)) continue;

                var collisionPoints = SGeometry.Intersect(shape, obj.GetCollider().GetShape());
                if (collisionPoints.Valid) infos.Add(new(obj, collisionPoints));
            }

            if (sorted && infos.Count > 1)
            {
                infos.Sort
                (
                    (a, b) =>
                    {
                        Vector2 pos = shape.GetCentroid();
                        float la = (pos - a.intersection.CollisionSurface.Point).LengthSquared();
                        float lb = (pos - b.intersection.CollisionSurface.Point).LengthSquared();

                        if (la > lb) return 1;
                        else if (la == lb) return 0;
                        else return -1;
                    }
                );
            }

            return infos;
        }

        
        public List<ICollidable> CastSpace(ICollidable caster, bool sorted = false)
        {
            return CastSpace(caster.GetCollider(), sorted, caster.GetCollisionMask());
        }
        public List<ICollidable> CastSpace(ICollider caster, bool sorted = false, params uint[] collisionMask)
        {
            List<ICollidable> bodies = new();
            List<ICollidable> objects = spatialHash.GetObjects(caster, collisionMask);
            foreach (ICollidable obj in objects)
            {
                if(obj == caster) continue;

                if (SGeometry.Overlap(caster, obj.GetCollider()))
                {
                    bodies.Add(obj);
                }
            }
            if (sorted && bodies.Count > 1)
            {
                bodies.Sort
                (
                    (a, b) =>
                    {
                        Vector2 pos = caster.Pos;
                        float la = (pos - a.GetCollider().Pos).LengthSquared();
                        float lb = (pos - b.GetCollider().Pos).LengthSquared();

                        if (la > lb) return 1;
                        else if (la == lb) return 0;
                        else return -1;
                    }
                );
            }
            return bodies;
        }
        public List<ICollidable> CastSpace(IShape castShape, bool sorted = false, params uint[] collisionMask)
        {
            List<ICollidable> bodies = new();
            List<ICollidable> objects = spatialHash.GetObjects(castShape, collisionMask);
            foreach (ICollidable obj in objects)
            {
                if (SGeometry.Overlap(castShape, obj.GetCollider().GetShape()))
                {
                    bodies.Add(obj);
                }
            }
            if (sorted && bodies.Count > 1)
            {
                bodies.Sort
                (
                    (a, b) =>
                    {
                        Vector2 pos = castShape.GetCentroid();
                        float la = (pos - a.GetCollider().Pos).LengthSquared();
                        float lb = (pos - b.GetCollider().Pos).LengthSquared();

                        if (la > lb) return 1;
                        else if (la == lb) return 0;
                        else return -1;
                    }
                );
            }
            return bodies;
        }
        

        public void DebugDrawGrid(Raylib_CsLo.Color border, Raylib_CsLo.Color fill)
        {
            spatialHash.DebugDrawGrid(border, fill);
        }
    }

}


/*
        public static void SortQueryInfoPoints(Vector2 p, QueryInfo info)
        {
            if (!info.intersection.valid) return;
            if (info.intersection.points.Count <= 1) return;
            info.intersection.points.Sort
            (
                (a, b) =>
                {
                    float la = (p - a.p).LengthSquared();
                    float lb = (p - b.p).LengthSquared();

                    if (la > lb) return 1;
                    else if (la == lb) return 0;
                    else return -1;
                }
            );
        }
        public static void SortQueryInfoPoints(Vector2 p, List<QueryInfo> infos)
        {
            foreach (var info in infos)
            {
                SortQueryInfoPoints(p, info);
            }
        }
        */




/*
        public List<ICollidable> CastSpace(Rect rect, bool sorted = false, params uint[] collisionMask)
        {
            RectCollider collider = new(rect);
            return GetCastBodies(collider, sorted, collisionMask);
        }
        public List<ICollidable> CastSpace(Vector2 pos, float r, bool sorted = false, params uint[] collisionMask)
        {
            CircleCollider collider = new(pos, r);
            return GetCastBodies(collider, sorted, collisionMask);
        }
        public List<ICollidable> CastSpace(Vector2 pos, Vector2 size, Vector2 alignement, bool sorted = false, params uint[] collisionMask)
        {
            RectCollider collider = new(pos, size, alignement);
            return GetCastBodies(collider, sorted, collisionMask);
        }
        public List<ICollidable> CastSpace(Vector2 pos, Vector2 dir, float length, bool sorted = false, params uint[] collisionMask)
        {
            SegmentCollider collider = new(pos, dir, length);
            return GetCastBodies(collider, sorted, collisionMask);
        }
        public List<ICollidable> CastSpace(Vector2 start, Vector2 end, bool sorted = false, params uint[] collisionMask)
        {
            SegmentCollider collider = new(start, end);
            return GetCastBodies(collider, sorted, collisionMask);
        }
        */
//public List<QueryInfo> QuerySpace(Rect rect, bool sorted = false, params uint[] collisionMask)
        //{
        //    return QuerySpace(rect, sorted, collisionMask);
        //    //RectCollider collider = new(rect);
        //    //return GetQueryInfo(collider, sorted, collisionMask);
        //}
        //public List<QueryInfo> QuerySpace(Vector2 pos, float r, bool sorted = false, params uint[] collisionMask)
        //{
        //    return QuerySpace(new Circle(pos, r), sorted, collisionMask);
        //    //CircleCollider collider = new(pos, r);
        //    //return GetQueryInfo(collider, sorted, collisionMask);
        //}
        //public List<QueryInfo> QuerySpace(Vector2 pos, Vector2 dir, float length, bool sorted = false, params uint[] collisionMask)
        //{
        //    SegmentCollider collider = new(pos, dir, length);
        //    return GetQueryInfo(collider, sorted, collisionMask);
        //}
        //public List<QueryInfo> QuerySpace(Vector2 start, Vector2 end, bool sorted = false, params uint[] collisionMask)
        //{
        //    SegmentCollider collider = new(start, end);
        //    return GetQueryInfo(collider, sorted, collisionMask);
        //}
//public List<QueryInfo> QuerySpace(Vector2 pos, Vector2 size, Vector2 alignement, bool sorted = false, params uint[] collisionMask)
        //{
        //    return QuerySpace(new Rect(pos, size, alignement), sorted, collisionMask);
        //    //RectCollider collider = new(pos, size, alignement);
        //    //return GetQueryInfo(collider, sorted, collisionMask);
        //}