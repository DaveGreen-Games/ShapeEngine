using ShapeEngine.Lib;
using System.Numerics;
using System.Xml.Schema;

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
        public QueryInfo(ICollidable collidable, Intersection intersection)
        {
            this.collidable = collidable;
            this.intersection = intersection;
        }

    }

    public class CollisionHandler
    {
        protected List<ICollidable> collidables = new();
        protected List<ICollidable> tempHolding = new();
        protected List<ICollidable> tempRemoving = new();
        protected SpatialHash spatialHash;

        protected List<CollisionInfo> overlapInfos = new();
        protected List<(ICollidable collider, ICollidable other)> overlapEnded= new();

        protected Dictionary<ICollidable, List<ICollidable>> overlaps = new();


        public CollisionHandler(float x, float y, float w, float h, int rows, int cols) { spatialHash = new(x, y, w, h, rows, cols); }
        public CollisionHandler(Rect bounds, int rows, int cols) { spatialHash = new(bounds.x, bounds.y, bounds.width, bounds.height, rows, cols); }
        
        
        public void UpdateBounds(Rect newBounds) { spatialHash = spatialHash.Resize(newBounds); }
        //{
        //    int rows = spatialHash.GetRows();
        //    int cols = spatialHash.GetCols();
        //    spatialHash.Close();
        //    spatialHash = new(newBounds.x, newBounds.y, newBounds.width, newBounds.height, rows, cols);
        //}
        
        
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
        }
        public void Close()
        {
            Clear();
            spatialHash.Close();
        }
        
        public virtual void Update(float dt)
        {
            spatialHash.Clear();

            for (int i = collidables.Count - 1; i >= 0; i--)
            {
                var collider = collidables[i];
                if (collider.GetCollider().Enabled)
                {
                    spatialHash.Add(collider);
                }
            }

            for (int i = 0; i < collidables.Count; i++)
            {
                ICollidable collidable = collidables[i];
                var collider = collidable.GetCollider();
                
                if (!collider.Enabled || !collider.ComputeCollision) continue;
                uint[] collisionMask = collidable.GetCollisionMask();

                if (collider.CCD)
                {
                    Segment centerRay = new(collider.GetPrevPos(), collider.Pos);
                    var centerQuery = QuerySpace(centerRay, true, collisionMask);
                    if(centerQuery.Count > 0)
                    {
                        var closest = centerQuery[0].intersection.p;
                        var r = collider.GetShape().GetBoundingCircle().radius;
                        collider.Pos = closest - centerRay.Dir * r;
                    }
                }

                List<ICollidable> collisions = new();
                List<ICollidable> others = spatialHash.GetObjects(collider);
               
                foreach (ICollidable other in others)
                {
                    if(!other.GetCollider().Enabled) continue;

                    uint otherLayer = other.GetCollisionLayer();
                    if (collisionMask.Length > 0)
                    {
                        if (!collisionMask.Contains(otherLayer)) continue;
                    }//else collide with everything


                    //either do bounding box / circle check here or within funcion

                    var info = SGeometry.GetCollisionInfo(collidable, other);

                    if (overlaps.ContainsKey(collidable) && overlaps[collidable].Contains(other))//collision has happend last frame as well
                    {
                        if (info.overlapping)//has overlapped last frame and this frame
                        {
                            overlapInfos.Add(info);
                            collisions.Add(other);
                        }
                        else//has overlapped last frame but not this frame -> overlap ended
                        {
                            //overlap ended
                            overlapEnded.Add((collidable, other));
                        }
                    }
                    else //collision has not happend last frame
                    {
                        if (info.overlapping)//overlapping for the first time => called collision
                        {
                            info.collision = true;
                            info.overlapping = false;
                            overlapInfos.Add(info);
                            collisions.Add(other);
                        }
                        //else no overlap last frame or this frame => nothing happens
                    }

                }


                //update overlaps dictionary for the current collider
                if(collisions.Count > 0)
                {
                    if (overlaps.ContainsKey(collidable))
                    {
                        overlaps[collidable] = collisions;
                    }
                    else
                    {
                        overlaps.Add(collidable, collisions);
                    }
                }
                else
                {
                    if(overlaps.ContainsKey(collidable))
                    {
                        overlaps[collidable].Clear();
                    }
                }

            }
            Resolve();
        }
        protected virtual void Resolve()
        {
            //collidables.AddRange(tempHolding);
            foreach (var collider in tempHolding)
            {
                collidables.Add(collider);
            }
            tempHolding.Clear();

            foreach (var collider in tempRemoving)
            {
                collidables.Remove(collider);
                overlaps.Remove(collider);
            }
            tempRemoving.Clear();


            foreach (CollisionInfo info in overlapInfos)
            {
                if (info.other == null || info.self == null) continue;
                //info.other.Overlap(info);
                info.self.Overlap(info);
            }
            overlapInfos.Clear();

            foreach (var pair in overlapEnded)
            {
                if (pair.collider == null || pair.other == null) continue;
                pair.collider.OverlapEnded(pair.other);
            }
            overlapEnded.Clear();

        }

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
        
        private List<QueryInfo> GetQueryInfo(ICollider caster, bool sorted = false, params uint[] collisionMask)
        {
            List<QueryInfo> infos = new();
            List<ICollidable> objects = spatialHash.GetObjects(caster);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    var intersection = SGeometry.Intersect(caster, obj.GetCollider());
                    if (intersection.valid) infos.Add( new(obj, intersection) );
                }
                else
                {
                    if (collisionMask.Contains(obj.GetCollisionLayer()))
                    {
                        var intersection = SGeometry.Intersect(caster, obj.GetCollider());
                        if (intersection.valid) infos.Add(new(obj, intersection));
                    }
                }
            }

            if (sorted && infos.Count > 1)
            {
                infos.Sort
                (
                    (a, b) =>
                    {
                        Vector2 pos = caster.Pos;
                        float la = (pos - a.intersection.p).LengthSquared();
                        float lb = (pos - b.intersection.p).LengthSquared();

                        if (la > lb) return 1;
                        else if (la == lb) return 0;
                        else return -1;
                    }
                );
            }

            return infos;
        }
        public List<QueryInfo> QuerySpace(ICollidable caster, bool sorted = false)
        {
            return GetQueryInfo(caster.GetCollider(), sorted, caster.GetCollisionMask());
        }
        public List<QueryInfo> QuerySpace(ICollider collider, bool sorted = false, params uint[] collisionMask)
        {
            return GetQueryInfo(collider, sorted, collisionMask);
        }
        public List<QueryInfo> QuerySpace(IShape shape, bool sorted = false, params uint[] collisionMask)
        {
            List<QueryInfo> infos = new();
            List<ICollidable> objects = spatialHash.GetObjects(shape);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    var intersection = SGeometry.Intersect(shape, obj.GetCollider().GetShape());
                    if (intersection.valid) infos.Add(new(obj, intersection));
                }
                else
                {
                    if (collisionMask.Contains(obj.GetCollisionLayer()))
                    {
                        var intersection = SGeometry.Intersect(shape, obj.GetCollider().GetShape());
                        if (intersection.valid) infos.Add(new(obj, intersection));
                    }
                }
            }

            if (sorted && infos.Count > 1)
            {
                infos.Sort
                (
                    (a, b) =>
                    {
                        Vector2 pos = shape.GetCentroid();
                        float la = (pos - a.intersection.p).LengthSquared();
                        float lb = (pos - b.intersection.p).LengthSquared();

                        if (la > lb) return 1;
                        else if (la == lb) return 0;
                        else return -1;
                    }
                );
            }

            return infos;
        }

        private List<ICollidable> GetCastBodies(ICollider caster, bool sorted = false, params uint[] collisionMask)
        {
            List<ICollidable> bodies = new();
            List<ICollidable> objects = spatialHash.GetObjects(caster);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (SGeometry.Overlap(caster, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (collisionMask.Contains(obj.GetCollisionLayer()))
                    {
                        if (SGeometry.Overlap(caster, obj.GetCollider()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }
            }
            if (sorted && bodies.Count > 1)
            {
                bodies.Sort
                (
                    (a, b) =>
                    {
                        Vector2 pos = caster.Pos;
                        float la = (pos - a.GetPosition()).LengthSquared();
                        float lb = (pos - b.GetPosition()).LengthSquared();

                        if (la > lb) return 1;
                        else if (la == lb) return 0;
                        else return -1;
                    }
                );
            }
            return bodies;
        }
        public List<ICollidable> CastSpace(ICollidable caster, bool sorted = false)
        {
            return GetCastBodies(caster.GetCollider(), sorted, caster.GetCollisionMask());
        }
        public List<ICollidable> CastSpace(ICollider collider, bool sorted = false, params uint[] collisionMask)
        {
            return GetCastBodies(collider, sorted, collisionMask);
        }
        public List<ICollidable> CastSpace(IShape castShape, bool sorted = false, params uint[] collisionMask)
        {
            List<ICollidable> bodies = new();
            List<ICollidable> objects = spatialHash.GetObjects(castShape);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (SGeometry.Overlap(castShape, obj.GetCollider().GetShape()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (collisionMask.Contains(obj.GetCollisionLayer()))
                    {
                        if (SGeometry.Overlap(castShape, obj.GetCollider().GetShape()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }
            }
            if (sorted && bodies.Count > 1)
            {
                bodies.Sort
                (
                    (a, b) =>
                    {
                        Vector2 pos = castShape.GetCentroid();
                        float la = (pos - a.GetPosition()).LengthSquared();
                        float lb = (pos - b.GetPosition()).LengthSquared();

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