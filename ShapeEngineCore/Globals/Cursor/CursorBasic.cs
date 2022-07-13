using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals.Cursor
{
    public class CursorBasic
    {
        protected float size = 20f;
        protected Color color = WHITE;
        public string Name { get; set; } = "";
        public CursorBasic() { }
        public CursorBasic(float size, Color color)
        {
            this.size = size;
            this.color = color;
        }

        public virtual void Draw(Vector2 mousePos)
        {
            Vector2 tip = mousePos;
            Vector2 right = mousePos + Vec.Rotate(Vec.Right() * size, 90.0f * DEG2RAD);
            Vector2 left = mousePos + Vec.Rotate(Vec.Right() * size, 135.0f * DEG2RAD);
            DrawTriangle(tip, left, right, color);

        }
    }
    public class CursorNull : CursorBasic
    {
        public CursorNull() { }
        public override void Draw(Vector2 mousePos) { }
    }
    public class CursorGame : CursorBasic
    {
        public CursorGame(float size, Color color) : base(size, color) { }

        public override void Draw(Vector2 mousePos)
        {
            DrawLineEx(mousePos + new Vector2(-size / 2, -size / 2), mousePos + new Vector2(-size / 4, -size / 2), 4.0f, color);
            DrawLineEx(mousePos + new Vector2(-size / 2, -size / 2), mousePos + new Vector2(-size / 2, -size / 4), 4.0f, color);

            DrawLineEx(mousePos + new Vector2(size / 2, -size / 2), mousePos + new Vector2(size / 4, -size / 2), 4.0f, color);
            DrawLineEx(mousePos + new Vector2(size / 2, -size / 2), mousePos + new Vector2(size / 2, -size / 4), 4.0f, color);

            DrawLineEx(mousePos + new Vector2(-size / 2, size / 2), mousePos + new Vector2(-size / 4, size / 2), 4.0f, color);
            DrawLineEx(mousePos + new Vector2(-size / 2, size / 2), mousePos + new Vector2(-size / 2, size / 4), 4.0f, color);

            DrawLineEx(mousePos + new Vector2(size / 2, size / 2), mousePos + new Vector2(size / 4, size / 2), 4.0f, color);
            DrawLineEx(mousePos + new Vector2(size / 2, size / 2), mousePos + new Vector2(size / 2, size / 4), 4.0f, color);

        }
    }

}
