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

public class BroadphaseQuadTree : IBroadphase
{
    private class QuadTreeNode
    {
        private readonly int capacity;
        private readonly Size minSize;

        private bool newBoundsSet = false;
        private Rect newBounds;
        private Rect bounds;
        public Rect Bounds
        {
            get => bounds;
            set
            {
                if (parent != null) throw new InvalidOperationException("Cannot set bounds of a non-root QuadTreeNode.");
                if(value.Width <= 0 || value.Height <= 0)
                {
                    Game.Instance.Logger.LogWarning($"QuadTreeNode Bounds setter failed: invalid bounds {value}, bounds size must be positive.");
                    return;
                }
                newBounds = value;
                newBoundsSet = true;
            }
        }
        private readonly BroadphaseBucket splitBucket;
        private readonly BroadphaseBucket bucket;
        private readonly QuadTreeNode? parent;
        private readonly QuadTreeNode root;
        private QuadTreeNode[]? children; //4 children
        public QuadTreeNode(Rect bounds, int capacity, Size minSize)
        {
            this.minSize = minSize;
            if (capacity <= 1) capacity = 2; //safeguard against invalid capacity
            this.capacity = capacity;
            this.bounds = bounds;
            root = this;
            parent = null;
            children = null;
            splitBucket = [];
            bucket = [];
        }
        private QuadTreeNode(QuadTreeNode parent, QuadTreeNode root, AnchorPoint anchor, int capacity, Size minSize)
        {
            var parentBounds = parent.Bounds;
            var halfSize = parentBounds.Size / 2;
            var tl = parentBounds.TopLeft + anchor * halfSize;
            var br = tl + halfSize;
            bounds = new Rect(tl, br);
            children = null;
            splitBucket = [];
            bucket = [];
            this.root = root;
            this.parent = parent;
            this.capacity = capacity;
            this.minSize = minSize;
        }

        public bool Clear()
        {
            if (parent != null) return false;
            if (children != null)
            {
                foreach (var child in children)
                {
                    child.Clear();
                }
                children = null;
            }
            splitBucket.Clear();
            bucket.Clear();
            if (newBoundsSet)
            {
                bounds = newBounds;
                newBounds = new();
                newBoundsSet = false;
            }
            return true;
        }
        public bool Add(Collider collider)
        {
            if (!collider.Enabled)
            {
                return false;
            }
            
            var boundingBox = collider.GetBoundingBox();
            
            if (!bounds.OverlapShape(boundingBox))
            {
                return false; // out of bounds
            }
            
            var halfSize = bounds.Size / 2;
            if(boundingBox.Size.Width > halfSize.Width || boundingBox.Size.Height > halfSize.Height)
            {
                // too big, add to this bucket
                bucket.Add(collider);
                return true;
            }

            if (children == null)
            {
                if(splitBucket.Count >= capacity && CanSplit())//split
                {
                    Split();
                    splitBucket.Add(collider);
                    ProcessSplitBucket();
                }
                else
                {
                    splitBucket.Add(collider);
                }
            }
            else
            {
                ProcessCollider(collider);
            }

            return true;
        }


        public int GetCandidateBuckets(CollisionObject collidable, ref List<BroadphaseBucket> candidateBuckets)
        {
            int count = 0;
            foreach (var collider in collidable.Colliders)
            {
                count += GetCandidateBuckets(collider, ref candidateBuckets);
            }

            return count;
        }
        public int GetCandidateBuckets(Collider collider, ref List<BroadphaseBucket> candidateBuckets)
        {
            if (!collider.Enabled) return 0;
            
            switch (collider.GetShapeType())
            {
                case ShapeType.Circle: return GetCandidateBuckets(collider.GetCircleShape(), ref candidateBuckets); 
                case ShapeType.Segment: return GetCandidateBuckets(collider.GetSegmentShape(), ref candidateBuckets); 
                case ShapeType.Line: return GetCandidateBuckets(collider.GetLineShape(), ref candidateBuckets); 
                case ShapeType.Ray: return GetCandidateBuckets(collider.GetRayShape(), ref candidateBuckets); 
                case ShapeType.Triangle: return GetCandidateBuckets(collider.GetTriangleShape(), ref candidateBuckets); 
                case ShapeType.Rect: return GetCandidateBuckets(collider.GetRectShape(), ref candidateBuckets); 
                case ShapeType.Quad: return GetCandidateBuckets(collider.GetQuadShape(), ref candidateBuckets); 
                case ShapeType.Poly: return GetCandidateBuckets(collider.GetPolygonShape(), ref candidateBuckets); 
                case ShapeType.PolyLine: return GetCandidateBuckets(collider.GetPolylineShape(), ref candidateBuckets); 
            }

            return 0;
        }
        public int GetCandidateBuckets(Segment segment, ref List<BroadphaseBucket> candidateBuckets)
        {
            // var boundingBox = segment.GetBoundingBox();
            if (!bounds.OverlapShape(segment)) return 0; // out of bounds
            
            var added = 0;
            
            if (bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
            
            if (children == null)
            {
                if (splitBucket.Count > 0)
                {
                    candidateBuckets.Add(splitBucket);
                    added++;
                }
                return added;
            }
            
            foreach (var child in children)
            {
                added += child.GetCandidateBuckets(segment, ref candidateBuckets);
            }
            
            return added;
        }
        public int GetCandidateBuckets(Line line, ref List<BroadphaseBucket> candidateBuckets)
        {
            // var boundingBox = line.GetBoundingBox();
            if (!bounds.OverlapShape(line)) return 0; // out of bounds
            
            var added = 0;
            
            if (bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
            
            if (children == null)
            {
                if (splitBucket.Count > 0)
                {
                    candidateBuckets.Add(splitBucket);
                    added++;
                }
                return added;
            }
            
            foreach (var child in children)
            {
                added += child.GetCandidateBuckets(line, ref candidateBuckets);
            }
            
            return added;
        }
        public int GetCandidateBuckets(Ray ray, ref List<BroadphaseBucket> candidateBuckets)
        {
            // var boundingBox = ray.GetBoundingBox();
            if (!bounds.OverlapShape(ray)) return 0; // out of bounds
            
            var added = 0;
            
            if (bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
            
            if (children == null)
            {
                if (splitBucket.Count > 0)
                {
                    candidateBuckets.Add(splitBucket);
                    added++;
                }
                return added;
            }
            
            foreach (var child in children)
            {
                added += child.GetCandidateBuckets(ray, ref candidateBuckets);
            }
            
            return added;
        }
        public int GetCandidateBuckets(Circle circle, ref List<BroadphaseBucket> candidateBuckets)
        {
            var boundingBox = circle.GetBoundingBox();
            if (!bounds.OverlapShape(boundingBox)) return 0; // out of bounds
            
            var added = 0;
            
            if (bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
            
            if (children == null)
            {
                if (splitBucket.Count > 0)
                {
                    candidateBuckets.Add(splitBucket);
                    added++;
                }
                return added;
            }
            
            foreach (var child in children)
            {
                added += child.GetCandidateBuckets(circle, ref candidateBuckets);
            }
            
            return added;
        }
        public int GetCandidateBuckets(Triangle triangle, ref List<BroadphaseBucket> candidateBuckets)
        {
            var boundingBox = triangle.GetBoundingBox();
            if (!bounds.OverlapShape(boundingBox)) return 0; // out of bounds
            
            var added = 0;
            
            if (bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
            
            if (children == null)
            {
                if (splitBucket.Count > 0)
                {
                    candidateBuckets.Add(splitBucket);
                    added++;
                }
                return added;
            }
            
            foreach (var child in children)
            {
                added += child.GetCandidateBuckets(triangle, ref candidateBuckets);
            }
            
            return added;
        }
        public int GetCandidateBuckets(Rect rect, ref List<BroadphaseBucket> candidateBuckets)
        {
            if (!bounds.OverlapShape(rect)) return 0; // out of bounds
            
            var added = 0;
            
            if (bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
            
            if (children == null)
            {
                if (splitBucket.Count > 0)
                {
                    candidateBuckets.Add(splitBucket);
                    added++;
                }
                return added;
            }
            
            foreach (var child in children)
            {
                added += child.GetCandidateBuckets(rect, ref candidateBuckets);
            }
            
            return added;
        }
        public int GetCandidateBuckets(Quad quad, ref List<BroadphaseBucket> candidateBuckets)
        {
            var boundingBox = quad.GetBoundingBox();
            if (!bounds.OverlapShape(boundingBox)) return 0; // out of bounds
            
            var added = 0;
            
            if (bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
            
            if (children == null)
            {
                if (splitBucket.Count > 0)
                {
                    candidateBuckets.Add(splitBucket);
                    added++;
                }
                return added;
            }
            
            foreach (var child in children)
            {
                added += child.GetCandidateBuckets(quad, ref candidateBuckets);
            }
            
            return added;
        }
        public int GetCandidateBuckets(Polygon poly, ref List<BroadphaseBucket> candidateBuckets)
        {
            var boundingBox = poly.GetBoundingBox();
            if (!bounds.OverlapShape(boundingBox)) return 0; // out of bounds
            
            var added = 0;
            
            if (bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
            
            if (children == null)
            {
                if (splitBucket.Count > 0)
                {
                    candidateBuckets.Add(splitBucket);
                    added++;
                }
                return added;
            }
            
            foreach (var child in children)
            {
                added += child.GetCandidateBuckets(poly, ref candidateBuckets);
            }
            
            return added;
        }
        public int GetCandidateBuckets(Polyline polyLine, ref List<BroadphaseBucket> candidateBuckets)
        {
            var boundingBox = polyLine.GetBoundingBox();
            if (!bounds.OverlapShape(boundingBox)) return 0; // out of bounds
            
            var added = 0;
            
            if (bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
            
            if (children == null)
            {
                if (splitBucket.Count > 0)
                {
                    candidateBuckets.Add(splitBucket);
                    added++;
                }
                return added;
            }
            
            foreach (var child in children)
            {
                added += child.GetCandidateBuckets(polyLine, ref candidateBuckets);
            }
            
            return added;
        }
        
        public void DebugDraw(ColorRgba border, ColorRgba fill)
        {
            var lt = bounds.Size.Max() * 0.005f;
            if (lt < 0.5f) lt = 0.5f;
            // lt = 10;
            bounds.Draw(fill);
            bounds.DrawLines(lt, border);
            if (children == null) return;
            foreach (var child in children)
            {
                child.DebugDraw(border, fill);
            }
        }
        
        
        private void ProcessSplitBucket()
        {
            if (children == null) return;
            foreach (var c in splitBucket)
            {
                ProcessCollider(c);
            }
            splitBucket.Clear();
        }
        private void ProcessCollider(Collider collider)
        {
            if (children == null) return;
            
            var boundingBox = collider.GetBoundingBox();
            QuadTreeNode? firstChild = null;
            foreach (var child in children)
            {
                if (child.Bounds.OverlapShape(boundingBox))
                {
                    
                    if (firstChild == null)
                    {
                        firstChild = child;
                    }
                    else
                    {
                        // overlaps multiple children, add to this bucket (too big)
                        bucket.Add(collider);
                        return;
                    }
                }
            }

            if (firstChild != null)
            {
                firstChild.Add(collider);
            }
            else
            {
                bucket.Add(collider);
            }
            
        }
        private bool CanSplit()
        {
            if(children is { Length: > 0 }) return false;
            
            if(minSize is { Width: > 0, Height: > 0 })
            {
                if (bounds.Width / 2 < minSize.Width || bounds.Height / 2 < minSize.Height)
                {
                    return false; // cannot split further
                }
            }
            return true;
        }
        private void Split()
        {
            children = new QuadTreeNode[4];
            children[0] = new QuadTreeNode(this, root, AnchorPoint.TopLeft, capacity, minSize);
            children[1] = new QuadTreeNode(this, root, AnchorPoint.TopRight, capacity, minSize);
            children[2] = new QuadTreeNode(this, root, AnchorPoint.BottomRight, capacity, minSize);
            children[3] = new QuadTreeNode(this, root, AnchorPoint.BottomLeft, capacity, minSize);
        }
    }
    

    private readonly QuadTreeNode root;
    public BroadphaseQuadTree(Rect bounds, int capacity, Size minBoundsSize)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            Game.Instance.Logger.LogError($"BroadphaseQuadTree creation failed: invalid bounds {bounds}, bounds size must be positive.");
            throw new ArgumentException($"BroadphaseQuadTree creation failed: Invalid bounds {bounds}, bounds size must be positive.", nameof(bounds));
        }

        if (capacity <= 1)
        {
            Game.Instance.Logger.LogError($"BroadphaseQuadTree creation failed: maxObjects must be greater than one, got {capacity}.");
            throw new ArgumentException($"BroadphaseQuadTree creation failed: Invalid maxObjects {capacity}, must be greater than one.", nameof(capacity));
        }
        
        if (minBoundsSize.Width >= bounds.Size.Width || minBoundsSize.Height >= bounds.Size.Height)
        {
            minBoundsSize = new Size(0, 0); // disabled
            Game.Instance.Logger.LogWarning($"BroadphaseQuadTree creation: minBoundsSize {minBoundsSize} is larger than or equal to bounds size {bounds.Size}, disabling minimum bounds size.");
        }
        
        root = new(bounds, capacity, minBoundsSize);
    }


    public Rect GetBounds() => root.Bounds;

    public void Fill(IEnumerable<CollisionObject> collisionBodies)
    {
        root.Clear();
        foreach (var body in collisionBodies)
        {
            if (!body.Enabled || !body.HasColliders ) continue;
            foreach (var collider in body.Colliders)
            {
                root.Add(collider);
            }
        }
    }
    public void Close()
    {
        root.Clear();
    }
    public void ResizeBounds(Rect targetBounds)
    {
        root.Bounds = targetBounds;
    }
    public void DebugDraw(ColorRgba border, ColorRgba fill)
    {
       root.DebugDraw(border, fill);
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
        // if (register.TryGetValue(collider, out var registerBucket))
        // {
        //     if (registerBucket.Count <= 0) return 0;
        //     candidateBuckets.Add(registerBucket);
        //     return 1;
        //
        // }
        return root.GetCandidateBuckets(collider, ref candidateBuckets);
    }
    public int GetCandidateBuckets(Segment segment, ref List<BroadphaseBucket> candidateBuckets)
    {
        return root.GetCandidateBuckets(segment, ref candidateBuckets);
    }
    public int GetCandidateBuckets(Line line, ref List<BroadphaseBucket> candidateBuckets)
    {
        return root.GetCandidateBuckets(line, ref candidateBuckets);
    }
    public int GetCandidateBuckets(Ray ray, ref List<BroadphaseBucket> candidateBuckets)
    {
        return root.GetCandidateBuckets(ray, ref candidateBuckets);
    }
    public int GetCandidateBuckets(Circle circle, ref List<BroadphaseBucket> candidateBuckets)
    {
        return root.GetCandidateBuckets(circle, ref candidateBuckets);
    }
    public int GetCandidateBuckets(Triangle triangle, ref List<BroadphaseBucket> candidateBuckets)
    {
        return root.GetCandidateBuckets(triangle, ref candidateBuckets);
    }
    public int GetCandidateBuckets(Rect rect, ref List<BroadphaseBucket> candidateBuckets)
    {
        return root.GetCandidateBuckets(rect, ref candidateBuckets);
    }
    public int GetCandidateBuckets(Quad quad, ref List<BroadphaseBucket> candidateBuckets)
    {
        return root.GetCandidateBuckets(quad, ref candidateBuckets);
    }
    public int GetCandidateBuckets(Polygon poly, ref List<BroadphaseBucket> candidateBuckets)
    {
        return root.GetCandidateBuckets(poly, ref candidateBuckets);
    }
    public int GetCandidateBuckets(Polyline polyLine, ref List<BroadphaseBucket> candidateBuckets)
    {
        return root.GetCandidateBuckets(polyLine, ref candidateBuckets);
    }
}