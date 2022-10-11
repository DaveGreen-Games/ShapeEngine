using Raylib_CsLo;
using ShapeEngineCore.Globals.Screen;
using System.Numerics;
using System.Reflection;

namespace ShapeEngineCore.Globals.UI
{
    public class UIElement
    {
        protected Rectangle rect;

        public Rectangle GetRect(Alignement alignement = Alignement.TOPLEFT) 
        { 
            if(alignement == Alignement.TOPLEFT) return rect;
            else
            {
                Vector2 topLeft = new Vector2(rect.X, rect.Y);
                Vector2 size = GetSize();
                Vector2 offset = size * UIHandler.GetAlignementVector(alignement);
                return new
                    (
                        topLeft.X + offset.X,
                        topLeft.Y + offset.Y,
                        size.X,
                        size.Y
                    );
            }
        }
        

        public virtual void UpdateRect(Vector2 pos, Vector2 size, Alignement alignement = Alignement.CENTER)
        {
            Vector2 align = UIHandler.GetAlignementVector(alignement);
            Vector2 offset = size * align;
            rect = new(pos.X - offset.X, pos.Y - offset.Y, size.X, size.Y);
        }
        public virtual void UpdateRect(Rectangle rect, Alignement alignement = Alignement.CENTER) 
        { 
            UpdateRect(new Vector2(rect.x, rect.y), new Vector2(rect.width, rect.height), alignement);
        }
        //public virtual void SetTopLeft(Vector2 newPos) { rect.X = newPos.X; rect.Y = newPos.Y; }
        //public virtual void SetCenter(Vector2 newPos) { SetTopLeft(newPos - GetSize() / 2); }
        //public virtual void SetBottomRight(Vector2 newPos) { SetTopLeft(newPos - GetSize()); }
        //public virtual void SetSize(Vector2 newSize) { rect.width = newSize.X; rect.height = newSize.Y; }
        
        
        public virtual float GetWidth() { return rect.width; }
        public virtual float GetHeight() { return rect.height; }
        //public virtual Vector2 GetTopLeft() { return new(rect.x, rect.y); }
        //public virtual Vector2 GetCenter() { return new(rect.x + rect.width / 2, rect.y + rect.height / 2); }
        //public virtual Vector2 GetBottomRight() { return new(rect.x + rect.width, rect.y + rect.height); }
        public virtual Vector2 GetPos(Alignement alignement = Alignement.CENTER) 
        {
            Vector2 topLeft = new Vector2(rect.X, rect.Y);
            Vector2 offset = GetSize() * UIHandler.GetAlignementVector(alignement);
            return topLeft + offset;
        }
        public virtual Vector2 GetPos(Vector2 alignement)
        {
            Vector2 topLeft = new Vector2(rect.X, rect.Y);
            Vector2 offset = GetSize() * alignement;
            return topLeft + offset;
        }
        public virtual Vector2 GetSize() { return new(rect.width, rect.height); }

        //public Rectangle GetScaledRect() { return Utils.MultiplyRectangle(rect, ScreenHandler.GetUIStretchFactor()); }
        //public Vector2 GetScaledSize() { return GetSize() * ScreenHandler.GetUIStretchFactor(); }


        public bool IsPointInside(Vector2 uiPos)
        {
            return CheckCollisionPointRec(uiPos, rect); // GetScaledRect());
        }
        public virtual void Update(float dt, Vector2 mousePosUI)
        {

        }
        public virtual void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {

        }

    }

}
