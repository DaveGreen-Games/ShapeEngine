using System.Numerics;
using ShapeEngine.Text;

namespace ShapeEngine.Effects
{
    public class TextEffect : Effect
    {
        public string Text { get; set; } = "";

        public TextEffect(Vector2 pos, Vector2 size, float rotRad, string text) : base(pos, size, rotRad) { this.Text = text; }
        public TextEffect(Vector2 pos, Vector2 size, float rotRad, string text, float lifeTime) : base(pos, size, rotRad, lifeTime) { this.Text = text; }

        protected void DrawText(TextFont textFont, Vector2 alignement)
        {
            textFont.DrawTextWrapNone(Text, GetBoundingBox(), alignement);
        }
    }
}
