using System.Numerics;
using Raylib_CsLo;
using ShapeLib;
using ShapeScreen;
using ShapeColor;

namespace ShapeCore
{
    public class TextEffect : Effect
    {

        protected Color color = WHITE;
        protected string text = "";
        protected float fontSize = 80;
        protected float fontSpacing = 5;
        protected Vector2 textAlignement = new(0.5f);
        protected Font font;

        //public TextEffect(Vector2 pos, string text, float duration, Color color) : base(pos, duration)
        //{
        //    this.text = text;
        //    this.color = color;
        //}
        //public TextEffect(Vector2 pos, string text, float duration, Color color, FontSize fontSize) : base(pos, duration)
        //{
        //    this.text = text;
        //    this.fontSize = UIHandler.GetFontSizeScaled(fontSize);
        //    this.color = color;
        //}
        public TextEffect(Vector2 pos, string text, float duration, Color color, float fontSize, float fontSpacing, Font font, Vector2 alignement) : base(pos, duration)
        {
            this.text = text;
            this.fontSize = fontSize;
            this.fontSpacing = fontSpacing;
            this.textAlignement = alignement;
            this.color = color;
            this.font = font;
        }
        //public TextEffect(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float fontSpacing, Alignement alignement) : base(pos, duration)
        //{
        //    this.text = text;
        //    this.fontSize = UIHandler.GetFontSizeScaled(fontSize);
        //    this.fontSpacing = fontSpacing;
        //    textAlignement = alignement;
        //    this.color = color;
        //}

        public override void DrawUI(Vector2 uiSize)
        {
            if (IsDead()) return;
            Vector2 uiPos = ScreenHandler.TransformPositionToUI(gamePos);
            SDrawing.DrawTextAligned(text, uiPos, fontSize, fontSpacing, color, font, textAlignement);
        }
    }
    public class TextEffectPro : TextEffect
    {
        protected float rot = 0f;

        public TextEffectPro(Vector2 pos, string text, float duration, Color color, float fontSize, Font font) : base(pos, text, duration, color, fontSize, 1, font, new Vector2(0.5f))
        {
            rot = SRNG.randF(-5f, 5f) * DEG2RAD;
        }
        public TextEffectPro(Vector2 pos, string text, float duration, Color color, float rotation, float fontSize, Font font) : base(pos, text, duration, color, fontSize, 1, font, new Vector2(0.5f))
        {
            rot = rotation;
        }
        //public TextEffectPro(Vector2 pos, string text, float duration, Color color, float rotation, FontSize fontSize)
        //    : base(pos, text, duration, color, fontSize)
        //{
        //    rot = rotation;
        //}
        //public TextEffectPro(Vector2 pos, string text, float duration, Color color, FontSize fontSize)
        //    : base(pos, text, duration, color, fontSize)
        //{
        //    rot = RNG.randF(-5f, 5f) * DEG2RAD;
        //}
        public TextEffectPro(Vector2 pos, string text, float duration, Color color, float rotation, float fontSize, float fontSpacing, Font font, Vector2 alignement)
            : base(pos, text, duration, color, fontSize, fontSpacing, font, alignement)
        {
            rot = rotation;
        }

        //public TextEffectPro(Vector2 pos, string text, float duration, Color color, float rotation, FontSize fontSize, float fontSpacing, Alignement alignement)
        //    : base(pos, text, duration, color, fontSize, fontSpacing, alignement)
        //{
        //    rot = rotation;
        //}

        public override void DrawUI(Vector2 uiSize)
        {
            if (IsDead()) return;
            Vector2 uiPos = ScreenHandler.TransformPositionToUI(gamePos);
            SDrawing.DrawTextAlignedPro(text, uiPos, rot, fontSize, fontSpacing, color, font, textAlignement);
        }
    }
    public class TextEffectEase : TextEffect
    {
        protected float easeDuration = 0f;
        protected EasingType easeType;
        protected bool easing = false;

        public TextEffectEase(Vector2 pos, string text, float duration, Color color, float fontSize, Font font, float easeDuration, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, 1, font, new Vector2(0.5f))
        {
            this.easeDuration = easeDuration;
            this.easeType = easeType;
        }
        //public TextEffectEase(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float easeDuration, EasingType easeType = EasingType.LINEAR_OUT)
        //    : base(pos, text, duration, color, fontSize)
        //{
        //    this.easeDuration = easeDuration;
        //    this.easeType = easeType;
        //}
        public TextEffectEase(Vector2 pos, string text, float duration, Color color, float fontSize, float fontSpacing, Font font, Vector2 alignement, float easeDuration, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, font, alignement)
        {
            this.easeDuration = easeDuration;
            this.easeType = easeType;
        }
        //public TextEffectEase(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float fontSpacing, Alignement alignement, float easeDuration, EasingType easeType = EasingType.LINEAR_OUT)
        //    : base(pos, text, duration, color, fontSize, fontSpacing, alignement)
        //{
        //    this.easeDuration = easeDuration;
        //    this.easeType = easeType;
        //}

        public override void Update(float dt)
        {
            if (IsDead()) return;
            lifetimeTimer.Update(dt);
            if (lifetimeTimer.IsFinished() && !easing)
            {
                lifetimeTimer.Start(easeDuration);
                easing = true;
            }

            if (easing) Easing();
        }
        protected virtual void Easing()
        {

        }
        public override bool IsDead()
        {
            return easing && lifetimeTimer.IsFinished();
        }
    }
    public class TextEffectEasePos : TextEffectEase
    {
        protected Vector2 easeFrom;
        protected Vector2 easeChange;

        public TextEffectEasePos(Vector2 pos, string text, float duration, Color color, float fontSize, Font font, float easeDuration, Vector2 easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, font, easeDuration, easeType)
        {
            easeFrom = pos;
            this.easeChange = easeChange;
        }
        //public TextEffectEasePos(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float easeDuration, Vector2 easeChange, EasingType easeType = EasingType.LINEAR_OUT)
        //    : base(pos, text, duration, color, fontSize, easeDuration, easeType)
        //{
        //    easeFrom = pos;
        //    this.easeChange = easeChange;
        //}
        public TextEffectEasePos(Vector2 pos, string text, float duration, Color color, float fontSize, float fontSpacing, Font font, Vector2 alignement, float easeDuration, Vector2 easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, font, alignement, easeDuration, easeType)
        {
            easeFrom = pos;
            this.easeChange = easeChange;
        }
        public TextEffectEasePos(Vector2 pos, Vector2 easeTo, string text, float duration, Color color, float fontSize, float fontSpacing, Font font, Vector2 alignement, float easeDuration, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, font,alignement, easeDuration, easeType)
        {
            this.easeFrom = pos;
            this.easeChange = easeTo - this.easeFrom;
        }
        //public TextEffectEasePos(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float fontSpacing, Alignement alignement, float easeDuration, Vector2 easeChange, EasingType easeType = EasingType.LINEAR_OUT)
        //    : base(pos, text, duration, color, fontSize, fontSpacing, alignement, easeDuration, easeType)
        //{
        //    easeFrom = pos;
        //    this.easeChange = easeChange;
        //}

        protected override void Easing()
        {
            gamePos = SEase.Advanced(easeFrom, easeChange, lifetimeTimer.GetElapsed(), easeDuration, easeType);
        }
    }
    public class TextEffectEaseColor : TextEffectEase
    {
        protected Color easeFrom;
        protected Color easeChange;

        public TextEffectEaseColor(Vector2 pos, string text, float duration, Color color, float fontSize, Font font, float easeDuration, Color easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, font, easeDuration, easeType)
        {
            easeFrom = color;
            this.easeChange = easeChange;
        }
        //public TextEffectEaseColor(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float easeDuration, Color easeChange, EasingType easeType = EasingType.LINEAR_OUT)
        //    : base(pos, text, duration, color, fontSize, easeDuration, easeType)
        //{
        //    easeFrom = color;
        //    this.easeChange = easeChange;
        //}
        public TextEffectEaseColor(Vector2 pos, string text, float duration, Color color, float fontSize, Font font, float fontSpacing, Vector2 alignement, float easeDuration, Color easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, font, alignement, easeDuration, easeType)
        {
            easeFrom = color;
            this.easeChange = easeChange;
        }
        public TextEffectEaseColor(Vector2 pos, string text, float duration, Color color, Color easeTo, float fontSize, float fontSpacing, Font font, Vector2 alignement, float easeDuration, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, font, alignement, easeDuration, easeType)
        {
            this.easeFrom = color;
            this.easeChange = SColor.SubtractColors(easeTo, this.easeFrom);
        }
        //public TextEffectEaseColor(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float fontSpacing, Alignement alignement, float easeDuration, Color easeChange, EasingType easeType = EasingType.LINEAR_OUT)
        //    : base(pos, text, duration, color, fontSize, fontSpacing, alignement, easeDuration, easeType)
        //{
        //    easeFrom = color;
        //    this.easeChange = easeChange;
        //}

        protected override void Easing()
        {
            color = SEase.Advanced(easeFrom, easeChange, lifetimeTimer.GetElapsed(), easeDuration, easeType);
        }
    }
    public class TextEffectEaseSize : TextEffectEase
    {
        protected float easeFrom;
        protected float easeChange;

        public TextEffectEaseSize(Vector2 pos, string text, float duration, Color color, float fontSize, Font font, float easeDuration, float easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, font, easeDuration, easeType)
        {
            easeFrom = fontSize;
            this.easeChange = easeChange;
        }
        //public TextEffectEaseSize(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float easeDuration, float easeChange, EasingType easeType = EasingType.LINEAR_OUT)
        //    : base(pos, text, duration, color, fontSize, easeDuration, easeType)
        //{
        //    easeFrom = this.fontSize;
        //    this.easeChange = UIHandler.GetFontSizeScaled(easeChange);
        //}
        public TextEffectEaseSize(Vector2 pos, string text, float duration, Color color, float fontSize, float fontSpacing, Font font, Vector2 alignement, float easeDuration, float easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, font, alignement, easeDuration, easeType)
        {
            this.easeFrom = fontSize;
            this.easeChange = easeChange;
        }
        public TextEffectEaseSize(Vector2 pos, string text, float duration, Color color, float fontSize, Font font, float easeTo, float fontSpacing, Vector2 alignement, float easeDuration, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, font, alignement, easeDuration, easeType)
        {
            this.easeFrom = fontSize;
            this.easeChange = easeTo - this.easeFrom;
        }
        //public TextEffectEaseSize(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float fontSpacing, Alignement alignement, float easeDuration, float easeChange, EasingType easeType = EasingType.LINEAR_OUT)
        //    : base(pos, text, duration, color, fontSize, fontSpacing, alignement, easeDuration, easeType)
        //{
        //    easeFrom = this.fontSize;
        //    this.easeChange = UIHandler.GetFontSizeScaled(easeChange);
        //}

        protected override void Easing()
        {
            //this.fontSize = Ease.Simple(easeFrom, easeFrom + easeChange, lifetimeTimer.GetF(), easeType);
            fontSize = SEase.Advanced(easeFrom, easeChange, lifetimeTimer.GetElapsed(), easeDuration, easeType);
        }
    }

}
