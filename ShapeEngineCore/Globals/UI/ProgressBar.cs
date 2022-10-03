using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals.UI
{


    public class ProgressBarPro : ProgressBar
    {
        Vector2 pivotRelative;
        float rotDeg = 0f; //in degrees

        protected ProgressBarPro() { }
        public ProgressBarPro(Vector2 topLeftRelative, Vector2 sizeRelative, BarType barType = BarType.LEFTRIGHT, float transitionSpeed = 0.1f, float rotDeg = 0f, bool centered = false)
        {
            //correct way of doing it but calculating progress bar must be fixed first
            //this.rect = new(topLeft.X, topLeft.Y, size.X, size.Y);
            //this.pivot = new(0f, 0f);
            rect = new(topLeftRelative.X + sizeRelative.X / 2, topLeftRelative.Y + sizeRelative.Y / 2, sizeRelative.X, sizeRelative.Y);
            pivotRelative = new(sizeRelative.X / 2, sizeRelative.Y / 2);
            this.barType = barType;
            this.transitionSpeed = transitionSpeed;
            outlineSizeRelative = 0;
            this.rotDeg = rotDeg;
            this.centered = centered;
            SetF(1.0f, true);
        }
        public ProgressBarPro(Vector2 topLeftRelative, Vector2 sizeRelative, Vector2 barOffsetRelative, BarType barType = BarType.LEFTRIGHT, float transitionSpeed = 0.1f, float rotDeg = 0f, bool centered = false)
        {
            //correct way of doing it but calculating progress bar must be fixed first
            //this.rect = new(topLeft.X, topLeft.Y, size.X, size.Y);
            //this.pivot = new(0f, 0f);
            rect = new(topLeftRelative.X + sizeRelative.X / 2, topLeftRelative.Y + sizeRelative.Y / 2, sizeRelative.X, sizeRelative.Y);
            pivotRelative = new(sizeRelative.X / 2, sizeRelative.Y / 2);
            this.barType = barType;
            this.transitionSpeed = transitionSpeed;
            outlineSizeRelative = 0;
            this.rotDeg = rotDeg;
            this.centered = centered;
            this.barOffsetRelative = barOffsetRelative;
            SetF(1.0f, true);
        }

        public float GetRotationDeg() { return rotDeg; }
        public float GetRotationRad() { return rotDeg * DEG2RAD; }
        public Vector2 Transform(Vector2 v) { return Vec.Rotate(v, GetRotationRad()); }

        public override Vector2 GetTopLeft(bool absolute = true)
        {
            if(absolute) return GetCenter(true) - Vec.Rotate(GetSize(true) / 2, GetRotationRad());
            else return GetCenter(false) - Vec.Rotate(GetSize(false) / 2, GetRotationRad());
        }
        public override Vector2 GetCenter(bool absolute = true)
        {
            if (absolute) return ToAbsolute(new Vector2(rect.x, rect.y));
            else return new(rect.x, rect.y);
        }
        public override Vector2 GetBottomRight(bool absolute = true)
        {
            if(absolute) return GetCenter(true) + Vec.Rotate(GetSize(true) / 2, GetRotationRad());
            else return GetCenter(false) + Vec.Rotate(GetSize(false) / 2, GetRotationRad());
        }


        public override void SetTopLeft(Vector2 newPosRelative)
        {
            rect.x = newPosRelative.X + GetWidth(false) / 2;
            rect.y = newPosRelative.Y + GetHeight(false) / 2;
        }
        public override void SetCenter(Vector2 newPosRelative)
        {
            rect.X = newPosRelative.X;
            rect.y = newPosRelative.Y;
        }
        public override void SetBottomRight(Vector2 newPosRelative)
        {
            rect.X = newPosRelative.X - GetWidth(false) / 2;
            rect.y = newPosRelative.Y - GetHeight(false) / 2;
        }

        public override void Draw()
        {
            if (HasBackground())
            {
                if (HasReservedPart())
                {
                    DrawRectanglePro(ToAbsolute(rect), ToAbsolute(pivotRelative), rotDeg, reservedColor);
                    var reservedRect = CalculateProgressRectRelative(1f - reservedF, new(0f));
                    DrawRectanglePro(ToAbsolute(reservedRect.rect), ToAbsolute(reservedRect.pivot), rotDeg, bgColor);
                }
                else DrawRectanglePro(ToAbsolute(rect), ToAbsolute(pivotRelative), rotDeg, bgColor);
            }

            if (HasTransition())
            {
                if (transitionF > f)
                {
                    var transitionInfo = CalculateProgressRectRelative(transitionF, barOffsetRelative);
                    DrawRectanglePro(ToAbsolute(transitionInfo.rect), ToAbsolute(transitionInfo.pivot), rotDeg, transitionColor);
                    if (HasBar())
                    {
                        var progressInfo = CalculateProgressRectRelative(f, barOffsetRelative);
                        DrawRectanglePro(ToAbsolute(progressInfo.rect), ToAbsolute(progressInfo.pivot), rotDeg, barColor);
                    }
                }
                else if (transitionF < f)
                {
                    var progressInfo = CalculateProgressRectRelative(f, barOffsetRelative);
                    DrawRectanglePro(ToAbsolute(progressInfo.rect), ToAbsolute(progressInfo.pivot), rotDeg, transitionColor);
                    if (barColor.a > 0)
                    {
                        var transitionInfo = CalculateProgressRectRelative(transitionF, barOffsetRelative);
                        DrawRectanglePro(ToAbsolute(transitionInfo.rect), ToAbsolute(transitionInfo.pivot), rotDeg, barColor);
                    }
                }
                else
                {
                    if (HasBar())
                    {
                        var progressInfo = CalculateProgressRectRelative(f, barOffsetRelative);
                        DrawRectanglePro(ToAbsolute(progressInfo.rect), ToAbsolute(progressInfo.pivot), rotDeg, barColor);
                    }
                }
            }
            else
            {
                if (HasBar())
                {
                    var progressInfo = CalculateProgressRectRelative(f, barOffsetRelative);
                    DrawRectanglePro(ToAbsolute(progressInfo.rect), ToAbsolute(progressInfo.pivot), rotDeg, barColor);
                }
            }
        }

        protected (Rectangle rect, Vector2 pivot) CalculateProgressRectRelative(float f, Vector2 barOffsetRelative)
        {
            var rect = CalculateBarRectProRelative(f, barOffsetRelative);
            return (rect, new(rect.width / 2, rect.height / 2));
        }
        protected Vector2 GetTranslationRelative(float f)
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
        protected Rectangle CalculateBarRectProRelative(float f, Vector2 barOffsetRelative)
        {
            var rect = this.rect;
            if (!centered)
            {
                Vector2 translation = GetTranslationRelative(f);
                rect.x += translation.X;
                rect.y += translation.Y;
            }
            rect.x += barOffsetRelative.X;
            rect.y += barOffsetRelative.Y;

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
        protected float outlineSizeRelative = 0f;
        protected float f = 0f;
        protected float reservedF = 0f;
        protected float transitionF = 0f;
        protected float transitionSpeed = 0.1f;
        protected bool centered = false;
        protected Vector2 barOffsetRelative = new(0f, 0f);
        protected ProgressBar() { }
        public ProgressBar(Vector2 topLeftRelative, Vector2 sizeRelative, BarType barType = BarType.LEFTRIGHT, float transitionSpeed = 0.1f, float outlineSizeRelative = 0f, bool centered = false)
        {
            rect = new(topLeftRelative.X, topLeftRelative.Y, sizeRelative.X, sizeRelative.Y);
            //this.center = new(size.X/2, size.Y / 2);
            this.outlineSizeRelative = outlineSizeRelative;
            this.barType = barType;
            this.transitionSpeed = transitionSpeed;
            this.centered = centered;
            SetF(1.0f, true);
        }
        public ProgressBar(Vector2 topLeftRelative, Vector2 sizeRelative, Vector2 barOffsetRelative, BarType barType = BarType.LEFTRIGHT, float transitionSpeed = 0.1f, float outlineSizeRelative = 0f, bool centered = false)
        {
            rect = new(topLeftRelative.X, topLeftRelative.Y, sizeRelative.X, sizeRelative.Y);
            //this.center = new(size.X/2, size.Y / 2);
            this.outlineSizeRelative = outlineSizeRelative;
            this.barType = barType;
            this.transitionSpeed = transitionSpeed;
            this.centered = centered;
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
        public override void Update(float dt, Vector2 mousePos)
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
        public override void Draw()
        {
            if (HasBackground())
            {
                if (HasReservedPart())
                {
                    DrawRectangleRec(ToAbsolute(rect), reservedColor);
                    DrawRectangleRec(ToAbsolute(CalculateBarRectRelative(1f - reservedF)), bgColor);
                }
                else DrawRectangleRec(ToAbsolute(rect), bgColor);
            }

            if (HasTransition())
            {
                if (transitionF > f)
                {
                    DrawRectangleRec(ToAbsolute( CalculateBarRectRelative(transitionF) ), transitionColor);
                    if (HasBar()) DrawRectangleRec(ToAbsolute( CalculateBarRectRelative(f) ), barColor);
                }
                else if (transitionF < f)
                {
                    DrawRectangleRec(ToAbsolute( CalculateBarRectRelative(f) ), transitionColor);
                    if (barColor.a > 0) DrawRectangleRec(ToAbsolute( CalculateBarRectRelative(transitionF) ), barColor);
                }
                else
                {
                    if (HasBar()) DrawRectangleRec(ToAbsolute( CalculateBarRectRelative(f) ), barColor);
                }
            }
            else
            {
                if (HasBar()) DrawRectangleRec(ToAbsolute( CalculateBarRectRelative(f) ), barColor);
            }

            if (HasOutline()) DrawRectangleLinesEx(ToAbsolute(rect), outlineSizeRelative * ToAbsolute(rect).width, outlineColor);
        }
        protected Rectangle CalculateBarRectRelative(float f)
        {
            var rect = this.rect;
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
            rect.x += barOffsetRelative.X;
            rect.y += barOffsetRelative.Y;

            if (barType == BarType.RIGHTLEFT || barType == BarType.LEFTRIGHT) rect.width *= f;
            else if (barType == BarType.BOTTOMTOP || barType == BarType.TOPBOTTOM) rect.height *= f;

            return rect;
        }
    }
}
