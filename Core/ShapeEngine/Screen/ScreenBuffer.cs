using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngine.Screen
{

    internal sealed class ScreenBufferArray
    {
        //private List<ScreenBuffer> screenBuffers = new();
        public ScreenBuffer? A { get; private set; }
        public ScreenBuffer? B { get; private set; }
        public bool Loaded { get; private set; } = false;
        public ScreenBuffer? GetByIndex(int index)
        {
            if (!Loaded) return null;
            if (index == 0) return A;
            if (index == 1) return B;
            else return null;
        }
        public void Load(int width, int height)
        {
            
            if (Loaded) Unload();

            Loaded = true;
            A = new ScreenBuffer(width, height);
            B = new ScreenBuffer(width, height);
        }
        public void Unload()
        {
            if (!Loaded) return;
            Loaded = false;

            A.Unload();
            B.Unload();

            A = null;
            B = null;
        }

    }

    internal sealed class ScreenBuffer
    {
        private Raylib_CsLo.Color clearColor = new(0, 0, 0, 255);
        private RenderTexture texture;
        private Rectangle sourceRec;
        //private Rectangle destRec;
        //private Vector2 origin;

        public ScreenBuffer(int width, int height)//, int targetWidth, int targetHeight)
        {
            texture = LoadRenderTexture(width, height);

            sourceRec = new Rectangle(0, 0, width, -height);
            //destRec = new Rectangle(targetWidth / 2, targetHeight / 2, targetWidth, targetHeight);
            //origin = new Vector2(targetWidth / 2, targetHeight / 2);
        }

        public void StartTextureMode()
        {
            BeginTextureMode(texture);
            ClearBackground(clearColor);
        }
        public void EndTextureMode()
        {
            Raylib.EndTextureMode();
        }

        public void DrawTexture(int targetWidth, int targetHeight, int blendMode = -1)
        {

            Vector2 origin = new Vector2(targetWidth / 2, targetHeight / 2);
            Vector2 size = GetDestRectSize(targetWidth, targetHeight);
            float w = size.X;
            float h = size.Y;
            Rectangle destRec = new();
            destRec.x = targetWidth * 0.5f;
            destRec.y = targetHeight * 0.5f;
            destRec.width = w;
            destRec.height = h;
            origin.X = w * 0.5f;
            origin.Y = h * 0.5f;


            if (blendMode < 0)
            {
                DrawTexturePro(texture.texture, sourceRec, destRec, origin, 0f, clearColor);
            }
            else
            {
                BeginBlendMode(blendMode);
                DrawTexturePro(texture.texture, sourceRec, destRec, origin, 0f, clearColor);
                EndBlendMode();
            }




        }

        private Vector2 GetDestRectSize(int width, int height)
        {
            float w, h;
            float fWidth = width / (float)texture.texture.width;
            float fHeight = height / (float)texture.texture.height;
            if (fWidth <= fHeight)
            {
                w = width;
                float f = texture.texture.height / (float)texture.texture.width;
                h = w * f;
            }
            else
            {
                h = height;
                float f = texture.texture.width / (float)texture.texture.height;
                w = h * f;
            }
            return new(w, h);
        }
        public void Unload()
        {
            UnloadRenderTexture(texture);
        }
    }

}
