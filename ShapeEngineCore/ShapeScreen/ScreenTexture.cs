using System.Numerics;
using Raylib_CsLo;
using ShapeLib;
using ShapeCore;

namespace ShapeScreen
{
    internal class ScreenFlash
    {
        private float maxDuration = 0.0f;
        private float flashTimer = 0.0f;
        private Color startColor = new(0, 0, 0, 0);
        private Color endColor = new(0, 0, 0, 0);
        private Color curColor = new(0, 0, 0, 0);

        public ScreenFlash(float duration, Color start, Color end)
        {

            maxDuration = duration;
            flashTimer = duration;
            startColor = start;
            curColor = start;
            endColor = end;
        }

        public void Update(float dt)
        {
            if (flashTimer > 0.0f)
            {
                flashTimer -= dt;
                float f = 1.0f - flashTimer / maxDuration;
                curColor = startColor.Lerp(endColor, f); // SColor.LerpColor(startColor, endColor, f);
                if (flashTimer <= 0.0f)
                {
                    flashTimer = 0.0f;
                    curColor = endColor;
                }
            }
        }
        public bool IsFinished() { return flashTimer <= 0.0f; }
        public Color GetColor() { return curColor; }

    }
    public class ScreenTexture
    {
        /// <summary>
        /// A factor to scale from this texture to the screen. (useful for positions)
        /// </summary>
        public float TextureToScreen { get; private set; } = 1f;
        /// <summary>
        /// A factor to scale from the screen to this texture. (useful for positions)
        /// </summary>
        public float ScreenToTexture { get; private set; } = 1f;
        /// <summary>
        /// An inversed factor to scale from the screen to this texture. (useful for sizes)
        /// </summary>
        public float ScreenToTextureInverse { get; private set; } = 1f;
        /// <summary>
        /// An inversed factor to scale from this texture to the screen. (Useful for sizes)
        /// </summary>
        public float TextureToScreenInverse { get; private set; } = 1f;

        /// <summary>
        /// Determines if the texture should be updated and used for drawing.
        /// </summary>
        public bool Active { get; set; } = true;
        /// <summary>
        /// The ID of this texture. It is automatically assigned in the constructor.
        /// </summary>
        public uint ID { get; private set; }
        /// <summary>
        /// The draw order for drawing the texture to screen. Lower number are drawn first to the screen.
        /// </summary>
        public int DrawOrder { get; set; } = 0;
        /// <summary>
        /// The blend mode of this texture used when drawing the texture to the screen.
        /// </summary>
        public int BlendMode { get; set; } = -1;
        /// <summary>
        /// The relative mouse pos of this texture.
        /// </summary>
        public Vector2 MousePos { get; set; } = new(0f);
        /// <summary>
        /// The clear color for drawing to this texture.
        /// </summary>
        public Color BackgroundColor { get; set; } = new(0, 0, 0, 0);
        /// <summary>
        /// A tint for this texture.
        /// </summary>
        public Color Tint { get; set; } = WHITE;
        /// <summary>
        /// The base size specified in the constructor.
        /// </summary>
        public (int width, int height) BaseSize;

        /// <summary>
        /// The shader device that returns all active shaders for drawing to the screen.
        /// </summary>
        public IShaderDevice? ShaderDevice { private get; set; } = null;
        private ICamera? camera = null;

        private RenderTexture texture;
        private Rectangle sourceRec;
        private ScreenBuffer[] screenBuffers = new ScreenBuffer[0];
        private List<ScreenFlash> screenFlashes = new List<ScreenFlash>();

        public ScreenTexture(int width, int height, int drawOrder = 0)
        {
            this.ID = SID.NextID;
            this.BaseSize = (width, height);
            this.Load(width, height);
            this.DrawOrder = drawOrder;
        }
        private void Load(int width, int height)
        {
            this.texture = LoadRenderTexture(width, height);
            this.sourceRec = new Rectangle(0, 0, width, -height);
            screenBuffers = new ScreenBuffer[]
            {
                new(width, height),
                new(width, height)
            };
        }
        /// <summary>
        /// Adjust this textures size to the target size. Matches the aspect ratio of the target size, with the closest representation of the base size of the texture.
        /// If this textures base size aspect ratio matches the target size aspect ratio than the size is not changed.
        /// </summary>
        /// <param name="targetWidth">The target width to match.</param>
        /// <param name="targetHeight">The target height to match.</param>
        public void AdjustSize(int targetWidth, int targetHeight)
        {
            float fWidth = (float)targetWidth / (float)targetHeight;
            float fHeight = (float)targetHeight / (float)targetWidth;

            int w = BaseSize.width;
            int h = BaseSize.height;

            float newWidth = ((h * fWidth) + w) * 0.5f;
            float newHeight = ((w * fHeight) + (h)) * 0.5f;

            int adjustedWidth = (int)newWidth;
            int adjustedHeight = (int)newHeight;

            if (adjustedWidth != w || adjustedHeight != h)
            {
                UnloadRenderTexture(texture);
                screenBuffers[0].Unload();
                screenBuffers[1].Unload();
                Load(adjustedWidth, adjustedHeight);
                if (camera != null)
                {
                    camera.AdjustSize(new(adjustedWidth, adjustedHeight));
                }
            }
            float tw = GetSize().X;
            float sw = (float)targetWidth;
            ScreenToTexture = sw / tw;
            TextureToScreen = tw / sw;
            ScreenToTextureInverse = 1 / ScreenToTexture;
            TextureToScreenInverse = 1 / TextureToScreen;
        }

        /// <summary>
        /// Set the cur camera of this texture.
        /// </summary>
        /// <param name="camera"></param>
        public void SetCamera(ICamera camera)
        {
            this.camera = camera;
            this.camera.AdjustSize(GetSize());
        }
        /// <summary>
        /// Sets the camera to null.
        /// </summary>
        public void ClearCamera() { camera = null; }
        /// <summary>
        /// Updates the current screen flashes and the camera.
        /// </summary>
        /// <param name="dt"></param>
        public void Update(float dt)
        {
            if(camera != null) camera.Update(dt);

            for (int i = screenFlashes.Count() - 1; i >= 0; i--)
            {
                var flash = screenFlashes[i];
                flash.Update(dt);
                if (flash.IsFinished()) { screenFlashes.RemoveAt(i); }
            }
        }
        /// <summary>
        /// Draw this texture onto another surface like the screen.
        /// </summary>
        /// <param name="targetWidth">The target width of the surface to draw to.</param>
        /// <param name="targetHeight">The target height of the surface to draw to.</param>
        /// <param name="blendMode">The blend mode for drawing to the surface.</param>
        public void DrawTexture(int targetWidth, int targetHeight, int blendMode = -1)
        {
            var destRec = new Rectangle();
            destRec.x = targetWidth * 0.5f;
            destRec.y = targetHeight * 0.5f;
            destRec.width = targetWidth;
            destRec.height = targetHeight;

            Vector2 origin = new();
            origin.X = targetWidth * 0.5f;
            origin.Y = targetHeight * 0.5f;

            if (blendMode < 0)
            {
                DrawTexturePro(texture.texture, sourceRec, destRec, origin, 0f, Tint);
            }
            else
            {
                BeginBlendMode(blendMode);
                DrawTexturePro(texture.texture, sourceRec, destRec, origin, 0f, Tint);
                EndBlendMode();
            }
        }
        /// <summary>
        /// Close this texture. Unloads the texture and the screen buffers.
        /// </summary>
        public void Close()
        {
            UnloadRenderTexture(texture);
            foreach (ScreenBuffer screenBuffer in screenBuffers)
            {
                screenBuffer.Unload();
            }
            screenBuffers = new ScreenBuffer[0];
        }

        /// <summary>
        /// Start to draw on this texture.
        /// </summary>
        public void BeginTextureMode()
        {
            if (camera != null)
            {
                Raylib.BeginTextureMode(texture);
                BeginMode2D(camera.GetCamera());
                ClearBackground(BackgroundColor);
            }
            else
            {
                Raylib.BeginTextureMode(texture);
                ClearBackground(BackgroundColor);
            }
        }
        /// <summary>
        /// End the drawing to this texture.
        /// </summary>
        public void EndTextureMode()
        {
            if (camera != null)
            {
                var camera = this.camera.GetCamera();
                foreach (var flash in screenFlashes)
                {
                    Vector2 sizeOffset = new(5f, 5f);
                    Vector2 center = camera.target;
                    Vector2 size = camera.offset * 2 * (1f / camera.zoom);
                    var r = new Rect(center, size + sizeOffset, new(0.5f, 0.5f));
                    r.Draw(new Vector2(0.5f, 0.5f), -camera.rotation, flash.GetColor());
                    //SDrawing.DrawRect(new(center, size + sizeOffset, new(0.5f)), new Vector2(0.5f, 0.5f), -camera.rotation, flash.GetColor());
                }
                EndMode2D();
                Raylib.EndTextureMode();
            }
            else
            {
                foreach (var flash in screenFlashes)
                {
                    DrawRectangle(-1, -1, GetTextureWidth() + 1, GetTextureHeight() + 1, flash.GetColor());
                }
                Raylib.EndTextureMode();
            }
        }
        /// <summary>
        /// Draws the texture to the screen with all active shaders applied.
        /// </summary>
        /// <param name="targetWidth">The width of the screen.</param>
        /// <param name="targetHeight">The height of the screen.</param>
        public void DrawToScreen(int targetWidth, int targetHeight)
        {
            //if (camera != null && camera.IsPixelSmoothingCameraEnabled())
            //    BeginMode2D(camera.GetPixelSmoothingCamera());


            List<ScreenShader> shadersToApply = ShaderDevice != null ? ShaderDevice.GetCurActiveShaders() : new();
            if (shadersToApply.Count <= 0)
            {
                DrawTexture(targetWidth, targetHeight, BlendMode);
                return;
            }
            else if (shadersToApply.Count == 1)
            {
                ScreenShader s = shadersToApply[0];
                BeginShaderMode(s.GetShader());
                DrawTexture(targetWidth, targetHeight, BlendMode);
                EndShaderMode();
            }
            else if (shadersToApply.Count == 2)
            {
                ScreenShader s = shadersToApply[0];
                screenBuffers[0].StartTextureMode();
                BeginShaderMode(s.GetShader());
                DrawTexture(GetTextureWidth(), GetTextureHeight());
                EndShaderMode();
                screenBuffers[0].EndTextureMode();

                s = shadersToApply[1];

                BeginShaderMode(s.GetShader());
                screenBuffers[0].DrawTexture(targetWidth, targetHeight, BlendMode);
                EndShaderMode();
            }
            else
            {
                ScreenShader s = shadersToApply[0];
                shadersToApply.RemoveAt(0);

                ScreenShader endshader = shadersToApply[shadersToApply.Count - 1];
                shadersToApply.RemoveAt(shadersToApply.Count - 1);

                //draw game texture to first screenbuffer and first shader is already applied
                screenBuffers[0].StartTextureMode();
                BeginShaderMode(s.GetShader());
                DrawTexture(GetTextureWidth(), GetTextureHeight());
                EndShaderMode();
                screenBuffers[0].EndTextureMode();

                int currentIndex = 0;
                int nextIndex = 0;
                for (int i = 0; i < shadersToApply.Count; i++)
                {
                    s = shadersToApply[i];
                    nextIndex = currentIndex == 0 ? 1 : 0;
                    ScreenBuffer current = screenBuffers[currentIndex];
                    ScreenBuffer next = screenBuffers[nextIndex];
                    next.StartTextureMode();
                    BeginShaderMode(s.GetShader());
                    current.DrawTexture(GetTextureWidth(), GetTextureHeight());
                    EndShaderMode();
                    next.EndTextureMode();
                    currentIndex = currentIndex == 0 ? 1 : 0;
                }

                BeginShaderMode(endshader.GetShader());
                screenBuffers[nextIndex].DrawTexture(targetWidth, targetHeight, BlendMode);
                EndShaderMode();
            }


            //if (camera != null && camera.IsPixelSmoothingCameraEnabled()) EndMode2D();
        }

        /// <summary>
        /// Get the size of the screen texture.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetSize() { return new(texture.texture.width, texture.texture.height); }
        /// <summary>
        /// Get the width of the screen texture.
        /// </summary>
        /// <returns></returns>
        public int GetTextureWidth() { return texture.texture.width; }
        /// <summary>
        /// Get the height of the screen texture.
        /// </summary>
        /// <returns></returns>
        public int GetTextureHeight() { return texture.texture.height; }
        /// <summary>
        /// Get a factor for scaling a a position relative to this texture to a position relative to "toSize".
        /// </summary>
        /// <param name="toSize">The target size for the scale factor.</param>
        /// <returns>Returns a factor for scaling positions.</returns>

        public float GetScaleFactorTo(Vector2 toSize) { return toSize.X / GetSize().X; }
        /// <summary>
        /// Get a factor for scaling a position relative to this texture to another texture.
        /// </summary>
        /// <param name="to">The target texture for the scale factor.</param>
        /// <returns>Returns a factor for scaling positions.</returns>
        public float GetScaleFactorTo(ScreenTexture to) { return to.GetSize().X / GetSize().X; }
        /// <summary>
        /// Get a factor for scaling a position relative to "fromSize" to this texture.
        /// </summary>
        /// <param name="fromSize"></param>
        /// <returns>Returns a factor for scaling positions.</returns>
        public float GetScaleFactorFrom(Vector2 fromSize) { return GetSize().X / fromSize.X; }
        /// <summary>
        /// Scale a position from "fromSize" to this texture.
        /// </summary>
        /// <param name="pos">The position to scale.</param>
        /// <param name="fromSize">The size the position is relative to.</param>
        /// <returns>Returns the scaled position in world space.</returns>
        public Vector2 ScalePosition(Vector2 pos, Vector2 fromSize) { return ScreenToWorld(pos * GetScaleFactorFrom(fromSize)); }
        /// <summary>
        /// Scales a position from this texture to the target texture.
        /// </summary>
        /// <param name="pos">The position to scale.</param>
        /// <param name="to">The target texture to scale to.</param>
        /// <returns>Return the position scaled to the target texture in world space.</returns>
        public Vector2 ScalePosition(Vector2 pos, ScreenTexture to)  {  return to.ScreenToWorld(WorldToScreen(pos) * GetScaleFactorTo(to.GetSize()));  }


        /// <summary>
        /// Scales the value with considering the zoom levels of both textures.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public Vector2 ScaleVector(Vector2 v, ScreenTexture to)
        {
            float cf = camera != null ? camera.GetZoomFactor() : 1.0f;
            float toCf = to.camera != null ? to.camera.GetZoomFactorInverse() : 1f;
            return v * GetScaleFactorTo(to) * cf * toCf;
        }
        /// <summary>
        /// Scales the value with considering the zoom levels of both textures.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public float ScaleFloat(float v, ScreenTexture to)
        {
            float cf = camera != null ? camera.GetZoomFactor() : 1.0f;
            float toCf = to.camera != null ? to.camera.GetZoomFactorInverse() : 1f;
            return v * GetScaleFactorTo(to) * cf * toCf;
        }
        public float AdjustToZoom(float v, ScreenTexture other)
        {
            float selfF = camera != null ? camera.GetZoomFactorInverse() : 1.0f;
            float otherF = other.camera != null ? other.camera.GetZoomFactor() : 1f;
            return v * selfF * otherF;
        }
        public float AdjustToZoom(float v)
        {
            float selfF = camera != null ? camera.GetZoomFactorInverse() : 1.0f;
            return v * selfF;
        }
        public Vector2 AdjustToZoom(Vector2 v, ScreenTexture other)
        {
            float selfF = camera != null ? camera.GetZoomFactorInverse() : 1.0f;
            float otherF = other.camera != null ? other.camera.GetZoomFactor() : 1f;
            return v * selfF * otherF;
        }
        public Vector2 AdjustToZoom(Vector2 v)
        {
            float selfF = camera != null ? camera.GetZoomFactorInverse() : 1.0f;
            return v * selfF;
        }

        /// <summary>
        /// Transforms a world position to a relative screen position if Camera != null.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector2 WorldToScreen(Vector2 pos)
        {
            if (camera != null) return camera.WorldToScreen(pos);
            else return pos;
        }
        /// <summary>
        /// Transforms a relative screen position to a world position if Camera != null.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector2 ScreenToWorld(Vector2 pos)
        {
            if (camera != null) return camera.ScreenToWorld(pos);
            else return pos;
        }

        /// <summary>
        /// Add a flash to this texture for a certain duration. The color of the flash lerps from start to end color.
        /// </summary>
        /// <param name="duration">Must be bigger than 0.</param>
        /// <param name="startColor">The flash start color.</param>
        /// <param name="endColor">The flash end color.</param>
        public void Flash(float duration, Color startColor, Color endColor)
        {
            if (duration <= 0.0f) return;
            ScreenFlash flash = new(duration, startColor, endColor);
            screenFlashes.Add(flash);
        }
        /// <summary>
        /// Stop all current flashes.
        /// </summary>
        public void StopFlash() { screenFlashes.Clear(); }


    }

}

/*
    public interface IScreenTexture { 
        
        public Vector2 Offset { get; set; }
        public float Rotation { get; set; }
        public float Scale { get; set; }

        public Color BackgroundColor { get; set; }
        public Color Tint { get; set; }

        //public void Load(int width, int height);
        public void Update(float dt);
        public void DrawTexture(int targetWidth, int targetHeight, int blendMode = -1);
        public void Close();

        public void BeginTextureMode(Camera2D camera);
        public void BeginTextureMode();
        public void EndTextureMode(Camera2D camera);
        public void EndTextureMode();


        public int GetTextureWidth();
        public int GetTextureHeight();
        public float GetTextureSizeFactor();
        public Vector2 GetCurResolutionFactorV(float targetWidth, float targetHeight);

        public Vector2 ScalePosition(Vector2 pos, int targetResWidth, int targetResHeight);
        public void Flash(float duration, Color startColor, Color endColor);
        public void StopFlash();


    }
    public class ScreenTexture : IScreenTexture
    {
        //public delegate void TextureSizeChanged(int w, int h, float factor);
        //public event TextureSizeChanged? OnTextureSizeChanged;

        public Vector2 Offset {get; set;} = new(0, 0);
        public float Rotation {get; set;} = 0.0f;
        public float Scale { get; set; } = 1.0f;
        
        public Color BackgroundColor { get; set; } = new(0, 0, 0, 0);
        public Color Tint { get; set; } = WHITE;

        //public (int width, int height) DevRes { get; private set; }
        //public (int width, int height) TargetRes { get; private set; }
        
        private float textureSizeFactor = 1.0f;
        private RenderTexture texture;
        private Rectangle sourceRec;
        
        private List<ScreenFlash> screenFlashes = new List<ScreenFlash>();


        public ScreenTexture(int devWidth, int devHeight, float scaleFactor)
        {
            //this.DevRes = (devWidth, devHeight);
            //this.TargetRes = (winWidth, winHeight);
            this.textureSizeFactor = scaleFactor;
            Load(devWidth, devHeight);
            
        }
        private void Load(int width, int height)
        {
            int textureWidth = (int)(width * textureSizeFactor);
            int textureHeight = (int)(height * textureSizeFactor);
            this.texture = LoadRenderTexture(textureWidth, textureHeight);
            this.sourceRec = new Rectangle(0, 0, textureWidth, -textureHeight);
        }
        
        //public ScreenTexture(int devWidth, int devHeight, int winWidth, int winHeight, float textureFactor)
        //{
        //    this.DevRes = (devWidth, devHeight);
        //    this.TargetRes = (winWidth, winHeight);
        //    this.TextureSizeFactor = textureFactor;
        //
        //    int textureWidth = (int)(DevRes.width * textureFactor);
        //    int textureHeight = (int)(DevRes.height * textureFactor);
        //    this.texture = LoadRenderTexture(textureWidth, textureHeight);
        //    this.sourceRec = new Rectangle(0, 0, textureWidth, -textureHeight);
        //    
        //    //this.destRec = new Rectangle(winWidth / 2, winHeight / 2, winWidth, winHeight);
        //    //this.origin = new Vector2(winWidth / 2, winHeight / 2);
        //    //this.fixedSize = fixedSize;
        //    //this.curWindowSize.width = winWidth;
        //    //this.curWindowSize.height = winHeight;
        //    //
        //    //if (!fixedSize)
        //    //{
        //    //    float fWidth = winWidth / (float)developmentResolution.width;
        //    //    float fHeight = winHeight / (float)developmentResolution.height;
        //    //    float f = fWidth <= fHeight ? fWidth : fHeight;
        //    //
        //    //    TARGET_RESOLUTION = ((int)(winWidth / f), (int)(winHeight / f));
        //    //}
        //    //else this.TARGET_RESOLUTION = (devWidth, devHeight);
        //    //
        //    //this.STRETCH_FACTOR = new Vector2
        //    //    (
        //    //        (float)TARGET_RESOLUTION.width / (float)developmentResolution.width,
        //    //        (float)TARGET_RESOLUTION.height / (float)developmentResolution.height
        //    //    );
        //    //this.STRETCH_AREA_FACTOR = STRETCH_FACTOR.X * STRETCH_FACTOR.Y;
        //    //this.STRETCH_AREA_SIDE_FACTOR = MathF.Sqrt(STRETCH_AREA_FACTOR);
        //}
        

        public void Update(float dt)
        {
            
            ////screenShake.Update(dt);
            ////if (screenShake.IsActive())
            ////{
            ////    screenShakeCurScale = RayMath.Lerp(screenShakeScale, 0.0f, 1.0f - screenShake.GetF());
            ////    screenShakeOffset.X = screenShakeStrength.X * screenShake.GetCurX();
            ////    screenShakeOffset.Y = screenShakeStrength.Y * screenShake.GetCurY();
            ////}
            ////else
            ////{
            ////    screenShakeCurScale = 0f;
            ////    screenShakeOffset.X = 0.0f;
            ////    screenShakeOffset.Y = 0.0f;
            ////}
            //if (blendModeTimer > 0.0f)
            //{
            //    blendModeTimer -= dt;
            //    if (blendModeTimer <= 0.0f)
            //    {
            //        blendModeTimer = 0.0f;
            //        blendModeDuration = 0.0f;
            //        blendMode = prevBlendMode;
            //    }
            //}
            //
            //if (flashTintTimer > 0.0f)
            //{
            //    flashTintTimer -= dt;
            //    float f = 1.0f - flashTintTimer / flashTintDuration;
            //    curTint = SColor.Lerp(flashTintStartColor, Tint, f);
            //    //flashTintCurScale = RayMath.Lerp(flashTintScale, 0.0f, f);
            //    if (flashTintTimer <= 0.0f)
            //    {
            //        flashTintTimer = 0.0f;
            //        curTint = Tint;
            //        //flashTintScale = 0.0f;
            //        //flashTintCurScale = 0.0f;
            //    }
            //}
            


            //VARIANT 1
            for (int i = screenFlashes.Count() - 1; i >= 0; i--)
            {
                var flash = screenFlashes[i];
                flash.Update(dt);
                if (flash.IsFinished()) { screenFlashes.RemoveAt(i); }

            }
        }
        public void DrawTexture(int targetWidth, int targetHeight, int blendMode = -1)
        {
            //TargetRes = (targetWidth, targetHeight);
            float s = Scale;
            float w = targetWidth * s;
            float h = targetHeight * s;
            //if (fixedSize)
            //{
            //    Vector2 size = GetDestRectSize(targetWidth, targetHeight);
            //    w = size.X * s;
            //    h = size.Y * s;
            //    destRec.x = targetWidth * 0.5f / s + Offset.X;
            //    destRec.y = targetHeight * 0.5f / s + Offset.Y;
            //}
            //else
            //{
            //    destRec.x = w * 0.5f / s + Offset.X;
            //    destRec.y = h * 0.5f / s + Offset.Y;
            //}
            var destRec = new Rectangle();
            destRec.x = w * 0.5f / s + Offset.X;
            destRec.y = h * 0.5f / s + Offset.Y;
            destRec.width = w;
            destRec.height = h;
            
            Vector2 origin = new();
            origin.X = w * 0.5f;
            origin.Y = h * 0.5f;

            if (blendMode < 0)
            {
                DrawTexturePro(texture.texture, sourceRec, destRec, origin, Rotation, Tint);
            }
            else
            {
                BeginBlendMode(blendMode);
                DrawTexturePro(texture.texture, sourceRec, destRec, origin, Rotation, Tint);
                EndBlendMode();
            }
        }
        public void Close()
        {
            UnloadRenderTexture(texture);
        }
        
        public void BeginTextureMode(Camera2D camera)
        {
            Raylib.BeginTextureMode(texture);
            BeginMode2D(camera);
            ClearBackground(BackgroundColor);
        }
        public void BeginTextureMode()
        {
            Raylib.BeginTextureMode(texture);
            ClearBackground(BackgroundColor);
        }
        public void EndTextureMode(Camera2D camera)//Camera? camera = null
        {
            foreach (var flash in screenFlashes)
            {
                Vector2 sizeOffset = new(5f, 5f);
                Vector2 center = camera.target;
                Vector2 size = camera.offset * 2 * (1f / camera.zoom);
                var r = new Rect(center, size + sizeOffset, new(0.5f, 0.5f));
                r.Draw(new Vector2(0.5f, 0.5f), -camera.rotation, flash.GetColor());
                //SDrawing.DrawRect(new(center, size + sizeOffset, new(0.5f)), new Vector2(0.5f, 0.5f), -camera.rotation, flash.GetColor());
            }
            EndMode2D();
            Raylib.EndTextureMode();
        }
        public void EndTextureMode()
        {
            foreach (var flash in screenFlashes)
            {
                DrawRectangle(-1, -1, GetTextureWidth() + 1, GetTextureHeight() + 1, flash.GetColor());
            }
            Raylib.EndTextureMode();
        }


        //public RenderTexture GetTexture() { return texture; }
        public int GetTextureWidth() { return texture.texture.width; }
        public int GetTextureHeight() { return texture.texture.height; }
        public float GetTextureSizeFactor() { return textureSizeFactor; }
        
        public Vector2 GetCurResolutionFactorV(float targetWidth, float targetHeight)
        {
            float x = GetTextureWidth() / targetWidth;// (float)TargetRes.width;
            float y = GetTextureHeight() / targetHeight;// (float)TargetRes.height;
            return new(x, y);
        }
        public Vector2 ScalePosition(Vector2 pos, int targetResWidth, int targetResHeight)
        {
            Vector2 size = GetDestRectSize(targetResWidth, targetResHeight);
            Vector2 dif = new Vector2(targetResWidth, targetResHeight) - size;
            dif *= 0.5f;
            pos -= dif;
            float fWidth = GetTextureWidth() / size.X;
            float fHeight = GetTextureHeight() / size.Y;
            pos.X = Clamp(pos.X * fWidth, 0, GetTextureWidth());
            pos.Y = Clamp(pos.Y * fHeight, 0, GetTextureHeight());
            return pos;
        }
        private Vector2 GetDestRectSize(int width, int height)
        {
            float w, h;
            float fWidth = width / (float)GetTextureWidth();
            float fHeight = height / (float)GetTextureHeight();
            if (fWidth <= fHeight)
            {
                w = width;
                float f = GetTextureHeight() / (float)GetTextureWidth();
                h = w * f;
            }
            else
            {
                h = height;
                float f = GetTextureWidth() / (float)GetTextureHeight();
                w = h * f;
            }
            return new(w, h);
        }
        
        public void Flash(float duration, Color startColor, Color endColor)
        {
            if (duration <= 0.0f) return;
            ScreenFlash flash = new(duration, startColor, endColor);
            screenFlashes.Add(flash);
        }
        public void StopFlash() { screenFlashes.Clear(); }

        




    }
    */
/*
    public class ScreenTexture
    {
        public delegate void TextureSizeChanged(int w, int h, float factor);
        public event TextureSizeChanged? OnTextureSizeChanged;

        private Vector2 offset = new(0, 0);
        private float rotation = 0.0f;
        private float scale = 1.0f;

        private RenderTexture texture;
        private Rectangle sourceRec;
        private Rectangle destRec;
        private Vector2 origin;
        //private Vector2 prevTextureSize = new(0f, 0f);
        private Color backgroundColor = new(0, 0, 0, 0);


        private (int width, int height) developmentResolution;
        public (int width, int height) TARGET_RESOLUTION { get; private set; } = (0, 0);
        private (int width, int height) curWindowSize;
        private float curTextureSizeFactor = 1.0f;
        private bool fixedSize = true;
        public Vector2 STRETCH_FACTOR { get; private set; } = new(1f);
        public float STRETCH_AREA_FACTOR { get; private set; } = 1f;
        public float STRETCH_AREA_SIDE_FACTOR { get; private set; } = 1f;

        private int blendMode = -1;
        private int prevBlendMode = -1;
        private float blendModeDuration = 0.0f;
        private float blendModeTimer = 0.0f;


        private Color tint = WHITE;
        private float flashTintTimer = 0.0f;
        private float flashTintDuration = 0.0f;
        private Color flashTintStartColor = new(0, 0, 0, 0);
        private Color curTint = WHITE;

        private List<ScreenFlash> screenFlashes = new List<ScreenFlash>();

        public ScreenTexture(int devWidth, int devHeight, int winWidth, int winHeight, float factor, bool fixedSize = false)
        {
            this.fixedSize = fixedSize;
            this.developmentResolution.width = devWidth;
            this.developmentResolution.height = devHeight;
            this.curTextureSizeFactor = factor;
            this.curWindowSize.width = winWidth;
            this.curWindowSize.height = winHeight;

            if (!fixedSize)
            {
                float fWidth = winWidth / (float)developmentResolution.width;
                float fHeight = winHeight / (float)developmentResolution.height;
                float f = fWidth <= fHeight ? fWidth : fHeight;

                TARGET_RESOLUTION = ((int)(winWidth / f), (int)(winHeight / f));
            }
            else this.TARGET_RESOLUTION = (devWidth, devHeight);

            this.STRETCH_FACTOR = new Vector2
                (
                    (float)TARGET_RESOLUTION.width / (float)developmentResolution.width,
                    (float)TARGET_RESOLUTION.height / (float)developmentResolution.height
                );
            this.STRETCH_AREA_FACTOR = STRETCH_FACTOR.X * STRETCH_FACTOR.Y;
            this.STRETCH_AREA_SIDE_FACTOR = MathF.Sqrt(STRETCH_AREA_FACTOR);
            int textureWidth = (int)(TARGET_RESOLUTION.width * factor);
            int textureHeight = (int)(TARGET_RESOLUTION.height * factor);
            //this.prevTextureSize = new(textureWidth, textureHeight);
            this.texture = LoadRenderTexture(textureWidth, textureHeight);


            this.sourceRec = new Rectangle(0, 0, textureWidth, -textureHeight);
            this.destRec = new Rectangle(winWidth / 2, winHeight / 2, winWidth, winHeight);
            this.origin = new Vector2(winWidth / 2, winHeight / 2);
        }


        public float GetRotation() { return rotation; }
        public void SetRotation(float rotation) { this.rotation = rotation; }
        public void Rotate(float amount) { rotation += amount; }

        public Vector2 GetOffset() { return offset; }
        public void MoveTo(Vector2 point) { offset = point; }
        public void MoveBy(Vector2 movement) { offset += movement; }

        public float GetScale() { return scale; }
        public void SetScale(float newScale) { scale = newScale; }
        public void AddScale(float amount) { scale += amount; }


        public void StopShake() { }
        public void StopFlash() { screenFlashes.Clear(); }
        public void StopFlashTint()
        {
            flashTintTimer = 0.0f;
            flashTintDuration = 0.0f;
            curTint = tint;
        }

        //public Vector2 UpdatePosition(Vector2 pos)
        //{
        //    float newX = Utils.RemapFloat(pos.X, 0, prevTextureSize.X, 0, GetTextureWidth());
        //    float newY = Utils.RemapFloat(pos.Y, 0, prevTextureSize.Y, 0, GetTextureHeight());
        //    return new(newX, newY);
        //}

        public RenderTexture GetTexture() { return texture; }

        public int GetTextureWidth() { return texture.texture.width; }
        public int GetTextureHeight() { return texture.texture.height; }
        public float GetTextureSizeFactor() { return curTextureSizeFactor; }

        private float GetCurResolutionFactorRawX()
        {
            return ((float)developmentResolution.width * curTextureSizeFactor) / (float)curWindowSize.width;
        }
        private float GetCurResolutionFactorRawY()
        {
            return ((float)developmentResolution.height * curTextureSizeFactor) / (float)curWindowSize.height;
        }
        private float GetCurResolutionFactorX()
        {
            return GetTextureWidth() / (float)curWindowSize.width;
        }
        private float GetCurResolutionFactorY()
        {
            return GetTextureHeight() / (float)curWindowSize.height;
        }
        public Vector2 GetCurResolutionFactorV()
        {
            return new(GetCurResolutionFactorX(), GetCurResolutionFactorY());
        }
        public Vector2 ScalePosition(float x, float y)
        {
            return ScalePositionV(new(x, y));
        }
        public Vector2 ScalePositionV(Vector2 pos)
        {
            if (fixedSize)
            {
                Vector2 size = GetDestRectSize(curWindowSize.width, curWindowSize.height);
                Vector2 dif = new Vector2(curWindowSize.width, curWindowSize.height) - size;
                dif *= 0.5f;
                pos -= dif;
                float fWidth = GetTextureWidth() / size.X;
                float fHeight = GetTextureHeight() / size.Y;
                pos.X = Clamp(pos.X * fWidth, 0, GetTextureWidth());
                pos.Y = Clamp(pos.Y * fHeight, 0, GetTextureHeight());
                return pos;

            }
            else
            {
                return new(pos.X * GetCurResolutionFactorX(), pos.Y * GetCurResolutionFactorY());
            }
        }
        //public Vector2 ScalePositionRawV(Vector2 pos)
        //{
        //    if (fixedSize)
        //    {
        //        Vector2 size = GetDestRectSize(curWindowSize.width, curWindowSize.height);
        //        Vector2 dif = new Vector2(curWindowSize.width, curWindowSize.height) - size;
        //        dif *= 0.5f;
        //        pos -= dif;
        //        float fWidth = GetTextureWidth() / size.X;
        //        float fHeight = GetTextureHeight() / size.Y;
        //        pos.X = Clamp(pos.X * fWidth, 0, GetTextureWidth());
        //        pos.Y = Clamp(pos.Y * fHeight, 0, GetTextureHeight());
        //        return pos;
        //        //float w, h;
        //        //float fWidth = curWindowSize.width / (float)developmentResolution.width;
        //        //float fHeight = curWindowSize.height / (float)developmentResolution.height;
        //        //if (fWidth <= fHeight)
        //        //{
        //        //    w = curWindowSize.width;
        //        //    float f = developmentResolution.height / (float)developmentResolution.width;
        //        //    h = w * f;
        //        //
        //        //}
        //        //else
        //        //{
        //        //    h = curWindowSize.height;
        //        //    float f = developmentResolution.width / (float)developmentResolution.height;
        //        //    w = h * f;
        //        //}
        //        //
        //        //Vector2 size = new Vector2(w, h); // GetDestRectSize(curWindowSize.width, curWindowSize.height);
        //        //Vector2 dif = new Vector2(curWindowSize.width, curWindowSize.height) - size;
        //        //dif *= 0.5f;
        //        //pos -= dif;
        //        //float fW = developmentResolution.width / size.X;
        //        //float fH = developmentResolution.height / size.Y;
        //        //pos.X = Clamp(pos.X * fW, 0, developmentResolution.width);
        //        //pos.Y = Clamp(pos.Y * fH, 0, developmentResolution.height);
        //        //return pos;
        //
        //    }
        //    else
        //    {
        //        return new Vector2(pos.X * GetCurResolutionFactorRawX(), pos.Y * GetCurResolutionFactorRawY());
        //    }
        //}
        public Color GetTint() { return tint; }
        public void SetTint(Color newTint)
        {
            tint = newTint;
            if (flashTintTimer <= 0.0f)
            {
                curTint = tint;
            }
        }
        public void SetClearColor(Color color) { backgroundColor = color; }
        public Color GetClearColor() { return backgroundColor; }

        public BlendMode GetBlendMode() { return (BlendMode)blendMode; }
        public void SetBlendMode(int newBlendMode)
        {
            if (blendModeTimer > 0.0f)
            {
                blendModeTimer = 0.0f;
                blendModeDuration = 0.0f;
                prevBlendMode = newBlendMode;
            }
            blendMode = newBlendMode;
        }
        public void ClearBlendMode()
        {
            SetBlendMode(-1);
        }
        public void AddBlendMode(float duration, int newBlendMode)
        {
            if (duration <= 0.0f) return;

            blendModeDuration = duration;
            blendModeTimer = duration;
            prevBlendMode = blendMode;
            blendMode = newBlendMode;
        }



        //public void MonitorChanged(int devWidth, int devHeight, int winWidth, int winHeight)
        //{
        //    developmentResolution.width = devWidth;
        //    developmentResolution.height = devHeight;
        //
        //    int textureWidth = (int)(devWidth * curTextureSizeFactor);
        //    int textureHeight = (int)(devHeight * curTextureSizeFactor);
        //    ChangeSize(textureWidth, textureHeight);
        //    //texture = LoadRenderTexture(textureWidth, textureHeight);
        //    //curTextureSizeFactor = factor;
        //
        //    ChangeWindowSize(winWidth, winHeight);
        //
        //    //sourceRec = new Rectangle(0, 0, textureWidth, -textureHeight);
        //    destRec = new Rectangle(winWidth / 2, winHeight / 2, winWidth, winHeight);
        //    origin = new Vector2(winWidth / 2, winHeight / 2);
        //}
        //public void ChangeTextureSizeFactor(float newFactor)
        //{
        //    if (newFactor <= 0.0f) return;
        //    if (newFactor == curTextureSizeFactor) return;
        //
        //    curTextureSizeFactor = newFactor;
        //    int textureWidth = (int)(developmentResolution.width * newFactor);
        //    int textureHeight = (int)(developmentResolution.height * newFactor);
        //    ChangeSize(textureWidth, textureHeight);
        //}
        private void ChangeTextureSize(int width, int height)
        {
            //prevTextureSize = new(GetTextureWidth(), GetTextureHeight());
            UnloadRenderTexture(texture);
            texture = LoadRenderTexture(width, height);
            sourceRec.width = width;
            sourceRec.height = -height;
            OnTextureSizeChanged?.Invoke(width, height, STRETCH_AREA_SIDE_FACTOR);
        }
        public void ChangeWindowSize(int winWidth, int winHeight)
        {
            curWindowSize.width = winWidth;
            curWindowSize.height = winHeight;

            if (!fixedSize)
            {
                float fWidth = winWidth / (float)developmentResolution.width;
                float fHeight = winHeight / (float)developmentResolution.height;
                float f = fWidth <= fHeight ? fWidth : fHeight;

                TARGET_RESOLUTION = ((int)(winWidth / f), (int)(winHeight / f));
                //TARGET_RESOLUTION = (winWidth, winHeight);
                STRETCH_FACTOR = new Vector2
                (
                    (float)TARGET_RESOLUTION.width / (float)developmentResolution.width,
                    (float)TARGET_RESOLUTION.height / (float)developmentResolution.height
                );
                STRETCH_AREA_FACTOR = STRETCH_FACTOR.X * STRETCH_FACTOR.Y;
                STRETCH_AREA_SIDE_FACTOR = MathF.Sqrt(STRETCH_AREA_FACTOR);
                int textureWidth = (int)(TARGET_RESOLUTION.width * curTextureSizeFactor);
                int textureHeight = (int)(TARGET_RESOLUTION.height * curTextureSizeFactor);
                ChangeTextureSize(textureWidth, textureHeight);
            }

            destRec = new Rectangle(winWidth / 2, winHeight / 2, winWidth, winHeight);
            origin = new Vector2(winWidth / 2, winHeight / 2);
        }



        public void Update(float dt)
        {
            //screenShake.Update(dt);
            //if (screenShake.IsActive())
            //{
            //    screenShakeCurScale = RayMath.Lerp(screenShakeScale, 0.0f, 1.0f - screenShake.GetF());
            //    screenShakeOffset.X = screenShakeStrength.X * screenShake.GetCurX();
            //    screenShakeOffset.Y = screenShakeStrength.Y * screenShake.GetCurY();
            //}
            //else
            //{
            //    screenShakeCurScale = 0f;
            //    screenShakeOffset.X = 0.0f;
            //    screenShakeOffset.Y = 0.0f;
            //}

            if (blendModeTimer > 0.0f)
            {
                blendModeTimer -= dt;
                if (blendModeTimer <= 0.0f)
                {
                    blendModeTimer = 0.0f;
                    blendModeDuration = 0.0f;
                    blendMode = prevBlendMode;
                }
            }

            if (flashTintTimer > 0.0f)
            {
                flashTintTimer -= dt;
                float f = 1.0f - flashTintTimer / flashTintDuration;
                curTint = SColor.Lerp(flashTintStartColor, tint, f);
                //flashTintCurScale = RayMath.Lerp(flashTintScale, 0.0f, f);
                if (flashTintTimer <= 0.0f)
                {
                    flashTintTimer = 0.0f;
                    curTint = tint;
                    //flashTintScale = 0.0f;
                    //flashTintCurScale = 0.0f;
                }
            }


            //VARIANT 1
            for (int i = screenFlashes.Count() - 1; i >= 0; i--)
            {
                var flash = screenFlashes[i];
                flash.Update(dt);
                if (flash.IsFinished()) { screenFlashes.RemoveAt(i); }

            }
        }
        private Vector2 GetDestRectSize(int width, int height)
        {
            float w, h;
            float fWidth = width / (float)GetTextureWidth();
            float fHeight = height / (float)GetTextureHeight();
            if (fWidth <= fHeight)
            {
                w = width;
                float f = GetTextureHeight() / (float)GetTextureWidth();
                h = w * f;

                // w *= s;
                // h *= s;
            }
            else
            {
                h = height;
                float f = GetTextureWidth() / (float)GetTextureHeight();
                w = h * f;

                // h *= s;
                // w *= s;
            }
            return new(w, h);
        }
        public void Draw()
        {
            float s = scale;
            float w = curWindowSize.width * s;
            float h = curWindowSize.height * s;
            if (fixedSize)
            {
                Vector2 size = GetDestRectSize(curWindowSize.width, curWindowSize.height);
                w = size.X * s;
                h = size.Y * s;
                destRec.x = curWindowSize.width * 0.5f / s + offset.X;// + screenShakeOffset.X;
                destRec.y = curWindowSize.height * 0.5f / s + offset.Y;// + screenShakeOffset.Y;
            }
            else
            {
                destRec.x = w * 0.5f / s + offset.X;// + screenShakeOffset.X;
                destRec.y = h * 0.5f / s + offset.Y;// + screenShakeOffset.Y;
            }

            //destRec.x = (w * 0.5f / s) + offset.X + screenShakeOffset.X;
            //destRec.y = (h * 0.5f / s) + offset.Y + screenShakeOffset.Y;
            destRec.width = w;
            destRec.height = h;
            origin.X = w * 0.5f;
            origin.Y = h * 0.5f;

            if (blendMode < 0)
            {
                DrawTexturePro(texture.texture, sourceRec, destRec, origin, rotation, curTint);
            }
            else
            {
                BeginBlendMode(blendMode);
                DrawTexturePro(texture.texture, sourceRec, destRec, origin, rotation, curTint);
                EndBlendMode();
            }
        }
        public void DrawPro(int targetWidth, int targetHeight)
        {
            float s = scale;
            float w = targetWidth * s;
            float h = targetHeight * s;

            destRec.x = w * 0.5f / s + offset.X;// + screenShakeOffset.X;
            destRec.y = h * 0.5f / s + offset.Y;// + screenShakeOffset.Y;
            destRec.width = w;
            destRec.height = h;
            origin.X = w * 0.5f;
            origin.Y = h * 0.5f;

            DrawTexturePro(texture.texture, sourceRec, destRec, origin, rotation, curTint);
        }
        public void BeginTextureMode(Camera2D camera)
        {
            Raylib.BeginTextureMode(texture);
            BeginMode2D(camera);
            ClearBackground(backgroundColor);
        }
        public void BeginTextureMode()
        {
            Raylib.BeginTextureMode(texture);
            ClearBackground(backgroundColor);
        }
        public void EndTextureMode(Camera2D camera)//Camera? camera = null
        {
            foreach (var flash in screenFlashes)
            {
                Vector2 sizeOffset = new(5f, 5f);
                Vector2 center = camera.target;
                Vector2 size = camera.offset * 2 * (1f / camera.zoom);
                var r = new Rect(center, size + offset, new(0.5f, 0.5f));
                r.Draw(new Vector2(0.5f, 0.5f), -camera.rotation, flash.GetColor());
                //SDrawing.DrawRect(new(center, size + sizeOffset, new(0.5f)), new Vector2(0.5f, 0.5f), -camera.rotation, flash.GetColor());
            }
            EndMode2D();
            Raylib.EndTextureMode();
        }
        public void EndTextureMode()
        {
            foreach (var flash in screenFlashes)
            {
                DrawRectangle(-1, -1, GetTextureWidth() + 1, GetTextureHeight() + 1, flash.GetColor());
            }
            Raylib.EndTextureMode();
        }

        public void Close()
        {
            UnloadRenderTexture(texture);
        }


        //public void Shake(float duration, Vector2 strength, float smoothness = 0.75f, float scalePercentage = 0.0f)
        //{
        //    screenShake.Start(duration, smoothness);
        //    screenShakeStrength = strength;
        //    screenShakeScale = scalePercentage;
        //}
        public void Flash(float duration, Color startColor, Color endColor)
        {
            if (duration <= 0.0f) return;
            ScreenFlash flash = new(duration, startColor, endColor);
            screenFlashes.Add(flash);
        }
        public void FlashTint(float duration, Color color)
        {
            flashTintDuration = duration;
            flashTintTimer = duration;
            flashTintStartColor = color;
            curTint = color;
        }
    }
    */
//public Vector2 ScalePositionV(Vector2 pos)
//{
//    return ScalePosition(pos, TargetRes.width, TargetRes.height);
//
//    //if (fixedSize)
//    //{
//    //    Vector2 size = GetDestRectSize(curWindowSize.width, curWindowSize.height);
//    //    Vector2 dif = new Vector2(curWindowSize.width, curWindowSize.height) - size;
//    //    dif *= 0.5f;
//    //    pos -= dif;
//    //    float fWidth = GetTextureWidth() / size.X;
//    //    float fHeight = GetTextureHeight() / size.Y;
//    //    pos.X = Clamp(pos.X * fWidth, 0, GetTextureWidth());
//    //    pos.Y = Clamp(pos.Y * fHeight, 0, GetTextureHeight());
//    //    return pos;
//    //
//    //}
//    //else
//    //{
//    //    return new(pos.X * GetCurResolutionFactorX(), pos.Y * GetCurResolutionFactorY());
//    //}
//}


//public Vector2 STRETCH_FACTOR { get; private set; } = new(1f);
//public float STRETCH_AREA_FACTOR { get; private set; } = 1f;
//public float STRETCH_AREA_SIDE_FACTOR { get; private set; } = 1f;
//private Rectangle destRec;
//private Vector2 origin;
//public (int width, int height) TARGET_RESOLUTION { get; private set; } = (0, 0);
//private (int width, int height) curWindowSize;
//private bool fixedSize = true;
//public Vector2 ScalePosition(float x, float y)
//{
//    return ScalePositionV(new(x, y));
//}
/*
        private void ChangeTextureSize(int width, int height)
        {
            //prevTextureSize = new(GetTextureWidth(), GetTextureHeight());
            UnloadRenderTexture(texture);
            texture = LoadRenderTexture(width, height);
            sourceRec.width = width;
            sourceRec.height = -height;
            OnTextureSizeChanged?.Invoke(width, height, STRETCH_AREA_SIDE_FACTOR);
        }
        */
//public void ChangeWindowSize(int winWidth, int winHeight)
//{
//    curWindowSize.width = winWidth;
//    curWindowSize.height = winHeight;
//    
//    if (!fixedSize)
//    {
//        float fWidth = winWidth / (float)developmentResolution.width;
//        float fHeight = winHeight / (float)developmentResolution.height;
//        float f = fWidth <= fHeight ? fWidth : fHeight;
//    
//        TARGET_RESOLUTION = ((int)(winWidth / f), (int)(winHeight / f));
//        //TARGET_RESOLUTION = (winWidth, winHeight);
//        STRETCH_FACTOR = new Vector2
//        (
//            (float)TARGET_RESOLUTION.width / (float)developmentResolution.width,
//            (float)TARGET_RESOLUTION.height / (float)developmentResolution.height
//        );
//        STRETCH_AREA_FACTOR = STRETCH_FACTOR.X * STRETCH_FACTOR.Y;
//        STRETCH_AREA_SIDE_FACTOR = MathF.Sqrt(STRETCH_AREA_FACTOR);
//        int textureWidth = (int)(TARGET_RESOLUTION.width * curTextureSizeFactor);
//        int textureHeight = (int)(TARGET_RESOLUTION.height * curTextureSizeFactor);
//        ChangeTextureSize(textureWidth, textureHeight);
//    }
//    
//    destRec = new Rectangle(winWidth / 2, winHeight / 2, winWidth, winHeight);
//    origin = new Vector2(winWidth / 2, winHeight / 2);
//}
/*
        public void Draw()
        {
            float s = Scale;
            float w = curWindowSize.width * s;
            float h = curWindowSize.height * s;
            if (fixedSize)
            {
                Vector2 size = GetDestRectSize(curWindowSize.width, curWindowSize.height);
                w = size.X * s;
                h = size.Y * s;
                destRec.x = curWindowSize.width * 0.5f / s + Offset.X;
                destRec.y = curWindowSize.height * 0.5f / s + Offset.Y;
            }
            else
            {
              destRec.x = w * 0.5f / s + Offset.X;
              destRec.y = h * 0.5f / s + Offset.Y;
            }
            destRec.width = w;
            destRec.height = h;
            origin.X = w * 0.5f;
            origin.Y = h * 0.5f;

            if (blendMode < 0)
            {
                DrawTexturePro(texture.texture, sourceRec, destRec, origin, Rotation, Tint);
            }
            else
            {
                BeginBlendMode(blendMode);
                DrawTexturePro(texture.texture, sourceRec, destRec, origin, Rotation, Tint);
                EndBlendMode();
            }
        }
        public void DrawPro(int targetWidth, int targetHeight)
        {
            float s = Scale;
            float w = targetWidth * s;
            float h = targetHeight * s;

            destRec.x = w * 0.5f / s + Offset.X;
            destRec.y = h * 0.5f / s + Offset.Y;
            destRec.width = w;
            destRec.height = h;
            origin.X = w * 0.5f;
            origin.Y = h * 0.5f;

            DrawTexturePro(texture.texture, sourceRec, destRec, origin, Rotation, Tint);
        }
        */


/*
        private int blendMode = -1;
        private int prevBlendMode = -1;
        private float blendModeTimer = 0.0f;
        private float blendModeDuration = 0.0f;
        private float flashTintTimer = 0.0f;
        private float flashTintDuration = 0.0f;
        private Color flashTintStartColor = new(0, 0, 0, 0);
        private Color curTint = WHITE;
        */
/*
        public void ClearBlendMode()
        {
            SetBlendMode(-1);
        }
        public BlendMode GetBlendMode() { return (BlendMode)blendMode; }
        public void SetBlendMode(int newBlendMode) { blendMode = newBlendMode; }
        public void SetBlendMode(BlendMode newBlendMode) { blendMode = (int)newBlendMode; }
        */

/*
        private float GetCurResolutionFactorRawX()
        {
            return ((float)developmentResolution.width * curTextureSizeFactor) / (float)curWindowSize.width;
        }
        private float GetCurResolutionFactorRawY()
        {
            return ((float)developmentResolution.height * curTextureSizeFactor) / (float)curWindowSize.height;
        }
        */
/*
        
        public void SetBlendMode(int newBlendMode)
        {
            if (blendModeTimer > 0.0f)
            {
                blendModeTimer = 0.0f;
                blendModeDuration = 0.0f;
                prevBlendMode = newBlendMode;
            }
            blendMode = newBlendMode;
        }
        public void AddBlendMode(float duration, int newBlendMode)
        {
            if (duration <= 0.0f) return;

            blendModeDuration = duration;
            blendModeTimer = duration;
            prevBlendMode = blendMode;
            blendMode = newBlendMode;
        }
        */
/*
        public void FlashTint(float duration, Color color)
        {
            flashTintDuration = duration;
            flashTintTimer = duration;
            flashTintStartColor = color;
            curTint = color;
        }
        public void StopFlashTint()
        {
            flashTintTimer = 0.0f;
            flashTintDuration = 0.0f;
            curTint = Tint;
        }
        public Color GetTint() { return Tint; }
        public void SetTint(Color newTint)
        {
            Tint = newTint;
            if (flashTintTimer <= 0.0f)
            {
                curTint = Tint;
            }
        }
        */
/*
        public void SetClearColor(Color color) { backgroundColor = color; }
        public Color GetClearColor() { return backgroundColor; }
        */
/*
        public float GetRotation() { return rotation; }
        public void SetRotation(float rotation) { this.rotation = rotation; }
        public void Rotate(float amount) { rotation += amount; }

        public Vector2 GetOffset() { return offset; }
        public void MoveTo(Vector2 point) { offset = point; }
        public void MoveBy(Vector2 movement) { offset += movement; }

        public float GetScale() { return scale; }
        public void SetScale(float newScale) { scale = newScale; }
        public void AddScale(float amount) { scale += amount; }
        */
/*
        public Vector2 ScalePositionRawV(Vector2 pos)
        {
            if (fixedSize)
            {
                Vector2 size = GetDestRectSize(curWindowSize.width, curWindowSize.height);
                Vector2 dif = new Vector2(curWindowSize.width, curWindowSize.height) - size;
                dif *= 0.5f;
                pos -= dif;
                float fWidth = GetTextureWidth() / size.X;
                float fHeight = GetTextureHeight() / size.Y;
                pos.X = Clamp(pos.X * fWidth, 0, GetTextureWidth());
                pos.Y = Clamp(pos.Y * fHeight, 0, GetTextureHeight());
                return pos;
                //float w, h;
                //float fWidth = curWindowSize.width / (float)developmentResolution.width;
                //float fHeight = curWindowSize.height / (float)developmentResolution.height;
                //if (fWidth <= fHeight)
                //{
                //    w = curWindowSize.width;
                //    float f = developmentResolution.height / (float)developmentResolution.width;
                //    h = w * f;
                //
                //}
                //else
                //{
                //    h = curWindowSize.height;
                //    float f = developmentResolution.width / (float)developmentResolution.height;
                //    w = h * f;
                //}
                //
                //Vector2 size = new Vector2(w, h); // GetDestRectSize(curWindowSize.width, curWindowSize.height);
                //Vector2 dif = new Vector2(curWindowSize.width, curWindowSize.height) - size;
                //dif *= 0.5f;
                //pos -= dif;
                //float fW = developmentResolution.width / size.X;
                //float fH = developmentResolution.height / size.Y;
                //pos.X = Clamp(pos.X * fW, 0, developmentResolution.width);
                //pos.Y = Clamp(pos.Y * fH, 0, developmentResolution.height);
                //return pos;
        
            }
            else
            {
                return new Vector2(pos.X * GetCurResolutionFactorRawX(), pos.Y * GetCurResolutionFactorRawY());
            }
        }
        public Vector2 UpdatePosition(Vector2 pos)
        {
            float newX = Utils.RemapFloat(pos.X, 0, prevTextureSize.X, 0, GetTextureWidth());
            float newY = Utils.RemapFloat(pos.Y, 0, prevTextureSize.Y, 0, GetTextureHeight());
            return new(newX, newY);
        }
        public void MonitorChanged(int devWidth, int devHeight, int winWidth, int winHeight)
        {
            developmentResolution.width = devWidth;
            developmentResolution.height = devHeight;
        
            int textureWidth = (int)(devWidth * curTextureSizeFactor);
            int textureHeight = (int)(devHeight * curTextureSizeFactor);
            ChangeSize(textureWidth, textureHeight);
            //texture = LoadRenderTexture(textureWidth, textureHeight);
            //curTextureSizeFactor = factor;
        
            ChangeWindowSize(winWidth, winHeight);
        
            //sourceRec = new Rectangle(0, 0, textureWidth, -textureHeight);
            destRec = new Rectangle(winWidth / 2, winHeight / 2, winWidth, winHeight);
            origin = new Vector2(winWidth / 2, winHeight / 2);
        }
        public void ChangeTextureSizeFactor(float newFactor)
        {
            if (newFactor <= 0.0f) return;
            if (newFactor == curTextureSizeFactor) return;
        
            curTextureSizeFactor = newFactor;
            int textureWidth = (int)(developmentResolution.width * newFactor);
            int textureHeight = (int)(developmentResolution.height * newFactor);
            ChangeSize(textureWidth, textureHeight);
        }
        
        public void StopShake() { }
        public void Shake(float duration, Vector2 strength, float smoothness = 0.75f, float scalePercentage = 0.0f)
        {
            screenShake.Start(duration, smoothness);
            screenShakeStrength = strength;
            screenShakeScale = scalePercentage;
        }
        */