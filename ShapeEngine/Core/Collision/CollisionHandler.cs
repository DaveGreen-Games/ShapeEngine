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
        private class ObjectRegister<T>
        {
            public readonly List<T> AllObjects = new();
            private readonly List<T> tempHolding = new();
            private readonly List<T> tempRemoving = new();
            
            public void Add(T obj) => tempHolding.Add(obj);

            public void AddRange(IEnumerable<T> objs) => tempHolding.AddRange(objs);

            public void AddRange(params T[] objs) => tempHolding.AddRange(objs);

            public void Remove(T obj) => tempRemoving.Add(obj);

            public void RemoveRange(IEnumerable<T> objs) => tempRemoving.AddRange(objs);

            public void RemoveRange(params T[] objs) => tempRemoving.AddRange(objs);

            public void Process()
            {
                foreach (var obj in tempHolding)
                {
                    AllObjects.Add(obj);
                }
                tempHolding.Clear();

                foreach (var obj in tempRemoving)
                {
                    AllObjects.Remove(obj);
                }
                tempRemoving.Clear();
            }
            public void Clear()
            {
                AllObjects.Clear();
                tempHolding.Clear();
                tempRemoving.Clear();
            }
        }
        
        // private readonly List<CollisionObject> collisionBodies = new();
        // private readonly List<CollisionObject> tempHolding = new();
        // private readonly List<CollisionObject> tempRemoving = new();

        private readonly ObjectRegister<CollisionObject> collisionBodyRegister = new();
        private readonly ObjectRegister<Collider> colliderRegister = new();
        
        private readonly SpatialHash spatialHash;
        private readonly Dictionary<Collider, List<Collision>> collisionStack = new();
        
        //use for detecting when overlap has ended
        private readonly OverlapRegister activeRegister = new();
        private readonly OverlapRegister oldRegister = new();

        private readonly HashSet<Collider> collisionCandidateCheckRegister = new();
        private List<SpatialHash.Bucket> collisionCandidateBuckets = new();

        public int Count => collisionBodyRegister.AllObjects.Count; // collisionBodies.Count;

        public Rect Bounds => spatialHash.Bounds;

        public CollisionHandler(float x, float y, float w, float h, int rows, int cols) { spatialHash = new(x, y, w, h, rows, cols); }
        public CollisionHandler(Rect bounds, int rows, int cols) { spatialHash = new(bounds.X, bounds.Y, bounds.Width, bounds.Height, rows, cols); }
        
        
        public void ResizeBounds(Rect newBounds) { spatialHash.ResizeBounds(newBounds); }

        public void Add(CollisionObject collisionObject) => collisionBodyRegister.Add(collisionObject);
        public void AddRange(IEnumerable<CollisionObject> collisionObjects) => collisionBodyRegister.AddRange(collisionObjects);
        public void AddRange(params CollisionObject[] collisionObjects)=> collisionBodyRegister.AddRange(collisionObjects);
        public void Remove(CollisionObject collisionObject)=> collisionBodyRegister.Remove(collisionObject);
        public void RemoveRange(IEnumerable<CollisionObject> collisionObjects)  => collisionBodyRegister.RemoveRange(collisionObjects);
        public void RemoveRange(params CollisionObject[] collisionObjects)  => collisionBodyRegister.RemoveRange(collisionObjects);
        
        public void Add(Collider collider) => colliderRegister.Add(collider);
        public void AddRange(IEnumerable<Collider> colliders) => colliderRegister.AddRange(colliders);
        public void AddRange(params Collider[] colliders)=> colliderRegister.AddRange(colliders);
        public void Remove(Collider collider)=> colliderRegister.Remove(collider);
        public void RemoveRange(IEnumerable<Collider> colliders)  => colliderRegister.RemoveRange(colliders);
        public void RemoveRange(params Collider[] colliders)  => colliderRegister.RemoveRange(colliders);
        
        public void Clear()
        {
            collisionBodyRegister.Clear();
            colliderRegister.Clear();
            collisionStack.Clear();
        }
        public void Close()
        {
            Clear();
            spatialHash.Close();
            
        }

        public void Update()
        {
            spatialHash.Fill(collisionBodyRegister.AllObjects, colliderRegister.AllObjects);

            ProcessCollisions();
            
            Resolve();
        }
        private void ProcessCollisions()
        {
            foreach (var collisionBody in collisionBodyRegister.AllObjects)
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
                            if (candidate.Parent != null && collider.Parent != null && candidate.Parent == collider.Parent) continue;
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
            collisionBodyRegister.Process();
            colliderRegister.Process();


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
        public Dictionary<Collider, QueryInfos>? QuerySpace(CollisionObject collisionObject, Vector2 origin, bool sorted = true)
        {
            if (!collisionObject.Enabled || !collisionObject.HasColliders) return null;

            Dictionary<Collider, QueryInfos>? result = null;
            
            foreach (var collider in collisionObject.Colliders)
            {
                var info = QuerySpace(collider, origin, sorted);
                if (info != null && info.Count > 0)
                {
                    result ??= new();
                    result.Add(collider, info);
                }
            }

            return result;
        }
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

        
        public void CastSpace(CollisionObject collisionBody, ref List<Collider> result, bool sorted = false)
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

        public static HashSet<CollisionObject>? GetParents(List<Collider> colliders)
        {
            if (colliders.Count <= 0) return null;
            
            var parents = new HashSet<CollisionObject>();
            
            foreach (var collider in colliders)
            {
                var parent = collider.Parent;
                if (parent != null)
                {
                    parents.Add(parent);
                }
            }

            return parents;
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
        
