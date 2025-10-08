using System.Numerics;
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

//NOTE: finish in the easiest way possible to test it out. After that decide between changing, optimizing or scrapping it.
public class BroadphaseQuadTree : IBroadphase
{
    private class QuadTreeNode
    {
        private readonly int capacity;
        private readonly Size minSize;

        private Rect newBounds = new();
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
            }
        }
        private readonly BroadphaseBucket splitBucket;
        private readonly BroadphaseBucket bucket;
        private readonly QuadTreeNode? parent;
        private QuadTreeNode[]? children; //4 children
        
        public QuadTreeNode(Rect bounds, int capacity, Size minSize)
        {
            this.minSize = minSize;
            this.capacity = capacity;
            Bounds = bounds;
            parent = null;
            children = null;
            splitBucket = [];
            bucket = [];
        }
        private QuadTreeNode(QuadTreeNode parent, AnchorPoint anchor, int capacity, Size minSize)
        {
            var parentBounds = parent.Bounds;
            var halfSize = parentBounds.Size / 2;
            var tl = parentBounds.TopLeft + anchor * halfSize;
            var br = tl + halfSize;
            Bounds = new Rect(tl, br);
            
            children = null;
            splitBucket = [];
            bucket = [];
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
            if (newBounds != bounds)
            {
                bounds = newBounds;
                newBounds = new();
            }
            return true;
        }
        public bool Add(Collider collider)
        {
            if (!collider.Enabled) return false;

            var boundingBox = collider.GetBoundingBox();
            
            if (!bounds.OverlapShape(boundingBox)) return false; // out of bounds
            
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

        //TODO: Implement
        public int GetCandidateBuckets(CollisionObject collidable, ref List<BroadphaseBucket> candidateBuckets, bool registeredOnly = false)
        {
            throw new NotImplementedException();
        }
        public int GetCandidateBuckets(Collider collider, ref List<BroadphaseBucket> candidateBuckets, bool registeredOnly = false)
        {
            throw new NotImplementedException();
        }
        public int GetCandidateBuckets(Segment segment, ref List<BroadphaseBucket> candidateBuckets)
        {
            throw new NotImplementedException();
        }
        public int GetCandidateBuckets(Line line, ref List<BroadphaseBucket> candidateBuckets)
        {
            throw new NotImplementedException();
        }
        public int GetCandidateBuckets(Ray ray, ref List<BroadphaseBucket> candidateBuckets)
        {
            throw new NotImplementedException();
        }
        public int GetCandidateBuckets(Circle circle, ref List<BroadphaseBucket> candidateBuckets)
        {
            throw new NotImplementedException();
        }
        public int GetCandidateBuckets(Triangle triangle, ref List<BroadphaseBucket> candidateBuckets)
        {
            throw new NotImplementedException();
        }
        public int GetCandidateBuckets(Rect rect, ref List<BroadphaseBucket> candidateBuckets)
        {
            throw new NotImplementedException();
        }
        public int GetCandidateBuckets(Quad quad, ref List<BroadphaseBucket> candidateBuckets)
        {
            throw new NotImplementedException();
        }
        public int GetCandidateBuckets(Polygon poly, ref List<BroadphaseBucket> candidateBuckets)
        {
            throw new NotImplementedException();
        }
        public int GetCandidateBuckets(Polyline polyLine, ref List<BroadphaseBucket> candidateBuckets)
        {
            throw new NotImplementedException();
        }
        
        
        public void DebugDraw(ColorRgba border, ColorRgba fill)
        {
            var lt = bounds.Size.Max() * 0.02f;
            if (lt < 0.5f) lt = 0.5f;
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
                        bucket.Add(collider);
                    }
                }
            }
            firstChild?.Add(collider);
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
            children[0] = new QuadTreeNode(this, AnchorPoint.TopLeft, capacity, minSize);
            children[1] = new QuadTreeNode(this, AnchorPoint.TopRight, capacity, minSize);
            children[2] = new QuadTreeNode(this, AnchorPoint.BottomRight, capacity, minSize);
            children[3] = new QuadTreeNode(this, AnchorPoint.BottomLeft, capacity, minSize);
        }
    }
    

    private readonly QuadTreeNode root;
    public Rect Bounds => root.Bounds;
    

    public BroadphaseQuadTree(Rect bounds, int maxObjects, Size minBoundsSize)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            Game.Instance.Logger.LogError($"BroadphaseQuadTree creation failed: invalid bounds {bounds}, bounds size must be positive.");
            throw new ArgumentException($"BroadphaseQuadTree creation failed: Invalid bounds {bounds}, bounds size must be positive.", nameof(bounds));
        }

        if (maxObjects <= 0)
        {
            Game.Instance.Logger.LogError($"BroadphaseQuadTree creation failed: maxObjects must be greater than zero, got {maxObjects}.");
            throw new ArgumentException($"BroadphaseQuadTree creation failed: Invalid maxObjects {maxObjects}, must be greater than zero.", nameof(maxObjects));
        }
        
        if (minBoundsSize.Width >= bounds.Size.Width || minBoundsSize.Height >= bounds.Size.Height)
        {
            minBoundsSize = new Size(0, 0); // disabled
            Game.Instance.Logger.LogWarning($"BroadphaseQuadTree creation: minBoundsSize {minBoundsSize} is larger than or equal to bounds size {bounds.Size}, disabling minimum bounds size.");
        }
        
        root = new(bounds, maxObjects, minBoundsSize);
    }
    
    
    
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
    
    
    //TODO: Implement
    public int GetCandidateBuckets(CollisionObject collidable, ref List<BroadphaseBucket> candidateBuckets, bool registeredOnly = false)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Collider collider, ref List<BroadphaseBucket> candidateBuckets, bool registeredOnly = false)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Segment segment, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Line line, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Ray ray, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Circle circle, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Triangle triangle, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Rect rect, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Quad quad, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Polygon poly, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Polyline polyLine, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
}