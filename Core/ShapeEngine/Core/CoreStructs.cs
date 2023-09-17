using ShapeEngine.Lib;
using ShapeEngine.Screen;
using ShapeEngine.Timing;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace ShapeEngine.Core
{
    
    public readonly struct ClosestPoint
    {
        public readonly bool Valid => Closest.Valid;
        public readonly CollisionPoint Closest;
        public readonly float Distance;
        public readonly float DistanceSquared => Distance * Distance;
        
        public ClosestPoint()
        {
            Closest = new();
            Distance = 0f;
        }
        public ClosestPoint(Vector2 point, Vector2 normal, float distance)
        {
            Closest = new(point, normal);
            Distance = distance;
        }
        public ClosestPoint(CollisionPoint closest, float distance)
        {
            Closest = closest;
            Distance = distance;
        }
    }
    public readonly struct ClosestSegment
    {
        public readonly Segment Segment;
        public readonly ClosestPoint Point;
        public readonly bool Valid => Point.Valid;
        public ClosestSegment()
        {
            Segment = new();
            Point = new();
        }

        public ClosestSegment(Segment segment, CollisionPoint point, float distance)
        {
            Segment = segment;
            Point = new(point, distance);
        }
        public ClosestSegment(Segment segment, Vector2 point, Vector2 normal, float distance)
        {
            Segment = segment;
            Point = new(point, normal, distance);
            
        }
        public ClosestSegment(Segment segment, Vector2 point, float distance)
        {
            Segment = segment;
            Point = new(point, segment.Normal, distance);
        }
    }
    public readonly struct ClosestItem<T> where T : struct
    {
        public readonly T Item;
        public readonly ClosestPoint Point;
        public readonly bool Valid => Point.Valid;
        public ClosestItem()
        {
            Item = default(T);
            Point = new();
        }
        public ClosestItem(T item, CollisionPoint point, float distance)
        {
            Item = item;
            Point = new(point, distance);
        }
        public ClosestItem(T item, Vector2 point, Vector2 normal, float distance)
        {
            Item = item;
            Point = new(point, normal, distance);
            
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
            return index >= 0 && index < Count;
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
        #region Constructors
        public Points(params Vector2[] points) { AddRange(points); }
        public Points(IEnumerable<Vector2> points) { AddRange(points); }
        #endregion

        #region Equals & HashCode
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
        #endregion

        #region Public
        /// <summary>
        /// Gets the value at the specified index wrapping around if index is smaller than 0 or bigger than count
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector2 Get(int index)
        {
            return Count <= 0 ? new() : this[index % Count];
        }
        public ClosestPoint GetClosest(Vector2 p)
        {
            if (Count <= 0) return new();

            float minDisSquared = float.PositiveInfinity;
            Vector2 closestPoint = new();

            for (var i = 0; i < Count; i++)
            {
                var point = this[i];

                float disSquared = (point - p).LengthSquared();
                if (disSquared < minDisSquared)
                {
                    minDisSquared = disSquared;
                    closestPoint = point;
                }
            }
            return new(closestPoint, (p -closestPoint), MathF.Sqrt(minDisSquared));
        }
        public Points GetUniquePoints()
        {
            var uniqueVertices = new HashSet<Vector2>();
            for (var i = 0; i < Count; i++)
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
        #endregion

        #region Static
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
        #endregion
    }
    public class Segments : ShapeList<Segment>
    {
        #region Constructors
        public Segments() { }
        public Segments(IShape shape) { AddRange(shape.GetEdges()); }
        public Segments(params Segment[] edges) { AddRange(edges); }
        public Segments(IEnumerable<Segment> edges) { AddRange(edges); }
        #endregion

        #region Equals & HashCode
        public bool Equals(Segments? other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;
            for (var i = 0; i < Count; i++)
            {
                if (this[i] != other[i]) return false;
            }
            return true;
        }
        public override int GetHashCode() { return SUtils.GetHashCode(this); }
        #endregion

        #region Public
        public ClosestSegment GetClosestSegment(Vector2 p)
        {
            if (Count <= 0) return new();

            float minDisSquared = float.PositiveInfinity;
            Segment closestSegment = new();
            Vector2 closestSegmentPoint = new();
            for (var i = 0; i < Count; i++)
            {
                var seg = this[i];
                var closestPoint = seg.GetClosestPoint(p).Point;
                float disSquared = (closestPoint - p).LengthSquared();
                if(disSquared < minDisSquared)
                {
                    minDisSquared = disSquared;
                    closestSegment = seg;
                    closestSegmentPoint = closestPoint;
                }
            }

            return new(closestSegment, closestSegmentPoint, MathF.Sqrt(minDisSquared));
        }
        public Points GetUniquePoints()
        {
            var uniqueVertices = new HashSet<Vector2>();
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
            var uniqueSegments = new HashSet<Segment>();
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
        #endregion

        #region Overlap
        public bool OverlapShape(Segments b)
        {
            foreach (var segA in this)
            {
                if (segA.OverlapShape(b)) return true;
            }
            return false;
        }
        public bool OverlapShape(Segment s) { return s.OverlapShape(this); }
        public bool OverlapShape(Circle c) { return c.OverlapShape(this); }
        public bool OverlapShape(Triangle t) { return t.OverlapShape(this); }
        public bool OverlapShape(Rect r) { return r.OverlapShape(this); }
        public bool OverlapShape(Polyline pl) { return pl.OverlapShape(this); }

        #endregion

        #region Intersection
        public CollisionPoints IntersectShape(Segment s)
        {
            CollisionPoints points = new();

            foreach (var seg in this)
            {
                var collisionPoints = seg.IntersectShape(s);
                if (collisionPoints.Valid)
                {
                    points.AddRange(collisionPoints);
                }
            }
            return points;
        }
        public CollisionPoints IntersectShape(Circle c)
        {
            CollisionPoints points = new();
            foreach (var seg in this)
            {
                var intersectPoints = SGeometry.IntersectSegmentCircle(seg.Start, seg.End, c.Center, c.Radius);
                foreach (var p in intersectPoints)
                {
                    var n = SVec.Normalize(p - c.Center);
                    points.Add(new(p, n));
                }
            }
            return points;
        }
        public CollisionPoints IntersectShape(Segments b)
        {
            CollisionPoints points = new();
            foreach (var seg in this)
            {
                var collisionPoints = seg.IntersectShape(b);
                if (collisionPoints.Valid)
                {
                    points.AddRange(collisionPoints);
                }
            }
            return points;
        }

        #endregion

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
        #region Constructors
        public Triangulation() { }
        public Triangulation(IShape shape) { AddRange(shape.Triangulate()); }
        public Triangulation(params Triangle[] triangles) { AddRange(triangles); }
        public Triangulation(IEnumerable<Triangle> triangles) { AddRange(triangles); }
        #endregion
        
        #region Equals & HashCode
        public override int GetHashCode() { return SUtils.GetHashCode(this); }
        public bool Equals(Triangulation? other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;
            for (var i = 0; i < Count; i++)
            {
                if (this[i] != other[i]) return false;
            }
            return true;
        }
        #endregion

        #region Public
        public ClosestItem<Triangle> GetClosestTriangle(Vector2 p)
        {
            if (Count <= 0) return new();

            float minDisSquared = float.PositiveInfinity;
            Triangle closestTriangle = new();
            var contained = false;
            CollisionPoint closestTrianglePoint = new();

            for (var i = 0; i < Count; i++)
            {
                var tri = this[i];
                bool containsPoint = tri.ContainsPoint(p);
                var closestPoint = tri.GetClosestPoint(p);
                float disSquared = (closestPoint.Point - p).LengthSquared();
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
            var uniqueVertices = new HashSet<Vector2>();
            for (var i = 0; i < Count; i++)
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
            var unique = new HashSet<Segment>();
            for (var i = 0; i < Count; i++)
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
            var uniqueTriangles = new HashSet<Triangle>();
            for (var i = 0; i < Count; i++)
            {
                var tri = this[i];
                uniqueTriangles.Add(tri);
            }

            return new(uniqueTriangles);
        }
        public Triangulation GetContainingTriangles(Vector2 p)
        {
            Triangulation result = new();
            for (var i = 0; i < Count; i++)
            {
                var tri = this[i];
                if (tri.ContainsPoint(p)) result.Add(tri);
            }
            return result;
        }

        
        /// <summary>
        /// Get the total area of all triangles in this triangulation.
        /// </summary>
        /// <returns></returns>
        public float GetArea()
        {
            var total = 0f;
            foreach (var t in this)
            {
                total += t.GetArea();
            }
            return total;
        }

        /// <summary>
        /// Remove all triangles with an area less than the threshold. If threshold is smaller or equal to 0, nothing happens.
        /// </summary>
        /// <param name="areaThreshold"></param>
        /// <returns></returns>
        public int Remove(float areaThreshold)
        {
            if (areaThreshold <= 0f) return 0;

            var count = 0;
            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].GetArea() >= areaThreshold) continue;
                RemoveAt(i);
                count++;
            }

            return count;
        }
        #endregion

        #region Triangulation
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
        /// <param name="keepChance">The chance to keep a triangle and not subdivide it.</param>
        /// <param name="narrowValue">Triangles that are considered narrow will not be subdivided.</param>
        /// <returns></returns>
        public Triangulation Subdivide(float minArea, float maxArea, float keepChance = 0.5f, float narrowValue = 0.2f)
        {
            if (this.Count <= 0) return this;

            Triangulation final = new();
            Triangulation queue = new();

            queue.AddRange(this.Count == 1 ? this[0].Triangulate(minArea) : this);


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
        #endregion
    }



    internal class DelayedAction : ISequenceable
    {
        private readonly Action action;
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
                if (timer > 0f) return false;
                this.action.Invoke();
                return true;
            }
        }
    }
    internal class DeferredInfo
    {
        private readonly Action action;
        private int frames;
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
    public readonly struct ExitCode
    {
        public readonly bool Restart = false;
        public ExitCode(bool restart) { this.Restart = restart; }

    }
    public readonly struct Dimensions : IEquatable<Dimensions>, IFormattable
    {
        public readonly int Width;
        public readonly int Height;

        public Dimensions() { this.Width = 0; this.Height = 0; }
        public Dimensions(int value) { this.Width = value; this.Height = value; }
        public Dimensions(int width, int height) { this.Width = width; this.Height = height; }
        public Dimensions(float value) { this.Width = (int)value; this.Height = (int)value; }
        public Dimensions(float width, float height) { this.Width = (int)width; this.Height = (int)height; }
        public Dimensions(Vector2 v) { this.Width = (int)v.X; this.Height = (int)v.Y; }

        public bool IsValid() { return Width >= 0 && Height >= 0; }
        public float Area => Width * Height;

        public int MaxDimension => Width > Height ? Width : Height;
        public int MinDimension => Width < Height ? Width : Height;


        public Vector2 ToVector2() { return new Vector2(Width, Height); }

        public bool Equals(Dimensions other)
        {
            return Width == other.Width && Height == other.Height;
        }
        public override bool Equals(object? obj)
        {
            if (obj is Dimensions d)
            {
                return Equals(d);
            }
            return false;
        }
        public readonly override string ToString()
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

        public readonly override int GetHashCode()
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
                //if (x == null || y == null) return 0;

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
        public readonly Polygons NewShapes;
        public readonly Polygons Cutouts;
        public readonly Triangulation Pieces;

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
        public readonly List<Collision> Collisions;
        public readonly CollisionSurface CollisionSurface;
        public CollisionInformation(List<Collision> collisions, bool computesIntersections)
        {
            this.Collisions = collisions;
            if (!computesIntersections) this.CollisionSurface = new();
            else
            {
                Vector2 avgPoint = new();
                Vector2 avgNormal = new();
                var count = 0;
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
        public readonly bool FirstContact;
        public readonly ICollidable Self;
        public readonly ICollidable Other;
        public readonly Vector2 SelfVel;
        public readonly Vector2 OtherVel;
        public readonly Intersection Intersection;

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
        public readonly bool Valid;
        public readonly CollisionSurface CollisionSurface;
        public readonly CollisionPoints ColPoints;

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
                var count = 0;
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
    
    public readonly struct CollisionSurface
    {
        public readonly Vector2 Point;
        public readonly Vector2 Normal;
        public readonly bool Valid => Normal.X != 0f || Normal.Y != 0f;

        public CollisionSurface() { Point = new(); Normal = new();}
        public CollisionSurface(Vector2 point, Vector2 normal)
        {
            this.Point = point;
            this.Normal = normal;
        }

    }
    public readonly struct CollisionPoint : IEquatable<CollisionPoint>
    {
        public readonly bool Valid => Normal.X != 0f || Normal.Y != 0f;
        public readonly Vector2 Point;
        public readonly Vector2 Normal;

        public CollisionPoint() 
        { 
            Point = new(); 
            Normal = new();
        }

        public CollisionPoint(Vector2 p, Vector2 n)
        {
            Point = p; 
            Normal = n;
        }

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
                        if (!a.Points.Valid) return 1;
                        else if (!b.Points.Valid) return -1;
                        
                        float la = (origin - a.Points.Closest.Point).LengthSquared();
                        float lb = (origin - b.Points.Closest.Point).LengthSquared();
            
                        if (la > lb) return 1;
                        else if (MathF.Abs(la - lb) < 0.01f) return 0;
                        else return -1;
                    }
                );
            }
        }
    }
    public class QueryInfo
    {
        public readonly Vector2 Origin;
        public readonly ICollidable Collidable;
        public readonly QueryPoints Points;

        public QueryInfo(ICollidable collidable, Vector2 origin)
        {
            this.Collidable = collidable;
            this.Origin = origin;
            this.Points = new();
        }
        public QueryInfo(ICollidable collidable, Vector2 origin, CollisionPoints points)
        {
            this.Collidable = collidable;
            this.Origin = origin;
            this.Points = new(points, origin);
        }
    }
    public class QueryPoints
    {
        public readonly bool Valid;
        public readonly CollisionPoints Points;
        public readonly CollisionPoint Closest;

        public QueryPoints()
        {
            this.Valid = false;
            this.Points = new();
            this.Closest = new();
        }
        public QueryPoints(CollisionPoints points, Vector2 origin)
        {
            if(points.Count <= 0)
            {
                this.Valid = false;
                this.Points = new();
                this.Closest = new();
            }
            else
            {
                this.Valid = true;
                points.SortClosest(origin);
                this.Points = points;
                this.Closest = points[0];
            }
        }
    }
    
    
    public class CollisionPoints : ShapeList<CollisionPoint>
    {
        public CollisionPoints(params CollisionPoint[] points) { AddRange(points); }
        public CollisionPoints(IEnumerable<CollisionPoint> points) { AddRange(points); }


        public bool Valid => Count > 0;

        public void FlipNormals(Vector2 referencePoint)
        {
            for (var i = 0; i < Count; i++)
            {
                var p = this[i];
                var dir = referencePoint - p.Point;
                if (dir.IsFacingTheOppositeDirection(p.Normal))
                    this[i] = this[i].FlipNormal();
            }
        }
        
        
        public ClosestPoint GetClosestPoint(Vector2 p)
        {
            if (Count <= 0) return new();

            float minDisSquared = float.PositiveInfinity;
            CollisionPoint closestPoint = new();

            for (var i = 0; i < Count; i++)
            {
                var point = this[i];

                float disSquared = (point.Point - p).LengthSquared();
                if (disSquared > minDisSquared) continue;
                minDisSquared = disSquared;
                closestPoint = point;
            }
            return new(closestPoint, minDisSquared);
        }

        public override int GetHashCode() { return SUtils.GetHashCode(this); }
        public bool Equals(CollisionPoints? other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;
            for (var i = 0; i < Count; i++)
            {
                if (this[i].Equals(other[i])) return false;
            }
            return true;
        }


        public Points GetUniquePoints()
        {
            var uniqueVertices = new HashSet<Vector2>();
            for (var i = 0; i < Count; i++)
            {
                uniqueVertices.Add(this[i].Point);
            }
            return new(uniqueVertices);
        }
        public CollisionPoints GetUniqueCollisionPoints()
        {
            var unique = new HashSet<CollisionPoint>();
            for (var i = 0; i < Count; i++)
            {
                unique.Add(this[i]);
            }
            return new(unique);
        }

        public void SortClosest(Vector2 refPoint)
        {
            this.Sort
                (
                    comparison: (a, b) =>
                    {
                        float la = (refPoint - a.Point).LengthSquared();
                        float lb = (refPoint - b.Point).LengthSquared();

                        if (la > lb) return 1;
                        else if (MathF.Abs(x: la - lb) < 0.01f) return 0;
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