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
    private ClipperOffset offsetEngine;
    public ClipperScale Scale;
    private readonly Path64 bufferPath64 = new(256);
    
    public double MiterLimit
    {
        get => offsetEngine.MiterLimit;
        set => offsetEngine.MiterLimit = value;
    }

    public double ArcTolerance
    {
        get => offsetEngine.ArcTolerance;
        set => offsetEngine.ArcTolerance = value;
    }

    public bool PreseveCollinear
    {
        get => offsetEngine.PreserveCollinear;
        set => offsetEngine.PreserveCollinear = value;
    }

    public bool ReverseSolution
    {
        get => offsetEngine.ReverseSolution;
        set => offsetEngine.ReverseSolution = value;
    }
    
    public ShapeClipperOffset(int decimalPlaces = 4, double miterLimit = 2.0, double arcTolerance = 0.0, bool preseveCollinear = false, bool reverseSolution = false)
    {
        offsetEngine = new(miterLimit, arcTolerance, preseveCollinear, reverseSolution);
        Scale = new(decimalPlaces);
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

        double delta = offsetWorld * Scale.Scale;
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

        double delta = offsetWorldPositive * Scale.Scale;
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
    private readonly Clipper64 clipEngine;
    private readonly Paths64PooledBuffer paths64SubjectBuffer = new();
    private readonly Paths64PooledBuffer paths64ClipBuffer = new();
    private readonly Path64 path64SubjectBuffer = new();
    private readonly Path64 path64ClipBuffer = new();
    private readonly Paths64 paths64SolutionBuffer = new();
    
    public ShapeClipperFillRule FillRule = ShapeClipperFillRule.NonZero;

    public bool PreserveCollinear
    {
        get => clipEngine.PreserveCollinear;
        set => clipEngine.PreserveCollinear = value;
    }

    public bool ReverseSolution
    {
        get => clipEngine.ReverseSolution;
        set => clipEngine.ReverseSolution = value;
    }

    public ShapeClipper64(bool preserveCollinear = true, bool reverseSolution = false)
    {
        clipEngine = new();
        clipEngine.PreserveCollinear = preserveCollinear;
        clipEngine.ReverseSolution = reverseSolution;
    }
    
    public void Execute(Paths64 subject, Paths64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(Paths64 subject, Path64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(Path64 subject, Paths64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(Path64 subject, Path64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    
    public void ExecuteManyClips(Paths64 subject, Paths64 clips, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        foreach (var c in clips)
        {
            clipEngine.Clear();
            //CHECK: Does this work! -> using solution here and in execute
            clipEngine.AddSubject(started ? solutionClosed : subject);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, solutionClosed);

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
            clipEngine.Clear();
            //CHECK: Does this work! -> using solution here and in execute
            if(started)clipEngine.AddSubject(solutionClosed);
            else clipEngine.AddSubject(subject);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, solutionClosed);

            if (!started) started = true;
        }
    }
    
    
    public void Execute(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<IReadOnlyList<Vector2>> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        paths64ClipBuffer.PrepareBuffer(clip.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        clip.ToPaths64(paths64ClipBuffer.Buffer);
        clipEngine.AddSubject(paths64SubjectBuffer.Buffer);
        clipEngine.AddClip(paths64ClipBuffer.Buffer);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        clipEngine.AddSubject(paths64SubjectBuffer.Buffer);
        clip.ToPath64(path64ClipBuffer);
        clipEngine.AddClip(path64ClipBuffer);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<IReadOnlyList<Vector2>> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipEngine.AddSubject(path64SubjectBuffer);
        
        paths64ClipBuffer.PrepareBuffer(clip.Count);
        clip.ToPaths64(paths64ClipBuffer.Buffer);
        clipEngine.AddClip(paths64ClipBuffer.Buffer);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipEngine.AddSubject(path64SubjectBuffer);
        clip.ToPath64(path64ClipBuffer);
        clipEngine.AddClip(path64ClipBuffer);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
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
            clipEngine.Clear();
            //CHECK: Does this work! -> using solution here and in execute
            clipEngine.AddSubject(started ? solutionClosed : paths64SubjectBuffer.Buffer);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, solutionClosed);

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
            clipEngine.Clear();
            //CHECK: Does this work! -> using solution here and in execute
            if(started)clipEngine.AddSubject(solutionClosed);
            else clipEngine.AddSubject(path64ClipBuffer);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, solutionClosed);

            if (!started) started = true;
        }
    }
    
    
    public void Execute(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<IReadOnlyList<Vector2>> clip, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        clipEngine.Clear();
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        paths64ClipBuffer.PrepareBuffer(clip.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        clip.ToPaths64(paths64ClipBuffer.Buffer);
        clipEngine.AddSubject(paths64SubjectBuffer.Buffer);
        clipEngine.AddClip(paths64ClipBuffer.Buffer);
        
        paths64SolutionBuffer.Clear();
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    public void Execute(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        clipEngine.Clear();
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        clipEngine.AddSubject(paths64SubjectBuffer.Buffer);
        clip.ToPath64(path64ClipBuffer);
        clipEngine.AddClip(path64ClipBuffer);
        
        paths64SolutionBuffer.Clear();
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<IReadOnlyList<Vector2>> clip, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        clipEngine.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipEngine.AddSubject(path64SubjectBuffer);
        
        paths64ClipBuffer.PrepareBuffer(clip.Count);
        clip.ToPaths64(paths64ClipBuffer.Buffer);
        clipEngine.AddClip(paths64ClipBuffer.Buffer);
        
        paths64SolutionBuffer.Clear();
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        clipEngine.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipEngine.AddSubject(path64SubjectBuffer);
        clip.ToPath64(path64ClipBuffer);
        clipEngine.AddClip(path64ClipBuffer);
        
        paths64SolutionBuffer.Clear();
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
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
            clipEngine.Clear();
            //CHECK: Does this work! -> using solution here and in execute
            clipEngine.AddSubject(started ? paths64SolutionBuffer : paths64SubjectBuffer.Buffer);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, paths64SolutionBuffer);

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
            clipEngine.Clear();
            //CHECK: Does this work! -> using solution here and in execute
            if(started)clipEngine.AddSubject(paths64SolutionBuffer);
            else clipEngine.AddSubject(path64ClipBuffer);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, paths64SolutionBuffer);

            if (!started) started = true;
        }
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    
    public void Execute(Paths64 subject, Paths64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed, Paths64 solutionOpen)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed, solutionOpen);
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
    /// <summary>Max cached triangulations (simple cap; cache clears when exceeded).</summary>
    public static int MaxTriangulationCacheEntries = 512;
    
    public static int DecimalPlaces
    {
        get => scale.DecimalPlaces;
        set
        {
            scale = new(value);
            OffsetEngine.Scale = scale;
        }
    }

    public static double Scale => scale.Scale;
    public static double InvScale => scale.InvScale;
    #endregion
    
    #region Private Settings
    private static ClipperScale scale = new(4);
    #endregion
    
    #region Reused Clipper Engines
    public static ShapeClipperOffset OffsetEngine { get; private set; } = new();
    public static ShapeClipper64 ClipEngine { get; private set; } = new();
    #endregion
    
    #region Reused Buffers

    private static readonly Path64 path64Buffer = new();
    private static readonly Path64 path64Buffer2 = new();
    private static readonly Paths64 paths64Buffer = new();
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

        if (polygonCCW.Count < 3 || thickness <= 0f) return;

        OffsetEngine.OffsetPolygon(polygonCCW, +thickness, miterLimit, beveled, _tmpOuter);
        OffsetEngine.OffsetPolygon(polygonCCW, -thickness, miterLimit, beveled, _tmpInner);
        if (_tmpOuter.Count == 0) return;
        
        _tmpRing.Clear();
        ClipEngine.Execute(_tmpOuter, _tmpInner, ShapeClipperClipType.Difference, _tmpRing);
        
        if (_tmpRing.Count == 0) return;
    
        result.TriangulatePaths64ToMesh(_tmpRing, useDelaunay);
    }

    public static void CreatePolylineTriangulation(IReadOnlyList<Vector2> polyline, float thickness, float miterLimit, bool beveled, EndType endType, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polyline.Count < 2 || thickness <= 0f) return;

        OffsetEngine.OffsetPolyline(polyline, thickness, miterLimit, beveled, endType, _tmpStroke);
        if (_tmpStroke.Count == 0) return;
        
        result.TriangulatePaths64ToMesh(_tmpStroke, useDelaunay);
    }
    
    public static void CreatePolygonTriangulation(Paths64 polygonWithHoles, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonWithHoles.Count == 0) return;
        
        result.TriangulatePaths64ToMesh(polygonWithHoles, useDelaunay);
    }
    
    public static void CreatePolygonTriangulation(Polygons polygonWithHoles, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonWithHoles.Count == 0) return;
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        
        result.TriangulatePaths64ToMesh(paths64ConversionBuffer.Buffer, useDelaunay);
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
    
    #region Create Shapes
    
    public static Path64 CreateEllipse(Vector2 center, double radiusX, double radiusY = 0f, int steps = 0)
    {
        return Clipper.Ellipse(center.ToPoint64(), radiusX, radiusY, steps);
    }
    public static void CreateEllipse(Vector2 center, double radiusX, double radiusY, int steps, Paths64 result)
    {
        result.Clear();
        result.Add(Clipper.Ellipse(center.ToPoint64(), radiusX, radiusY, steps));
    }
    public static void CreateEllipse(Vector2 center, double radiusX, double radiusY, int steps, List<Vector2> result)
    {
        var ellipse = Clipper.Ellipse(center.ToPoint64(), radiusX, radiusY, steps);
        ellipse.ToVector2List(result);
    }
    public static void CreateEllipse(Vector2 center, double radiusX, double radiusY, int steps, List<List<Vector2>> result)
    {
        var ellipse = Clipper.Ellipse(center.ToPoint64(), radiusX, radiusY, steps);
        ellipse.ToVector2Lists(result);
    }
    #endregion
    
    #region Trim Collinear 
    public static Path64 TrimCollinear(this Path64 polygon, bool isOpen = false)
    {
        return Clipper.TrimCollinear(polygon, isOpen);
    }
    public static Path64 TrimCollinear(this IReadOnlyList<Vector2> polygon, bool isOpen = false)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.TrimCollinear(path64Buffer, isOpen);
    }
    public static void TrimCollinear(this IReadOnlyList<Vector2> polygon, bool isOpen, List<Vector2> result)
    {
        polygon.ToPath64(path64Buffer);
        var trimmedPath = Clipper.TrimCollinear(path64Buffer, isOpen);
        trimmedPath.ToVector2List(result);
    }
    public static void TrimCollinear(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool isOpen, Paths64 result)
    {
        foreach (var p in polygonWithHoles)
        {
            p.ToPath64(path64Buffer);
            var trimmedPath = Clipper.TrimCollinear(path64Buffer, isOpen);
            result.Add(trimmedPath);
        }
    }
    public static void TrimCollinear(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool isOpen, List<List<Vector2>> result)
    {
        paths64Buffer.Clear();
        for (int i = 0; i < polygonWithHoles.Count; i++)
        {
            polygonWithHoles[i].ToPath64(path64Buffer);
            var trimmedPath = Clipper.TrimCollinear(path64Buffer, isOpen);
            paths64Buffer.Add(trimmedPath);
        }
        paths64Buffer.ToVector2Lists(result);
    }
    #endregion
    
    #region Strip Duplicates
    public static Path64 StripDuplicates(this Path64 polygon, bool isClosedPath = false)
    {
        return Clipper.StripDuplicates(polygon, isClosedPath);
    }
    public static Path64 StripDuplicates(this IReadOnlyList<Vector2> polygon, bool isClosedPath = false)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.StripDuplicates(path64Buffer, isClosedPath);
    }
    public static void StripDuplicates(this IReadOnlyList<Vector2> polygon, bool isClosedPath, List<Vector2> result)
    {
        polygon.ToPath64(path64Buffer);
        var trimmedPath = Clipper.StripDuplicates(path64Buffer, isClosedPath);
        trimmedPath.ToVector2List(result);
    }
    public static void StripDuplicates(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool isClosedPath, Paths64 result)
    {
        foreach (var p in polygonWithHoles)
        {
            p.ToPath64(path64Buffer);
            var trimmedPath = Clipper.StripDuplicates(path64Buffer, isClosedPath);
            result.Add(trimmedPath);
        }
    }
    public static void StripDuplicates(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool isClosedPath, List<List<Vector2>> result)
    {
        paths64Buffer.Clear();
        for (int i = 0; i < polygonWithHoles.Count; i++)
        {
            polygonWithHoles[i].ToPath64(path64Buffer);
            var trimmedPath = Clipper.StripDuplicates(path64Buffer, isClosedPath);
            paths64Buffer.Add(trimmedPath);
        }
        paths64Buffer.ToVector2Lists(result);
    }
    #endregion

    #region Point In Polygon
    
    public static PointInPolygonResult PointInPolygon(this Path64 polygon, Vector2 p)
    {
        return Clipper.PointInPolygon(p.ToPoint64(), polygon);
    }
    public static PointInPolygonResult PointInPolygon(this IReadOnlyList<Vector2> polygon, Vector2 p)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.PointInPolygon(p.ToPoint64(), path64Buffer);
    }
    public static PointInPolygonResult PointInPolygons(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, Vector2 p)
    {
        if (polygonWithHoles.Count <= 0) return PointInPolygonResult.IsOutside;
        if(polygonWithHoles.Count == 1) return PointInPolygon(polygonWithHoles[0], p);
        var result = PointInPolygon(polygonWithHoles[0], p);
        
        //point is outside of the main polygon
        if (result == PointInPolygonResult.IsOutside) return result;

        var point = p.ToPoint64();
        for (int i = 1; i < polygonWithHoles.Count; i++)
        {
            var hole = polygonWithHoles[i];
            hole.ToPath64(path64Buffer);
            var holeResult = Clipper.PointInPolygon(point, path64Buffer);
            
            //point is inside the polygon but also inside a hole -> therefore it is considered outside
            if (holeResult == PointInPolygonResult.IsInside) return PointInPolygonResult.IsOutside;
        }

        //point is inside polygon but not inside any hole
        return PointInPolygonResult.IsInside;
    }
    #endregion
    
    #region Minkowski
    public static Paths64 MinkowskiDiff(this Path64 polygon, Path64 pattern, bool isClosed = false)
    {
        return Minkowski.Diff(pattern, polygon, isClosed);
    }
    public static Paths64 MinkowskiDiff(this IReadOnlyList<Vector2> polygon, IReadOnlyList<Vector2> pattern, bool isClosed = false)
    {
        polygon.ToPath64(path64Buffer);
        pattern.ToPath64(path64Buffer2);
        
        return Minkowski.Diff(path64Buffer2, path64Buffer, isClosed);
    }
    public static void MinkowskiDiff(this IReadOnlyList<Vector2> polygon, IReadOnlyList<Vector2> pattern, bool isClosed, List<List<Vector2>> result)
    {
        polygon.ToPath64(path64Buffer);
        pattern.ToPath64(path64Buffer2);
        
        var diff = Minkowski.Diff(path64Buffer2, path64Buffer, isClosed);
        diff.ToVector2Lists(result);
    }
    public static Paths64 MinkowskiSum(this Path64 polygon, Path64 pattern, bool isClosed = false)
    {
        return Minkowski.Sum(pattern, polygon, isClosed);
    }
    public static Paths64 MinkowskiSum(this IReadOnlyList<Vector2> polygon, IReadOnlyList<Vector2> pattern, bool isClosed = false)
    {
        polygon.ToPath64(path64Buffer);
        pattern.ToPath64(path64Buffer2);
        
        return Minkowski.Sum(path64Buffer2, path64Buffer, isClosed);
    }
    public static void MinkowskiSum(this IReadOnlyList<Vector2> polygon, IReadOnlyList<Vector2> pattern, bool isClosed, List<List<Vector2>> result)
    {
        polygon.ToPath64(path64Buffer);
        pattern.ToPath64(path64Buffer2);
        
        var diff = Minkowski.Sum(path64Buffer2, path64Buffer, isClosed);
        diff.ToVector2Lists(result);
    }
    #endregion
    
    #region Simplify
    public static Path64 Simplify(this Path64 polygon, float epsilon, bool isClosedPath = false)
    {
        return Clipper.SimplifyPath(polygon, epsilon, isClosedPath);
    }
    public static Paths64 Simplify(this Paths64 polygonWithHoles, float epsilon, bool isClosedPath = false)
    {
        return Clipper.SimplifyPaths(polygonWithHoles, epsilon, isClosedPath);
    }

    public static Path64 Simplify(this IReadOnlyList<Vector2> polygon, float epsilon, bool isClosedPath = false)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.SimplifyPath(path64Buffer, epsilon, isClosedPath);
    }
    public static Paths64 Simplify(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, float epsilon, bool isClosedPath = false)
    {
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.SimplifyPaths(paths64ConversionBuffer.Buffer, epsilon, isClosedPath);
    }
    
    public static void Simplify(this IReadOnlyList<Vector2> polygon, float epsilon, bool isClosedPath, List<Vector2> result)
    {
        polygon.ToPath64(path64Buffer);
        var solution = Clipper.SimplifyPath(path64Buffer, epsilon, isClosedPath);
        solution.ToVector2List(result);
    }
    public static void Simplify(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, float epsilon, bool isClosedPath, List<List<Vector2>> result)
    {
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        var solution =  Clipper.SimplifyPaths(paths64ConversionBuffer.Buffer, epsilon, isClosedPath);
        solution.ToVector2Lists(result);
    }
    #endregion

    #region Simplify Ramer-Douglas-Peucker

    public static Path64 SimplifyRamerDouglasPeucker(this Path64 polygon, float epsilon)
    {
        return Clipper.RamerDouglasPeucker(polygon, epsilon);
    }
    public static Paths64 SimplifyRamerDouglasPeucker(this Paths64 polygonWithHoles, float epsilon)
    {
        return Clipper.RamerDouglasPeucker(polygonWithHoles, epsilon);
    }

    public static Path64 SimplifyRamerDouglasPeucker(this IReadOnlyList<Vector2> polygon, float epsilon)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.RamerDouglasPeucker(path64Buffer, epsilon);
    }
    public static Paths64 SimplifyRamerDouglasPeucker(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, float epsilon)
    {
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RamerDouglasPeucker(paths64ConversionBuffer.Buffer, epsilon);
    }
    
    public static void SimplifyRamerDouglasPeucker(this IReadOnlyList<Vector2> polygon, float epsilon, List<Vector2> result)
    {
        polygon.ToPath64(path64Buffer);
        var solution = Clipper.RamerDouglasPeucker(path64Buffer, epsilon);
        solution.ToVector2List(result);
    }
    public static void SimplifyRamerDouglasPeucker(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, float epsilon, List<List<Vector2>> result)
    {
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        var solution =  Clipper.RamerDouglasPeucker(paths64ConversionBuffer.Buffer, epsilon);
        solution.ToVector2Lists(result);
    }

    #endregion
    
    #region Conversion
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
        long left   = (long)Math.Round(r.X * Scale);
        long top    = (long)Math.Round(r.Y * Scale);
        long right  = (long)Math.Round((r.X + r.Width) * Scale);
        long bottom = (long)Math.Round((r.Y + r.Height) * Scale);
        return new Rect64(left, top, right, bottom);
    }
    public static Rect ToRect(this Rect64 r)
    {
        float x = (float)(r.left * InvScale);
        float y = (float)(r.top * InvScale);
        float w = (float)((r.right - r.left) * InvScale);
        float h = (float)((r.bottom - r.top) * InvScale);

        return new Rect(x, y, w, h);
    }
    
    public static Point64 ToPoint64(this Vector2 v)
    {
        long x = (long)Math.Round(v.X * Scale);
        long y = (long)Math.Round(v.Y * Scale);
        return new Point64(x,y);
    }
    
    public static Vector2 ToVec2(this Point64 p)
    {
        return new Vector2((float)(p.X * InvScale), (float)(p.Y * InvScale));
    }
    #endregion
    
    #region Enum Conversion
    /// <summary>
    /// Converts a <see cref="ShapeClipperFillRule"/> to the Clipper <see cref="FillRule"/> enum.
    /// </summary>
    /// <param name="fillRule">The ShapeClipper fill rule to convert.</param>
    /// <returns>The equivalent <see cref="FillRule"/> value.</returns>
    public static FillRule ToClipperFillRule(this ShapeClipperFillRule fillRule)
    {
        return (FillRule)fillRule;
    }
    
    /// <summary>
    /// Converts a <see cref="ShapeClipperJoinType"/> to the Clipper <see cref="JoinType"/> enum.
    /// </summary>
    /// <param name="joinType">The ShapeClipper join type to convert.</param>
    /// <returns>The equivalent <see cref="JoinType"/> value.</returns>
    public static JoinType ToClipperJoinType(this ShapeClipperJoinType joinType)
    {
        return (JoinType)joinType;
    }
    
    /// <summary>
    /// Converts a <see cref="ShapeClipperEndType"/> to the Clipper <see cref="EndType"/> enum.
    /// </summary>
    /// <param name="endType">The ShapeClipper end type to convert.</param>
    /// <returns>The equivalent <see cref="EndType"/> value.</returns>
    public static EndType ToClipperEndType(this ShapeClipperEndType endType)
    {
        return (EndType)endType;
    }

    //TODO: Docs
    public static ClipType ToClipperClipType(this ShapeClipperClipType clipType)
    {
        return (ClipType)clipType;
    }
    
    /// <summary>
    /// Converts a Clipper <see cref="FillRule"/> to the local <see cref="ShapeClipperFillRule"/> enum.
    /// </summary>
    /// <param name="fillRule">The Clipper fill rule to convert.</param>
    /// <returns>The equivalent <see cref="ShapeClipperFillRule"/> value.</returns>
    public static ShapeClipperFillRule ToShapeClipperFillRule(this FillRule fillRule)
    {
        return (ShapeClipperFillRule)fillRule;
    }
    
    /// <summary>
    /// Converts a Clipper <see cref="JoinType"/> to the local <see cref="ShapeClipperJoinType"/> enum.
    /// </summary>
    /// <param name="joinType">The Clipper join type to convert.</param>
    /// <returns>The equivalent <see cref="ShapeClipperJoinType"/> value.</returns>
    public static ShapeClipperJoinType ToShapeClipperJoinType(this JoinType joinType)
    {
        return (ShapeClipperJoinType)joinType;
    }
    
    /// <summary>
    /// Converts a Clipper <see cref="EndType"/> to the local <see cref="ShapeClipperEndType"/> enum.
    /// </summary>
    /// <param name="endType">The Clipper end type to convert.</param>
    /// <returns>The equivalent <see cref="ShapeClipperEndType"/> value.</returns>
    public static ShapeClipperEndType ToShapeClipperEndType(this EndType endType)
    {
        return (ShapeClipperEndType)endType;
    }
    
    //TODO: Docs
    public static ShapeClipperClipType ToShapeClipperClipType(this ClipType clipType)
    {
        return (ShapeClipperClipType)clipType;
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