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
    /// <param name="polygon">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Polygon polygon, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = polygon.GetCentroid();

        polygon.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension; //offsets the point to outside the polygon in the opposite direction of the ray
        for (int i = 0; i < steps; i++)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, polygon, ref collisionPointsReference);
            if (count >= 2) //minimum of 2 points for drawing needed
            {
                if (count >= 4) //only if there is 4 or more points, sort the points for drawing
                {
                    collisionPointsReference.SortClosestFirst(cur);
                }

                for (int j = 0; j < collisionPointsReference.Count - 1; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    SegmentDrawing.DrawSegment(p1, p2, striped);
                }
            }

            collisionPointsReference.Clear();

            cur += dir * spacing;
        }
    }

    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="polygon">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The first line drawing info for drawing even lines.</param>
    /// <param name="alternatingStriped">The second line drawing info for drawing odd lines.</param>
    public static void DrawStriped(this Polygon polygon, float spacing, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = polygon.GetCentroid();

        polygon.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);

        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension; //offsets the point to outside the polygon in the opposite direction of the ray
        for (int i = 0; i < steps; i++)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, polygon, ref collisionPointsReference);
            if (count >= 2) //minimum of 2 points for drawing needed
            {
                if (count >= 4) //only if there is 4 or more points, sort the points for drawing
                {
                    collisionPointsReference.SortClosestFirst(cur);
                }

                var info = i % 2 == 0 ? striped : alternatingStriped;
                for (int j = 0; j < collisionPointsReference.Count - 1; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    SegmentDrawing.DrawSegment(p1, p2, info);
                }
            }

            collisionPointsReference.Clear();
            cur += dir * spacing;
        }
    }

    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="polygon">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="alternatingStriped">The line drawing infos for drawing each line. Each info is used in sequence and wraps around if there are more lines.</param>
    public static void DrawStriped(this Polygon polygon, float spacing, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacing <= 0) return;
        if (alternatingStriped.Length <= 0) return;
        var center = polygon.GetCentroid();

        polygon.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);

        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension; //offsets the point to outside the polygon in the opposite direction of the ray
        for (int i = 0; i < steps; i++)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, polygon, ref collisionPointsReference);
            if (count >= 2) //minimum of 2 points for drawing needed
            {
                if (count >= 4) //only if there is 4 or more points, sort the points for drawing
                {
                    collisionPointsReference.SortClosestFirst(cur);
                }

                var infoIndex = i % alternatingStriped.Length;
                var info = alternatingStriped[infoIndex];
                for (int j = 0; j < collisionPointsReference.Count - 1; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    SegmentDrawing.DrawSegment(p1, p2, info);
                }
            }

            collisionPointsReference.Clear();
            cur += dir * spacing;
        }
    }

    /// <summary>
    /// Draws a striped pattern inside the specified shape.
    /// </summary>
    /// <param name="polygon">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    public static void DrawStriped(this Polygon polygon, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = polygon.GetCentroid();
        polygon.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        if (!spacingCurve.Sample(0f, out float spacing)) return;

        if (spacing > maxDimension || spacing <= 0) return;

        var start = center - (dir * maxDimension * 0.5f);
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension; //offsets the point to outside the polygon in the opposite direction of the ray
        var targetLength = spacing;

        while (targetLength < maxDimension)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, polygon, ref collisionPointsReference);
            if (count >= 2) //minimum of 2 points for drawing needed
            {
                if (count >= 4) //only if there is 4 or more points, sort the points for drawing
                {
                    collisionPointsReference.SortClosestFirst(cur);
                }

                for (int j = 0; j < collisionPointsReference.Count - 1; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    SegmentDrawing.DrawSegment(p1, p2, striped);
                }
            }

            collisionPointsReference.Clear();
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
    /// <param name="polygon">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The first line drawing info for drawing even lines.</param>
    /// <param name="alternatingStriped">The second line drawing info for drawing odd lines.</param>
    public static void DrawStriped(this Polygon polygon, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = polygon.GetCentroid();
        polygon.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        if (!spacingCurve.Sample(0f, out float spacing)) return;

        if (spacing > maxDimension || spacing <= 0) return;

        var start = center - (dir * maxDimension * 0.5f);
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension; //offsets the point to outside the polygon in the opposite direction of the ray
        var targetLength = spacing;
        int i = 0;
        while (targetLength < maxDimension)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, polygon, ref collisionPointsReference);
            if (count >= 2) //minimum of 2 points for drawing needed
            {
                if (count >= 4) //only if there is 4 or more points, sort the points for drawing
                {
                    collisionPointsReference.SortClosestFirst(cur);
                }

                var info = i % 2 == 0 ? striped : alternatingStriped;
                for (int j = 0; j < collisionPointsReference.Count - 1; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    SegmentDrawing.DrawSegment(p1, p2, info);
                }
            }

            collisionPointsReference.Clear();

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
    /// <param name="polygon">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="alternatingStriped">The line drawing infos for drawing each line. Each info is used in sequence and wraps around if there are more lines.</param>
    public static void DrawStriped(this Polygon polygon, CurveFloat spacingCurve, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (alternatingStriped.Length <= 0) return;
        if (spacingCurve.HasKeys == false) return;
        var center = polygon.GetCentroid();
        polygon.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        if (!spacingCurve.Sample(0f, out float spacing)) return;

        if (spacing > maxDimension || spacing <= 0) return;

        var start = center - (dir * maxDimension * 0.5f);
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension; //offsets the point to outside the polygon in the opposite direction of the ray
        var targetLength = spacing;

        int i = 0;
        while (targetLength < maxDimension)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, polygon, ref collisionPointsReference);
            if (count >= 2) //minimum of 2 points for drawing needed
            {
                if (count >= 4) //only if there is 4 or more points, sort the points for drawing
                {
                    collisionPointsReference.SortClosestFirst(cur);
                }

                var infoIndex = i % alternatingStriped.Length;
                var info = alternatingStriped[infoIndex];
                for (int j = 0; j < collisionPointsReference.Count - 1; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    SegmentDrawing.DrawSegment(p1, p2, info);
                }
            }

            collisionPointsReference.Clear();

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
    /// <param name="angleDeg">The angle of the striped pattern.
    /// 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Polygon outsideShape, Circle insideShape, float spacing, float angleDeg, LineDrawingInfo striped,
        float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();

        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

        var cur = start + dir * spacing;
        cur -= lineDir * maxDimension; //offsets the point to the outside for using rays instead of lines

        for (int i = 0; i < steps; i++)
        {
            var count = Line.IntersectLinePolygon(cur, lineDir, outsideShape, ref collisionPointsReference);
            if (count < 2)
            {
                cur += dir * spacing;
                continue;
            }

            var insideShapePoints = Line.IntersectLineCircle(cur, lineDir, insideShape.Center, insideShape.Radius);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the lines in the outside shape
            {
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    var segment = new Segment(p1, p2);
                    segment.Draw(striped);
                }
            }
            else
            {
                //remove all intersection points of the outside shape that are inside the inside shape
                for (int j = collisionPointsReference.Count - 1; j >= 0; j--)
                {
                    var p = collisionPointsReference[j].Point;
                    if (insideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                }

                if (outsideShape.ContainsPoint(insideShapePoints.a.Point)) collisionPointsReference.Add(insideShapePoints.a);
                if (outsideShape.ContainsPoint(insideShapePoints.b.Point)) collisionPointsReference.Add(insideShapePoints.b);

                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    var segment = new Segment(p1, p2);
                    segment.Draw(striped);
                }
            }

            collisionPointsReference.Clear();

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
    /// <param name="angleDeg">The angle of the striped pattern.
    /// 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Polygon outsideShape, Triangle insideShape, float spacing, float angleDeg, LineDrawingInfo striped,
        float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();

        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

        var cur = start + dir * spacing;
        cur -= lineDir * maxDimension; //offsets the point to the outside for using rays instead of lines

        for (int i = 0; i < steps; i++)
        {
            var count = Line.IntersectLinePolygon(cur, lineDir, outsideShape, ref collisionPointsReference);
            if (count < 2)
            {
                cur += dir * spacing;
                continue;
            }

            var insideShapePoints = Line.IntersectLineTriangle(cur, lineDir, insideShape.A, insideShape.B, insideShape.C);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the lines in the outside shape
            {
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    var segment = new Segment(p1, p2);
                    segment.Draw(striped);
                }
            }
            else
            {
                //remove all intersection points of the outside shape that are inside the inside shape
                for (int j = collisionPointsReference.Count - 1; j >= 0; j--)
                {
                    var p = collisionPointsReference[j].Point;
                    if (insideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                }

                if (outsideShape.ContainsPoint(insideShapePoints.a.Point)) collisionPointsReference.Add(insideShapePoints.a);
                if (outsideShape.ContainsPoint(insideShapePoints.b.Point)) collisionPointsReference.Add(insideShapePoints.b);

                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    var segment = new Segment(p1, p2);
                    segment.Draw(striped);
                }
            }

            collisionPointsReference.Clear();

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
    /// <param name="angleDeg">The angle of the striped pattern.
    /// 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Polygon outsideShape, Quad insideShape, float spacing, float angleDeg, LineDrawingInfo striped,
        float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();

        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

        var cur = start + dir * spacing;
        cur -= lineDir * maxDimension; //offsets the point to the outside for using rays instead of lines

        for (int i = 0; i < steps; i++)
        {
            var count = Line.IntersectLinePolygon(cur, lineDir, outsideShape, ref collisionPointsReference);
            if (count < 2)
            {
                cur += dir * spacing;
                continue;
            }

            var insideShapePoints = Line.IntersectLineQuad(cur, lineDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the lines in the outside shape
            {
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    var segment = new Segment(p1, p2);
                    segment.Draw(striped);
                }
            }
            else
            {
                //remove all intersection points of the outside shape that are inside the inside shape
                for (int j = collisionPointsReference.Count - 1; j >= 0; j--)
                {
                    var p = collisionPointsReference[j].Point;
                    if (insideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                }

                if (outsideShape.ContainsPoint(insideShapePoints.a.Point)) collisionPointsReference.Add(insideShapePoints.a);
                if (outsideShape.ContainsPoint(insideShapePoints.b.Point)) collisionPointsReference.Add(insideShapePoints.b);

                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    var segment = new Segment(p1, p2);
                    segment.Draw(striped);
                }
            }

            collisionPointsReference.Clear();

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
    /// <param name="angleDeg">The angle of the striped pattern.
    /// 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Polygon outsideShape, Rect insideShape, float spacing, float angleDeg, LineDrawingInfo striped,
        float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();

        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

        var cur = start + dir * spacing;
        cur -= lineDir * maxDimension; //offsets the point to the outside for using rays instead of lines

        for (int i = 0; i < steps; i++)
        {
            var count = Line.IntersectLinePolygon(cur, lineDir, outsideShape, ref collisionPointsReference);
            if (count < 2)
            {
                cur += dir * spacing;
                continue;
            }

            var insideShapePoints = Line.IntersectLineRect(cur, lineDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the lines in the outside shape
            {
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    var segment = new Segment(p1, p2);
                    segment.Draw(striped);
                }
            }
            else
            {
                //remove all intersection points of the outside shape that are inside the inside shape
                for (int j = collisionPointsReference.Count - 1; j >= 0; j--)
                {
                    var p = collisionPointsReference[j].Point;
                    if (insideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                }

                if (outsideShape.ContainsPoint(insideShapePoints.a.Point)) collisionPointsReference.Add(insideShapePoints.a);
                if (outsideShape.ContainsPoint(insideShapePoints.b.Point)) collisionPointsReference.Add(insideShapePoints.b);

                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    var segment = new Segment(p1, p2);
                    segment.Draw(striped);
                }
            }

            collisionPointsReference.Clear();

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
    /// <param name="angleDeg">The angle of the striped pattern.
    /// 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Polygon outsideShape, Polygon insideShape, float spacing, float angleDeg, LineDrawingInfo striped,
        float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();

        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;

        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var totalSpacingOffset = spacing * spacingOffset;
        var start = center - dir * (maxDimension * 0.5f + totalSpacingOffset);
        int steps = (int)((maxDimension + totalSpacingOffset) / spacing);

        var cur = start + dir * spacing;
        cur -= lineDir * maxDimension; //offsets the point to the outside for using rays instead of lines

        for (int i = 0; i < steps; i++)
        {
            var outsideCount = Line.IntersectLinePolygon(cur, lineDir, outsideShape, ref collisionPointsReference);
            if (outsideCount < 2)
            {
                cur += dir * spacing;
                continue;
            }

            var insideCount = Line.IntersectLinePolygon(cur, lineDir, insideShape, ref collisionPointsReference);
            //this is correct, insideCount <= 0 leads to crashes!
            if (insideCount < 0) //draw the lines in the outside shape
            {
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    var segment = new Segment(p1, p2);
                    segment.Draw(striped);
                }
            }
            else
            {
                //remove all intersection points of the outside shape that are inside the inside shape
                for (int j = collisionPointsReference.Count - 1; j >= 0; j--)
                {
                    var p = collisionPointsReference[j].Point;
                    if (j >= outsideCount) //we are processing the points from the inside shape
                    {
                        if (!outsideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                    }
                    else // we are processing the points from the outside shape
                    {
                        if (insideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                    }
                }

                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j += 2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    var segment = new Segment(p1, p2);
                    segment.Draw(striped);
                }
            }

            collisionPointsReference.Clear();

            cur += dir * spacing;
        }
    }
}