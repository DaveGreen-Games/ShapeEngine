using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapeStripedDrawing
{
     public static void DrawStriped(this Circle circle, float spacing, float angleDeg, LineDrawingInfo striped, float sideLength = 8f)
    {
        if (spacing <= 0) return;
        float maxDimension = circle.Diameter;

        if (spacing > maxDimension) return;
        
        var center = circle.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        for (int i = 0; i < steps; i++)
        {
            var intersection = Line.IntersectLineCircle(cur, lineDir, circle.Center, circle.Radius);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                segment.Draw(striped);
            }
            cur += dir * spacing;
        }
    }
    public static void DrawStriped(this Circle circle, float spacing, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped, float sideLength = 8f)
    {
        if (spacing <= 0) return;
        float maxDimension = circle.Diameter;

        if (spacing > maxDimension) return;
        
        var center = circle.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        for (int i = 0; i < steps; i++)
        {
            var intersection = Line.IntersectLineCircle(cur, lineDir, circle.Center, circle.Radius);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var info = i % 2 == 0 ? striped : alternatingStriped;
                segment.Draw(info);
            }
            cur += dir * spacing;
        }
    }
    public static void DrawStriped(this Circle circle, float spacing, float angleDeg, float sideLength, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacing <= 0) return;
        if (alternatingStriped.Length <= 0) return;
        if(alternatingStriped.Length == 1) DrawStriped(circle, spacing, angleDeg, alternatingStriped[0], sideLength);
        
        float maxDimension = circle.Diameter;

        if (spacing > maxDimension) return;
        
        var center = circle.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        for (int i = 0; i < steps; i++)
        {
            var intersection = Line.IntersectLineCircle(cur, lineDir, circle.Center, circle.Radius);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var index = i % alternatingStriped.Length;
                var info = alternatingStriped[index];
                segment.Draw(info);
            }
            cur += dir * spacing;
        }
    }

    public static void DrawStriped(this Triangle triangle, float spacing, float angleDeg, LineDrawingInfo striped)
    {
        if (spacing <= 0) return;
        var center = triangle.GetCentroid();
        var fv = triangle.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var a = triangle.A;
        var b = triangle.B;
        var c = triangle.C;
        
        var cur = start + dir * spacing;
        for (int i = 0; i < steps; i++)
        {
            var intersection = Line.IntersectLineTriangle(cur, lineDir, a, b, c);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                segment.Draw(striped);
            }
            cur += dir * spacing;
        }
    }
    public static void DrawStriped(this Triangle triangle, float spacing, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = triangle.GetCentroid();
        var fv = triangle.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var a = triangle.A;
        var b = triangle.B;
        var c = triangle.C;
        
        var cur = start + dir * spacing;
        for (int i = 0; i < steps; i++)
        {
            var intersection = Line.IntersectLineTriangle(cur, lineDir, a, b, c);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var info = i % 2 == 0 ? striped : alternatingStriped;
                segment.Draw(info);
            }
            cur += dir * spacing;
        }
    }
    public static void DrawStriped(this Triangle triangle, float spacing, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = triangle.GetCentroid();
        var fv = triangle.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var a = triangle.A;
        var b = triangle.B;
        var c = triangle.C;
        
        var cur = start + dir * spacing;
        for (int i = 0; i < steps; i++)
        {
            var intersection = Line.IntersectLineTriangle(cur, lineDir, a, b, c);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var infoIndex = i % alternatingStriped.Length;
                var info = alternatingStriped[infoIndex];
                segment.Draw(info);
            }
            cur += dir * spacing;
        }
    }
    
    public static void DrawStriped(this Quad quad, float spacing, float angleDeg, LineDrawingInfo striped)
    {
        if (spacing <= 0) return;
        var center = quad.Center;
        var fv = quad.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var a = quad.A;
        var b = quad.B;
        var c = quad.C;
        var d = quad.D;
        
        var cur = start + dir * spacing;
        for (int i = 0; i < steps; i++)
        {
            var intersection = Line.IntersectLineQuad(cur, lineDir, a, b, c, d);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                segment.Draw(striped);
            }
            cur += dir * spacing;
        }
    }
    public static void DrawStriped(this Quad quad, float spacing, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = quad.Center;
        var fv = quad.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var a = quad.A;
        var b = quad.B;
        var c = quad.C;
        var d = quad.D;
        
        var cur = start + dir * spacing;
        for (int i = 0; i < steps; i++)
        {
            var intersection = Line.IntersectLineQuad(cur, lineDir, a, b, c, d);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var info = i % 2 == 0 ? striped : alternatingStriped;
                segment.Draw(info);
            }
            cur += dir * spacing;
        }
    }
    public static void DrawStriped(this Quad quad, float spacing, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = quad.Center;
        var fv = quad.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var a = quad.A;
        var b = quad.B;
        var c = quad.C;
        var d = quad.D;
        
        var cur = start + dir * spacing;
        for (int i = 0; i < steps; i++)
        {
            var intersection = Line.IntersectLineQuad(cur, lineDir, a, b, c, d);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var infoIndex = i % alternatingStriped.Length;
                var info = alternatingStriped[infoIndex];
                segment.Draw(info);
            }
            cur += dir * spacing;
        }
    }
    
    public static void DrawStriped(this Rect rect, float spacing, float angleDeg, LineDrawingInfo striped)
    {
        if (spacing <= 0) return;
        float maxDimension = (rect.TopLeft - rect.BottomRight).Length();

        if (spacing > maxDimension) return;
        
        var center = rect.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var a = rect.A;
        var b = rect.B;
        var c = rect.C;
        var d = rect.D;
        
        var cur = start + dir * spacing;
        for (int i = 0; i < steps; i++)
        {
            var intersection = Line.IntersectLineRect(cur, lineDir, a, b, c, d);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                segment.Draw(striped);
            }
            cur += dir * spacing;
        }
    }
    public static void DrawStriped(this Rect rect, float spacing, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacing <= 0) return;
        float maxDimension = (rect.TopLeft - rect.BottomRight).Length();

        if (spacing > maxDimension) return;
        
        var center = rect.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var a = rect.A;
        var b = rect.B;
        var c = rect.C;
        var d = rect.D;
        
        var cur = start + dir * spacing;
        for (int i = 0; i < steps; i++)
        {
            var intersection = Line.IntersectLineRect(cur, lineDir, a, b, c, d);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var info = i % 2 == 0 ? striped : alternatingStriped;
                segment.Draw(info);
            }
            cur += dir * spacing;
        }
    }
    public static void DrawStriped(this Rect rect, float spacing, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacing <= 0) return;
        float maxDimension = (rect.TopLeft - rect.BottomRight).Length();

        if (spacing > maxDimension) return;
        
        var center = rect.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var a = rect.A;
        var b = rect.B;
        var c = rect.C;
        var d = rect.D;
        
        var cur = start + dir * spacing;
        for (int i = 0; i < steps; i++)
        {
            var intersection = Line.IntersectLineRect(cur, lineDir, a, b, c, d);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var infoIndex = i % alternatingStriped.Length;
                var info = alternatingStriped[infoIndex];
                segment.Draw(info);
            }
            cur += dir * spacing;
        }
    }
   
    public static void DrawStriped(this Polygon polygon, float spacing, float angleDeg, LineDrawingInfo striped)
    {
        if (spacing <= 0) return;
        var center = polygon.GetCentroid();
        
        polygon.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to outside of the polygon in the opposite direction of the ray
        for (int i = 0; i < steps; i++)
        {
            var segments = polygon.CutRayWithPolygon(cur, rayDir);
            if (segments != null && segments.Count > 0)
            {
                foreach (var segment in segments)
                {
                    segment.Draw(striped);
                }
            }
            cur += dir * spacing;
        }
    }
    public static void DrawStriped(this Polygon polygon, float spacing, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = polygon.GetCentroid();
        
        polygon.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to outside of the polygon in the opposite direction of the ray
        for (int i = 0; i < steps; i++)
        {
            var segments = polygon.CutRayWithPolygon(cur, rayDir);
            if (segments != null && segments.Count > 0)
            {
                var info = i % 2 == 0 ? striped : alternatingStriped;
                foreach (var segment in segments)
                {
                    segment.Draw(info);
                }
            }
            cur += dir * spacing;
        }
    }
    public static void DrawStriped(this Polygon polygon, float spacing, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = polygon.GetCentroid();
        
        polygon.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to outside of the polygon in the opposite direction of the ray
        for (int i = 0; i < steps; i++)
        {
            var segments = polygon.CutRayWithPolygon(cur, rayDir);
            if (segments != null && segments.Count > 0)
            {
                var infoIndex = i % alternatingStriped.Length;
                var info = alternatingStriped[infoIndex];
                foreach (var segment in segments)
                {
                    segment.Draw(info);
                }
            }
            cur += dir * spacing;
        }
    }
    
}