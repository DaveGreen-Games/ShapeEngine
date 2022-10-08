using Raylib_CsLo;
using ShapeEngineCore.Globals.Screen;
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

        //public Rectangle GetScaledRect() { return Utils.MultiplyRectangle(rect, ScreenHandler.GetUIStretchFactor()); }
        //public Vector2 GetScaledSize() { return GetSize() * ScreenHandler.GetUIStretchFactor(); }


        public bool IsPointInside(Vector2 posRaw)
        {
            return CheckCollisionPointRec(posRaw, rect); // GetScaledRect());
        }
        public virtual void Update(float dt, Vector2 mousePosRaw)
        {

        }
        public virtual void Draw(Vector2 devRes, Vector2 stretchFactor)
        {

        }

    }

}
