using ShapeEngine.Lib;
using ShapeEngine.Screen;
using ShapeEngine.Timing;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace ShapeEngine.Core
{
    
    //public interface IClosest
    //{
    //    public Vector2 GetClosestPoint(Vector2 p);
    //}
    public struct ClosestItem<T> where T : struct
    {
        public bool Valid;
        public Vector2 ClosestPoint;
        public T Object;
        public float DisSquared;

        public ClosestItem()
        {
            Object = default(T);
            ClosestPoint = new();
            DisSquared = 0f;
            Valid = false;
        }
        public ClosestItem(T obj, Vector2 closestPoint, float disSquared)
        {
            Object = obj;
            ClosestPoint = closestPoint;
            DisSquared = disSquared;
            Valid = true;
        }
    }

    public class ShapeList<T> : List<T>
    {
        public void AddRange(params T[] items) { AddRange(items as IEnumerable<T>);}
        public ShapeList<T> Copy()
        {
            ShapeList<T> newList = new();
            newList.AddRange(this);
            return newList;
        }
        public bool IsIndexValid(int index)
        {
            if (index < 0 || index >= Count) return false;
            return true;
        }
        public override int GetHashCode()
        {
            HashCode hash = new();
            foreach (var element in this)
            {
                hash.Add(element);
            }
            return hash.ToHashCode();
        }
    }
    public class Points : ShapeList<Vector2>, IEquatable<Points>
    {
        public Points(params Vector2[] points) { AddRange(points); }
        public Points(IEnumerable<Vector2> points) { AddRange(points); }

        public override int GetHashCode() { return SUtils.GetHashCode(this); }
        public bool Equals(Points? other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] != other[i]) return false;
            }
            return true;
        }

        public ClosestItem<Vector2> GetClosestItem(Vector2 p)
        {
            if (Count <= 0) return new();

            float minDisSquared = float.PositiveInfinity;
            Vector2 closestPoint = new();

            for (int i = 0; i < Count; i++)
            {
                var point = this[i];

                float disSquared = (point - p).LengthSquared();
                if (disSquared < minDisSquared)
                {
                    minDisSquared = disSquared;
                    closestPoint = point;
                }
            }
            return new(closestPoint, closestPoint, minDisSquared);
        }
        public Points GetUniquePoints()
        {
            HashSet<Vector2> uniqueVertices = new HashSet<Vector2>();
            for (int i = 0; i < Count; i++)
            {
                uniqueVertices.Add(this[i]);
            }
            return new(uniqueVertices);
        }


        public Polygon ToPolygon()
        {
            return new Polygon(this);
        }
        public Polyline ToPolyline()
        {
            return new Polyline(this);
        }

        public void Floor() { Points.Floor(this); }
        public void Ceiling() { Points.Ceiling(this); }
        public void Truncate() { Points.Truncate(this); }
        public void Round() { Points.Round(this); }


        public static void Floor(List<Vector2> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = points[i].Floor();
            }
        }
        public static void Ceiling(List<Vector2> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = points[i].Ceiling();
            }
        }
        public static void Round(List<Vector2> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = points[i].Round();
            }
        }
        public static void Truncate(List<Vector2> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = points[i].Truncate();
            }
        }

    }
    public class Segments : ShapeList<Segment>
    {
        public Segments() { }
        public Segments(IShape shape) { AddRange(shape.GetEdges()); }
        public Segments(params Segment[] edges) { AddRange(edges); }
        public Segments(IEnumerable<Segment> edges) { AddRange(edges); }

        public bool Equals(Segments? other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] != other[i]) return false;
            }
            return true;
        }
        public override int GetHashCode() { return SUtils.GetHashCode(this); }
        public ClosestItem<Segment> GetClosestItem(Vector2 p)
        {
            if (Count <= 0) return new();

            float minDisSquared = float.PositiveInfinity;
            Segment closestSegment = new();
            Vector2 closestSegmentPoint = new();
            for (int i = 0; i < Count; i++)
            {
                var seg = this[i];
                Vector2 closestPoint = seg.GetClosestPoint(p).Point;
                float disSquared = (closestPoint - p).LengthSquared();
                if(disSquared < minDisSquared)
                {
                    minDisSquared = disSquared;
                    closestSegment = seg;
                    closestSegmentPoint = closestPoint;
                }
            }

            return new(closestSegment, closestSegmentPoint, minDisSquared);
        }
        public Points GetUniquePoints()
        {
            HashSet<Vector2> uniqueVertices = new HashSet<Vector2>();
            for (int i = 0; i < Count; i++)
            {
                var seg = this[i];
                uniqueVertices.Add(seg.Start);
                uniqueVertices.Add(seg.End);
            }

            return new(uniqueVertices);
        }
        
        public Segments GetUniqueSegments()
        {
            HashSet<Segment> uniqueSegments = new HashSet<Segment>();
            for (int i = 0; i < Count; i++)
            {
                var seg = this[i];
                uniqueSegments.Add(seg);
            }

            return new(uniqueSegments);
        }




        /// <summary>
        /// Counts how often the specified segment appears in the list.
        /// </summary>
        /// <param name="seg"></param>
        /// <returns></returns>
        public int GetCount(Segment seg) { return this.Count((s) => s.Equals(seg)); }

        /// <summary>
        /// Counts how often the specified segment appears in the list disregarding the direction of each segment.
        /// </summary>
        /// <param name="seg"></param>
        /// <returns></returns>
        public int GetCountSimilar(Segment seg) { return this.Count((s) => s.IsSimilar(seg)); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seg"></param>
        /// <returns>Returns true if seg is already in the list.</returns>
        public bool ContainsSegment(Segment seg)
        {
            foreach (var segment in this) { if (segment.Equals(seg)) return true; }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="seg"></param>
        /// <returns>Returns true if similar segment is already in the list.</returns>
        public bool ContainsSegmentSimilar(Segment seg)
        {
            foreach (var segment in this) { if (segment.IsSimilar(seg)) return true; }
            return false;
        }



        /*
        /// <summary>
        /// Only add the segment if it not already contained in the list.
        /// </summary>
        /// <param name="seg"></param>
        public void AddUnique(Segment seg)
        {
            if (!ContainsSegment(seg)) Add(seg);
        }
        /// <summary>
        /// Only add the segments that are not already contained in the list.
        /// </summary>
        /// <param name="edges"></param>
        public void AddUnique(IEnumerable<Segment> edges)
        {
            foreach (var edge in edges)
            {
                AddUnique(edge);
            }
        }
        */
    }
    public class Triangulation : ShapeList<Triangle>
    {
        public Triangulation() { }
        public Triangulation(IShape shape) { AddRange(shape.Triangulate()); }
        public Triangulation(params Triangle[] triangles) { AddRange(triangles); }
        public Triangulation(IEnumerable<Triangle> triangles) { AddRange(triangles); }

        
        public ClosestItem<Triangle> GetClosestItem(Vector2 p)
        {
            if (Count <= 0) return new();

            float minDisSquared = float.PositiveInfinity;
            Triangle closestTriangle = new();
            bool contained = false;
            Vector2 closestTrianglePoint = new();

            for (int i = 0; i < Count; i++)
            {
                var tri = this[i];
                bool containsPoint = tri.IsPointInside(p);
                Vector2 closestPoint = tri.GetClosestPoint(p).Point;
                float disSquared = (closestPoint - p).LengthSquared();
                if (disSquared < minDisSquared)
                {
                    if(containsPoint || !contained)
                    {
                        minDisSquared = disSquared;
                        closestTriangle = tri;
                        closestTrianglePoint = closestPoint;
                        if (containsPoint) contained = true;
                    }
                }
                else
                {
                    if (containsPoint && !contained)
                    {
                        contained = true;
                        minDisSquared = disSquared;
                        closestTriangle = tri;
                        closestTrianglePoint = closestPoint;
                    }
                }
            }
            return new(closestTriangle, closestTrianglePoint, minDisSquared);
        }
        
        
        public Points GetUniquePoints()
        {
            HashSet<Vector2> uniqueVertices = new HashSet<Vector2>();
            for (int i = 0; i < Count; i++)
            {
                var tri = this[i];
                uniqueVertices.Add(tri.A);
                uniqueVertices.Add(tri.B);
                uniqueVertices.Add(tri.C);
            }

            return new(uniqueVertices);
        }
        public Segments GetUniqueSegments()
        {
            HashSet<Segment> unique = new HashSet<Segment>();
            for (int i = 0; i < Count; i++)
            {
                var tri = this[i];
                unique.Add(tri.SegmentA);
                unique.Add(tri.SegmentB);
                unique.Add(tri.SegmentC);
            }

            return new(unique);
        }
        public Triangulation GetUniqueTriangles()
        {
            HashSet<Triangle> uniqueTriangles = new HashSet<Triangle>();
            for (int i = 0; i < Count; i++)
            {
                var tri = this[i];
                uniqueTriangles.Add(tri);
            }

            return new(uniqueTriangles);
        }
        public Triangulation GetContainingTriangles(Vector2 p)
        {
            Triangulation result = new();
            for (int i = 0; i < Count; i++)
            {
                var tri = this[i];
                if (tri.IsPointInside(p)) result.Add(tri);
            }
            return result;
        }
        

        public override int GetHashCode() { return SUtils.GetHashCode(this); }
        public bool Equals(Triangulation? other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] != other[i]) return false;
            }
            return true;
        }
        /// <summary>
        /// Get the total area of all triangles in this triangulation.
        /// </summary>
        /// <returns></returns>
        public float GetArea()
        {
            float total = 0f;
            foreach (var t in this)
            {
                total += t.GetArea();
            }
            return total;
        }

        /// <summary>
        /// Remove all triangles with an area less than the threshold. If threshold is <= 0, nothing happens.
        /// </summary>
        /// <param name="areaThreshold"></param>
        /// <returns></returns>
        public int Remove(float areaThreshold)
        {
            if (areaThreshold <= 0f) return 0;

            int count = 0;
            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].GetArea() < areaThreshold)
                {
                    RemoveAt(i);
                    count++;
                }
            }

            return count;
        }



        /// <summary>
        /// Get a new triangulation with triangles with an area >= areaThreshold.
        /// </summary>
        /// <param name="areaThreshold"></param>
        /// <returns></returns>
        public Triangulation Get(float areaThreshold)
        {
            Triangulation newTriangulation = new();
            if (areaThreshold <= 0f) return newTriangulation;

            for (int i = Count - 1; i >= 0; i--)
            {
                var t = this[i];
                if (t.GetArea() >= areaThreshold)
                {
                    newTriangulation.Add(t);
                }
            }

            return newTriangulation;
        }

        /// <summary>
        /// Subdivide the triangulation until all triangles are smaller than min area.
        /// </summary>
        /// <param name="minArea">A triangle will always be subdivided if the area is bigger than min area.s</param>
        /// <returns></returns>
        public Triangulation Subdivide(float minArea)
        {
            Triangulation final = new();

            Triangulation queue = new();
            queue.AddRange(this);
            while (queue.Count > 0)
            {
                int endIndex = queue.Count - 1;
                var tri = queue[endIndex];

                var triArea = tri.GetArea();
                if (triArea < minArea) final.Add(tri);
                else queue.AddRange(tri.Triangulate(minArea));
                queue.RemoveAt(endIndex);
            }
            return final;
        }

        /// <summary>
        /// Subdivide the triangles further based on the parameters.
        /// </summary>
        /// <param name="minArea">Triangles with an area smaller than min area will never be subdivided.</param>
        /// <param name="maxArea">Triangles with an area bigger than maxArea will always be subdivided.</param>
        /// <param name="narrowValue">Triangles that are considered narrow will not be subdivided.</param>
        /// <returns></returns>
        public Triangulation Subdivide(float minArea, float maxArea, float keepChance = 0.5f, float narrowValue = 0.2f)
        {
            if (this.Count <= 0) return this;

            Triangulation final = new();
            Triangulation queue = new();

            if (this.Count == 1)
            {
                queue.AddRange(this[0].Triangulate(minArea));
            }
            else queue.AddRange(this);


            while (queue.Count > 0)
            {
                int endIndex = queue.Count - 1;
                var tri = queue[endIndex];

                var triArea = tri.GetArea();
                if (triArea < minArea || tri.IsNarrow(narrowValue)) //too small or narrow
                {
                    final.Add(tri);
                }
                else if (triArea > maxArea) //always subdivide because too big
                {
                    queue.AddRange(tri.Triangulate(minArea));
                }
                else //subdivde or keep
                {
                    float chance = keepChance;
                    if (keepChance < 0 || keepChance > 1f)
                    {
                        chance = (triArea - minArea) / (maxArea - minArea);
                    }

                    if (SRNG.chance(chance)) final.Add(tri);
                    else queue.AddRange(tri.Triangulate(minArea));
                }
                queue.RemoveAt(endIndex);
            }
            return final;
        }
    }



    internal class DelayedAction : ISequenceable
    {
        private Action action;
        private float timer;

        public DelayedAction(float delay, Action action)
        {
            if (delay <= 0)
            {
                this.timer = 0f;
                this.action = action;
                this.action();
            }
            else
            {
                this.timer = delay;
                this.action = action;
            }
        }
        public bool Update(float dt)
        {
            if (timer <= 0f) return true;
            else
            {
                timer -= dt;
                if (timer <= 0f)
                {
                    this.action.Invoke();
                    return true;
                }
            }
            return false;
        }
    }
    internal class DeferredInfo
    {
        private Action action;
        private int frames = 0;
        public DeferredInfo(Action action, int frames)
        {
            this.action = action;
            this.frames = frames;
        }

        public bool Call()
        {
            if (frames <= 0)
            {
                action.Invoke();
                return true;
            }
            else
            {
                frames--;
                return false;
            }
        }

    }

    /// <summary>
    /// Returned by the Run() function in the GameLoop class.
    /// </summary>
    public struct ExitCode
    {
        public bool restart = false;
        public ExitCode(bool restart) { this.restart = restart; }

    }
    public struct Dimensions : IEquatable<Dimensions>, IFormattable
    {
        public int Width;
        public int Height;

        public Dimensions() { this.Width = 0; this.Height = 0; }
        public Dimensions(int value) { this.Width = value; this.Height = value; }
        public Dimensions(int width, int height) { this.Width = width; this.Height = height; }
        public Dimensions(float value) { this.Width = (int)value; this.Height = (int)value; }
        public Dimensions(float width, float height) { this.Width = (int)width; this.Height = (int)height; }
        public Dimensions(Vector2 v) { this.Width = (int)v.X; this.Height = (int)v.Y; }

        public bool IsValid() { return Width >= 0 && Height >= 0; }
        public float Area { get => Width * Height; }
        public int MaxDimension
        {
            get
            {
                if (Width > Height) return Width;
                else return Height;
            }
        }
        public int MinDimension
        {
            get
            {
                if (Width < Height) return Width;
                else return Height;
            }
        }


        public Vector2 ToVector2() { return new Vector2(Width, Height); }

        public bool Equals(Dimensions other)
        {
            return Width == other.Width && Height == other.Height;
        }
        public override bool Equals(object? obj)
        {
            if (obj != null && obj is Dimensions d)
            {
                return Equals(d);
            }
            return false;
        }
        public override readonly string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }
        public readonly string ToString(string? format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }
        public readonly string ToString(string? format, IFormatProvider? formatProvider)
        {
            StringBuilder sb = new StringBuilder();
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
            sb.Append('<');
            sb.Append(Width.ToString(format, formatProvider));
            sb.Append(separator);
            sb.Append(' ');
            sb.Append(Height.ToString(format, formatProvider));
            sb.Append('>');
            return sb.ToString();
        }

        public static Dimensions operator +(Dimensions left, Dimensions right)
        {
            return new Dimensions(
                left.Width + right.Width,
                left.Height + right.Height
            );
        }
        public static Dimensions operator /(Dimensions left, Dimensions right)
        {
            return new Dimensions(
                left.Width / right.Width,
                left.Height / right.Height
            );
        }
        public static Dimensions operator /(Dimensions value1, int value2)
        {
            return value1 / new Dimensions(value2);
        }
        public static Dimensions operator /(Dimensions value1, float value2)
        {
            return new Dimensions(value1.Width / value2, value1.Height / value2);
        }
        public static bool operator ==(Dimensions left, Dimensions right)
        {
            return (left.Width == right.Width)
                && (left.Height == right.Height);
        }
        public static bool operator !=(Dimensions left, Dimensions right)
        {
            return !(left == right);
        }
        public static Dimensions operator *(Dimensions left, Dimensions right)
        {
            return new Dimensions(
                left.Width * right.Width,
                left.Height * right.Height
            );
        }
        public static Dimensions operator *(Dimensions left, float right)
        {
            return new Dimensions(left.Width * right, left.Height * right);
        }
        public static Dimensions operator *(float left, Dimensions right)
        {
            return right * left;
        }
        public static Dimensions operator -(Dimensions left, Dimensions right)
        {
            return new Dimensions(
                left.Width - right.Width,
                left.Height - right.Height
            );
        }
        public static Dimensions operator -(Dimensions value)
        {
            return new Dimensions(0) - value;
        }


        public static Dimensions Abs(Dimensions value)
        {
            return new Dimensions(
                (int)MathF.Abs(value.Width),
                (int)MathF.Abs(value.Height)
            );
        }
        public static Dimensions Clamp(Dimensions value1, Dimensions min, Dimensions max)
        {
            return Min(Max(value1, min), max);
        }
        public static Dimensions Lerp(Dimensions value1, Dimensions value2, float amount)
        {
            return (value1 * (1.0f - amount)) + (value2 * amount);
        }
        public static Dimensions Max(Dimensions value1, Dimensions value2)
        {
            return new Dimensions(
                (value1.Width > value2.Width) ? value1.Width : value2.Width,
                (value1.Height > value2.Height) ? value1.Height : value2.Height
            );
        }
        public static Dimensions Min(Dimensions value1, Dimensions value2)
        {
            return new Dimensions(
                (value1.Width < value2.Width) ? value1.Width : value2.Width,
                (value1.Height < value2.Height) ? value1.Height : value2.Height
            );
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Width, Height);
        }


    }


    public class ScreenTextures : Dictionary<uint, ScreenTexture>
    {
        public ActiveScreenTextures GetActive(ScreenTextureMask screenTextureMask)
        {
            if (screenTextureMask.Count <= 0)
                return new(this.Values.Where((st) => st.Active));
            else
                return new(this.Values.Where((st) => st.Active && screenTextureMask.Contains(st.ID)));
        }
        public ActiveScreenTextures GetActive()
        {
            return new(this.Values.Where((st) => st.Active));
        }
        public List<ScreenTexture> GetAll() { return this.Values.ToList(); }

    }
    public class ActiveScreenTextures : List<ScreenTexture>
    {
        public ActiveScreenTextures(IEnumerable<ScreenTexture> textures)
        {
            this.AddRange(textures);
        }
        public ActiveScreenTextures(params ScreenTexture[] textures)
        {
            this.AddRange(textures);
        }
        public ActiveScreenTextures SortDrawOrder()
        {
            this.Sort(delegate (ScreenTexture x, ScreenTexture y)
            {
                if (x == null || y == null) return 0;

                if (x.DrawOrder < y.DrawOrder) return -1;
                else if (x.DrawOrder > y.DrawOrder) return 1;
                else return 0;
            });
            return this;
        }
    }
    public class ScreenTextureMask : HashSet<uint>
    {
        public ScreenTextureMask() { }
        public ScreenTextureMask(params uint[] mask)
        {
            foreach (var id in mask)
            {
                this.Add(id);
            }
        }
        public ScreenTextureMask(IEnumerable<uint> mask)
        {
            foreach (var id in mask)
            {
                this.Add(id);
            }
        }
        public ScreenTextureMask(HashSet<uint> mask)
        {
            foreach (var id in mask)
            {
                this.Add(id);
            }
        }
    }


    public class FractureInfo
    {
        public Polygons NewShapes;
        public Polygons Cutouts;
        public Triangulation Pieces;

        public FractureInfo(Polygons newShapes, Polygons cutouts, Triangulation pieces)
        {
            this.NewShapes = newShapes;
            this.Cutouts = cutouts;
            this.Pieces = pieces;
        }
    }
    public class FractureHelper
    {
        public float MinArea { get; set; }
        public float MaxArea { get; set; }
        public float KeepChance { get; set; }
        public float NarrowValue { get; set; }

        //public float DivisionChance { get; set; } = 0.5f;
        //public int MinDivisionCount { get; set; } = 3;
        //public int MaxDivisionCount { get; set; } = 9;

        public FractureHelper(float minArea, float maxArea, float keepChance = 0.5f, float narrowValue = 0.2f)
        {
            this.MinArea = minArea;
            this.MaxArea = maxArea;
            this.KeepChance = keepChance;
            this.NarrowValue = narrowValue;
        }

        public FractureInfo Fracture(Polygon shape, Polygon cutShape)
        {
            var cutOuts = SClipper.Intersect(shape, cutShape).ToPolygons(true);
            var newShapes = SClipper.Difference(shape, cutShape).ToPolygons(true);
            Triangulation pieces = new();
            foreach (var cutOut in cutOuts)
            {
                var fracturePieces = cutOut.Triangulate().Subdivide(MinArea, MaxArea, KeepChance, NarrowValue);
                pieces.AddRange(fracturePieces);
            }

            return new(newShapes, cutOuts, pieces);
        }
    }


    public class CollisionInformation
    {
        public List<Collision> Collisions;
        public CollisionSurface CollisionSurface;
        public CollisionInformation(List<Collision> collisions, bool computesIntersections)
        {
            this.Collisions = collisions;
            if (!computesIntersections) this.CollisionSurface = new();
            else
            {
                Vector2 avgPoint = new();
                Vector2 avgNormal = new();
                int count = 0;
                foreach (var col in collisions)
                {
                    if (col.Intersection.Valid)
                    {
                        count++;
                        var surface = col.Intersection.CollisionSurface;
                        avgPoint += surface.Point;
                        avgNormal += surface.Normal;
                    }
                }

                if (count > 0)
                {
                    avgPoint /= count;
                    avgNormal = avgNormal.Normalize();
                    this.CollisionSurface = new(avgPoint, avgNormal);
                }
                else
                {
                    this.CollisionSurface = new();
                }
            }
        }

        //public bool ContainsCollidable(TCollidable other)
        //{
        //    foreach (var c in Collisions)
        //    {
        //        if (c.Other == other) return true;
        //    }
        //    return false;
        //}
        public List<Collision> FilterCollisions(Predicate<Collision> match)
        {
            List<Collision> filtered = new();
            foreach (var c in Collisions)
            {
                if (match(c)) filtered.Add(c);
            }
            return filtered;
        }
        public List<ICollidable> FilterObjects(Predicate<ICollidable> match)
        {
            HashSet<ICollidable> filtered = new();
            foreach (var c in Collisions)
            {
                if (match(c.Other)) filtered.Add(c.Other);
            }
            return filtered.ToList();
        }
        public List<ICollidable> GetAllObjects()
        {
            HashSet<ICollidable> others = new();
            foreach (var c in Collisions)
            {
                others.Add(c.Other);

            }
            return others.ToList();
        }
        public List<Collision> GetFirstContactCollisions()
        {
            return FilterCollisions((c) => c.FirstContact);
        }
        public List<ICollidable> GetFirstContactObjects()
        {
            var filtered = GetFirstContactCollisions();
            HashSet<ICollidable> others = new();
            foreach (var c in filtered)
            {
                others.Add(c.Other);
            }
            return others.ToList();
        }
    }
    public class Collision
    {
        public bool FirstContact;
        public ICollidable Self;
        public ICollidable Other;
        public Vector2 SelfVel;
        public Vector2 OtherVel;
        public Intersection Intersection;

        public Collision(ICollidable self, ICollidable other, bool firstContact)
        {
            this.Self = self;
            this.Other = other;
            this.SelfVel = self.GetCollider().Vel;
            this.OtherVel = other.GetCollider().Vel;
            this.Intersection = new();
            this.FirstContact = firstContact;
        }
        public Collision(ICollidable self, ICollidable other, bool firstContact, CollisionPoints collisionPoints)
        {
            this.Self = self;
            this.Other = other;
            this.SelfVel = self.GetCollider().Vel;
            this.OtherVel = other.GetCollider().Vel;
            this.Intersection = new(collisionPoints, SelfVel, self.GetCollider().Pos);
            this.FirstContact = firstContact;
        }

    }
    public class Intersection
    {
        public bool Valid;
        public CollisionSurface CollisionSurface;
        public CollisionPoints ColPoints;

        public Intersection() { this.Valid = false; this.CollisionSurface = new(); this.ColPoints = new(); }
        public Intersection(CollisionPoints points, Vector2 vel, Vector2 refPoint)
        {
            if (points.Count <= 0)
            {
                this.Valid = false;
                this.CollisionSurface = new();
                this.ColPoints = new();
            }
            else
            {
                Vector2 avgPoint = new();
                Vector2 avgNormal = new();
                int count = 0;
                foreach (var p in points)
                {
                    if (DiscardNormal(p.Normal, vel)) continue;
                    if (DiscardNormal(p, refPoint)) continue;

                    count++;
                    avgPoint += p.Point;
                    avgNormal += p.Normal;
                }
                if (count > 0)
                {
                    this.Valid = true;
                    this.ColPoints = points;
                    avgPoint /= count;
                    avgNormal = avgNormal.Normalize();
                    this.CollisionSurface = new(avgPoint, avgNormal);
                }
                else
                {
                    this.Valid = false;
                    this.ColPoints = points;
                    this.CollisionSurface = new();
                }
            }
        }
        public Intersection(CollisionPoints points)
        {
            if (points.Count <= 0)
            {
                this.Valid = false;
                this.CollisionSurface = new();
                this.ColPoints = new();
            }
            else
            {
                this.Valid = true;
                this.ColPoints = points;

                Vector2 avgPoint = new();
                Vector2 avgNormal = new();
                foreach (var p in points)
                {
                    avgPoint += p.Point;
                    avgNormal += p.Normal;
                }
                if (points.Count > 0)
                {
                    avgPoint /= points.Count;
                    avgNormal = avgNormal.Normalize();
                    this.CollisionSurface = new(avgPoint, avgNormal);
                }
                else this.CollisionSurface = new();
            }
        }


        private static bool DiscardNormal(Vector2 n, Vector2 vel)
        {
            return n.IsFacingTheSameDirection(vel);
        }
        private static bool DiscardNormal(CollisionPoint p, Vector2 refPoint)
        {
            Vector2 dir = p.Point - refPoint;
            return p.Normal.IsFacingTheSameDirection(dir);
        }

        //public void FlipNormals(Vector2 refPoint)
        //{
        //    if (points.Count <= 0) return;
        //
        //    List<(Vector2 p, Vector2 n)> newPoints = new();
        //    foreach (var p in points)
        //    {
        //        Vector2 dir = refPoint - p.p;
        //        if (dir.IsFacingTheOppositeDirection(p.n)) newPoints.Add((p.p, p.n.Flip()));
        //        else newPoints.Add(p);
        //    }
        //    this.points = newPoints;
        //    this.n = points[0].n;
        //}
        //public Intersection CheckVelocityNew(Vector2 vel)
        //{
        //    List<(Vector2 p, Vector2 n)> newPoints = new();
        //    
        //    for (int i = points.Count - 1; i >= 0; i--)
        //    {
        //        var intersection = points[i];
        //        if (intersection.n.IsFacingTheSameDirection(vel)) continue;
        //        newPoints.Add(intersection);
        //    }
        //    return new(newPoints);
        //}

    }
    
    public struct CollisionSurface
    {
        public Vector2 Point;
        public Vector2 Normal;
        public bool Valid;

        public CollisionSurface() { Point = new(); Normal = new(); Valid = false; }
        public CollisionSurface(Vector2 point, Vector2 normal)
        {
            this.Point = point;
            this.Normal = normal;
            this.Valid = true;
        }

    }
    public struct CollisionPoint : IEquatable<CollisionPoint>
    {
        public Vector2 Point;
        public Vector2 Normal;

        public CollisionPoint() { Point = new(); Normal = new(); }
        public CollisionPoint(Vector2 p, Vector2 n) { Point = p; Normal = n; }

        public bool Equals(CollisionPoint other)
        {
            return other.Point == Point && other.Normal == Normal;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Point, Normal);
        }

        public CollisionPoint FlipNormal()
        {
            return new(Point, Normal.Flip());
        }
        public CollisionPoint FlipNormal(Vector2 referencePoint)
        {
            Vector2 dir = referencePoint - Point;
            if (dir.IsFacingTheOppositeDirection(Normal)) return FlipNormal();

            return this;
        }
    }
    
    public class QueryInfos : List<QueryInfo>
    {
        public QueryInfos(params QueryInfo[] infos) { AddRange(infos); }
        public QueryInfos(IEnumerable<QueryInfo> infos) { AddRange(infos); }


        public void AddRange(params QueryInfo[] newInfos) { AddRange(newInfos as IEnumerable<QueryInfo>); }
        public QueryInfos Copy() { return new(this); }
        public void SortClosest(Vector2 origin)
        {
            if (Count > 1)
            {
                Sort
                (
                    (a, b) =>
                    {
                        if (!a.points.valid) return 1;
                        else if (!b.points.valid) return -1;
                        
                        float la = (origin - a.points.closest.Point).LengthSquared();
                        float lb = (origin - b.points.closest.Point).LengthSquared();
            
                        if (la > lb) return 1;
                        else if (la == lb) return 0;
                        else return -1;
                    }
                );
            }
        }
    }
    public class QueryInfo
    {
        public Vector2 origin;
        public ICollidable collidable;
        public QueryPoints points;

        public QueryInfo(ICollidable collidable, Vector2 origin)
        {
            this.collidable = collidable;
            this.origin = origin;
            this.points = new();
        }
        public QueryInfo(ICollidable collidable, Vector2 origin, CollisionPoints points)
        {
            this.collidable = collidable;
            this.origin = origin;
            this.points = new(points, origin);
        }
    }
    public class QueryPoints
    {
        public bool valid;
        public CollisionPoints points;
        public CollisionPoint closest;

        public QueryPoints()
        {
            this.valid = false;
            this.points = new();
            this.closest = new();
        }
        public QueryPoints(CollisionPoints points, Vector2 origin)
        {
            if(points.Count <= 0)
            {
                this.valid = false;
                this.points = new();
                this.closest = new();
            }
            else
            {
                this.valid = true;
                points.SortClosest(origin);
                this.points = points;
                this.closest = points[0];
            }
        }
    }
    
    
    public class CollisionPoints : ShapeList<CollisionPoint>
    {
        public CollisionPoints(params CollisionPoint[] points) { AddRange(points); }
        public CollisionPoints(IEnumerable<CollisionPoint> points) { AddRange(points); }


        public bool Valid { get { return Count > 0; } }
        public void FlipNormals(Vector2 referencePoint)
        {
            for (int i = 0; i < Count; i++)
            {
                var p = this[i];
                Vector2 dir = referencePoint - p.Point;
                if (dir.IsFacingTheOppositeDirection(p.Normal))
                    this[i] = this[i].FlipNormal();
            }
        }
        
        
        public ClosestItem<CollisionPoint> GetClosestPoint(Vector2 p)
        {
            if (Count <= 0) return new();

            float minDisSquared = float.PositiveInfinity;
            CollisionPoint closestPoint = new();

            for (int i = 0; i < Count; i++)
            {
                var point = this[i];

                float disSquared = (point.Point - p).LengthSquared();
                if (disSquared < minDisSquared)
                {
                    minDisSquared = disSquared;
                    closestPoint = point;
                }
            }
            return new(closestPoint, closestPoint.Point, minDisSquared);
        }

        public override int GetHashCode() { return SUtils.GetHashCode(this); }
        public bool Equals(CollisionPoints? other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Equals(other[i])) return false;
            }
            return true;
        }


        public Points GetUniquePoints()
        {
            HashSet<Vector2> uniqueVertices = new HashSet<Vector2>();
            for (int i = 0; i < Count; i++)
            {
                uniqueVertices.Add(this[i].Point);
            }
            return new(uniqueVertices);
        }
        public CollisionPoints GetUniqueCollisionPoints()
        {
            HashSet<CollisionPoint> unique = new HashSet<CollisionPoint>();
            for (int i = 0; i < Count; i++)
            {
                unique.Add(this[i]);
            }
            return new(unique);
        }

        public void SortClosest(Vector2 refPoint)
        {
            this.Sort
                (
                    (a, b) =>
                    {
                        float la = (refPoint - a.Point).LengthSquared();
                        float lb = (refPoint - b.Point).LengthSquared();

                        if (la > lb) return 1;
                        else if (la == lb) return 0;
                        else return -1;
                    }
                );
        }

    }



}

    //public struct QueryInfo
    //{
    //    public ICollidable collidable;
    //    public Intersection intersection;
    //    public QueryInfo(ICollidable collidable)
    //    {
    //        this.collidable = collidable;
    //        this.intersection = new();
    //    }
    //    public QueryInfo(ICollidable collidable, CollisionPoints points)
    //    {
    //        this.collidable = collidable;
    //        this.intersection = new(points);
    //    }
    //
    //}