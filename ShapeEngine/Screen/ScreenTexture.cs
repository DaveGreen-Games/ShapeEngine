using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Lib;
using ShapeEngine.Core;

namespace ShapeEngine.Screen
{
    /*internal class ScreenFlash
    {
        private float maxDuration = 0.0f;
        private float flashTimer = 0.0f;
        private Raylib_CsLo.Color startColor = new(0, 0, 0, 0);
        private Raylib_CsLo.Color endColor = new(0, 0, 0, 0);
        private Raylib_CsLo.Color curColor = new(0, 0, 0, 0);

        public ScreenFlash(float duration, Raylib_CsLo.Color start, Raylib_CsLo.Color end)
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
        public Raylib_CsLo.Color GetColor() { return curColor; }

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
        public Raylib_CsLo.Color BackgroundColor { get; set; } = new(0, 0, 0, 0);
        /// <summary>
        /// A tint for this texture.
        /// </summary>
        public Raylib_CsLo.Color Tint { get; set; } = WHITE;
        /// <summary>
        /// The base size specified in the constructor.
        /// </summary>
        public Dimensions BaseSize;

        /// <summary>
        /// The shader device that returns all active shaders for drawing to the screen.
        /// </summary>
        public IShaderDevice? ShaderDevice { private get; set; } = null;
        private ICamera? camera = null;

        private RenderTexture texture;
        private Rectangle sourceRec;
        //private ScreenBuffer[] screenBuffers = new ScreenBuffer[0];
        private ScreenBufferArray screenBuffers = new();
        private List<ScreenFlash> screenFlashes = new List<ScreenFlash>();

        public ScreenTexture(Dimensions dimensions, int drawOrder = 0)
        {
            this.ID = SID.NextID;
            this.BaseSize = dimensions;
            this.Load(dimensions);
            this.DrawOrder = drawOrder;
        }
        public ScreenTexture(Dimensions dimensions, uint id, int drawOrder = 0)
        {
            this.ID = id;
            this.BaseSize = dimensions;
            this.Load(dimensions);
            this.DrawOrder = drawOrder;
        }
        private void Load(Dimensions dimensions)
        {
            this.texture = LoadRenderTexture(dimensions.Width, dimensions.Height);
            this.sourceRec = new Rectangle(0, 0, dimensions.Width, -dimensions.Height);

            if(ShaderDevice != null && ShaderDevice.IsMultiShader())
                this.screenBuffers.Load(dimensions);
            //screenBuffers = new ScreenBuffer[]
            //{
            //    new(width, height),
            //    new(width, height)
            //};
        }
        /// <summary>
        /// Adjust this textures size to the target size. Matches the aspect ratio of the target size, with the closest representation of the base size of the texture.
        /// If this textures base size aspect ratio matches the target size aspect ratio than the size is not changed.
        /// </summary>
        /// <param name="targetWidth">The target width to match.</param>
        /// <param name="targetHeight">The target height to match.</param>
        public void AdjustSize(Dimensions targetDimensions)
        {
            float fWidth = (float)targetDimensions.Width / (float)targetDimensions.Height;
            float fHeight = (float)targetDimensions.Height / (float)targetDimensions.Width;

            int w = BaseSize.Width;
            int h = BaseSize.Height;

            float newWidth = ((h * fWidth) + w) * 0.5f;
            float newHeight = ((w * fHeight) + (h)) * 0.5f;

            //int adjustedWidth = (int)newWidth;
            //int adjustedHeight = (int)newHeight;
            Dimensions adjustedDimensions = new(newWidth, newHeight);

            if (adjustedDimensions.Width != w || adjustedDimensions.Height != h)
            {
                UnloadRenderTexture(texture);
                //screenBuffers[0].Unload();
                //screenBuffers[1].Unload();
                screenBuffers.Unload();
                Load(adjustedDimensions);
                if (camera != null)
                {
                    camera.AdjustSize(adjustedDimensions.ToVector2());
                }
            }
            float tw = GetSize().X;
            float sw = (float)targetDimensions.Width;
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
        public ICamera? GetCamera()
        {
            return camera;
        }
        public T? GetCamera<T>() where T : ICamera
        {
            if (camera == null) return default(T);
            if (camera is T t) return t;// (T)camera;
            else return default(T);
        }
        
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
        public void DrawTexture(Dimensions targetDimensions, int blendMode = -1)
        {
            var destRec = new Rectangle();
            destRec.x = targetDimensions.Width * 0.5f;
            destRec.y = targetDimensions.Height * 0.5f;
            destRec.width = targetDimensions.Width;
            destRec.height = targetDimensions.Height;

            Vector2 origin = new();
            origin.X = targetDimensions.Width * 0.5f;
            origin.Y = targetDimensions.Height * 0.5f;

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
            screenBuffers.Unload();
            //foreach (ScreenBuffer screenBuffer in screenBuffers)
            //{
            //    screenBuffer.Unload();
            //}
            //screenBuffers = new ScreenBuffer[0];
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
        public void DrawToScreen(Dimensions targetDimensions)
        {
            //if (camera != null && camera.IsPixelSmoothingCameraEnabled())
            //    BeginMode2D(camera.GetPixelSmoothingCamera());


            List<ScreenShader> shadersToApply = ShaderDevice != null ? ShaderDevice.GetActiveShaders() : new();
            if (shadersToApply.Count <= 0)
            {
                DrawTexture(targetDimensions, BlendMode);
                return;
            }
            else if (shadersToApply.Count == 1)
            {
                ScreenShader s = shadersToApply[0];
                BeginShaderMode(s.GetShader());
                DrawTexture(targetDimensions, BlendMode);
                EndShaderMode();
            }
            else if (shadersToApply.Count == 2)
            {
                if (screenBuffers.Loaded)
                {
                    ScreenShader s = shadersToApply[0];
                    screenBuffers.A.StartTextureMode();
                    BeginShaderMode(s.GetShader());
                    DrawTexture(GetTextureDimensions());
                    EndShaderMode();
                    screenBuffers.A.EndTextureMode();

                    s = shadersToApply[1];

                    BeginShaderMode(s.GetShader());
                    screenBuffers.A.DrawTexture(targetDimensions, BlendMode);
                    EndShaderMode();
                }
                
            }
            else
            {
                if (screenBuffers.Loaded)
                {
                    ScreenShader s = shadersToApply[0];
                    shadersToApply.RemoveAt(0);

                    ScreenShader endshader = shadersToApply[shadersToApply.Count - 1];
                    shadersToApply.RemoveAt(shadersToApply.Count - 1);

                    //draw game texture to first screenbuffer and first shader is already applied
                    screenBuffers.A.StartTextureMode();
                    BeginShaderMode(s.GetShader());
                    DrawTexture(GetTextureDimensions());
                    EndShaderMode();
                    screenBuffers.A.EndTextureMode();

                    int currentIndex = 0;
                    int nextIndex = 0;
                    for (int i = 0; i < shadersToApply.Count; i++)
                    {
                        s = shadersToApply[i];
                        nextIndex = currentIndex == 0 ? 1 : 0;
                        ScreenBuffer current = screenBuffers.GetByIndex(currentIndex);
                        ScreenBuffer next = screenBuffers.GetByIndex(nextIndex);
                        next.StartTextureMode();
                        BeginShaderMode(s.GetShader());
                        current.DrawTexture(GetTextureDimensions());
                        EndShaderMode();
                        next.EndTextureMode();
                        currentIndex = currentIndex == 0 ? 1 : 0;
                    }

                    BeginShaderMode(endshader.GetShader());
                    screenBuffers.GetByIndex(nextIndex).DrawTexture(targetDimensions, BlendMode);
                    EndShaderMode();
                }
                
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
        public Dimensions GetTextureDimensions() { return new(texture.texture.width, texture.texture.height); }
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
        public void Flash(float duration, Raylib_CsLo.Color startColor, Raylib_CsLo.Color endColor)
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
    */

}
