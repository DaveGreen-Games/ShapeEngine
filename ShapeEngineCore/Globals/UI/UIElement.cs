using Raylib_CsLo;
using System.Numerics;

namespace ShapeEngineCore.Globals.UI
{
    public class UIElement
    {
        protected Rectangle rect;

        public Rectangle GetRect() { return rect; }

        public virtual void SetTopLeft(Vector2 newPos) { rect.X = newPos.X; rect.Y = newPos.Y; }
        public virtual void SetCenter(Vector2 newPos) { SetTopLeft(newPos - GetSize() / 2); }
        public virtual void SetBottomRight(Vector2 newPos) { SetTopLeft(newPos - GetSize()); }
        public virtual void SetSize(Vector2 newSize) { rect.width = newSize.X; rect.height = newSize.Y; }
        public virtual float GetWidth() { return rect.width; }
        public virtual float GetHeight() { return rect.height; }
        public virtual Vector2 GetTopLeft() { return new(rect.x, rect.y); }
        public virtual Vector2 GetCenter() { return new(rect.x + rect.width / 2, rect.y + rect.height / 2); }
        public virtual Vector2 GetBottomRight() { return new(rect.x + rect.width, rect.y + rect.height); }
        public virtual Vector2 GetSize() { return new(rect.width, rect.height); }
        public bool IsPointInside(Vector2 pos)
        {
            return CheckCollisionPointRec(pos, rect);
        }
        public virtual void Update(float dt, Vector2 mousePos)
        {

        }
        public virtual void Draw()
        {

        }

    }

}
