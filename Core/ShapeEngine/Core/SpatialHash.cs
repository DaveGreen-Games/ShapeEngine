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

    public class SpatialHash
    {
        
        //rework this?
        private float origin_x;
        private float origin_y;

        private float width;
        private float height;

        //Rect bounds;
        private float spacing_x;
        private float spacing_y;

        private int bucketCount = 0;
        private int rows = 0;
        private int cols = 0;

        private SpatialHashBucket[] buckets;

        public SpatialHash(float x, float y, float w, float h, int rows, int cols)
        {
            origin_x = x;
            origin_y = y;

            width = w;
            height = h;

            spacing_x = width / cols;
            spacing_y = height / rows;


            this.rows = rows;
            this.cols = cols;
            bucketCount = rows * cols;
            buckets = new SpatialHashBucket[bucketCount];
            for (int i = 0; i < bucketCount; i++)
            {
                buckets[i] = new();
            }
        }
        public SpatialHash(Rect bounds, int rows, int cols)
        {
            origin_x = bounds.x;
            origin_y = bounds.y;

            width = bounds.width;
            height = bounds.height;

            spacing_x = width / cols;
            spacing_y = height / rows;


            this.rows = rows;
            this.cols = cols; 
            bucketCount = rows * cols;
            buckets = new SpatialHashBucket[bucketCount];
            for (int i = 0; i < bucketCount; i++)
            {
                buckets[i] = new();
            }
        }
        
        public int GetRows() { return rows; }
        public int GetCols() { return cols; }
        public int GetBucketCount() { return bucketCount; }

        public SpatialHash Resize(Rect newBounds) { return new(newBounds, rows, cols); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colors">First color is used as border color, second color is used as fill color.</param>
        public void DebugDraw(Raylib_CsLo.Color border, Raylib_CsLo.Color fill)
        {
            for (int i = 0; i < bucketCount; i++)
            {
                var coords = GetCoordinatesGrid(i);
                var rect = new Rectangle(origin_x + coords.x * spacing_x, origin_y + coords.y * spacing_y, spacing_x, spacing_y);

                Raylib.DrawRectangleLinesEx(rect, 1, border);
                int id = GetCellID(coords.x, coords.y);
                if (buckets[id].Count > 0)
                {
                    //DrawRectangleLinesEx(rect, 1, full);
                    Raylib.DrawRectangleRec(rect, fill);
                }

            }
        }


        public (int x, int y) GetCoordinatesGrid(int index)
        {
            return (index % cols, index / cols);
            //return new Tuple<int x, int y>(index % cols, index / cols);
        }
        public Vector2 GetCoordinatesWorld(int index)
        {
            var coord = GetCoordinatesGrid(index);
            return new Vector2(coord.x * spacing_x, coord.y * spacing_y);
        }
        public Rect GetCellRectangle(int x, int y)
        {
            return new Rect(origin_x + x * spacing_x, origin_y + y * spacing_y, spacing_x, spacing_y);
        }
        public Rect GetCellRectangle(int index)
        {
            return GetCellRectangle(index % cols, index / cols);
        }
        public int GetCellID(int x, int y)
        {
            return x + y * cols;
        }
        public int GetCellID(float x, float y)
        {
            int xi = Math.Clamp((int)Math.Floor((x - origin_x) / spacing_x), 0, cols - 1);
            int yi = Math.Clamp((int)Math.Floor((y - origin_y) / spacing_y), 0, rows - 1);
            return GetCellID(xi, yi);
        }
        public (int x, int y) GetCellCoordinate(float x, float y)
        {
            int xi = Math.Clamp((int)Math.Floor((x - origin_x) / spacing_x), 0, cols - 1);
            int yi = Math.Clamp((int)Math.Floor((y - origin_y) / spacing_y), 0, rows - 1);
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
            var hashes = GetCellIDs(collidable.GetCollider().GetShape());
            foreach (int hash in hashes)
            {
                buckets[hash].Add(collidable);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < bucketCount; i++)
            {
                buckets[i].Clear();
            }
        }
        public void Close()
        {
            Clear();
            buckets = new SpatialHashBucket[0];  //new HashSet<ICollidable>[0];
        }

        
        public SpatialHashBucketInfo GetBucketInfo(int bucketIndex)
        {
            if (bucketIndex < 0 || bucketIndex >= bucketCount) return new();
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

    }
}
