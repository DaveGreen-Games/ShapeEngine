using ShapeEngine.Lib;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Collision
{
 
    public class CollisionHandler : IBounds
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

        /*
        /// <summary>
        /// Bucket First algorithm iterates over all buckets and checks collision within each indiviual bucket.
        /// Does not scale well with increase bucket count. If you use large areas with a lot of rows/columns this algorithm might perform worse!
        /// If false the default algorithm is used. The default algorithm iterates over all collidables and checks for other collidables that are 
        /// in the surounding buckets. 
        /// </summary>
        public bool BucketFirstAlgorithm = false;
        */

        private List<ICollidable> collidables = new();
        private List<ICollidable> tempHolding = new();
        private List<ICollidable> tempRemoving = new();
        private SpatialHash spatialHash;
        private Dictionary<ICollidable, List<Collision>> collisionStack = new();
        private OverlapRegister activeRegister = new();
        private OverlapRegister oldRegister = new();

        private HashSet<ICollidable> collisionPartners = new();

        public int Count { get { return collidables.Count; } }

        public Rect Bounds { get { return spatialHash.Bounds; } }

        public CollisionHandler(float x, float y, float w, float h, int rows, int cols) { spatialHash = new(x, y, w, h, rows, cols); }
        public CollisionHandler(Rect bounds, int rows, int cols) { spatialHash = new(bounds.X, bounds.Y, bounds.Width, bounds.Height, rows, cols); }
        
        
        public void ResizeBounds(Rect newBounds) { spatialHash.ResizeBounds(newBounds); }
        
        public void Add(ICollidable collidable)
        {
            tempHolding.Add(collidable);
        }
        public void AddRange(IEnumerable<ICollidable> collidables)
        {
            tempHolding.AddRange(collidables);
        }
        public void AddRange(params ICollidable[] collidables)
        {
            tempHolding.AddRange(collidables);
        }
        public void Remove(ICollidable collidable)
        {
            tempRemoving.Add(collidable);
        }
        public void RemoveRange(IEnumerable<ICollidable> collidables)
        {
            tempRemoving.AddRange(collidables);
        }
        public void RemoveRange(params ICollidable[] collidables)
        {
            tempRemoving.AddRange(collidables);
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

        public void Update()
        {
            spatialHash.Fill(collidables);
            // FillSpatialHash();//memory problem (38%)

            ProcessCollisions(); //memory problem (55%)
            
            Resolve();
        }
        

        // private void FillSpatialHash()
        // {
        //     spatialHash.Fill(collidables);
        //     // spatialHash.Clear();
        //     //
        //     // //fill spatial hash and filter out all disabled colliders
        //     // for (int i = collidables.Count - 1; i >= 0; i--)
        //     // {
        //     //     var collidable = collidables[i];
        //     //     var collider = collidable.GetCollider();
        //     //     if (collider.Enabled)
        //     //     {
        //     //         spatialHash.Add(collidable); //add is the problem 
        //     //     }
        //     //     //IterationsPerFrame++;
        //     // }
        //     //
        //     //
        // }
        private void ProcessCollisions()
        {
            foreach (var collidable in collidables)
            {
                collisionPartners.Clear();
                
                spatialHash.GetRegisteredObjects(collidable, ref collisionPartners);
                // if(others == null || others.Count <= 0) continue; //memory problem 51% of 55%
                if(collisionPartners.Count <= 0) continue;

                var mask = collidable.GetCollisionMask();
                bool computeIntersections = collidable.GetCollider().ComputeIntersections;
                List<Collision>? cols = null;
                foreach (var other in collisionPartners)
                {
                    if(other == collidable) continue;
                    if (!mask.Has(other.GetCollisionLayer())) continue;
                    
                    bool overlap = ShapeGeometry.Overlap(collidable, other);
                    if (overlap)
                    {
                        bool firstContact = !oldRegister.RemoveEntry(collidable, other);
                        activeRegister.AddEntry(collidable, other);
                        if (computeIntersections)
                        {
                            //CollisionChecksPerFrame++;
                            var collisionPoints = ShapeGeometry.Intersect(collidable, other);

                            //shapes overlap but no collision points means collidable is completely inside other
                            //closest point on bounds of other are now used for collision point
                            if (collisionPoints.Count <= 0)
                            {
                                Vector2 refPoint = collidable.GetCollider().GetPreviousPosition();
                                var shape = other.GetCollider().GetShape();
                                if (!shape.ContainsPoint(refPoint))
                                {
                                    CollisionPoint closest = shape.GetClosestCollisionPoint(refPoint);
                                    collisionPoints.Add(closest);
                                    //ClosestPointChecksPerFrame++;
                                }
                                //CollisionPoint closest = shape.GetClosestPoint(refPoint);
                                //collisionPoints.Add(closest);

                            }

                            Collision c = new(collidable, other, firstContact, collisionPoints);
                            cols ??= new();
                            cols.Add(c);
                        }
                        else
                        {
                            Collision c = new(collidable, other, firstContact);
                            cols ??= new();
                            cols.Add(c);
                        }
                    }
                }

                if (cols != null && cols.Count > 0)
                {
                    if (collisionStack.ContainsKey(collidable))
                    {
                        collisionStack[collidable].AddRange(cols);
                    }
                    else collisionStack.Add(collidable, cols);
                }
            }
        }
        private void Resolve()
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
        
        
        /*
        private void ProcessCollisionsBucketFirst()
        {
            int bucketCount = spatialHash.GetBucketCount();
            for (int i = 0; i < bucketCount; i++)
            {
                var bucketInfo = spatialHash.GetBucketInfo(i);
                if (!bucketInfo.Valid) continue;

                foreach (var collidable in bucketInfo.Active)
                {
                    var others = bucketInfo.GetOthers(collidable);

                    bool computeIntersections = collidable.GetCollider().ComputeIntersections;
                    List<Collision<TCollidable>> cols = new();
                    foreach (var other in others)
                    {
                        //IterationsPerFrame++;
                        if (activeRegister.HasEntry(collidable, other)) continue;

                        bool overlap = SGeometry.Overlap(collidable, other);
                        if (overlap)
                        {
                            bool firstContact = !oldRegister.RemoveEntry(collidable, other);
                            activeRegister.AddEntry(collidable, other);
                            if (computeIntersections)
                            {
                                //CollisionChecksPerFrame++;
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
                                        //ClosestPointChecksPerFrame++;
                                    }

                                }

                                Collision<TCollidable> c = new(collidable, other, firstContact, collisionPoints);
                                cols.Add(c);
                            }
                            else
                            {
                                Collision<TCollidable> c = new(collidable, other, firstContact);
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
        */
        
        
        public QueryInfos QuerySpace(ICollidable collidable, Vector2 origin, bool sorted = true)
        {
            // return QuerySpace(collidable.GetCollider(), origin,collidable.GetCollisionMask(), sorted);
            QueryInfos infos = new();
            var colPartners = new HashSet<ICollidable>();
            spatialHash.GetObjects(collidable, ref colPartners);
            if (colPartners.Count <= 0) return infos;

            var mask = collidable.GetCollisionMask();
            var collider = collidable.GetCollider();
            foreach (var obj in colPartners)
            {
                // if (obj == collidable) continue;
                if(!mask.Has(obj.GetCollisionLayer())) continue;
                var collisionPoints = ShapeGeometry.Intersect(collider, obj.GetCollider());
                
                if (collisionPoints.Valid) infos.Add(new(obj, origin, collisionPoints));
            }
            if(sorted) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos QuerySpace(ICollider collider, Vector2 origin,BitFlag collisionMask, bool sorted = true)
        {
            QueryInfos infos = new();
            var colPartners = new HashSet<ICollidable>();
            spatialHash.GetObjects(collider, collisionMask, ref colPartners);
            if (colPartners.Count <= 0) return infos;
            // var objects = spatialHash.GetObjectsFiltered(collider, collisionMask);
            foreach (var obj in colPartners)
            {
                // if (obj.GetCollider() == collider) continue;
                if(!collisionMask.Has(obj.GetCollisionLayer())) continue;
                var collisionPoints = ShapeGeometry.Intersect(collider, obj.GetCollider());
                
                if (collisionPoints.Valid) infos.Add(new(obj, origin, collisionPoints));
            }
            if(sorted) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos QuerySpace<T>(T shape, Vector2 origin, BitFlag collisionMask, bool sorted = true) where T : IShape
        {
            QueryInfos infos = new();
            // var objects = spatialHash.GetObjectsFiltered(shape, collisionMask);
            var colPartners = new HashSet<ICollidable>();
            spatialHash.GetObjects(shape, collisionMask, ref colPartners);
            if (colPartners.Count <= 0) return infos;
            foreach (var obj in colPartners)
            {
                if(!collisionMask.Has(obj.GetCollisionLayer())) continue;
                var collisionPoints = ShapeGeometry.Intersect(shape, obj.GetCollider().GetShape());
                if (collisionPoints.Valid) infos.Add(new(obj, origin, collisionPoints));
            }
            if(sorted) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos QuerySpace<T>(T shape, Vector2 origin, ICollidable[] exceptions,BitFlag collisionMask, bool sorted = true) where T : IShape
        {
            QueryInfos infos = new();
            // var objects = spatialHash.GetObjectsFiltered(shape, collisionMask);
            var colPartners = new HashSet<ICollidable>();
            spatialHash.GetObjects(shape, collisionMask, ref colPartners);
            if (colPartners.Count <= 0) return infos;
            foreach (var obj in colPartners)
            {
                if(!collisionMask.Has(obj.GetCollisionLayer())) continue;
                if(exceptions.Contains(obj)) continue;

                var collisionPoints = ShapeGeometry.Intersect(shape, obj.GetCollider().GetShape());
                if (collisionPoints.Valid) infos.Add(new(obj, origin, collisionPoints));
            }
            if (sorted) infos.SortClosest(origin);
            return infos;
        }
        

        public void CastSpace(ICollidable collidable, ref List<ICollidable> result, bool sorted = false)
        {
            // HashSet<ICollidable> bodies = new();
            collisionPartners.Clear();
            spatialHash.GetObjects(collidable, ref collisionPartners);
            if (collisionPartners.Count <= 0) return;
            var mask = collidable.GetCollisionMask();
            foreach (var obj in collisionPartners)
            {
                // if(obj == collidable) continue;
                if (!mask.Has(obj.GetCollisionLayer())) continue;

                if (ShapeGeometry.Overlap(collidable.GetCollider(), obj.GetCollider()))
                {
                    result.Add(obj);
                }
            }
            if (sorted && result.Count > 1)
            {
                var origin = collidable.GetCollider().Pos;
                result.Sort
                (
                    (a, b) =>
                    {
                        float la = (origin - a.GetCollider().Pos).LengthSquared();
                        float lb = (origin - b.GetCollider().Pos).LengthSquared();

                        if (la > lb) return 1;
                        if (ShapeMath.EqualsF(la, lb)) return 0;
                        return -1;
                    }
                );
            }
        }
        public void CastSpace(ICollider collider, BitFlag collisionMask,  ref List<ICollidable> result, bool sorted = false)
        {
            collisionPartners.Clear();
            spatialHash.GetObjects(collider, collisionMask, ref collisionPartners);
            if (collisionPartners.Count <= 0) return;
            
            foreach (var obj in collisionPartners)
            {
                // if(obj.GetCollider() == collider) continue;
                if(!collisionMask.Has(obj.GetCollisionLayer())) continue;

                if (ShapeGeometry.Overlap(collider, obj.GetCollider()))
                {
                    result.Add(obj);
                }
            }
            if (sorted && result.Count > 1)
            {
                var origin = collider.Pos;
                result.Sort
                (
                    (a, b) =>
                    {
                        float la = (origin - a.GetCollider().Pos).LengthSquared();
                        float lb = (origin - b.GetCollider().Pos).LengthSquared();

                        if (la > lb) return 1;
                        if (ShapeMath.EqualsF(la, lb)) return 0;
                        return -1;
                    }
                );
            }
        }
        public void CastSpace<T>(T castShape,BitFlag collisionMask, ref List<ICollidable> result, bool sorted = false) where T : IShape
        {
            collisionPartners.Clear();
            spatialHash.GetObjectsFiltered(castShape, collisionMask, ref collisionPartners);
            if (collisionPartners.Count <= 0) return;
            
            foreach (var obj in collisionPartners)
            {
                if(!collisionMask.Has(obj.GetCollisionLayer())) continue;
                
                if (ShapeGeometry.Overlap(castShape, obj.GetCollider().GetShape()))
                {
                    result.Add(obj);
                }
            }
            if (sorted && result.Count > 1)
            {
                var origin = castShape.GetCentroid();
                result.Sort
                (
                    (a, b) =>
                    {
                        float la = (origin - a.GetCollider().Pos).LengthSquared();
                        float lb = (origin - b.GetCollider().Pos).LengthSquared();

                        if (la > lb) return 1;
                        if (ShapeMath.EqualsF(la, lb)) return 0;
                        return -1;
                    }
                );
            }
        }
        
        public void DebugDraw(ColorRgba border, ColorRgba fill)
        {
            spatialHash.DebugDraw(border, fill);
        }

        
    }

}


