using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    /// <summary>
    /// Splits the rectangle vertically at a specified factor.
    /// </summary>
    /// <param name="f">A value between 0 and 1 indicating the vertical split position, where 0 is the top and 1 is the bottom.</param>
    /// <returns>A tuple containing the top and bottom rectangles after the split.</returns>
    /// <remarks>
    /// The split is performed by interpolating between the top and bottom edges at the given factor.
    /// </remarks>
    public (Rect top, Rect bottom) SplitV(float f)
    {
        var leftPoint = TopLeft.Lerp(BottomLeft, f);
        var rightPoint = TopRight.Lerp(BottomRight, f);
        Rect top = new(TopLeft, rightPoint);
        Rect bottom = new(leftPoint, BottomRight);
        return (top, bottom);
    }

    /// <summary>
    /// Splits the rectangle horizontally at a specified factor.
    /// </summary>
    /// <param name="f">A value between 0 and 1 indicating the horizontal split position, where 0 is the left and 1 is the right.</param>
    /// <returns>A tuple containing the left and right rectangles after the split.</returns>
    /// <remarks>
    /// The split is performed by interpolating between the left and right edges at the given factor.
    /// </remarks>
    public (Rect left, Rect right) SplitH(float f)
    {
        var topPoint = TopLeft.Lerp(TopRight, f);
        var bottomPoint = BottomLeft.Lerp(BottomRight, f);
        Rect left = new(TopLeft, bottomPoint);
        Rect right = new(topPoint, BottomRight);
        return (left, right);
    }

    /// <summary>
    /// Splits the rectangle into four quadrants based on horizontal and vertical factors.
    /// </summary>
    /// <param name="horizontal">A value between 0 and 1 indicating the horizontal split position.</param>
    /// <param name="vertical">A value between 0 and 1 indicating the vertical split position.</param>
    /// <returns>A tuple containing the top-left, bottom-left, bottom-right, and top-right rectangles.</returns>
    /// <remarks>
    /// This method first splits the rectangle horizontally, then splits each resulting rectangle vertically.
    /// </remarks>
    public (Rect topLeft, Rect bottomLeft, Rect bottomRight, Rect TopRight) Split(float horizontal, float vertical)
    {
        var hor = SplitH(horizontal);
        var left = hor.left.SplitV(vertical);
        var right = hor.right.SplitV(vertical);
        return (left.top, left.bottom, right.bottom, right.top);
    }

    /// <summary>
    /// Splits the rectangle vertically into multiple segments based on an array of factors.
    /// </summary>
    /// <param name="factors">An array of values between 0 and 1 representing the relative heights of each segment. The sum is capped at 1.</param>
    /// <returns>A list of rectangles representing the vertical segments.</returns>
    /// <remarks>
    /// Each factor determines the height of a segment relative to the original rectangle. Remaining area is included as the last segment.
    /// </remarks>
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
    /// Splits the rectangle horizontally into multiple segments based on an array of factors.
    /// </summary>
    /// <param name="factors">An array of values between 0 and 1 representing the relative widths of each segment. The sum is capped at 1.</param>
    /// <returns>A list of rectangles representing the horizontal segments.</returns>
    /// <remarks>
    /// Each factor determines the width of a segment relative to the original rectangle. Remaining area is included as the last segment.
    /// </remarks>
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

    /// <summary>
    /// Splits the rectangle into a grid based on arrays of horizontal and vertical factors.
    /// </summary>
    /// <param name="horizontal">An array of values between 0 and 1 for horizontal splits.</param>
    /// <param name="vertical">An array of values between 0 and 1 for vertical splits.</param>
    /// <returns>A list of rectangles representing the grid cells.</returns>
    /// <remarks>
    /// The rectangle is first split vertically, then each resulting rectangle is split horizontally.
    /// </remarks>
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

    /// <summary>
    /// Splits the rectangle into a specified number of equal-width columns.
    /// </summary>
    /// <param name="columns">The number of columns to split into. Must be at least 2.</param>
    /// <returns>A list of rectangles representing the columns.</returns>
    /// <remarks>
    /// If columns is less than 2, the original rectangle is returned as a single element list.
    /// </remarks>
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

    /// <summary>
    /// Splits the rectangle into a specified number of equal-height rows.
    /// </summary>
    /// <param name="rows">The number of rows to split into.</param>
    /// <returns>A list of rectangles representing the rows.</returns>
    /// <remarks>
    /// If rows is less than 2, the original rectangle is returned as a single element list.
    /// </remarks>
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

    /// <summary>
    /// Splits the rectangle into a grid of columns and rows.
    /// </summary>
    /// <param name="columns">The number of columns in the grid.</param>
    /// <param name="rows">The number of rows in the grid.</param>
    /// <param name="leftToRight">If true, splits vertically first then horizontally; otherwise, splits horizontally first then vertically.</param>
    /// <returns>A list of rectangles representing the grid cells.</returns>
    /// <remarks>
    /// Use this method to create a uniform grid of rectangles within the original rectangle.
    /// </remarks>
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