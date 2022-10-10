using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals.UI
{


    public class ProgressBarPro : ProgressBar
    {
        Vector2 pivot;
        float rot = 0f; //in degrees

        protected ProgressBarPro() { }
        public ProgressBarPro(Vector2 topLeft, Vector2 size, BarType barType = BarType.LEFTRIGHT, float transitionSpeed = 0.1f, float rotation = 0f, bool centered = false)
        {
            //correct way of doing it but calculating progress bar must be fixed first
            //this.rect = new(topLeft.X, topLeft.Y, size.X, size.Y);
            //this.pivot = new(0f, 0f);
            rect = new(topLeft.X + size.X / 2, topLeft.Y + size.Y / 2, size.X, size.Y);
            pivot = new(size.X / 2, size.Y / 2);
            this.barType = barType;
            this.transitionSpeed = transitionSpeed;
            outlineSize = 0;
            rot = rotation;
            this.centered = centered;
            SetF(1.0f, true);
        }
        public ProgressBarPro(Vector2 topLeft, Vector2 size, Vector2 barOffset, BarType barType = BarType.LEFTRIGHT, float transitionSpeed = 0.1f, float rotation = 0f, bool centered = false)
        {
            //correct way of doing it but calculating progress bar must be fixed first
            //this.rect = new(topLeft.X, topLeft.Y, size.X, size.Y);
            //this.pivot = new(0f, 0f);
            rect = new(topLeft.X + size.X / 2, topLeft.Y + size.Y / 2, size.X, size.Y);
            pivot = new(size.X / 2, size.Y / 2);
            this.barType = barType;
            this.transitionSpeed = transitionSpeed;
            outlineSize = 0;
            rot = rotation;
            this.centered = centered;
            this.barOffset = barOffset;
            SetF(1.0f, true);
        }

        public float GetRotationDeg() { return rot; }
        public float GetRotationRad() { return rot * DEG2RAD; }
        public Vector2 Transform(Vector2 v) { return Vec.Rotate(v, GetRotationRad()); }

        public override Vector2 GetTopLeft()
        {
            return GetCenter() - Vec.Rotate(GetSize() / 2, GetRotationRad());
        }
        public override Vector2 GetCenter()
        {
            return new(rect.x, rect.y);
        }
        public override Vector2 GetBottomRight()
        {
            return GetCenter() + Vec.Rotate(GetSize() / 2, GetRotationRad());
        }
        public override void SetTopLeft(Vector2 newPos)
        {
            rect.x = newPos.X + GetWidth() / 2;
            rect.y = newPos.Y + GetHeight() / 2;
        }
        public override void SetCenter(Vector2 newPos)
        {
            rect.X = newPos.X;
            rect.y = newPos.Y;
        }
        public override void SetBottomRight(Vector2 newPos)
        {
            rect.X = newPos.X - GetWidth() / 2;
            rect.y = newPos.Y - GetHeight() / 2;
        }
        public override void SetSize(Vector2 newSize)
        {
            base.SetSize(newSize);
            pivot = newSize / 2;
        }
        public override void Draw(Vector2 devRes, Vector2 stretchFactor)
        {
            //Rectangle scaledRect = Utils.MultiplyRectangle(rect, stretchFactor);
            Vector2 scaledtopLeft = new Vector2(rect.X - rect.width / 2, rect.Y - rect.height / 2) * stretchFactor;
            Vector2 scaledSize = new Vector2(rect.width, rect.height) * stretchFactor;
            Vector2 scaledPivot = scaledSize / 2;
            Rectangle scaledRect = new
                (
                    scaledtopLeft.X + scaledSize.X / 2,//new  center x
                    scaledtopLeft.Y + scaledSize.Y / 2,// new center y
                    scaledSize.X,
                    scaledSize.Y
                );
            if (HasBackground())
            {
                if (HasReservedPart())
                {
                    DrawRectanglePro(scaledRect, scaledPivot, rot, reservedColor);
                    var reservedRect = CalculateProgressRect(scaledRect, 1f - reservedF, new(0f));
                    DrawRectanglePro(reservedRect.rect, reservedRect.pivot, rot, bgColor);
                }
                else DrawRectanglePro(scaledRect, scaledPivot, rot, bgColor);
            }

            if (HasTransition())
            {
                if (transitionF > f)
                {
                    var transitionInfo = CalculateProgressRect(scaledRect, transitionF, barOffset);
                    DrawRectanglePro(transitionInfo.rect, transitionInfo.pivot, rot, transitionColor);
                    if (HasBar())
                    {
                        var progressInfo = CalculateProgressRect(scaledRect, f, barOffset);
                        DrawRectanglePro(progressInfo.rect, progressInfo.pivot, rot, barColor);
                    }
                }
                else if (transitionF < f)
                {
                    var progressInfo = CalculateProgressRect(scaledRect, f, barOffset);
                    DrawRectanglePro(progressInfo.rect, progressInfo.pivot, rot, transitionColor);
                    if (barColor.a > 0)
                    {
                        var transitionInfo = CalculateProgressRect(scaledRect, transitionF, barOffset);
                        DrawRectanglePro(transitionInfo.rect, transitionInfo.pivot, rot, barColor);
                    }
                }
                else
                {
                    if (HasBar())
                    {
                        var progressInfo = CalculateProgressRect(scaledRect, f, barOffset);
                        DrawRectanglePro(progressInfo.rect, progressInfo.pivot, rot, barColor);
                    }
                }
            }
            else
            {
                if (HasBar())
                {
                    var progressInfo = CalculateProgressRect(scaledRect, f, barOffset);
                    DrawRectanglePro(progressInfo.rect, progressInfo.pivot, rot, barColor);
                }
            }
        }

        protected (Rectangle rect, Vector2 pivot) CalculateProgressRect(Rectangle rect, float f, Vector2 barOffset)
        {
            var rectPro = CalculateBarRectPro(rect, f, barOffset);
            return (rectPro, new(rectPro.width / 2, rectPro.height / 2));
        }
        protected Vector2 GetTranslation(float f)
        {
            switch (barType)
            {
                case BarType.LEFTRIGHT:
                    return Vec.Rotate(new(-rect.width * (1.0f - f) * 0.5f, 0f), GetRotationRad());
                case BarType.RIGHTLEFT:
                    return Vec.Rotate(new(rect.width * (1.0f - f) * 0.5f, 0f), GetRotationRad());
                case BarType.TOPBOTTOM:
                    return Vec.Rotate(new(0f, -rect.height * (1.0f - f) * 0.5f), GetRotationRad());
                case BarType.BOTTOMTOP:
                    return Vec.Rotate(new(0f, rect.height * (1.0f - f) * 0.5f), GetRotationRad());
                default:
                    return Vec.Rotate(new(-rect.width * (1.0f - f) * 0.5f, 0f), GetRotationRad());
            }
        }
        protected Rectangle CalculateBarRectPro(Rectangle rect, float f, Vector2 barOffset)
        {
            //var rect = this.rect;
            if (!centered)
            {
                Vector2 translation = GetTranslation(f);
                rect.x += translation.X;
                rect.y += translation.Y;
            }
            rect.x += barOffset.X;
            rect.y += barOffset.Y;

            if (barType == BarType.RIGHTLEFT || barType == BarType.LEFTRIGHT) rect.width *= f;
            else if (barType == BarType.BOTTOMTOP || barType == BarType.TOPBOTTOM) rect.height *= f;

            return rect;
        }
    }

    public class ProgressBar : UIElement
    {
        protected Color bgColor = DARKGRAY;
        protected Color barColor = GREEN;
        protected Color transitionColor = WHITE;
        protected Color outlineColor = GRAY;
        protected Color reservedColor = YELLOW;
        protected BarType barType = BarType.LEFTRIGHT;
        protected float outlineSize = 0f;
        protected float f = 0f;
        protected float reservedF = 0f;
        protected float transitionF = 0f;
        protected float transitionSpeed = 0.1f;
        protected bool centered = false;
        protected Vector2 barOffset = new(0f, 0f);
        protected ProgressBar() { }
        public ProgressBar(Vector2 topLeft, Vector2 size, BarType barType = BarType.LEFTRIGHT, float transitionSpeed = 0.1f, float outlineSize = 0f, bool centered = false)
        {
            rect = new(topLeft.X, topLeft.Y, size.X, size.Y);
            //this.center = new(size.X/2, size.Y / 2);
            this.outlineSize = outlineSize;
            this.barType = barType;
            this.transitionSpeed = transitionSpeed;
            this.centered = centered;
            SetF(1.0f, true);
        }
        public ProgressBar(Vector2 topLeft, Vector2 size, Vector2 barOffset, BarType barType = BarType.LEFTRIGHT, float transitionSpeed = 0.1f, float outlineSize = 0f, bool centered = false)
        {
            rect = new(topLeft.X, topLeft.Y, size.X, size.Y);
            //this.center = new(size.X/2, size.Y / 2);
            this.outlineSize = outlineSize;
            this.barType = barType;
            this.transitionSpeed = transitionSpeed;
            this.centered = centered;
            this.barOffset = barOffset;
            SetF(1.0f, true);
        }
        //public void SetColors(params Color[] colors)
        //{
        //    if (colors.Length <= 0) return;
        //    else if (colors.Length == 1) SetColors(colors[0]);
        //    else if (colors.Length == 2) SetColors(colors[0], colors[1]);
        //    else if (colors.Length == 3) SetColors(colors[0], colors[1], colors[2]);
        //    else if (colors.Length >= 4) SetColors(colors[0], colors[1], colors[2], colors[3]);
        //}
        public void SetColors(Color barColor)
        {
            this.barColor = barColor;
        }
        public void SetColors(Color barColor, Color bgColor)
        {
            this.barColor = barColor;
            this.bgColor = bgColor;
        }
        public void SetColors(Color barColor, Color bgColor, Color reservedColor)
        {
            this.barColor = barColor;
            this.bgColor = bgColor;
            this.reservedColor = reservedColor;
        }
        public void SetColors(Color barColor, Color bgColor, Color reservedColor, Color transitionColor)
        {
            this.barColor = barColor;
            this.bgColor = bgColor;
            this.transitionColor = transitionColor;
            this.reservedColor = reservedColor;
        }
        public void SetColors(Color barColor, Color bgColor, Color reservedColor, Color transitionColor, Color outlineColor)
        {
            this.barColor = barColor;
            this.bgColor = bgColor;
            this.transitionColor = transitionColor;
            this.outlineColor = outlineColor;
            this.reservedColor = reservedColor;
        }
        public void SetTransitionSpeed(float value) { transitionSpeed = value; }
        public void SetReservedF(float value) { reservedF = value; }
        public void SetF(float value, bool setTransitionF = false)
        {
            f = Clamp(value, 0f, 1f);
            if (setTransitionF) transitionF = f;
        }
        public override void Update(float dt, Vector2 mousePosRaw)
        {
            if (transitionSpeed > 0f)
            {
                if (f > transitionF)
                {
                    transitionF = transitionF + MathF.Min(transitionSpeed * dt, f - transitionF);
                }
                else if (f < transitionF)
                {
                    transitionF = transitionF - MathF.Min(transitionSpeed * dt, transitionF - f);
                }
            }
        }

        public bool HasReservedPart() { return reservedColor.a > 0 && reservedF > 0f; }
        public bool HasBackground() { return bgColor.a > 0; }
        public bool HasBar() { return barColor.a > 0 && f > 0f; }
        public bool HasTransition() { return transitionSpeed > 0f && transitionF > 0f && transitionColor.a > 0; }
        public bool HasOutline() { return outlineSize > 0f && outlineColor.a > 0; }
        public override void Draw(Vector2 devRes, Vector2 stretchFactor)
        {
            Rectangle scaledRect = Utils.MultiplyRectangle(rect, stretchFactor);
            if (HasBackground())
            {
                if (HasReservedPart())
                {
                    DrawRectangleRec(scaledRect, reservedColor);
                    DrawRectangleRec(CalculateBarRect(scaledRect, 1f - reservedF), bgColor);
                }
                else DrawRectangleRec(scaledRect, bgColor);
            }

            if (HasTransition())
            {
                if (transitionF > f)
                {
                    DrawRectangleRec(CalculateBarRect(scaledRect, transitionF), transitionColor);
                    if (HasBar()) DrawRectangleRec(CalculateBarRect(scaledRect,f), barColor);
                }
                else if (transitionF < f)
                {
                    DrawRectangleRec(CalculateBarRect(scaledRect, f), transitionColor);
                    if (barColor.a > 0) DrawRectangleRec(CalculateBarRect(scaledRect, transitionF), barColor);
                }
                else
                {
                    if (HasBar()) DrawRectangleRec(CalculateBarRect(scaledRect, f), barColor);
                }
            }
            else
            {
                if (HasBar()) DrawRectangleRec(CalculateBarRect(scaledRect, f), barColor);
            }

            if (HasOutline()) DrawRectangleLinesEx(scaledRect, outlineSize, outlineColor);
        }
        protected Rectangle CalculateBarRect(Rectangle rect, float f)
        {
            //var rect = this.rect;
            if (!centered)
            {
                if (barType == BarType.RIGHTLEFT) rect.X += rect.width * (1.0f - f);
                else if (barType == BarType.BOTTOMTOP) rect.Y += rect.height * (1.0f - f);
            }
            else
            {
                switch (barType)
                {
                    case BarType.LEFTRIGHT: rect.X += rect.width * (1.0f - f) * 0.5f; break;
                    case BarType.RIGHTLEFT: rect.X += rect.width * (1.0f - f) * 0.5f; break;
                    case BarType.TOPBOTTOM: rect.Y += rect.height * (1.0f - f) * 0.5f; break;
                    case BarType.BOTTOMTOP: rect.Y += rect.height * (1.0f - f) * 0.5f; break;
                }
            }
            rect.x += barOffset.X;
            rect.y += barOffset.Y;

            if (barType == BarType.RIGHTLEFT || barType == BarType.LEFTRIGHT) rect.width *= f;
            else if (barType == BarType.BOTTOMTOP || barType == BarType.TOPBOTTOM) rect.height *= f;

            return rect;
        }
    }
}
