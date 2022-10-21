using Raylib_CsLo;
using ShapeEngineCore.Globals.Screen;
using System.Numerics;
using System.Reflection;

namespace ShapeEngineCore.Globals.UI
{
    public struct UIMargins
    {
        public float top = 0f;
        public float right = 0f;
        public float bottom = 0f;
        public float left = 0f;

        //public UIMargins() { }
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

        public Rectangle Apply(Rectangle rect)
        {
            Vector2 tl = new(rect.x, rect.y);
            Vector2 size = new(rect.width, rect.height);
            Vector2 br = tl + size;

            tl.X += size.X * left;
            tl.Y += size.Y * top;
            br.X -= size.X * right;
            br.Y -= size.Y * bottom;

            //tl += size * new Vector2(left, top);
            //br -= size * new Vector2(right, bottom);

            Vector2 finalTopLeft = new (MathF.Min(tl.X, br.X), MathF.Min(tl.Y, br.Y));
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
    public class UIElement
    {
        protected Rectangle rect;

        protected float stretchFactor = 1f; //used for containers
        protected UIMargins margins = new();


        public void SetMargins(float left = 0, float right = 0, float top = 0, float bottom = 0)
        {
            margins = new(top, right, bottom, left);
        }
        public void SetStretchFactor(float newFactor) { stretchFactor = newFactor; }

        public Rectangle GetRect(Alignement alignement = Alignement.TOPLEFT) 
        { 
            if(alignement == Alignement.TOPLEFT) return margins.Apply(rect);
            else
            {
                Vector2 topLeft = new Vector2(rect.X, rect.Y);
                Vector2 size = GetSize();
                Vector2 offset = size * UIHandler.GetAlignementVector(alignement);
                return margins.Apply(new
                    (
                        topLeft.X + offset.X,
                        topLeft.Y + offset.Y,
                        size.X,
                        size.Y
                    ));
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
        
        
        public virtual float GetWidth() { return GetRect().width; }
        public virtual float GetHeight() { return GetRect().height; }
        //public virtual Vector2 GetTopLeft() { return new(rect.x, rect.y); }
        //public virtual Vector2 GetCenter() { return new(rect.x + rect.width / 2, rect.y + rect.height / 2); }
        //public virtual Vector2 GetBottomRight() { return new(rect.x + rect.width, rect.y + rect.height); }
        public virtual Vector2 GetPos(Alignement alignement = Alignement.CENTER) 
        {
            Rectangle rect = GetRect();
            Vector2 topLeft = new Vector2(rect.X, rect.Y);
            Vector2 offset = GetSize() * UIHandler.GetAlignementVector(alignement);
            return topLeft + offset;
        }
        public virtual Vector2 GetPos(Vector2 alignement)
        {
            Rectangle rect = GetRect();
            Vector2 topLeft = new Vector2(rect.X, rect.Y);
            Vector2 offset = GetSize() * alignement;
            return topLeft + offset;
        }
        public virtual Vector2 GetSize() 
        {
            Rectangle rect = GetRect();
            return new(rect.width, rect.height); 
        }

        //public Rectangle GetScaledRect() { return Utils.MultiplyRectangle(rect, ScreenHandler.GetUIStretchFactor()); }
        //public Vector2 GetScaledSize() { return GetSize() * ScreenHandler.GetUIStretchFactor(); }


        public bool IsPointInside(Vector2 uiPos)
        {
            return CheckCollisionPointRec(uiPos, GetRect()); // GetScaledRect());
        }
        public virtual void Update(float dt, Vector2 mousePosUI)
        {

        }
        public virtual void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {

        }

    }

}
