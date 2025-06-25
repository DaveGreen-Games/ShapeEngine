using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Rect;

public readonly partial struct Rect
{
    public (Rect top, Rect bottom) SplitV(float f)
    {
        var leftPoint = TopLeft.Lerp(BottomLeft, f);
        var rightPoint = TopRight.Lerp(BottomRight, f);
        Rect top = new(TopLeft, rightPoint);
        Rect bottom = new(leftPoint, BottomRight);
        return (top, bottom);
    }

    public (Rect left, Rect right) SplitH(float f)
    {
        var topPoint = TopLeft.Lerp(TopRight, f);
        var bottomPoint = BottomLeft.Lerp(BottomRight, f);
        Rect left = new(TopLeft, bottomPoint);
        Rect right = new(topPoint, BottomRight);
        return (left, right);
    }

    public (Rect topLeft, Rect bottomLeft, Rect bottomRight, Rect TopRight) Split(float horizontal, float vertical)
    {
        var hor = SplitH(horizontal);
        var left = hor.left.SplitV(vertical);
        var right = hor.right.SplitV(vertical);
        return (left.top, left.bottom, right.bottom, right.top);
    }

    /// <summary>
    /// Splits the rect according to the factors. The factors are accumulated and the total factor is capped at 1.
    /// Individual factor values range is between 0 and 1.
    /// </summary>
    /// <param name="factors"></param>
    /// <returns></returns>
    public List<Rect> SplitV(params float[] factors)
    {
        List<Rect> rects = new();
        var curFactor = 0f;
        var original = this;
        var curTopLeft = original.TopLeft;

        foreach (var f in factors)
        {
            if (f <= 0f) continue;

            curFactor += f;
            if (curFactor >= 1f) break;

            var split = original.SplitV(curFactor);
            Rect r = new(curTopLeft, split.top.BottomRight);
            rects.Add(r);
            curTopLeft = split.bottom.TopLeft;
        }

        rects.Add(new(curTopLeft, original.BottomRight));
        return rects;
    }

    /// <summary>
    /// Splits the rect according to the factors. The factors are accumulated and the total factor is capped at 1.
    /// Individual factor values range is between 0 and 1.
    /// </summary>
    /// <param name="factors"></param>
    /// <returns></returns>
    public List<Rect> SplitH(params float[] factors)
    {
        List<Rect> rects = new();
        var curFactor = 0f;
        var original = this;
        var curTopLeft = original.TopLeft;

        foreach (var f in factors)
        {
            if (f <= 0f) continue;

            curFactor += f;
            if (curFactor >= 1f) break;

            var split = original.SplitH(curFactor);
            Rect r = new(curTopLeft, split.left.BottomRight);
            rects.Add(r);
            curTopLeft = split.right.TopLeft;
        }

        rects.Add(new(curTopLeft, original.BottomRight));
        return rects;
    }

    public List<Rect> Split(float[] horizontal, float[] vertical)
    {
        List<Rect> rects = new();
        var verticalRects = SplitV(vertical);
        foreach (var r in verticalRects)
        {
            rects.AddRange(r.SplitH(horizontal));
        }

        return rects;
    }

    public List<Rect> SplitH(int columns)
    {
        if (columns < 2) return new() { this };
        List<Rect> rects = new();
        Vector2 startPos = new(X, Y);

        float elementWidth = Width / columns;
        Vector2 offset = new(0f, 0f);
        for (int i = 0; i < columns; i++)
        {
            var size = new Size(elementWidth, Height);
            Rect r = new(startPos + offset, size, new(0f));
            rects.Add(r);
            offset += new Vector2(elementWidth, 0f);
        }

        return rects;
    }

    public List<Rect> SplitV(int rows)
    {
        List<Rect> rects = new();
        Vector2 startPos = new(X, Y);

        float elementHeight = Height / rows;
        Vector2 offset = new(0f, 0f);
        for (int i = 0; i < rows; i++)
        {
            var size = new Size(Width, elementHeight);
            Rect r = new(startPos + offset, size, new(0f));
            rects.Add(r);
            offset += new Vector2(0, elementHeight);
        }

        return rects;
    }

    public List<Rect> Split(int columns, int rows, bool leftToRight = true)
    {
        var rects = new List<Rect>();
        if (leftToRight)
        {
            var verticals = SplitV(rows);
            foreach (var vertical in verticals)
            {
                rects.AddRange(vertical.SplitH(columns));
            }
        }
        else
        {
            var horizontals = SplitH(columns);
            foreach (var horizontal in horizontals)
            {
                rects.AddRange(horizontal.SplitV(rows));
            }
        }


        return rects;
    }
}