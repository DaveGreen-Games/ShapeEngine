﻿using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Screen;
using ShapeEngineCore.Globals.UI;

namespace ShapeEngineCore
{
    public class TextEffect : Effect
    {

        protected Color color = WHITE;
        protected string text = "";
        protected float fontSize = UIHandler.GetFontSizeScaled(FontSize.MEDIUM);
        protected float fontSpacing = UIHandler.Scale(2f);
        protected Alignement textAlignement = Alignement.CENTER;

        public TextEffect(Vector2 pos, string text, float duration, Color color) : base(pos, duration)
        {
            this.text = text;
            this.color = color;
        }
        public TextEffect(Vector2 pos, string text, float duration, Color color, FontSize fontSize) : base(pos, duration)
        {
            this.text = text;
            this.fontSize = UIHandler.GetFontSizeScaled(fontSize);
            this.color = color;
        }
        public TextEffect(Vector2 pos, string text, float duration, Color color, float fontSize, float fontSpacing, Alignement alignement) : base(pos, duration)
        {
            this.text = text;
            this.fontSize = fontSize;
            this.fontSpacing = fontSpacing;
            textAlignement = alignement;
            this.color = color;
        }
        public TextEffect(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float fontSpacing, Alignement alignement) : base(pos, duration)
        {
            this.text = text;
            this.fontSize = UIHandler.GetFontSizeScaled(fontSize);
            this.fontSpacing = fontSpacing;
            textAlignement = alignement;
            this.color = color;
        }

        public override void DrawUI()
        {
            if (IsDead()) return;
            Vector2 p = ScreenHandler.TransformPositionToUI(pos);
            UIHandler.DrawTextAligned(text, p, fontSize, fontSpacing, color, textAlignement);
        }
    }
    public class TextEffectPro : TextEffect
    {
        protected float rot = 0f;

        public TextEffectPro(Vector2 pos, string text, float duration, Color color) : base(pos, text, duration, color)
        {
            rot = RNG.randF(-5f, 5f) * DEG2RAD;
        }
        public TextEffectPro(Vector2 pos, string text, float duration, Color color, float rotation) : base(pos, text, duration, color)
        {
            rot = rotation;
        }
        public TextEffectPro(Vector2 pos, string text, float duration, Color color, float rotation, FontSize fontSize)
            : base(pos, text, duration, color, fontSize)
        {
            rot = rotation;
        }
        public TextEffectPro(Vector2 pos, string text, float duration, Color color, FontSize fontSize)
            : base(pos, text, duration, color, fontSize)
        {
            rot = RNG.randF(-5f, 5f) * DEG2RAD;
        }
        public TextEffectPro(Vector2 pos, string text, float duration, Color color, float rotation, float fontSize, float fontSpacing, Alignement alignement)
            : base(pos, text, duration, color, fontSize, fontSpacing, alignement)
        {
            rot = rotation;
        }
        public TextEffectPro(Vector2 pos, string text, float duration, Color color, float rotation, FontSize fontSize, float fontSpacing, Alignement alignement)
            : base(pos, text, duration, color, fontSize, fontSpacing, alignement)
        {
            rot = rotation;
        }

        public override void DrawUI()
        {
            if (IsDead()) return;
            UIHandler.DrawTextAlignedPro(text, pos, rot, fontSize, fontSpacing, color, textAlignement);
        }
    }
    public class TextEffectEase : TextEffect
    {
        protected float easeDuration = 0f;
        protected EasingType easeType;
        protected bool easing = false;

        public TextEffectEase(Vector2 pos, string text, float duration, Color color, float easeDuration, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color)
        {
            this.easeDuration = easeDuration;
            this.easeType = easeType;
        }
        public TextEffectEase(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float easeDuration, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize)
        {
            this.easeDuration = easeDuration;
            this.easeType = easeType;
        }
        public TextEffectEase(Vector2 pos, string text, float duration, Color color, float fontSize, float fontSpacing, Alignement alignement, float easeDuration, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, alignement)
        {
            this.easeDuration = easeDuration;
            this.easeType = easeType;
        }
        public TextEffectEase(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float fontSpacing, Alignement alignement, float easeDuration, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, alignement)
        {
            this.easeDuration = easeDuration;
            this.easeType = easeType;
        }

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

        public TextEffectEasePos(Vector2 pos, string text, float duration, Color color, float easeDuration, Vector2 easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, easeDuration, easeType)
        {
            easeFrom = pos;
            this.easeChange = easeChange;
        }
        public TextEffectEasePos(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float easeDuration, Vector2 easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, easeDuration, easeType)
        {
            easeFrom = pos;
            this.easeChange = easeChange;
        }
        public TextEffectEasePos(Vector2 pos, string text, float duration, Color color, float fontSize, float fontSpacing, Alignement alignement, float easeDuration, Vector2 easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, alignement, easeDuration, easeType)
        {
            easeFrom = pos;
            this.easeChange = easeChange;
        }
        public TextEffectEasePos(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float fontSpacing, Alignement alignement, float easeDuration, Vector2 easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, alignement, easeDuration, easeType)
        {
            easeFrom = pos;
            this.easeChange = easeChange;
        }

        protected override void Easing()
        {
            pos = Ease.Advanced(easeFrom, easeChange, lifetimeTimer.GetElapsed(), easeDuration, easeType);
        }
    }
    public class TextEffectEaseColor : TextEffectEase
    {
        protected Color easeFrom;
        protected Color easeChange;

        public TextEffectEaseColor(Vector2 pos, string text, float duration, Color color, float easeDuration, Color easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, easeDuration, easeType)
        {
            easeFrom = color;
            this.easeChange = easeChange;
        }
        public TextEffectEaseColor(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float easeDuration, Color easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, easeDuration, easeType)
        {
            easeFrom = color;
            this.easeChange = easeChange;
        }
        public TextEffectEaseColor(Vector2 pos, string text, float duration, Color color, float fontSize, float fontSpacing, Alignement alignement, float easeDuration, Color easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, alignement, easeDuration, easeType)
        {
            easeFrom = color;
            this.easeChange = easeChange;
        }
        public TextEffectEaseColor(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float fontSpacing, Alignement alignement, float easeDuration, Color easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, alignement, easeDuration, easeType)
        {
            easeFrom = color;
            this.easeChange = easeChange;
        }

        protected override void Easing()
        {
            color = Ease.Advanced(easeFrom, easeChange, lifetimeTimer.GetElapsed(), easeDuration, easeType);
        }
    }
    public class TextEffectEaseSize : TextEffectEase
    {
        protected float easeFrom;
        protected float easeChange;

        public TextEffectEaseSize(Vector2 pos, string text, float duration, Color color, float easeDuration, float easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, easeDuration, easeType)
        {
            easeFrom = fontSize;
            this.easeChange = UIHandler.GetFontSizeScaled(easeChange);
        }
        public TextEffectEaseSize(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float easeDuration, float easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, easeDuration, easeType)
        {
            easeFrom = this.fontSize;
            this.easeChange = UIHandler.GetFontSizeScaled(easeChange);
        }
        public TextEffectEaseSize(Vector2 pos, string text, float duration, Color color, float fontSize, float fontSpacing, Alignement alignement, float easeDuration, float easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, alignement, easeDuration, easeType)
        {
            easeFrom = this.fontSize;
            this.easeChange = easeChange;
        }
        public TextEffectEaseSize(Vector2 pos, string text, float duration, Color color, FontSize fontSize, float fontSpacing, Alignement alignement, float easeDuration, float easeChange, EasingType easeType = EasingType.LINEAR_OUT)
            : base(pos, text, duration, color, fontSize, fontSpacing, alignement, easeDuration, easeType)
        {
            easeFrom = this.fontSize;
            this.easeChange = UIHandler.GetFontSizeScaled(easeChange);
        }

        protected override void Easing()
        {
            //this.fontSize = Ease.Simple(easeFrom, easeFrom + easeChange, lifetimeTimer.GetF(), easeType);
            fontSize = Ease.Advanced(easeFrom, easeChange, lifetimeTimer.GetElapsed(), easeDuration, easeType);
        }
    }

}
