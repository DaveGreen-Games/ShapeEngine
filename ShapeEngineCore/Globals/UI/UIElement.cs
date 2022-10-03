using Microsoft.Toolkit.HighPerformance;
using Raylib_CsLo;
using ShapeEngineCore.Globals.Screen;
using System.Numerics;

namespace ShapeEngineCore.Globals.UI
{
    public class UIElement
    {
        protected Rectangle rect;

        public static Vector2 ToAbsolute(Vector2 relative)
        {
            float w = ScreenHandler.UIWidth();
            float h = ScreenHandler.UIHeight();

            return
                new(
                    relative.X * w,
                    relative.Y * h
                    );
        }

        public static Vector2 ToRelative(Vector2 absolute)
        {
            float w = ScreenHandler.UIWidth();
            float h = ScreenHandler.UIHeight();

            return
                new(
                    absolute.X / w,
                    absolute.Y / h
                    );
        }

        public static Rectangle ToAbsolute(Rectangle relative)
        {
            float w = ScreenHandler.UIWidth();
            float h = ScreenHandler.UIHeight();
            return new
                (
                    relative.x * w,
                    relative.Y * h,
                    relative.width * w,
                    relative.height * h
                );
        }

        public static Rectangle ToRelative(Rectangle absolute)
        {
            float w = ScreenHandler.UIWidth();
            float h = ScreenHandler.UIHeight();
            return new
                (
                    absolute.x / w,
                    absolute.Y / h,
                    absolute.width / w,
                    absolute.height / h
                );
        }
        public Rectangle GetRect(bool absolute = true) 
        {
            if (absolute) return ToAbsolute(rect);
            else return rect; 
        }

        public virtual void SetTopLeft(Vector2 newPosRelative) 
        {
            rect.X = newPosRelative.X; rect.Y = newPosRelative.Y;
        }
        public virtual void SetCenter(Vector2 newPosRelative) { SetTopLeft(newPosRelative - GetSize(false) / 2); }
        public virtual void SetBottomRight(Vector2 newPosRelative) { SetTopLeft(newPosRelative - GetSize(false)); }
        public virtual void SetSize(Vector2 newSizeRelative) { rect.width = newSizeRelative.X; rect.height = newSizeRelative.Y; }
        public virtual float GetWidth(bool absolute = true) 
        {
            if (absolute)
            {
                float w = ScreenHandler.UIWidth();
                return rect.width * w;
            }
            else return rect.width; 
        }
        public virtual float GetHeight(bool absolute = true) 
        {
            if (absolute)
            {
                float h = ScreenHandler.UIHeight();
                return rect.height * h;
            }
            else return rect.height; 
        }
        public virtual Vector2 GetTopLeft(bool absolute = true)
        {
            if (absolute) return ToAbsolute(new Vector2 (rect.X, rect.y));
            return new(rect.x, rect.y); 
        }
        public virtual Vector2 GetCenter(bool absolute = true) 
        {
            if (absolute)
            {
                return ToAbsolute(new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2));
            }
            else return new(rect.x + rect.width / 2, rect.y + rect.height / 2); 
        }
        public virtual Vector2 GetBottomRight(bool absolute = true) 
        {
            if (absolute)
            {
                return ToAbsolute(new Vector2(rect.x + rect.width, rect.y + rect.height));
            }
            else return new(rect.x + rect.width, rect.y + rect.height);
        }
        public virtual Vector2 GetSize(bool absolute = true) 
        {
            if (absolute) return ToAbsolute(new Vector2(rect.width, rect.height));
            else return new(rect.width, rect.height); 
        }
        public bool IsPointInside(Vector2 posAbsolute)
        {
            return CheckCollisionPointRec(posAbsolute, ToAbsolute(rect));
        }
        public virtual void Update(float dt, Vector2 mousePos)
        {

        }
        public virtual void Draw()
        {

        }

    }

}
