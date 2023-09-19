using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Core;

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
        public void Load(Dimensions dimensions)
        {
            
            if (Loaded) Unload();

            Loaded = true;
            A = new ScreenBuffer(dimensions);
            B = new ScreenBuffer(dimensions);
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
        public ScreenBuffer(Dimensions dimensions)//, int targetWidth, int targetHeight)
        {
            texture = LoadRenderTexture(dimensions.Width, dimensions.Height);

            sourceRec = new Rectangle(0, 0, dimensions.Width, -dimensions.Height);
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

        public void DrawTexture(Dimensions targetdimensions, int blendMode = -1)
        {

            Vector2 origin = new Vector2(targetdimensions.Width / 2, targetdimensions.Height / 2);
            Vector2 size = GetDestRectSize(targetdimensions);
            float w = size.X;
            float h = size.Y;
            Rectangle destRec = new();
            destRec.x = targetdimensions.Width * 0.5f;
            destRec.y = targetdimensions.Height * 0.5f;
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

        private Vector2 GetDestRectSize(Dimensions dimensions)
        {
            float w, h;
            float fWidth = dimensions.Width / (float)texture.texture.width;
            float fHeight = dimensions.Width / (float)texture.texture.height;
            if (fWidth <= fHeight)
            {
                w = dimensions.Width;
                float f = texture.texture.height / (float)texture.texture.width;
                h = w * f;
            }
            else
            {
                h = dimensions.Height;
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
