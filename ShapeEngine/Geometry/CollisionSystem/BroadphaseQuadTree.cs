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


public class BroadphaseQuadTree : IBroadphase
{
    private class QuadTreeNode
    {
        private readonly int maxObjects;
        private readonly Size minBoundsSize;
        
        private Rect bounds;
        private readonly AnchorPoint anchor;
        private readonly BroadphaseBucket bucket;
        private readonly QuadTreeNode? parent;
        private QuadTreeNode[]? children; //4 children
        
        public QuadTreeNode(Rect bounds, int maxObjects, Size minBoundsSize)//root constructor
        {
            // if (bounds.Width <= 0 || bounds.Height <= 0)
            // {
            //     Game.Instance.Logger.LogError($"BroadphaseQuadTree root node creation failed: invalid bounds {bounds}, bounds size must be positive.");
            //     throw new ArgumentException($"BroadphaseQuadTree root node creation failed: Invalid bounds {bounds}, bounds size must be positive.", nameof(bounds));
            // }
            // if (maxObjects <= 0)
            // {
            //     Game.Instance.Logger.LogError($"BroadphaseQuadTree root node creation failed: maxObjects must be greater than zero, got {maxObjects}.");
            //     throw new ArgumentException($"BroadphaseQuadTree root node creation failed: Invalid maxObjects {maxObjects}, must be greater than zero.", nameof(maxObjects));
            // }
            //
            // this.minBoundsSize = minBoundsSize;
            // if (minBoundsSize.Width >= bounds.Size.Width || minBoundsSize.Height >= bounds.Size.Height)
            // {
            //     this.minBoundsSize = new Size(0, 0); // disabled
            //     Game.Instance.Logger.LogWarning($"BroadphaseQuadTree root node creation: minBoundsSize {minBoundsSize} is larger than or equal to bounds size {bounds.Size}, disabling minimum bounds size.");
            // }
            this.minBoundsSize = minBoundsSize;
            this.maxObjects = maxObjects;
            this.bounds = bounds;
            anchor = AnchorPoint.Center;//is ignored for root
            parent = null;
            children = null;
            bucket = [];
        }
        private QuadTreeNode(QuadTreeNode parent, AnchorPoint anchor, int maxObjects, Size minBoundsSize)
        {
            this.parent = parent;
            this.anchor = anchor;
            children = null;
            bucket = [];
            bounds = new();
            this.maxObjects = maxObjects;
            this.minBoundsSize = minBoundsSize;
        }

        private bool CanSplit()
        {
            if(children is { Length: > 0 }) return false;
            
            if(minBoundsSize is { Width: > 0, Height: > 0 })
            {
                var currentBounds = GetBounds();
                if (currentBounds.Width / 2 < minBoundsSize.Width || currentBounds.Height / 2 < minBoundsSize.Height)
                {
                    return false; // cannot split further
                }
            }

            return true;
        }
        private bool ShouldSplit(int added)
        {
            if (!CanSplit()) return false;
            return bucket.Count + added > maxObjects;
        }
        private bool Split()
        {
            if (!CanSplit()) return false;
            
            children = new QuadTreeNode[4];
            children[0] = new QuadTreeNode(this, AnchorPoint.TopLeft, maxObjects, minBoundsSize);
            children[1] = new QuadTreeNode(this, AnchorPoint.TopRight, maxObjects, minBoundsSize);
            children[2] = new QuadTreeNode(this, AnchorPoint.BottomRight, maxObjects, minBoundsSize);
            children[3] = new QuadTreeNode(this, AnchorPoint.BottomLeft, maxObjects, minBoundsSize);
            
            return true;
        }
        public bool SetBounds(Rect newBounds)
        {
            if (parent != null) return false;
            if(newBounds.Width <= 0 || newBounds.Height <= 0)
            {
                Game.Instance.Logger.LogWarning($"QuadTreeNode SetBounds failed: invalid bounds {newBounds}, bounds size must be positive.");
                return false;
            }
            bounds = newBounds;
            return true;
        }
        public Rect GetBounds()
        {
            if (parent == null) return bounds;
            
            var parentBounds = parent.GetBounds();
            var halfSize = parentBounds.Size / 2;
            var tl = parentBounds.TopLeft + anchor * halfSize;
            var br = tl + halfSize;
            return new Rect(tl, br);
        }
        
    }
    
    
    //NOTE: Tree structure?

    private readonly QuadTreeNode root;
    public Rect Bounds => root.GetBounds();
    

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
        
    }

    public void Close()
    {
        
    }

    public void ResizeBounds(Rect targetBounds)
    {
        
    }

    public void DebugDraw(ColorRgba border, ColorRgba fill)
    {
       
    }
    
    
    
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