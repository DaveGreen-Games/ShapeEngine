using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

public static class CircleDrawing
{
    #region Draw
    
    /// <summary>
    /// Draws the filled circle.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="color">The color of the circle.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public static void Draw(this Circle c, float rotDeg, ColorRgba color, float smoothness = 0.5f)
    {
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        Raylib.DrawCircleSector(c.Center, c.Radius, rotDeg, 360 + rotDeg, sides, color.ToRayColor());
    }

    /// <summary>
    /// Draws the filled circle.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="color">The color of the circle.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public static void Draw(this Circle c, ColorRgba color, float smoothness = 0.5f)
    {
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        Raylib.DrawCircleSector(c.Center, c.Radius, 0, 360, sides, color.ToRayColor());
    }
    
    /// <summary>
    /// Draws the circle as a square (fastest). Usefull for very small circles to save performance and still look good.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="color">The color of the circle.</param>
    public static void DrawFast(this Circle c, ColorRgba color)
    {
        RectDrawing.DrawRect(c.Center - new Vector2(c.Radius, c.Radius), c.Center + new Vector2(c.Radius, c.Radius), color);
    }

    #endregion
    
    #region Draw Scaled

    /// <summary>
    /// Draws a filled circle with each side scaled towards the origin of the side.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="rotDeg">Rotation of the circle in degrees.</param>
    /// <param name="color">Color of the circle.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="sideScaleFactor">Scale factor for each side (0 = no circle, 1 = normal circle).</param>
    /// <param name="sideScaleOrigin">Point along each segment to scale from (0-1, default 0.5).</param>
    public static void DrawScaled(this Circle c, float rotDeg, ColorRgba color, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (c.Radius <= 0f || sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            c.Draw(color, smoothness);
            return;
        }
        
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out float angleStep, out int sides)) return;
        
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var rayColor = color.ToRayColor();
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * nextIndex);

            var scaledSegment = new Segment(start, end).ScaleSegment(sideScaleFactor, sideScaleOrigin);
            
            Raylib.DrawTriangle(c.Center, scaledSegment.End, scaledSegment.Start, rayColor);
        }
    }
    #endregion
    
    #region Draw Percentage
    
    /// <summary>
    /// Draws a filled circle sector based on a percentage value.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="f">The fill percentage (0 to 1 or 0 to -1). Positive values are clockwise, negative are counter-clockwise.</param>
    /// <param name="rotDeg">The rotation offset in degrees.</param>
    /// <param name="color">The color of the circle sector.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawPercentage(this Circle c, float f, float rotDeg, ColorRgba color, float smoothness = 0.5f)
    {
        if (f == 0) return;

        if (MathF.Abs(f) >= 1f)
        {
            c.Draw(rotDeg, color, smoothness);
            return;
        }
        
        if (TransformPercentageToAngles(f, out float startAngleDeg, out float endAngleDeg))
        {
            c.DrawSector(startAngleDeg + rotDeg, endAngleDeg + rotDeg, color, smoothness);
        }
    }
    #endregion
    
    #region Draw Lines
    
    /// <summary>
    /// Draws the outline of the circle.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public static void DrawLines(this Circle c, float lineThickness, ColorRgba color, float smoothness = 0.5f)
    {
        if (c.Radius < lineThickness)
        {
            var circle = c.SetRadius(lineThickness * 2); 
            circle.Draw(color, smoothness);
            return;
        }
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius + lineThickness, 0f, lineThickness * 2, color.ToRayColor());
    }

    /// <summary>
    /// Draws the outline of the circle.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public static void DrawLines(this Circle c, float rotDeg, float lineThickness, ColorRgba color, float smoothness = 0.5f)
    {
        if (c.Radius < lineThickness)
        {
            var circle = c.SetRadius(lineThickness * 2); 
            circle.Draw(color, smoothness);
            return;
        }
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius + lineThickness, rotDeg, lineThickness * 2, color.ToRayColor());
    }

    /// <summary>
    /// Draws the outline of the circle with detailed line drawing options.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public static void DrawLines(this Circle c, LineDrawingInfo lineInfo, float smoothness = 0.5f)
    {
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2); 
            circle.Draw(lineInfo.Color, smoothness);
            return;
        }
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        var lineThickness = lineInfo.Thickness;
        Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius + lineThickness, 0f, lineThickness * 2, lineInfo.Color.ToRayColor());
    }

    /// <summary>
    /// Draws the outline of the circle with detailed line drawing options.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public static void DrawLines(this Circle c, float rotDeg, LineDrawingInfo lineInfo, float smoothness = 0.5f)
    {
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2); 
            circle.Draw(lineInfo.Color, smoothness);
            return;
        }
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        var lineThickness = lineInfo.Thickness;
        Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius + lineThickness, rotDeg, lineThickness * 2, lineInfo.Color.ToRayColor());
    }
    #endregion
    
    #region Draw Lines Scaled

    /// <summary>
    /// Draws the outline of the circle where each segment is scaled towards the segment center.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="sideScaleFactor">Scale factor for each side segment (0 = invisible, 1 = full connected circle).</param>
    /// <param name="sideScaleOrigin">The point on the segment to scale from (0-1, default 0.5 is center).</param>
    public static void DrawLinesScaled(this Circle c, LineDrawingInfo lineInfo, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        DrawLinesScaled(c, 0f, lineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
    }
    
    /// <summary>
    /// Draws the outline of the circle where each segment is scaled towards the segment center.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="sideScaleFactor">Scale factor for each side segment (0 = invisible, 1 = full connected circle).</param>
    /// <param name="sideScaleOrigin">The point on the segment to scale from (0-1, default 0.5 is center).</param>
    public static void DrawLinesScaled(this Circle c, float rotDeg, LineDrawingInfo lineInfo, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2);
            circle.DrawScaled(rotDeg, lineInfo.Color, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            c.DrawLines(lineInfo, smoothness);
            return;
        }
        
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out float angleStep, out int sides)) return;
        
        var rotRad = rotDeg * ShapeMath.DEGTORAD;

        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * nextIndex);

            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }
    
    #endregion
    
    #region Draw Lines Percentage

    /// <summary>
    /// Draws the outline of a circle sector based on a percentage value.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="f">The fill percentage (0 to 1 or 0 to -1). Positive values are clockwise, negative are counter-clockwise.</param>
    /// <param name="rotDeg">The rotation offset in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawLinesPercentage(this Circle c, float f, float rotDeg, float lineThickness, ColorRgba color,  float smoothness = 0.5f)
    {
        if (f == 0) return;

        if (MathF.Abs(f) >= 1f)
        {
            c.DrawLines(rotDeg, lineThickness, color, smoothness);
            return;
        }
        
        if (TransformPercentageToAngles(f, out float startAngleDeg, out float endAngleDeg))
        {
            c.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, lineThickness, color, smoothness);
        }
    }
    
    /// <summary>
    /// Draws the outline of a circle sector based on a percentage value.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="f">The fill percentage (0 to 1 or 0 to -1). Positive values are clockwise, negative are counter-clockwise.</param>
    /// <param name="rotDeg">The rotation offset in degrees.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawLinesPercentage(this Circle c, float f, float rotDeg, LineDrawingInfo lineInfo, float smoothness = 0.5f)
    {
        // DrawLinesPercentage(c, f, lineInfo.Thickness, rotDeg, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints, smoothness);
        if (f == 0) return;

        if (MathF.Abs(f) >= 1f)
        {
            c.DrawLines(rotDeg, lineInfo, smoothness);
            return;
        }
        
        if (TransformPercentageToAngles(f, out float startAngleDeg, out float endAngleDeg))
        {
            c.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, lineInfo, smoothness);
        }
    }

    #endregion
    
    #region Draw Sector
    /// <summary>
    /// Draws a filled sector of the circle.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="color">The color of the sector.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawSector(this Circle c, float startAngleDeg, float endAngleDeg, ColorRgba color, float smoothness = 0.5f)
    {
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg, smoothness, out float angleDifRad, out float _, out int sides, false)) return;
        Raylib.DrawCircleSector(c.Center, c.Radius, startAngleDeg, startAngleDeg + (angleDifRad * ShapeMath.RADTODEG), sides, color.ToRayColor());
    }
    
    /// <summary>
    /// Draws a filled sector of the circle.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotDeg">The rotation offset in degrees.</param>
    /// <param name="color">The color of the sector.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawSector(this Circle c, float startAngleDeg, float endAngleDeg, float rotDeg, ColorRgba color, float smoothness = 0.5f)
    {
        c.DrawSector(startAngleDeg + rotDeg, endAngleDeg + rotDeg, color, smoothness);
    }
    #endregion
    
    #region Draw Sector Scaled

    /// <summary>
    /// Draws a filled sector of the circle where the outside edge of each (pie slice) segment is scaled towards the point determined by <paramref name="sideScaleOrigin"/>.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotDeg">The rotation offset in degrees.</param>
    /// <param name="color">The color of the sector.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="sideScaleFactor">Scale factor for each side segment (0 = invisible, 1 = full connected circle).</param>
    /// <param name="sideScaleOrigin">The point on the segment to scale from (0-1, default 0.5 is center).</param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawSectorScaled(this Circle c, float startAngleDeg, float endAngleDeg, float rotDeg, ColorRgba color, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (c.Radius <= 0f || sideScaleFactor <= 0f) return;
        
        if (sideScaleFactor >= 1f)
        {
            c.DrawSector(startAngleDeg, endAngleDeg, rotDeg, color, smoothness);
            return;
        }
        
        startAngleDeg = startAngleDeg + rotDeg;
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg + rotDeg, smoothness, out float angleDifRad, out float angleStep, out int sides, false)) return;
        
        var absAngleDifRad = MathF.Abs(angleDifRad);
        if (absAngleDifRad < 0.00001f) return;
        
        if (absAngleDifRad >= MathF.Tau)
        {
            c.DrawScaled(rotDeg, color, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        var rayColor = color.ToRayColor();
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = i + 1;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * nextIndex);

            var scaledSegment = new Segment(start, end).ScaleSegment(sideScaleFactor, sideScaleOrigin);

            if (angleDifRad < 0)
            {
                Raylib.DrawTriangle(scaledSegment.Start, scaledSegment.End,c.Center, rayColor);
            }
            else
            {
                Raylib.DrawTriangle(c.Center, scaledSegment.End, scaledSegment.Start, rayColor);
            }
            
        }
    }
    #endregion
    
    #region Draw Sector Lines
   
    /// <summary>
    /// Draws the outline framework of a circle sector (perimeter lines including the radii).
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawSectorLinesClosed(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float smoothness = 0.5f)
    {
        var color = lineInfo.Color.SetAlpha(255);
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2);
            circle.DrawSector(startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo.Color, smoothness);
            return;
        }
        
        startAngleDeg = startAngleDeg + rotOffsetDeg;
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg + rotOffsetDeg, smoothness, out float angleDifRad, out float _, out int sides, false)) return;
        endAngleDeg = startAngleDeg + angleDifRad * ShapeMath.RADTODEG;
        
        var absAngleDifRad = MathF.Abs(angleDifRad);
        if (absAngleDifRad < 0.00001f) return;
        
        var start = c.Center + (ShapeVec.Right() * c.Radius).Rotate(startAngleDeg * ShapeMath.DEGTORAD);
        SegmentDrawing.DrawSegment(c.Center, start, lineInfo.Thickness, color, LineCapType.CappedExtended, lineInfo.CapPoints);
        
        if (absAngleDifRad >= MathF.Tau)
        {
            c.DrawLines(rotOffsetDeg, lineInfo.Thickness, color, smoothness);
            return;
        }
        
        var end = c.Center + (ShapeVec.Right() * c.Radius).Rotate(endAngleDeg * ShapeMath.DEGTORAD);
        SegmentDrawing.DrawSegment(c.Center, end, lineInfo.Thickness, color, LineCapType.CappedExtended, lineInfo.CapPoints);
        
        Raylib.DrawRing(c.Center, c.Radius - lineInfo.Thickness, c.Radius + lineInfo.Thickness, startAngleDeg, endAngleDeg, sides, color.ToRayColor());
    }
    
    /// <summary>
    /// Draws the outline arc of a circle sector (only the curved part).
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float smoothness = 0.5f)
    {
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2);
            circle.DrawSector(startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo.Color, smoothness);
            return;
        }
        
        startAngleDeg = startAngleDeg + rotOffsetDeg;
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg + rotOffsetDeg, smoothness, out float angleDifRad, out float _, out int sides, false)) return;
        endAngleDeg = startAngleDeg + angleDifRad * ShapeMath.RADTODEG;
        
        var absAngleDifRad = MathF.Abs(angleDifRad);
        if (absAngleDifRad < 0.00001f) return;
        if (absAngleDifRad >= MathF.Tau)
        {
            c.DrawLines(rotOffsetDeg, lineInfo, smoothness);
            return;
        }
        
        var lineCapType = lineInfo.CapType;
        var capPoints = lineInfo.CapPoints;
        var drawCap = (lineCapType is LineCapType.Capped or LineCapType.CappedExtended) && capPoints > 0;

        if (drawCap)
        {
            var startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
            var endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
            
            var p1Inner = c.Center + (ShapeVec.Right() * (c.Radius - lineInfo.Thickness)).Rotate(startAngleRad);
            var p1Outer = c.Center + (ShapeVec.Right() * (c.Radius + lineInfo.Thickness)).Rotate(startAngleRad);
        
            var p2Inner = c.Center + (ShapeVec.Right() * (c.Radius - lineInfo.Thickness)).Rotate(endAngleRad);
            var p2Outer = c.Center + (ShapeVec.Right() * (c.Radius + lineInfo.Thickness)).Rotate(endAngleRad);
            
            if (angleDifRad > 0)
            {
                SegmentDrawing.DrawRoundCap(p2Inner, p2Outer, lineInfo.CapPoints, lineInfo.Color);
                SegmentDrawing.DrawRoundCap(p1Outer, p1Inner, lineInfo.CapPoints, lineInfo.Color);
            }
            else
            {
                SegmentDrawing.DrawRoundCap(p2Outer, p2Inner, lineInfo.CapPoints, lineInfo.Color);
                SegmentDrawing.DrawRoundCap(p1Inner, p1Outer, lineInfo.CapPoints, lineInfo.Color);
            }
        }
        
        Raylib.DrawRing(c.Center, c.Radius - lineInfo.Thickness, c.Radius + lineInfo.Thickness, startAngleDeg, endAngleDeg, sides, lineInfo.Color.ToRayColor());
        
    }
    
    /// <summary>
    /// Draws the outline arc of a circle sector (only the curved part).
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba color, float smoothness = 0.5f)
    {
        if (c.Radius < lineThickness)
        {
            var circle = c.SetRadius(lineThickness * 2);
            circle.DrawSector(startAngleDeg, endAngleDeg, rotOffsetDeg, color, smoothness);
            return;
        }
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, smoothness, out float angleDifRad, out float _, out int sides, false)) return;
        Raylib.DrawRing(c.Center, c.Radius - lineThickness, c.Radius + lineThickness, startAngleDeg + rotOffsetDeg, startAngleDeg + rotOffsetDeg + (angleDifRad * ShapeMath.RADTODEG), sides, color.ToRayColor());
    }
    #endregion
    
    #region Draw Sector Lines Scaled
    
    /// <summary>
    /// Draws the outline of a circle sector where each segment is scaled.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="sideScaleFactor">Scale factor for each side segment (0 = invisible, 1 = full connected circle).</param>
    /// <param name="sideScaleOrigin">The point on the segment to scale from (0-1, default 0.5 is center).</param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawSectorLinesScaledClosed(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2);
            circle.DrawSectorScaled(startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo.Color, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        if (sideScaleFactor <= 0f) return;
        
        if (sideScaleFactor >= 1f)
        {
            DrawSectorLinesClosed(c, startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo, smoothness);
            return;
        }
        startAngleDeg = startAngleDeg + rotOffsetDeg;
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg + rotOffsetDeg, smoothness, out float angleDifRad, out float angleStep, out int sides, false)) return;
        endAngleDeg = startAngleDeg + angleDifRad * ShapeMath.RADTODEG;
        
        var absAngleDifRad = MathF.Abs(angleDifRad);
        if (absAngleDifRad < 0.00001f) return;
        
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        
        var sectorStart = c.Center + (ShapeVec.Right() * c.Radius).Rotate(startAngleRad);
        SegmentDrawing.DrawSegment(c.Center, sectorStart, lineInfo, sideScaleFactor, sideScaleOrigin);
        
        if (absAngleDifRad >= MathF.Tau)
        {
            c.DrawLinesScaled(rotOffsetDeg, lineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        
        var sectorEnd = c.Center + (ShapeVec.Right() * c.Radius).Rotate(endAngleRad);
        SegmentDrawing.DrawSegment(c.Center, sectorEnd, lineInfo, sideScaleFactor, sideScaleOrigin);
        
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = i + 1;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * nextIndex);

            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }
    
    /// <summary>
    /// Draws the outline arc of a circle sector where each segment is scaled (only the curved part).
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="sideScaleFactor">Scale factor for each side segment (0 = invisible, 1 = full connected circle).</param>
    /// <param name="sideScaleOrigin">The point on the segment to scale from (0-1, default 0.5 is center).</param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawSectorLinesScaled(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2);
            circle.DrawSectorScaled(startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo.Color, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        if (sideScaleFactor <= 0f) return;
        
        if (sideScaleFactor >= 1f)
        {
            c.DrawSectorLines(startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo, smoothness);
            return;
        }
        
        startAngleDeg = startAngleDeg + rotOffsetDeg;
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg + rotOffsetDeg, smoothness, out float angleDifRad, out float angleStep, out int sides, false)) return;
        
        var absAngleDifRad = MathF.Abs(angleDifRad);
        if (absAngleDifRad < 0.00001f) return;
        
        if (absAngleDifRad >= MathF.Tau)
        {
            c.DrawLinesScaled(rotOffsetDeg, lineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = i + 1;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * nextIndex);

            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }
    #endregion
    
    #region Draw Masked
    /// <summary>
    /// Draws the circle outline as individual line segments clipped by a <see cref="Triangle"/> mask.
    /// </summary>
    /// <param name="circle">The source <see cref="Circle"/> whose circumference will be approximated by segments.</param>
    /// <param name="mask">The <see cref="Triangle"/> used to mask (clip) each generated segment.</param>
    /// <param name="lineInfo">Parameters controlling line drawing (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">Rotation offset in degrees applied to the entire approximated polygon.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Circle circle, Triangle mask, LineDrawingInfo lineInfo, float rotDeg, float smoothness, bool reversedMask = false)
    {
        if (!CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var radius = circle.Radius;
        var center = circle.Center;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            var segment = new Segment(curP, nextP);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the circle outline as individual line segments clipped by a <see cref="Circle"/> mask.
    /// </summary>
    /// <param name="circle">The source <see cref="Circle"/> whose circumference will be approximated by segments.</param>
    /// <param name="mask">The <see cref="Circle"/> used to mask (clip) each generated segment.</param>
    /// <param name="lineInfo">Parameters controlling line drawing (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">Rotation offset in degrees applied to the entire approximated polygon.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Circle circle, Circle mask, LineDrawingInfo lineInfo, float rotDeg, float smoothness, bool reversedMask = false)
    {
        if (!CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var radius = circle.Radius;
        var center = circle.Center;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            var segment = new Segment(curP, nextP);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the circle outline as individual line segments clipped by a <see cref="Rect"/> mask.
    /// </summary>
    /// <param name="circle">The source <see cref="Circle"/> whose circumference will be approximated by segments.</param>
    /// <param name="mask">The <see cref="Rect"/> used to mask (clip) each generated segment.</param>
    /// <param name="lineInfo">Parameters controlling line drawing (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">Rotation offset in degrees applied to the entire approximated polygon.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Circle circle, Rect mask, LineDrawingInfo lineInfo, float rotDeg, float smoothness, bool reversedMask = false)
    {
        if (!CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var radius = circle.Radius;
        var center = circle.Center;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            var segment = new Segment(curP, nextP);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the circle outline as individual line segments clipped by a <see cref="Quad"/> mask.
    /// </summary>
    /// <param name="circle">The source <see cref="Circle"/> whose circumference will be approximated by segments.</param>
    /// <param name="mask">The <see cref="Quad"/> used to mask (clip) each generated segment.</param>
    /// <param name="lineInfo">Parameters controlling line drawing (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">Rotation offset in degrees applied to the entire approximated polygon.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Circle circle, Quad mask, LineDrawingInfo lineInfo, float rotDeg, float smoothness, bool reversedMask = false)
    {
        if (!CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var radius = circle.Radius;
        var center = circle.Center;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            var segment = new Segment(curP, nextP);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the circle outline as individual line segments clipped by a <see cref="Polygon"/> mask.
    /// </summary>
    /// <param name="circle">The source <see cref="Circle"/> whose circumference will be approximated by segments.</param>
    /// <param name="mask">The <see cref="Polygon"/> used to mask (clip) each generated segment.</param>
    /// <param name="lineInfo">Parameters controlling line drawing (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">Rotation offset in degrees applied to the entire approximated polygon.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Circle circle, Polygon mask, LineDrawingInfo lineInfo, float rotDeg, float smoothness, bool reversedMask = false)
    {
        if (!CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var radius = circle.Radius;
        var center = circle.Center;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            var segment = new Segment(curP, nextP);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the circle outline as individual line segments clipped by a mask of any closed shape type.
    /// </summary>
    /// <typeparam name="T">The mask type implementing <see cref="IClosedShapeTypeProvider"/> (for example: <see cref="Triangle"/>, <see cref="Circle"/>, <see cref="Rect"/>, <see cref="Quad"/>, <see cref="Polygon"/>).</typeparam>
    /// <param name="circle">The source <see cref="Circle"/> whose circumference will be approximated by segments.</param>
    /// <param name="mask">The mask used to clip each generated segment.</param>
    /// <param name="lineInfo">Parameters controlling line drawing (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">Rotation offset in degrees applied to the entire approximated polygon.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked<T>(this Circle circle, T mask, LineDrawingInfo lineInfo, float rotDeg, float smoothness, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        if (!CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var radius = circle.Radius;
        var center = circle.Center;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            var segment = new Segment(curP, nextP);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    #endregion
    
    #region Draw Checkered
    /// <summary>
    /// Draws a checkered pattern of lines inside a circle, optionally with a background color.
    /// </summary>
    /// <param name="pos">The position of the circle (before alignment).</param>
    /// <param name="alignment">The anchor point alignment for the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="spacing">The spacing between checkered lines.</param>
    /// <param name="lineThickness">The thickness of the checkered lines.</param>
    /// <param name="angleDeg">The rotation of the checkered pattern in degrees.</param>
    /// <param name="lineColorRgba">The color of the checkered lines.</param>
    /// <param name="bgColorRgba">The background color of the circle (drawn first if alpha &gt; 0).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// Useful for visualizing grid or checkered overlays on circular shapes.
    /// </remarks>
    public static void DrawCircleCheckeredLines(Vector2 pos, AnchorPoint alignment, float radius, float spacing,
        float lineThickness, float angleDeg, ColorRgba lineColorRgba, ColorRgba bgColorRgba, float smoothness = 0.5f)
    {

        float maxDimension = radius;
        var size = new Vector2(radius, radius) * 2f;
        var aVector = alignment.ToVector2() * size;
        var center = pos - aVector + size / 2;
        float rotRad = angleDeg * ShapeMath.DEGTORAD;

        if (bgColorRgba.A > 0)
        {
            var circle = new Circle(center, radius);
            circle.Draw(bgColorRgba, smoothness);
        }

        var cur = new Vector2(-spacing / 2, 0f);
        while (cur.X > -maxDimension)
        {
            var p = center + cur.Rotate(rotRad);

            //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
            float angle = MathF.Acos(cur.X / radius);
            float y = radius * MathF.Sin(angle);

            var up = new Vector2(0f, -y);
            var down = new Vector2(0f, y);
            var start = p + up.Rotate(rotRad);
            var end = p + down.Rotate(rotRad);
            SegmentDrawing.DrawSegment(start, end, lineThickness, lineColorRgba);
            cur.X -= spacing;
        }

        cur = new(spacing / 2, 0f);
        while (cur.X < maxDimension)
        {
            var p = center + cur.Rotate(rotRad);
            //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
            float angle = MathF.Acos(cur.X / radius);
            float y = radius * MathF.Sin(angle);

            var up = new Vector2(0f, -y);
            var down = new Vector2(0f, y);
            var start = p + up.Rotate(rotRad);
            var end = p + down.Rotate(rotRad);
            SegmentDrawing.DrawSegment(start, end, lineThickness, lineColorRgba);
            cur.X += spacing; 
        }
    }
    #endregion
    
    #region Draw Ring Lines

    /// <summary>
    /// Draws a ring (annulus) outline.
    /// </summary>
    /// <param name="ring">The circle defining the center and outer radius of the ring.</param>
    /// <param name="ringThickness">The thickness of the ring (inwards from the circle radius).</param>
    /// <param name="rotDeg">The rotation of the ring in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline lines.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
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

        // 1. Calculate the effective radius of the inner ring, applying the clamp constraint.
        // The inner ring represents the centerline of the inner stroke.
        float effectiveInnerRadius = MathF.Max(ring.Radius - ringThickness, lineThickness * 2f);
        
        // 2. Calculate the outer edge of the inner stroke.
        // Stroke is centered on effectiveInnerRadius with total width (lineThickness * 2).
        // It extends outwards by lineThickness.
        float innerStrokeOuterEdge = effectiveInnerRadius + lineThickness;

        // 3. Calculate the inner edge of the outer stroke.
        // Outer ring is at Radius + ringThickness.
        // Stroke extends inwards by lineThickness.
        float outerStrokeInnerEdge = (ring.Radius + ringThickness) - lineThickness;

        if (innerStrokeOuterEdge >= outerStrokeInnerEdge)
        {
            // Calculating the total bounds of the merged shape
            float totalInnerRadius = effectiveInnerRadius - lineThickness;
            float totalOuterRadius = (ring.Radius + ringThickness) + lineThickness;

            // Calculate the new center radius and thickness for the merged ring
            float totalThickness = (totalOuterRadius - totalInnerRadius) * 0.5f;
            float newRadius = totalInnerRadius + totalThickness;

            // Draw the merged result using the calculated geometric center and thickness
            var mergedRing = ring.SetRadius(newRadius);
            mergedRing.DrawLines(rotDeg, lineThickness, color, smoothness);
            return;
        }
        
        var innerRing = ring.SetRadius(effectiveInnerRadius);
        var outerRing = ring.SetRadius(ring.Radius + ringThickness);
        
        innerRing.DrawLines(rotDeg, lineThickness, color, smoothness);
        outerRing.DrawLines(rotDeg, lineThickness, color, smoothness);
    }

    /// <summary>
    /// Draws a ring (annulus) outline using line drawing info.
    /// </summary>
    /// <param name="ring">The circle defining the center and outer radius of the ring.</param>
    /// <param name="ringThickness">The thickness of the ring (inwards from the circle radius).</param>
    /// <param name="rotDeg">The rotation of the ring in degrees.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public static void DrawRingLines(this Circle ring, float ringThickness, float rotDeg, LineDrawingInfo lineInfo, float smoothness)
    {
        ring.DrawRingLines(ringThickness, rotDeg, lineInfo.Thickness, lineInfo.Color, smoothness);
    }
    
    #endregion
    
    #region Draw Ring Lines Scaled
    
    /// <summary>
    /// Draws a ring (annulus) outline where each segment is scaled.
    /// </summary>
    /// <param name="ring">The circle defining the center and outer radius of the ring.</param>
    /// <param name="ringThickness">The thickness of the ring (inwards from the circle radius).</param>
    /// <param name="rotDeg">The rotation of the ring in degrees.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="sideScaleFactor">Scale factor for each side segment (0 = invisible, 1 = full connected circle).</param>
    /// <param name="sideScaleOrigin">The point on the segment to scale from (0-1, default 0.5 is center).</param>
    public static void DrawRingLinesScaled(this Circle ring, float ringThickness, float rotDeg, LineDrawingInfo lineInfo, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f)
        {
            return;
        }
        
        var lineThickness = lineInfo.Thickness;
        var color = lineInfo.Color;
        
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
            var newLineInfo = lineInfo.SetThickness(ringThickness);
            ring.DrawLinesScaled(rotDeg, newLineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        
        if (ringThickness <= 0)
        {
            ring.DrawLinesScaled(rotDeg, lineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }

        
        // 1. Calculate the effective radius of the inner ring, applying the clamp constraint.
        // The inner ring represents the centerline of the inner stroke.
        float effectiveInnerRadius = MathF.Max(ring.Radius - ringThickness, lineThickness * 2f);
        
        // 2. Calculate the outer edge of the inner stroke.
        // Stroke is centered on effectiveInnerRadius with total width (lineThickness * 2).
        // It extends outwards by lineThickness.
        float innerStrokeOuterEdge = effectiveInnerRadius + lineThickness;

        // 3. Calculate the inner edge of the outer stroke.
        // Outer ring is at Radius + ringThickness.
        // Stroke extends inwards by lineThickness.
        float outerStrokeInnerEdge = (ring.Radius + ringThickness) - lineThickness;

        if (innerStrokeOuterEdge >= outerStrokeInnerEdge)
        {
            // Calculating the total bounds of the merged shape
            float totalInnerRadius = effectiveInnerRadius - lineThickness;
            float totalOuterRadius = (ring.Radius + ringThickness) + lineThickness;

            // Calculate the new center radius and thickness for the merged ring
            float totalThickness = (totalOuterRadius - totalInnerRadius) * 0.5f;
            float newRadius = totalInnerRadius + totalThickness;

            // Draw the merged result using the calculated geometric center and thickness
            var mergedRing = ring.SetRadius(newRadius);
            var newLineInfo = lineInfo.SetThickness(totalThickness);
            mergedRing.DrawLinesScaled(rotDeg, newLineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        
        var innerRing = ring.SetRadius(effectiveInnerRadius);
        var outerRing = ring.SetRadius(ring.Radius + ringThickness);

        innerRing.DrawLinesScaled(rotDeg, lineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
        outerRing.DrawLinesScaled(rotDeg, lineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
    }
    
    #endregion
    
    #region Draw Ring Lines Percentage
    
    /// <summary>
    /// Draws a ring (annulus) sector outline based on a percentage value.
    /// </summary>
    /// <param name="ring">The circle defining the center and outer radius of the ring.</param>
    /// <param name="ringThickness">The thickness of the ring (inwards from the circle radius).</param>
    /// <param name="f">The fill percentage (0 to 1 or 0 to -1). Positive values are clockwise, negative are counter-clockwise.</param>
    /// <param name="rotDeg">The rotation offset in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="closed">If true, draws the start and end caps of the sector.</param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawRingLinesPercentage(this Circle ring, float ringThickness, float f, float rotDeg, float lineThickness, ColorRgba color, float smoothness, bool closed = true)
    {
        if (!TransformPercentageToAngles(f, out var startAngleDeg, out var endAngleDeg)) return;
        ring.DrawRingSectorLines(ringThickness, startAngleDeg, endAngleDeg, rotDeg, lineThickness, color, smoothness, closed);
        
    }
    
    /// <summary>
    /// Draws a ring (annulus) sector outline based on a percentage value.
    /// </summary>
    /// <param name="ring">The circle defining the center and outer radius of the ring.</param>
    /// <param name="ringThickness">The thickness of the ring (inwards from the circle radius).</param>
    /// <param name="f">The fill percentage (0 to 1 or 0 to -1). Positive values are clockwise, negative are counter-clockwise.</param>
    /// <param name="rotDeg">The rotation offset in degrees.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawRingLinesPercentage(this Circle ring, float ringThickness, float f, float rotDeg, LineDrawingInfo lineInfo, float smoothness)
    {
        if (!TransformPercentageToAngles(f, out var startAngleDeg, out var endAngleDeg)) return;
        ring.DrawRingSectorLines(ringThickness, startAngleDeg, endAngleDeg, rotDeg, lineInfo, smoothness);
      
    }
    
    #endregion 
    
    #region Draw Ring Sector Lines
   
    /// <summary>
    /// Draws a ring (annulus) sector outline.
    /// </summary>
    /// <param name="ring">The circle defining the center and outer radius of the ring.</param>
    /// <param name="ringThickness">The thickness of the ring (inwards from the circle radius).</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotDeg">The rotation offset in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="closed">If true, draws the start and end caps of the sector.</param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawRingSectorLines(this Circle ring, float ringThickness, float startAngleDeg, float endAngleDeg, float rotDeg, float lineThickness, ColorRgba color, float smoothness, bool closed = true)
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
        
        // 1. Calculate the effective radius of the inner ring, applying the clamp constraint.
        // The inner ring represents the centerline of the inner stroke.
        float effectiveInnerRadius = MathF.Max(ring.Radius - ringThickness, lineThickness * 2f);
        
        // 2. Calculate the outer edge of the inner stroke.
        // Stroke is centered on effectiveInnerRadius with total width (lineThickness * 2).
        // It extends outwards by lineThickness.
        float innerStrokeOuterEdge = effectiveInnerRadius + lineThickness;

        // 3. Calculate the inner edge of the outer stroke.
        // Outer ring is at Radius + ringThickness.
        // Stroke extends inwards by lineThickness.
        float outerStrokeInnerEdge = (ring.Radius + ringThickness) - lineThickness;

        if (innerStrokeOuterEdge >= outerStrokeInnerEdge)
        {
            // Calculating the total bounds of the merged shape
            float totalInnerRadius = effectiveInnerRadius - lineThickness;
            float totalOuterRadius = (ring.Radius + ringThickness) + lineThickness;

            // Calculate the new center radius and thickness for the merged ring
            float totalThickness = (totalOuterRadius - totalInnerRadius) * 0.5f;
            float newRadius = totalInnerRadius + totalThickness;

            // Draw the merged result using the calculated geometric center and thickness
            var mergedRing = ring.SetRadius(newRadius);
            mergedRing.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, totalThickness, color, smoothness);
            return;
        }
        
        var innerRing = ring.SetRadius(effectiveInnerRadius);
        var outerRing = ring.SetRadius(ring.Radius + ringThickness);

        var innerParametersValid = CalculateCircleDrawingParameters(innerRing.Radius, startAngleDeg + rotDeg, endAngleDeg + rotDeg, smoothness,
            out float innerAngleDifRad, out float _, out int innerSides, false);
        if (!innerParametersValid) return;
        
        var outerParametersValid = CalculateCircleDrawingParameters(outerRing.Radius, startAngleDeg + rotDeg, endAngleDeg + rotDeg, smoothness,
            out float outerAngleDifRad, out float _, out int outerSides, false);
        if (!outerParametersValid) return;
        
        var innerStartAngleDeg = startAngleDeg + rotDeg;
        var outerStartAngleDeg = startAngleDeg + rotDeg;
        var innerEndAngleDeg = innerStartAngleDeg + (innerAngleDifRad * ShapeMath.RADTODEG);
        var outerEndAngleDeg = outerStartAngleDeg + (outerAngleDifRad * ShapeMath.RADTODEG);
        var rayColor = color.ToRayColor();
        
        if (closed)
        {
            var innerStartAngleRad = innerStartAngleDeg * ShapeMath.DEGTORAD;
            var outerStartAngleRad = outerStartAngleDeg * ShapeMath.DEGTORAD;
            var innerEndAngleRad = innerEndAngleDeg * ShapeMath.DEGTORAD;
            var outerEndAngleRad = outerEndAngleDeg * ShapeMath.DEGTORAD;

            var ccw = innerAngleDifRad < 0;
            var innerStartPoint = innerRing.Center + new Vector2(innerRing.Radius - lineThickness, 0f).Rotate(innerStartAngleRad);
            var outerStartPoint = outerRing.Center + new Vector2(outerRing.Radius + lineThickness, 0f).Rotate(outerStartAngleRad);
            var startDir = (outerStartPoint - innerStartPoint).Normalize();
            var startPerp = ccw ? startDir.GetPerpendicularRight() : startDir.GetPerpendicularLeft();
            var innerStartOffsetPoint = innerStartPoint + startPerp * lineThickness * 2;
            var outerStartOffsetPoint = outerStartPoint + startPerp * lineThickness * 2;
            
            var innerEndPoint = innerRing.Center + new Vector2(innerRing.Radius - lineThickness, 0f).Rotate(innerEndAngleRad);
            var outerEndPoint = outerRing.Center + new Vector2(outerRing.Radius + lineThickness, 0f).Rotate(outerEndAngleRad);
            var endDir = (outerEndPoint - innerEndPoint).Normalize();
            var endPerp = ccw ? endDir.GetPerpendicularLeft() : endDir.GetPerpendicularRight();
            var innerEndOffsetPoint = innerEndPoint + endPerp * lineThickness * 2;
            var outerEndOffsetPoint = outerEndPoint + endPerp * lineThickness * 2;
            
            if (ccw)
            {
                Raylib.DrawTriangle(outerStartOffsetPoint, innerStartPoint, innerStartOffsetPoint, rayColor);
                Raylib.DrawTriangle(outerStartOffsetPoint, outerStartPoint, innerStartPoint, rayColor);
            
                Raylib.DrawTriangle(outerEndPoint, innerEndOffsetPoint, innerEndPoint, rayColor);
                Raylib.DrawTriangle(outerEndPoint, outerEndOffsetPoint, innerEndOffsetPoint, rayColor);
            }
            else
            {
                Raylib.DrawTriangle(innerStartOffsetPoint, innerStartPoint, outerStartPoint, rayColor);
                Raylib.DrawTriangle(innerStartOffsetPoint, outerStartPoint, outerStartOffsetPoint, rayColor);
            
                Raylib.DrawTriangle(innerEndPoint, innerEndOffsetPoint, outerEndOffsetPoint, rayColor);
                Raylib.DrawTriangle(innerEndPoint, outerEndOffsetPoint, outerEndPoint, rayColor);
            }
        }
        
        Raylib.DrawRing(innerRing.Center, innerRing.Radius - lineThickness, innerRing.Radius + lineThickness, innerStartAngleDeg, innerEndAngleDeg, innerSides, rayColor);
        Raylib.DrawRing(outerRing.Center, outerRing.Radius - lineThickness, outerRing.Radius + lineThickness, outerStartAngleDeg, outerEndAngleDeg, outerSides, rayColor);
    }
    
    /// <summary>
    /// Draws a ring (annulus) sector outline.
    /// </summary>
    /// <param name="ring">The circle defining the center and outer radius of the ring.</param>
    /// <param name="ringThickness">The thickness of the ring (inwards from the circle radius).</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotDeg">The rotation offset in degrees.</param>
    /// <param name="lineInfo">Contains line drawing parameters (thickness, color, caps, etc.).</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// This function does not clamp the resulting side count with <see cref="CircleSideCountRange"/>.
    /// </remarks>
    public static void DrawRingSectorLines(this Circle ring, float ringThickness, float startAngleDeg, float endAngleDeg, float rotDeg, LineDrawingInfo lineInfo, float smoothness)
    {
        var lineThickness = lineInfo.Thickness;
        var color = lineInfo.Color;
        if(ringThickness <= 0 && lineThickness <= 0)
        {
            ring.DrawSector(startAngleDeg, endAngleDeg, rotDeg, color, smoothness);
            return;
        }
        
        if (lineThickness <= 0)
        {
            var newLineInfo = lineInfo.SetThickness(ringThickness);
            ring.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, newLineInfo, smoothness);
            return;
        }
        
        if (ringThickness <= 0)
        {
            ring.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, lineInfo, smoothness);
            return;
        }
        
        // 1. Calculate the effective radius of the inner ring, applying the clamp constraint.
        // The inner ring represents the centerline of the inner stroke.
        float effectiveInnerRadius = MathF.Max(ring.Radius - ringThickness, lineThickness * 2f);
        
        // 2. Calculate the outer edge of the inner stroke.
        // Stroke is centered on effectiveInnerRadius with total width (lineThickness * 2).
        // It extends outwards by lineThickness.
        float innerStrokeOuterEdge = effectiveInnerRadius + lineThickness;

        // 3. Calculate the inner edge of the outer stroke.
        // Outer ring is at Radius + ringThickness.
        // Stroke extends inwards by lineThickness.
        float outerStrokeInnerEdge = (ring.Radius + ringThickness) - lineThickness;

        if (innerStrokeOuterEdge >= outerStrokeInnerEdge)
        {
            // Calculating the total bounds of the merged shape
            float totalInnerRadius = effectiveInnerRadius - lineThickness;
            float totalOuterRadius = (ring.Radius + ringThickness) + lineThickness;

            // Calculate the new center radius and thickness for the merged ring
            float totalThickness = (totalOuterRadius - totalInnerRadius) * 0.5f;
            float newRadius = totalInnerRadius + totalThickness;

            // Draw the merged result using the calculated geometric center and thickness
            var mergedRing = ring.SetRadius(newRadius);
            var newLineInfo = lineInfo.SetThickness(totalThickness);
            mergedRing.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, newLineInfo, smoothness);
            return;
        }
        
        var innerRing = ring.SetRadius(effectiveInnerRadius);
        var outerRing = ring.SetRadius(ring.Radius + ringThickness);

        innerRing.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, lineInfo, smoothness);
        outerRing.DrawSectorLines(startAngleDeg, endAngleDeg, rotDeg, lineInfo, smoothness);
    }
    
    #endregion 
    
    #region Gapped
    /// <summary>
    /// Draws a gapped outline for a circle, creating a dashed or segmented circular outline.
    /// </summary>
    /// <param name="circle">The circle to draw.</param>
    /// <param name="lineInfo">Parameters describing how to draw the outline.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleDrawing.CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// - If <paramref name="gapDrawingInfo.Gaps"/> is 0 or <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 0, the outline is drawn solid.
    /// - If <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 1 or greater, no outline is drawn.
    /// </remarks>
    public static void DrawGappedOutline(this Circle circle, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo, float rotDeg, float smoothness)
    {
        if (!CircleDrawing.CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            circle.DrawLines(rotDeg, lineInfo, smoothness);
            return;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;
        
        float angleRad = rotDeg * ShapeMath.DEGTORAD;
        Vector2[] circlePoints = new Vector2[sides];
        
        float circumference = 0f;
        for (int i = 0; i < sides; i++)
        {
            var curP = circle.GetVertex(angleRad, angleStep, i);
            circlePoints[i] = curP;
            var nextP = circle.GetVertex(angleRad, angleStep, (i + 1) % sides); 
            circumference += (nextP - curP).Length();
        }

        var startDistance = circumference * gapDrawingInfo.StartOffset;
        var curDistance = 0f;
        var nextDistance = startDistance;
        
        var curIndex = 0;
        var curPoint = circlePoints[0];
        var nextPoint= circlePoints[1];
        var curW = nextPoint - curPoint;
        var curDis = curW.Length();
        
        var points = new List<Vector2>(3);

        int whileCounter = gapDrawingInfo.Gaps;
        
        while (whileCounter > 0)
        {
            if (curDistance + curDis >= nextDistance)
            {
                var p = curPoint + (curW / curDis) * (nextDistance - curDistance);
                
                
                if (points.Count == 0)
                {
                    nextDistance += nonGapPercentageRange * circumference;
                    points.Add(p);

                }
                else
                {
                    nextDistance += gapPercentageRange * circumference;
                    points.Add(p);
                    if (points.Count == 2)
                    {
                        SegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            SegmentDrawing.DrawSegment(p1, p2, lineInfo);
                        }
                    }
                    
                    points.Clear();
                    whileCounter--;
                }

            }
            else
            {
                
                if(points.Count > 0) points.Add(nextPoint);
                
                curDistance += curDis;
                curIndex = (curIndex + 1) % sides;
                curPoint = circlePoints[curIndex];
                nextPoint = circlePoints[(curIndex + 1) % sides];
                curW = nextPoint - curPoint;
                curDis = curW.Length();
            }
            
        }
    }
   
    /// <summary>
    /// Draws a gapped outline for a ring (annulus), creating dashed or segmented outlines for both inner and outer circles.
    /// </summary>
    /// <param name="center">The center of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle. If zero or negative, only the outer circle is drawn.</param>
    /// <param name="outerRadius">The radius of the outer circle. If zero or negative, only the inner circle is drawn.</param>
    /// <param name="lineInfo">Parameters describing how to draw the outlines.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <param name="rotDeg">The rotation of the ring in degrees.</param>
    /// <param name="sideLength">The approximate length of each side used to approximate the circles.</param>
    /// <remarks>
    /// - If both radii are zero or negative, nothing is drawn.
    /// - The number of sides for each circle is determined by the radius and <paramref name="sideLength"/>.
    /// </remarks>
    public static void DrawGappedRing(Vector2 center, float innerRadius, float outerRadius, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo, float rotDeg, float sideLength = 8f)
    {
        if (innerRadius <= 0 && outerRadius <= 0) return;
        
        
        int outerSides = CircleDrawing.GetCircleSideCount(outerRadius, sideLength);
        if (innerRadius <= 0)
        {
            DrawGappedOutline(new Circle(center, outerRadius), lineInfo, gapDrawingInfo, rotDeg, outerSides);
            return;
        }

        int innerSides = CircleDrawing.GetCircleSideCount(innerRadius, sideLength);
        if (outerRadius <= 0)
        {
            DrawGappedOutline(new Circle(center, innerRadius), lineInfo, gapDrawingInfo, rotDeg, innerSides);
            return;
        }
        
        DrawGappedOutline(new Circle(center, innerRadius), lineInfo, gapDrawingInfo, rotDeg, innerSides);
        DrawGappedOutline(new Circle(center, outerRadius), lineInfo, gapDrawingInfo, rotDeg, outerSides);
    }
    
    #endregion
    
    #region Math
    /// <summary>
    /// Defines the minimum and maximum allowed side lengths for circle approximation.
    /// Used to control the smoothness and performance of circle drawing by limiting the number of polygon sides.
    /// </summary>
    /// <remarks>
    /// The smaller a side length is the more sides are used to approximate the circle, resulting in a smoother appearance but potentially worse performance.
    /// The smoothness value in the CircleDrawing methods is used to inversly interpolate between the minimum and maximum side lengths. (Lerp(min, max, 1 - smoothness))
    /// </remarks>
    /// <example>
    /// A smoothness of 0 will use the maximum side length (fewer sides, less smooth), while a smoothness of 1 will use the minimum side length (more sides, smoother).
    /// </example>
    public static ValueRange CircleSideLengthRange = new ValueRange(2f, 75f);
    
    /// <summary>
    /// Defines the minimum and maximum allowed number of sides for circle approximation.
    /// Used to control the smoothness and performance of circle drawing by limiting the number of polygon sides.
    /// </summary>
    /// <remarks>
    /// This is the most useful for constraining the minimum number of sides to a value that still resembles a circle (for example, 3 or 4 sides would look like a triangle or square, not a circle).
    /// Any function that uses an arc length (Sector / Percentage variations) do not clamp the side count.
    /// </remarks>
    public static ValueRangeInt CircleSideCountRange = new ValueRangeInt(8, 128);

    /// <summary>
    /// Calculates the smoothness value required to match a given number of sides for a circle.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="sides">The target number of sides.</param>
    /// <param name="smoothness">Output smoothness value (0-1).</param>
    /// <returns>True if calculation was successful, false if radius is invalid.</returns>
    public static bool GetCircleSmoothness(float radius, int sides, out float smoothness)
    {
        smoothness = 0f;
        if (radius <= 0f) return false;
        sides = CircleSideCountRange.Clamp(sides);
        float circumference = 2.0f * ShapeMath.PI * radius;
        float sideLength = circumference / sides;
        float f = CircleSideLengthRange.Inverse(sideLength);
        smoothness = ShapeMath.Clamp(f, 0f, 1f);
        return true;
    }
    
    /// <summary>
    /// Calculates the smoothness value required to match a given number of sides for a circle arc.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle of the arc in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the arc in degrees.</param>
    /// <param name="sides">The target number of sides.</param>
    /// <param name="smoothness">Output smoothness value (0-1).</param>
    /// <returns>True if calculation was successful, false if inputs are invalid.</returns>
    public static bool GetCircleSmoothness(float radius, float startAngleDeg, float endAngleDeg, int sides, out float smoothness)
    {
        smoothness = 0f;
        if (radius <= 0f) return false;
        
        // Normalize angle difference to [-360, 360], preserving sign
        float angleDiffDeg = endAngleDeg - startAngleDeg;
        if (angleDiffDeg > 360f) angleDiffDeg %= 360f;
        if (angleDiffDeg < -360f) angleDiffDeg %= 360f;
        float absAngleDiffDeg = MathF.Abs(angleDiffDeg);
        if (absAngleDiffDeg < 0.00001f) return false;
        
        sides = CircleSideCountRange.Clamp(sides);
        float arcLength = 2f * ShapeMath.PI * radius * (absAngleDiffDeg / 360f);
        float sideLength = arcLength / sides;
        float f = CircleSideLengthRange.Inverse(sideLength);
        smoothness = ShapeMath.Clamp(f, 0f, 1f);
        return true;
    }
    
    /// <summary>
    /// Calculates drawing parameters for a circle sector (arc) based on smoothness.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle in degrees.</param>
    /// <param name="endAngleDeg">The ending angle in degrees.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="angleDifRad">Output total angular difference in radians.</param>
    /// <param name="angleStepRad">Output angle step per side in radians.</param>
    /// <param name="sides">Output number of sides.</param>
    /// <param name="clampSides">If true, clamps the number of sides to a min/max range set in <see cref="CircleSideCountRange"/>.</param>
    /// <returns>True if parameters were calculated successfully.</returns>
    public static bool CalculateCircleDrawingParameters(float radius, float startAngleDeg, float endAngleDeg, float smoothness, out float angleDifRad, out float angleStepRad, out int sides, bool clampSides = true)
    {
        angleStepRad = 0f;
        sides = 0;
        angleDifRad = 0f;
        if (radius <= 0f) return false;
        
        // Normalize angle difference to [-360, 360], preserving sign
        float angleDiffDeg = endAngleDeg - startAngleDeg;
        if (angleDiffDeg > 360f) angleDiffDeg %= 360f;
        if (angleDiffDeg < -360f) angleDiffDeg %= 360f;

        float absAngleDiffDeg = MathF.Abs(angleDiffDeg);
        if (absAngleDiffDeg < 0.00001f) return false;

        angleDifRad = angleDiffDeg * ShapeMath.DEGTORAD;
        
        smoothness = ShapeMath.Clamp(smoothness, 0f, 1f);
        float sideLength = CircleSideLengthRange.LerpInverse(smoothness);
        if(sideLength <= 0f) return false;

        // Arc length for the given angle
        float arcLength = 2f * ShapeMath.PI * radius * (absAngleDiffDeg / 360f);

        sides = (int)MathF.Ceiling(arcLength / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);

        // Angle step in radians (preserve direction)
        angleStepRad = angleDifRad / sides;

        return true;
    }
    
    /// <summary>
    /// Calculates drawing parameters for a circle sector (arc) based on smoothness.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle in degrees.</param>
    /// <param name="endAngleDeg">The ending angle in degrees.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="angleStepRad">Output angle step per side in radians.</param>
    /// <param name="sides">Output number of sides.</param>
    /// <param name="clampSides">If true, clamps the number of sides to a min/max range set in <see cref="CircleSideCountRange"/>.</param>
    /// <returns>True if parameters were calculated successfully.</returns>
    public static bool CalculateCircleDrawingParameters(float radius, float startAngleDeg, float endAngleDeg, float smoothness, out float angleStepRad, out int sides, bool clampSides = true)
    {
        angleStepRad = 0f;
        sides = 0;
        if (radius <= 0f) return false;
        
        // Normalize angle difference to [-360, 360], preserving sign
        float angleDiffDeg = endAngleDeg - startAngleDeg;
        if (angleDiffDeg > 360f) angleDiffDeg %= 360f;
        if (angleDiffDeg < -360f) angleDiffDeg %= 360f;

        float absAngleDiffDeg = MathF.Abs(angleDiffDeg);
        if (absAngleDiffDeg < 0.00001f) return false;

        smoothness = ShapeMath.Clamp(smoothness, 0f, 1f);
        float sideLength = CircleSideLengthRange.LerpInverse(smoothness);
        if(sideLength <= 0f) return false;

        // Arc length for the given angle
        float arcLength = 2f * ShapeMath.PI * radius * (absAngleDiffDeg / 360f);

        sides = (int)MathF.Ceiling(arcLength / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);

        // Angle step in radians (preserve direction)
        angleStepRad = (angleDiffDeg * ShapeMath.DEGTORAD) / sides;

        return true;
    }
    
    /// <summary>
    /// Calculates the number of sides for a circle sector (arc) based on smoothness.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle in degrees.</param>
    /// <param name="endAngleDeg">The ending angle in degrees.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="sides">Output number of sides.</param>
    /// <param name="clampSides">If true, clamps the number of sides to a min/max range set in <see cref="CircleSideCountRange"/>.</param>
    /// <returns>True if parameters were calculated successfully.</returns>
    public static bool CalculateCircleDrawingParameters(float radius, float startAngleDeg, float endAngleDeg, float smoothness, out int sides, bool clampSides = true)
    {
        sides = 0;
        if (radius <= 0f) return false;
        
        // Normalize angle difference to [-360, 360], preserving sign
        float angleDiffDeg = endAngleDeg - startAngleDeg;
        if (angleDiffDeg > 360f) angleDiffDeg %= 360f;
        if (angleDiffDeg < -360f) angleDiffDeg %= 360f;

        float absAngleDiffDeg = MathF.Abs(angleDiffDeg);
        if (absAngleDiffDeg < 0.00001f) return false;

        smoothness = ShapeMath.Clamp(smoothness, 0f, 1f);
        float sideLength = CircleSideLengthRange.LerpInverse(smoothness);
        if(sideLength <= 0f) return false;

        // Arc length for the given angle
        float arcLength = 2f * ShapeMath.PI * radius * (absAngleDiffDeg / 360f);

        sides = (int)MathF.Ceiling(arcLength / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);
        
        return true;
    }
    
    /// <summary>
    /// Calculates drawing parameters for a full circle based on smoothness.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="angleStepRad">Output angle step per side in radians.</param>
    /// <param name="sides">Output number of sides.</param>
    /// <param name="clampSides">If true, clamps the number of sides to a min/max range set in <see cref="CircleSideCountRange"/>.</param>
    /// <returns>True if parameters were calculated successfully.</returns>
    public static bool CalculateCircleDrawingParameters(float radius, float smoothness, out float angleStepRad, out int sides, bool clampSides = true)
    {
        sides = 0;
        angleStepRad = 0f;
        if(radius <= 0f) return false;
        
        smoothness = ShapeMath.Clamp(smoothness, 0f, 1f);
        float sideLength = CircleSideLengthRange.LerpInverse(smoothness);
        if(sideLength <= 0f) return false;
        
        float circumference = 2.0f * ShapeMath.PI * radius;
        sides = (int)MathF.Ceiling(circumference / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);
        
        angleStepRad = MathF.Tau / sides;
        return true;
    }
    
    /// <summary>
    /// Calculates the number of sides for a full circle based on smoothness.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <param name="sides">Output number of sides.</param>
    /// <param name="clampSides">If true, clamps the number of sides to a min/max range set in <see cref="CircleSideCountRange"/>.</param>
    /// <returns>True if parameters were calculated successfully.</returns>
    public static bool CalculateCircleDrawingParameters(float radius, float smoothness, out int sides, bool clampSides = true)
    {
        sides = 0;
        if(radius <= 0f) return false;
        
        smoothness = ShapeMath.Clamp(smoothness, 0f, 1f);
        float sideLength = CircleSideLengthRange.LerpInverse(smoothness);
        if(sideLength <= 0f) return false;
        
        float circumference = 2.0f * ShapeMath.PI * radius;
        sides = (int)MathF.Ceiling(circumference / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);
        
        return true;
    }
    
    /// <summary>
    /// Calculates the number of sides for a circle based on a target side length.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="sideLength">The desired length of each side segment.</param>
    /// <param name="clampSides">If true, clamps the number of sides to a min/max range set in <see cref="CircleSideCountRange"/>.</param>
    /// <returns>The calculated number of sides.</returns>
    public static int GetCircleSideCount(float radius, float sideLength = 10f, bool clampSides = true)
    {
        if (radius <= 0f || sideLength <= 0f) return 0;
        float circumference = 2.0f * ShapeMath.PI * radius;
        var sides = (int)MathF.Ceiling(circumference / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);
        return sides;
    }

    /// <summary>
    /// Calculates the number of sides for a circle arc based on a target side length.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="angleDeg">The angular span of the arc in degrees.</param>
    /// <param name="sideLength">The desired length of each side segment.</param>
    /// <param name="clampSides">If true, clamps the number of sides to a min/max range set in <see cref="CircleSideCountRange"/>.</param>
    /// <returns>The calculated number of sides.</returns>
    public static int GetCircleArcSideCount(float radius, float angleDeg, float sideLength = 10f, bool clampSides = true)
    {
        if (radius <= 0f || angleDeg <= 0f || sideLength <= 0f) return 0;
        
        float circumference = 2.0f * ShapeMath.PI * radius * (angleDeg / 360f);
        var sides = (int)MathF.Ceiling(circumference / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);
        return sides;
    }
    
    #endregion
    
    #region Helper
    private static bool TransformPercentageToAngles(float f, out float startAngleDeg, out float endAngleDeg)
    {
        startAngleDeg = 0f;
        endAngleDeg = 0f;
        
        if(f == 0) return false;
        
        if (f >= 0f)
        {
            startAngleDeg = 0f;
            endAngleDeg = 360f * ShapeMath.Clamp(f, 0f, 1f);
            return true;
        }

        startAngleDeg = 0f;
        endAngleDeg = 360f * ShapeMath.Clamp(f, -1f, 0f);
        return true;
    }
    #endregion
}
