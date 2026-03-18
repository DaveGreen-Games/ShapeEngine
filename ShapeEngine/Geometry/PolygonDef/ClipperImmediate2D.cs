using System.Numerics;
using Clipper2Lib;
using Raylib_cs;
using ShapeEngine.Color;

//NOTE:
// - Triangulation should return Triangulation
// - TriMesh to Triangulation function
// - How to make it work with Polygon/Polyline without creating extra garbage?
// - Test and see its performance and GC performance

//TODO: In all public functions make sure that JoinType, FillRule, EndType use ShapeEngine wrapper enums!
//TODO: Add Path64 and Paths64 Conversions to ShapeClipper or to here
//TODO: Should I add this class to ShapeClipper instead of having its own class for it? I could make this a partial ShapeClipper class to keep it seperate
public static class ClipperImmediate2D
{
    #region Public Settings
    /// <summary>
    /// Decimal places used for:
    /// 1) Scaling Vector2 (world) -> Point64 (Clipper) by Scale = 10^DecimalPlaces
    /// 2) Clipper.Triangulate's decimalPlaces parameter
    /// Typical values: 2..5. Default = 4.
    /// </summary>
    public static int DecimalPlaces
    {
        get => _decimalPlaces;
        set
        {
            _decimalPlaces = Math.Clamp(value, 0, 8);
            _scale = Pow10(_decimalPlaces);
            _invScale = 1.0 / _scale;
        }
    }

    /// <summary>Hard requirement from you: use NonZero fill rule.</summary>
    public static FillRule FillRule = FillRule.NonZero; //TODO: Change to ShapeClipperFillRule!

    /// <summary>Max cached triangulations (simple cap; cache clears when exceeded).</summary>
    public static int MaxTriangulationCacheEntries = 512;
    #endregion
    
    #region Public Outputs
    //TODO: I could keep TriMesh and not use Triangulation here? 
    //TODO: I could also rework Triangulation to work like TriMesh?
    //TODO: I could also keep both and have conversion between the 2?
    //TODO: If I keep TriMesh then I have to add Translation, Rotation, Scaling methods and more to it
    public sealed class TriMesh
    {
        /// <summary>
        /// Flat list of triangles: every 3 consecutive vertices form one CCW triangle.
        /// </summary>
        public readonly List<Vector2> Triangles = new(512);

        public void Clear() => Triangles.Clear();

        public void Draw(ColorRgba color)
        {
            var t = Triangles;
            var rayColor = color.ToRayColor();
            for (int i = 0; i + 2 < t.Count; i += 3)
            {
                Vector2 a = t[i];
                Vector2 b = t[i + 1];
                Vector2 c = t[i + 2];

                Raylib.DrawTriangle(a, b, c, rayColor);
            }
        }
    }
    #endregion
    
    #region Private Settings
    private static int _decimalPlaces = 4;
    private static double _scale = Pow10(4);
    private static double _invScale = 1.0 / Pow10(4);

    static ClipperImmediate2D()
    {
        // ensure derived values set for default
        DecimalPlaces = 4;
    }
    #endregion
    
    #region Reused Clipper Engines
    private static readonly ClipperOffset _offset = new();
    private static readonly Clipper64 _clipper = new();
    #endregion
    
    #region Reused Buffers
    private static readonly Path64 _tmpPath64 = new(256);
    private static readonly Paths64 _tmpOuter = new();
    private static readonly Paths64 _tmpInner = new();
    private static readonly Paths64 _tmpRing = new();
    private static readonly Paths64 _tmpStroke = new();
    #endregion

    #region Cache
    private static int _nextTriId = 1;
    private static readonly Dictionary<TriKey, int> _keyToId = new(256);
    private static readonly Dictionary<int, TriMesh> _idToMesh = new(256);
    private static readonly Stack<TriMesh> _meshPool = new();
    #endregion
    
    #region Drawing
    //Info: Caching should be used for static polygons / polylines only! Otherwise create triangulation and then move, scale, rotate triangulation and then draw triangulation each frame
    public static void DrawPolygonOutline(IReadOnlyList<Vector2> polygonCCW, float thickness, ColorRgba color, float miterLimit = 2f, bool beveled = false, bool useDelaunay = false, bool cached = false)
    {
        if (polygonCCW.Count < 3 || thickness <= 0f) return;

        if (cached)
        {
            int id = CachePolygonOutlineTriangulation(polygonCCW, thickness, miterLimit, beveled, useDelaunay);
            DrawCachedTriangulation(id, color);
        }
        else
        {
            var mesh = RentMesh();
            try
            {
                CreatePolygonOutlineTriangulation(polygonCCW, thickness, miterLimit, beveled, useDelaunay, mesh);
                mesh.Draw(color);
            }
            finally { ReturnMesh(mesh); }
        }
    }

    public static void DrawPolyline(IReadOnlyList<Vector2> polyline, float thickness, ColorRgba color, float miterLimit = 2f, bool beveled = false, EndType endType = EndType.Butt, bool useDelaunay = false, bool cached = false)
    {
        if (polyline.Count < 2 || thickness <= 0f) return;

        if (cached)
        {
            int id = CachePolylineTriangulation(polyline, thickness, miterLimit, beveled, endType, useDelaunay);
            DrawCachedTriangulation(id, color);
        }
        else
        {
            var mesh = RentMesh();
            try
            {
                CreatePolylineTriangulation(polyline, thickness, miterLimit, beveled, endType, useDelaunay, mesh);
                mesh.Draw(color);
            }
            finally { ReturnMesh(mesh); }
        }
    }

    public static void DrawCachedTriangulation(int triangulationId, ColorRgba color)
    {
        if (triangulationId == 0) return;
        if (!_idToMesh.TryGetValue(triangulationId, out var mesh)) return;
        mesh.Draw(color);
    }
    #endregion
    
    #region Triangulation
    public static void CreatePolygonOutlineTriangulation(IReadOnlyList<Vector2> polygonCCW, float thickness, float miterLimit, bool beveled, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonCCW.Count < 3 || thickness <= 0f)
        {
            Console.WriteLine("Triangulating failed - Count less than 3 or thickness less than 0");
            return;
        }

        OffsetPolygonToPaths64(polygonCCW, +thickness, miterLimit, beveled, _tmpOuter);
        OffsetPolygonToPaths64(polygonCCW, -thickness, miterLimit, beveled, _tmpInner);
        if (_tmpOuter.Count == 0)
        {
            Console.WriteLine("Offsetting failed");
            return;
        }

        // ring = outer - inner (allocation-free using Clipper64 engine)
        _tmpRing.Clear();
        _clipper.Clear();
        _clipper.AddSubject(_tmpOuter);
        _clipper.AddClip(_tmpInner);
        _clipper.Execute(ClipType.Difference, FillRule, _tmpRing);

        if (_tmpRing.Count == 0)
        {
            Console.WriteLine("Clipping failed");
            return;
        }

        TriangulatePaths64ToMesh(_tmpRing, useDelaunay, result);
    }

    public static void CreatePolylineTriangulation(IReadOnlyList<Vector2> polyline, float thickness, float miterLimit, bool beveled, EndType endType, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polyline.Count < 2 || thickness <= 0f) return;

        OffsetPolylineToPaths64(polyline, thickness, miterLimit, beveled, endType, _tmpStroke);
        if (_tmpStroke.Count == 0) return;

        TriangulatePaths64ToMesh(_tmpStroke, useDelaunay, result);
    }

    public static void CreatePolygonTriangulation(Paths64 polygonWithHoles, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonWithHoles.Count == 0) return;
        TriangulatePaths64ToMesh(polygonWithHoles, useDelaunay, result);
    }
    #endregion
    
    #region Cached Triangulation
    public static int CachePolygonOutlineTriangulation(IReadOnlyList<Vector2> polygonCCW, float thickness, float miterLimit, bool beveled, bool useDelaunay)
    {
        if (polygonCCW.Count < 3 || thickness <= 0f) return 0;

        var key = TriKey.FromPolygonOutline(polygonCCW, thickness, miterLimit, beveled, useDelaunay, DecimalPlaces);
        if (_keyToId.TryGetValue(key, out int id)) return id;

        EnsureCacheSpace();

        id = _nextTriId++;
        var mesh = RentMesh();
        CreatePolygonOutlineTriangulation(polygonCCW, thickness, miterLimit, beveled, useDelaunay, mesh);

        _keyToId[key] = id;
        _idToMesh[id] = mesh;
        return id;
    }

    public static int CachePolylineTriangulation(IReadOnlyList<Vector2> polyline, float thickness, float miterLimit, bool beveled, EndType endType, bool useDelaunay)
    {
        if (polyline.Count < 2 || thickness <= 0f) return 0;

        var key = TriKey.FromPolyline(polyline, thickness, miterLimit, beveled, endType, useDelaunay, DecimalPlaces);
        if (_keyToId.TryGetValue(key, out int id)) return id;

        EnsureCacheSpace();

        id = _nextTriId++;
        var mesh = RentMesh();
        CreatePolylineTriangulation(polyline, thickness, miterLimit, beveled, endType, useDelaunay, mesh);

        _keyToId[key] = id;
        _idToMesh[id] = mesh;
        return id;
    }

    public static int CachePolygonTriangulation(Paths64 polygonWithHoles, bool useDelaunay)
    {
        if (polygonWithHoles.Count == 0) return 0;

        var key = TriKey.FromPaths64(polygonWithHoles, useDelaunay, DecimalPlaces);
        if (_keyToId.TryGetValue(key, out int id)) return id;

        EnsureCacheSpace();

        id = _nextTriId++;
        var mesh = RentMesh();
        CreatePolygonTriangulation(polygonWithHoles, useDelaunay, mesh);

        _keyToId[key] = id;
        _idToMesh[id] = mesh;
        return id;
    }

    public static bool GetCachedTriangulation(int triangulationId, out TriMesh mesh)
    {
        return _idToMesh.TryGetValue(triangulationId, out mesh!);
    }

    public static void ClearTriangulationCache()
    {
        _keyToId.Clear();

        foreach (var kv in _idToMesh)
        {
            kv.Value.Clear();
            _meshPool.Push(kv.Value);
        }
        _idToMesh.Clear();
    }
    #endregion
    
    #region Offsetting
    public static void OffsetPolygon(IReadOnlyList<Vector2> polygonCCW, float offset, float miterLimit, bool beveled, Paths64 result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonCCW.Count < 3) return;

        if (offset == 0f)
        {
            _tmpPath64.Clear();
            ToPath64(polygonCCW, _tmpPath64);
            result.Add(new Path64(_tmpPath64));
            return;
        }

        OffsetPolygonToPaths64(polygonCCW, offset, miterLimit, beveled, result);
    }

    public static void OffsetPolyline(IReadOnlyList<Vector2> polyline, float offsetPositive, float miterLimit, bool beveled, EndType endType, Paths64 result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polyline.Count < 2 || offsetPositive <= 0f) return;

        OffsetPolylineToPaths64(polyline, offsetPositive, miterLimit, beveled, endType, result);
    }
    #endregion
    
    #region Internal Offset
    
    private static void OffsetPolygonToPaths64(IReadOnlyList<Vector2> polygonCCW, float offsetWorld, float miterLimit, bool beveled, Paths64 outPaths)
    {
        outPaths.Clear();

        _tmpPath64.Clear();
        ToPath64(polygonCCW, _tmpPath64);

        JoinType jt = SelectJoinType(miterLimit, beveled);

        _offset.Clear();
        if (miterLimit > 2f) _offset.MiterLimit = miterLimit;

        _offset.AddPath(_tmpPath64, jt, EndType.Polygon);

        double delta = offsetWorld * _scale;
        _offset.Execute(delta, outPaths);
    }

    private static void OffsetPolylineToPaths64(IReadOnlyList<Vector2> polyline, float offsetWorldPositive, float miterLimit, bool beveled, EndType endType, Paths64 outPaths)
    {
        outPaths.Clear();

        _tmpPath64.Clear();
        ToPath64(polyline, _tmpPath64);

        JoinType jt = SelectJoinType(miterLimit, beveled);

        _offset.Clear();
        if (miterLimit > 2f) _offset.MiterLimit = miterLimit;

        _offset.AddPath(_tmpPath64, jt, endType);

        double delta = offsetWorldPositive * _scale;
        _offset.Execute(delta, outPaths);
    }

    private static JoinType SelectJoinType(float miterLimit, bool beveled)
    {
        // your rule
        if (miterLimit > 2f) return JoinType.Miter;
        return beveled ? JoinType.Bevel : JoinType.Square;
    }
    #endregion
    
    #region Internal Triangulation
    private static void TriangulatePaths64ToMesh(Paths64 subject, bool useDelaunay, TriMesh mesh)
    {
        mesh.Clear();

        TriangulateResult res = Clipper.Triangulate(subject, out Paths64 tris, useDelaunay);
        if (res != TriangulateResult.success || tris.Count == 0)
        {
            Console.WriteLine("TriangulatePaths64ToMesh failed");
            return;
        }

        FillMesh(mesh, tris);
        // // Each Path64 in tris is a triangle with 3 points (CCW).
        // int triCount = tris.Count;
        // mesh.Triangles.Capacity = Math.Max(mesh.Triangles.Capacity, triCount * 3);
        //
        // for (int i = 0; i < triCount; i++)
        // {
        //     var tri = tris[i];
        //     if (tri.Count < 3) continue;
        //
        //     // Convert int coords back to world coords
        //     Vector2 a = ToV2(tri[0]);
        //     Vector2 b = ToV2(tri[1]);
        //     Vector2 c = ToV2(tri[2]);
        //
        //     // // enforce CCW (defensive)
        //     // if (Cross(b - a, c - a) > 0f)
        //     // {
        //     //     Console.WriteLine("Enforce CCW triangle vertices");
        //     //     var tmp = b; b = c; c = tmp;
        //     // }
        //
        //     mesh.Triangles.Add(a);
        //     mesh.Triangles.Add(b);
        //     mesh.Triangles.Add(c);
        // }
    }

    private static void FillMesh(TriMesh mesh, Paths64 tris)
    {
        // Each Path64 in tris is a triangle with 3 points (CCW).
        int triCount = tris.Count;
        mesh.Triangles.Capacity = Math.Max(mesh.Triangles.Capacity, triCount * 3);
        
        for (int i = 0; i < triCount; i++)
        {
            var tri = tris[i];
            if (tri.Count < 3) continue;
        
            // Convert int coords back to world coords
            Vector2 a = ToV2(tri[0]);
            Vector2 b = ToV2(tri[1]);
            Vector2 c = ToV2(tri[2]);
        
            // enforce CCW (defensive)
            if (Cross(b - a, c - a) > 0f)
            {
                var tmp = b; b = c; c = tmp;
            }
        
            mesh.Triangles.Add(a);
            mesh.Triangles.Add(b);
            mesh.Triangles.Add(c);
        }
    }
    #endregion
    
    #region Internal Conversion
    private static void ToPath64(IReadOnlyList<Vector2> src, Path64 dst)
    {
        dst.Clear();
        dst.EnsureCapacity(src.Count);

        double scale = _scale;
        for (int i = 0; i < src.Count; i++)
        {
            var v = src[i];
            long x = (long)Math.Round(v.X * scale);
            long y = (long)Math.Round(v.Y * scale);
            dst.Add(new Point64(x, y));
        }
    }

    private static Vector2 ToV2(Point64 p)
    {
        // Console.WriteLine($"Point64 {p.X}, {p.Y} -> Vector2 {(float)(p.X * _invScale)}, {-(float)(p.Y * _invScale)}");
        return new Vector2((float)(p.X * _invScale), (float)(p.Y * _invScale));
    }
    #endregion
    
    #region Internal Cache
    private static void EnsureCacheSpace()
    {
        if (_idToMesh.Count < MaxTriangulationCacheEntries) return;
        ClearTriangulationCache();
    }

    private readonly struct TriKey : IEquatable<TriKey>
    {
        public readonly ulong Hash;
        public readonly int DP;
        public readonly byte Kind;
        public readonly bool UseDelaunay;

        private TriKey(ulong hash, int dp, byte kind, bool useDelaunay)
        {
            Hash = hash;
            DP = dp;
            Kind = kind;
            UseDelaunay = useDelaunay;
        }

        public static TriKey FromPolygonOutline(IReadOnlyList<Vector2> polygon, float thickness, float miterLimit, bool beveled, bool useDelaunay, int dp)
        {
            ulong h = HashPoints(polygon, dp);
            h = HashFloat(h, thickness, dp);
            h = HashFloat(h, miterLimit, dp);
            h = HashBool(h, beveled);
            return new TriKey(h, dp, kind: 1, useDelaunay);
        }

        public static TriKey FromPolyline(IReadOnlyList<Vector2> polyline, float thickness, float miterLimit, bool beveled, EndType endType, bool useDelaunay, int dp)
        {
            ulong h = HashPoints(polyline, dp);
            h = HashFloat(h, thickness, dp);
            h = HashFloat(h, miterLimit, dp);
            h = HashBool(h, beveled);
            h = HashInt(h, (int)endType);
            return new TriKey(h, dp, kind: 2, useDelaunay);
        }

        public static TriKey FromPaths64(Paths64 paths, bool useDelaunay, int dp)
        {
            ulong h = HashPaths64(paths);
            return new TriKey(h, dp, kind: 3, useDelaunay);
        }

        public bool Equals(TriKey other) =>
            Hash == other.Hash && DP == other.DP && Kind == other.Kind && UseDelaunay == other.UseDelaunay;

        public override bool Equals(object? obj) => obj is TriKey k && Equals(k);

        public override int GetHashCode()
        {
            unchecked
            {
                int hc = (int)(Hash ^ (Hash >> 32));
                hc = (hc * 397) ^ DP;
                hc = (hc * 397) ^ Kind;
                hc = (hc * 397) ^ (UseDelaunay ? 1 : 0);
                return hc;
            }
        }

        private static ulong HashPoints(IReadOnlyList<Vector2> pts, int dp)
        {
            const ulong FNV_OFFSET = 14695981039346656037UL;
            const ulong FNV_PRIME = 1099511628211UL;

            ulong h = FNV_OFFSET;
            unchecked
            {
                h ^= (ulong)pts.Count; h *= FNV_PRIME;

                double scale = ToScale(dp);
                for (int i = 0; i < pts.Count; i++)
                {
                    long qx = (long)Math.Round(pts[i].X * scale);
                    long qy = (long)Math.Round(pts[i].Y * scale);
                    h ^= (ulong)qx; h *= FNV_PRIME;
                    h ^= (ulong)qy; h *= FNV_PRIME;
                }
            }
            return h;
        }

        private static ulong HashPaths64(Paths64 paths)
        {
            const ulong FNV_OFFSET = 14695981039346656037UL;
            const ulong FNV_PRIME = 1099511628211UL;

            ulong h = FNV_OFFSET;
            unchecked
            {
                h ^= (ulong)paths.Count; h *= FNV_PRIME;

                for (int i = 0; i < paths.Count; i++)
                {
                    var path = paths[i];
                    h ^= (ulong)path.Count; h *= FNV_PRIME;

                    for (int j = 0; j < path.Count; j++)
                    {
                        var p = path[j];
                        h ^= (ulong)p.X; h *= FNV_PRIME;
                        h ^= (ulong)p.Y; h *= FNV_PRIME;
                    }
                }
            }
            return h;
        }

        private static ulong HashFloat(ulong h, float v, int dp)
        {
            const ulong FNV_PRIME = 1099511628211UL;
            double scale = ToScale(dp);
            long q = (long)Math.Round(v * scale);
            unchecked { h ^= (ulong)q; h *= FNV_PRIME; }
            return h;
        }

        private static ulong HashInt(ulong h, int v)
        {
            const ulong FNV_PRIME = 1099511628211UL;
            unchecked { h ^= (ulong)v; h *= FNV_PRIME; }
            return h;
        }

        private static ulong HashBool(ulong h, bool v)
        {
            const ulong FNV_PRIME = 1099511628211UL;
            unchecked { h ^= (ulong)(v ? 1 : 0); h *= FNV_PRIME; }
            return h;
        }

        private static double ToScale(int dp)
        {
            if (dp <= 0) return 1.0;
            double s = 1.0;
            for (int i = 0; i < dp; i++) s *= 10.0;
            return s;
        }
    }
    #endregion
    
    #region Internal Pooling + Draw
    private static TriMesh RentMesh()
    {
        if (_meshPool.Count > 0)
        {
            var m = _meshPool.Pop();
            m.Clear();
            return m;
        }
        return new TriMesh();
    }

    private static void ReturnMesh(TriMesh mesh)
    {
        if (_meshPool.Count < MaxTriangulationCacheEntries)
        {
            mesh.Clear();
            _meshPool.Push(mesh);
        }
    }
    
    private static float Cross(in Vector2 a, in Vector2 b) => a.X * b.Y - a.Y * b.X;

    private static double Pow10(int dp)
    {
        if (dp <= 0) return 1.0;
        double s = 1.0;
        for (int i = 0; i < dp; i++) s *= 10.0;
        return s;
    }
    #endregion
}