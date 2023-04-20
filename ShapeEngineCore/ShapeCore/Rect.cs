using System.Numerics;
using Raylib_CsLo;

namespace ShapeCore
{
    public struct Rect
    {
        public float x;

        public float y;

        public float width;

        public float height;

        public Vector2 TopLeft { get { return new Vector2(x, y); } }
        public Vector2 TopRight { get { return new Vector2(x + width, 0); } }
        public Vector2 BottomRight { get { return new Vector2(x+width, y+height); } }
        public Vector2 BottomLeft { get { return new Vector2(x, y + height); } }
        public Vector2 Center { get { return new Vector2(x + width * 0.5f, y + height * 0.5f); } }

        public Vector2 Size { get { return new Vector2(width, height); } }
        public Rectangle Rectangle { get { return new(x, y, width, height); } }
        
        public Rect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        public Rect(Vector2 topLeft, Vector2 bottomRight)
        {
            if (topLeft.X > bottomRight.X)
            {
                this.x = bottomRight.X;
                this.width = topLeft.X - bottomRight.X;
            }
            else
            {
                this.x = topLeft.X;
                this.width = bottomRight.X - topLeft.X;
            }

            if (topLeft.Y > bottomRight.Y)
            {
                this.y = bottomRight.Y;
                this.height = topLeft.Y - bottomRight.Y;
            }
            else
            {
                this.y = topLeft.Y;
                this.height = bottomRight.Y - topLeft.Y;
            }
        }
        public Rect(Vector2 pos, Vector2 size, Vector2 alignement)
        {
            Vector2 offset = size * alignement;
            Vector2 topLeft = pos - offset;
            this.x = topLeft.X;
            this.y = topLeft.Y;
            this.width = size.X;
            this.height = size.Y;
        }
        public Rect(Rectangle rect)
        {
            this.x = rect.X;
            this.y = rect.Y;
            this.width = rect.width;
            this.height = rect.height;
        }

        public Vector2 GetPos(Vector2 alignement)
        {
            Vector2 offset = Size * alignement;
            return TopLeft + offset;
        }
        public bool IsPointInside(Vector2 p)
        {
            Vector2 tl = TopLeft;
            Vector2 br = BottomRight;
            return p.X > tl.X && p.X < br.X && p.Y > tl.Y && p.Y < br.Y;
        }
    }
}
