using System.Numerics;
using Clipper2Lib;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.StaticLib;

//CHECK: [DONE] Why does ShapeClipper need to flip y and ClipperImmediate does not?! -> Flipping is not needed, output triangles vertices are just in cw order if y is not flipped
//CHECK: [DONE] PolygonMath Triangulation & OutlineTriangulation vs ClipperImmediate Triangultion & OutlineTriangulation performance wise
// - Result they are about the same performance (so I dont need the old ones)

//TODO: 
// - [] Implement functions for turning a Polygon/List<Vector2> into a polyline by percentage/perimeter values -> use cached buffer to write new polyline into and use that buffer for generating the outline triangulation
// - [] Reimplement all functions from ShapeClipper here with optimizing memory allocation in mind
// - [] Clipper2 -> positive winding order = ccw/ filled shape and negative winding order = cw/hole -> if y is not flipped for using in clipper2 than this orientation changes where positive = cw/hole and negative = ccw/filled!
// - [] Reimplement all functions regarding Triangulation from PolygonMath here with optimizing memory allocation in mind
// - [] Use Clipper64 / ClipperOffset static classes or new wrapped classes if possible
// - [] Create seperate files for ShapeClipper enum wrappers
// - [] Move all clipper related wrapper classes to a seperate namespace
// - [] All functions should use Path64 / Paths64 instead of PathD / PathsD
// - [] All fuctions that return any sort of collection should use a parameter called result instead of a return value!
// - [] All major functions should have 1 variant that returns clipper based classes (Paths64 for instance) and 1 variant that returns shape engine based classes (Polygon for instance)
// - [] Functions that use result Parameter like Polygon/Polyline etc should use internal buffers for calculating everything and then transforming the result to expexted output format once at the end
// - [] At the end Rename this class to ShapeClipper and remove old ShapeClipper
// - [] Major Functions:
//  - [] TriangulateOutlinePerimeterPolygon
//  - [] TriangulateOutlinePercentagePolygon
//  - [DONE] Clip
//  - [DONE] Intersect
//  - [DONE] Difference
//  - [DONE] Union
//  - [DONE] Offset (instead of inflate)
//  - [DONE] TriangulateOutlinePolygon
//  - [DONE] TriangulatePolygon (polygons with and without holes) -> Paths64/Polygons is with holes and Path64/Polygon is without holes
//  - [DONE] TriangulateOutlinePolyline
// - [DONE] Implement an instance class that uses Clipper64 / ClipperOffset internally with automatic buffers etc. -> use those classes here as static engines
// - [DONE] Add conversion functions for Path64 / Paths64
// - [DONE] How to handle Triangulation vs TriMesh? -> I would opt for having both and just add explicit and implicit conversion functions (internally only TriMesh is used and there are optional overloads with Triangulation as result)


public class ShapeClipperOffset
{
    private readonly ClipperOffset offsetEngine;
    private readonly Path64 bufferPath64 = new(256);
    private ClipperScale scale;
    
    public ShapeClipperOffset(int decimalPlaces = 4, double miterLimit = 2.0, double arcTolerance = 0.0, bool preseveCollinear = false, bool reverseSolution = false)
    {
        offsetEngine = new(miterLimit, arcTolerance, preseveCollinear, reverseSolution);
        scale = new(decimalPlaces);
    }
    
    #region Offsetting
    public void OffsetPolygon(IReadOnlyList<Vector2> polygonCCW, float offset, float miterLimit, bool beveled, Paths64 result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonCCW.Count < 3) return;

        if (offset == 0f)
        {
            var path = new Path64();
            ClipperImmediate2D.ToPath64(polygonCCW, path);
            result.Add(path);
            return;
        }

        OffsetPolygonToPaths64(polygonCCW, offset, miterLimit, beveled, result);
    }

    public void OffsetPolyline(IReadOnlyList<Vector2> polyline, float offsetPositive, float miterLimit, bool beveled, EndType endType, Paths64 result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polyline.Count < 2 || offsetPositive <= 0f) return;

        OffsetPolylineToPaths64(polyline, offsetPositive, miterLimit, beveled, endType, result);
    }
    #endregion

    #region Private
    
    private void OffsetPolygonToPaths64(IReadOnlyList<Vector2> polygonCCW, float offsetWorld, float miterLimit, bool beveled, Paths64 outPaths)
    {
        outPaths.Clear();

        bufferPath64.Clear();
        ClipperImmediate2D.ToPath64(polygonCCW, bufferPath64);

        JoinType jt = SelectJoinType(miterLimit, beveled);

        offsetEngine.Clear();
        if (miterLimit > 2f) offsetEngine.MiterLimit = miterLimit;

        offsetEngine.AddPath(bufferPath64, jt, EndType.Polygon);

        double delta = offsetWorld * scale.Scale;
        offsetEngine.Execute(delta, outPaths);
    }

    private void OffsetPolylineToPaths64(IReadOnlyList<Vector2> polyline, float offsetWorldPositive, float miterLimit, bool beveled, EndType endType, Paths64 outPaths)
    {
        outPaths.Clear();

        bufferPath64.Clear();
        ClipperImmediate2D.ToPath64(polyline, bufferPath64);

        JoinType jt = SelectJoinType(miterLimit, beveled);

        offsetEngine.Clear();
        if (miterLimit > 2f) offsetEngine.MiterLimit = miterLimit;

        offsetEngine.AddPath(bufferPath64, jt, endType);

        double delta = offsetWorldPositive * scale.Scale;
        offsetEngine.Execute(delta, outPaths);
    }

    private JoinType SelectJoinType(float miterLimit, bool beveled)
    {
        if (miterLimit > 2f) return JoinType.Miter;
        return beveled ? JoinType.Bevel : JoinType.Square;
    }
    
    #endregion
}

public class ShapeClipper64
{
    private readonly Clipper64 clipper = new();
    private readonly Paths64PooledBuffer paths64SubjectBuffer = new();
    private readonly Paths64PooledBuffer paths64ClipBuffer = new();
    private readonly Path64 path64SubjectBuffer = new();
    private readonly Path64 path64ClipBuffer = new();
    private readonly Paths64 paths64SolutionBuffer = new();
    
    public ShapeClipperFillRule FillRule = ShapeClipperFillRule.NonZero;

    public void Execute(Paths64 subject, Paths64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipper.Clear();
        clipper.AddSubject(subject);
        clipper.AddClip(clip);
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(Paths64 subject, Path64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipper.Clear();
        clipper.AddSubject(subject);
        clipper.AddClip(clip);
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(Path64 subject, Paths64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipper.Clear();
        clipper.AddSubject(subject);
        clipper.AddClip(clip);
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(Path64 subject, Path64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipper.Clear();
        clipper.AddSubject(subject);
        clipper.AddClip(clip);
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    
    public void ExecuteManyClips(Paths64 subject, Paths64 clips, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        foreach (var c in clips)
        {
            clipper.Clear();
            //CHECK: Does this work! -> using solution here and in execute
            clipper.AddSubject(started ? solutionClosed : subject);
            clipper.AddClip(c);
            clipper.Execute(clipperClipType, clipperFillRule, solutionClosed);

            if (!started) started = true;
        }
    }
    
    public void ExecuteManyClips(Path64 subject, Paths64 clips, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        foreach (var c in clips)
        {
            clipper.Clear();
            //CHECK: Does this work! -> using solution here and in execute
            if(started)clipper.AddSubject(solutionClosed);
            else clipper.AddSubject(subject);
            clipper.AddClip(c);
            clipper.Execute(clipperClipType, clipperFillRule, solutionClosed);

            if (!started) started = true;
        }
    }
    
    
    public void Execute(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<IReadOnlyList<Vector2>> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipper.Clear();
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        paths64ClipBuffer.PrepareBuffer(clip.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        clip.ToPaths64(paths64ClipBuffer.Buffer);
        clipper.AddSubject(paths64SubjectBuffer.Buffer);
        clipper.AddClip(paths64ClipBuffer.Buffer);
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipper.Clear();
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        clipper.AddSubject(paths64SubjectBuffer.Buffer);
        clip.ToPath64(path64ClipBuffer);
        clipper.AddClip(path64ClipBuffer);
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<IReadOnlyList<Vector2>> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipper.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipper.AddSubject(path64SubjectBuffer);
        
        paths64ClipBuffer.PrepareBuffer(clip.Count);
        clip.ToPaths64(paths64ClipBuffer.Buffer);
        clipper.AddClip(paths64ClipBuffer.Buffer);
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipper.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipper.AddSubject(path64SubjectBuffer);
        clip.ToPath64(path64ClipBuffer);
        clipper.AddClip(path64ClipBuffer);
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    
    public void ExecuteManyClips(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<IReadOnlyList<Vector2>> clips, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        
        paths64ClipBuffer.PrepareBuffer(clips.Count);
        clips.ToPaths64(paths64ClipBuffer.Buffer);
        
        foreach (var c in paths64ClipBuffer.Buffer)
        {
            clipper.Clear();
            //CHECK: Does this work! -> using solution here and in execute
            clipper.AddSubject(started ? solutionClosed : paths64SubjectBuffer.Buffer);
            clipper.AddClip(c);
            clipper.Execute(clipperClipType, clipperFillRule, solutionClosed);

            if (!started) started = true;
        }
    }
    
    public void ExecuteManyClips(IReadOnlyList<Vector2> subject, IReadOnlyList<IReadOnlyList<Vector2>> clips, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        
        subject.ToPath64(path64ClipBuffer);
        
        paths64ClipBuffer.PrepareBuffer(clips.Count);
        clips.ToPaths64(paths64ClipBuffer.Buffer);
        
        foreach (var c in paths64ClipBuffer.Buffer)
        {
            clipper.Clear();
            //CHECK: Does this work! -> using solution here and in execute
            if(started)clipper.AddSubject(solutionClosed);
            else clipper.AddSubject(path64ClipBuffer);
            clipper.AddClip(c);
            clipper.Execute(clipperClipType, clipperFillRule, solutionClosed);

            if (!started) started = true;
        }
    }
    
    
    public void Execute(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<IReadOnlyList<Vector2>> clip, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        clipper.Clear();
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        paths64ClipBuffer.PrepareBuffer(clip.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        clip.ToPaths64(paths64ClipBuffer.Buffer);
        clipper.AddSubject(paths64SubjectBuffer.Buffer);
        clipper.AddClip(paths64ClipBuffer.Buffer);
        
        paths64SolutionBuffer.Clear();
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    public void Execute(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        clipper.Clear();
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        clipper.AddSubject(paths64SubjectBuffer.Buffer);
        clip.ToPath64(path64ClipBuffer);
        clipper.AddClip(path64ClipBuffer);
        
        paths64SolutionBuffer.Clear();
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<IReadOnlyList<Vector2>> clip, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        clipper.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipper.AddSubject(path64SubjectBuffer);
        
        paths64ClipBuffer.PrepareBuffer(clip.Count);
        clip.ToPaths64(paths64ClipBuffer.Buffer);
        clipper.AddClip(paths64ClipBuffer.Buffer);
        
        paths64SolutionBuffer.Clear();
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        clipper.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipper.AddSubject(path64SubjectBuffer);
        clip.ToPath64(path64ClipBuffer);
        clipper.AddClip(path64ClipBuffer);
        
        paths64SolutionBuffer.Clear();
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    
    public void ExecuteManyClips(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<IReadOnlyList<Vector2>> clips, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        
        paths64ClipBuffer.PrepareBuffer(clips.Count);
        clips.ToPaths64(paths64ClipBuffer.Buffer);
        
        paths64SolutionBuffer.Clear();
        
        foreach (var c in paths64ClipBuffer.Buffer)
        {
            clipper.Clear();
            //CHECK: Does this work! -> using solution here and in execute
            clipper.AddSubject(started ? paths64SolutionBuffer : paths64SubjectBuffer.Buffer);
            clipper.AddClip(c);
            clipper.Execute(clipperClipType, clipperFillRule, paths64SolutionBuffer);

            if (!started) started = true;
        }
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    public void ExecuteManyClips(IReadOnlyList<Vector2> subject, IReadOnlyList<IReadOnlyList<Vector2>> clips, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        
        subject.ToPath64(path64ClipBuffer);
        
        paths64ClipBuffer.PrepareBuffer(clips.Count);
        clips.ToPaths64(paths64ClipBuffer.Buffer);
        
        paths64SolutionBuffer.Clear();
        
        foreach (var c in paths64ClipBuffer.Buffer)
        {
            clipper.Clear();
            //CHECK: Does this work! -> using solution here and in execute
            if(started)clipper.AddSubject(paths64SolutionBuffer);
            else clipper.AddSubject(path64ClipBuffer);
            clipper.AddClip(c);
            clipper.Execute(clipperClipType, clipperFillRule, paths64SolutionBuffer);

            if (!started) started = true;
        }
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    
    public void Execute(Paths64 subject, Paths64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed, Paths64 solutionOpen)
    {
        clipper.Clear();
        clipper.AddSubject(subject);
        clipper.AddClip(clip);
        clipper.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed, solutionOpen);
    }
    
}

public readonly struct ClipperScale
{
    public readonly int DecimalPlaces;
    public readonly double Scale;
    public readonly double InvScale;
    
    public ClipperScale(int decimalPlaces = 4)
    {
        DecimalPlaces = Math.Clamp(decimalPlaces, 0, 8);;
        Scale = Pow10(DecimalPlaces);
        InvScale = 1.0 / Scale;
    }
    
    private double Pow10(int dp)
    {
        if (dp <= 0) return 1.0;
        double s = 1.0;
        for (int i = 0; i < dp; i++) s *= 10.0;
        return s;
    }
}

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
    
    public bool TriangulatePaths64ToMesh(Paths64 subject, bool useDelaunay)
    {
        Clear();

        TriangulateResult res = Clipper.Triangulate(subject, out Paths64 tris, useDelaunay);
        if (res != TriangulateResult.success || tris.Count == 0)
        {
            return false;
        }

        FillMesh(tris);
        return true;
    }

    private void FillMesh(Paths64 tris)
    {
        // Each Path64 in tris is a triangle with 3 points (CCW).
        int triCount = tris.Count;
        Triangles.Capacity = Math.Max(Triangles.Capacity, triCount * 3);
        
        for (int i = 0; i < triCount; i++)
        {
            var tri = tris[i];
            if (tri.Count < 3) continue;
        
            // Convert int coords back to world coords
            // because y is never flipped - return triangles are in cw order and flipping 0 with 1 turns them into ccw order!
            Vector2 a = ClipperImmediate2D.ToVec2(tri[1]);
            Vector2 b = ClipperImmediate2D.ToVec2(tri[0]);
            Vector2 c = ClipperImmediate2D.ToVec2(tri[2]);
        
            // enforce CCW (defensive)
            if (Cross(b - a, c - a) > 0f)
            {
                var tmp = b; b = c; c = tmp;
            }
        
            Triangles.Add(a);
            Triangles.Add(b);
            Triangles.Add(c);
        }
    }
    
    private float Cross(in Vector2 a, in Vector2 b) => a.X * b.Y - a.Y * b.X;

    public void ToTriangulation(Triangulation dst)
    {
        dst.Clear();
        for (int i = 0; i + 2 < Triangles.Count; i += 3)
        {
            Vector2 a = Triangles[i];
            Vector2 b = Triangles[i + 1];
            Vector2 c = Triangles[i + 2];
            ShapeEngine.Geometry.TriangleDef.Triangle t = new(a, b, c);
            dst.Add(t);
        }
    }
}

public sealed class Paths64PooledBuffer
{
    private Stack<Path64> path64Pool;

    public Paths64 Buffer = new();
        
    public Paths64PooledBuffer(int poolCapacity = 64)
    {
        path64Pool = new Stack<Path64>(poolCapacity);
    }

    public void PrepareBuffer(int targetCount)
    {
        if (Buffer.Count > targetCount)
        {
            for (int i = Buffer.Count - 1; i >= targetCount; i--)
            {
                var path = Buffer[i];
                Buffer.RemoveAt(i);
                ReturnPath64(path);
            }
        }
        else if (Buffer.Count < targetCount)
        {
            var diff = targetCount - Buffer.Count;
            for (int i = 0; i < diff; i++)
            {
                Buffer.Add(RentPath64());
            }
        }
    }
    public void ClearBuffer()
    {
        foreach (var path in Buffer)
        {
            ReturnPath64(path);
        }
        Buffer.Clear();
    }
        
    private Path64 RentPath64()
    {
        if (path64Pool.Count > 0) return path64Pool.Pop();
        return new Path64();
    }
    private void ReturnPath64(Path64 path64)
    {
        path64Pool.Push(path64);
    }
}

public static class ClipperImmediate2D
{
    #region Public Settings
    //NOTE: Moved to ClipperScale
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
    public static double Scale => _scale;
    //NOTE: ---
    
    
    //NOTE: Moved to ShapeClipper64
    /// <summary>Hard requirement from you: use NonZero fill rule.</summary>
    public static FillRule FillRule = FillRule.NonZero; //TODO: Change to ShapeClipperFillRule!

    /// <summary>Max cached triangulations (simple cap; cache clears when exceeded).</summary>
    public static int MaxTriangulationCacheEntries = 512;
    #endregion
    
    #region Private Settings
    //NOTE: Moved to ClipperScale
    private static int _decimalPlaces = 4;
    private static double _scale = Pow10(4);
    private static double _invScale = 1.0 / Pow10(4);
    //NOTE: ---
    
    static ClipperImmediate2D()
    {
        // ensure derived values set for default
        DecimalPlaces = 4;
    }
    #endregion
    
    #region Reused Clipper Engines
    //TODO: Replace with new classes
    private static readonly ClipperOffset _offset = new();
    private static readonly Clipper64 _clipper = new();
    #endregion
    
    #region Reused Buffers
    //CHECK: What is no longer needed
    private static readonly Path64 _tmpPath64 = new(256);
    private static readonly Paths64 _tmpOuter = new();
    private static readonly Paths64 _tmpInner = new();
    private static readonly Paths64 _tmpRing = new();
    private static readonly Paths64 _tmpStroke = new();
    private static readonly TriMesh _triMeshBuffer = new();


    private static readonly Paths64PooledBuffer paths64ConversionBuffer = new();
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
    
    public static void CreatePolygonTriangulation(Polygons polygonWithHoles, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonWithHoles.Count == 0) return;
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        TriangulatePaths64ToMesh(paths64ConversionBuffer.Buffer, useDelaunay, result);
    }
    
    public static void CreatePolygonOutlineTriangulation(IReadOnlyList<Vector2> polygonCCW, float thickness, float miterLimit, bool beveled, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolygonOutlineTriangulation(polygonCCW, thickness, miterLimit, beveled, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }

    public static void CreatePolylineTriangulation(IReadOnlyList<Vector2> polyline, float thickness, float miterLimit, bool beveled, EndType endType, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolylineTriangulation(polyline, thickness, miterLimit, beveled, endType, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }
    
    public static void CreatePolygonTriangulation(Paths64 polygonWithHoles, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolygonTriangulation(polygonWithHoles, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }
    
    public static void CreatePolygonTriangulation(Polygons polygonWithHoles, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolygonTriangulation(polygonWithHoles, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
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

    public static int CachePolygonTriangulation(Polygons polygonWithHoles, bool useDelaunay)
    {
        if (polygonWithHoles.Count == 0) return 0;
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        var key = TriKey.FromPaths64(paths64ConversionBuffer.Buffer, useDelaunay, DecimalPlaces);
        if (_keyToId.TryGetValue(key, out int id)) return id;

        EnsureCacheSpace();

        id = _nextTriId++;
        var mesh = RentMesh();
        CreatePolygonTriangulation(paths64ConversionBuffer.Buffer, useDelaunay, mesh);

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

    #region Rect Clipping
    public static Paths64 ClipRect(this Rect rect, Paths64 poly)
    {
        return Clipper.RectClip(rect.ToRect64(), poly);
    }
    public static Paths64 ClipRectLines(this Rect rect, Paths64 poly)
    {
        return Clipper.RectClipLines(rect.ToRect64(), poly);
    }
    public static Paths64 ClipRect(this Rect rect, List<Vector2> poly)
    {
        paths64ConversionBuffer.PrepareBuffer(poly.Count);
        poly.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RectClip(rect.ToRect64(), paths64ConversionBuffer.Buffer);
    }
    public static Paths64 ClipRectLines(this Rect rect, List<Vector2> poly)
    {
        paths64ConversionBuffer.PrepareBuffer(poly.Count);
        poly.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RectClipLines(rect.ToRect64(), paths64ConversionBuffer.Buffer);
    }
    public static Paths64 ClipRect(this Rect rect, List<List<Vector2>> polyWithHoles)
    {
        paths64ConversionBuffer.PrepareBuffer(polyWithHoles.Count);
        polyWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RectClip(rect.ToRect64(), paths64ConversionBuffer.Buffer);
    }
    public static Paths64 ClipRectLines(this Rect rect, List<List<Vector2>> polyWithHoles)
    {
        paths64ConversionBuffer.PrepareBuffer(polyWithHoles.Count);
        polyWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RectClipLines(rect.ToRect64(), paths64ConversionBuffer.Buffer);
    }
    #endregion
    
    #region Offsetting -> MOVED TO ShapeClipperOffset
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
    
    #region Internal Offset -> MOVED TO ShapeClipperOffset
    
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
    
    #region Internal Triangulation -> MOVED to TriMesh
    private static void TriangulatePaths64ToMesh(Paths64 subject, bool useDelaunay, TriMesh mesh)
    {
        mesh.Clear();
    
        TriangulateResult res = Clipper.Triangulate(subject, out Paths64 tris, useDelaunay);
        if (res != TriangulateResult.success || tris.Count == 0)
        {
            return;
        }
    
        FillMesh(mesh, tris);
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
            // because y is never flipped - return triangles are in cw order and flipping 0 with 1 turns them into ccw order!
            Vector2 a = ToVec2(tri[1]);
            Vector2 b = ToVec2(tri[0]);
            Vector2 c = ToVec2(tri[2]);
        
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
    //Single to Single
    public static void ToPath64(this IReadOnlyList<Vector2> src, Path64 dst)
    {
        dst.Clear();
        dst.EnsureCapacity(src.Count);

        for (int i = 0; i < src.Count; i++)
        {
            dst.Add(src[i].ToPoint64());
        }
    }
    public static void ToVector2List(this Path64 src, List<Vector2> dst)
    {
        dst.Clear();
        dst.EnsureCapacity(src.Count);
        
        for (int i = 0; i < src.Count; i++)
        {
            dst.Add(src[i].ToVec2());
        }
    }
    
    //Multi to Multi
    public static void ToPaths64(this IReadOnlyList<IReadOnlyList<Vector2>> src, Paths64 dst) 
    {
        for (int i = 0; i < src.Count; i++)
        {
            if(dst.Count <= i)
            {
                var dstItem = new Path64();
                dst.Add(dstItem);
                
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                
                srcItem.ToPath64(dstItem);
            }
            else
            {
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                var dstItem = dst[i];
                srcItem.ToPath64(dstItem);
            }
        }
        if (dst.Count > src.Count)
        {
            for (int i = dst.Count - 1; i >= src.Count; i--)
            {
                dst.RemoveAt(i);
            }
        }
    }
    public static void ToVector2Lists(this Paths64 src, List<List<Vector2>> dst)
    {
        for (int i = 0; i < src.Count; i++)
        {
            if(dst.Count <= i)
            {
                var dstItem = new List<Vector2>();
                dst.Add(dstItem);
                
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                
                srcItem.ToVector2List(dstItem);
            }
            else
            {
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                var dstItem = dst[i];
                srcItem.ToVector2List(dstItem);
            }
        }
        if (dst.Count > src.Count)
        {
            for (int i = dst.Count - 1; i >= src.Count; i--)
            {
                dst.RemoveAt(i);
            }
        }
    }
    
    //Single to Multi
    public static void ToPaths64(this IReadOnlyList<Vector2> src, Paths64 dst)
    {
        if (dst.Count <= 0)
        {
            var dstItem = new Path64();
            src.ToPath64(dstItem);
            dst.Add(dstItem);
        }
        else
        {
            var dstItem = dst[0];
            src.ToPath64(dstItem);
            if (dst.Count > 1)
            {
                dst.Clear();
                dst.Add(dstItem);
            }
        }
    }
    public static void ToVector2Lists(this Path64 src, List<List<Vector2>> dst)
    {
        if (dst.Count <= 0)
        {
            var dstItem = new List<Vector2>();
            src.ToVector2List(dstItem);
            dst.Add(dstItem);
        }
        else
        {
            var dstItem = dst[0];
            src.ToVector2List(dstItem);
            if (dst.Count > 1)
            {
                dst.Clear();
                dst.Add(dstItem);
            }
        }
    }

    public static Rect64 ToRect64(this Rect r)
    {
        long left   = (long)Math.Round(r.X * _scale);
        long top    = (long)Math.Round(r.Y * _scale);
        long right  = (long)Math.Round((r.X + r.Width) * _scale);
        long bottom = (long)Math.Round((r.Y + r.Height) * _scale);
        return new Rect64(left, top, right, bottom);
    }
    public static Rect ToRect(this Rect64 r)
    {
        float x = (float)(r.left * _invScale);
        float y = (float)(r.top * _invScale);
        float w = (float)((r.right - r.left) * _invScale);
        float h = (float)((r.bottom - r.top) * _invScale);

        return new Rect(x, y, w, h);
    }
    
    public static Point64 ToPoint64(this Vector2 v)
    {
        long x = (long)Math.Round(v.X * _scale);
        long y = (long)Math.Round(v.Y * _scale);
        return new Point64(x,y);
    }
    
    public static Vector2 ToVec2(this Point64 p)
    {
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
    
    #region Internal TriMesh Pooling
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
    
    //NOTE: Moved to TriMesh
    private static float Cross(in Vector2 a, in Vector2 b) => a.X * b.Y - a.Y * b.X;

    //NOTE: Moved to ClipperScale struct
    private static double Pow10(int dp)
    {
        if (dp <= 0) return 1.0;
        double s = 1.0;
        for (int i = 0; i < dp; i++) s *= 10.0;
        return s;
    }
    #endregion
}