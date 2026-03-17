using System;
using System.Collections.Generic;
using System.Numerics;
using Clipper2Lib;
using Raylib_cs;

//NOTE:
// - Triangulation should return Triangulation
// - TriMesh to Triangulation function
// - How to make it work with Polygon/Polyline without creating extra garbage?
// - Test and see its performance and GC performance

public static class ClipperImmediate2D
{
    // ---------------------------
    // Public settings
    // ---------------------------

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
    public static FillRule FillRule = FillRule.NonZero;

    /// <summary>Max cached triangulations (simple cap; cache clears when exceeded).</summary>
    public static int MaxTriangulationCacheEntries = 512;

    // ---------------------------
    // Public outputs
    // ---------------------------

    /// <summary>
    /// Allocation-friendly triangle mesh: vertices + indices (3 per triangle).
    /// Vertices are world-space Vector2.
    /// Indices reference Vertices.
    /// </summary>
    public sealed class TriMesh
    {
        public readonly List<Vector2> Vertices = new(256);
        public readonly List<int> Indices = new(512);
        public void Clear() { Vertices.Clear(); Indices.Clear(); }
    }

    // ---------------------------
    // Private settings (derived)
    // ---------------------------

    private static int _decimalPlaces = 4;
    private static double _scale = Pow10(4);
    private static double _invScale = 1.0 / Pow10(4);

    static ClipperImmediate2D()
    {
        // ensure derived values set for default
        DecimalPlaces = 4;
    }

    // ---------------------------
    // Reused engines (avoid per-call allocations)
    // ---------------------------

    private static readonly ClipperOffset _offset = new();
    private static readonly Clipper64 _clipper = new();

    // ---------------------------
    // Reused buffers
    // ---------------------------

    private static readonly Path64 _tmpPath64 = new(256);

    private static readonly Paths64 _tmpOuter = new();
    private static readonly Paths64 _tmpInner = new();
    private static readonly Paths64 _tmpRing = new();
    private static readonly Paths64 _tmpStroke = new();

    private static readonly Paths64 _tmpTriangles = new();

    // Cache
    private static int _nextTriId = 1;
    private static readonly Dictionary<TriKey, int> _keyToId = new(256);
    private static readonly Dictionary<int, TriMesh> _idToMesh = new(256);
    private static readonly Stack<TriMesh> _meshPool = new();

    // ---------------------------
    // Public: Drawing
    // ---------------------------

    public static void DrawPolygonOutline(
        IReadOnlyList<Vector2> polygonCCW,
        float thickness,
        Color color,
        float miterLimit,
        bool beveled,
        bool useDelaunay,
        bool cached)
    {
        if (polygonCCW == null || polygonCCW.Count < 3 || thickness <= 0f) return;

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
                DrawMesh(mesh, color);
            }
            finally { ReturnMesh(mesh); }
        }
    }

    public static void DrawPolyline(
        IReadOnlyList<Vector2> polyline,
        float thickness,
        Color color,
        float miterLimit,
        bool beveled,
        EndType endType,
        bool useDelaunay,
        bool cached)
    {
        if (polyline == null || polyline.Count < 2 || thickness <= 0f) return;

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
                DrawMesh(mesh, color);
            }
            finally { ReturnMesh(mesh); }
        }
    }

    public static void DrawCachedTriangulation(int triangulationId, Color color)
    {
        if (triangulationId == 0) return;
        if (!_idToMesh.TryGetValue(triangulationId, out var mesh)) return;
        DrawMesh(mesh, color);
    }

    // ---------------------------
    // Public: Create triangulations (no caching)
    // ---------------------------

    public static void CreatePolygonOutlineTriangulation(
        IReadOnlyList<Vector2> polygonCCW,
        float thickness,
        float miterLimit,
        bool beveled,
        bool useDelaunay,
        TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonCCW == null || polygonCCW.Count < 3 || thickness <= 0f) return;

        OffsetPolygonToPaths64(polygonCCW, +thickness, miterLimit, beveled, _tmpOuter);
        OffsetPolygonToPaths64(polygonCCW, -thickness, miterLimit, beveled, _tmpInner);
        if (_tmpOuter.Count == 0) return;

        // ring = outer - inner (allocation-free using Clipper64 engine)
        _tmpRing.Clear();
        _clipper.Clear();
        _clipper.AddSubject(_tmpOuter);
        _clipper.AddClip(_tmpInner);
        _clipper.Execute(ClipType.Difference, FillRule, _tmpRing);

        if (_tmpRing.Count == 0) return;

        TriangulatePaths64ToMesh(_tmpRing, useDelaunay, result);
    }

    public static void CreatePolylineTriangulation(
        IReadOnlyList<Vector2> polyline,
        float thickness,
        float miterLimit,
        bool beveled,
        EndType endType,
        bool useDelaunay,
        TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polyline == null || polyline.Count < 2 || thickness <= 0f) return;

        //TODO: thickness * 2 needed here?
        OffsetPolylineToPaths64(polyline, thickness, miterLimit, beveled, endType, _tmpStroke);
        if (_tmpStroke.Count == 0) return;

        TriangulatePaths64ToMesh(_tmpStroke, useDelaunay, result);
    }

    public static void CreatePolygonTriangulation(Paths64 polygonWithHoles, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonWithHoles == null || polygonWithHoles.Count == 0) return;
        TriangulatePaths64ToMesh(polygonWithHoles, useDelaunay, result);
    }

    // ---------------------------
    // Public: Cache triangulations (returns id)
    // ---------------------------

    public static int CachePolygonOutlineTriangulation(
        IReadOnlyList<Vector2> polygonCCW,
        float thickness,
        float miterLimit,
        bool beveled,
        bool useDelaunay)
    {
        if (polygonCCW == null || polygonCCW.Count < 3 || thickness <= 0f) return 0;

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

    public static int CachePolylineTriangulation(
        IReadOnlyList<Vector2> polyline,
        float thickness,
        float miterLimit,
        bool beveled,
        EndType endType,
        bool useDelaunay)
    {
        if (polyline == null || polyline.Count < 2 || thickness <= 0f) return 0;

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
        if (polygonWithHoles == null || polygonWithHoles.Count == 0) return 0;

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
        => _idToMesh.TryGetValue(triangulationId, out mesh!);

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

    // ---------------------------
    // Public: Offsetting wrappers (allocation-friendly result parameter)
    // ---------------------------

    public static void OffsetPolygon(
        IReadOnlyList<Vector2> polygonCCW,
        float offset,
        float miterLimit,
        bool beveled,
        Paths64 result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonCCW == null || polygonCCW.Count < 3) return;

        if (offset == 0f)
        {
            _tmpPath64.Clear();
            ToPath64(polygonCCW, _tmpPath64);
            result.Add(new Path64(_tmpPath64));
            return;
        }

        OffsetPolygonToPaths64(polygonCCW, offset, miterLimit, beveled, result);
    }

    public static void OffsetPolyline(
        IReadOnlyList<Vector2> polyline,
        float offsetPositive,
        float miterLimit,
        bool beveled,
        EndType endType,
        Paths64 result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polyline == null || polyline.Count < 2 || offsetPositive <= 0f) return;

        OffsetPolylineToPaths64(polyline, offsetPositive, miterLimit, beveled, endType, result);
    }

    // ---------------------------
    // Internal: Offset
    // ---------------------------

    private static void OffsetPolygonToPaths64(
        IReadOnlyList<Vector2> polygonCCW,
        float offsetWorld,
        float miterLimit,
        bool beveled,
        Paths64 outPaths)
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

    private static void OffsetPolylineToPaths64(
        IReadOnlyList<Vector2> polyline,
        float offsetWorldPositive,
        float miterLimit,
        bool beveled,
        EndType endType,
        Paths64 outPaths)
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

    // ---------------------------
    // Internal: Triangulation
    // ---------------------------

    private static void TriangulatePaths64ToMesh(Paths64 subject, bool useDelaunay, TriMesh mesh)
    {
        mesh.Clear();
        _tmpTriangles.Clear();

        TriangulateResult res = Clipper.Triangulate(subject, out Paths64 tris, useDelaunay);
        if (res != TriangulateResult.success || tris == null || tris.Count == 0)
            return;

        // Keep a reusable reference list; this is optional (you can iterate tris directly).
        _tmpTriangles.EnsureCapacity(tris.Count);
        for (int i = 0; i < tris.Count; i++)
            _tmpTriangles.Add(tris[i]);

        BuildMeshFromTrianglePaths(_tmpTriangles, mesh);
    }

    private static void BuildMeshFromTrianglePaths(Paths64 triangles, TriMesh mesh)
    {
        mesh.Vertices.Clear();
        mesh.Indices.Clear();

        int triCount = triangles.Count;
        mesh.Vertices.Capacity = Math.Max(mesh.Vertices.Capacity, triCount * 3);
        mesh.Indices.Capacity = Math.Max(mesh.Indices.Capacity, triCount * 3);

        for (int i = 0; i < triCount; i++)
        {
            var t = triangles[i];
            if (t.Count < 3) continue;

            int baseIndex = mesh.Vertices.Count;

            Vector2 a = ToV2(t[0]);
            Vector2 b = ToV2(t[1]);
            Vector2 c = ToV2(t[2]);

            // enforce CCW (should already be CCW)
            if (Cross(b - a, c - a) < 0f)
            {
                var tmp = b; b = c; c = tmp;
            }

            mesh.Vertices.Add(a);
            mesh.Vertices.Add(b);
            mesh.Vertices.Add(c);

            mesh.Indices.Add(baseIndex);
            mesh.Indices.Add(baseIndex + 1);
            mesh.Indices.Add(baseIndex + 2);
        }
    }

    // ---------------------------
    // Internal: Conversion
    // ---------------------------

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
        => new((float)(p.X * _invScale), (float)(p.Y * _invScale));

    // ---------------------------
    // Internal: Cache
    // ---------------------------

    private static void EnsureCacheSpace()
    {
        if (_idToMesh.Count < MaxTriangulationCacheEntries) return;
        ClearTriangulationCache();
    }

    private readonly struct TriKey : IEquatable<TriKey>
    {
        public readonly ulong Hash;
        public readonly int DecimalPlaces;
        public readonly byte Kind;
        public readonly bool UseDelaunay;

        private TriKey(ulong hash, int dp, byte kind, bool useDelaunay)
        {
            Hash = hash;
            DecimalPlaces = dp;
            Kind = kind;
            UseDelaunay = useDelaunay;
        }

        public static TriKey FromPolygonOutline(
            IReadOnlyList<Vector2> polygon,
            float thickness,
            float miterLimit,
            bool beveled,
            bool useDelaunay,
            int dp)
        {
            ulong h = HashPoints(polygon, dp);
            h = HashFloat(h, thickness, dp);
            h = HashFloat(h, miterLimit, dp);
            h = HashBool(h, beveled);
            return new TriKey(h, dp, kind: 1, useDelaunay);
        }

        public static TriKey FromPolyline(
            IReadOnlyList<Vector2> polyline,
            float thickness,
            float miterLimit,
            bool beveled,
            EndType endType,
            bool useDelaunay,
            int dp)
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
            Hash == other.Hash && DecimalPlaces == other.DecimalPlaces && Kind == other.Kind && UseDelaunay == other.UseDelaunay;

        public override bool Equals(object? obj) => obj is TriKey k && Equals(k);

        public override int GetHashCode()
        {
            unchecked
            {
                int hc = (int)(Hash ^ (Hash >> 32));
                hc = (hc * 397) ^ DecimalPlaces;
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

                double scale = Pow10(dp);
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
            double scale = Pow10(dp);
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

        private static double Pow10(int dp)
        {
            if (dp <= 0) return 1.0;
            double s = 1.0;
            for (int i = 0; i < dp; i++) s *= 10.0;
            return s;
        }
    }

    // ---------------------------
    // Internal: Pooling + draw
    // ---------------------------

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

    private static void DrawMesh(TriMesh mesh, Color color)
    {
        var verts = mesh.Vertices;
        var inds = mesh.Indices;

        for (int i = 0; i + 2 < inds.Count; i += 3)
        {
            Vector2 a = verts[inds[i]];
            Vector2 b = verts[inds[i + 1]];
            Vector2 c = verts[inds[i + 2]];

            // enforce CCW
            if (Cross(b - a, c - a) < 0f)
            {
                var t = b; b = c; c = t;
            }

            Raylib.DrawTriangle(a, b, c, color);
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
}