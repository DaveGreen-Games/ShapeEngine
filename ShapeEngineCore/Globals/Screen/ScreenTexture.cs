using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals.Screen
{
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
            OnTextureSizeChanged?.Invoke(width, height, STRETCH_AREA_FACTOR);
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
                //targetResolution = (winWidth, winHeight);
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
                curTint = Utils.LerpColor(flashTintStartColor, tint, f);
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

            //VARIANT 2
            /*
            foreach (var flash in screenFlashes)
            {
                flash.Update(dt);
            }
            screenFlashes.RemoveAll(flash => flash.IsFinished());
            */
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
        public void BeginTextureMode(Camera? camera = null)
        {
            Raylib.BeginTextureMode(texture);
            if (camera != null) BeginMode2D(camera.Cam);
            ClearBackground(backgroundColor);
        }
        public void EndTextureMode(Camera? camera = null)
        {
            foreach (var flash in screenFlashes)
            {
                if (camera != null)
                {
                    Vector2 sizeOffset = new(5f, 5f);
                    Vector2 center = camera.CameraPos;
                    Vector2 size = camera.CameraOffset * 2 * (1f / camera.CameraZoom);
                    Drawing.DrawRectangle(center, size + sizeOffset, new(0.5f, 0.5f), -camera.CameraRotDeg, flash.GetColor());
                }
                else DrawRectangle(-1, -1, GetTextureWidth() + 1, GetTextureHeight() + 1, flash.GetColor());
            }
            if (camera != null) EndMode2D();
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

}
