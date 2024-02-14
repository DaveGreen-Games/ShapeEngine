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
        
        private class OverlapEntry
        {
            public readonly ICollidable Self;
            public readonly ICollidable Other;

            public OverlapEntry(ICollidable self, ICollidable other)
            {
                this.Self = self;
                this.Other = other;
            }
        }

        private readonly List<ICollidable> collidables = new();
        private readonly List<ICollidable> tempHolding = new();
        private readonly List<ICollidable> tempRemoving = new();
        private readonly SpatialHash spatialHash;
        private readonly Dictionary<ICollidable, List<Collision>> collisionStack = new();
        
        
        //use for detecting when overlap has ended
        private HashSet<OverlapEntry> activeRegister = new();
        private HashSet<OverlapEntry> oldRegister = new();

        private readonly HashSet<ICollidable> collisionCandidateCheckRegister = new();
        private List<SpatialHash.Bucket> collisionCandidateBuckets = new();
        
        public int Count => collidables.Count;

        public Rect Bounds => spatialHash.Bounds;

        public CollisionHandler(float x, float y, float w, float h, int rows, int cols) { spatialHash = new(x, y, w, h, rows, cols); }
        public CollisionHandler(Rect bounds, int rows, int cols) { spatialHash = new(bounds.X, bounds.Y, bounds.Width, bounds.Height, rows, cols); }
        
        
        public void ResizeBounds(Rect newBounds) { spatialHash.ResizeBounds(newBounds); }
        
        public void Add(ICollidable collidable)
        {
            tempHolding.Add(collidable);
        }
        public void AddRange(IEnumerable<ICollidable> candidates)
        {
            tempHolding.AddRange(candidates);
        }
        public void AddRange(params ICollidable[] candidates)
        {
            tempHolding.AddRange(candidates);
        }
        public void Remove(ICollidable collidable)
        {
            tempRemoving.Add(collidable);
        }
        public void RemoveRange(IEnumerable<ICollidable> candidates)
        {
            tempRemoving.AddRange(candidates);
        }
        public void RemoveRange(params ICollidable[] candidates)
        {
            tempRemoving.AddRange(candidates);
        }
        
        public void Clear()
        {
            collidables.Clear();
            tempHolding.Clear();
            tempRemoving.Clear();
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

            ProcessCollisions();
            
            Resolve();
        }
        private void ProcessCollisions()
        {
            foreach (var collidable in collidables)
            {
                collisionCandidateBuckets.Clear();
                collisionCandidateCheckRegister.Clear();
                
                spatialHash.GetRegisteredCollisionCandidateBuckets(collidable, ref collisionCandidateBuckets);
                if(collisionCandidateBuckets.Count <= 0) continue;                

                var mask = collidable.GetCollisionMask();
                bool computeIntersections = collidable.GetCollider().ComputeIntersections;
                List<Collision>? cols = null;
                foreach (var bucket in collisionCandidateBuckets)
                {
                    foreach (var candidate in bucket)
                    {
                        if(candidate == collidable) continue;
                        if (!mask.Has(candidate.GetCollisionLayer())) continue;
                        if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                        
                        bool overlap = ShapeGeometry.Overlap(collidable, candidate);
                        if (overlap)
                        {
                            // bool firstContact = !oldRegister.RemoveEntry(collidable, other);
                            // activeRegister.AddEntry(collidable, other);

                            var entry = new OverlapEntry(collidable, candidate);
                            bool firstContact = !oldRegister.Remove(entry);
                            activeRegister.Add(entry);
                            
                            if (computeIntersections)
                            {
                                //CollisionChecksPerFrame++;
                                var collisionPoints = ShapeGeometry.Intersect(collidable, candidate);

                                //shapes overlap but no collision points means collidable is completely inside other
                                //closest point on bounds of other are now used for collision point
                                if (collisionPoints.Count <= 0)
                                {
                                    Vector2 refPoint = collidable.GetCollider().GetPreviousPosition();
                                    var shape = candidate.GetCollider().GetShape();
                                    if (!shape.ContainsPoint(refPoint))
                                    {
                                        CollisionPoint closest = shape.GetClosestCollisionPoint(refPoint);
                                        collisionPoints.Add(closest);
                                        //ClosestPointChecksPerFrame++;
                                    }
                                    //CollisionPoint closest = shape.GetClosestPoint(refPoint);
                                    //collisionPoints.Add(closest);

                                }

                                Collision c = new(collidable, candidate, firstContact, collisionPoints);
                                cols ??= new();
                                cols.Add(c);
                            }
                            else
                            {
                                Collision c = new(collidable, candidate, firstContact);
                                cols ??= new();
                                cols.Add(c);
                            }
                        }
                    }
                }
                // foreach (var other in collisionPartners)
                // {
                //     if(other == collidable) continue;
                //     if (!mask.Has(other.GetCollisionLayer())) continue;
                //     
                //     bool overlap = ShapeGeometry.Overlap(collidable, other);
                //     if (overlap)
                //     {
                //         // bool firstContact = !oldRegister.RemoveEntry(collidable, other);
                //         // activeRegister.AddEntry(collidable, other);
                //
                //         var entry = new OverlapEntry(collidable, other);
                //         bool firstContact = !oldRegister.Remove(entry);
                //         activeRegister.Add(entry);
                //         
                //         if (computeIntersections)
                //         {
                //             //CollisionChecksPerFrame++;
                //             var collisionPoints = ShapeGeometry.Intersect(collidable, other);
                //
                //             //shapes overlap but no collision points means collidable is completely inside other
                //             //closest point on bounds of other are now used for collision point
                //             if (collisionPoints.Count <= 0)
                //             {
                //                 Vector2 refPoint = collidable.GetCollider().GetPreviousPosition();
                //                 var shape = other.GetCollider().GetShape();
                //                 if (!shape.ContainsPoint(refPoint))
                //                 {
                //                     CollisionPoint closest = shape.GetClosestCollisionPoint(refPoint);
                //                     collisionPoints.Add(closest);
                //                     //ClosestPointChecksPerFrame++;
                //                 }
                //                 //CollisionPoint closest = shape.GetClosestPoint(refPoint);
                //                 //collisionPoints.Add(closest);
                //
                //             }
                //
                //             Collision c = new(collidable, other, firstContact, collisionPoints);
                //             cols ??= new();
                //             cols.Add(c);
                //         }
                //         else
                //         {
                //             Collision c = new(collidable, other, firstContact);
                //             cols ??= new();
                //             cols.Add(c);
                //         }
                //     }
                // }

                if (cols is { Count: > 0 })
                {
                    if (!collisionStack.TryAdd(collidable, cols))
                    {
                        collisionStack[collidable].AddRange(cols);
                    }
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

            foreach (var entry in oldRegister)
            {
                entry.Self.OverlapEnded(entry.Other);
            }
            // List<(ICollidable self, ICollidable other)> overlapEndedPairs = oldRegister.GetPairs();
            // foreach (var pair in overlapEndedPairs)
            // {
            //     pair.self.OverlapEnded(pair.other);
            // }

            oldRegister = activeRegister;
            activeRegister = new();
        }
        
        
        public QueryInfos? QuerySpace(ICollidable collidable, Vector2 origin, bool sorted = true)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(collidable, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            var mask = collidable.GetCollisionMask();
            var collider = collidable.GetCollider();
            QueryInfos? infos = null; //new();
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate == collidable) continue;
                    if (!mask.Has(candidate.GetCollisionLayer())) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    var collisionPoints = ShapeGeometry.Intersect(collider, candidate.GetCollider());

                    if (!collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(ICollider collider, Vector2 origin,BitFlag collisionMask, bool sorted = true)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            QueryInfos? infos = null;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    var candidateCollider = candidate.GetCollider();
                    if(collider == candidateCollider) continue;
                    if (!collisionMask.Has(candidate.GetCollisionLayer())) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    var collisionPoints = ShapeGeometry.Intersect(collider, candidateCollider);

                    if (!collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace<T>(T shape, Vector2 origin, BitFlag collisionMask, bool sorted = true) where T : IShape
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            QueryInfos? infos = null;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.GetCollisionLayer())) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    var collisionPoints = ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());

                    if (!collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace<T>(T shape, Vector2 origin, ICollidable[] exceptions,BitFlag collisionMask, bool sorted = true) where T : IShape
        {
                
                collisionCandidateBuckets.Clear();
                collisionCandidateCheckRegister.Clear();
            
                spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
                if (collisionCandidateBuckets.Count <= 0) return null;
                QueryInfos? infos = null;
                foreach (var bucket in collisionCandidateBuckets)
                {
                    foreach (var candidate in bucket)
                    {
                        if (!collisionMask.Has(candidate.GetCollisionLayer())) continue;
                        if (exceptions.Contains(candidate)) continue;
                        if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                        var collisionPoints = ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());

                        if (!collisionPoints.Valid) continue;
                    
                        infos ??= new();
                        infos.Add(new(candidate, origin, collisionPoints));
                    }
                }
                if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
                return infos;
        }

        public void CastSpace(ICollidable collidable, ref List<ICollidable> result, bool sorted = false)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(collidable, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            var mask = collidable.GetCollisionMask();
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate == collidable) continue;
                    if (!mask.Has(candidate.GetCollisionLayer())) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    if (ShapeGeometry.Overlap(collidable.GetCollider(), candidate.GetCollider()))
                    {
                        result.Add(candidate);
                    }
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
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate == collider) continue;
                    if (!collisionMask.Has(candidate.GetCollisionLayer())) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    if (ShapeGeometry.Overlap(collider, candidate.GetCollider()))
                    {
                        result.Add(candidate);
                    }
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
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(castShape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.GetCollisionLayer())) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (ShapeGeometry.Overlap(castShape, candidate.GetCollider().GetShape()))
                    {
                        result.Add(candidate);
                    }
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
        
