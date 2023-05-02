using System.Numerics;
using Raylib_CsLo;
using ShapeLib;

namespace ShapeCore
{
    public class SpatialHash
    {
        private float origin_x;
        private float origin_y;

        private float width;
        private float height;

        private float spacing_x;
        private float spacing_y;

        private int bucket_size = 0;
        private int rows = 0;
        private int cols = 0;

        private List<ICollidable>[] buckets;

        //figure out something better than spacing (is not always divisable by screen size)
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
            bucket_size = rows * cols;
            buckets = new List<ICollidable>[bucket_size];
            for (int i = 0; i < bucket_size; i++)
            {
                buckets[i] = new List<ICollidable>();
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
            bucket_size = rows * cols;
            buckets = new List<ICollidable>[bucket_size];
            for (int i = 0; i < bucket_size; i++)
            {
                buckets[i] = new List<ICollidable>();
            }
        }
        
        public int GetRows() { return rows; }
        public int GetCols() { return cols; }
        public int GetBucketSize() { return bucket_size; }

        public SpatialHash Resize(Rect newBounds) { return new(newBounds, rows, cols); }

        public void DebugDrawGrid(Color border, Color fill)
        {
            for (int i = 0; i < bucket_size; i++)
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

        //TODO (DAVID):
        //get all the cells in the bounding rect area and then make overlap test between the shape and each cell in the bounding area
        //only make those checks if there are more than 1 cell
        public List<int> GetCellIDs(ICollider shape)
        {
            Rect boundingRect = shape.GetShape().GetBoundingBox();
            List<int> hashes = new List<int>();
            (int x, int y) topLeft = GetCellCoordinate(boundingRect.x, boundingRect.y);
            (int x, int y) bottomRight = GetCellCoordinate(boundingRect.x + boundingRect.width, boundingRect.y + boundingRect.height);

            //if(boundingRect.width <= 0f || boundingRect.height <= 0f)
            ////if (topLeft.x == bottomRight.x && topLeft.y == bottomRight.y)
            //{
            //    hashes.Add(GetCellID(topLeft.x, topLeft.y));
            //    return hashes;
            //}


            for (int j = topLeft.y; j <= bottomRight.y; j++)
            {
                for (int i = topLeft.x; i <= bottomRight.x; i++)
                {
                    int id = GetCellID(i, j);

                    if (!hashes.Contains(id)) //do i still need this check?
                    {
                        if(SGeometry.OverlapShape(GetCellRectangle(id), boundingRect))
                        {
                            if(SGeometry.Overlap(GetCellRectangle(id), shape))
                                hashes.Add(id);
                        }
                    }
                }
            }

            /*
            int hash_a = GetCellID(boundingRect.x, boundingRect.y);
            int hash_b = GetCellID(boundingRect.x + boundingRect.width, boundingRect.y);
            int hash_c = GetCellID(boundingRect.x + boundingRect.width, boundingRect.y + boundingRect.height);
            int hash_d = GetCellID(boundingRect.x, boundingRect.y + boundingRect.height);

            hashes.Add(hash_a);
            //hashes.Add(hash_center);
            //if (!hashes.Contains(hash_a)) { hashes.Add(hash_a); }
            if (!hashes.Contains(hash_b)) { hashes.Add(hash_b); }
            if (!hashes.Contains(hash_c)) { hashes.Add(hash_c); }
            if (!hashes.Contains(hash_d)) { hashes.Add(hash_d); }
            */
            return hashes;
        }


        public void AddRange(List<ICollidable> colliders)
        {
            foreach (ICollidable collider in colliders)
            {
                Add(collider);
            }
        }
        public void Add(ICollidable collider)
        {
            var hashes = GetCellIDs(collider.GetCollider());
            foreach (int hash in hashes)
            {
                if (!buckets[hash].Contains(collider)) { buckets[hash].Add(collider); }
            }
        }

        public void Remove(ICollidable collider)
        {
            var hashes = GetCellIDs(collider.GetCollider());
            foreach (int hash in hashes)
            {
                buckets[hash].Remove(collider);
            }
        }
        public void Clear()
        {
            for (int i = 0; i < bucket_size; i++)
            {
                buckets[i].Clear();
            }
        }
        public void Close()
        {
            Clear();
            buckets = new List<ICollidable>[0];
        }

        //public List<ICollidable> GetObjects(Collider shape, bool dynamicBoundingBox = false)
        //{
        //    //HashSet<ICollidable> uniqueObjects = new();
        //    List<ICollidable> objects = new List<ICollidable>();
        //    var hashes = GetCellIDs(shape, dynamicBoundingBox);
        //    foreach (int hash in hashes)
        //    {
        //        objects.AddRange(bucketsDynamic[hash]);
        //        objects.AddRange(bucketsStatic[hash]);
        //    }
        //    
        //    return objects.Distinct().ToList();
        //}

        public List<ICollidable> GetObjects(ICollider shape)
        {
            HashSet<ICollidable> uniqueObjects = new();
            var hashes = GetCellIDs(shape);
            foreach (int hash in hashes)
            {
                foreach (var dyn in buckets[hash])
                {
                    if (dyn.GetCollider() == shape) continue;
                    uniqueObjects.Add(dyn);
                }
            }
            return uniqueObjects.ToList();
        }

        //public List<ICollidable> GetObjects2(ICollidable collider)
        //{
        //    HashSet<ICollidable> uniqueObjects = new();
        //    var hashes = GetCellIDs(collider.GetCollider(), collider.HasDynamicBoundingBox());
        //    foreach (int hash in hashes)
        //    {
        //        foreach (var dyn in bucketsDynamic[hash])
        //        {
        //            uniqueObjects.Add(dyn);
        //        }
        //        foreach (var sta in bucketsStatic[hash])
        //        {
        //            uniqueObjects.Add(sta);
        //        }
        //    }
        //    uniqueObjects.Remove(collider);
        //    return uniqueObjects.ToList();
        //}
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
