using System.Numerics;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    /// <summary>
    /// Represents margin values for each side of a rectangle.
    /// </summary>
    /// <remarks>
    /// Margins can be set individually for top, right, bottom, and left, or uniformly for all sides.
    /// </remarks>
    public readonly struct Margins
    {
        /// <summary>
        /// Gets a value indicating whether any margin is non-zero.
        /// </summary>
        public bool Valid => Top != 0 || Bottom != 0 || Left != 0 || Right != 0;

        /// <summary>Top margin value.</summary>
        public readonly float Top;
        /// <summary>Bottom margin value.</summary>
        public readonly float Bottom;
        /// <summary>Left margin value.</summary>
        public readonly float Left;
        /// <summary>Right margin value.</summary>
        public readonly float Right;

        /// <summary>
        /// Initializes a new <see cref="Margins"/> struct with all margins set to zero.
        /// </summary>
        public Margins()
        {
            this.Top = 0f;
            this.Right = 0f;
            this.Bottom = 0f;
            this.Left = 0f;
        }
        /// <summary>
        /// Initializes a new <see cref="Margins"/> struct with all margins set to the same value.
        /// </summary>
        /// <param name="margin">The value to use for all margins.</param>
        public Margins(float margin)
        {
            this.Top = margin;
            this.Right = margin;
            this.Bottom = margin;
            this.Left = margin;
        }
        /// <summary>
        /// Initializes a new <see cref="Margins"/> struct with horizontal and vertical margins.
        /// </summary>
        /// <param name="horizontal">The value for left and right margins.</param>
        /// <param name="vertical">The value for top and bottom margins.</param>
        public Margins(float horizontal, float vertical)
        {
            this.Top = vertical;
            this.Right = horizontal;
            this.Bottom = vertical;
            this.Left = horizontal;
        }
        /// <summary>
        /// Initializes a new <see cref="Margins"/> struct with individual values for each side.
        /// </summary>
        /// <param name="top">Top margin value.</param>
        /// <param name="right">Right margin value.</param>
        /// <param name="bottom">Bottom margin value.</param>
        /// <param name="left">Left margin value.</param>
        public Margins(float top, float right, float bottom, float left)
        {
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
            this.Left = left;
        }
        /// <summary>
        /// Initializes a new <see cref="Margins"/> struct with horizontal and vertical vectors.
        /// </summary>
        /// <param name="horizontal">X: left, Y: right margin values.</param>
        /// <param name="vertical">X: top, Y: bottom margin values.</param>
        public Margins(Vector2 horizontal, Vector2 vertical)
        {
            this.Left = horizontal.X;
            this.Right = horizontal.Y;
            this.Top = vertical.X;
            this.Bottom = vertical.Y;
        }

    }
    
    /// <summary>
    /// Returns a new rectangle with the specified margins applied (relative to the current rectangle).
    /// </summary>
    /// <param name="margins">The margins to apply.</param>
    /// <returns>A new rectangle with margins applied, or the original rectangle if margins are not valid.</returns>
    /// <remarks>
    /// Margins are subtracted from each side of the rectangle.
    /// </remarks>
    public Rect ApplyMargins(Margins margins)
    {
        return !margins.Valid ? this : ApplyMargins(margins.Left, margins.Right, margins.Top, margins.Bottom);
    }

    /// <summary>
    /// Returns a new rectangle with the specified margins applied as absolute values (relative to the current rectangle).
    /// </summary>
    /// <param name="margins">The margins to apply.</param>
    /// <returns>A new rectangle with absolute margins applied, or the original rectangle if margins are not valid.</returns>
    /// <remarks>
    /// Margins are applied as absolute distances from each side.
    /// </remarks>
    public Rect ApplyMarginsAbsolute(Margins margins)
    {
        return !margins.Valid ? this : ApplyMarginsAbsolute(margins.Left, margins.Right, margins.Top, margins.Bottom);
    }

    /// <summary>
    /// Returns a new rectangle with the same margin applied to all sides.
    /// </summary>
    /// <param name="margin">The margin to apply to all sides.</param>
    /// <returns>A new rectangle with margins applied.</returns>
    public Rect ApplyMargins(float margin) => ApplyMargins(margin, margin, margin, margin);
    /// <summary>
    /// Returns a new rectangle with the same absolute margin applied to all sides.
    /// </summary>
    /// <param name="margin">The absolute margin to apply to all sides.</param>
    /// <returns>A new rectangle with absolute margins applied.</returns>
    public Rect ApplyMarginsAbsolute(float margin) => ApplyMarginsAbsolute(margin, margin, margin, margin);

    /// <summary>
    /// Returns a new rectangle with the specified margins applied to each side as a percentage of the current size.
    /// </summary>
    /// <param name="left">The left margin as a percentage of the width (-1 to 1).</param>
    /// <param name="right">The right margin as a percentage of the width (-1 to 1).</param>
    /// <param name="top">The top margin as a percentage of the height (-1 to 1).</param>
    /// <param name="bottom">The bottom margin as a percentage of the height (-1 to 1).</param>
    /// <returns>A new rectangle with margins applied.</returns>
    /// <remarks>
    /// Margins are clamped to the range [-1, 1] and subtracted from each side of the rectangle.
    /// </remarks>
    public Rect ApplyMargins(float left, float right, float top, float bottom)
    {
        if (left == 0f && right == 0f && top == 0f && bottom == 0f) return this;
        
        left = ShapeMath.Clamp(left, -1f, 1f);
        right = ShapeMath.Clamp(right, -1f, 1f);
        top = ShapeMath.Clamp(top, -1f, 1f);
        bottom = ShapeMath.Clamp(bottom, -1f, 1f);


        var tl = TopLeft;
        var size = Size;
        var br = tl + size;

        tl.X += size.Width * left;
        tl.Y += size.Height * top;
        br.X -= size.Width * right;
        br.Y -= size.Height * bottom;

        Vector2 finalTopLeft = new(MathF.Min(tl.X, br.X), MathF.Min(tl.Y, br.Y));
        Vector2 finalBottomRight = new(MathF.Max(tl.X, br.X), MathF.Max(tl.Y, br.Y));
        return new
            (
                finalTopLeft.X,
                finalTopLeft.Y,
                finalBottomRight.X - finalTopLeft.X,
                finalBottomRight.Y - finalTopLeft.Y
            );
    }
    /// <summary>
    /// Returns a new rectangle with the specified absolute margins applied to each side.
    /// </summary>
    /// <param name="left">The left margin in absolute units.</param>
    /// <param name="right">The right margin in absolute units.</param>
    /// <param name="top">The top margin in absolute units.</param>
    /// <param name="bottom">The bottom margin in absolute units.</param>
    /// <returns>A new rectangle with absolute margins applied.</returns>
    /// <remarks>
    /// Margins are subtracted from each side of the rectangle in absolute units.
    /// </remarks>
    public Rect ApplyMarginsAbsolute(float left, float right, float top, float bottom)
    {
        if (left == 0f && right == 0f && top == 0f && bottom == 0f) return this;
        var tl = TopLeft;
        var br = BottomRight;
        
        tl.X += left;
        tl.Y += top;
        br.X -= right;
        br.Y -= bottom;

        Vector2 finalTopLeft = new(MathF.Min(tl.X, br.X), MathF.Min(tl.Y, br.Y));
        Vector2 finalBottomRight = new(MathF.Max(tl.X, br.X), MathF.Max(tl.Y, br.Y));
        return new
            (
                finalTopLeft.X,
                finalTopLeft.Y,
                finalBottomRight.X - finalTopLeft.X,
                finalBottomRight.Y - finalTopLeft.Y
            );
    }
    
}