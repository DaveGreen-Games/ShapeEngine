﻿using Raylib_CsLo;
using System.Numerics;
using ShapeShaders;
using ShapeColor;

namespace ShapeScreen
{
    internal class ShaderFlash
    {
        private float duration = 0f;
        private float timer = 0f;
        private int shaderID;
        private bool shaderEnabled = false;
        public ShaderFlash(float dur, int shaderID)
        {
            this.shaderID = shaderID;
            duration = dur;
            timer = dur;

            if (timer > 0f)
            {
                ScreenHandler.SHADERS.EnableScreenShader(shaderID);
                shaderEnabled = true;
            }
        }

        public int GetShaderID() { return shaderID; }
        public float Percentage()
        {
            if (duration <= 0.0f) return 0f;
            return timer / duration;
        }
        public bool IsFinished() { return timer <= 0f; }
        public void Reset(float dur)
        {
            if (dur <= 0f)
            {
                Stop();
                return;
            }

            duration = dur;
            timer = dur;
            if (!shaderEnabled)
            {
                ScreenHandler.SHADERS.EnableScreenShader(shaderID);
                shaderEnabled = true;
            }
        }
        public void Stop()
        {
            timer = 0f;
            if (shaderEnabled)
            {
                ScreenHandler.SHADERS.DisableScreenShader(shaderID);
                shaderEnabled = false;
            }
        }
        public void Restart()
        {
            if (duration <= 0f) return;
            timer = duration;
            if (!shaderEnabled)
            {
                ScreenHandler.SHADERS.EnableScreenShader(shaderID);
                shaderEnabled = true;
            }
        }
        public void Update(float dt)
        {
            if (timer > 0.0f)
            {
                timer -= dt;
                if (timer <= 0.0f)
                {
                    ScreenHandler.SHADERS.DisableScreenShader(shaderID);
                    shaderEnabled = false;
                    timer = 0f;
                }
            }
        }

    }
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
                curColor = SColor.LerpColor(startColor, endColor, f);
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

    public static class ScreenHandler
    {
        public delegate void WindowSizeChanged(int w, int h);
        public static event WindowSizeChanged? OnWindowSizeChanged;

        public static float SCREEN_EFFECT_INTENSITY = 1.0f;
        public static float CAMERA_SHAKE_INTENSITY = 1.0f;
        public static float GAME_FACTOR { get; private set; }
        public static float UI_FACTOR { get; private set; }
        public static float UI_TO_GAME { get; private set; } = 1f;
        public static float GAME_TO_UI { get; private set; } = 1f;

        public static int FRAME_RATE_LIMIT { get; private set; } = 60;
        public static int FPS { get; private set; }
        public static bool VSYNC { get; private set; } = true;

        //private static (int width, int height) MONITOR_SIZE = (0, 0);
        //private static (int width, int height) DEFAULT_WINDOW_SIZE = (0, 0);
        public static (int width, int height) CUR_WINDOW_SIZE { get; private set; } = (0, 0);
        public static (int width, int height) WINDOWED_WINDOW_SIZE { get; private set; } = (0, 0);
        public static MonitorHandler MONITOR_HANDLER { get; private set; } = new();
        public static Camera CAMERA { get; private set; }
        public static ScreenTexture GAME { get; private set; }
        public static ScreenTexture UI { get; private set; }
        
        public static ShaderHandler SHADERS = new();

        private static Dictionary<int, ShaderFlash> shaderFlashes = new();
        private static ScreenBuffer[] screenBuffers = new ScreenBuffer[0];
        
        public static (int width, int height) DEVELOPMENT_RESOLUTION { get; private set; } = (0, 0);

        //private static Vector2 MONITOR_OFFSET = new();

        public static Rectangle CameraArea() { return CAMERA.GetCameraArea(); }
        public static Rectangle GameArea() { return new(0, 0, GAME.GetTextureWidth(), GAME.GetTextureHeight()); }
        public static Rectangle UIArea() { return new(0, 0, UI.GetTextureWidth(), UI.GetTextureHeight()); }
        public static Vector2 GameCenter() { return GameSize() / 2f; }
        public static Vector2 UICenter() { return UISize() / 2f; }
        public static Vector2 GameSize() { return new(GAME.GetTextureWidth(), GAME.GetTextureHeight()); }
        public static Vector2 UISize() { return new(UI.GetTextureWidth(), UI.GetTextureHeight()); }
        public static int GameWidth() { return GAME.GetTextureWidth(); }
        public static int GameHeight() { return GAME.GetTextureHeight(); }
        public static int UIWidth() { return UI.GetTextureWidth(); }
        public static int UIHeight() { return UI.GetTextureHeight(); }

        //public static Vector2 GetUIStretchFactor() { return UI.STRETCH_FACTOR * UI_FACTOR; }
        public static void Initialize(int devWidth, int devHeight, float gameSizeFactor = 1.0f, float uiSizeFactor = 1.0f, string windowName = "Raylib Game", bool fixedTexture = true, bool pixelSmoothing = false, bool hideCursor = false)
        {
            InitWindow(0, 0, windowName);
            if(hideCursor) HideCursor();
            SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            ClearWindowState(ConfigFlags.FLAG_VSYNC_HINT);

            FRAME_RATE_LIMIT = 60;
            SetVsync(true);


            MONITOR_HANDLER = new();
            GAME_FACTOR = gameSizeFactor;
            UI_FACTOR = uiSizeFactor;
            GAME_TO_UI = UI_FACTOR / GAME_FACTOR;
            UI_TO_GAME = GAME_FACTOR / UI_FACTOR;
            DEVELOPMENT_RESOLUTION = (devWidth, devHeight);

            //SetMonitorOffset();
            SetupWindowDimensions();

            GAME = new ScreenTexture(
                devWidth, devHeight,
                CUR_WINDOW_SIZE.width,
                CUR_WINDOW_SIZE.height,
                gameSizeFactor,
                fixedTexture
            );
            UI = new ScreenTexture(
                devWidth, devHeight,
                CUR_WINDOW_SIZE.width,
                CUR_WINDOW_SIZE.height,
                uiSizeFactor,
                fixedTexture
            );

            GAME.OnTextureSizeChanged += GameTextureSizeChanged;

            //CAMERA = new(GameSize(), 1f, GAME.STRETCH_AREA_FACTOR, 0f, -1f, 1.5f);
            CAMERA = new(GameSize(), 1f, GAME.STRETCH_AREA_SIDE_FACTOR, 0f, -1f, 1.5f);
            screenBuffers = new ScreenBuffer[]
            {
                new(ScreenHandler.GameWidth(), ScreenHandler.GameHeight(), ScreenHandler.GameWidth(), ScreenHandler.GameHeight()),
                new(ScreenHandler.GameWidth(), ScreenHandler.GameHeight(), ScreenHandler.GameWidth(), ScreenHandler.GameHeight())
            };
        }

        public static void Close()
        {
            foreach (ScreenBuffer screenBuffer in screenBuffers)
            {
                screenBuffer.Unload();
            }
            GAME.OnTextureSizeChanged -= GameTextureSizeChanged;
            screenBuffers = new ScreenBuffer[0];
            shaderFlashes.Clear();
            GAME.Close();
            UI.Close();
            SHADERS.Close();
            CloseWindow();
        }

        
        public static void UpdateCamera(float dt)
        {
            CAMERA.Update(dt);
        }
        public static void Update(float dt)
        {
            foreach (var shaderFlash in shaderFlashes.Values)
            {
                shaderFlash.Update(dt);
                if (shaderFlash.IsFinished()) shaderFlashes.Remove(shaderFlash.GetShaderID()); //does that work?
            }

            var newMonitor = MONITOR_HANDLER.HasMonitorSetupChanged();
            if (newMonitor.available)
            {
                MonitorChanged(newMonitor);
            }

            GAME.Update(dt);
            UI.Update(dt);

        }

        public static void Draw()
        {
            List<ScreenShader> shadersToApply = SHADERS.GetCurActiveShaders();
            if (shadersToApply.Count <= 0)
            {
                GAME.Draw();
                return;
            }
            else if (shadersToApply.Count == 1)
            {
                ScreenShader s = shadersToApply[0];
                BeginShaderMode(s.GetShader());
                GAME.Draw();
                EndShaderMode();
            }
            else if (shadersToApply.Count == 2)
            {
                ScreenShader s = shadersToApply[0];
                screenBuffers[0].StartTextureMode();
                BeginShaderMode(s.GetShader());
                GAME.DrawPro(ScreenHandler.GameWidth(), ScreenHandler.GameHeight());
                EndShaderMode();
                screenBuffers[0].EndTextureMode();

                s = shadersToApply[1];

                BeginShaderMode(s.GetShader());
                screenBuffers[0].DrawPro(CUR_WINDOW_SIZE.width, CUR_WINDOW_SIZE.height);
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
                GAME.DrawPro(GameWidth(), GameHeight());
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
                    current.DrawPro(GameWidth(), GameHeight());
                    EndShaderMode();
                    next.EndTextureMode();
                    currentIndex = currentIndex == 0 ? 1 : 0;
                }

                BeginShaderMode(endshader.GetShader());
                screenBuffers[nextIndex].DrawPro(CUR_WINDOW_SIZE.width, CUR_WINDOW_SIZE.height);
                EndShaderMode();
            }
        }
        public static void DrawUI()
        {
            UI.Draw();
        }





        public static Vector2 TransformPositionToUI(Vector2 gamePos)
        {
            return CAMERA.TransformPositionToUI(gamePos);
        }
        //public static Vector2 TransformPositionToUIRaw(Vector2 gamePos)
        //{
        //    return CAMERA.TransformPositionToUI(gamePos) / UI.STRETCH_FACTOR;
        //}
        public static Vector2 TransformPositionToGame(Vector2 uiPos)
        {
            return CAMERA.TransformPositionToGame(uiPos);
        }

        public static bool SetMonitor(int newMonitor)
        {
            var monitor = MONITOR_HANDLER.SetMonitor(newMonitor);
            if (monitor.available)
            {
                MonitorChanged(monitor);
                return true;
            }
            return false;
        }
        public static void NextMonitor()
        {
            var nextMonitor = MONITOR_HANDLER.Next();
            if (nextMonitor.available)
            {
                MonitorChanged(nextMonitor);
            }
        }



        private static void SetFPS(int newFps)
        {
            FPS = newFps;
            SetTargetFPS(FPS);
        }
        
        public static void SetFrameRateLimit(int newLimit)
        {
            if (newLimit < 30) newLimit = 30;
            else if (newLimit > 240) newLimit = 240;
            FRAME_RATE_LIMIT = newLimit;
            if (!IsVsyncEnabled())
            {
                SetFPS(FRAME_RATE_LIMIT);
            }
        }

        public static bool IsVsyncEnabled() { return VSYNC; }
        public static void SetVsync(bool enabled)
        {
            if (enabled)
            {
                VSYNC = true;
                SetFPS(MONITOR_HANDLER.CurMonitor().refreshrate);
            }
            else
            {
                VSYNC = false;
                SetFPS(FRAME_RATE_LIMIT);
            }
        }
        public static bool ToggleVsync()
        {
            SetVsync(!IsVsyncEnabled());
            return IsVsyncEnabled();
        }

        public static void ResizeWindow(int newWidth, int newHeight)
        {
            ChangeWindowDimensions(newWidth, newHeight, false);
        }
        

        public static void ResetWindow()
        {
            if (IsWindowFullscreen())
            {
                Raylib.ToggleFullscreen();
            }
            var monitor = MONITOR_HANDLER.CurMonitor();
            ChangeWindowDimensions(monitor.width / 2, monitor.height / 2, false);
        }

        public static bool IsFullscreen() { return IsWindowFullscreen(); }
        public static void SetFullscreen(bool enabled)
        {
            if (enabled && IsWindowFullscreen()) { return; }
            if (!enabled && !IsWindowFullscreen()) { return; }

            ToggleFullscreen();
        }
        public static bool ToggleFullscreen()
        {
            var monitor = MONITOR_HANDLER.CurMonitor();
            if (IsWindowFullscreen())
            {
                ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                if (WINDOWED_WINDOW_SIZE.width > monitor.width || WINDOWED_WINDOW_SIZE.height > monitor.height)
                {
                    WINDOWED_WINDOW_SIZE = (monitor.width / 2, monitor.height / 2);
                }

                ChangeWindowDimensions(WINDOWED_WINDOW_SIZE.width, WINDOWED_WINDOW_SIZE.height, false);
                ChangeWindowDimensions(WINDOWED_WINDOW_SIZE.width, WINDOWED_WINDOW_SIZE.height, false);//needed for some monitors ...
            }
            else
            {
                ChangeWindowDimensions(monitor.width, monitor.height, true);
                SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            }

            return IsFullscreen();
        }

        public static void ShaderFlash(float duration, params int[] shaderIDs)
        {
            if (shaderIDs.Length <= 0) return;

            foreach (var id in shaderIDs)
            {
                if (!SHADERS.HasScreenShader(id)) continue;
                if (shaderFlashes.ContainsKey(id))
                {
                    shaderFlashes[id].Reset(duration);
                }
                else
                {
                    shaderFlashes.Add(id, new(duration, id));
                }
            }
        }
        public static void StopAllShaderFlashes()
        {
            foreach (ShaderFlash shaderFlash in shaderFlashes.Values)
            {
                shaderFlash.Stop();
            }
            shaderFlashes.Clear();
        }
        public static void StartDraw(bool game = true)
        {
            if (game) GAME.BeginTextureMode(CAMERA);
            else UI.BeginTextureMode(null);
        }
        public static void EndDraw(bool game = true)
        {
            if (game) GAME.EndTextureMode(CAMERA);
            else UI.EndTextureMode(null);
        }
        public static void Flash(float duration, Color startColor, Color endColor, bool game = true)
        {
            byte startColorAlpha = (byte)(startColor.a * SCREEN_EFFECT_INTENSITY);
            startColor.a = startColorAlpha;
            byte endColorAlpha = (byte)(endColor.a * SCREEN_EFFECT_INTENSITY);
            endColor.a = endColorAlpha;
            if (game) GAME.Flash(duration, startColor, endColor);
            else UI.Flash(duration, startColor, endColor);
        }
        public static void FlashTint(float duration, Color color, bool game = true)
        {
            byte colorAlpha = (byte)(color.a * SCREEN_EFFECT_INTENSITY);
            color.a = colorAlpha;
            if (game) GAME.FlashTint(duration, color);
            else UI.FlashTint(duration, color);
        }



        private static void GameTextureSizeChanged(int w, int h, float factor)
        {
            CAMERA.ChangeSize(new Vector2(w, h), factor);
        }
        private static void MonitorChanged(MonitorHandler.MonitorInfo monitor)
        {
            int prevWidth = CUR_WINDOW_SIZE.width;
            int prevHeight = CUR_WINDOW_SIZE.height;
            
            if (IsWindowFullscreen())
            {
                SetWindowMonitor(monitor.index);
                ChangeWindowDimensions(monitor.width, monitor.height, true);
                SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            }
            else
            {
                int windowWidth = prevWidth;
                int windowHeight = prevHeight;
                if(windowWidth > monitor.width || windowHeight > monitor.height)
                {
                    windowWidth = monitor.width / 2;
                    windowHeight = monitor.height / 2;
                }
                ChangeWindowDimensions(monitor.width, monitor.height, true);
                SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                SetWindowMonitor(monitor.index);
                ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                ChangeWindowDimensions(windowWidth, windowHeight, false);
            }

            if(CUR_WINDOW_SIZE.width != prevWidth || CUR_WINDOW_SIZE.height != prevHeight)
            {
                GAME.ChangeWindowSize(CUR_WINDOW_SIZE.width, CUR_WINDOW_SIZE.height);
                UI.ChangeWindowSize(CUR_WINDOW_SIZE.width, CUR_WINDOW_SIZE.height);
            }

            if (IsVsyncEnabled())
            {
                SetFPS(monitor.refreshrate);
            }
        }
        private static void ChangeWindowDimensions(int newWidth, int newHeight, bool fullscreenChange = false)
        {
            //if (newWidth == CUR_WINDOW_SIZE.width && newHeight == CUR_WINDOW_SIZE.height) return;

            CUR_WINDOW_SIZE = (newWidth, newHeight);
            if (!fullscreenChange) WINDOWED_WINDOW_SIZE = (newWidth, newHeight);
            GAME.ChangeWindowSize(newWidth, newHeight);
            UI.ChangeWindowSize(newWidth, newHeight);

            SetWindowSize(newWidth, newHeight);
            var monitor = MONITOR_HANDLER.CurMonitor();

            int winPosX = monitor.width / 2 - newWidth / 2;
            int winPosY = monitor.height / 2 - newHeight / 2;
            //SetWindowPosition(winPosX + (int)MONITOR_OFFSET.X, winPosY + (int)MONITOR_OFFSET.Y);
            SetWindowPosition(winPosX + (int)monitor.position.X, winPosY + (int)monitor.position.Y);

            OnWindowSizeChanged?.Invoke(newWidth, newHeight);
        }
        private static void SetupWindowDimensions()
        {
            var monitor = MONITOR_HANDLER.CurMonitor();
            int newWidth = monitor.width / 2;
            int newHeight = monitor.height / 2;

            if (newWidth == CUR_WINDOW_SIZE.width && newHeight == CUR_WINDOW_SIZE.height) return;

            CUR_WINDOW_SIZE = (newWidth, newHeight);
            WINDOWED_WINDOW_SIZE = (newWidth, newHeight);

            SetWindowSize(newWidth, newHeight);
            int winPosX = monitor.width / 2 - newWidth / 2;
            int winPosY = monitor.height / 2 - newHeight / 2;
            //SetWindowPosition(winPosX + (int)MONITOR_OFFSET.X, winPosY + (int)MONITOR_OFFSET.Y);
            SetWindowPosition(winPosX + (int)monitor.position.X, winPosY + (int)monitor.position.Y);

            OnWindowSizeChanged?.Invoke(newWidth, newHeight);
        }





        


        //private static void UpdateMonitorRelevantInfo()
        //{
        //    //if (stretchMode)
        //    //{
        //    //    float fWidth = MONITOR_SIZE.width / (float)DEVELOPMENT_TARGET_RESOLUTION.width;
        //    //    float fHeight = MONITOR_SIZE.height / (float)DEVELOPMENT_TARGET_RESOLUTION.height;
        //    //    float f;
        //    //    if (fWidth <= fHeight)
        //    //    {
        //    //        f = MONITOR_SIZE.width / (float)DEVELOPMENT_TARGET_RESOLUTION.width;
        //    //    }
        //    //    else
        //    //    {
        //    //        f = MONITOR_SIZE.height / (float)DEVELOPMENT_TARGET_RESOLUTION.height;
        //    //    }
        //    //
        //    //    DEVELOPMENT_RESOLUTION.width = (int)(MONITOR_SIZE.width / f);
        //    //    DEVELOPMENT_RESOLUTION.height = (int)(MONITOR_SIZE.height / f);
        //    //}
        //    //else
        //    //{
        //    DEVELOPMENT_RESOLUTION = DEVELOPMENT_TARGET_RESOLUTION;
        //    //}
        //
        //    DEFAULT_WINDOW_SIZE.width = MONITOR_SIZE.width / 2;
        //    DEFAULT_WINDOW_SIZE.height = MONITOR_SIZE.height / 2;
        //    CUR_WINDOW_SIZE = DEFAULT_WINDOW_SIZE;
        //
        //}
        
        
        
        //private static void SetNativeResolution()
        //{
        //    if (monitorHandler == null) return;
        //    var size = monitorHandler.GetSize();
        //    MONITOR_SIZE = size;
        //    //MONITOR_SIZE.width = GetScreenWidth();
        //    //MONITOR_SIZE.height = GetScreenHeight();
        //}

        /*

        private static (int width, int height) DEVELOPMENT_TARGET_RESOLUTION = (0, 0);
        private static (int width, int height) DEVELOPMENT_RESOLUTION = (0, 0);
        private static (int width, int height) MONITOR_SIZE = (0, 0);
        private static (int width, int height) DEFAULT_WINDOW_SIZE = (0, 0);
        private static (int width, int height) CUR_WINDOW_SIZE = (0, 0);
        private static Vector2 MONITOR_OFFSET = new();

        private static Dictionary<string, ShaderFlash> shaderFlashes = new();
        //private static Dictionary<string, ScreenTexture> screenTextures = new Dictionary<string, ScreenTexture>();
        private static MonitorHandler monitorHandler = new();
        private static ScreenTexture gameTexture;
        private static ScreenTexture uiTexture;
        private static Camera camera;
        public static Camera Cam { get { return camera; } }
        public static ScreenTexture Game { get { return gameTexture; } }
        public static ScreenTexture UI { get { return uiTexture; } }
        private static ScreenBuffer[] screenBuffers = new ScreenBuffer[0];
        //public static bool IsStretchEnabled() { return stretchMode; }
        public static int GetMonitorSizeWidth() { return MONITOR_SIZE.width; }
        public static int GetMonitorSizeHeight() { return MONITOR_SIZE.height; }
        public static int GetDefaultWindowSizeWidth() { return DEFAULT_WINDOW_SIZE.width; }
        public static int GetDefaultWindowSizeHeight() { return DEFAULT_WINDOW_SIZE.height; }
        public static int GetCurWindowSizeWidth() { return CUR_WINDOW_SIZE.width; }
        public static int GetCurWindowSizeHeight() { return CUR_WINDOW_SIZE.height; }
        public static int GetDevelopmentResolutionWidth() { return DEVELOPMENT_RESOLUTION.width; }
        public static int GetDevelopmentResolutionHeight() { return DEVELOPMENT_RESOLUTION.height; }

        //public static ScreenTexture GetTexture(string name = "game") 
        //{
        //    if (!screenTextures.ContainsKey(name))
        //    {
        //        return screenTextures["game"];
        //    }
        //    return screenTextures[name];
        //}
        //private static int GameTextureWidth(string name = "game") {return GetTexture(name).GetTextureWidth(); }
        //private static int GameTextureHeight(string name = "game") { return GetTexture(name).GetTextureHeight(); }
        //private static Vector2 GameTextureSize(string name = "game") { return new(GameTextureWidth(name), GameTextureHeight(name)); }
        public static Vector2 UpdateTextureRelevantPosition(Vector2 pos, bool game = true)
        {
            if (game) return Game.UpdatePosition(pos);
            else return UI.UpdatePosition(pos);
        }
        public static Rectangle GameArea() { return new(0, 0, GameWidth(), GameHeight()); }
        public static Rectangle UIArea() { return new(0, 0, UIWidth(), UIHeight()); }
        public static Vector2 GameCenter() { return GameSize() / 2f; }
        public static Vector2 UICenter() { return UISize() / 2f; }
        public static Vector2 GameSize() { return new(DEVELOPMENT_RESOLUTION.width * GAME_FACTOR, DEVELOPMENT_RESOLUTION.height * GAME_FACTOR); }
        public static Vector2 UISize() { return new(DEVELOPMENT_RESOLUTION.width * UI_FACTOR, DEVELOPMENT_RESOLUTION.height * UI_FACTOR); }
        public static int GameWidth() { return (int)(DEVELOPMENT_RESOLUTION.width * GAME_FACTOR); }
        public static int GameHeight() { return (int)(DEVELOPMENT_RESOLUTION.height * GAME_FACTOR); }
        public static int UIWidth() { return (int)(DEVELOPMENT_RESOLUTION.width * UI_FACTOR); }
        public static int UIHeight() { return (int)(DEVELOPMENT_RESOLUTION.height * UI_FACTOR); }

        public static void Flash(float duration, Color startColor, Color endColor, bool game = true)
        {
            byte startColorAlpha = (byte)(startColor.a * SCREEN_EFFECT_INTENSITY);
            startColor.a = startColorAlpha;
            byte endColorAlpha = (byte)(endColor.a * SCREEN_EFFECT_INTENSITY);
            endColor.a = endColorAlpha;
            if (game) Game.Flash(duration, startColor, endColor);
            else UI.Flash(duration, startColor, endColor);
        }
        public static void FlashTint(float duration, Color color, bool game = true)
        {
            byte colorAlpha = (byte)(color.a * SCREEN_EFFECT_INTENSITY);
            color.a = colorAlpha;
            if (game) Game.FlashTint(duration, color);
            else UI.FlashTint(duration, color);
        }
        //public static void Shake(float duration, Vector2 strength, float smoothness = 0.75f, float scalePercentage = 0f, string name = "game")
        //{
        //    if (!screenTextures.ContainsKey(name)) return;
        //    GetTexture(name).Shake(duration, strength, smoothness, scalePercentage);
        //}
        public static void ShaderFlash(float duration, params string[] shaders)
        {
            if (shaders.Length <= 0) return;

            foreach (string shader in shaders)
            {
                if (!ShaderHandler.HasScreenShader(shader)) continue;
                if (shaderFlashes.ContainsKey(shader))
                {
                    shaderFlashes[shader].Reset(duration);
                }
                else
                {
                    shaderFlashes.Add(shader, new(duration, shader));
                }
            }
        }
        public static void StopAllShaderFlashes()
        {
            foreach (ShaderFlash shaderFlash in shaderFlashes.Values)
            {
                shaderFlash.Stop();
            }
            shaderFlashes.Clear();
        }
        public static void StartDraw(bool game = true)
        {
            if (game) Game.BeginTextureMode(camera);
            else UI.BeginTextureMode(null);
        }
        public static void EndDraw(bool game = true)
        {
            if (game) Game.EndTextureMode(camera);
            else UI.EndTextureMode(null);
        }
        public static Vector2 ScalePosition(int x, int y, bool game = true)
        {
            if (game) return Game.ScalePosition(x, y);
            else return UI.ScalePosition(x, y);
        }
        public static Vector2 ScalePositionV(Vector2 pos, bool game = true)
        {
            if (game) return Game.ScalePositionV(pos);
            else return UI.ScalePositionV(pos);
        }

        public static Vector2 TransformPositionToUI(Vector2 gamePos)
        {
            return camera.TransformPositionToUI(gamePos);
        }

        public static Vector2 TransformPositionToGame(Vector2 uiPos)
        {
            return camera.TransformPositionToGame(uiPos);
        }

        //public static float ScaleX(float x, string screen = "game") { return GetTexture(screen).GetCurResolutionFactorX() * x; }
        //public static float ScaleY(float y, string screen = "game") { return GetTexture(screen).GetCurResolutionFactorY() * y; }


        public static void Initialize(int devWidth, int devHeight, float gameSizeFactor = 1.0f, float uiSizeFactor = 1.0f, string windowName = "Raylib Game", int fps = 60, bool vsync = true, bool fullscreen = false, int monitor = 0, bool pixelSmoothing = false)
        {
            InitWindow(0, 0, windowName);
            HideCursor();
            //if(stretch)SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            monitorHandler = new();
            //stretchMode = stretch;
            GAME_FACTOR = gameSizeFactor;
            UI_FACTOR = uiSizeFactor;
            GAME_TO_UI = UI_FACTOR / GAME_FACTOR;
            UI_TO_GAME = GAME_FACTOR / UI_FACTOR;
            DEVELOPMENT_TARGET_RESOLUTION = (devWidth, devHeight);
            //var monitorSize = GetMonitorSize();
            SetNativeResolution();
            SetMonitorOffset();
            UpdateMonitorRelevantInfo();

            if (monitor > 0) SetMonitor(monitor);
            if (fullscreen) TimerHandler.Add(0.1f, () => SetFullscreen(true));
            CreateGameTexture();
            CreateUITexture();

            ChangeWindowDimensions(CUR_WINDOW_SIZE.width, CUR_WINDOW_SIZE.height);
            if (vsync) { EnableVsync(); }
            else { DisableVsync(); }
            SetFPS(fps);

            camera = new(GameSize(), 1f, 0f, -1f, 1.5f);
            camera.PIXEL_SMOOTHING_ENABLED = pixelSmoothing;

            screenBuffers = new ScreenBuffer[]
            {
                new(ScreenHandler.GameWidth(), ScreenHandler.GameHeight(), ScreenHandler.GameWidth(), ScreenHandler.GameHeight()),
                new(ScreenHandler.GameWidth(), ScreenHandler.GameHeight(), ScreenHandler.GameWidth(), ScreenHandler.GameHeight())
            };
        }
        public static void Close()
        {
            foreach (ScreenBuffer screenBuffer in screenBuffers)
            {
                screenBuffer.Unload();
            }
            screenBuffers = new ScreenBuffer[0];
            shaderFlashes.Clear();
            Game.Close();
            UI.Close();

            CloseWindow();
        }
        public static void UpdateCamera(float dt)
        {
            camera.Update(dt);
        }
        public static void Update(float dt)
        {
            foreach (var shaderFlash in shaderFlashes.Values)
            {
                shaderFlash.Update(dt);
                if (shaderFlash.IsFinished()) shaderFlashes.Remove(shaderFlash.GetShader()); //does that work?
            }

            if (monitorHandler != null)
            {
                //does work but I think it is better to make the window undecorated and therefore not moveable
                if (!IsWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED))
                {
                    var info = monitorHandler.HasIndexChanged();
                    if (info.changed)
                    {
                        SetNativeResolution();
                        if (info.newIndex == 0)
                        {
                            MONITOR_OFFSET = new Vector2(1, 1);
                            UpdateMonitorRelevantInfo();
                            UpdateMonitorRelevantInfo();
                            ChangeWindowDimensions(MONITOR_SIZE.width, MONITOR_SIZE.height);
                            ChangeWindowDimensions(DEFAULT_WINDOW_SIZE.width, DEFAULT_WINDOW_SIZE.height);
                        }
                        else
                        {
                            SetMonitor(info.oldIndex);
                            SetMonitor(info.newIndex);
                        }
                        //monitorChanged = true;
                    }
                }
                var newMonitor = monitorHandler.HasMonitorSetupChanged();
                if (newMonitor.available)
                {
                    MonitorChanged(false);
                }
            }

            Game.Update(dt);
            UI.Update(dt);
            
        }

        public static void Draw(List<ScreenShader> shadersToApply)
        {
            if (shadersToApply.Count <= 0)
            {
                Game.Draw();
                return;
            }
            else if (shadersToApply.Count == 1)
            {
                ScreenShader s = shadersToApply[0];
                BeginShaderMode(s.GetShader());
                Game.Draw();
                EndShaderMode();
            }
            else if (shadersToApply.Count == 2)
            {
                ScreenShader s = shadersToApply[0];
                screenBuffers[0].StartTextureMode();
                BeginShaderMode(s.GetShader());
                Game.DrawPro(ScreenHandler.GameWidth(), ScreenHandler.GameHeight());
                EndShaderMode();
                screenBuffers[0].EndTextureMode();

                s = shadersToApply[1];

                BeginShaderMode(s.GetShader());
                screenBuffers[0].DrawPro(ScreenHandler.GetCurWindowSizeWidth(), ScreenHandler.GetCurWindowSizeHeight());
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
                Game.DrawPro(GameWidth(), GameHeight());
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
                    current.DrawPro(GameWidth(), GameHeight());
                    EndShaderMode();
                    next.EndTextureMode();
                    currentIndex = currentIndex == 0 ? 1 : 0;
                }

                BeginShaderMode(endshader.GetShader());
                screenBuffers[nextIndex].DrawPro(GetCurWindowSizeWidth(), GetCurWindowSizeHeight());
                EndShaderMode();
            }
        }
        public static void DrawUI()
        {
            UI.Draw();
        }
        
        
        
        //public static void EndUpdate(float dt)
        //{
        //    monitorChanged = false;
        //}

       // public static bool HasMonitorChanged() { return stretchMode ? monitorChanged : false; }
        public static MonitorHandler GetMonitorHandler() { return monitorHandler; }
        //public static MonitorHandler.MonitorInfo GetCurMonitor()
        //{
        //    if (monitorHandler == null) return new();
        //    return monitorHandler.Get();
        //}
        //public static int GetCurMonitorCount()
        //{
        //    if (monitorHandler == null) return 1;
        //    else return monitorHandler.MonitorCount();
        //}
        //public static List<MonitorHandler.MonitorInfo> GetMonitorInfos()
        //{
        //    if (monitorHandler == null) return new() { };
        //    else return monitorHandler.GetAllMonitorInfo();
        //}
        //public static bool IsMonitorIndexValid(int index)
        //{
        //    if (monitorHandler == null) return false;
        //    else return monitorHandler.IsValidIndex(index);
        //}
        public static void NextMonitor()
        {
            if (monitorHandler == null) return;
            var nextMonitor = monitorHandler.Next();
            if (nextMonitor.available)
            {
                MonitorChanged(true);

            }
        }
        public static bool SetMonitor(int newMonitor)
        {
            if (monitorHandler == null) return false;
            var monitor = monitorHandler.SetMonitor(newMonitor);
            if (monitor.available)
            { 
                MonitorChanged(true);
                return true;
            }
            return false;
        }
        private static void MonitorChanged(bool setWindowMonitor = false)
        {
            if (IsWindowFullscreen())
            {
                if (setWindowMonitor && monitorHandler != null) SetWindowMonitor(monitorHandler.GetCurIndex());
                SetNativeResolution();
                SetMonitorOffset();
                UpdateMonitorRelevantInfo();
                UpdateMonitorRelevantTextures();
                ChangeWindowDimensions(MONITOR_SIZE.width, MONITOR_SIZE.height);
                SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            }
            else
            {
                //var monitorSize = GetMonitorSize();
                ChangeWindowDimensions(MONITOR_SIZE.width, MONITOR_SIZE.height);
                SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                if (setWindowMonitor && monitorHandler != null) SetWindowMonitor(monitorHandler.GetCurIndex());
                SetNativeResolution();
                SetMonitorOffset();
                UpdateMonitorRelevantInfo();
                UpdateMonitorRelevantTextures();
                ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                ChangeWindowDimensions(MONITOR_SIZE.width, MONITOR_SIZE.height);
                ChangeWindowDimensions(DEFAULT_WINDOW_SIZE.width, DEFAULT_WINDOW_SIZE.height);
            }
            //monitorChanged = true;
        }


        private static void UpdateMonitorRelevantInfo()
        {FTran
            //if (stretchMode)
            //{
            //    float fWidth = MONITOR_SIZE.width / (float)DEVELOPMENT_TARGET_RESOLUTION.width;
            //    float fHeight = MONITOR_SIZE.height / (float)DEVELOPMENT_TARGET_RESOLUTION.height;
            //    float f;
            //    if (fWidth <= fHeight)
            //    {
            //        f = MONITOR_SIZE.width / (float)DEVELOPMENT_TARGET_RESOLUTION.width;
            //    }
            //    else
            //    {
            //        f = MONITOR_SIZE.height / (float)DEVELOPMENT_TARGET_RESOLUTION.height;
            //    }
            //
            //    DEVELOPMENT_RESOLUTION.width = (int)(MONITOR_SIZE.width / f);
            //    DEVELOPMENT_RESOLUTION.height = (int)(MONITOR_SIZE.height / f);
            //}
            //else
            //{
            DEVELOPMENT_RESOLUTION = DEVELOPMENT_TARGET_RESOLUTION;
            //}

            DEFAULT_WINDOW_SIZE.width = MONITOR_SIZE.width / 2;
            DEFAULT_WINDOW_SIZE.height = MONITOR_SIZE.height / 2;
            CUR_WINDOW_SIZE = DEFAULT_WINDOW_SIZE;

        }
        private static void UpdateMonitorRelevantTextures()
        {
            Game.MonitorChanged(DEVELOPMENT_RESOLUTION.width, DEVELOPMENT_RESOLUTION.height, CUR_WINDOW_SIZE.width, CUR_WINDOW_SIZE.height);
            UI.MonitorChanged(DEVELOPMENT_RESOLUTION.width, DEVELOPMENT_RESOLUTION.height, CUR_WINDOW_SIZE.width, CUR_WINDOW_SIZE.height);
        }
        private static void SetMonitorOffset()
        {
            MONITOR_OFFSET = GetWindowPosition() + new Vector2(1, 1);
        }


        private static void CreateGameTexture() { gameTexture = CreateScreenTexture(GAME_FACTOR); }
        private static void CreateUITexture() { uiTexture = CreateScreenTexture(UI_FACTOR); }
        public static ScreenTexture CreateScreenTexture(float factor = 1.0f)
        {
            return new ScreenTexture(
                DEVELOPMENT_RESOLUTION.width,
                DEVELOPMENT_RESOLUTION.height,
                GetCurWindowSizeWidth(),
                GetCurWindowSizeHeight(),
                factor
            );
        }



        public static bool IsVsyncEnabled() { return IsWindowState(ConfigFlags.FLAG_VSYNC_HINT); }
        public static void EnableVsync()
        {
            if (IsWindowState(ConfigFlags.FLAG_VSYNC_HINT)) return;
            SetWindowState(ConfigFlags.FLAG_VSYNC_HINT);
        }
        public static void DisableVsync()
        {
            if (!IsWindowState(ConfigFlags.FLAG_VSYNC_HINT)) return;
            ClearWindowState(ConfigFlags.FLAG_VSYNC_HINT);
        }
        public static void SetVsync(bool enabled)
        {
            if (enabled) EnableVsync();
            else DisableVsync();
        }
        public static bool ToggleVsync()
        {
            if (IsVsyncEnabled())
            {
                DisableVsync();
            }
            else
            {
                EnableVsync();
            }
            return IsVsyncEnabled();
        }
        public static void SetFPS(int newFps)
        {
            FPS = newFps;
            SetTargetFPS(FPS);
        }

        public static bool IsFullscreen() { return IsWindowFullscreen(); }
        public static void SetFullscreen(bool enabled)
        {
            if (enabled && IsWindowFullscreen()) { return; }
            if (!enabled && !IsWindowFullscreen()) { return; }

            ToggleFullscreen();
        }
        public static bool ToggleFullscreen()
        {
            if (IsWindowFullscreen())
            {
                ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                //var monitorSize = GetMonitorSize();
                ChangeWindowDimensions(MONITOR_SIZE.width, MONITOR_SIZE.height);
                ChangeWindowDimensions(DEFAULT_WINDOW_SIZE.width, DEFAULT_WINDOW_SIZE.height);
            }
            else
            {
                //var monitorSize = GetMonitorSize();
                ChangeWindowDimensions(MONITOR_SIZE.width, MONITOR_SIZE.height);
                SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            }

            return IsFullscreen();
        }



        public static void ResetWindow()
        {
            if (IsWindowFullscreen())
            {
                Raylib.ToggleFullscreen();
            }
            ChangeWindowDimensions(DEFAULT_WINDOW_SIZE.width, DEFAULT_WINDOW_SIZE.height);
        }
        public static void SetDefaultWindowSize(int new_width, int new_height)
        {
            DEFAULT_WINDOW_SIZE.width = new_width;
            DEFAULT_WINDOW_SIZE.height = new_height;

            if (!IsWindowFullscreen())
            {
                ChangeWindowDimensions(new_width, new_height);
            }
        }
        private static void SetNativeResolution()
        {
            if (monitorHandler == null) return;
            var size = monitorHandler.GetSize();
            MONITOR_SIZE = size;
            //MONITOR_SIZE.width = GetScreenWidth();
            //MONITOR_SIZE.height = GetScreenHeight();
        }
        private static void ChangeWindowDimensions(int new_window_width, int new_window_height)
        {
            CUR_WINDOW_SIZE.width = new_window_width;
            CUR_WINDOW_SIZE.height = new_window_height;

            Game.ChangeWindowSize(CUR_WINDOW_SIZE.width, CUR_WINDOW_SIZE.height);
            UI.ChangeWindowSize(CUR_WINDOW_SIZE.width, CUR_WINDOW_SIZE.height);

            SetWindowSize(new_window_width, new_window_height);
            //var monitorSize = GetMonitorSize();
            int winPosX = MONITOR_SIZE.width / 2 - new_window_width / 2;
            int winPosY = MONITOR_SIZE.height / 2 - new_window_height / 2;
            SetWindowPosition(winPosX + (int)MONITOR_OFFSET.X, winPosY + (int)MONITOR_OFFSET.Y);
        }

        public static void DEBUG_DrawMonitorInfo(int x, int y, float fontSize)
        {
            var monitorInfo = monitorHandler.Get(); // GetCurMonitor();

            DrawText(string.Format("Name:{0}", monitorInfo.name), x, y, fontSize, RED);
            string text = string.Format("W:{0} - H:{1} - RR:{2} - Slot: {3}", monitorInfo.width, monitorInfo.height, monitorInfo.refreshrate, monitorInfo.index);
            DrawText(text, x, y + fontSize * 1.1f, fontSize, RED);
        }
        */
    }
}
