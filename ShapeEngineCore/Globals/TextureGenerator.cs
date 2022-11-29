using Raylib_CsLo;
using ShapeEngineCore.Globals.UI;
using System.Numerics;

namespace ShapeEngineCore.Globals
{
    public static class TextureGenerator
    {

        public static RenderTexture DrawAction(int w, int h, Color bgColor, Action drawAction)
        {
            RenderTexture rt = LoadRenderTexture(w, h);
            BeginTextureMode(rt);
            ClearBackground(bgColor);
            drawAction();
            EndTextureMode();
            return rt;
        }
        public static RenderTexture Circle(int radius, Color color)
        {
            int w = radius * 2;
            int h = radius * 2;
            RenderTexture rt = LoadRenderTexture(w, h);
            BeginTextureMode(rt);
            DrawCircle(w / 2, h / 2, radius, color);
            EndTextureMode();

            return rt;
        }

        
        public static RenderTexture GenerateCheckeredLineCircle(float radius, float spacing, float lineThickness, float angleDeg, Color lineColor, Color bgColor)
        {

            float maxDimension = radius;
            Vector2 size = new Vector2(radius, radius) * 2f;
            Vector2 center = size / 2;
            float rotRad = angleDeg * DEG2RAD;

            RenderTexture rt = LoadRenderTexture((int)size.X, (int)size.Y);
            BeginTextureMode(rt);

            if (bgColor.a > 0) DrawCircleV(center, radius, bgColor);

            Vector2 cur = new(-spacing / 2, 0f);
            while (cur.X > -maxDimension)
            {
                Vector2 p = center + Vec.Rotate(cur, rotRad);

                //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
                float angle = MathF.Acos(cur.X / radius);
                float y = radius * MathF.Sin(angle);

                Vector2 up = new(0f, -y);
                Vector2 down = new(0f, y);
                Vector2 start = p + Vec.Rotate(up, rotRad);
                Vector2 end = p + Vec.Rotate(down, rotRad);
                DrawLineEx(start, end, lineThickness, lineColor);
                cur.X -= spacing;
            }

            cur = new(spacing / 2, 0f);
            while (cur.X < maxDimension)
            {
                Vector2 p = center + Vec.Rotate(cur, rotRad);
                //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
                float angle = MathF.Acos(cur.X / radius);
                float y = radius * MathF.Sin(angle);

                Vector2 up = new(0f, -y);
                Vector2 down = new(0f, y);
                Vector2 start = p + Vec.Rotate(up, rotRad);
                Vector2 end = p + Vec.Rotate(down, rotRad);
                DrawLineEx(start, end, lineThickness, lineColor);
                cur.X += spacing;
            }

            EndTextureMode();
            return rt;
        }
        public static RenderTexture GenerateCheckeredLinesRect(Vector2 size, float spacing, float lineThickness, float angleDeg, Color lineColor, Color bgColor)
        {
            //Rectangle rect = new(0, 0, size.X, size.Y);
            float maxDimension = MathF.Max(size.X, size.Y);
            Vector2 center = size / 2;
            float rotRad = angleDeg * DEG2RAD;

            RenderTexture rt = LoadRenderTexture((int)size.X, (int)size.Y);
            BeginTextureMode(rt);

            if (bgColor.a > 0) DrawRectangleRec(new(0, 0, size.X, size.Y), bgColor);

            Vector2 cur = new(-spacing / 2, 0f);
            while (cur.X > -maxDimension)
            {
                Vector2 p = center + Vec.Rotate(cur, rotRad);
                Vector2 up = new(0f, -maxDimension * 2);
                Vector2 down = new(0f, maxDimension * 2);
                Vector2 start = p + Vec.Rotate(up, rotRad);
                Vector2 end = p + Vec.Rotate(down, rotRad);
                DrawLineEx(start, end, lineThickness, lineColor);
                cur.X -= spacing;
            }

            cur = new(spacing / 2, 0f);
            while (cur.X < maxDimension)
            {
                Vector2 p = center + Vec.Rotate(cur, rotRad);
                Vector2 up = new(0f, -maxDimension * 2);
                Vector2 down = new(0f, maxDimension * 2);
                Vector2 start = p + Vec.Rotate(up, rotRad);
                Vector2 end = p + Vec.Rotate(down, rotRad);
                DrawLineEx(start, end, lineThickness, lineColor);
                cur.X += spacing;
            }

            EndTextureMode();
            return rt;
        }

        public static void Stamp(Texture texture, Vector2 pos, float rotDeg, float scale, Color tint, Alignement alignement = Alignement.CENTER)
        {
            Vector2 size = new(texture.width, texture.height);
            Vector2 aVector = UIHandler.GetAlignementVector(alignement);// + new Vector2(-0.5f, -0.5f);

            Rectangle sourceRec = new Rectangle(0, 0, size.X, -size.Y);
            Vector2 origin = aVector * size * scale;
            Rectangle destRec = new Rectangle(pos.X, pos.Y, size.X * scale, size.Y * scale);

            DrawTexturePro(texture, sourceRec, destRec, origin, rotDeg, tint);
        }
    }
}
