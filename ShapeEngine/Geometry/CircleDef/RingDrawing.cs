using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

//TODO: Check and cleanup based on CircleDrawing changes

/// <summary>
/// Provides static methods for drawing ring shapes and ring outlines with various customization options.
/// </summary>
/// <remarks>
/// This class contains utility methods for drawing rings, sector rings, and their outlines with support for line thickness, color, rotation, scaling, and side count.
/// </remarks>
public static class RingDrawing
{
    //TODO: Move all remaining functions to circle drawing - delete RingDrawing
    
    #region Draw Ring Lines

    public static void DrawRingLines(this Circle ring, float ringThickness, float outerRotDeg, float innerRotDeg, 
        LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float innerSmoothness, float outerSmoothness)
    {
        if(ringThickness <= 0 || innerLineInfo.Thickness <= 0 || outerLineInfo.Thickness <= 0)
        {
            return;
        }

        if (ringThickness >= ring.Radius)
        {
            ring = ring.SetRadius(ring.Radius + ringThickness);
            ring.DrawLines(outerRotDeg, outerLineInfo, outerSmoothness);
            return;
        }
        
        var innerRing = ring.SetRadius(ring.Radius - ringThickness);
        var outerRing = ring.SetRadius(ring.Radius + ringThickness);
        
        innerRing.DrawLines(innerRotDeg, innerLineInfo, innerSmoothness);
        outerRing.DrawLines(outerRotDeg, outerLineInfo, outerSmoothness);
    }
    
    public static void DrawRingLines(this Circle ring, float ringThickness, float rotDeg, float lineThickness, ColorRgba color, float smoothness)
    {
        if(ringThickness <= 0 && lineThickness <= 0)
        {
            ring.Draw(rotDeg, color, smoothness);
            return;
        }
        
        if (lineThickness <= 0)
        {
            ring.DrawLines(rotDeg, ringThickness, color, smoothness);
            return;
        }
        
        if (ringThickness <= 0)
        {
            ring.DrawLines(rotDeg, lineThickness, color, smoothness);
            return;
        }

        if (ringThickness >= ring.Radius)
        {
            ring = ring.SetRadius(ring.Radius + ringThickness);
            ring.DrawLines(rotDeg, lineThickness, color, smoothness);
            return;
        }
        
        var innerRing = ring.SetRadius(ring.Radius - ringThickness);
        var outerRing = ring.SetRadius(ring.Radius + ringThickness);
        
        innerRing.DrawLines(rotDeg, lineThickness, color, smoothness);
        outerRing.DrawLines(rotDeg, lineThickness, color, smoothness);
    }
    
    #endregion
    
    #region Draw Ring Lines Percentage
    public static void DrawRingLinesPercentage(this Circle ring, float ringThickness, float f, float rotDeg, float lineThickness, ColorRgba color, float smoothness)
    {
        if(ringThickness <= 0 && lineThickness <= 0)
        {
            ring.DrawPercentage(f, rotDeg, color, smoothness);
            return;
        }
        
        if (lineThickness <= 0)
        {
            ring.DrawLinesPercentage(f, rotDeg, ringThickness, color, smoothness);
            return;
        }
        
        if (ringThickness <= 0)
        {
            ring.DrawLinesPercentage(f, rotDeg, lineThickness, color, smoothness);
            return;
        }

        if (ringThickness >= ring.Radius)
        {
            ring = ring.SetRadius(ring.Radius + ringThickness);
            ring.DrawLinesPercentage(f, rotDeg, lineThickness, color, smoothness);
            return;
        }
        
        var innerRing = ring.SetRadius(ring.Radius - ringThickness);
        var outerRing = ring.SetRadius(ring.Radius + ringThickness);
        
        innerRing.DrawLinesPercentage(f, rotDeg, lineThickness, color, smoothness);
        outerRing.DrawLinesPercentage(f, rotDeg, lineThickness, color, smoothness);
    }
    
    public static void DrawRingLinesPercengate(this Circle ring, float ringThickness, float f, float outerRotDeg, float innerRotDeg, 
        LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float innerSmoothness, float outerSmoothness)
    {
        if(ringThickness <= 0 || innerLineInfo.Thickness <= 0 || outerLineInfo.Thickness <= 0)
        {
            return;
        }

        if (ringThickness >= ring.Radius)
        {
            ring = ring.SetRadius(ring.Radius + ringThickness);
            ring.DrawLinesPercentage(f, outerRotDeg, outerLineInfo, outerSmoothness);
            return;
        }
        
        var innerRing = ring.SetRadius(ring.Radius - ringThickness);
        var outerRing = ring.SetRadius(ring.Radius + ringThickness);
        
        innerRing.DrawLinesPercentage(f, innerRotDeg, innerLineInfo, innerSmoothness);
        outerRing.DrawLinesPercentage(f, outerRotDeg, outerLineInfo, outerSmoothness);
    }
    
    #endregion 

    #region Draw Ring Lines Scaled
    
    public static void DrawRingLinesScaled(this Circle ring, float ringThickness, float outerRotDeg, float innerRotDeg, 
        LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float innerSmoothness, float outerSmoothness, 
        float innerSideScaleFactor, float outerSideScaleFactor, float innerSideScaleOrigin, float outerSideScaleOrigin)
    {
        if (innerSideScaleFactor <= 0f || outerSideScaleFactor <= 0f || ringThickness <= 0 || innerLineInfo.Thickness <= 0 || outerLineInfo.Thickness <= 0)
        {
            return;
        }
        
        if (innerSideScaleFactor >= 1f && outerSideScaleFactor >= 1f)
        {
            ring.DrawRingLines(ringThickness, innerRotDeg, outerRotDeg, innerLineInfo, outerLineInfo, innerSmoothness, outerSmoothness);
            return;
        }
        
        if (ringThickness >= ring.Radius)
        {
            ring = ring.SetRadius(ring.Radius + ringThickness);
            ring.DrawLinesScaled(outerRotDeg, outerLineInfo, outerSmoothness, outerSideScaleFactor, outerSideScaleOrigin);
            return;
        }

        if (innerSideScaleOrigin < 1f)
        {
            var innerRing = ring.SetRadius(ring.Radius - ringThickness);
            innerRing.DrawLinesScaled(innerRotDeg, innerLineInfo, innerSmoothness, innerSideScaleFactor, innerSideScaleOrigin);
        }
        
        if (outerSideScaleOrigin < 1f)
        {
            var outerRing = ring.SetRadius(ring.Radius + ringThickness);
            outerRing.DrawLinesScaled(outerRotDeg, outerLineInfo, outerSmoothness, outerSideScaleFactor, outerSideScaleOrigin);
        }
    }
    
    public static void DrawRingLinesScaled(this Circle ring, float ringThickness, float rotDeg, float lineThickness, ColorRgba color, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f)
        {
            return;
        }
        
        if (sideScaleFactor >= 1f)
        {
            ring.DrawRingLines(ringThickness, rotDeg, lineThickness, color, smoothness);
            return;
        }
        
        if(ringThickness <= 0 && lineThickness <= 0)
        {
            ring.DrawScaled(rotDeg, color, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        
        if (lineThickness <= 0)
        {
            ring.DrawLinesScaled(rotDeg, new LineDrawingInfo(lineThickness, color), smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        
        if (ringThickness <= 0)
        {
            ring.DrawLinesScaled(rotDeg, new LineDrawingInfo(lineThickness, color), smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }

        if (ringThickness >= ring.Radius)
        {
            ring = ring.SetRadius(ring.Radius + ringThickness);
            ring.DrawLinesScaled(rotDeg, new LineDrawingInfo(lineThickness, color), smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        
        var innerRing = ring.SetRadius(ring.Radius - ringThickness);
        var outerRing = ring.SetRadius(ring.Radius + ringThickness);
        var lineInfo = new LineDrawingInfo(lineThickness, color);
        innerRing.DrawLinesScaled(rotDeg, lineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
        outerRing.DrawLinesScaled(rotDeg, lineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
    }
    
    #endregion
    
    #region Draw Ring Sector Lines
    public static void DrawRingSectorLines(this Circle ring, float ringThickness, float startAngleDeg, float endAngleDeg, float rotDeg, float lineThickness, ColorRgba color, float smoothness)
    {
        if(ringThickness <= 0 && lineThickness <= 0)
        {
            ring.DrawSector(startAngleDeg, endAngleDeg, rotDeg, color, smoothness);
            return;
        }
        
        if (lineThickness <= 0)
        {
            ring.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, ringThickness, color, smoothness);
            return;
        }
        
        if (ringThickness <= 0)
        {
            ring.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, lineThickness, color, smoothness);
            return;
        }

        if (ringThickness >= ring.Radius)
        {
            ring = ring.SetRadius(ring.Radius + ringThickness);
            ring.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, lineThickness, color, smoothness);
            return;
        }
        
        var innerRing = ring.SetRadius(ring.Radius - ringThickness);
        var outerRing = ring.SetRadius(ring.Radius + ringThickness);
        
        innerRing.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, lineThickness, color, smoothness);
        outerRing.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, lineThickness, color, smoothness);
    }
    
    public static void DrawRingSectorLines(this Circle ring, float ringThickness, float startAngleDeg, float endAngleDeg, float outerRotDeg, float innerRotDeg, 
        LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float innerSmoothness, float outerSmoothness)
    {
        if(ringThickness <= 0 || innerLineInfo.Thickness <= 0 || outerLineInfo.Thickness <= 0)
        {
            return;
        }

        if (ringThickness >= ring.Radius)
        {
            ring = ring.SetRadius(ring.Radius + ringThickness);
            ring.DrawSectorLines(startAngleDeg, endAngleDeg, outerRotDeg, outerLineInfo, outerSmoothness);
            return;
        }
        
        var innerRing = ring.SetRadius(ring.Radius - ringThickness);
        var outerRing = ring.SetRadius(ring.Radius + ringThickness);
        
        innerRing.DrawSectorLines(startAngleDeg, endAngleDeg, innerRotDeg, innerLineInfo, innerSmoothness);
        outerRing.DrawSectorLines(startAngleDeg, endAngleDeg, outerRotDeg, outerLineInfo, outerSmoothness);
    }
    
    #endregion 
    

}