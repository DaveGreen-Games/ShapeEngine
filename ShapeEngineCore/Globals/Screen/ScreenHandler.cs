using Raylib_CsLo;
using System.Numerics;
using ShapeEngineCore.Globals.Timing;
using ShapeEngineCore.Globals.Shaders;

namespace ShapeEngineCore.Globals.Screen
{
    internal class ShaderFlash
    {
        private float duration = 0f;
        private float timer = 0f;
        private string shader;
        private bool shaderEnabled = false;
        public ShaderFlash(float dur, string shader)
        {
            this.shader = shader;
            duration = dur;
            timer = dur;

            if (timer > 0f)
            {
                ShaderHandler.EnableScreenShader(shader);
                shaderEnabled = true;
            }
        }

        public string GetShader() { return shader; }
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
                ShaderHandler.EnableScreenShader(shader);
                shaderEnabled = true;
            }
        }
        public void Stop()
        {
            timer = 0f;
            if (shaderEnabled)
            {
                ShaderHandler.DisableScreenShader(shader);
                shaderEnabled = false;
            }
        }
        public void Restart()
        {
            if (duration <= 0f) return;
            timer = duration;
            if (!shaderEnabled)
            {
                ShaderHandler.EnableScreenShader(shader);
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
                    ShaderHandler.DisableScreenShader(shader);
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
                curColor = Utils.LerpColor(startColor, endColor, f);
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
        public static float SCREEN_EFFECT_INTENSITY = 1.0f;
        public static float CAMERA_SHAKE_INTENSITY = 1.0f;
        public static float GAME_FACTOR { get; private set; }
        public static float UI_FACTOR { get; private set; }
        public static float UI_TO_GAME { get; private set; } = 1f;
        public static float GAME_TO_UI { get; private set; } = 1f;
        public static int FPS { get; private set; }
        //private static bool stretchMode = false;
        //private static bool monitorChanged = false;
        private static (int width, int height) DEVELOPMENT_TARGET_RESOLUTION = (0, 0);
        private static (int width, int height) DEVELOPMENT_RESOLUTION = (0, 0);
        private static (int width, int height) MONITOR_SIZE = (0, 0);
        private static (int width, int height) DEFAULT_WINDOW_SIZE = (0, 0);
        private static (int width, int height) CUR_WINDOW_SIZE = (0, 0);
        private static Vector2 MONITOR_OFFSET = new();
        private static MonitorHandler? monitorHandler = null;

        private static Dictionary<string, ShaderFlash> shaderFlashes = new();
        //private static Dictionary<string, ScreenTexture> screenTextures = new Dictionary<string, ScreenTexture>();
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
        public static MonitorHandler.MonitorInfo GetCurMonitor()
        {
            if (monitorHandler == null) return new();
            return monitorHandler.Get();
        }
        public static void NextMonitor()
        {
            if (monitorHandler == null) return;
            var nextMonitor = monitorHandler.Next();
            if (nextMonitor.available)
            {
                MonitorChanged(true);

            }
        }
        public static void SetMonitor(int newMonitor)
        {
            if (monitorHandler == null) return;
            var monitor = monitorHandler.SetMonitor(newMonitor);
            if (monitor.available) MonitorChanged(true);
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
        {
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
        public static void SetVsny(bool enabled)
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
            var monitorInfo = GetCurMonitor();

            DrawText(string.Format("Name:{0}", monitorInfo.name), x, y, fontSize, RED);
            string text = string.Format("W:{0} - H:{1} - RR:{2}", monitorInfo.width, monitorInfo.height, monitorInfo.refreshrate);
            DrawText(text, x, y + fontSize * 1.1f, fontSize, RED);
        }

    }
}
