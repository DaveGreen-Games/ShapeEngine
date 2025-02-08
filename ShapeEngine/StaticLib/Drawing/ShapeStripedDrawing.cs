
using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapeStripedDrawing
{
    private static CollisionPoints collisionPointsReference = new CollisionPoints(6);
    
    #region Circle
    /// <summary>
    /// Draws a striped pattern inside the specified shape.
    /// </summary>
    /// <param name="circle">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Circle circle, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        float maxDimension = circle.Diameter;

        if (spacing > maxDimension) return;
        
        var center = circle.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
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
    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="circle">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The first line drawing info for drawing even lines.</param>
    /// <param name="alternatingStriped">The second line drawing info for drawing odd lines.</param>
    public static void DrawStriped(this Circle circle, float spacing, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
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
    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="circle">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="alternatingStriped">The line drawing infos for drawing each line. Each info is used in sequence and wraps around if there are more lines.</param>
    public static void DrawStriped(this Circle circle, float spacing, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacing <= 0) return;
        if (alternatingStriped.Length <= 0) return;
        if(alternatingStriped.Length == 1) DrawStriped(circle, spacing, angleDeg, alternatingStriped[0]);
        
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
    
    /// <summary>
    /// Draws a striped pattern inside the specified shape.
    /// </summary>
    /// <param name="circle">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    public static void DrawStriped(this Circle circle, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped)
    {
        if (spacingCurve.HasKeys == false) return;
        float maxDimension = circle.Diameter;
        
        var center = circle.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        if (!spacingCurve.Sample(0f, out float spacing)) return;
        
        if (spacing > maxDimension || spacing <= 0) return;

        var start = center - (dir * maxDimension * 0.5f);
        var cur = start + dir * spacing;
        var targetLength = spacing;
        
        while (targetLength < maxDimension)
        {
            var intersection = Line.IntersectLineCircle(cur, lineDir, circle.Center, circle.Radius);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                segment.Draw(striped);
            }
            
            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return;//prevents infinite loop
            
            targetLength += spacing;
            cur += dir * spacing;
        }
    }
    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="circle">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The first line drawing info for drawing even lines.</param>
    /// <param name="alternatingStriped">The second line drawing info for drawing odd lines.</param>
    public static void DrawStriped(this Circle circle, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacingCurve.HasKeys == false) return;
        float maxDimension = circle.Diameter;
        
        var center = circle.Center;
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
            var intersection = Line.IntersectLineCircle(cur, lineDir, circle.Center, circle.Radius);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var info = i % 2 == 0 ? striped : alternatingStriped;
                segment.Draw(info);
            }
            
            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return;//prevents infinite loop
            
            targetLength += spacing;
            cur += dir * spacing;
            i++;
        }
    }
    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="circle">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="alternatingStriped">The line drawing infos for drawing each line. Each info is used in sequence and wraps around if there are more lines.</param>
    public static void DrawStriped(this Circle circle, CurveFloat spacingCurve, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacingCurve.HasKeys == false) return;
        float maxDimension = circle.Diameter;
        
        var center = circle.Center;
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
            var intersection = Line.IntersectLineCircle(cur, lineDir, circle.Center, circle.Radius);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var index = i % alternatingStriped.Length;
                var info = alternatingStriped[index];
                segment.Draw(info);
            }
            
            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return;//prevents infinite loop
            
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
    public static void DrawStriped(this Circle outsideShape, Circle insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        float maxDimension = outsideShape.Diameter;

        if (spacing > maxDimension) return;
        
        var center = outsideShape.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayCircle(cur, rayDir, outsideShape.Center, outsideShape.Radius);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayCircle(cur, rayDir, insideShape.Center, insideShape.Radius);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Circle outsideShape, Triangle insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        float maxDimension = outsideShape.Diameter;

        if (spacing > maxDimension) return;
        
        var center = outsideShape.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayCircle(cur, rayDir, outsideShape.Center, outsideShape.Radius);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, insideShape.A, insideShape.B, insideShape.C);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Circle outsideShape, Quad insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        float maxDimension = outsideShape.Diameter;

        if (spacing > maxDimension) return;
        
        var center = outsideShape.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayCircle(cur, rayDir, outsideShape.Center, outsideShape.Radius);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayQuad(cur, rayDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Circle outsideShape, Rect insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        float maxDimension = outsideShape.Diameter;

        if (spacing > maxDimension) return;
        
        var center = outsideShape.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayCircle(cur, rayDir, outsideShape.Center, outsideShape.Radius);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayRect(cur, rayDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Circle outsideShape, Polygon insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        float maxDimension = outsideShape.Diameter;

        if (spacing > maxDimension) return;
        
        var center = outsideShape.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayCircle(cur, rayDir, outsideShape.Center, outsideShape.Radius);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var count = Ray.IntersectRayPolygon(cur, rayDir, insideShape, ref collisionPointsReference);
            
            if(count < 2) //ray did not hit the inside shape, draw ray between edge of the outside shape
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

                collisionPointsReference.SortClosestFirst(cur);
                var insideFurthestPoint = collisionPointsReference.Last.Point;
                var insideClosestPoint = collisionPointsReference.First.Point;
                float insideFurthestDis =  (insideFurthestPoint - cur).LengthSquared();
                float insideClosestDis = (insideClosestPoint - cur).LengthSquared();
                
                //the inside shape is outside the outside shape so draw the ray from edge to edge of the outside shape
                if (insideClosestDis > outsideFurthestDis || insideFurthestDis < outsideClosestDis)
                {
                    var segment = new Segment(outsideClosestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else
                {
                    //if the first segment is valid (completely inside), draw it
                    if (insideClosestDis > outsideClosestDis && insideClosestDis < outsideFurthestDis)
                    {
                        var segment = new Segment(outsideClosestPoint, insideClosestPoint);
                        segment.Draw(striped);
                    }   
                    
                    //if the last segment is valid (completely inside), draw it
                    if (insideFurthestDis < outsideFurthestDis && insideFurthestDis > outsideClosestDis)
                    {
                        var segment = new Segment(insideFurthestPoint, outsideFurthestPoint);
                        segment.Draw(striped);
                    }

                    for (int j = 1; j < collisionPointsReference.Count - 1; j+=2)
                    {
                        var p1 = collisionPointsReference[j].Point;
                        var p2 = collisionPointsReference[j + 1].Point;
                        var p1Dis = (p1 - cur).LengthSquared();
                        var p2Dis = (p2 - cur).LengthSquared();
                        
                        //if both points are outside on either side, don't draw the segment
                        if((p1Dis < outsideClosestDis || p1Dis > outsideFurthestDis) && (p2Dis < outsideClosestDis || p2Dis > outsideFurthestDis)) continue;
                        
                        //if 1 point is outside, take the corresponding point on the outside shape
                        if (p1Dis < outsideClosestDis) p1 = outsideClosestPoint;
                        else if(p1Dis > outsideFurthestDis) p1 = outsideFurthestPoint;
                        
                        //if 1 point is outside, take the corresponding point on the outside shape
                        if(p2Dis < outsideClosestDis) p2 = outsideClosestPoint;
                        else if(p2Dis > outsideFurthestDis) p2 = outsideFurthestPoint;
                        
                        var segment = new Segment(p1, p2);
                        segment.Draw(striped);
                    }
                }
                
                collisionPointsReference.Clear();
            }
            
            cur += dir * spacing;
        }
    }

    
    #endregion
    
    #region Triangle
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
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
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
            if (spacing <= 0f) return;//prevents infinite loop
            
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
            if (spacing <= 0f) return;//prevents infinite loop
            
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
            if (spacing <= 0f) return;//prevents infinite loop
            
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
    public static void DrawStriped(this Triangle outsideShape, Circle insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;

        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayCircle(cur, rayDir, insideShape.Center, insideShape.Radius);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Triangle outsideShape, Triangle insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, insideShape.A, insideShape.B, insideShape.C);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Triangle outsideShape, Quad insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayQuad(cur, rayDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Triangle outsideShape, Rect insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayRect(cur, rayDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Triangle outsideShape, Polygon insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var count = Ray.IntersectRayPolygon(cur, rayDir, insideShape, ref collisionPointsReference);
            
            if(count < 2) //ray did not hit the inside shape, draw ray between edge of the outside shape
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

                collisionPointsReference.SortClosestFirst(cur);
                var insideFurthestPoint = collisionPointsReference.Last.Point;
                var insideClosestPoint = collisionPointsReference.First.Point;
                float insideFurthestDis =  (insideFurthestPoint - cur).LengthSquared();
                float insideClosestDis = (insideClosestPoint - cur).LengthSquared();
                
                //the inside shape is outside the outside shape so draw the ray from edge to edge of the outside shape
                if (insideClosestDis > outsideFurthestDis || insideFurthestDis < outsideClosestDis)
                {
                    var segment = new Segment(outsideClosestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else
                {
                    //if the first segment is valid (completely inside), draw it
                    if (insideClosestDis > outsideClosestDis && insideClosestDis < outsideFurthestDis)
                    {
                        var segment = new Segment(outsideClosestPoint, insideClosestPoint);
                        segment.Draw(striped);
                    }   
                    
                    //if the last segment is valid (completely inside), draw it
                    if (insideFurthestDis < outsideFurthestDis && insideFurthestDis > outsideClosestDis)
                    {
                        var segment = new Segment(insideFurthestPoint, outsideFurthestPoint);
                        segment.Draw(striped);
                    }

                    for (int j = 1; j < collisionPointsReference.Count - 1; j+=2)
                    {
                        var p1 = collisionPointsReference[j].Point;
                        var p2 = collisionPointsReference[j + 1].Point;
                        var p1Dis = (p1 - cur).LengthSquared();
                        var p2Dis = (p2 - cur).LengthSquared();
                        
                        //if both points are outside on either side, don't draw the segment
                        if((p1Dis < outsideClosestDis || p1Dis > outsideFurthestDis) && (p2Dis < outsideClosestDis || p2Dis > outsideFurthestDis)) continue;
                        
                        //if 1 point is outside, take the corresponding point on the outside shape
                        if (p1Dis < outsideClosestDis) p1 = outsideClosestPoint;
                        else if(p1Dis > outsideFurthestDis) p1 = outsideFurthestPoint;
                        
                        //if 1 point is outside, take the corresponding point on the outside shape
                        if(p2Dis < outsideClosestDis) p2 = outsideClosestPoint;
                        else if(p2Dis > outsideFurthestDis) p2 = outsideFurthestPoint;
                        
                        var segment = new Segment(p1, p2);
                        segment.Draw(striped);
                    }
                }
                
                collisionPointsReference.Clear();
            }
            
            cur += dir * spacing;
        }
    }

    #endregion
    
    #region Quad
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
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
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
            if (spacing <= 0f) return;//prevents infinite loop
            
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
            if (spacing <= 0f) return;//prevents infinite loop
            
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
            if (spacing <= 0f) return;//prevents infinite loop
            
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
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayCircle(cur, rayDir, insideShape.Center, insideShape.Radius);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Quad outsideShape, Triangle insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.Center;
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, insideShape.A, insideShape.B, insideShape.C);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayQuad(cur, rayDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayRect(cur, rayDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Quad outsideShape, Polygon insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.Center;
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayQuad(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var count = Ray.IntersectRayPolygon(cur, rayDir, insideShape, ref collisionPointsReference);
            
            if(count < 2) //ray did not hit the inside shape, draw ray between edge of the outside shape
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

                collisionPointsReference.SortClosestFirst(cur);
                var insideFurthestPoint = collisionPointsReference.Last.Point;
                var insideClosestPoint = collisionPointsReference.First.Point;
                float insideFurthestDis =  (insideFurthestPoint - cur).LengthSquared();
                float insideClosestDis = (insideClosestPoint - cur).LengthSquared();
                
                //the inside shape is outside the outside shape so draw the ray from edge to edge of the outside shape
                if (insideClosestDis > outsideFurthestDis || insideFurthestDis < outsideClosestDis)
                {
                    var segment = new Segment(outsideClosestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else
                {
                    //if the first segment is valid (completely inside), draw it
                    if (insideClosestDis > outsideClosestDis && insideClosestDis < outsideFurthestDis)
                    {
                        var segment = new Segment(outsideClosestPoint, insideClosestPoint);
                        segment.Draw(striped);
                    }   
                    
                    //if the last segment is valid (completely inside), draw it
                    if (insideFurthestDis < outsideFurthestDis && insideFurthestDis > outsideClosestDis)
                    {
                        var segment = new Segment(insideFurthestPoint, outsideFurthestPoint);
                        segment.Draw(striped);
                    }

                    for (int j = 1; j < collisionPointsReference.Count - 1; j+=2)
                    {
                        var p1 = collisionPointsReference[j].Point;
                        var p2 = collisionPointsReference[j + 1].Point;
                        var p1Dis = (p1 - cur).LengthSquared();
                        var p2Dis = (p2 - cur).LengthSquared();
                        
                        //if both points are outside on either side, don't draw the segment
                        if((p1Dis < outsideClosestDis || p1Dis > outsideFurthestDis) && (p2Dis < outsideClosestDis || p2Dis > outsideFurthestDis)) continue;
                        
                        //if 1 point is outside, take the corresponding point on the outside shape
                        if (p1Dis < outsideClosestDis) p1 = outsideClosestPoint;
                        else if(p1Dis > outsideFurthestDis) p1 = outsideFurthestPoint;
                        
                        //if 1 point is outside, take the corresponding point on the outside shape
                        if(p2Dis < outsideClosestDis) p2 = outsideClosestPoint;
                        else if(p2Dis > outsideFurthestDis) p2 = outsideFurthestPoint;
                        
                        var segment = new Segment(p1, p2);
                        segment.Draw(striped);
                    }
                }
                
                collisionPointsReference.Clear();
            }
            
            cur += dir * spacing;
        }
    }

    #endregion
    
    #region Rect
    /// <summary>
    /// Draws a striped pattern inside the specified shape.
    /// </summary>
    /// <param name="rect">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    /// <param name="spacingOffset">An offset for the spacing between 0 and 1. Can be used for a continuously moving pattern.</param>
    public static void DrawStriped(this Rect rect, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        float maxDimension = (rect.TopLeft - rect.BottomRight).Length();

        if (spacing > maxDimension) return;
        
        var center = rect.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
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
    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="rect">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The first line drawing info for drawing even lines.</param>
    /// <param name="alternatingStriped">The second line drawing info for drawing odd lines.</param>
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
    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="rect">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacing">How far apart the lines are.</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="alternatingStriped">The line drawing infos for drawing each line. Each info is used in sequence and wraps around if there are more lines.</param>
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
   
    /// <summary>
    /// Draws a striped pattern inside the specified shape.
    /// </summary>
    /// <param name="rect">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The line drawing info for how the lines should be drawn.</param>
    public static void DrawStriped(this Rect rect, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = rect.Center;
        float maxDimension = (rect.TopLeft - rect.BottomRight).Length();
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var lineDir = dir.GetPerpendicularRight();
        if (!spacingCurve.Sample(0f, out float spacing)) return;
        
        if (spacing > maxDimension || spacing <= 0) return;

        var start = center - (dir * maxDimension * 0.5f);
        var cur = start + dir * spacing;
        var targetLength = spacing;
        
        while (targetLength < maxDimension)
        {
            var intersection = Line.IntersectLineRect(cur, lineDir, rect.A, rect.B, rect.C, rect.D);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                segment.Draw(striped);
            }
            
            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return;//prevents infinite loop
            
            targetLength += spacing;
            cur += dir * spacing;
        }
    }
    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="rect">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="striped">The first line drawing info for drawing even lines.</param>
    /// <param name="alternatingStriped">The second line drawing info for drawing odd lines.</param>
    public static void DrawStriped(this Rect rect, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = rect.Center;
        float maxDimension = (rect.TopLeft - rect.BottomRight).Length();
        
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
            var intersection = Line.IntersectLineRect(cur, lineDir, rect.A, rect.B, rect.C, rect.D);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var info = i % 2 == 0 ? striped : alternatingStriped;
                segment.Draw(info);
            }
            
            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return;//prevents infinite loop
            
            targetLength += spacing;
            cur += dir * spacing;
            i++;
        }
    }
    /// <summary>
    /// Draws an alternating striped pattern inside the specified shape.
    /// </summary>
    /// <param name="rect">The shape for drawing the striped pattern inside.</param>
    /// <param name="spacingCurve">The curve to determine the spacing along the shape. The value of each key has to be bigger than 0, otherwise the function will return early!</param>
    /// <param name="angleDeg">The angle of the striped pattern. 0 degrees would be vertical lines, 90 degrees would be horizontal lines.</param>
    /// <param name="alternatingStriped">The line drawing infos for drawing each line. Each info is used in sequence and wraps around if there are more lines.</param>
    public static void DrawStriped(this Rect rect, CurveFloat spacingCurve, float angleDeg, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = rect.Center;
        float maxDimension = (rect.TopLeft - rect.BottomRight).Length();
        
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
            var intersection = Line.IntersectLineRect(cur, lineDir, rect.A, rect.B, rect.C, rect.D);
            if (intersection.a.Valid && intersection.b.Valid)
            {
                var segment = new Segment(intersection.a.Point, intersection.b.Point);
                var infoIndex = i % alternatingStriped.Length;
                var info = alternatingStriped[infoIndex];
                segment.Draw(info);
            }
            
            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return;//prevents infinite loop
            
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
    public static void DrawStriped(this Rect outsideShape, Circle insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        float maxDimension = (outsideShape.TopLeft - outsideShape.BottomRight).Length();

        if (spacing > maxDimension) return;
        
        var center = outsideShape.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayRect(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayCircle(cur, rayDir, insideShape.Center, insideShape.Radius);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Rect outsideShape, Triangle insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        float maxDimension = (outsideShape.TopLeft - outsideShape.BottomRight).Length();

        if (spacing > maxDimension) return;
        
        var center = outsideShape.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayRect(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, insideShape.A, insideShape.B, insideShape.C);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Rect outsideShape, Quad insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        float maxDimension = (outsideShape.TopLeft - outsideShape.BottomRight).Length();

        if (spacing > maxDimension) return;
        
        var center = outsideShape.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayRect(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayQuad(cur, rayDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Rect outsideShape, Rect insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        float maxDimension = (outsideShape.TopLeft - outsideShape.BottomRight).Length();

        if (spacing > maxDimension) return;
        
        var center = outsideShape.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayRect(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var insideShapePoints = Ray.IntersectRayRect(cur, rayDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the ray - circle intersection points
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
                
                var insideDisA = (insideShapePoints.a.Point - cur).LengthSquared();
                var insideDisB = (insideShapePoints.b.Point - cur).LengthSquared();
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
    public static void DrawStriped(this Rect outsideShape, Polygon insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        float maxDimension = (outsideShape.TopLeft - outsideShape.BottomRight).Length();

        if (spacing > maxDimension) return;
        
        var center = outsideShape.Center;
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideShapePoints = Ray.IntersectRayRect(cur, rayDir, outsideShape.A, outsideShape.B, outsideShape.C, outsideShape.D);
            if(!outsideShapePoints.a.Valid || !outsideShapePoints.b.Valid) continue;
            
            var count = Ray.IntersectRayPolygon(cur, rayDir, insideShape, ref collisionPointsReference);
            
            if(count < 2) //ray did not hit the inside shape, draw ray between edge of the outside shape
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

                collisionPointsReference.SortClosestFirst(cur);
                var insideFurthestPoint = collisionPointsReference.Last.Point;
                var insideClosestPoint = collisionPointsReference.First.Point;
                float insideFurthestDis =  (insideFurthestPoint - cur).LengthSquared();
                float insideClosestDis = (insideClosestPoint - cur).LengthSquared();
                
                //the inside shape is outside the outside shape so draw the ray from edge to edge of the outside shape
                if (insideClosestDis > outsideFurthestDis || insideFurthestDis < outsideClosestDis)
                {
                    var segment = new Segment(outsideClosestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else
                {
                    //if the first segment is valid (completely inside), draw it
                    if (insideClosestDis > outsideClosestDis && insideClosestDis < outsideFurthestDis)
                    {
                        var segment = new Segment(outsideClosestPoint, insideClosestPoint);
                        segment.Draw(striped);
                    }   
                    
                    //if the last segment is valid (completely inside), draw it
                    if (insideFurthestDis < outsideFurthestDis && insideFurthestDis > outsideClosestDis)
                    {
                        var segment = new Segment(insideFurthestPoint, outsideFurthestPoint);
                        segment.Draw(striped);
                    }

                    for (int j = 1; j < collisionPointsReference.Count - 1; j+=2)
                    {
                        var p1 = collisionPointsReference[j].Point;
                        var p2 = collisionPointsReference[j + 1].Point;
                        var p1Dis = (p1 - cur).LengthSquared();
                        var p2Dis = (p2 - cur).LengthSquared();
                        
                        //if both points are outside on either side, don't draw the segment
                        if((p1Dis < outsideClosestDis || p1Dis > outsideFurthestDis) && (p2Dis < outsideClosestDis || p2Dis > outsideFurthestDis)) continue;
                        
                        //if 1 point is outside, take the corresponding point on the outside shape
                        if (p1Dis < outsideClosestDis) p1 = outsideClosestPoint;
                        else if(p1Dis > outsideFurthestDis) p1 = outsideFurthestPoint;
                        
                        //if 1 point is outside, take the corresponding point on the outside shape
                        if(p2Dis < outsideClosestDis) p2 = outsideClosestPoint;
                        else if(p2Dis > outsideFurthestDis) p2 = outsideFurthestPoint;
                        
                        var segment = new Segment(p1, p2);
                        segment.Draw(striped);
                    }
                }
                
                collisionPointsReference.Clear();
            }
            
            cur += dir * spacing;
        }
    }

    #endregion
    
    #region Polygon
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
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to outside the polygon in the opposite direction of the ray
        for (int i = 0; i < steps; i++)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, polygon, ref collisionPointsReference);
            if (count >= 2)
            {
                for (int j = 0; j < collisionPointsReference.Count - 1; j+=2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    ShapeSegmentDrawing.DrawSegment(p1, p2, striped);
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
        cur -= rayDir * maxDimension;//offsets the point to outside the polygon in the opposite direction of the ray
        for (int i = 0; i < steps; i++)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, polygon, ref collisionPointsReference);
            if (count >= 2)
            {
                var info = i % 2 == 0 ? striped : alternatingStriped;
                for (int j = 0; j < collisionPointsReference.Count - 1; j+=2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    ShapeSegmentDrawing.DrawSegment(p1, p2, info);
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
        var center = polygon.GetCentroid();
        
        polygon.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to outside the polygon in the opposite direction of the ray
        for (int i = 0; i < steps; i++)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, polygon, ref collisionPointsReference);
            if (count >= 2)
            {
                var infoIndex = i % alternatingStriped.Length;
                var info = alternatingStriped[infoIndex];
                for (int j = 0; j < collisionPointsReference.Count - 1; j+=2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    ShapeSegmentDrawing.DrawSegment(p1, p2, info);
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
        cur -= rayDir * maxDimension;//offsets the point to outside the polygon in the opposite direction of the ray
        var targetLength = spacing;
        
        while (targetLength < maxDimension)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, polygon, ref collisionPointsReference);
            if (count >= 2)
            {
                for (int j = 0; j < collisionPointsReference.Count - 1; j+=2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    ShapeSegmentDrawing.DrawSegment(p1, p2, striped);
                }
            }
            collisionPointsReference.Clear();
            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return;//prevents infinite loop
            
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
        cur -= rayDir * maxDimension;//offsets the point to outside the polygon in the opposite direction of the ray
        var targetLength = spacing;
        int i = 0;
        while (targetLength < maxDimension)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, polygon, ref collisionPointsReference);
            if (count >= 2)
            {
                var info = i % 2 == 0 ? striped : alternatingStriped;
                for (int j = 0; j < collisionPointsReference.Count - 1; j+=2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    ShapeSegmentDrawing.DrawSegment(p1, p2, info);
                }
            }
            collisionPointsReference.Clear();
            
            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return;//prevents infinite loop
            
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
        cur -= rayDir * maxDimension;//offsets the point to outside the polygon in the opposite direction of the ray
        var targetLength = spacing;

        int i = 0;
        while (targetLength < maxDimension)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, polygon, ref collisionPointsReference);
            if (count >= 2)
            {
                var infoIndex = i % alternatingStriped.Length;
                var info = alternatingStriped[infoIndex];
                for (int j = 0; j < collisionPointsReference.Count - 1; j+=2)
                {
                    var p1 = collisionPointsReference[j].Point;
                    var p2 = collisionPointsReference[j + 1].Point;
                    ShapeSegmentDrawing.DrawSegment(p1, p2, info);
                }
            }
            collisionPointsReference.Clear();
            
            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return;//prevents infinite loop
            
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
    public static void DrawStriped(this Polygon outsideShape, Circle insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
        
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
    
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, outsideShape, ref collisionPointsReference);
            if (count < 2)
            {
                cur += dir * spacing;
                continue;
            }
            
            var insideShapePoints = Ray.IntersectRayCircle(cur, rayDir, insideShape.Center, insideShape.Radius);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the lines in the outside shape
            {
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j+=2)
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
                    if(insideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                }
                
                if(outsideShape.ContainsPoint(insideShapePoints.a.Point)) collisionPointsReference.Add(insideShapePoints.a);
                if(outsideShape.ContainsPoint(insideShapePoints.b.Point)) collisionPointsReference.Add(insideShapePoints.b);
                
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j+=2)
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
    public static void DrawStriped(this Polygon outsideShape, Triangle insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
        
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
    
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, outsideShape, ref collisionPointsReference);
            if (count < 2)
            {
                cur += dir * spacing;
                continue;
            }
            
            var insideShapePoints = Ray.IntersectRayTriangle(cur, rayDir, insideShape.A, insideShape.B, insideShape.C);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the lines in the outside shape
            {
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j+=2)
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
                    if(insideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                }
                
                if(outsideShape.ContainsPoint(insideShapePoints.a.Point)) collisionPointsReference.Add(insideShapePoints.a);
                if(outsideShape.ContainsPoint(insideShapePoints.b.Point)) collisionPointsReference.Add(insideShapePoints.b);
                
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j+=2)
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
    public static void DrawStriped(this Polygon outsideShape, Quad insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
        
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
    
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, outsideShape, ref collisionPointsReference);
            if (count < 2)
            {
                cur += dir * spacing;
                continue;
            }
            
            var insideShapePoints = Ray.IntersectRayQuad(cur, rayDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the lines in the outside shape
            {
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j+=2)
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
                    if(insideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                }
                
                if(outsideShape.ContainsPoint(insideShapePoints.a.Point)) collisionPointsReference.Add(insideShapePoints.a);
                if(outsideShape.ContainsPoint(insideShapePoints.b.Point)) collisionPointsReference.Add(insideShapePoints.b);
                
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j+=2)
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
    public static void DrawStriped(this Polygon outsideShape, Rect insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
        
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
    
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var count = Ray.IntersectRayPolygon(cur, rayDir, outsideShape, ref collisionPointsReference);
            if (count < 2)
            {
                cur += dir * spacing;
                continue;
            }
            
            var insideShapePoints = Ray.IntersectRayRect(cur, rayDir, insideShape.A, insideShape.B, insideShape.C, insideShape.D);
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //draw the lines in the outside shape
            {
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j+=2)
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
                    if(insideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                }
                
                if(outsideShape.ContainsPoint(insideShapePoints.a.Point)) collisionPointsReference.Add(insideShapePoints.a);
                if(outsideShape.ContainsPoint(insideShapePoints.b.Point)) collisionPointsReference.Add(insideShapePoints.b);
                
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j+=2)
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
    public static void DrawStriped(this Polygon outsideShape, Polygon insideShape, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = outsideShape.GetCentroid();
        
        outsideShape.GetFurthestVertex(center, out float disSquared, out int _);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
    
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
        int steps = (int)(maxDimension / spacing);
        
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to the outside for using rays instead of lines
        
        for (int i = 0; i < steps; i++)
        {
            var outsideCount = Ray.IntersectRayPolygon(cur, rayDir, outsideShape, ref collisionPointsReference);
            if (outsideCount < 2)
            {
                cur += dir * spacing;
                continue;
            }
            
            var insideCount = Ray.IntersectRayPolygon(cur, rayDir, insideShape, ref collisionPointsReference);
            if (insideCount < 0) //draw the lines in the outside shape
            {
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j+=2)
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
                    if (j >= outsideCount)//we are processing the points from the inside shape
                    {
                        if(!outsideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                    }
                    else// we are processing the points from the outside shape
                    {
                        if(insideShape.ContainsPoint(p)) collisionPointsReference.RemoveAt(j);
                    }
                    
                }
                
                collisionPointsReference.SortClosestFirst(cur);
                for (int j = 0; j < collisionPointsReference.Count; j+=2)
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
    
    #endregion
}