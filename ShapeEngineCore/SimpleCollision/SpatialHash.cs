using Raylib_CsLo;
using ShapeEngineCore.Globals;
using System.Numerics;
//using System.Drawing;

namespace ShapeEngineCore.SimpleCollision
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

        //change that class to something else -> gameobject probably
        private List<ICollidable>[] bucketsDynamic;
        private List<ICollidable>[] bucketsStatic;

        //private Color debug_color_dark = new Color(200, 200, 200, 25);
        //private Color debug_color_light = new Color(125, 125, 125, 50);

        //figure out something better than spacing (is not always dividedable by screen size)
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
            bucketsDynamic = new List<ICollidable>[bucket_size];
            bucketsStatic = new List<ICollidable>[bucket_size];
            for (int i = 0; i < bucket_size; i++)
            {
                bucketsDynamic[i] = new List<ICollidable>();
                bucketsStatic[i] = new List<ICollidable>();
            }
        }

        public int GetRows() { return rows; }
        public int GetCols() { return cols; }
        public int getBucketSize()
        {
            return bucket_size;
        }

        public void DebugDrawGrid(Color border, Color fill)
        {
            for (int i = 0; i < bucket_size; i++)
            {
                var coords = GetCoordinatesGrid(i);
                var rect = new Rectangle(origin_x + coords.x * spacing_x, origin_y + coords.y * spacing_y, spacing_x, spacing_y);

                DrawRectangleLinesEx(rect, 1, border);
                int id = GetCellID(coords.x, coords.y);
                if (bucketsDynamic[id].Count > 0 || bucketsStatic[id].Count > 0)
                {
                    //DrawRectangleLinesEx(rect, 1, full);
                    DrawRectangleRec(rect, fill);
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

        public Rectangle GetCellRectangle(int x, int y)
        {
            return new Rectangle(origin_x + x * spacing_x, origin_y + y * spacing_y, spacing_x, spacing_y);
        }

        public Rectangle GetCellRectangle(int index)
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
        public List<int> GetCellIDs(Collider shape, bool dynamicBoundingBox = false)
        {
            Rectangle boundingRect = dynamicBoundingBox? shape.GetDynamicBoundingRect() : shape.GetBoundingRect();
            List<int> hashes = new List<int>();
            (int x, int y) topLeft = GetCellCoordinate(boundingRect.x, boundingRect.y);
            (int x, int y) bottomRight = GetCellCoordinate(boundingRect.x + boundingRect.width, boundingRect.y + boundingRect.height);

            if(boundingRect.width <= 0f || boundingRect.height <= 0f)
            //if (topLeft.x == bottomRight.x && topLeft.y == bottomRight.y)
            {
                hashes.Add(GetCellID(topLeft.x, topLeft.y));
                return hashes;
            }


            for (int j = topLeft.y; j <= bottomRight.y; j++)
            {
                for (int i = topLeft.x; i <= bottomRight.x; i++)
                {

                    int id = GetCellID(i, j);

                    if (!hashes.Contains(id)) //do i still need this check?
                    {
                        if(Overlap.OverlapRectRect(GetCellRectangle(id), boundingRect))
                        //if (Overlap.Check(GetCellRectangle(id), shape))
                        {
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


        public void AddRange(List<ICollidable> colliders, bool dynamic = true)
        {
            foreach (ICollidable collider in colliders)
            {
                Add(collider, dynamic);
            }
        }
        public void Add(ICollidable collider, bool dynamic = true)
        {
            var hashes = GetCellIDs(collider.GetCollider(), collider.HasDynamicBoundingBox());
            foreach (int hash in hashes)
            {
                if (dynamic)
                {
                    if (!bucketsDynamic[hash].Contains(collider)) { bucketsDynamic[hash].Add(collider); }
                }
                else
                {
                    if (!bucketsStatic[hash].Contains(collider)) { bucketsStatic[hash].Add(collider); }
                }
            }
        }

        public void Remove(ICollidable collider, bool dynamic = false)
        {
            var hashes = GetCellIDs(collider.GetCollider(), collider.HasDynamicBoundingBox());
            foreach (int hash in hashes)
            {
                if (dynamic)
                {
                    bucketsDynamic[hash].Remove(collider);
                }
                else
                {
                    bucketsStatic[hash].Remove(collider);
                }
            }
        }
        public void Clear()
        {
            ClearStatic();
            ClearDynamic();
        }
        public void Close()
        {
            Clear();
            bucketsStatic = new List<ICollidable>[0];
            bucketsDynamic = new List<ICollidable>[0];
        }
        public void ClearStatic()
        {
            for (int i = 0; i < bucket_size; i++)
            {
                bucketsStatic[i].Clear();
            }
        }
        public void ClearDynamic()
        {
            for (int i = 0; i < bucket_size; i++)
            {
                bucketsDynamic[i].Clear();
            }
        }

        public List<ICollidable> GetObjects(Collider shape, bool dynamicBoundingBox = false)
        {
            List<ICollidable> objects = new List<ICollidable>();
            var hashes = GetCellIDs(shape, dynamicBoundingBox);
            foreach (int hash in hashes)
            {
                objects.AddRange(bucketsDynamic[hash]);
                objects.AddRange(bucketsStatic[hash]);
            }
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
