using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Core;

namespace ShapeEngine.ShapeClipper;

/// <summary>
/// Wraps a <see cref="ClipperOffset"/> instance to offset polygons and polylines using the engine's world-space coordinate conventions.
/// </summary>
/// <remarks>
/// Input geometry is converted to Clipper's integer-based path representation using <see cref="Scale"/>, then written to the provided <see cref="Paths64"/> collection.
/// This type reuses internal buffers to reduce allocations and is intended for repeated offset operations.
/// </remarks>
public class ShapeClipperOffset
{
    private ClipperOffset offsetEngine;
    /// <summary>
    /// Gets or sets the decimal precision used to convert between world-space coordinates and Clipper's integer coordinates.
    /// </summary>
    public DecimalPrecision Scale;
    private readonly Path64 bufferPath64 = new(256);
    
    /// <summary>
    /// Gets or sets the miter limit used by the underlying offset engine.
    /// </summary>
    /// <remarks>
    /// Larger values allow sharper miter joins before Clipper falls back to other join behavior.
    /// </remarks>
    public double MiterLimit
    {
        get => offsetEngine.MiterLimit;
        set => offsetEngine.MiterLimit = value;
    }

    /// <summary>
    /// Gets or sets the arc tolerance used when approximating round joins or caps.
    /// </summary>
    public double ArcTolerance
    {
        get => offsetEngine.ArcTolerance;
        set => offsetEngine.ArcTolerance = value;
    }

    /// <summary>
    /// Gets or sets whether collinear edges should be preserved by the underlying offset engine.
    /// </summary>
    public bool PreseveCollinear
    {
        get => offsetEngine.PreserveCollinear;
        set => offsetEngine.PreserveCollinear = value;
    }

    /// <summary>
    /// Gets or sets whether generated solution paths should have reversed winding.
    /// </summary>
    public bool ReverseSolution
    {
        get => offsetEngine.ReverseSolution;
        set => offsetEngine.ReverseSolution = value;
    }
    
    /// <summary>
    /// Initializes a new <see cref="ShapeClipperOffset"/> with the specified conversion precision and offset engine settings.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places preserved when converting world-space coordinates to Clipper coordinates.</param>
    /// <param name="miterLimit">The default miter limit used for offset joins.</param>
    /// <param name="arcTolerance">The default tolerance used for arc approximation in the offset engine.</param>
    /// <param name="preseveCollinear">Whether the offset engine should preserve collinear edges.</param>
    /// <param name="reverseSolution">Whether the offset engine should reverse the winding of generated solution paths.</param>
    public ShapeClipperOffset(int decimalPlaces = 4, double miterLimit = 2.0, double arcTolerance = 0.0, bool preseveCollinear = false, bool reverseSolution = false)
    {
        offsetEngine = new(miterLimit, arcTolerance, preseveCollinear, reverseSolution);
        Scale = new(decimalPlaces);
    }
    
    #region Offsetting
    /// <summary>
    /// Offsets a closed polygon and writes the resulting paths into <paramref name="result"/>.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices to offset. The polygon is expected to be wound counterclockwise.</param>
    /// <param name="offset">The offset distance in world units. Positive values expand the polygon and negative values shrink it.</param>
    /// <param name="miterLimit">The miter limit to use when selecting join behavior for sharp corners.</param>
    /// <param name="beveled">Whether non-miter joins should use beveled corners instead of square corners.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the generated offset paths.</param>
    /// <remarks>
    /// If <paramref name="offset"/> is zero, this method returns a single path equivalent to the input polygon.
    /// If the polygon contains fewer than three points, no output is produced.
    /// </remarks>
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

    /// <summary>
    /// Offsets an open polyline and writes the resulting stroked outline paths into <paramref name="result"/>.
    /// </summary>
    /// <param name="polyline">The polyline vertices to offset.</param>
    /// <param name="offsetPositive">The positive offset distance in world units used to build the stroked outline.</param>
    /// <param name="miterLimit">The miter limit to use when selecting join behavior for sharp corners.</param>
    /// <param name="beveled">Whether non-miter joins should use beveled corners instead of square corners.</param>
    /// <param name="endType">The end-cap style to use for the open polyline.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the generated offset paths.</param>
    /// <remarks>
    /// If the polyline contains fewer than two points, or <paramref name="offsetPositive"/> is less than or equal to zero, no output is produced.
    /// </remarks>
    public void OffsetPolyline(IReadOnlyList<Vector2> polyline, float offsetPositive, float miterLimit, bool beveled, ShapeClipperEndType endType, Paths64 result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polyline.Count < 2 || offsetPositive <= 0f) return;

        OffsetPolylineToPaths64(polyline, offsetPositive, miterLimit, beveled, endType, result);
    }
    #endregion

    #region Private
    
    /// <summary>
    /// Converts a polygon to Clipper coordinates, applies the requested offset, and writes the resulting paths to <paramref name="outPaths"/>.
    /// </summary>
    /// <param name="polygonCCW">The counterclockwise polygon vertices to offset.</param>
    /// <param name="offsetWorld">The offset distance in world units.</param>
    /// <param name="miterLimit">The miter limit used to choose join behavior.</param>
    /// <param name="beveled">Whether non-miter joins should use beveled corners.</param>
    /// <param name="outPaths">The destination collection for the generated Clipper paths.</param>
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

    /// <summary>
    /// Converts a polyline to Clipper coordinates, applies the requested offset, and writes the resulting paths to <paramref name="outPaths"/>.
    /// </summary>
    /// <param name="polyline">The polyline vertices to offset.</param>
    /// <param name="offsetWorldPositive">The positive offset distance in world units.</param>
    /// <param name="miterLimit">The miter limit used to choose join behavior.</param>
    /// <param name="beveled">Whether non-miter joins should use beveled corners.</param>
    /// <param name="endType">The end-cap style to use for the polyline.</param>
    /// <param name="outPaths">The destination collection for the generated Clipper paths.</param>
    private void OffsetPolylineToPaths64(IReadOnlyList<Vector2> polyline, float offsetWorldPositive, float miterLimit, bool beveled, ShapeClipperEndType endType, Paths64 outPaths)
    {
        outPaths.Clear();

        bufferPath64.Clear();
        ClipperImmediate2D.ToPath64(polyline, bufferPath64);

        JoinType jt = SelectJoinType(miterLimit, beveled);

        offsetEngine.Clear();
        if (miterLimit > 2f) offsetEngine.MiterLimit = miterLimit;

        offsetEngine.AddPath(bufferPath64, jt, endType.ToClipperEndType());

        double delta = offsetWorldPositive * Scale.Scale;
        offsetEngine.Execute(delta, outPaths);
    }

    /// <summary>
    /// Selects the Clipper join type that should be used for an offset operation.
    /// </summary>
    /// <param name="miterLimit">The requested miter limit.</param>
    /// <param name="beveled">Whether non-miter joins should use beveled corners.</param>
    /// <returns>The <see cref="JoinType"/> that matches the requested join behavior.</returns>
    private JoinType SelectJoinType(float miterLimit, bool beveled)
    {
        if (miterLimit > 2f) return JoinType.Miter;
        return beveled ? JoinType.Bevel : JoinType.Square;
    }
    
    #endregion
}