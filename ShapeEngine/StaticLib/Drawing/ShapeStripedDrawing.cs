using System.Drawing;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapeStripedDrawing
{
    #region Circle
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
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //just draw the ray - circle intersection points
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

                //just draw outside shape segment because the inside shape points are outside of the outside shape
                if (insideClosestDis > outsideFurthestDis || insideFurthestDis < outsideClosestDis)
                {
                    var segment = new Segment(outsideClosestPoint, outsideFurthestPoint);
                    segment.Draw(striped);
                }
                else if (insideFurthestDis > outsideFurthestDis) //part of the the inside shape is outside of the outside shape
                {
                    var segment = new Segment(outsideClosestPoint, insideClosestPoint);
                    segment.Draw(striped);
                }   
                else if (insideClosestDis < outsideClosestDis) //part of the insde shape is outside of the outside shape
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
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //just draw the ray - circle intersection points
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
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //just draw the ray - circle intersection points
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
            if (!insideShapePoints.a.Valid || !insideShapePoints.b.Valid) //just draw the ray - circle intersection points
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
            
            var insideShapePoints = Ray.IntersectRayPolygon(cur, rayDir, insideShape);
            if (insideShapePoints == null || insideShapePoints.Count < 2)
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

                insideShapePoints.SortClosestFirst(cur);
                float insideFurthestDis =  (insideShapePoints.Last.Point - cur).LengthSquared();
                float insideClosestDis = (insideShapePoints.First.Point - cur).LengthSquared();
                var insideFurthestPoint = insideShapePoints.Last.Point;
                var insideClosestPoint = insideShapePoints.First.Point;
                
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
                    var segmentFirst = new Segment(outsideClosestPoint, insideClosestPoint);
                    segmentFirst.Draw(striped);

                    for (int j = 1; j < insideShapePoints.Count - 1; j+=2)
                    {
                        var p1 = insideShapePoints[j].Point;
                        var p2 = insideShapePoints[j + 1].Point;
                        var segment = new Segment(p1, p2);
                        segment.Draw(striped);
                    }
                    
                    var segmentLast = new Segment(insideFurthestPoint, outsideFurthestPoint);
                    segmentLast.Draw(striped);
                }
            }
            
            cur += dir * spacing;
        }
    }

    
    #endregion
    
    #region Triangle
    public static void DrawStriped(this Triangle triangle, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = triangle.GetCentroid();
        triangle.GetFurthestVertex(center, out float disSquared, out int index);
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
    public static void DrawStriped(this Triangle triangle, float spacing, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = triangle.GetCentroid();
        triangle.GetFurthestVertex(center, out float disSquared, out int index);
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
        triangle.GetFurthestVertex(center, out float disSquared, out int index);
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
    
    public static void DrawStriped(this Triangle triangle, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, float sideLength = 8f)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = triangle.GetCentroid();
        triangle.GetFurthestVertex(center, out float disSquared, out int index);
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
    public static void DrawStriped(this Triangle triangle, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped, float sideLength = 8f)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = triangle.GetCentroid();
        triangle.GetFurthestVertex(center, out float disSquared, out int index);
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
    public static void DrawStriped(this Triangle triangle, CurveFloat spacingCurve, float angleDeg, float sideLength, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = triangle.GetCentroid();
        triangle.GetFurthestVertex(center, out float disSquared, out int index);
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
    
    #endregion
    
    #region Quad
    
    public static void DrawStriped(this Quad quad, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int index);
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
    public static void DrawStriped(this Quad quad, float spacing, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        if (spacing <= 0) return;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int index);
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
        quad.GetFurthestVertex(center, out float disSquared, out int index);
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
    
    public static void DrawStriped(this Quad quad, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, float sideLength = 8f)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int index);
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
    public static void DrawStriped(this Quad quad, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped, float sideLength = 8f)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int index);
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
    public static void DrawStriped(this Quad quad, CurveFloat spacingCurve, float angleDeg, float sideLength, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = quad.Center;
        quad.GetFurthestVertex(center, out float disSquared, out int index);
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
    
    #endregion
    
    #region Rect
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
   
    public static void DrawStriped(this Rect rect, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, float sideLength = 8f)
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
    public static void DrawStriped(this Rect rect, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped, float sideLength = 8f)
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
    public static void DrawStriped(this Rect rect, CurveFloat spacingCurve, float angleDeg, float sideLength, params LineDrawingInfo[] alternatingStriped)
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
    
    #endregion
    
    #region Polygon
    
    public static void DrawStriped(this Polygon polygon, float spacing, float angleDeg, LineDrawingInfo striped, float spacingOffset = 0f)
    {
        if (spacing <= 0) return;
        var center = polygon.GetCentroid();
        
        polygon.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        if (spacing > maxDimension) return;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        spacingOffset = ShapeMath.WrapF(spacingOffset, 0f, 1f);
        var start = center - (dir * (maxDimension * 0.5f + spacing * spacingOffset));
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
    
    public static void DrawStriped(this Polygon polygon, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, float sideLength = 8f)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = polygon.GetCentroid();
        polygon.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        if (!spacingCurve.Sample(0f, out float spacing)) return;
        
        if (spacing > maxDimension || spacing <= 0) return;

        var start = center - (dir * maxDimension * 0.5f);
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to outside of the polygon in the opposite direction of the ray
        var targetLength = spacing;
        
        while (targetLength < maxDimension)
        {
            var segments = polygon.CutRayWithPolygon(cur, rayDir);
            if (segments != null && segments.Count > 0)
            {
                foreach (var segment in segments)
                {
                    segment.Draw(striped);
                }
            }
            
            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return;//prevents infinite loop
            
            targetLength += spacing;
            cur += dir * spacing;
        }
    }
    public static void DrawStriped(this Polygon polygon, CurveFloat spacingCurve, float angleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped, float sideLength = 8f)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = polygon.GetCentroid();
        polygon.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        if (!spacingCurve.Sample(0f, out float spacing)) return;
        
        if (spacing > maxDimension || spacing <= 0) return;

        var start = center - (dir * maxDimension * 0.5f);
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to outside of the polygon in the opposite direction of the ray
        var targetLength = spacing;
        int i = 0;
        while (targetLength < maxDimension)
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
            
            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return;//prevents infinite loop
            
            targetLength += spacing;
            cur += dir * spacing;
            i++;
        }
    }
    public static void DrawStriped(this Polygon polygon, CurveFloat spacingCurve, float angleDeg, float sideLength, params LineDrawingInfo[] alternatingStriped)
    {
        if (spacingCurve.HasKeys == false) return;
        var center = polygon.GetCentroid();
        polygon.GetFurthestVertex(center, out float disSquared, out int index);
        float maxDimension = MathF.Sqrt(disSquared) * 2;
        
        var dir = ShapeVec.VecFromAngleDeg(angleDeg);
        var rayDir = dir.GetPerpendicularRight();
        if (!spacingCurve.Sample(0f, out float spacing)) return;
        
        if (spacing > maxDimension || spacing <= 0) return;

        var start = center - (dir * maxDimension * 0.5f);
        var cur = start + dir * spacing;
        cur -= rayDir * maxDimension;//offsets the point to outside of the polygon in the opposite direction of the ray
        var targetLength = spacing;

        int i = 0;
        while (targetLength < maxDimension)
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
            
            var time = targetLength / maxDimension;
            if (!spacingCurve.Sample(time, out spacing)) return;
            if (spacing <= 0f) return;//prevents infinite loop
            
            targetLength += spacing;
            cur += dir * spacing;
            i++;
        }
    }
    
    #endregion
}