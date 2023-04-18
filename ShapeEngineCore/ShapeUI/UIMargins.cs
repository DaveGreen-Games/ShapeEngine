using Raylib_CsLo;
using ShapeCore;
using System.Numerics;

namespace ShapeUI
{
    public class UIMargins
    {
        public float top = 0f;
        public float right = 0f;
        public float bottom = 0f;
        public float left = 0f;

        public UIMargins() { }
        public UIMargins(float top, float right, float bottom, float left)
        {
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.left = left;
        }
        public UIMargins(Vector2 horizontal, Vector2 vertical)
        {
            this.left = horizontal.X;
            this.right = horizontal.Y;
            this.top = vertical.X;
            this.bottom = vertical.Y;
        }

        public Rect Apply(Rect rect)
        {
            Vector2 tl = new(rect.x, rect.y);
            Vector2 size = new(rect.width, rect.height);
            Vector2 br = tl + size;

            tl.X += size.X * left;
            tl.Y += size.Y * top;
            br.X -= size.X * right;
            br.Y -= size.Y * bottom;

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

}
