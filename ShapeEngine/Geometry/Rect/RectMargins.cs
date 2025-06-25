using System.Numerics;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Rect;

public readonly partial struct Rect
{
    public readonly struct Margins
    {
        public bool Valid => Top != 0 || Bottom != 0 || Left != 0 || Right != 0;

        public readonly float Top;
        public readonly float Bottom;
        public readonly float Left;
        public readonly float Right;

        public Margins()
        {
            this.Top = 0f;
            this.Right = 0f;
            this.Bottom = 0f;
            this.Left = 0f;
        }
        public Margins(float margin)
        {
            this.Top = margin;
            this.Right = margin;
            this.Bottom = margin;
            this.Left = margin;
        }
        public Margins(float horizontal, float vertical)
        {
            this.Top = vertical;
            this.Right = horizontal;
            this.Bottom = vertical;
            this.Left = horizontal;
        }
        public Margins(float top, float right, float bottom, float left)
        {
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
            this.Left = left;
        }

        public Margins(Vector2 horizontal, Vector2 vertical)
        {
            this.Left = horizontal.X;
            this.Right = horizontal.Y;
            this.Top = vertical.X;
            this.Bottom = vertical.Y;
        }

    }
    
    public Rect ApplyMargins(Margins margins)
    {
        return !margins.Valid ? this : ApplyMargins(margins.Left, margins.Right, margins.Top, margins.Bottom);
    }

    public Rect ApplyMarginsAbsolute(Margins margins)
    {
        return !margins.Valid ? this : ApplyMarginsAbsolute(margins.Left, margins.Right, margins.Top, margins.Bottom);
    }

    public Rect ApplyMargins(float margin) => ApplyMargins(margin, margin, margin, margin);
    public Rect ApplyMarginsAbsolute(float margin) => ApplyMarginsAbsolute(margin, margin, margin, margin);

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