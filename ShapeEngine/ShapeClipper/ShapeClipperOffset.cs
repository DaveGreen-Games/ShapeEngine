using System.Numerics;
using Clipper2Lib;

namespace ShapeEngine.ShapeClipper;

public class ShapeClipperOffset
{
    private ClipperOffset offsetEngine;
    public ShapeClipperScale Scale;
    private readonly Path64 bufferPath64 = new(256);
    
    public double MiterLimit
    {
        get => offsetEngine.MiterLimit;
        set => offsetEngine.MiterLimit = value;
    }

    public double ArcTolerance
    {
        get => offsetEngine.ArcTolerance;
        set => offsetEngine.ArcTolerance = value;
    }

    public bool PreseveCollinear
    {
        get => offsetEngine.PreserveCollinear;
        set => offsetEngine.PreserveCollinear = value;
    }

    public bool ReverseSolution
    {
        get => offsetEngine.ReverseSolution;
        set => offsetEngine.ReverseSolution = value;
    }
    
    public ShapeClipperOffset(int decimalPlaces = 4, double miterLimit = 2.0, double arcTolerance = 0.0, bool preseveCollinear = false, bool reverseSolution = false)
    {
        offsetEngine = new(miterLimit, arcTolerance, preseveCollinear, reverseSolution);
        Scale = new(decimalPlaces);
    }
    
    #region Offsetting
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

    public void OffsetPolyline(IReadOnlyList<Vector2> polyline, float offsetPositive, float miterLimit, bool beveled, ShapeClipperEndType endType, Paths64 result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polyline.Count < 2 || offsetPositive <= 0f) return;

        OffsetPolylineToPaths64(polyline, offsetPositive, miterLimit, beveled, endType, result);
    }
    #endregion

    #region Private
    
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

    private JoinType SelectJoinType(float miterLimit, bool beveled)
    {
        if (miterLimit > 2f) return JoinType.Miter;
        return beveled ? JoinType.Bevel : JoinType.Square;
    }
    
    #endregion
}