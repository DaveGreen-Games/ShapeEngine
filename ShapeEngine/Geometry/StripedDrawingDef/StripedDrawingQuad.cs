using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.StripedDrawingDef;

public static partial class StripedDrawing
{
    
    /// <summary>
    /// Generates a collection of line segments representing a striped pattern clipped to the specified quad.
    /// </summary>
    /// <param name="quad">The quad to populate with striped segments.</param>
    /// <param name="spacing">Distance between adjacent stripe lines. Values = 0 will produce an empty result.</param>
    /// <param name="angleDeg">Orientation of the stripes in degrees (0 = vertical, 90 = horizontal).</param>
    /// <param name="spacingOffset">Normalized offset in the range [0,1] used to shift the pattern (useful for animation).</param>
    /// <returns>A <see cref="Segments"/> instance containing segments that lie inside the quad.</returns>
    public static Segments GenerateStripedSegments(this Quad quad, float spacing, float angleDeg, float spacingOffset = 0f)
    {
        var segments = new Segments();
        if (spacing <= 0) return segments;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return segments;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

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
                segments.Add(segment);
            }

            cur += dir * spacing;
        }

        return segments;
    }
    /// <summary>
    /// Generates a collection of line segments representing a striped pattern clipped to the specified quad,
    /// where the distance between consecutive stripes is determined by a <see cref="CurveFloat"/>.
    /// </summary>
    /// <param name="quad">The quad to populate with striped segments.</param>
    /// <param name="spacingCurve">A curve that defines the spacing along the pattern. The curve must have keys and sampled values must be &gt; 0; otherwise the method returns an empty result.</param>
    /// <param name="angleDeg">Orientation of the stripes in degrees (0 = vertical, 90 = horizontal).</param>
    /// <returns>A <see cref="Segments"/> instance containing segments that lie inside the quad.</returns>
    public static Segments GenerateStripedSegments(this Quad quad, CurveFloat spacingCurve, float angleDeg)
    {
        var segments = new Segments();
        if (!spacingCurve.HasKeys) return segments;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        if (!spacingCurve.Sample(0f, out float spacing)) return segments;

        if (spacing > maxDimension || spacing <= 0) return segments;

        var start = center - (dir * maxDimension * 0.5f);
        var cur = start + dir * spacing;
        var targetLength = spacing;

        while (targetLength < maxDimension)
        {
            var intersection = Line.IntersectLineQuad(cur, lineDir, quad.A, quad.B, quad.C, quad.D);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                segments.Add(segment);
            }

            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return segments;
            if (spacing <= 0f) return segments; //prevents infinite loop

            targetLength += spacing;
            cur += dir * spacing;
        }

        return segments;
    }
    /// <summary>
    /// Generates striped segments clipped to the given outside quad while excluding the area of an inside shape.
    /// The method casts parallel lines/rays across the outside quad and subtracts intersections with the
    /// provided inside shape to produce the final visible stripe segments.
    /// </summary>
    /// <typeparam name="T">Type of the inside shape.
    /// Allowed types are handled inside the method.
    /// Supported inside shape types: <see cref="Triangle"/>, <see cref="Circle"/>,
    /// <see cref="Rect"/>, <see cref="Quad"/>, <see cref="Polygon"/>.</typeparam>
    /// <param name="outsideShape">Quad that defines the outer boundary for the stripes.</param>
    /// <param name="insideShape">Shape to be excluded from the stripes.</param>
    /// <param name="spacing">Distance between adjacent stripes. Values= 0 will produce an empty result.</param>
    /// <param name="angleDeg">Orientation of the stripes in degrees (0 = vertical, 90 = horizontal).</param>
    /// <param name="spacingOffset">Normalized offset in the range [0,1] used to shift the pattern (useful for animation).</param>
    /// <returns>
    /// A <see cref="Segments"/> instance containing segments that lie inside <paramref name="outsideShape"/>
    /// but outside <paramref name="insideShape"/>.
    /// </returns>
    public static Segments GenerateStripedSegments<T>(this Quad outsideShape, T insideShape, float spacing, float angleDeg, float spacingOffset = 0f) 
    {
        var segments = new Segments();
        if (spacing <= 0) return segments;
        var center = outsideShape.Center;
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        if (spacing > maxDimension) return segments;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension; //offsets the point to the outside for using rays instead of lines
        
        if (insideShape is Triangle triangle)
        {
            for (int i = 0; i < steps; i++)
            {
                var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
                if (!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid)
                {
                    cur += dir * spacing;
                    continue;
                }

                var insideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, triangle.A, triangle.B, triangle.C);
                if (!insideShapePoints.a.Valid && !insideShapePoints.b.Valid) //draw the ray - circle intersection points
                {
                    var segment = new Segment(outsideShapePoints.a.Point, outsideShapePoints.b.Point);
                    segments.Add(segment);
                }
                else
                {
                    AddSegmentsHelper(cur, outsideShapePoints, insideShapePoints, ref segments);
                }

                cur += dir * spacing;
            }
        }
        else if (insideShape is Circle circle)
        {
            for (int i = 0; i < steps; i++)
            {
                var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
                if (!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid)
                {
                    cur += dir * spacing;
                    continue;
                }

                var insideShapePoints = Ray.IntersectRayCircle(cur, rayDir, circle.Center, circle.Radius);
                if (!insideShapePoints.a.Valid && !insideShapePoints.b.Valid) //draw the ray - circle intersection points
                {
                    var segment = new Segment(outsideShapePoints.a.Point, outsideShapePoints.b.Point);
                    segments.Add(segment);
                }
                else
                {
                    AddSegmentsHelper(cur, outsideShapePoints, insideShapePoints, ref segments);
                }

                cur += dir * spacing;
            }
        }
        else if (insideShape is Rect rect)
        {
            for (int i = 0; i < steps; i++)
            {
                var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
                if (!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid)
                {
                    cur += dir * spacing;
                    continue;
                }

                var insideShapePoints = Ray.IntersectRayRect(cur, rayDir, rect.A, rect.B, rect.C, rect.D);
                if (!insideShapePoints.a.Valid && !insideShapePoints.b.Valid) //draw the ray - circle intersection points
                {
                    var segment = new Segment(outsideShapePoints.a.Point, outsideShapePoints.b.Point);
                    segments.Add(segment);
                }
                else
                {
                    AddSegmentsHelper(cur, outsideShapePoints, insideShapePoints, ref segments);
                }

                cur += dir * spacing;
            }
        }
        else if (insideShape is Quad quad)
        {
            for (int i = 0; i < steps; i++)
            {
                var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
                if (!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid)
                {
                    cur += dir * spacing;
                    continue;
                }

                var insideShapePoints = Ray.IntersectRayQuad(cur, rayDir, quad.A, quad.B, quad.C, quad.D);
                if (!insideShapePoints.a.Valid && !insideShapePoints.b.Valid) //draw the ray - circle intersection points
                {
                    var segment = new Segment(outsideShapePoints.a.Point, outsideShapePoints.b.Point);
                    segments.Add(segment);
                }
                else
                {
                    AddSegmentsHelper(cur, outsideShapePoints, insideShapePoints, ref segments);
                }

                cur += dir * spacing;
            }
        }
        else if (insideShape is Polygon polygon)
        {
            for (int i = 0; i < steps; i++)
            {
                var outsideShapePoints = Line.IntersectLineQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
                if (!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid)
                {
                    cur += dir * spacing;
                    continue;
                }

                var count = Line.IntersectLinePolygon(cur, rayDir, polygon, ref intersectionPointsReference);

                if (count <= 0) //ray did not hit the inside shape, draw ray between edge of the outside shape
                {
                    var segment = new Segment(outsideShapePoints.a.Point, outsideShapePoints.b.Point);
                    segments.Add(segment);
                }
                else
                {
                    //remove all inside shape intersection points that are outside the outside shape
                    for (int j = intersectionPointsReference.Count - 1; j >= 0; j--)
                    {
                        var p = intersectionPointsReference[j].Point;

                        if (!outsideShape.ContainsPoint(p)) intersectionPointsReference.RemoveAt(j);
                    }

                    if (outsideShapePoints.a.Valid && !polygon.ContainsPoint(outsideShapePoints.a.Point)) intersectionPointsReference.Add(outsideShapePoints.a);
                    if (outsideShapePoints.b.Valid && !polygon.ContainsPoint(outsideShapePoints.b.Point)) intersectionPointsReference.Add(outsideShapePoints.b);

                    //all points were remove so just draw the outside shape segment (even with only 1 point left, we continue)
                    if (intersectionPointsReference.Count <= 1)
                    {
                        cur += dir * spacing;
                        intersectionPointsReference.Clear();
                        continue;
                    }

                    if (intersectionPointsReference.Count == 2) //no sorting or loop needed for exactly 2 points
                    {
                        var segment = new Segment(intersectionPointsReference[0].Point, intersectionPointsReference[1].Point);
                        segments.Add(segment);
                        cur += dir * spacing;
                        intersectionPointsReference.Clear();
                        continue;
                    }

                    //now that only valid points remain, sort them by distance from the current point
                    intersectionPointsReference.SortClosestFirst(cur);

                    for (int j = 0; j < intersectionPointsReference.Count - 1; j += 2)
                    {
                        var p1 = intersectionPointsReference[j].Point;
                        var p2 = intersectionPointsReference[j + 1].Point;
                        var segment = new Segment(p1, p2);
                        segments.Add(segment);
                    }

                    intersectionPointsReference.Clear();
                }

                cur += dir * spacing;
            }
        }
        return segments;
    }
    
    
    /// <summary>
    /// Draws a striped pattern inside the specified shape.
    /// </summary>
    /// <param name="quad">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Quad quad, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

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

    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="quad">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The first line drawing info for drawing even lines.</param>
    /// <param name="alternatingStriped">The second line drawing info for drawing odd lines.</param>
    public static void DrawStriped(this Quad quad, float spacing, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int _);
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

    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="quad">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="alternatingStriped">The line drawing infos for drawing each line. Each info is used in sequence and wraps around if there are more lines.</param>
    public static void DrawStriped(this Quad quad, float spacing, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int _);
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

    /// <summary>
    /// Draws a striped pattern inside the specified shape.
    /// </summary>
    /// <param name="quad">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    public static void DrawStriped(this Quad quad, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        if (!spacingCurve.Sample(0f, out float spacing)) return;

        if (spacing > maxDimension || spacing <= 0) return;

        var start = center - (dir * maxDimension * 0.5f);
        var cur = start + dir * spacing;
        var targetLength = spacing;

        while (targetLength < maxDimension)
        {
            var intersection = Line.IntersectLineQuad(cur, lineDir, quad.A, quad.B, quad.C, quad.D);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                segment.Draw(striped);
            }

            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return; //prevents infinite loop

            targetLength += spacing;
            cur += dir * spacing;
        }
    }

    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="quad">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The first line drawing info for drawing even lines.</param>
    /// <param name="alternatingStriped">The second line drawing info for drawing odd lines.</param>
    public static void DrawStriped(this Quad quad, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        if (!spacingCurve.Sample(0f, out float spacing)) return;

        if (spacing > maxDimension || spacing <= 0) return;

        var start = center - (dir * maxDimension * 0.5f);
        var cur = start + dir * spacing;
        var targetLength = spacing;
        int i = 0;
        while (targetLength < maxDimension)
        {
            var intersection = Line.IntersectLineQuad(cur, lineDir, quad.A, quad.B, quad.C, quad.D);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var info = i % 2 == 0 ? striped : alternatingStriped;
                segment.Draw(info);
            }

            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return; //prevents infinite loop

            targetLength += spacing;
            cur += dir * spacing;
            i++;
        }
    }

    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="quad">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="alternatingStriped">The line drawing infos for drawing each line. Each info is used in sequence and wraps around if there are more lines.</param>
    public static void DrawStriped(this Quad quad, CurveFloat spacingCurve, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        if (!spacingCurve.Sample(0f, out float spacing)) return;

        if (spacing > maxDimension || spacing <= 0) return;

        var start = center - (dir * maxDimension * 0.5f);
        var cur = start + dir * spacing;
        var targetLength = spacing;

        int i = 0;
        while (targetLength < maxDimension)
        {
            var intersection = Line.IntersectLineQuad(cur, lineDir, quad.A, quad.B, quad.C, quad.D);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var infoIndex = i % alternatingStriped.Length;
                var info = alternatingStriped[infoIndex];
                segment.Draw(info);
            }

            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return; //prevents infinite loop

            targetLength += spacing;
            cur += dir * spacing;
            i++;
        }
    }

    /// <summary>
    /// Draws a striped pattern inside the outside shape without drawing in the inside shape.
    /// The inside shape does not have to be completely inside the outside shape.
    /// </summary>
    /// <param name="outsideShape">The shape for drawing the striped pattern inside.</param>
    /// <param name="insideShape">The shape to not draw any striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Quad outsideShape, Circle insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.Center;
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension; //offsets the point to the outside for using rays instead of lines

        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if (!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid)
            {
                cur += dir * spacing;
                continue;
            }

            var insideShapePoints = Ray.IntersectRayCircle(cur, rayDir, insideShape.Center, insideShape.Radius);
            if (!insideShapePoints.a.Valid && !insideShapePoints.b.Valid) //draw the ray - circle intersection points
            {
                var segment = new Segment(outsideShapePoints.a.Point, outsideShapePoints.b.Point);
                segment.Draw(striped);
            }
            else
            {
                var outsideDisA = (outsideShapePoints.a.Point - cur).LengthSquared();
                var outsideDisB = (outsideShapePoints.b.Point - cur).LengthSquared();
                float outsideFurthestDis;
                float outsideClosestDis;
                Vector2 outsideFurthestPoint;
                Vector2 outsideClosestPoint;

                if (outsideDisA < outsideDisB)
                {
                    outsideFurthestDis = outsideDisB;
                    outsideClosestDis = outsideDisA;
                    outsideFurthestPoint = outsideShapePoints.b.Point;
                    outsideClosestPoint = outsideShapePoints.a.Point;
                }
                else
                {
                    outsideFurthestDis = outsideDisA;
                    outsideClosestDis = outsideDisB;
                    outsideFurthestPoint = outsideShapePoints.a.Point;
                    outsideClosestPoint = outsideShapePoints.b.Point;
                }

                var insideDisA = insideShapePoints.a.Valid ? (insideShapePoints.a.Point - cur).LengthSquared() : 0f;
                var insideDisB = insideShapePoints.b.Valid ? (insideShapePoints.b.Point - cur).LengthSquared() : 0f;
                float insideFurthestDis;
                float insideClosestDis;
                Vector2 insideFurthestPoint;
                Vector2 insideClosestPoint;

                if (insideDisA < insideDisB)
                {
                    insideFurthestDis = insideDisB;
                    insideClosestDis = insideDisA;
                    insideFurthestPoint = insideShapePoints.b.Point;
                    insideClosestPoint = insideShapePoints.a.Point;
                }
                else
                {
                    insideFurthestDis = insideDisA;
                    insideClosestDis = insideDisB;
                    insideFurthestPoint = insideShapePoints.a.Point;
                    insideClosestPoint = insideShapePoints.b.Point;
                }

                //both outside shape points are inside the inside shape -> no drawing possible
                if (insideFurthestDis > outsideFurthestDis && insideClosestDis < outsideClosestDis)
                {
                    cur += dir * spacing;
                    continue;
                }

                //draw outside shape segments because the inside shape points are outside the outside shape
                if (insideClosestDis > outsideFurthestDis || insideFurthestDis < outsideClosestDis)
                {
                    var segment = new Segment(outsideClosestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else if (insideFurthestDis > outsideFurthestDis) //part of the inside shape is outside the outside shape
                {
                    var segment = new Segment(outsideClosestPoint, insideClosestPoint);
                    segment.Draw(striped);
                }
                else if (insideClosestDis < outsideClosestDis) //part of the inside shape is outside the outside shape
                {
                    var segment = new Segment(insideFurthestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else //inside shape is completely inside the outside shape - draw everything normal
                {
                    var segment1 = new Segment(outsideClosestPoint, insideClosestPoint);
                    segment1.Draw(striped);

                    var segment2 = new Segment(insideFurthestPoint, outsideFurthestPoint);
                    segment2.Draw(striped);
                }

                // var points = new Points(4);
                // points.Add(outsideShapePoints.a.Point);
                // points.Add(outsideShapePoints.b.Point);
                // points.Add(insideShapePoints.a.Point);
                // points.Add(insideShapePoints.b.Point);
                //
                // points.SortClosestFirst(cur);
                //
                // var segment1 = new Segment(points[0], points[1]);
                // segment1.Draw(striped);
                //
                // var segment2 = new Segment(points[2], points[3]);
                // segment2.Draw(striped);
            }

            cur += dir * spacing;
        }
    }

    /// <summary>
    /// Draws a striped pattern inside the outside shape without drawing in the inside shape.
    /// The inside shape does not have to be completely inside the outside shape.
    /// </summary>
    /// <param name="outsideShape">The shape for drawing the striped pattern inside.</param>
    /// <param name="insideShape">The shape to not draw any striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Quad outsideShape, Triangle insideShape, float spacing, float angleDeg, LineDrawingInfo striped,
        float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.Center;
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension; //offsets the point to the outside for using rays instead of lines

        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if (!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid)
            {
                cur += dir * spacing;
                continue;
            }

            var insideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, insideShape.A, insideShape.B, insideShape.C);
            if (!insideShapePoints.a.Valid && !insideShapePoints.b.Valid) //draw the ray - circle intersection points
            {
                var segment = new Segment(outsideShapePoints.a.Point, outsideShapePoints.b.Point);
                segment.Draw(striped);
            }
            else
            {
                var outsideDisA = (outsideShapePoints.a.Point - cur).LengthSquared();
                var outsideDisB = (outsideShapePoints.b.Point - cur).LengthSquared();
                float outsideFurthestDis;
                float outsideClosestDis;
                Vector2 outsideFurthestPoint;
                Vector2 outsideClosestPoint;

                if (outsideDisA < outsideDisB)
                {
                    outsideFurthestDis = outsideDisB;
                    outsideClosestDis = outsideDisA;
                    outsideFurthestPoint = outsideShapePoints.b.Point;
                    outsideClosestPoint = outsideShapePoints.a.Point;
                }
                else
                {
                    outsideFurthestDis = outsideDisA;
                    outsideClosestDis = outsideDisB;
                    outsideFurthestPoint = outsideShapePoints.a.Point;
                    outsideClosestPoint = outsideShapePoints.b.Point;
                }

                var insideDisA = insideShapePoints.a.Valid ? (insideShapePoints.a.Point - cur).LengthSquared() : 0f;
                var insideDisB = insideShapePoints.b.Valid ? (insideShapePoints.b.Point - cur).LengthSquared() : 0f;
                float insideFurthestDis;
                float insideClosestDis;
                Vector2 insideFurthestPoint;
                Vector2 insideClosestPoint;

                if (insideDisA < insideDisB)
                {
                    insideFurthestDis = insideDisB;
                    insideClosestDis = insideDisA;
                    insideFurthestPoint = insideShapePoints.b.Point;
                    insideClosestPoint = insideShapePoints.a.Point;
                }
                else
                {
                    insideFurthestDis = insideDisA;
                    insideClosestDis = insideDisB;
                    insideFurthestPoint = insideShapePoints.a.Point;
                    insideClosestPoint = insideShapePoints.b.Point;
                }

                //both outside shape points are inside the inside shape -> no drawing possible
                if (insideFurthestDis > outsideFurthestDis && insideClosestDis < outsideClosestDis)
                {
                    cur += dir * spacing;
                    continue;
                }

                if (insideClosestDis > outsideFurthestDis || insideFurthestDis < outsideClosestDis)
                {
                    var segment = new Segment(outsideClosestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else if (insideFurthestDis > outsideFurthestDis)
                {
                    var segment = new Segment(outsideClosestPoint, insideClosestPoint);
                    segment.Draw(striped);
                }
                else if (insideClosestDis < outsideClosestDis)
                {
                    var segment = new Segment(insideFurthestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else
                {
                    var segment1 = new Segment(outsideClosestPoint, insideClosestPoint);
                    segment1.Draw(striped);

                    var segment2 = new Segment(insideFurthestPoint, outsideFurthestPoint);
                    segment2.Draw(striped);
                }
            }

            cur += dir * spacing;
        }
    }

    /// <summary>
    /// Draws a striped pattern inside the outside shape without drawing in the inside shape.
    /// The inside shape does not have to be completely inside the outside shape.
    /// </summary>
    /// <param name="outsideShape">The shape for drawing the striped pattern inside.</param>
    /// <param name="insideShape">The shape to not draw any striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Quad outsideShape, Quad insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.Center;
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension; //offsets the point to the outside for using rays instead of lines

        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if (!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid)
            {
                cur += dir * spacing;
                continue;
            }

            var insideShapePoints = Ray.IntersectRayQuad(cur, rayDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid && !insideShapePoints.b.Valid) //draw the ray - circle intersection points
            {
                var segment = new Segment(outsideShapePoints.a.Point, outsideShapePoints.b.Point);
                segment.Draw(striped);
            }
            else
            {
                var outsideDisA = (outsideShapePoints.a.Point - cur).LengthSquared();
                var outsideDisB = (outsideShapePoints.b.Point - cur).LengthSquared();
                float outsideFurthestDis;
                float outsideClosestDis;
                Vector2 outsideFurthestPoint;
                Vector2 outsideClosestPoint;

                if (outsideDisA < outsideDisB)
                {
                    outsideFurthestDis = outsideDisB;
                    outsideClosestDis = outsideDisA;
                    outsideFurthestPoint = outsideShapePoints.b.Point;
                    outsideClosestPoint = outsideShapePoints.a.Point;
                }
                else
                {
                    outsideFurthestDis = outsideDisA;
                    outsideClosestDis = outsideDisB;
                    outsideFurthestPoint = outsideShapePoints.a.Point;
                    outsideClosestPoint = outsideShapePoints.b.Point;
                }

                var insideDisA = insideShapePoints.a.Valid ? (insideShapePoints.a.Point - cur).LengthSquared() : 0f;
                var insideDisB = insideShapePoints.b.Valid ? (insideShapePoints.b.Point - cur).LengthSquared() : 0f;
                float insideFurthestDis;
                float insideClosestDis;
                Vector2 insideFurthestPoint;
                Vector2 insideClosestPoint;

                if (insideDisA < insideDisB)
                {
                    insideFurthestDis = insideDisB;
                    insideClosestDis = insideDisA;
                    insideFurthestPoint = insideShapePoints.b.Point;
                    insideClosestPoint = insideShapePoints.a.Point;
                }
                else
                {
                    insideFurthestDis = insideDisA;
                    insideClosestDis = insideDisB;
                    insideFurthestPoint = insideShapePoints.a.Point;
                    insideClosestPoint = insideShapePoints.b.Point;
                }

                //both outside shape points are inside the inside shape -> no drawing possible
                if (insideFurthestDis > outsideFurthestDis && insideClosestDis < outsideClosestDis)
                {
                    cur += dir * spacing;
                    continue;
                }

                if (insideClosestDis > outsideFurthestDis || insideFurthestDis < outsideClosestDis)
                {
                    var segment = new Segment(outsideClosestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else if (insideFurthestDis > outsideFurthestDis)
                {
                    var segment = new Segment(outsideClosestPoint, insideClosestPoint);
                    segment.Draw(striped);
                }
                else if (insideClosestDis < outsideClosestDis)
                {
                    var segment = new Segment(insideFurthestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else
                {
                    var segment1 = new Segment(outsideClosestPoint, insideClosestPoint);
                    segment1.Draw(striped);

                    var segment2 = new Segment(insideFurthestPoint, outsideFurthestPoint);
                    segment2.Draw(striped);
                }
            }

            cur += dir * spacing;
        }
    }

    /// <summary>
    /// Draws a striped pattern inside the outside shape without drawing in the inside shape.
    /// The inside shape does not have to be completely inside the outside shape.
    /// </summary>
    /// <param name="outsideShape">The shape for drawing the striped pattern inside.</param>
    /// <param name="insideShape">The shape to not draw any striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Quad outsideShape, Rect insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.Center;
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension; //offsets the point to the outside for using rays instead of lines

        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if (!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid)
            {
                cur += dir * spacing;
                continue;
            }

            var insideShapePoints = Ray.IntersectRayRect(cur, rayDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid && !insideShapePoints.b.Valid) //draw the ray - circle intersection points
            {
                var segment = new Segment(outsideShapePoints.a.Point, outsideShapePoints.b.Point);
                segment.Draw(striped);
            }
            else
            {
                var outsideDisA = (outsideShapePoints.a.Point - cur).LengthSquared();
                var outsideDisB = (outsideShapePoints.b.Point - cur).LengthSquared();
                float outsideFurthestDis;
                float outsideClosestDis;
                Vector2 outsideFurthestPoint;
                Vector2 outsideClosestPoint;

                if (outsideDisA < outsideDisB)
                {
                    outsideFurthestDis = outsideDisB;
                    outsideClosestDis = outsideDisA;
                    outsideFurthestPoint = outsideShapePoints.b.Point;
                    outsideClosestPoint = outsideShapePoints.a.Point;
                }
                else
                {
                    outsideFurthestDis = outsideDisA;
                    outsideClosestDis = outsideDisB;
                    outsideFurthestPoint = outsideShapePoints.a.Point;
                    outsideClosestPoint = outsideShapePoints.b.Point;
                }

                var insideDisA = insideShapePoints.a.Valid ? (insideShapePoints.a.Point - cur).LengthSquared() : 0f;
                var insideDisB = insideShapePoints.b.Valid ? (insideShapePoints.b.Point - cur).LengthSquared() : 0f;
                float insideFurthestDis;
                float insideClosestDis;
                Vector2 insideFurthestPoint;
                Vector2 insideClosestPoint;

                if (insideDisA < insideDisB)
                {
                    insideFurthestDis = insideDisB;
                    insideClosestDis = insideDisA;
                    insideFurthestPoint = insideShapePoints.b.Point;
                    insideClosestPoint = insideShapePoints.a.Point;
                }
                else
                {
                    insideFurthestDis = insideDisA;
                    insideClosestDis = insideDisB;
                    insideFurthestPoint = insideShapePoints.a.Point;
                    insideClosestPoint = insideShapePoints.b.Point;
                }

                //both outside shape points are inside the inside shape -> no drawing possible
                if (insideFurthestDis > outsideFurthestDis && insideClosestDis < outsideClosestDis)
                {
                    cur += dir * spacing;
                    continue;
                }

                if (insideClosestDis > outsideFurthestDis || insideFurthestDis < outsideClosestDis)
                {
                    var segment = new Segment(outsideClosestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else if (insideFurthestDis > outsideFurthestDis)
                {
                    var segment = new Segment(outsideClosestPoint, insideClosestPoint);
                    segment.Draw(striped);
                }
                else if (insideClosestDis < outsideClosestDis)
                {
                    var segment = new Segment(insideFurthestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else
                {
                    var segment1 = new Segment(outsideClosestPoint, insideClosestPoint);
                    segment1.Draw(striped);

                    var segment2 = new Segment(insideFurthestPoint, outsideFurthestPoint);
                    segment2.Draw(striped);
                }
            }

            cur += dir * spacing;
        }
    }

    /// <summary>
    /// Draws a striped pattern inside the outside shape without drawing in the inside shape.
    /// The inside shape does not have to be completely inside the outside shape.
    /// </summary>
    /// <param name="outsideShape">The shape for drawing the striped pattern inside.</param>
    /// <param name="insideShape">The shape to not draw any striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Quad outsideShape, Polygon insideShape, float spacing, float angleDeg, LineDrawingInfo striped,
        float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.Center;
        outsideShape.GetFurthestVertex(center, out float disSq, out int _);
        float maxDimension = MathF.Sqrt(disSq) * 2;

        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

        var cur = start + dir * spacing;
        cur -= lineDir * maxDimension; //offset the line point to the outside of the outside shape to make sorting possible

        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Line.IntersectLineQuad(cur, lineDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if (!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid)
            {
                cur += dir * spacing;
                continue;
            }

            var count = Line.IntersectLinePolygon(cur, lineDir, insideShape, ref intersectionPointsReference);

            if (count <= 0) //ray did not hit the inside shape, draw ray between edge of the outside shape
            {
                var segment = new Segment(outsideShapePoints.a.Point, outsideShapePoints.b.Point);
                segment.Draw(striped);
            }
            else
            {
                //remove all inside shape intersection points that are outside the outside shape
                for (int j = intersectionPointsReference.Count - 1; j >= 0; j--)
                {
                    var p = intersectionPointsReference[j].Point;

                    if (!outsideShape.ContainsPoint(p)) intersectionPointsReference.RemoveAt(j);
                }

                if (outsideShapePoints.a.Valid && !insideShape.ContainsPoint(outsideShapePoints.a.Point)) intersectionPointsReference.Add(outsideShapePoints.a);
                if (outsideShapePoints.b.Valid && !insideShape.ContainsPoint(outsideShapePoints.b.Point)) intersectionPointsReference.Add(outsideShapePoints.b);

                //all points were remove so just draw the outside shape segment (even with only 1 point left, we continue)
                if (intersectionPointsReference.Count <= 1)
                {
                    cur += dir * spacing;
                    intersectionPointsReference.Clear();
                    continue;
                }

                if (intersectionPointsReference.Count == 2) //no sorting or loop needed for exactly 2 points
                {
                    var segment = new Segment(intersectionPointsReference[0].Point, intersectionPointsReference[1].Point);
                    segment.Draw(striped);
                    cur += dir * spacing;
                    intersectionPointsReference.Clear();
                    continue;
                }

                //now that only valid points remain, sort them by distance from the current point
                intersectionPointsReference.SortClosestFirst(cur);

                for (int j = 0; j < intersectionPointsReference.Count - 1; j += 2)
                {
                    var p1 = intersectionPointsReference[j].Point;
                    var p2 = intersectionPointsReference[j + 1].Point;
                    var segment = new Segment(p1, p2);
                    segment.Draw(striped);
                }

                intersectionPointsReference.Clear();
            }

            cur += dir * spacing;
        }
    }

}