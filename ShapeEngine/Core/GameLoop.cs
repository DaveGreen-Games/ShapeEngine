global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
using ShapeEngine.Screen;
using ShapeEngine.Timing;
using Raylib_CsLo;
using System.Numerics;
using System.Runtime.InteropServices;
using ShapeEngine.Lib;
using ShapeEngine.Core;
using System.Globalization;
using System.Text;

namespace ShapeEngine.Core
{

    public sealed class ShapeTexture
    {
        public static readonly float MinResolutionFactor = 0.25f;
        public static readonly float MaxResolutionFactor = 4f;
        
        public bool Valid { get; private set; } = false;
        public RenderTexture RenderTexture { get; private set; } = new();
        public int Width { get; private set; } = 0;
        public int Height { get; private set; } = 0;

        public ShapeTexture(){}

        public void Load(int w, int h)
        {
            if (Valid) return;
            Valid = true;
            SetTexture(w, h);
        }

        public void Unload()
        {
            if (!Valid) return;
            Valid = false;
            UnloadRenderTexture(RenderTexture);
        }
        
        public void Update(int w, int h)
        {
            if (!Valid) return;

            if (Width == w && Height == h) return;
            
            UnloadRenderTexture(RenderTexture);
            SetTexture(w, h);
        }
        public void Draw()
        {
            var destRec = new Rectangle
            {
            x = Width * 0.5f,
            y = Height * 0.5f,
            width = Width,
            height = Height
            };
            Vector2 origin = new()
            {
            X = Width * 0.5f,
            Y = Height * 0.5f
            };
            
            var sourceRec = new Rectangle(0, 0, Width, -Height);
            
            DrawTexturePro(RenderTexture.texture, sourceRec, destRec, origin, 0f, WHITE);
        }
        
        private void SetTexture(int w, int h)
        {
            Width = w;
            Height = h;
            RenderTexture = LoadRenderTexture(Width, Height);
        }
        //public void DrawTexture(int targetWidth, int targetHeight)
        //{
            //var destRec = new Rectangle
            //{
            //    x = targetWidth * 0.5f,
            //    y = targetHeight * 0.5f,
            //    width = targetWidth,
            //    height = targetHeight
            //};
            //Vector2 origin = new()
            //{
            //    X = targetWidth * 0.5f,
            //    Y = targetHeight * 0.5f
            //};
            //
            //
            //
            //var sourceRec = new Rectangle(0, 0, Width, -Height);
            //
            //DrawTexturePro(RenderTexture.texture, sourceRec, destRec, origin, 0f, WHITE);
        //
    }
    public sealed class ShapeCamera
    {
        public static float MinZoomLevel = 0.1f;
        public static float MaxZoomLevel = 10f;
        
        
        public Vector2 Position { get; set; }= new();
        public Vector2 Size { get; private set; }= new();
        public Vector2 Alignement{ get; private set; } = new(0.5f);
        public Vector2 Offset => Size * Alignement;
        public float ZoomLevel { get; private set; }= 1f;
        public float RotationDeg { get; private set; }= 0f;

        
        public ShapeCamera() { }
        public ShapeCamera(Vector2 pos)
        {
            this.Position = pos;
        }
        public ShapeCamera(Vector2 pos, Vector2 alignement)
        {
            this.Position = pos;
            this.SetAlignement(alignement);
        }
        public ShapeCamera(Vector2 pos, Vector2 alignement, float zoomLevel)
        {
            this.Position = pos;
            this.SetAlignement(alignement);
            this.SetZoom(zoomLevel);
        }
        public ShapeCamera(Vector2 pos, Vector2 alignement, float zoomLevel, float rotationDeg)
        {
            this.Position = pos;
            this.SetAlignement(alignement);
            this.SetZoom(zoomLevel);
            this.SetRotation(rotationDeg);
        }
        public ShapeCamera(Vector2 pos, float zoomLevel)
        {
            this.Position = pos;
            this.SetZoom(zoomLevel);
        }

        public Rectangle Area => new
        (
            Position.X - Offset.X * ZoomFactor, 
            Position.Y - Offset.Y * ZoomFactor, 
            Size.X * ZoomFactor,
            Size.Y * ZoomFactor
        );
        public Camera2D Camera => new()
        {
            target = Position,
            offset = Offset,
            zoom = ZoomLevel,
            rotation = RotationDeg
        };

        public float ZoomFactor => 1f / ZoomLevel;

        
        public void Update(float dt, int screenWidth, int screenHeight)
        {
            Size = new(screenWidth, screenHeight);
        }
        
        public void Reset()
        {
            Position = new();
            Alignement = new(0.5f);
            ZoomLevel = 1f;
            RotationDeg = 0f;
        }
        
        public void Zoom(float change) => SetZoom(ZoomLevel + change);
        public void SetZoom(float zoomLevel)
        {
            ZoomLevel = zoomLevel;
            if (ZoomLevel > MaxZoomLevel) ZoomLevel = MaxZoomLevel;
            else if (ZoomLevel < MinZoomLevel) ZoomLevel = MinZoomLevel;
        }

        public void Rotate(float deg) => SetRotation(RotationDeg + deg);
        public void SetRotation(float deg)
        {
            RotationDeg = deg;
            RotationDeg = Wrap(RotationDeg, 0f, 360f);
        }

        public void SetAlignement(Vector2 newAlignement) => Alignement = Vector2.Clamp(newAlignement, Vector2.Zero, Vector2.One);

        public Vector2 ScreenToWorld(Vector2 pos) => GetScreenToWorld2D(pos, Camera);
        public Vector2 WorldToScreen(Vector2 pos) => GetWorldToScreen2D(pos, Camera);
        private static float Wrap(float value, float min, float max) => value - (max - min) * MathF.Floor((float) (( value -  min) / ( max -  min)));
    }

    public class ShapeLoop
    {
        
    }
    
    
    /// <summary>
    /// Basic GameLoop class. Inherit this class override various methods for running your game/app. 
    /// Create your inherited GameLoop class, call CreateWindow and then Run() to start. 
    /// Run returns an exit code once the application is finished.
    /// </summary>
    public abstract class GameLoop
    {
        public static readonly string CURRENT_DIRECTORY = Environment.CurrentDirectory;
        public static bool EDITORMODE { get; private set; } = Directory.Exists("resources");

        public static OSPlatform OS_PLATFORM { get; private set; } =
           RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSPlatform.Windows :
           RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux :
           RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatform.OSX :
                                                                    OSPlatform.FreeBSD;
        public static bool IsWindows() { return OS_PLATFORM == OSPlatform.Windows; }
        public static bool IsLinux() { return OS_PLATFORM == OSPlatform.Linux; }
        public static bool IsOSX() { return OS_PLATFORM == OSPlatform.OSX; }


        public string[] LAUNCH_PARAMS { get; protected set; } = new string[0];
        public Vector2 MousePos { get; private set; } = new(0f);
        public Raylib_CsLo.Color BackgroundColor = BLACK;
        public float ScreenEffectIntensity = 1.0f;

        protected bool quit = false;
        protected bool restart = false;

        protected ScreenTextures screenTextures = new();

        private List<DeferredInfo> deferred = new();


        public delegate void WindowSizeChanged(Dimensions newDimensions);
        public event WindowSizeChanged? OnWindowSizeChanged;

        public int FrameRateLimit { get; private set; } = 60;

        public float DeltaTarget { get { return 1f / (float)FPSTarget; } }
        public float DeltaCurrent { get; private set; }
        public int FPSCurrent { get; private set; }
        public int FPSTarget { get; private set; }

        public bool VSync { get; private set; } = true;
        public Dimensions CurWindowSize { get; private set; } = new();
        public Dimensions WindowedWindowSize { get; private set; } = new();
        public Dimensions WindowMinSize { get; } = new (128, 128);

        private bool windowMaximized = false;
        private bool resizableState = false;
        private bool undecoratedState = false;

        //public float deltaCriticalTime = 0f;
        //public int skipDrawCount = 0;

        public MonitorDevice Monitor { get; private set; }
        public ICursor Cursor { get; private set; } = new NullCursor();

        public GameLoop()
        {
            InitWindow(0, 0, "");
            
            ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);

            ClearWindowState(ConfigFlags.FLAG_VSYNC_HINT);

            Monitor = new MonitorDevice();

            SetupWindowDimensions();

            FrameRateLimit = 60;
            SetVsync(true);
            Raylib.SetWindowMinSize(WindowMinSize.Width, WindowMinSize.Height);
        }
        public void SetupWindow(string windowName, bool undecorated, bool resizable)
        {
            SetWindowTitle(windowName);
            if (undecorated) SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            else ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);

            if (resizable) SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            else ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);

            undecoratedState = undecorated;
            resizableState = resizable;
        }

        /// <summary>
        /// Starts the gameloop. Runs until Quit() or Restart() is called or the Window is closed by the user.
        /// </summary>
        /// <returns>Returns an exit code for information how the application was quit. Restart has to be handled seperately.</returns>
        public ExitCode Run(params string[] launchParameters)
        {
            this.LAUNCH_PARAMS = launchParameters;

            quit = false;
            restart = false;
            Raylib.SetExitKey(-1);

            StartGameloop();
            RunGameloop();
            EndGameloop();
            CloseWindow();

            return new ExitCode(restart);
        }


        /// <summary>
        /// Quit the application with the exit code restart.
        /// </summary>
        public void Restart()
        {
            restart = true;
            quit = true;
        }
        /// <summary>
        /// Quit the application at the end of the frame.
        /// </summary>
        public void Quit()
        {
            restart = false;
            quit = true;
        }

        /// <summary>
        /// The action is called at the end of the frame or at the end after afterFrames.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="afterFrames">How many frames have to pass before the action is called.</param>
        public void CallDeferred(Action action, int afterFrames = 0)
        {
            deferred.Add(new(action, afterFrames));
        }
        private void ResolveDeferred()
        {
            for (int i = deferred.Count - 1; i >= 0; i--)
            {
                var info = deferred[i];
                if (info.Call()) deferred.RemoveAt(i);
            }
        }

        
        protected void Flash(ScreenTexture texture, float duration, Raylib_CsLo.Color startColor, Raylib_CsLo.Color endColor)
        {
            byte startColorAlpha = (byte)(startColor.a * ScreenEffectIntensity);
            startColor.a = startColorAlpha;
            byte endColorAlpha = (byte)(endColor.a * ScreenEffectIntensity);
            endColor.a = endColorAlpha;

            texture.Flash(duration, startColor, endColor);
        }
        public bool SwitchCursor(ICursor newCursor)
        {
            if (Cursor != newCursor)
            {
                Cursor.Deactivate();
                newCursor.Activate(Cursor);
                Cursor = newCursor;
                return true;
            }
            return false;
        }
        private void DrawCursor(Vector2 screenSize, Vector2 pos) { if (Cursor != null) Cursor.Draw(screenSize, pos); }

        public ScreenTexture AddScreenTexture(Dimensions dimensions, int drawOrder)
        {
            var newScreenTexture = new ScreenTexture(dimensions, drawOrder);

            AddScreenTexture(newScreenTexture);

            return newScreenTexture;
        }
        public bool AddScreenTexture(ScreenTexture newScreenTexture)
        {
            if (screenTextures.ContainsKey(newScreenTexture.ID)) return false;
            newScreenTexture.AdjustSize(CurWindowSize);
            screenTextures.Add(newScreenTexture.ID, newScreenTexture);
            return true;
        }
        public bool RemoveScreenTexture(ScreenTexture screenTexture) { return screenTextures.Remove(screenTexture.ID); }
        public bool RemoveScreenTexture(uint id) { return screenTextures.Remove(id); }

        //public ScreenTexture AddScreenTexture(int width, int height, int drawOrder)
        //{
        //    var newScreenTexture = new ScreenTexture(width, height, drawOrder);
        //
        //    AddScreenTexture(newScreenTexture);
        //
        //    return newScreenTexture;
        //}
        //public bool AddScreenTexture(ScreenTexture newScreenTexture)
        //{
        //    if (screenTextures.ContainsKey(newScreenTexture.ID)) return false;
        //    newScreenTexture.AdjustSize(CurWindowSize.width, CurWindowSize.height);
        //    screenTextures.Add(newScreenTexture.ID, newScreenTexture);
        //    return true;
        //}
        //public bool RemoveScreenTexture(ScreenTexture screenTexture) { return screenTextures.Remove(screenTexture.ID); }
        //public bool RemoveScreenTexture(uint id) { return screenTextures.Remove(id); }


        private void StartGameloop()
        {
            LoadContent();
            BeginRun();
        }
        private void RunGameloop()
        {
            while (!quit)
            {
                if (WindowShouldClose())
                {
                    Quit();
                    continue;
                }

                CheckWindowSizeChanged();

                var mp = GetMousePosition();
                if (!mp.IsNan()) SetMousePos(mp);


                float dt = GetFrameTime();

                DeltaCurrent = dt;
                FPSCurrent = (int) (1f / dt);

                
                UpdateMonitorDevice(dt);

                var activeScreenTextures = screenTextures.GetActive();
                activeScreenTextures.SortDrawOrder();

                UpdateScreenTextures(activeScreenTextures, dt);

                Update(dt);
                DrawGameloopToTextures(activeScreenTextures);
                DrawGameloopToScreen(activeScreenTextures);

                ResolveDeferred();
            }
        }
        private void EndGameloop()
        {
            EndRun();
            UnloadContent();
            foreach (var st in screenTextures.GetAll())
            {
                st.Close();
            }
        }
        private void UpdateMonitorDevice(float dt)
        {

            var newMonitor = Monitor.HasMonitorSetupChanged();
            if (newMonitor.Available)
            {
                MonitorChanged(newMonitor);
            }
        }
        private void UpdateScreenTextures(ActiveScreenTextures sortedTextures, float dt)
        {
            foreach (var st in sortedTextures)
            {
                st.MousePos = st.ScalePosition(MousePos, CurWindowSize.ToVector2());
                st.Update(dt);
            }
        }
        private void DrawGameloopToTextures(ActiveScreenTextures sortedTextures)
        {
            foreach (var st in sortedTextures)
            {
                st.BeginTextureMode();
                DrawToScreenTexture(st);
                st.EndTextureMode();
            }
        }
        private void DrawGameloopToScreen(ActiveScreenTextures sortedTextures)
        {

            Vector2 curScreenSize = CurWindowSize.ToVector2();
            BeginDrawing();
            ClearBackground(BackgroundColor);

            foreach (var st in sortedTextures)
            {
                st.DrawToScreen(CurWindowSize);
            }

            DrawToScreen(curScreenSize, MousePos);

            DrawCursor(curScreenSize, MousePos);

            EndDrawing();
        }
       
        private void SetMousePos(Vector2 newPos)
        {
            MousePos = newPos;
        }
        private void CheckWindowSizeChanged()
        {
            if (IsFullscreen()) return;

            //int w = GetScreenWidth();
            //int h = GetScreenHeight();
            Dimensions screenDim = new(GetScreenWidth(), GetScreenHeight());

            var monitor = Monitor.CurMonitor();
            Dimensions maxDim = monitor.Dimensions;
            //int maxW = monitor.Width;
            //int maxH = monitor.Height;

            if (CurWindowSize.Width != screenDim.Width || CurWindowSize.Height != screenDim.Height)
            {
                var newDim = Dimensions.Clamp(screenDim, WindowMinSize, maxDim);
                //int newW = SUtils.Clamp(w, WindowMinSize.Width, maxW);
                //int newH = SUtils.Clamp(h, WindowMinSize.Height, maxH);
                CurWindowSize = newDim;// new Dimension(newW, newH);

                WindowedWindowSize = CurWindowSize;

                OnWindowSizeChanged?.Invoke(newDim);

                foreach (var st in screenTextures.GetAll())
                {
                    st.AdjustSize(CurWindowSize);
                }
            }
        }

        public ActiveScreenTextures GetActiveScreenTextures(ScreenTextureMask mask)
        {
            return screenTextures.GetActive(mask);
        }

        //protected List<ScreenTexture> GetScreenTextures() { return screenTextures.Values.ToList(); }
        //protected List<ScreenTexture> GetActiveScreenTextures() { return screenTextures.Values.Where((st) => st.Active).ToList(); }
        

        /// <summary>
        /// Called first after starting the gameloop.
        /// </summary>
        protected virtual void LoadContent() { }
        /// <summary>
        /// Called after LoadContent but before the main loop has started.
        /// </summary>
        protected virtual void BeginRun() { }

        //protected virtual void HandleInput(float dt) { }
        protected virtual void Update(float dt) { }
        protected virtual void DrawToScreenTexture(ScreenTexture screenTexture) { }
        protected virtual void DrawToScreen(Vector2 size, Vector2 mousePos) { }

        /// <summary>
        /// Called before UnloadContent is called after the main gameloop has been exited.
        /// </summary>
        protected virtual void EndRun() { }
        /// <summary>
        /// Called after EndRun before the application terminates.
        /// </summary>
        protected virtual void UnloadContent() { }



        public bool SetMonitor(int newMonitor)
        {
            var monitor = Monitor.SetMonitor(newMonitor);
            if (monitor.Available)
            {
                MonitorChanged(monitor);
                return true;
            }
            return false;
        }
        public void NextMonitor()
        {
            var nextMonitor = Monitor.NextMonitor();
            if (nextMonitor.Available)
            {
                MonitorChanged(nextMonitor);
            }
        }
        /// <summary>
        /// Set the new frame rate limit. Only takes affect if Vsync is off!
        /// </summary>
        /// <param name="newLimit">Limit is clamped between 30 and 240!</param>
        public void SetFrameRateLimit(int newLimit)
        {
            if (newLimit < 30) newLimit = 30;
            else if (newLimit > 240) newLimit = 240;
            FrameRateLimit = newLimit;
            if (!VSync)
            {
                SetFPS(FrameRateLimit);
            }
        }
        
        private void SetFPS(int newFps)
        {
            FPSTarget = newFps;
            SetTargetFPS(FPSTarget);
        }
        public void SetVsync(bool enabled)
        {
            if (enabled)
            {
                VSync = true;
                SetFPS(Monitor.CurMonitor().Refreshrate);
            }
            else
            {
                VSync = false;
                SetFPS(FrameRateLimit);
            }
        }
        public bool ToggleVsync()
        {
            SetVsync(!VSync);
            return VSync;
        }
        public void ResizeWindow(Dimensions newDimensions)
        {
            ChangeWindowDimensions(newDimensions, false);
        }
        public void ResetWindow()
        {
            if (IsWindowFullscreen())
            {
                Raylib.ToggleFullscreen();
            }
            var monitor = Monitor.CurMonitor();
            ChangeWindowDimensions(monitor.Dimensions / 2, false);
        }
        public bool IsFullscreen() { return IsWindowFullscreen(); }
        public void SetFullscreen(bool enabled)
        {
            if (enabled && IsWindowFullscreen()) { return; }
            if (!enabled && !IsWindowFullscreen()) { return; }

            ToggleFullscreen();
        }
        public bool ToggleFullscreen()
        {
            var monitor = Monitor.CurMonitor();
            if (IsWindowFullscreen())
            {
                ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                if (WindowedWindowSize.Width > monitor.Width || WindowedWindowSize.Height > monitor.Height)
                {
                    WindowedWindowSize = monitor.Dimensions / 2; //new Dimension(monitor.width / 2, monitor.height / 2);
                }

                ChangeWindowDimensions(WindowedWindowSize, false);
                ChangeWindowDimensions(WindowedWindowSize, false);//needed for some monitors ...
            }
            else
            {
                ChangeWindowDimensions(monitor.Dimensions, true);
                SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            }

            return IsFullscreen();
        }

        public bool IsWindowUndecorated() { return undecoratedState; }
        public bool IsWindowResizable() { return resizableState; }

        public void SetWindowUndecorated(bool undecorated)
        {
            if (windowMaximized) return;
            if (undecoratedState == undecorated) return;
            if (undecorated)
            {
                undecoratedState = true;
                SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            }
            else
            {
                undecoratedState = false;
                ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            }
        }
        public void SetWindowResizable(bool resizable)
        {
            if (windowMaximized) return;
            if (resizableState == resizable) return;
            if (resizable)
            {
                resizableState= true;
                SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            }
            else
            {
                resizableState = false;
                ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            }
        }

        public bool ToggleWindowUndecorated() 
        { 
            SetWindowUndecorated(!undecoratedState); 
            return undecoratedState; 
        }
        public bool ToggleWindowResizable()
        {
            SetWindowResizable(!resizableState);
            return resizableState;
        }
        
        public bool IsWindowMaximized() { return windowMaximized; }
        public void SetWindowMaximized(bool maximized)
        {
            if (windowMaximized == maximized) return;

            if (maximized)
            {
                windowMaximized = true;
                var monitor = Monitor.CurMonitor();

                if(resizableState) ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
                if(!undecoratedState) SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
                ChangeWindowDimensions(monitor.Dimensions, true);
            }
            else
            {
                windowMaximized = false;
                ChangeWindowDimensions(WindowedWindowSize, true);
                if(resizableState) SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
                if(!undecoratedState) ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);



            }
        }
        public bool ToggleWindowMaximize()
        {
            SetWindowMaximized(!windowMaximized);
            return windowMaximized;
        }

        private void MonitorChanged(MonitorInfo monitor)
        {
            //int prevWidth = CurWindowSize.width;
            //int prevHeight = CurWindowSize.height;
            var prevDimensions = CurWindowSize;

            if (IsWindowFullscreen())
            {
                SetWindowMonitor(monitor.Index);
                ChangeWindowDimensions(monitor.Dimensions, true);
                SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            }
            else
            {
                //int windowWidth = prevWidth;
                //int windowHeight = prevHeight;
                var windowDimensions = prevDimensions;
                if (windowDimensions.Width > monitor.Width || windowDimensions.Height > monitor.Height)
                {
                    //windowWidth = monitor.Width / 2;
                    //windowHeight = monitor.Height / 2;
                    windowDimensions = monitor.Dimensions / 2;
                }
                ChangeWindowDimensions(monitor.Dimensions, true);
                SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                SetWindowMonitor(monitor.Index);
                ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                ChangeWindowDimensions(windowDimensions, false);
            }
            if (VSync)
            {
                SetFPS(monitor.Refreshrate);
            }
        }
        private void ChangeWindowDimensions(Dimensions dimensions, bool fullscreenChange = false)
        {
            //if (newWidth == CUR_WINDOW_SIZE.width && newHeight == CUR_WINDOW_SIZE.height) return;

            CurWindowSize = dimensions; // (newWidth, newHeight);
            if (!fullscreenChange) WindowedWindowSize = dimensions; // (newWidth, newHeight);
            //GAME.ChangeWindowSize(newWidth, newHeight);
            //UI.ChangeWindowSize(newWidth, newHeight);

            SetWindowSize(dimensions.Width, dimensions.Height);
            var monitor = Monitor.CurMonitor();

            int winPosX = monitor.Width / 2 - dimensions.Width / 2;
            int winPosY = monitor.Height / 2 - dimensions.Height / 2;
            //SetWindowPosition(winPosX + (int)MONITOR_OFFSET.X, winPosY + (int)MONITOR_OFFSET.Y);
            SetWindowPosition(winPosX + (int)monitor.Position.X, winPosY + (int)monitor.Position.Y);

            OnWindowSizeChanged?.Invoke(dimensions);
            
            foreach (var st in screenTextures.GetAll())
            {
                st.AdjustSize(CurWindowSize);
            }
        }
        private void SetupWindowDimensions()
        {
            var monitor = Monitor.CurMonitor();
            //int newWidth = monitor.width / 2;
            //int newHeight = monitor.height / 2;
            Dimensions newDimensions = monitor.Dimensions / 2;
            if (newDimensions == CurWindowSize) return;
            //if (newWidth == CurWindowSize.width && newHeight == CurWindowSize.height) return;

            CurWindowSize = newDimensions; // (newWidth, newHeight);
            WindowedWindowSize = newDimensions; // (newWidth, newHeight);

            SetWindowSize(newDimensions.Width, newDimensions.Height);
            int winPosX = monitor.Width / 2 - newDimensions.Width / 2;
            int winPosY = monitor.Height / 2 - newDimensions.Height / 2;
            //SetWindowPosition(winPosX + (int)MONITOR_OFFSET.X, winPosY + (int)MONITOR_OFFSET.Y);
            SetWindowPosition(winPosX + (int)monitor.Position.X, winPosY + (int)monitor.Position.Y);

            OnWindowSizeChanged?.Invoke(newDimensions);
        }

        private void WriteDebugInfo()
        {
            Console.WriteLine("--------Shape Engine Monitor Info--------");
            if(IsFullscreen())Console.WriteLine("Fullscreen is Enabled");
            else Console.WriteLine("Fullscreen is Disabled");
            
            if(IsWindowMaximized()) Console.WriteLine("Window is Maximized");
            else Console.WriteLine("Window is NOT Maximized");
            
            var dpi = Raylib.GetWindowScaleDPI();
            Console.WriteLine($"DPI: {dpi.X}/{dpi.Y}");

            var sWidth = Raylib.GetScreenWidth();
            var sHeight = Raylib.GetScreenHeight();
            Console.WriteLine($"Screen: {sWidth}/{sHeight}");

            var monitor = Raylib.GetCurrentMonitor();
            var mWidth = Raylib.GetMonitorWidth(monitor);
            var mHeight = Raylib.GetMonitorHeight(monitor);
            var mpWidth = Raylib.GetMonitorPhysicalWidth(monitor);
            var mpHeight = Raylib.GetMonitorPhysicalHeight(monitor);
            Console.WriteLine($"[{monitor}] Monitor: {mWidth}/{mHeight} Physical: {mpWidth}/{mpHeight}");


            var rWidth = Raylib.GetRenderWidth();
            var rHeight = Raylib.GetRenderHeight();
            Console.WriteLine($"Render Size: {rWidth}/{rHeight}");

            Monitor.CurMonitor().WriteDebugInfo();
            Console.WriteLine("---------------------------------------");
        }
    }

    /// <summary>
    /// Uses a distinct game and ui texture and the scene system. 
    /// </summary>
    public abstract class GameLoopScene : GameLoop
    {
        //public static readonly uint GAME_ID = 800;
        //public static readonly uint UI_ID = 900;

        /// <summary>
        /// Screen texture for drawing the game. Default draw order is 0. Default is basic camera.
        /// </summary>
        public ScreenTexture Game { get; private set; }
        /// <summary>
        /// Screen texture for drawing the ui. Default draw order is 1 (after game). Default is no camera.
        /// </summary>
        public ScreenTexture UI { get; private set; }
        /// <summary>
        /// The cur scene that is used. Only 1 scene can be active at any time. Use GoToScene function for changing between scenes.
        /// </summary>
        public IScene CurScene { get; private set; } = new SceneEmpty();

        public GameLoopScene(Dimensions gameDimensions, Dimensions uiDimensions) : base()
        {
            Game = new ScreenTexture(gameDimensions, 10);
            UI = new ScreenTexture(uiDimensions, 100);
            
            Game.SetCamera(new BasicCamera(new(0f), Game.GetSize(), new(0.5f), 1f, 0f));
            
            AddScreenTexture(Game);
            AddScreenTexture(UI);
        }

        /// <summary>
        /// Switches to the new scene. Deactivate is called on the old scene and then Activate is called on the new scene.
        /// </summary>
        /// <param name="newScene"></param>
        public void GoToScene(IScene newScene)
        {
            if (newScene == null) return;
            if (newScene == CurScene) return;
            CurScene.Deactivate();
            newScene.Activate(CurScene);
            CurScene = newScene;
        }

        protected override void Update(float dt)
        {
            CurScene.Update(dt, MousePos, Game, UI);
            //var area = CurScene.GetCurArea();
            //if (area != null) area.Update(dt, MousePos, Game, UI);
        }
        protected override void DrawToScreenTexture(ScreenTexture screenTexture)
        {
            //var area = CurScene.GetCurArea();
            

            if (screenTexture == Game)
            {
                Vector2 size = screenTexture.GetSize();
                Vector2 mousePos = screenTexture.MousePos;
                uint id = screenTexture.ID;

                CurScene.DrawGame(size, mousePos);
                //if (area != null) area.DrawGame(size, mousePos);
            }

            else if (screenTexture == UI)
            {
                Vector2 size = screenTexture.GetSize();
                Vector2 mousePos = screenTexture.MousePos;
                uint id = screenTexture.ID;

                CurScene.DrawUI(size, mousePos);
                //if (area != null) area.DrawUI(size, mousePos);
            }
            else
            {
                CurScene.DrawToTexture(screenTexture);
                //if (area != null) area.DrawToTexture(screenTexture);

            }
        }
        protected override void DrawToScreen(Vector2 size, Vector2 mousePos)
        {
            CurScene.DrawToScreen(size, mousePos);
            //var area = CurScene.GetCurArea();
            //if (area != null) area.DrawToScreen(size, mousePos);
        }
    
    }

    public abstract class GameLoopScene<TScene> : GameLoop where TScene : IScene
    {
        /// <summary>
        /// Screen texture for drawing the game. Default draw order is 0. Default is basic camera.
        /// </summary>
        public ScreenTexture Game { get; private set; }
        /// <summary>
        /// Screen texture for drawing the ui. Default draw order is 1 (after game). Default is no camera.
        /// </summary>
        public ScreenTexture UI { get; private set; }
        /// <summary>
        /// The cur scene that is used. Only 1 scene can be active at any time. Use GoToScene function for changing between scenes.
        /// </summary>
        public TScene CurScene { get; private set; }

        public GameLoopScene(Dimensions gameDimensions, Dimensions uiDimensions, TScene startScene) : base()
        {
            Game = new ScreenTexture(gameDimensions, 0);
            UI = new ScreenTexture(uiDimensions, 100);

            Game.SetCamera(new BasicCamera(new(0f), Game.GetSize(), new(0.5f), 1f, 0f));

            AddScreenTexture(Game);
            AddScreenTexture(UI);
            CurScene = startScene;
        }

        /// <summary>
        /// Switches to the new scene. Deactivate is called on the old scene and then Activate is called on the new scene.
        /// </summary>
        /// <param name="newScene"></param>
        public void GoToScene(TScene newScene)
        {
            if (newScene == null) return;
            CurScene.Deactivate();
            newScene.Activate(CurScene);
            CurScene = newScene;
        }

        protected override void Update(float dt)
        {
            CurScene.Update(dt, MousePos, Game, UI);
            //var area = CurScene.GetCurArea();
            //if (area != null) area.Update(dt, MousePos, Game, UI);
        }
        protected override void DrawToScreenTexture(ScreenTexture screenTexture)
        {
            var area = CurScene.GetCurArea();


            if (screenTexture == Game)
            {
                Vector2 size = screenTexture.GetSize();
                Vector2 mousePos = screenTexture.MousePos;
                uint id = screenTexture.ID;

                CurScene.DrawGame(size, mousePos);
                //if (area != null) area.DrawGame(size, mousePos);
            }

            else if (screenTexture == UI)
            {
                Vector2 size = screenTexture.GetSize();
                Vector2 mousePos = screenTexture.MousePos;
                uint id = screenTexture.ID;

                CurScene.DrawUI(size, mousePos);
                //if (area != null) area.DrawUI(size, mousePos);
            }
            else
            {
                CurScene.DrawToTexture(screenTexture);
                //if (area != null) area.DrawToTexture(screenTexture);

            }
        }
        protected override void DrawToScreen(Vector2 size, Vector2 mousePos)
        {
            CurScene.DrawToScreen(size, mousePos);
            //var area = CurScene.GetCurArea();
            //if (area != null) area.DrawToScreen(size, mousePos);
        }

    }

}