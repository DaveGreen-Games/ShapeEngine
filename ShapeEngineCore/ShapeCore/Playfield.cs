using System.Numerics;
using Raylib_CsLo;

namespace ShapeCore
{
    public class Playfield
    {
        protected Rectangle rect;
        //protected float wallThickness = 3f;
        //protected Color color = WHITE;
        protected float drawOrder = 100;
        public Playfield(Vector2 topleft, Vector2 bottomright, float drawOrder = 0f)
        {
            rect = new(topleft.X, topleft.Y, bottomright.X - topleft.X, bottomright.Y - topleft.Y);
            //this.wallThickness = wallThickness;
            this.drawOrder = drawOrder;
        }
        public Playfield(Rectangle rect, float drawOrder = 0f)
        {
            this.rect = rect;
            //this.wallThickness = wallThickness;
            this.drawOrder = drawOrder;
        }

        public float GetDrawOrder() { return drawOrder; }

        public void Change(Vector2 topleft, Vector2 bottomright) { ChangeTopLeft(topleft); ChangeBottomRight(bottomright); }
        public void Change(Vector2 topleft, float w, float h) { ChangeTopLeft(topleft); ChangeSize(w, h); }
        public void ChangeTopLeft(Vector2 value) { rect.x = value.X; rect.y = value.Y; }
        public void ChangeBottomRight(Vector2 value) { ChangeSize(value.X - rect.X, value.Y - rect.Y); }
        public void ChangeHeight(float value) { rect.height = value; }
        public void ChangeWidth(float value) { rect.width = value; }
        public void ChangeSize(float w, float h) { ChangeWidth(w); ChangeHeight(h); }
        public void ChangeSize(Vector2 size) { ChangeWidth(size.X); ChangeHeight(size.Y); }
        //public void SetWallThickness(float value) { if (value > 0f) wallThickness = value; }
        //public void SetColor(Color value) { color = value; }

        public (bool collided, Vector2 hitPoint, Vector2 n, Vector2 newPos) Collide(Vector2 pos, float radius)
        {
            bool collided = false;
            Vector2 hitPoint = pos;
            Vector2 n = new(0f, 0f);
            Vector2 newPos = pos;
            if (pos.X + radius > rect.x + rect.width)
            {
                hitPoint = new(rect.x + rect.width, pos.Y);
                newPos.X = hitPoint.X - radius;
                n = new(-1, 0);
                collided = true;
            }
            else if (pos.X - radius < rect.x)
            {
                hitPoint = new(rect.x, pos.Y);
                newPos.X = hitPoint.X + radius;
                n = new(1, 0);
                collided = true;
            }

            if (pos.Y + radius > rect.Y + rect.height)
            {
                hitPoint = new(pos.X, rect.Y + rect.height);
                newPos.Y = hitPoint.Y - radius;
                n = new(0, -1);
                collided = true;
            }
            else if (pos.Y - radius < rect.y)
            {
                hitPoint = new(pos.X, rect.Y);
                newPos.Y = hitPoint.Y + radius;
                n = new(0, 1);
                collided = true;
            }

            return (collided, hitPoint, n, newPos);
        }
        public (bool outOfBounds, Vector2 newPos) WrapAround(Vector2 pos, float radius)
        {
            bool outOfBounds = false;
            Vector2 newPos = pos;
            if (pos.X + radius > rect.x + rect.width)
            {
                newPos = new(rect.x, pos.Y);
                outOfBounds = true;
            }
            else if (pos.X - radius < rect.x)
            {
                newPos = new(rect.x + rect.width, pos.Y);
                outOfBounds = true;
            }

            if (pos.Y + radius > rect.Y + rect.height)
            {
                newPos = new(pos.X, rect.Y);
                outOfBounds = true;
            }
            else if (pos.Y - radius < rect.y)
            {
                newPos = new(pos.X, rect.Y + rect.height);
                outOfBounds = true;
            }

            return (outOfBounds, newPos);
        }

        public virtual void Draw()
        {
            //DrawRectangleLinesEx(rect, wallThickness, color);
        }
    }
}
