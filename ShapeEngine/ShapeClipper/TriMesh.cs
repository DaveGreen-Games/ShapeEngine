using System.Numerics;
using Clipper2Lib;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Geometry.TriangulationDef;

namespace ShapeEngine.ShapeClipper;

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