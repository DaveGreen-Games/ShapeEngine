using System.Numerics;
using System.Reflection.Metadata.Ecma335;
// using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Lib;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Collision
{
    
    public class SpatialHash : IBounds
    {
        public class Bucket : HashSet<ICollidable>
        {
            //maybe also a function that filters by bounding box overlap (only returns overlapping objects???)
            public Bucket? FilterObjects(BitFlag mask)
            {
                if (Count <= 0 || mask.IsEmpty()) return null;
                
                Bucket? objects = null;
                foreach (var collidable in this)
                {
                    if (mask.Has(collidable.GetCollisionLayer()))
                    {
                        objects ??= new();
                        objects.Add(collidable);
                    }
                }
                return objects;
            }
            public Bucket? Copy() => Count <= 0 ? null : (Bucket)this.ToHashSet();
        }
        // private class SpatialHashBucket
        // {
        //     // public Dictionary<uint, HashSet<ICollidable>> Layers = new();
        //     public HashSet<ICollidable>? Collidables = null;
        //
        //     private Dictionary<uint, int>? usedLayers = null;
        //     // public List<uint> UsedLayers = new();
        //     public int Count { get { return Collidables != null ? Collidables.Count : 0; } }
        //
        //     public void Clear()
        //     {
        //         Collidables?.Clear();
        //         usedLayers?.Clear();
        //     }
        //     public void Add(ICollidable collidable)
        //     {
        //         Collidables ??= new();
        //         usedLayers ??= new();
        //         
        //         Collidables.Add(collidable);
        //         
        //         var layer = collidable.GetCollisionLayer();
        //         if (usedLayers.ContainsKey(layer)) usedLayers[layer] += 1;
        //         else usedLayers.Add(layer, 1);
        //     }
        //     public void Remove(ICollidable collidable)
        //     {
        //         if (Collidables == null || Collidables.Count <= 0) return;
        //         
        //         var layer = collidable.GetCollisionLayer();
        //         bool removed = Collidables.Remove(collidable);
        //
        //         if (!removed || usedLayers == null) return;
        //         if (usedLayers.ContainsKey(layer))
        //         {
        //             if (usedLayers[layer] <= 1) usedLayers.Remove(layer);
        //             else usedLayers[layer] -= 1;
        //         }
        //     }
        //
        //     public bool HasLayer(BitFlag mask)
        //     {
        //         if (usedLayers == null) return false;
        //         
        //         foreach (var layer in usedLayers.Keys)
        //         {
        //             if (mask.Has(layer))
        //             {
        //                 return true;
        //             }
        //         }
        //
        //         return false;
        //     }
        //     public List<ICollidable>? GetObjects(BitFlag mask)
        //     {
        //         if (Collidables == null || Collidables.Count <= 0 || usedLayers == null || usedLayers.Count <= 0 || mask.IsEmpty()) return null;
        //         
        //         if (!HasLayer(mask)) return null;
        //
        //         List<ICollidable>? objects = null;
        //         foreach (var collidable in Collidables)
        //         {
        //             if (mask.Has(collidable.GetCollisionLayer()))
        //             {
        //                 objects ??= new();
        //                 objects.Add(collidable);
        //             }
        //         }
        //         return objects;
        //     }
        //
        //     public List<ICollidable>? GetAllObjectsList()
        //     {
        //         if (Collidables == null || Collidables.Count <= 0) return null;
        //         return Collidables.ToList();
        //     }
        //
        //     // public SpatialHashBucketInfo GetBucketInfo()
        //     // {
        //     //     HashSet<ICollidable> active = new();
        //     //     // List<uint> availableLayers = Layers.Keys.ToList();
        //     //     foreach (var collidable in Collidables)
        //     //     {
        //     //         if (collidable.GetCollider().ComputeCollision) active.Add(collidable);
        //     //         // {
        //     //         //     var mask = collidable.GetCollisionMask();
        //     //         //     foreach (var entry in mask)
        //     //         //     {
        //     //         //         if (Layers.ContainsKey(entry))
        //     //         //         {
        //     //         //             active.Add(collidable);
        //     //         //             break;
        //     //         //         }
        //     //         //     }
        //     //         //
        //     //         // }
        //     //     }
        //     //
        //     //     return new(active, Layers);
        //     // }
        //     //
        // }

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
        private readonly Dictionary<ICollidable, List<int>> register = new();
        private readonly HashSet<ICollidable> registerKeys = new();
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
        public void Fill(IEnumerable<ICollidable> collidables)
        {
            Clear();

            foreach (var collidable in collidables)
            {
                var collider = collidable.GetCollider();
                if (collider.Enabled)
                {
                    Add(collidable); //add is the problem 
                    
                }
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

        public void GetRegisteredCollisionCandidateBuckets(ICollidable collidable, ref List<Bucket> candidateBuckets)
        {
            if (!register.TryGetValue(collidable, out var bucketIds)) return;
            if (bucketIds.Count <= 0) return;
            foreach (var id in bucketIds)
            {
                var bucket = buckets[id];
                if(bucket.Count > 0) candidateBuckets.Add(buckets[id]);
            }
        }
        
        public void GetCandidateBuckets(ICollidable collidable, ref List<Bucket> candidateBuckets)
        {
            if (register.TryGetValue(collidable, out var bucketIds))
            {
                if (bucketIds.Count <= 0) return;
                foreach (var id in bucketIds)
                {
                    var bucket = buckets[id];
                    if(bucket.Count > 0) candidateBuckets.Add(buckets[id]);
                }
            }

            GetCandidateBuckets(collidable.GetCollider(), ref candidateBuckets);
        }
        public void GetCandidateBuckets(ICollider collider, ref List<Bucket> candidateBuckets) => GetCandidateBuckets(collider.GetShape(), ref candidateBuckets);
        public void GetCandidateBuckets<T>(T shape, ref List<Bucket> candidateBuckets) where T : IShape
        {
            List<int> bucketIds = new();
            GetCellIDs(shape, ref bucketIds);
            
            if (bucketIds.Count <= 0) return;
            foreach (var id in bucketIds)
            {
                var bucket = buckets[id];
                if(bucket.Count > 0) candidateBuckets.Add(buckets[id]);
            }
        }

        public void GetUniqueObjects(ICollidable collidable, ref HashSet<ICollidable> result)
        {
            if (register.TryGetValue(collidable, out var bucketIds))
            {
                if (bucketIds.Count <= 0) return;
                foreach (var id in bucketIds)
                {
                    var bucket = buckets[id];
                    if(bucket.Count > 0) result.UnionWith(bucket);
                }

                return;
            }

            GetUniqueObjects(collidable.GetCollider(), ref result);
        }
        public void GetUniqueObjects(ICollider collider, ref HashSet<ICollidable> result) => GetUniqueObjects(collider.GetShape(), ref result);
        public void GetUniqueObjects<T>(T shape, ref HashSet<ICollidable> result) where T : IShape
        {
            List<int> bucketIds = new();
            GetCellIDs(shape, ref bucketIds);
            
            if (bucketIds.Count <= 0) return;
            foreach (var id in bucketIds)
            {
                var bucket = buckets[id];
                if(bucket.Count > 0) result.UnionWith(bucket);
            }
        }

       
        public void DebugDraw(ColorRgba border, ColorRgba fill)
        {
            for (int i = 0; i < BucketCount; i++)
            {
                var coords = GetCoordinatesGrid(i);
                var rect = new Rect(Bounds.X + coords.x * SpacingX, Bounds.Y + coords.y * SpacingY, SpacingX, SpacingY);
                rect.DrawLines(1f, border);
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
        private void Add(ICollidable collidable)//38% memory problem
        {
            List<int> ids;
            if (register.ContainsKey(collidable)) ids = register[collidable];
            else
            {
                ids = new List<int>();
                register.Add(collidable, ids);
                
            }
            GetCellIDs(collidable.GetCollider().GetShape(), ref ids);
            // var hashes = GetCellIDs(collidable.GetCollider().GetShape()); //GetCellID 20% problem
            if (ids.Count <= 0) return;
            registerKeys.Remove(collidable);
            // register[collidable] = hashes;
            foreach (int hash in ids)
            {
                buckets[hash].Add(collidable); //Add 16% problem
            }
        }
        private void CleanRegister()
        {
            foreach (var collidable in registerKeys)
            {
                register.Remove(collidable);
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
        private void GetCellIDs<T>(T shape, ref List<int> idList) where T : IShape
        {
            var boundingRect = shape.GetBoundingBox();
            // List<int> hashes =  new List<int>();
            // cellIds.Clear();
            idList.Clear();
            var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
            var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

            for (int j = topLeft.y; j <= bottomRight.y; j++)
            {
                for (int i = topLeft.x; i <= bottomRight.x; i++)
                {
                    int id = GetCellId(i, j);
                    if (ShapeGeometry.Overlap(GetCellRectangle(id), shape)) //overlap 12% problem (probably because of boxing)
                    {
                        idList.Add(id);//6% problem
                        // cellIds.Add(id);//6% problem
                    }

                    
                }
            }
            //return hashes;
        }
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