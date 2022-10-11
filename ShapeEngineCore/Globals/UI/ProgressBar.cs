using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals.UI
{


    public class ProgressBarPro : ProgressBar
    {
        Vector2 pivot;
        float rot = 0f; //in degrees

        protected ProgressBarPro() { }
        public ProgressBarPro(BarType barType = BarType.LEFTRIGHT, float transitionSpeed = 0.1f, float rotation = 0f)
        {
            //correct way of doing it but calculating progress bar must be fixed first
            //this.rect = new(topLeft.X, topLeft.Y, size.X, size.Y);
            //this.pivot = new(0f, 0f);
            //rect = new(topLeft.X + size.X / 2, topLeft.Y + size.Y / 2, size.X, size.Y);
            //pivot = new(size.X / 2, size.Y / 2);
            this.barType = barType;
            this.transitionSpeed = transitionSpeed;
            outlineSizeRelative = 0;
            rot = rotation;
            //this.centered = centered;
            SetF(1.0f, true);
        }
        public ProgressBarPro(Vector2 barOffsetRelative, BarType barType = BarType.LEFTRIGHT, float transitionSpeed = 0.1f, float rotation = 0f)
        {
            //correct way of doing it but calculating progress bar must be fixed first
            //this.rect = new(topLeft.X, topLeft.Y, size.X, size.Y);
            //this.pivot = new(0f, 0f);
            //rect = new(topLeft.X + size.X / 2, topLeft.Y + size.Y / 2, size.X, size.Y);
            //pivot = new(size.X / 2, size.Y / 2);
            this.barType = barType;
            this.transitionSpeed = transitionSpeed;
            outlineSizeRelative = 0;
            rot = rotation;
            //this.centered = centered;
            this.barOffsetRelative = barOffsetRelative;
            SetF(1.0f, true);
        }

        public float GetRotationDeg() { return rot; }
        public float GetRotationRad() { return rot * DEG2RAD; }
        public Vector2 Transform(Vector2 v) { return Vec.Rotate(v, GetRotationRad()); }

        //public override Vector2 GetTopLeft()
        //{
        //    return GetCenter() - Vec.Rotate(GetSize() / 2, GetRotationRad());
        //}
        //public override Vector2 GetCenter()
        //{
        //    return new(rect.x, rect.y);
        //}
        //public override Vector2 GetBottomRight()
        //{
        //    return GetCenter() + Vec.Rotate(GetSize() / 2, GetRotationRad());
        //}
        public override void UpdateRect(Rectangle rect, Alignement alignement = Alignement.CENTER)
        {
            base.UpdateRect(rect);
            pivot = GetSize() / 2;
        }
        public override void UpdateRect(Vector2 center, Vector2 size, Alignement alignement = Alignement.CENTER)
        {
            base.UpdateRect(center, size);
            pivot = size / 2;
        }
        //public override void SetTopLeft(Vector2 newPos)
        //{
        //    rect.x = newPos.X + GetWidth() / 2;
        //    rect.y = newPos.Y + GetHeight() / 2;
        //}
        //public override void SetCenter(Vector2 newPos)
        //{
        //    rect.X = newPos.X;
        //    rect.y = newPos.Y;
        //}
        //public override void SetBottomRight(Vector2 newPos)
        //{
        //    rect.X = newPos.X - GetWidth() / 2;
        //    rect.y = newPos.Y - GetHeight() / 2;
        //}
        //public override void SetSize(Vector2 newSize)
        //{
        //    base.SetSize(newSize);
        //    pivot = newSize / 2;
        //}
        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            //Rectangle scaledRect = Utils.MultiplyRectangle(rect, stretchFactor);
            //Vector2 scaledtopLeft = new Vector2(rect.X - rect.width / 2, rect.Y - rect.height / 2) * stretchFactor;
            //Vector2 scaledSize = new Vector2(rect.width, rect.height) * stretchFactor;
            //Vector2 pivot = scaledSize / 2;
            Rectangle centerRect = GetRect(Alignement.CENTER);
            if (HasBackground())
            {
                if (HasReservedPart())
                {
                    DrawRectanglePro(centerRect, pivot, rot, reservedColor);
                    var reservedRect = CalculateProgressRect(centerRect, 1f - reservedF, new(0f));
                    DrawRectanglePro(reservedRect.rect, reservedRect.pivot, rot, bgColor);
                }
                else DrawRectanglePro(centerRect, pivot, rot, bgColor);
            }

            if (HasTransition())
            {
                if (transitionF > f)
                {
                    var transitionInfo = CalculateProgressRect(centerRect, transitionF, barOffsetRelative);
                    DrawRectanglePro(transitionInfo.rect, transitionInfo.pivot, rot, transitionColor);
                    if (HasBar())
                    {
                        var progressInfo = CalculateProgressRect(centerRect, f, barOffsetRelative);
                        DrawRectanglePro(progressInfo.rect, progressInfo.pivot, rot, barColor);
                    }
                }
                else if (transitionF < f)
                {
                    var progressInfo = CalculateProgressRect(centerRect, f, barOffsetRelative);
                    DrawRectanglePro(progressInfo.rect, progressInfo.pivot, rot, transitionColor);
                    if (barColor.a > 0)
                    {
                        var transitionInfo = CalculateProgressRect(centerRect, transitionF, barOffsetRelative);
                        DrawRectanglePro(transitionInfo.rect, transitionInfo.pivot, rot, barColor);
                    }
                }
                else
                {
                    if (HasBar())
                    {
                        var progressInfo = CalculateProgressRect(centerRect, f, barOffsetRelative);
                        DrawRectanglePro(progressInfo.rect, progressInfo.pivot, rot, barColor);
                    }
                }
            }
            else
            {
                if (HasBar())
                {
                    var progressInfo = CalculateProgressRect(centerRect, f, barOffsetRelative);
                    DrawRectanglePro(progressInfo.rect, progressInfo.pivot, rot, barColor);
                }
            }
        }

        protected (Rectangle rect, Vector2 pivot) CalculateProgressRect(Rectangle rect, float f, Vector2 barOffsetRelative)
        {
            var rectPro = CalculateBarRectPro(rect, f, barOffsetRelative);
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
        protected Rectangle CalculateBarRectPro(Rectangle rect, float f, Vector2 barOffsetRelative)
        {
            //var rect = this.rect;
            if (!centered)
            {
                Vector2 translation = GetTranslation(f);
                rect.x += translation.X;
                rect.y += translation.Y;
            }
            rect.x += barOffsetRelative.X * rect.width;
            rect.y += barOffsetRelative.Y * rect.height;

            if (barType == BarType.RIGHTLEFT || barType == BarType.LEFTRIGHT) rect.width *= f;
            else if (barType == BarType.BOTTOMTOP || barType == BarType.TOPBOTTOM) rect.height *= f;

            return rect;
        }
    }

    public class ProgressRing : ProgressBar
    {
        public ProgressRing(float startAngleRad, float endAngleRad, Vector2 barOffsetRelative, float barRadiusOffset = 0f, float innerRadiusFactor = 0f, float outlineSizeRelative = 0f, bool bgFull = false)
        {

        }
    }
    public class ProgressCircle : ProgressBar
    {

        Alignement alignement = Alignement.CENTER;

        public ProgressCircle(Alignement alignement = Alignement.CENTER, float transitionSpeed = 0.1f)
        {
            this.outlineSizeRelative = 0f;
            this.barOffsetRelative = new(0f);
            this.transitionSpeed = transitionSpeed;
            this.alignement = alignement;
            SetF(1f, true);
        }
        public ProgressCircle(Vector2 barOffsetRelative, Alignement alignement = Alignement.CENTER, float transitionSpeed = 0.1f)
        {
            this.outlineSizeRelative = 0f;
            this.barOffsetRelative = barOffsetRelative;
            this.transitionSpeed = transitionSpeed;
            this.alignement = alignement;
            SetF(1f, true);
        }
        public ProgressCircle(Alignement alignement = Alignement.CENTER, float transitionSpeed = 0.1f, float outlineSize = 0f)
        {
            this.outlineSizeRelative = outlineSize;
            this.barOffsetRelative = new(0f);
            this.transitionSpeed = transitionSpeed;
            this.alignement = alignement;
            SetF(1f, true);
        }
        public ProgressCircle(Vector2 barOffsetRelative, Alignement alignement = Alignement.CENTER, float transitionSpeed = 0.1f, float outlineSize = 0f)
        {
            this.outlineSizeRelative = outlineSize;
            this.barOffsetRelative = barOffsetRelative;
            this.transitionSpeed = transitionSpeed;
            this.alignement = alignement;
            SetF(1f, true);
        }

        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            //Rectangle rect = Utils.MultiplyRectangle(base.rect, stretchFactor);
            float radius = rect.width / 2;
            Vector2 center = GetPos(Alignement.CENTER);


            if (HasBackground())
            {
                if (HasReservedPart())
                {
                    DrawCircleV(center, radius, reservedColor);
                    DrawCircleV(center, radius * MathF.Sqrt(1f - reservedF), bgColor);
                }
                else DrawCircleV(center, radius, bgColor);
            }

            if (HasTransition())
            {
                Vector2 align = UIHandler.GetAlignementVector(alignement) - new Vector2(0.5f, 0.5f);
                Vector2 alignPos = center + align * radius * 2f;
                Vector2 offset = barOffsetRelative * GetSize();

                if (transitionF > f)
                {
                    DrawCircleV(Vec.Lerp(alignPos, center, transitionF) + offset , radius * transitionF, transitionColor);
                    if (HasBar()) DrawCircleV(Vec.Lerp(alignPos, center, f) + offset, radius * f, barColor);
                }
                else if (transitionF < f)
                {
                    DrawCircleV(Vec.Lerp(alignPos, center, f) + offset , radius * f, transitionColor);
                    if (HasBar()) DrawCircleV(Vec.Lerp(alignPos, center, transitionF) + offset, radius * transitionF, barColor);
                }
                else
                {
                    if (HasBar()) DrawCircleV(Vec.Lerp(alignPos, center, f) + offset, radius * f, barColor);
                }
            }
            else
            {
                Vector2 align = UIHandler.GetAlignementVector(alignement) - new Vector2(0.5f, 0.5f);
                Vector2 alignPos = center + align * radius * 2f;
                Vector2 offset = barOffsetRelative * GetSize();
                if (HasBar()) DrawCircleV(Vec.Lerp(alignPos, center, f) + offset , radius * f, barColor);
            }

            if (HasOutline()) Drawing.DrawCircleLines(center, radius * 1.01f, outlineSizeRelative * MathF.Max(rect.width, rect.height), outlineColor);
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
        protected float outlineSizeRelative = 0f;
        protected float f = 0f;
        protected float reservedF = 0f;
        protected float transitionF = 0f;
        protected float transitionSpeed = 0.1f;
        protected bool centered = false;
        protected Vector2 barOffsetRelative = new(0f, 0f);
        protected ProgressBar() { }
        public ProgressBar(BarType barType = BarType.LEFTRIGHT, float transitionSpeed = 0.1f, float outlineSizeRelative = 0f)
        {
            this.outlineSizeRelative = outlineSizeRelative;
            this.barType = barType;
            this.transitionSpeed = transitionSpeed;
            SetF(1.0f, true);
        }
        public ProgressBar(Vector2 barOffsetRelative, BarType barType = BarType.LEFTRIGHT, float transitionSpeed = 0.1f, float outlineSizeRelative = 0f)
        {
            this.outlineSizeRelative = outlineSizeRelative;
            this.barType = barType;
            this.transitionSpeed = transitionSpeed;
            this.barOffsetRelative = barOffsetRelative;
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
        public bool HasOutline() { return outlineSizeRelative > 0f && outlineColor.a > 0; }
        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            //Rectangle rect = Utils.MultiplyRectangle(base.rect, stretchFactor);
            if (HasBackground())
            {
                if (HasReservedPart())
                {
                    DrawRectangleRec(rect, reservedColor);
                    DrawRectangleRec(CalculateBarRect(rect, 1f - reservedF), bgColor);
                }
                else DrawRectangleRec(rect, bgColor);
            }

            if (HasTransition())
            {
                if (transitionF > f)
                {
                    DrawRectangleRec(CalculateBarRect(rect, transitionF), transitionColor);
                    if (HasBar()) DrawRectangleRec(CalculateBarRect(rect,f), barColor);
                }
                else if (transitionF < f)
                {
                    DrawRectangleRec(CalculateBarRect(rect, f), transitionColor);
                    if (barColor.a > 0) DrawRectangleRec(CalculateBarRect(rect, transitionF), barColor);
                }
                else
                {
                    if (HasBar()) DrawRectangleRec(CalculateBarRect(rect, f), barColor);
                }
            }
            else
            {
                if (HasBar()) DrawRectangleRec(CalculateBarRect(rect, f), barColor);
            }

            if (HasOutline()) DrawRectangleLinesEx(rect, outlineSizeRelative * MathF.Max(rect.width, rect.height), outlineColor);
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
            rect.x += barOffsetRelative.X * rect.width;
            rect.y += barOffsetRelative.Y * rect.height;

            if (barType == BarType.RIGHTLEFT || barType == BarType.LEFTRIGHT) rect.width *= f;
            else if (barType == BarType.BOTTOMTOP || barType == BarType.TOPBOTTOM) rect.height *= f;

            return rect;
        }
    }
}
