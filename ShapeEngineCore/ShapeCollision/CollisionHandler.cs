using ShapeLib;
using System.Numerics;
using Raylib_CsLo;
using System.Globalization;

namespace ShapeCollision
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

    //colliders have bounding sphere
    //check if bounding spheres would overlap and if the colliders relative velocity lets them move towards each other
    //only if both a true, check for final overlap
    public class CollisionHandler
    {
        protected List<ICollidable> collidables = new();
        protected List<ICollidable> tempHolding = new();
        protected List<ICollidable> tempRemoving = new();
        protected SpatialHash spatialHash;

        protected List<CollisionInfo> overlapInfos = new();
        protected List<(ICollidable collider, ICollidable other)> overlapEnded= new();

        protected Dictionary<ICollidable, List<ICollidable>> overlaps = new();


        public CollisionHandler(float x, float y, float w, float h, int rows, int cols)
        {
            spatialHash = new(x, y, w, h, rows, cols);
        }
        public void UpdateArea(Rectangle newArea)
        {
            int rows = spatialHash.GetRows();
            int cols = spatialHash.GetCols();
            spatialHash.Close();
            spatialHash = new(newArea.x, newArea.y, newArea.width, newArea.height, rows, cols);
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
                if (collider.GetCollider().IsEnabled())
                {
                    spatialHash.Add(collider);
                }
            }

            for (int i = 0; i < collidables.Count; i++)
            {
                ICollidable collider = collidables[i];
                if (!collider.GetCollider().IsEnabled() || !collider.GetCollider().CheckCollision) continue;
                string[] collisionMask = collider.GetCollisionMask();

                List<ICollidable> collisions = new();
                List<ICollidable> others = spatialHash.GetObjects(collider);
                foreach (ICollidable other in others)
                {
                    string otherLayer = other.GetCollisionLayer();
                    if (collisionMask.Length > 0)
                    {
                        if (!collisionMask.Contains(otherLayer)) continue;
                    }//else collide with everything

                    var selfC = collider.GetCollider();
                    var info = SGeometry.GetCollisionInfo(collider, other);

                    if (overlaps.ContainsKey(collider) && overlaps[collider].Contains(other))//collision has happend last frame as well
                    {
                        if (info.overlapping)//has overlapped last frame and this frame
                        {
                            overlapInfos.Add(info);
                            collisions.Add(other);
                        }
                        else//has overlapped last frame but not this frame -> overlap ended
                        {
                            //overlap ended
                            overlapEnded.Add((collider, other));
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
                    if (overlaps.ContainsKey(collider))
                    {
                        overlaps[collider] = collisions;
                    }
                    else
                    {
                        overlaps.Add(collider, collisions);
                    }
                }
                else
                {
                    if(overlaps.ContainsKey(collider))
                    {
                        overlaps[collider].Clear();
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
        
        private List<QueryInfo> GetQueryInfo(Collider caster, bool sorted = false, params string[] collisionMask)
        {
            List<QueryInfo> infos = new();
            List<ICollidable> objects = spatialHash.GetObjects(caster);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    var intersection = SGeometry.Intersection(caster, obj.GetCollider());
                    if (intersection.valid) infos.Add( new(obj, intersection) );

                    //if (SGeometry.Overlap(caster, obj.GetCollider()))
                    //{
                    //    QueryInfo q = new(obj, SGeometry.Intersection(caster, obj.GetCollider()));
                    //    infos.Add(q);
                    //}
                }
                else
                {
                    var intersection = SGeometry.Intersection(caster, obj.GetCollider());
                    if (intersection.valid) infos.Add(new(obj, intersection));


                    //if (SGeometry.Overlap(caster, obj.GetCollider()))
                    //{
                    //    if (collisionMask.Contains(obj.GetCollisionLayer()))
                    //    {
                    //        QueryInfo q = new(obj, SGeometry.Intersection(caster, obj.GetCollider()));
                    //        infos.Add(q);
                    //    }
                    //}
                }
            }

            if (sorted && infos.Count > 1)
            {
                infos.Sort
                (
                    (a, b) =>
                    {
                        //if (!a.intersection.valid) return 1;
                        //if (!b.intersection.valid) return -1;
                        Vector2 pos = caster.Pos;
                        //float la = (pos - a.collidable.GetPos()).LengthSquared();
                        //float lb = (pos - b.collidable.GetPos()).LengthSquared();
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
        public List<QueryInfo> QuerySpace(Collider collider, bool sorted = false, params string[] collisionMask)
        {
            return GetQueryInfo(collider, sorted, collisionMask);
        }
        public List<QueryInfo> QuerySpace(Rectangle rect, bool sorted = false, params string[] collisionMask)
        {
            RectCollider collider = new(rect);
            return GetQueryInfo(collider, sorted, collisionMask);
        }
        public List<QueryInfo> QuerySpace(Vector2 pos, float r, bool sorted = false, params string[] collisionMask)
        {
            CircleCollider collider = new(pos, r);
            return GetQueryInfo(collider, sorted, collisionMask);
        }
        public List<QueryInfo> QuerySpace(Vector2 pos, Vector2 size, Vector2 alignement, bool sorted = false, params string[] collisionMask)
        {
            RectCollider collider = new(pos, size, alignement);
            return GetQueryInfo(collider, sorted, collisionMask);
        }
        public List<QueryInfo> QuerySpace(Vector2 pos, Vector2 dir, float length, bool sorted = false, params string[] collisionMask)
        {
            SegmentCollider collider = new(pos, dir, length);
            return GetQueryInfo(collider, sorted, collisionMask);
        }
        public List<QueryInfo> QuerySpace(Vector2 start, Vector2 end, bool sorted = false, params string[] collisionMask)
        {
            SegmentCollider collider = new(start, end);
            return GetQueryInfo(collider, sorted, collisionMask);
        }


        private List<ICollidable> GetCastBodies(Collider caster, bool sorted = false, params string[] collisionMask)
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
                    if (SGeometry.Overlap(caster, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
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
                        float la = (pos - a.GetPos()).LengthSquared();
                        float lb = (pos - b.GetPos()).LengthSquared();

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
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(caster);
            //foreach (ICollidable obj in objects)
            //{
            //    if (caster.GetCollisionMask().Length <= 0)
            //    {
            //        if (SGeometry.Overlap(caster.GetCollider(), obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(caster.GetCollider(), obj.GetCollider()))
            //        {
            //            if (caster.GetCollisionMask().Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //}
            //return bodies;
        }
        public List<ICollidable> CastSpace(Collider collider, bool sorted = false, params string[] collisionMask)
        {
            return GetCastBodies(collider, sorted, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //}
            //return bodies;
        }
        public List<ICollidable> CastSpace(Rectangle rect, bool sorted = false, params string[] collisionMask)
        {
            RectCollider collider = new(rect);
            return GetCastBodies(collider, sorted, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 pos, float r, bool sorted = false, params string[] collisionMask)
        {
            CircleCollider collider = new(pos, r);
            return GetCastBodies(collider, sorted, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 pos, Vector2 size, Vector2 alignement, bool sorted = false, params string[] collisionMask)
        {
            RectCollider collider = new(pos, size, alignement);
            return GetCastBodies(collider, sorted, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 pos, Vector2 dir, float length, bool sorted = false, params string[] collisionMask)
        {
            SegmentCollider collider = new(pos, dir, length);
            return GetCastBodies(collider, sorted, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 start, Vector2 end, bool sorted = false, params string[] collisionMask)
        {
            SegmentCollider collider = new(start, end);
            return GetCastBodies(collider, sorted, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }


        public void DebugDrawGrid(Color border, Color fill)
        {
            spatialHash.DebugDrawGrid(border, fill);
        }
    }


}
