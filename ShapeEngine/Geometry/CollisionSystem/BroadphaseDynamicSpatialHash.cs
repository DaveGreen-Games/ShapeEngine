using ShapeEngine.Color;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.CollisionSystem;


//NOTE: 
// - coordinates struct -> needs to be hashable for dict (IEquatable?)
// - store buckets in Dict<Coordinates, Bucket>
// - store added colliders in register Dict<Collider, HashSet<Bucket>>
// - when adding a collider, get bounding box and use the 4 corners to calculate coordinate range -> add all buckets in that range to the collider's register entry and create buckets for unused coordinates
// - when querying, do the same (bounding box corners -> corrdinates -> coordinate range -> get all buckets in that range and return them)

//Q: Is there any way to set this up in a way that doesn´t require recalculating everything every frame? Just update colliders that are already in the register, and remove colliders that are no longer there?

//NOTE: 
// - set max amount of buckets to avoid memory issues
// - once max is reached, start reusing old buckets (either oldest, or empty ones, or least used ones?)
// - every frame all buckets are cleared but the could stay in the dict -> there needs to be a way to track which buckets are in used, which are empty, and which are old
// - the other way would be a simple pooling system where all buckets are cleared and removed from the dict and put into bucket pool.
// whenever a bucket is needed, one is taken from the pool or a new one is created if the pool is empty and is put into the inuse set

public class BroadphaseDynamicSpatialHash : IBroadphase
{
    private struct Coords(int x, int y) : IEquatable<Coords>
    {
        public readonly int X = x;
        public readonly int Y = y;

        public override bool Equals(object? obj)
        {
            if (obj is Coords other)
            {
                return X == other.X && Y == other.Y;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
        public bool Equals(Coords other)
        {
            return X == other.X && Y == other.Y;
        }
    }
    
    
    private Rect currentBounds = new();//calculated from added colliders

    //TODO: use spatial hash system to only remove colliders that are no longer used
    private readonly Dictionary<Collider, HashSet<BroadphaseBucket>> register = new(); //cleared every frame
    private readonly Dictionary<Coords, BroadphaseBucket> buckets = new();
    private readonly HashSet<BroadphaseBucket> availableBuckets = []; //could be a queue or stack as well
    private readonly Dictionary<BroadphaseBucket, Coords> emptyUsedBuckets = [];
    private readonly int maxBuckets;
    private readonly Size bucketSize;
    private int createdBuckets = 0;
    public BroadphaseDynamicSpatialHash(float bucketWidth, float bucketHeight, int maxBuckets)
    {
        this.maxBuckets = maxBuckets;
        bucketSize = new Size(bucketWidth, bucketHeight);
    }


    private void AddCollider(Collider collider)
    {
        if (!collider.Enabled) return;
        if (register.ContainsKey(collider)) return;

        var boundingBox = collider.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);
        
        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        var registerSet = new HashSet<BroadphaseBucket>(bucketCount);
        register[collider] = registerSet;
        
        if (bucketCount > 1)
        {
            var tlBucketRect = GetBucketBounds(new Coords(minX, minY));
            var brBucketRect = GetBucketBounds(new Coords(maxX, maxY));
            var totalBucketsRect = new Rect(tlBucketRect.TopLeft, brBucketRect.BottomRight);
            currentBounds = currentBounds.Union(totalBucketsRect);
        }
        else
        {
            currentBounds = currentBounds.Union(GetBucketBounds(new Coords(minX, minY)));
        }
        
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                var coords = new Coords(x, y);
                
                //collider does not overlap with this bucket - only checked with more than 4 buckets to save performance
                if (bucketCount > 4 && !OverlapsColliderShapeWithBucketRect(coords, collider)) continue;
                
                if (buckets.TryGetValue(coords, out var bucket))//bucket already exists
                {
                    if(bucket.Count == 0) emptyUsedBuckets.Remove(bucket);//no longer empty
                    bucket.Add(collider);
                    registerSet.Add(bucket);
                }
                else //bucket does not exist yet
                {
                    if (availableBuckets.Count > 0)
                    {
                        bucket = availableBuckets.First();
                        availableBuckets.Remove(bucket);
                    }
                    else
                    {
                        if (maxBuckets <= 0)
                        {
                            if (emptyUsedBuckets.Count > 0)
                            {
                                var kvp = emptyUsedBuckets.First();
                                bucket = kvp.Key;
                                buckets.Remove(kvp.Value);
                                emptyUsedBuckets.Remove(bucket);
                            }
                            else
                            {
                                //create a new bucket
                                bucket = [];
                                createdBuckets++;
                            }
                        }
                        else
                        {
                            if (createdBuckets >= maxBuckets)
                            {
                                if (emptyUsedBuckets.Count > 0)
                                {
                                    var kvp = emptyUsedBuckets.First();
                                    bucket = kvp.Key;
                                    buckets.Remove(kvp.Value);
                                    emptyUsedBuckets.Remove(bucket);
                                }
                                else
                                {
                                    Game.Instance.Logger.LogWarning($"BroadphaseDynamicSpatialHash: Max bucket count of {maxBuckets} reached and no empty used buckets available! Cannot add new bucket for collider {collider}.");
                                    continue;
                                }
                            }
                            else
                            {
                                //create a new bucket
                                bucket = [];
                                createdBuckets++;
                            }
                        }
                    }
                    
                    bucket.Add(collider);
                    buckets[coords] = bucket;
                    registerSet.Add(bucket);
                }
            }
        }

    }
    private void Clear()
    {
        register.Clear();
        foreach (var kvp in emptyUsedBuckets)
        {
            buckets.Remove(kvp.Value);
            availableBuckets.Add(kvp.Key);
        }
        emptyUsedBuckets.Clear();
        
        foreach (var kvp in buckets)
        {
            var coords = kvp.Key;
            var bucket = kvp.Value;
            //was not used this frame, remove from structure
            if (bucket.Count == 0) //should not happen, but just in case (emptyUsedBuckets should clear all of them out before)
            {
                availableBuckets.Add(bucket);
                buckets.Remove(coords);
            }
            else
            {
                bucket.Clear();
                emptyUsedBuckets.Add(bucket, coords);
            }
        }
    }
    private Rect GetBucketBounds(Coords coords)
    {
        return new Rect(coords.X * bucketSize.Width, coords.Y * bucketSize.Height, bucketSize.Width, bucketSize.Height);
    }
    private bool OverlapsColliderShapeWithBucketRect(Coords coords, Collider collider)
    {
        var bucketRect = GetBucketBounds(coords);
        return collider.Overlap(bucketRect);
    }
    
    public Rect GetBounds() => currentBounds;
    public void Fill(IEnumerable<CollisionObject> collisionBodies)
    {
        Clear();
        currentBounds = new();
        foreach (var body in collisionBodies)
        {
            if (!body.Enabled || !body.HasColliders) continue;
            foreach (var collider in body.Colliders)
            {
                AddCollider(collider);
            }
        }
    }
    public void Close()
    {
        register.Clear();
        buckets.Clear();
        availableBuckets.Clear();
        emptyUsedBuckets.Clear();
        createdBuckets = 0;
        currentBounds = new();
    }
    public void ResizeBounds(Rect targetBounds) { }
    public void DebugDraw(ColorRgba border, ColorRgba fill)
    {
        var borderLineThickness = currentBounds.Size.Max() * 0.0025f;
        if (borderLineThickness <= 1f) borderLineThickness = 1f;
        currentBounds.DrawLines(borderLineThickness, border);
        
        var lineThickness = bucketSize.Max() * 0.0025f;
        if(lineThickness <= 0.5f) lineThickness = 0.5f;
        
        foreach (var kvp in buckets)
        {
            var coords = kvp.Key;
            var bucket = kvp.Value;
            var rect = GetBucketBounds(coords);
            if (bucket.Count == 0)
            {
                rect.Draw(fill.ChangeAlpha(100));
                rect.DrawLines(lineThickness, border.ChangeAlpha(100));
            }
            else
            {
                rect.Draw(fill);
                rect.DrawLines(lineThickness, border);
            }
            
        }
    }

    public int GetCandidateBuckets(CollisionObject collisionObject, ref List<BroadphaseBucket> candidateBuckets)
    {
        var count = 0;
        
        foreach (var collider in collisionObject.Colliders)
        {
            count += GetCandidateBuckets(collider, ref candidateBuckets);
        }

        return count;
    }

    public int GetCandidateBuckets(Collider collider, ref List<BroadphaseBucket> candidateBuckets)
    {
        if (!collider.Enabled) return 0;
        
        if (register.TryGetValue(collider, out var registerBuckets))
        {
            foreach (var bucket in registerBuckets)
            {
                candidateBuckets.Add(bucket);
            }
            return registerBuckets.Count;
        }
        
        switch (collider.GetShapeType())
        {
            case ShapeType.Circle: return  GetCandidateBuckets(collider.GetCircleShape(), ref candidateBuckets);
            case ShapeType.Segment: return  GetCandidateBuckets(collider.GetSegmentShape(), ref candidateBuckets);
            case ShapeType.Line: return  GetCandidateBuckets(collider.GetLineShape(), ref candidateBuckets);
            case ShapeType.Ray: return  GetCandidateBuckets(collider.GetRayShape(), ref candidateBuckets);
            case ShapeType.Triangle: return  GetCandidateBuckets(collider.GetTriangleShape(), ref candidateBuckets);
            case ShapeType.Rect: return  GetCandidateBuckets(collider.GetRectShape(), ref candidateBuckets);
            case ShapeType.Quad: return  GetCandidateBuckets(collider.GetQuadShape(), ref candidateBuckets);
            case ShapeType.Poly: return  GetCandidateBuckets(collider.GetPolygonShape(), ref candidateBuckets);
            case ShapeType.PolyLine: return  GetCandidateBuckets(collider.GetPolylineShape(), ref candidateBuckets);
        }

        return 0;
    }

    public int GetCandidateBuckets(Segment segment, ref List<BroadphaseBucket> candidateBuckets)
    {
        var boundingBox = segment.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coords(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coords(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    var bucketRect = GetBucketBounds(coords);
                    if (!segment.OverlapShape(bucketRect)) continue;
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }

    public int GetCandidateBuckets(Line line, ref List<BroadphaseBucket> candidateBuckets)
    {
        var boundingBox = line.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coords(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coords(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    var bucketRect = GetBucketBounds(coords);
                    if (!line.OverlapShape(bucketRect)) continue;
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }

    public int GetCandidateBuckets(Ray ray, ref List<BroadphaseBucket> candidateBuckets)
    {
        var boundingBox = ray.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coords(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coords(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    var bucketRect = GetBucketBounds(coords);
                    if (!ray.OverlapShape(bucketRect)) continue;
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }

    public int GetCandidateBuckets(Circle circle, ref List<BroadphaseBucket> candidateBuckets)
    {
        var boundingBox = circle.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coords(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coords(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    var bucketRect = GetBucketBounds(coords);
                    if (!circle.OverlapShape(bucketRect)) continue;
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }

    public int GetCandidateBuckets(Triangle triangle, ref List<BroadphaseBucket> candidateBuckets)
    {
        var boundingBox = triangle.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coords(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coords(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    var bucketRect = GetBucketBounds(coords);
                    if (!triangle.OverlapShape(bucketRect)) continue;
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }

    public int GetCandidateBuckets(Rect rect, ref List<BroadphaseBucket> candidateBuckets)
    {
        var minX = (int)Math.Floor(rect.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(rect.Right / bucketSize.Width);
        var minY = (int)Math.Floor(rect.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(rect.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coords(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coords(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                    
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }

    public int GetCandidateBuckets(Quad quad, ref List<BroadphaseBucket> candidateBuckets)
    {
        var boundingBox = quad.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coords(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coords(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    var bucketRect = GetBucketBounds(coords);
                    if (!quad.OverlapShape(bucketRect)) continue;
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }

    public int GetCandidateBuckets(Polygon poly, ref List<BroadphaseBucket> candidateBuckets)
    {
        var boundingBox = poly.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coords(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coords(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    var bucketRect = GetBucketBounds(coords);
                    if (!poly.OverlapShape(bucketRect)) continue;
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }

    public int GetCandidateBuckets(Polyline polyLine, ref List<BroadphaseBucket> candidateBuckets)
    {
        var boundingBox = polyLine.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coords(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coords(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    var bucketRect = GetBucketBounds(coords);
                    if (!polyLine.OverlapShape(bucketRect)) continue;
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }
}