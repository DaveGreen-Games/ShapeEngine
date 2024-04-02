using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Lib;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Collision
{
    
    public class SpatialHash : IBounds
    {
        public class Bucket : HashSet<Collider>
        {
            public Bucket? FilterObjects(BitFlag mask)
            {
                if (Count <= 0 || mask.IsEmpty()) return null;
                
                Bucket? objects = null;
                foreach (var collidable in this)
                {
                    if (mask.Has(collidable.CollisionLayer))
                    {
                        objects ??= new();
                        objects.Add(collidable);
                    }
                }
                return objects;
            }
            public Bucket? Copy() => Count <= 0 ? null : (Bucket)this.ToHashSet();
        }

        #region Public Members
        public Rect Bounds { get; private set; }
        public float SpacingX { get; private set; }
        public float SpacingY { get; private set; }
        public int BucketCount { get; private set; }
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        #endregion

        #region Private Members
        private Bucket[] buckets;
        private readonly Dictionary<Collider, List<int>> register = new();
        private readonly HashSet<Collider> registerKeys = new();
        private bool boundsResizeQueued = false;
        private Rect newBounds = new();
        #endregion
        
        #region Constructors
        public SpatialHash(float x, float y, float w, float h, int rows, int cols)
        {
            this.Bounds = new(x, y, w, h);
            this.Rows = rows;
            this.Cols = cols;
            this.SetSpacing();
            this.BucketCount = rows * cols;
            this.buckets = new Bucket[this.BucketCount];
            for (int i = 0; i < BucketCount; i++)
            {
                this.buckets[i] = new();
            }
        }
        public SpatialHash(Rect bounds, int rows, int cols)
        {
            this.Bounds = bounds;
            this.Rows = rows;
            this.Cols = cols;
            this.SetSpacing();
            this.BucketCount = rows * cols;
            this.buckets = new Bucket[this.BucketCount];
            for (int i = 0; i < BucketCount; i++)
            {
                this.buckets[i] = new();
            }
        }
        #endregion
        
        #region Public
        public void Fill(IEnumerable<CollisionObject> collisionBodies, IEnumerable<Collider> colliders)
        {
            Clear();

            foreach (var body in collisionBodies)
            {
                if (body.Enabled && body.HasColliders)
                {
                    Add(body);
                }
            }

            foreach (var collider in colliders)
            {
                Add(collider);
            }
            
            CleanRegister();
        }
        public void Close()
        {
            Clear();
            register.Clear();
            buckets = Array.Empty<Bucket>();  //new HashSet<ICollidable>[0];
        }
        public void ResizeBounds(Rect targetBounds) 
        {
            newBounds = targetBounds;
            boundsResizeQueued = true;
        }
        
        /// <summary>
        /// Change the cols and rows of the grid. Clears the spatial hash!
        /// </summary>
        /// <param name="cols"></param>
        /// <param name="rows"></param>
        public void ChangeGrid(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            SetSpacing();
            BucketCount = rows * cols;
            buckets = new Bucket[BucketCount];
            for (int i = 0; i < BucketCount; i++)
            {
                buckets[i] = new();
            }
        }

        public void GetRegisteredCollisionCandidateBuckets(Collider collider, ref List<Bucket> candidateBuckets)
        {
            if (!register.TryGetValue(collider, out var bucketIds)) return;
            if (bucketIds.Count <= 0) return;
            foreach (var id in bucketIds)
            {
                var bucket = buckets[id];
                if(bucket.Count > 0) candidateBuckets.Add(buckets[id]);
            }
        }
        
        public void GetCandidateBuckets(CollisionObject collidable, ref List<Bucket> candidateBuckets)
        {
            foreach (var collider in collidable.Colliders)
            {
                GetCandidateBuckets(collider, ref candidateBuckets);
            }
        }
        public void GetCandidateBuckets(Collider collider, ref List<Bucket> candidateBuckets)
        {
            if (register.TryGetValue(collider, out var bucketIds))
            {
                if (bucketIds.Count <= 0) return;
                foreach (var id in bucketIds)
                {
                    var bucket = buckets[id];
                    if(bucket.Count > 0) candidateBuckets.Add(buckets[id]);
                }

                return;
            }
            List<int> ids = new();
            GetCellIDs(collider, ref ids);
            FillCandidateBuckets(ids, ref candidateBuckets);
        }

        public void GetCandidateBuckets(Segment segment, ref List<Bucket> candidateBuckets)
        {
            List<int> bucketIds = new();
            GetCellIDs(segment, ref bucketIds);
            
            FillCandidateBuckets(bucketIds, ref candidateBuckets);
        }
        public void GetCandidateBuckets(Circle circle, ref List<Bucket> candidateBuckets)
        {
            List<int> bucketIds = new();
            GetCellIDs(circle, ref bucketIds);
            
            FillCandidateBuckets(bucketIds, ref candidateBuckets);
        }
        public void GetCandidateBuckets(Triangle triangle, ref List<Bucket> candidateBuckets)
        {
            List<int> bucketIds = new();
            GetCellIDs(triangle, ref bucketIds);
            
            FillCandidateBuckets(bucketIds, ref candidateBuckets);
        }
        public void GetCandidateBuckets(Rect rect, ref List<Bucket> candidateBuckets)
        {
            List<int> bucketIds = new();
            GetCellIDs(rect, ref bucketIds);
            
            FillCandidateBuckets(bucketIds, ref candidateBuckets);
        }
        public void GetCandidateBuckets(Polygon poly, ref List<Bucket> candidateBuckets)
        {
            List<int> bucketIds = new();
            GetCellIDs(poly, ref bucketIds);
            
            FillCandidateBuckets(bucketIds, ref candidateBuckets);
        }
        public void GetCandidateBuckets(Polyline polyLine, ref List<Bucket> candidateBuckets)
        {
            List<int> bucketIds = new();
            GetCellIDs(polyLine, ref bucketIds);
            
            FillCandidateBuckets(bucketIds, ref candidateBuckets);
        }
        private void FillCandidateBuckets(List<int> bucketIds, ref List<Bucket> candidateBuckets)
        {
            if (bucketIds.Count <= 0) return;
            foreach (var id in bucketIds)
            {
                var bucket = buckets[id];
                if(bucket.Count > 0) candidateBuckets.Add(buckets[id]);
            }
        }

        public void GetUniqueCandidates(CollisionObject collisionBody, ref HashSet<Collider> candidates)
        {
            if (!collisionBody.HasColliders) return;
            foreach (var collider in collisionBody.Colliders)
            {
                GetUniqueCandidates(collider, ref candidates);
            }
        }
        public void GetUniqueCandidates(Collider collider, ref HashSet<Collider> candidates)
        {
            if (register.TryGetValue(collider, out var bucketIds))
            {
                if (bucketIds.Count <= 0) return;
                foreach (var id in bucketIds)
                {
                    var bucket = buckets[id];
                    if(bucket.Count > 0) candidates.UnionWith(bucket);
                }

                return;
            }
            
            List<int> ids = new();
            GetCellIDs(collider, ref ids);
            AccumulateUniqueCandidates(ids, ref candidates);
        }
        public void GetUniqueCandidates(Segment segment, ref HashSet<Collider> candidates)
        {
            List<int> bucketIds = new();
            GetCellIDs(segment, ref bucketIds);
            
            AccumulateUniqueCandidates(bucketIds, ref candidates);
        }
        public void GetUniqueCandidates(Circle circle, ref HashSet<Collider> candidates)
        {
            List<int> bucketIds = new();
            GetCellIDs(circle, ref bucketIds);
            
            AccumulateUniqueCandidates(bucketIds, ref candidates);
        }
        public void GetUniqueCandidates(Triangle triangle, ref HashSet<Collider> candidates)
        {
            List<int> bucketIds = new();
            GetCellIDs(triangle, ref bucketIds);
            
            AccumulateUniqueCandidates(bucketIds, ref candidates);
        }
        public void GetUniqueCandidates(Rect rect, ref HashSet<Collider> candidates)
        {
            List<int> bucketIds = new();
            GetCellIDs(rect, ref bucketIds);
            
            AccumulateUniqueCandidates(bucketIds, ref candidates);
        }
        public void GetUniqueCandidates(Polygon poly, ref HashSet<Collider> candidates)
        {
            List<int> bucketIds = new();
            GetCellIDs(poly, ref bucketIds);
            
            AccumulateUniqueCandidates(bucketIds, ref candidates);
        }
        public void GetUniqueCandidates(Polyline polyLine, ref HashSet<Collider> candidates)
        {
            List<int> bucketIds = new();
            GetCellIDs(polyLine, ref bucketIds);
            
            AccumulateUniqueCandidates(bucketIds, ref candidates);
        }
        private void AccumulateUniqueCandidates(List<int> bucketIds, ref HashSet<Collider> candidates)
        {
            if (bucketIds.Count <= 0) return;
            foreach (var id in bucketIds)
            {
                var bucket = buckets[id];
                if(bucket.Count > 0) candidates.UnionWith(bucket);
            }
        }

       
        public void DebugDraw(ColorRgba border, ColorRgba fill)
        {
            for (int i = 0; i < BucketCount; i++)
            {
                var coords = GetCoordinatesGrid(i);
                var rect = new Rect(Bounds.X + coords.x * SpacingX, Bounds.Y + coords.y * SpacingY, SpacingX, SpacingY);
                rect.DrawLines(2f, border);
                int id = GetCellId(coords.x, coords.y);
                if (buckets[id].Count > 0)
                {
                    rect.Draw(fill);
                }
                
                
                // var rect = new Rectangle(Bounds.X + coords.x * SpacingX, Bounds.Y + coords.y * SpacingY, SpacingX, SpacingY);

                // Raylib.DrawRectangleLinesEx(rect, 1, border.ToRayColor());
                // int id = GetCellID(coords.x, coords.y);
                // if (buckets[id].Count > 0)
                // {
                //     Raylib.DrawRectangleRec(rect, fill.ToRayColor());
                // }

            }
        }
        #endregion

        #region Private
        private (int x, int y) GetCoordinatesGrid(int index)
        {
            return (index % Cols, index / Cols);
            //return new Tuple<int x, int y>(index % cols, index / cols);
        }

        private Vector2 GetCoordinatesWorld(int index)
        {
            var coord = GetCoordinatesGrid(index);
            return new Vector2(coord.x * SpacingX, coord.y * SpacingY);
        }
        private Rect GetCellRectangle(int x, int y)
        {
            return new Rect(Bounds.X + x * SpacingX, Bounds.Y + y * SpacingY, SpacingX, SpacingY);
        }
        private Rect GetCellRectangle(int index)
        {
            return GetCellRectangle(index % Cols, index / Cols);
        }
        private int GetCellId(int x, int y)
        {
            return x + y * Cols;
        }

        private int GetCellId(float x, float y)
        {
            int xi = Math.Clamp((int)Math.Floor((x - Bounds.X) / SpacingX), 0, Cols - 1);
            int yi = Math.Clamp((int)Math.Floor((y - Bounds.Y) / SpacingY), 0, Rows - 1);
            return GetCellId(xi, yi);
        }
        private (int x, int y) GetCellCoordinate(float x, float y)
        {
            int xi = Math.Clamp((int)Math.Floor((x - Bounds.X) / SpacingX), 0, Cols - 1);
            int yi = Math.Clamp((int)Math.Floor((y - Bounds.Y) / SpacingY), 0, Rows - 1);
            return (xi, yi);
        }
        private void Add(CollisionObject collisionBody)
        {
            // if (!collisionBody.HasColliders) return;
            foreach (var collider in collisionBody.Colliders)
            {
                Add(collider);
            }
            /*
             if (!collider.Enabled || collider.Parent == null) continue;
               
               List<int> ids;
               if (register.TryGetValue(collider, out var value))
               {
                   ids = value;
                   ids.Clear();
               }
               else
               {
                   ids = new List<int>();
                   register.Add(collider, ids);
               
               }
               GetCellIDs(collider, ref ids);
               if (ids.Count <= 0) return;
               registerKeys.Remove(collider);
               foreach (int hash in ids)
               {
                   buckets[hash].Add(collider);
               }
             */
        }
        private void Add(Collider collider)
        {
            if (!collider.Enabled) return;
                
            List<int> ids;
            if (register.TryGetValue(collider, out var value))
            {
                ids = value;
                ids.Clear();
            }
            else
            {
                ids = new List<int>();
                register.Add(collider, ids);
                
            }
            GetCellIDs(collider, ref ids);
            if (ids.Count <= 0) return;
            registerKeys.Remove(collider);
            foreach (int hash in ids)
            {
                buckets[hash].Add(collider);
            }
        }

        private void CleanRegister()
        {
            foreach (var collider in registerKeys)
            {
                register.Remove(collider);
            }

            registerKeys.Clear();
            registerKeys.UnionWith(register.Keys);
            // registerKeys = register.Keys.ToHashSet();
        }
        private void Clear()
        {
            for (var i = 0; i < BucketCount; i++)
            {
                buckets[i].Clear();
            }

            if (boundsResizeQueued)
            {
                boundsResizeQueued = false;
                Bounds = newBounds;
                SetSpacing();
            }
        }
        
        private void GetCellIDs(Segment segment, ref List<int> idList)
        {
            var boundingRect = segment.GetBoundingBox();
            var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
            var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

            for (int j = topLeft.y; j <= bottomRight.y; j++)
            {
                for (int i = topLeft.x; i <= bottomRight.x; i++)
                {
                    int id = GetCellId(i, j);
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(segment)) idList.Add(id);
                }
            }
        }
        private void GetCellIDs(Triangle triangle, ref List<int> idList)
        {
            var boundingRect = triangle.GetBoundingBox();
            var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
            var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

            for (int j = topLeft.y; j <= bottomRight.y; j++)
            {
                for (int i = topLeft.x; i <= bottomRight.x; i++)
                {
                    int id = GetCellId(i, j);
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(triangle)) idList.Add(id);
                }
            }
        }
        private void GetCellIDs(Quad quad, ref List<int> idList)
        {
            var boundingRect = quad.GetBoundingBox();
            var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
            var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

            for (int j = topLeft.y; j <= bottomRight.y; j++)
            {
                for (int i = topLeft.x; i <= bottomRight.x; i++)
                {
                    int id = GetCellId(i, j);
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(quad)) idList.Add(id);
                }
            }
        }
        private void GetCellIDs(Circle circle, ref List<int> idList)
        {
            var boundingRect = circle.GetBoundingBox();
            var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
            var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

            for (int j = topLeft.y; j <= bottomRight.y; j++)
            {
                for (int i = topLeft.x; i <= bottomRight.x; i++)
                {
                    int id = GetCellId(i, j);
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(circle)) idList.Add(id);
                }
            }
        }
        private void GetCellIDs(Rect rect, ref List<int> idList)
        {
            var topLeft = GetCellCoordinate(rect.X, rect.Y);
            var bottomRight = GetCellCoordinate(rect.X + rect.Width, rect.Y + rect.Height);

            for (int j = topLeft.y; j <= bottomRight.y; j++)
            {
                for (int i = topLeft.x; i <= bottomRight.x; i++)
                {
                    int id = GetCellId(i, j);
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(rect)) idList.Add(id);
                }
            }
        }
        private void GetCellIDs(Polygon poly, ref List<int> idList)
        {
            var boundingRect = poly.GetBoundingBox();
            var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
            var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

            for (int j = topLeft.y; j <= bottomRight.y; j++)
            {
                for (int i = topLeft.x; i <= bottomRight.x; i++)
                {
                    int id = GetCellId(i, j);
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(poly)) idList.Add(id);
                }
            }
        }
        private void GetCellIDs(Polyline polyLine, ref List<int> idList)
        {
            var boundingRect = polyLine.GetBoundingBox();
            var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
            var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

            for (int j = topLeft.y; j <= bottomRight.y; j++)
            {
                for (int i = topLeft.x; i <= bottomRight.x; i++)
                {
                    int id = GetCellId(i, j);
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(polyLine)) idList.Add(id);
                }
            }
        }
        private void GetCellIDs(Collider collider, ref List<int> idList)
        {
            if (!collider.Enabled) return;
            
            switch (collider.GetShapeType())
            {
                case ShapeType.Circle: GetCellIDs(collider.GetCircleShape(), ref idList); 
                    break;
                case ShapeType.Segment: GetCellIDs(collider.GetSegmentShape(), ref idList); 
                    break;
                case ShapeType.Triangle: GetCellIDs(collider.GetTriangleShape(), ref idList); 
                    break;
                case ShapeType.Rect: GetCellIDs(collider.GetRectShape(), ref idList); 
                    break;
                case ShapeType.Quad: GetCellIDs(collider.GetQuadShape(), ref idList); 
                    break;
                case ShapeType.Poly: GetCellIDs(collider.GetPolygonShape(), ref idList); 
                    break;
                case ShapeType.PolyLine: GetCellIDs(collider.GetPolylineShape(), ref idList); 
                    break;
            }
        }

        // private void GetCellIDs(CollisionBody collidable, ref List<int> idList)
        // {
        //     if (!collidable.HasColliders) return;
        //
        //     foreach (var collider in collidable.Colliders)
        //     {
        //         GetCellIDs(collider, ref idList);
        //     }
        // }
        
        
        private void SetSpacing()
        {
            SpacingX = Bounds.Width / Cols;
            SpacingY = Bounds.Height / Rows;
        }
        #endregion
    }
}


//
// /// <summary>
// /// Get all collision partners for specified collidable without using the mask
// /// </summary>
// /// <param name="collidable"></param>
// /// <param name="result"></param>
//
// //simplify & rework all of this -> no mask filtering, just returns the buckets for the cell ids that are 
// //stored in register or if not, calculated 
// public void GetRegisteredObjects(ICollidable collidable, ref HashSet<ICollidable> result)
// {
//     if (!register.ContainsKey(collidable)) return;
//     CollectUniqueObjects(register[collidable], collidable.GetCollisionMask(), ref result);
// }
//
// public void GetUniqueObjects(ICollidable collidable, ref HashSet<ICollidable> result)
// {
//     if (register.ContainsKey(collidable))
//     {
//         CollectUniqueObjects(register[collidable], collidable.GetCollisionMask(), ref result);
//     }
//
//     GetUniqueObjects(collidable.GetCollider(), collidable.GetCollisionMask(), ref result);
// }
// public void GetUniqueObjects(ICollider collider, BitFlag mask, ref HashSet<ICollidable> result) => GetUniqueObjects(collider.GetShape(), mask, ref result);
// public void GetUniqueObjects<T>(T shape, BitFlag mask, ref HashSet<ICollidable> result) where T : IShape
// {
//     List<int> bucketIds = new();
//     GetCellIDs(shape, ref bucketIds);
//     
//     if (bucketIds.Count <= 0) return;
//     CollectUniqueObjects(bucketIds, mask, ref result);
// }
//
// //---------------------------------
//

//  
//  /// <summary>
//  /// Get all the collision partners for the specified collidable and filter the result it by the collision mask
//  /// </summary>
//  /// <param name="collidable"></param>
//  /// <param name="result"></param>
//  public void GetObjectsFiltered(ICollidable collidable, ref HashSet<ICollidable> result)
//  {
//      if (register.ContainsKey(collidable))
//      {
//          CollectUniqueObjects(register[collidable], collidable.GetCollisionMask(), ref result);
//      }
//
//      GetObjectsFiltered(collidable.GetCollider(), collidable.GetCollisionMask(), ref result);
//  }
// /// <summary>
// /// Get all the collision partners for the specified collider and filter the result it by the collision mask
// /// </summary>
// /// <param name="collider"></param>
// /// <param name="mask"></param>
// /// <param name="result"></param>
//  public void GetObjectsFiltered(ICollider collider, BitFlag mask, ref HashSet<ICollidable> result) => GetObjectsFiltered(collider.GetShape(), mask, ref result);
//  /// <summary>
//  /// Get all the collision partners for the specified shape and filter the result it by the collision mask
//  /// </summary>
//  /// <param name="shape"></param>
//  /// <param name="mask"></param>
//  /// <param name="result"></param>
//  /// <typeparam name="T"></typeparam>
//  public void GetObjectsFiltered<T>(T shape, BitFlag mask, ref HashSet<ICollidable> result) where T : IShape
// {
//     List<int> bucketIds = new();
//     GetCellIDs(shape, ref bucketIds);
//     
//     if (bucketIds.Count <= 0) return;
//     CollectUniqueObjectsFiltered(bucketIds, mask, ref result);
// }
        

// private void CollectUniqueObjects(List<int> bucketIds, BitFlag mask, ref HashSet<ICollidable> result)
// {
//     if (bucketIds.Count <= 0) return;
//     foreach (var id in bucketIds)
//     {
//         var bucket = buckets[id];
//         if(bucket.Collidables != null && bucket.HasLayer(mask))
//             result.UnionWith(bucket.Collidables);
//     }
// }
// private void CollectUniqueObjectsFiltered(List<int> bucketIds, BitFlag mask, ref HashSet<ICollidable> result)
// {
//     if (bucketIds.Count <= 0 || mask.IsEmpty()) return;
//     foreach (var id in bucketIds)
//     {
//         var bucket = buckets[id];
//         var bucketObjects = bucket.GetObjects(mask);
//         if (bucketObjects == null || bucketObjects.Count <= 0) continue;
//         result.UnionWith(bucketObjects);
//     }
//
//     // if (allObjects == null || allObjects.Count <= 0) return;
//     // return new HashSet<ICollidable>(allObjects).ToList();
// }