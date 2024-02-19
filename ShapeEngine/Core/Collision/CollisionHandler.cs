using ShapeEngine.Lib;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Collision
{
    
    public class CollisionHandler : IBounds
    {
        private class OverlapRegister
        {
            private HashSet<OverlapEntry> entries = new();

            public OverlapEntry? FindEntry(Collider self, Collider other)
            {
                foreach (var entry in entries)
                {
                    if (self == entry.Self && other == entry.Other)
                    {
                        return entry;
                    }
                }

                return null;
            }

            public bool AddEntry(OverlapEntry entry) => entries.Add(entry);

            public bool RemoveEntry(OverlapEntry entry) => entries.Remove(entry);

            public void ProcessEntries()
            {
                foreach (var entry in entries)
                {
                    entry.Self.ResolveCollisionEnded(entry.Other);
                }
            }

            public void Clear() => entries.Clear();
            public void Swap(OverlapRegister other) => (entries, other.entries) = (other.entries, entries);
        }
        private class OverlapEntry
        {
            public readonly Collider Self;
            public readonly Collider Other;

            public OverlapEntry(Collider self, Collider other)
            {
                this.Self = self;
                this.Other = other;
            }
        }

        private readonly List<CollisionBody> collisionBodies = new();
        private readonly List<CollisionBody> tempHolding = new();
        private readonly List<CollisionBody> tempRemoving = new();
        private readonly SpatialHash spatialHash;
        private readonly Dictionary<Collider, List<Collision>> collisionStack = new();
        
        //use for detecting when overlap has ended
        private readonly OverlapRegister activeRegister = new();
        private readonly OverlapRegister oldRegister = new();

        private readonly HashSet<Collider> collisionCandidateCheckRegister = new();
        private List<SpatialHash.Bucket> collisionCandidateBuckets = new();
        
        public int Count => collisionBodies.Count;

        public Rect Bounds => spatialHash.Bounds;

        public CollisionHandler(float x, float y, float w, float h, int rows, int cols) { spatialHash = new(x, y, w, h, rows, cols); }
        public CollisionHandler(Rect bounds, int rows, int cols) { spatialHash = new(bounds.X, bounds.Y, bounds.Width, bounds.Height, rows, cols); }
        
        
        public void ResizeBounds(Rect newBounds) { spatialHash.ResizeBounds(newBounds); }
        
        public void Add(CollisionBody collidable)
        {
            tempHolding.Add(collidable);
        }
        public void AddRange(IEnumerable<CollisionBody> candidates)
        {
            tempHolding.AddRange(candidates);
        }
        public void AddRange(params CollisionBody[] candidates)
        {
            tempHolding.AddRange(candidates);
        }
        public void Remove(CollisionBody collidable)
        {
            tempRemoving.Add(collidable);
        }
        public void RemoveRange(IEnumerable<CollisionBody> candidates)
        {
            tempRemoving.AddRange(candidates);
        }
        public void RemoveRange(params CollisionBody[] candidates)
        {
            tempRemoving.AddRange(candidates);
        }
        
        public void Clear()
        {
            collisionBodies.Clear();
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
            spatialHash.Fill(collisionBodies);

            ProcessCollisions();
            
            Resolve();
        }
        private void ProcessCollisions()
        {
            foreach (var collisionBody in collisionBodies)
            {
                if (!collisionBody.Enabled || !collisionBody.HasColliders) continue;

                foreach (var collider in collisionBody.Colliders)
                {
                    if (!collider.Enabled) continue;
                    
                    collisionCandidateBuckets.Clear();
                    collisionCandidateCheckRegister.Clear();
                    spatialHash.GetRegisteredCollisionCandidateBuckets(collider, ref collisionCandidateBuckets);
                    
                    if(collisionCandidateBuckets.Count <= 0) continue;     
                    
                    var mask = collider.CollisionMask;
                    bool computeIntersections = collider.ComputeIntersections;
                    List<Collision>? cols = null;
                    
                    foreach (var bucket in collisionCandidateBuckets)
                    {
                        foreach (var candidate in bucket)
                        {
                            if(candidate == collider) continue;
                            if (candidate.Parent == collider.Parent) continue;
                            if (!mask.Has(candidate.CollisionLayer)) continue;
                            if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                            bool overlap = collider.Overlap(candidate); // ShapeGeometry.Overlap(collider, candidate);
                            if (overlap)
                            {
                                var oldEntry = oldRegister.FindEntry(collider, candidate);
                                bool firstContact;
                                if (oldEntry != null)
                                {
                                    firstContact = false;
                                    oldRegister.RemoveEntry(oldEntry);
                                    activeRegister.AddEntry(oldEntry);

                                }
                                else
                                {
                                    firstContact = true;
                                    activeRegister.AddEntry(new OverlapEntry(collider, candidate));
                                }
                                
                                if (computeIntersections)
                                {
                                    //CollisionChecksPerFrame++;
                                    var collisionPoints = collider.Intersect(candidate); // ShapeGeometry.Intersect(collider, candidate);

                                    //shapes overlap but no collision points means collidable is completely inside other
                                    //closest point on bounds of other are now used for collision point
                                    if (collisionPoints == null || collisionPoints.Count <= 0)
                                    {
                                        var refPoint = collider.PrevTransform.Position;// PrevPosition;
                                        if (!candidate.ContainsPoint(refPoint))
                                        {
                                            var closest = candidate.GetClosestCollisionPoint(refPoint);
                                            collisionPoints ??= new();
                                            collisionPoints.Add(closest);
                                        }
                                        //CollisionPoint closest = shape.GetClosestPoint(refPoint);
                                        //collisionPoints.Add(closest);

                                    }

                                    Collision c = new(collider, candidate, firstContact, collisionPoints);
                                    cols ??= new();
                                    cols.Add(c);
                                }
                                else
                                {
                                    Collision c = new(collider, candidate, firstContact);
                                    cols ??= new();
                                    cols.Add(c);
                                }
                            }
                        }
                    }

                    if (cols is { Count: > 0 })
                    {
                        if (!collisionStack.TryAdd(collider, cols))
                        {
                            collisionStack[collider].AddRange(cols);
                        }
                    }
                }
            }
        }
        private void Resolve()
        {
            foreach (var collidable in tempHolding)
            {
                collisionBodies.Add(collidable);
            }
            tempHolding.Clear();

            foreach (var collidable in tempRemoving)
            {
                collisionBodies.Remove(collidable);
            }
            tempRemoving.Clear();


            foreach (var kvp in collisionStack)
            {
                var collider = kvp.Key;
                var cols = kvp.Value;
                if(cols.Count > 0)
                {
                    CollisionInformation collisionInfo = new(cols, collider.ComputeIntersections);
                    collider.ResolveCollision(collisionInfo);
                }
            }
            collisionStack.Clear();

            oldRegister.ProcessEntries();
            oldRegister.Swap(activeRegister);
            activeRegister.Clear();
        }
        
        
        // public QueryInfos? QuerySpace(CollisionBody collisionBody, Vector2 origin, bool sorted = true)
        // {
        //     if (!collisionBody.HasColliders) return null;
        //     QueryInfos? infos = null;
        //     
        //     foreach (var collider in collisionBody.Colliders)
        //     {
        //         collisionCandidateBuckets.Clear();
        //         collisionCandidateCheckRegister.Clear();
        //         spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
        //         if (collisionCandidateBuckets.Count <= 0) return null;
        //         
        //         var mask = collider.CollisionMask;
        //         foreach (var bucket in collisionCandidateBuckets)
        //         {
        //             foreach (var candidate in bucket)
        //             {
        //                 if (candidate == collider) continue;
        //                 if (!mask.Has(candidate.CollisionLayer)) continue;
        //                 if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        //
        //                 var collisionPoints = collider.Intersect(candidate);
        //         
        //                 if (collisionPoints == null || !collisionPoints.Valid) continue;
        //                 
        //                 infos ??= new();
        //                 infos.Add(new(collider, candidate, origin, collisionPoints));
        //             }
        //         }
        //     }
        //     
        //     if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
        //     return infos;
        // }
        //
        public QueryInfos? QuerySpace(Collider collider, Vector2 origin, bool sorted = true)
        {
            QueryInfos? infos = null;
            
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            
            var mask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate == collider) continue;
                    if (!mask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    var collisionPoints = collider.Intersect(candidate);
            
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(Segment shape, Vector2 origin, BitFlag collisionMask, bool sorted = true)
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
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        
                    var collisionPoints = shape.Intersect(candidate); //  ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(Triangle shape, Vector2 origin, BitFlag collisionMask, bool sorted = true)
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
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        
                    var collisionPoints = shape.Intersect(candidate); //  ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(Circle shape, Vector2 origin, BitFlag collisionMask, bool sorted = true)
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
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        
                    var collisionPoints = shape.Intersect(candidate); //  ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(Rect shape, Vector2 origin, BitFlag collisionMask, bool sorted = true)
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
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        
                    var collisionPoints = shape.Intersect(candidate); //  ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(Polygon shape, Vector2 origin, BitFlag collisionMask, bool sorted = true)
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
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        
                    var collisionPoints = shape.Intersect(candidate); //  ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(Polyline shape, Vector2 origin, BitFlag collisionMask, bool sorted = true)
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
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        
                    var collisionPoints = shape.Intersect(candidate); //  ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }

        
        public void CastSpace(CollisionBody collisionBody, ref List<Collider> result, bool sorted = false)
        {
            if (!collisionBody.HasColliders) return;

            foreach (var collider in collisionBody.Colliders)
            {
                collisionCandidateBuckets.Clear();
                collisionCandidateCheckRegister.Clear();
                spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
                if (collisionCandidateBuckets.Count <= 0) return;

                var mask = collider.CollisionMask;
                foreach (var bucket in collisionCandidateBuckets)
                {
                    foreach (var candidate in bucket)
                    {
                        if (candidate == collider) continue;
                        if (!mask.Has(candidate.CollisionLayer)) continue;
                        if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                        if (collider.Overlap(candidate))
                        {
                            result.Add(candidate);
                        }
                    }
                }
            }
            if (sorted && result.Count > 1)
            {
                var origin = collisionBody.Transform.Position; // collidable.GetCollider().Pos;
                result.Sort
                (
                    (a, b) =>
                    {
                        float la = (origin - a.CurTransform.Position).LengthSquared();
                        float lb = (origin - b.CurTransform.Position).LengthSquared();

                        if (la > lb) return 1;
                        if (ShapeMath.EqualsF(la, lb)) return 0;
                        return -1;
                    }
                );
            }
        }
        public void CastSpace(Collider collider,  ref List<Collider> result, bool sorted = false)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            var mask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate == collider) continue;
                    if (!mask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    if (collider.Overlap(candidate))
                    {
                        result.Add(candidate);
                    }
                }
            }
            if (sorted && result.Count > 1)
            {
                var origin = collider.CurTransform.Position;
                result.Sort
                (
                    (a, b) =>
                    {
                        float la = (origin - a.CurTransform.Position).LengthSquared();
                        float lb = (origin - b.CurTransform.Position).LengthSquared();

                        if (la > lb) return 1;
                        if (ShapeMath.EqualsF(la, lb)) return 0;
                        return -1;
                    }
                );
            }
        }
        public void CastSpace(Segment shape, BitFlag collisionMask, ref List<Collider> result)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
        public void CastSpace(Triangle shape, BitFlag collisionMask, ref List<Collider> result)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
        public void CastSpace(Circle shape, BitFlag collisionMask, ref List<Collider> result)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
        public void CastSpace(Rect shape, BitFlag collisionMask, ref List<Collider> result)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
        public void CastSpace(Polygon shape, BitFlag collisionMask, ref List<Collider> result)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
        public void CastSpace(Polyline shape, BitFlag collisionMask, ref List<Collider> result)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
        public void SortCastResult(ref List<Collider> result, Vector2 sortOrigin)
        {
            if (result.Count > 1)
            {
                result.Sort
                (
                    (a, b) =>
                    {
                        float la = (sortOrigin - a.CurTransform.Position).LengthSquared();
                        float lb = (sortOrigin - b.CurTransform.Position).LengthSquared();

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
        
