using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.StripedDrawingDef;

public static partial class StripedDrawing
{
    /// <summary>
    /// Draws a striped pattern inside the specified shape.
    /// </summary>
    /// <param name="triangle">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Triangle triangle, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = triangle.GetCentroid();
        triangle.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

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

    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="triangle">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The first line drawing info for drawing even lines.</param>
    /// <param name="alternatingStriped">The second line drawing info for drawing odd lines.</param>
    public static void DrawStriped(this Triangle triangle, float spacing, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = triangle.GetCentroid();
        triangle.GetFurthestVertex(center, out float disSquared, out int _);
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

    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="triangle">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="alternatingStriped">The line drawing infos for drawing each line. Each info is used in sequence and wraps around if there are more lines.</param>
    public static void DrawStriped(this Triangle triangle, float spacing, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = triangle.GetCentroid();
        triangle.GetFurthestVertex(center, out float disSquared, out int _);
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

    /// <summary>
    /// Draws a striped pattern inside the specified shape.
    /// </summary>
    /// <param name="triangle">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    public static void DrawStriped(this Triangle triangle, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = triangle.GetCentroid();
        triangle.GetFurthestVertex(center, out float disSquared, out int _);
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
            var intersection = Line.IntersectLineTriangle(cur, lineDir, triangle.A, triangle.B, triangle.C);
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
    /// <param name="triangle">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The first line drawing info for drawing even lines.</param>
    /// <param name="alternatingStriped">The second line drawing info for drawing odd lines.</param>
    public static void DrawStriped(this Triangle triangle, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = triangle.GetCentroid();
        triangle.GetFurthestVertex(center, out float disSquared, out int _);
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
            var intersection = Line.IntersectLineTriangle(cur, lineDir, triangle.A, triangle.B, triangle.C);
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
    /// <param name="triangle">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="alternatingStriped">The line drawing infos for drawing each line. Each info is used in sequence and wraps around if there are more lines.</param>
    public static void DrawStriped(this Triangle triangle, CurveFloat spacingCurve, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = triangle.GetCentroid();
        triangle.GetFurthestVertex(center, out float disSquared, out int _);
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
            var intersection = Line.IntersectLineTriangle(cur, lineDir, triangle.A, triangle.B, triangle.C);
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
    public static void DrawStriped(this Triangle outsideShape, Circle insideShape, float spacing, float angleDeg, LineDrawingInfo striped,
        float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
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
            var outsideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C);
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
    public static void DrawStriped(this Triangle outsideShape, Triangle insideShape, float spacing, float angleDeg, LineDrawingInfo striped,
        float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
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
            var outsideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C);
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
    public static void DrawStriped(this Triangle outsideShape, Quad insideShape, float spacing, float angleDeg, LineDrawingInfo striped,
        float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
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
            var outsideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C);
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
    public static void DrawStriped(this Triangle outsideShape, Rect insideShape, float spacing, float angleDeg, LineDrawingInfo striped,
        float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
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
            var outsideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C);
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
    public static void DrawStriped(this Triangle outsideShape, Polygon insideShape, float spacing, float angleDeg, LineDrawingInfo striped,
        float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
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
            var outsideShapePoints = Line.IntersectLineTriangle(cur, lineDir, outsideShape.A, outsideShape.B, outsideShape.C);
            if (!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid)
            {
                cur += dir * spacing;
                continue;
            }

            var count = Line.IntersectLinePolygon(cur, lineDir, insideShape, ref collisionPointsReference);

            if (count <= 0) //ray did not hit the inside shape, draw ray between edge of the outside shape
            {
                var segment = new Segment(outsideShapePoints.a.Point, outsideShapePoints.b.Point);
                segment.Draw(striped);
            }
            else
            {
                //remove all inside shape intersection points that are outside the outside shape
                for (int j = collisionPointsReference.Count - 1; j >= 0; j--)
                {
                    var p = collisionPointsReference[j].Point;

                    if (!outsideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                }

                if (outsideShapePoints.a.Valid && !insideShape.ContainsPoint(outsideShapePoints.a.Point)) collisionPointsReference.Add(outsideShapePoints.a);
                if (outsideShapePoints.b.Valid && !insideShape.ContainsPoint(outsideShapePoints.b.Point)) collisionPointsReference.Add(outsideShapePoints.b);

                //all points were remove so just draw the outside shape segment (even with only 1 point left, we continue)
                if (collisionPointsReference.Count <= 1)
                {
                    cur += dir * spacing;
                    collisionPointsReference.Clear();
                    continue;
                }

                if (collisionPointsReference.Count == 2) //no sorting or loop needed for exactly 2 points
                {
                    var segment = new Segment(collisionPointsReference[0].Point, collisionPointsReference[1].Point);
                    segment.Draw(striped);
                    cur += dir * spacing;
                    collisionPointsReference.Clear();
                    continue;
                }

                //now that only valid points remain, sort them by distance from the current point
                collisionPointsReference.SortClosestFirst(cur);

                for (int j = 0; j < collisionPointsReference.Count - 1; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    var segment = new Segment(p1, p2);
                    segment.Draw(striped);
                }

                collisionPointsReference.Clear();
            }

            cur += dir * spacing;
        }
    }

}