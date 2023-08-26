using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Lib;

namespace ShapeEngine.Core
{

    
    
    public class SpatialHashBucketInfo
    {
        public bool Valid { get; private set; }
        public Dictionary<uint, HashSet<ICollidable>> Layers;
        public HashSet<ICollidable> Active;
        public SpatialHashBucketInfo(HashSet<ICollidable> active, Dictionary<uint, HashSet<ICollidable>> layers)
        {
            this.Active = active;
            this.Layers = layers;
            this.Valid = true;
        }
        public SpatialHashBucketInfo()
        {
            this.Layers = new();
            this.Active = new();
            this.Valid = false;
        }

        public List<ICollidable> GetOthers(ICollidable collidable)
        {
            List<ICollidable> others = new();
            var mask = collidable.GetCollisionMask();
            if (mask.Length <= 0) return new();

            foreach (var layer in mask)
            {
                if (Layers.ContainsKey(layer)) others.AddRange(Layers[layer]);
            }
            return others;
        }

    }

    public class SpatialHashBucket
    {
        public Dictionary<uint, HashSet<ICollidable>> Layers = new();
        public HashSet<ICollidable> Collidables = new();
        public int Count { get { return Collidables.Count; } }

        public void Clear()
        {
            Layers.Clear();
            Collidables.Clear();
        }
        public void Add(ICollidable collidable)
        {
            var layer = collidable.GetCollisionLayer();
            if (Layers.ContainsKey(layer)) Layers[layer].Add(collidable);
            else Layers.Add(layer, new() { collidable });
            Collidables.Add(collidable);
        }
        public void Remove(ICollidable collidable)
        {
            var layer = collidable.GetCollisionLayer();
            if (Layers.ContainsKey(layer)) Layers[layer].Remove(collidable);
            Collidables.Remove(collidable);
        }

        public HashSet<ICollidable> GetObjects(params uint[] mask)
        {
            HashSet<ICollidable> objects = new();
            foreach (var entry in mask)
            {
                if (Layers.ContainsKey(entry)) objects.UnionWith(Layers[entry]);
            }
            return objects;
        }
        
        public SpatialHashBucketInfo GetBucketInfo()
        {
            HashSet<ICollidable> active = new();
            List<uint> availableLayers = Layers.Keys.ToList();
            foreach (var collidable in Collidables)
            {
                if (collidable.GetCollider().ComputeCollision)
                {
                    var mask = collidable.GetCollisionMask();
                    foreach (var entry in mask)
                    {
                        if (availableLayers.Contains(entry))
                        {
                            active.Add(collidable);
                            break;
                        }
                    }

                }
            }

            return new(active, Layers);
        }
        
    }

    public class SpatialHash : IBounds
    {
        public Rect Bounds { get; private set; }
        public float SpacingX { get; private set; }
        public float SpacingY { get; private set; }
        public int BucketCount { get; private set; }
        public int Rows { get; private set; }
        public int Cols { get; private set; }

        //private bool isClear = true;
        //public bool IsClear { get { return isClear; } }

        
        private SpatialHashBucket[] buckets;

        private bool boundsResizeQueued = false;
        private Rect newBounds = new();

        public SpatialHash(float x, float y, float w, float h, int rows, int cols)
        {
            this.Bounds = new(x, y, w, h);
            this.Rows = rows;
            this.Cols = cols;
            this.SetSpacing();
            this.BucketCount = rows * cols;
            this.buckets = new SpatialHashBucket[this.BucketCount];
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
            this.buckets = new SpatialHashBucket[this.BucketCount];
            for (int i = 0; i < BucketCount; i++)
            {
                this.buckets[i] = new();
            }
        }
        


        /// <summary>
        /// Resize the bounds. Clears the spatial hash!
        /// </summary>
        /// <param name="newBounds"></param>
        public void ResizeBounds(Rect newBounds) 
        {
            //if(!isClear) Clear();
            this.newBounds = newBounds;
            //Bounds = newBounds;
            //SetSpacing();
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
            buckets = new SpatialHashBucket[BucketCount];
            for (int i = 0; i < BucketCount; i++)
            {
                buckets[i] = new();
            }
        }


        public (int x, int y) GetCoordinatesGrid(int index)
        {
            return (index % Cols, index / Cols);
            //return new Tuple<int x, int y>(index % cols, index / cols);
        }
        public Vector2 GetCoordinatesWorld(int index)
        {
            var coord = GetCoordinatesGrid(index);
            return new Vector2(coord.x * SpacingX, coord.y * SpacingY);
        }
        public Rect GetCellRectangle(int x, int y)
        {
            return new Rect(Bounds.x + x * SpacingX, Bounds.y + y * SpacingY, SpacingX, SpacingY);
        }
        public Rect GetCellRectangle(int index)
        {
            return GetCellRectangle(index % Cols, index / Cols);
        }
        public int GetCellID(int x, int y)
        {
            return x + y * Cols;
        }
        public int GetCellID(float x, float y)
        {
            int xi = Math.Clamp((int)Math.Floor((x - Bounds.x) / SpacingX), 0, Cols - 1);
            int yi = Math.Clamp((int)Math.Floor((y - Bounds.y) / SpacingY), 0, Rows - 1);
            return GetCellID(xi, yi);
        }
        public (int x, int y) GetCellCoordinate(float x, float y)
        {
            int xi = Math.Clamp((int)Math.Floor((x - Bounds.x) / SpacingX), 0, Cols - 1);
            int yi = Math.Clamp((int)Math.Floor((y - Bounds.y) / SpacingY), 0, Rows - 1);
            return (xi, yi);
        }
        public List<int> GetCellIDs(IShape shape)
        {
            Rect boundingRect = shape.GetBoundingBox();
            List<int> hashes = new List<int>();
            (int x, int y) topLeft = GetCellCoordinate(boundingRect.x, boundingRect.y);
            (int x, int y) bottomRight = GetCellCoordinate(boundingRect.x + boundingRect.width, boundingRect.y + boundingRect.height);

            for (int j = topLeft.y; j <= bottomRight.y; j++)
            {
                for (int i = topLeft.x; i <= bottomRight.x; i++)
                {
                    int id = GetCellID(i, j);
                    if (SGeometry.Overlap(GetCellRectangle(id), shape))
                        hashes.Add(id);
                }
            }
            return hashes;
        }


        public void AddRange(IEnumerable<ICollidable> colliders)
        {
            foreach (var collider in colliders)
            {
                Add(collider);
            }
        }
        public void Add(ICollidable collidable)
        {
            if (!collidable.GetCollider().Enabled) return;
            //isClear = false;
            var hashes = GetCellIDs(collidable.GetCollider().GetShape());
            foreach (int hash in hashes)
            {
                buckets[hash].Add(collidable);
            }
        }


        public void Clear()
        {
            //if(isClear) return;

            for (int i = 0; i < BucketCount; i++)
            {
                buckets[i].Clear();
            }

            if (boundsResizeQueued)
            {
                boundsResizeQueued = false;
                Bounds = newBounds;
                SetSpacing();
            }
            //isClear = true;
        }
        public void Close()
        {
            Clear();
            buckets = new SpatialHashBucket[0];  //new HashSet<ICollidable>[0];
        }
        

        public SpatialHashBucketInfo GetBucketInfo(int bucketIndex)
        {
            if (bucketIndex < 0 || bucketIndex >= BucketCount) return new();
            var bucket = buckets[bucketIndex];
            return bucket.GetBucketInfo();
        }


        public List<ICollidable> GetObjects(ICollidable collidable)
        {
            return GetObjects(collidable.GetCollider(), collidable.GetCollisionMask());
        }
        public List<ICollidable> GetObjects(ICollider collider, params uint[] mask)
        {
            return GetObjects(collider.GetShape(), mask);
        }
        public List<ICollidable> GetObjects(IShape shape, params uint[] mask)
        {
            HashSet<ICollidable> uniqueObjects = new();

            var bucketIDs = GetCellIDs(shape);
            foreach (var id in bucketIDs)
            {
                var bucket = buckets[id];
                uniqueObjects.UnionWith(bucket.GetObjects(mask));
            }

            return uniqueObjects.ToList();
        }


        public void DebugDraw(Raylib_CsLo.Color border, Raylib_CsLo.Color fill)
        {
            for (int i = 0; i < BucketCount; i++)
            {
                var coords = GetCoordinatesGrid(i);
                var rect = new Rectangle(Bounds.x + coords.x * SpacingX, Bounds.y + coords.y * SpacingY, SpacingX, SpacingY);

                Raylib.DrawRectangleLinesEx(rect, 1, border);
                int id = GetCellID(coords.x, coords.y);
                if (buckets[id].Count > 0)
                {
                    //DrawRectangleLinesEx(rect, 1, full);
                    Raylib.DrawRectangleRec(rect, fill);
                }

            }
        }


        private void SetSpacing()
        {
            SpacingX = Bounds.width / Cols;
            SpacingY = Bounds.height / Rows;
        }
    }
}
