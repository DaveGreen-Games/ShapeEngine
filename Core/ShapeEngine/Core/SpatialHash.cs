using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Lib;

namespace ShapeEngine.Core
{
    public class BucketInfo
    {
        public bool Valid { get; private set; }
        public Dictionary<uint, HashSet<ICollidable>> Layers;
        public HashSet<ICollidable> Active;
        public BucketInfo(HashSet<ICollidable> active, Dictionary<uint, HashSet<ICollidable>> layers)
        {
            this.Active = active;
            this.Layers = layers;
            this.Valid = true;
        }
        public BucketInfo()
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
   
    public class SpatialHash
    {
        internal class Bucket
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
            public BucketInfo GetBucketInfo()
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
      
        private float origin_x;
        private float origin_y;

        private float width;
        private float height;

        private float spacing_x;
        private float spacing_y;

        private int bucketCount = 0;
        private int rows = 0;
        private int cols = 0;

        //private List<ICollidable>[] buckets; //change to hashset<ICollidable>[]
        //figure out something better than spacing (is not always divisable by screen size)

        private Bucket[] buckets;

        public SpatialHash(float x, float y, float w, float h, int rows, int cols)
        {
            origin_x = x;
            origin_y = y;

            width = w;
            height = h;

            spacing_x = width / cols;
            spacing_y = height / rows;


            this.rows = rows;// (int)Math.Floor(height / spacing);
            this.cols = cols;// (int)Math.Floor(width / spacing); 
            bucketCount = rows * cols;
            //buckets = new HashSet<ICollidable>[bucketCount];
            buckets = new Bucket[bucketCount];
            for (int i = 0; i < bucketCount; i++)
            {
                buckets[i] = new(); // new HashSet<ICollidable>();
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


            this.rows = rows;// (int)Math.Floor(height / spacing);
            this.cols = cols;// (int)Math.Floor(width / spacing); 
            bucketCount = rows * cols;
            //buckets = new HashSet<ICollidable>[bucketCount];
            buckets = new Bucket[bucketCount];
            for (int i = 0; i < bucketCount; i++)
            {
                buckets[i] = new(); // new HashSet<ICollidable>();
            }
        }
        
        public int GetRows() { return rows; }
        public int GetCols() { return cols; }
        public int GetBucketCount() { return bucketCount; }

        public SpatialHash Resize(Rect newBounds) { return new(newBounds, rows, cols); }

        public void DebugDrawGrid(Raylib_CsLo.Color border, Raylib_CsLo.Color fill)
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

        
        //public List<int> GetCellIDs(ICollider col)
        //{
        //    return GetCellIDs(col.GetShape());
        //
        //    //Rect boundingRect = col.GetShape().GetBoundingBox();
        //    //List<int> hashes = new List<int>();
        //    //(int x, int y) topLeft = GetCellCoordinate(boundingRect.x, boundingRect.y);
        //    //(int x, int y) bottomRight = GetCellCoordinate(boundingRect.x + boundingRect.width, boundingRect.y + boundingRect.height);
        //    //
        //    //for (int j = topLeft.y; j <= bottomRight.y; j++)
        //    //{
        //    //    for (int i = topLeft.x; i <= bottomRight.x; i++)
        //    //    {
        //    //        int id = GetCellID(i, j);
        //    //
        //    //        if (!hashes.Contains(id))
        //    //        {
        //    //            if (SGeometry.Overlap(GetCellRectangle(id), col))
        //    //                hashes.Add(id);
        //    //            //if (SGeometry.OverlapShape(GetCellRectangle(id), boundingRect))
        //    //            //{
        //    //            //    
        //    //            //}
        //    //        }
        //    //    }
        //    //}
        //    //return hashes;
        //}
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
            foreach (ICollidable collider in colliders)
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

        /*
        public void Remove(ICollidable collider)
        {
            var hashes = GetCellIDs(collider.GetCollider().GetShape());
            foreach (int hash in hashes)
            {
                buckets[hash].Remove(collider);
            }
        }
        */
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
            buckets = new Bucket[0];  //new HashSet<ICollidable>[0];
        }

        public BucketInfo GetBucketInfo(int bucketIndex)
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


        /*
        public List<ICollidable> GetObjects(IShape shape)
        {
            HashSet<ICollidable> result = new();
            var hashes = GetCellIDs(shape);
            foreach (int hash in hashes)
            {
                foreach (var obj in buckets[hash].Collidables)
                {
                    //var collider = obj.GetCollider();
                    //if (!collider.Enabled) continue;
                    result.Add(obj);
                }
            }
            return result.ToList();
        }
        public List<ICollidable> GetObjects(params IShape[] shapes)
        {
            HashSet<ICollidable> result = new();
            foreach (var shape in shapes)
            {
                var hashes = GetCellIDs(shape);
                foreach (int hash in hashes)
                {
                    foreach (var obj in buckets[hash].Collidables)
                    {
                        result.Add(obj);
                    }
                }
            }
            return result.ToList();
            
        }
        public List<ICollidable> GetObjects(ICollider col, params IShape[] shapes)
        {
            HashSet<ICollidable> uniqueObjects = new();
            foreach (var shape in shapes)
            {
                var hashes = GetCellIDs(shape);
                foreach (int hash in hashes)
                {
                    foreach (var obj in buckets[hash].Collidables)
                    {
                        //var collider = obj.GetCollider();
                        if (obj.GetCollider() == col) continue;
                        uniqueObjects.Add(obj);
                    }
                }
            }
            return uniqueObjects.ToList();
        }
        public List<ICollidable> GetObjects(ICollider col)
        {
            return GetObjects(col, col.GetShape());
        }
        public List<ICollidable> GetObjects(ICollidable collider)
        {
            return GetObjects(collider.GetCollider());
        }
        */

        /* old versions
        public List<ICollidable> GetObjects(IShape shape)
        {
            HashSet<ICollidable> result = new();
            var hashes = GetCellIDs(shape);
            foreach (int hash in hashes)
            {
                foreach (var obj in buckets[hash]) result.Add(obj);
            }
            return result.ToList();
        }
        public List<ICollidable> GetObjects(ICollider col)
        {
            HashSet<ICollidable> uniqueObjects = new();
            var hashes = GetCellIDs(col);
            foreach (int hash in hashes)
            {
                foreach (var obj in buckets[hash])
                {
                    if (obj.GetCollider() == col) continue;
                    uniqueObjects.Add(obj);
                }
            }
            return uniqueObjects.ToList();
        }
        public List<ICollidable> GetObjects(ICollidable collider)
        {
            List<ICollidable> objects = new();
            var hashes = GetCellIDs(collider.GetCollider());
            foreach (int hash in hashes)
            {
                objects.AddRange(buckets[hash]);
            }
            objects = objects.Distinct().ToList();
            objects.Remove(collider);
            return objects;
        }
        */

        /*public int GetNotEmptyCount()
        {
            int count = 0;
            for (int i = 0; i < bucket_size; i++)
            {
                if (bucketsDynamic[i].Count > 0)
                {
                    count++;
                }
            }
            return count;
        }*/
    }
}
