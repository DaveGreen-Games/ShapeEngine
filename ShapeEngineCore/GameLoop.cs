
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Screen;
using ShapeEngineCore.Globals.Audio;
using ShapeEngineCore.Globals.Input;
using ShapeEngineCore.Globals.UI;
using ShapeEngineCore.Globals.Timing;
using ShapeEngineCore.Globals.Shaders;
using ShapeEngineCore.Globals.Persistent;
using ShapeEngineCore.Globals.Cursor;
using Raylib_CsLo;
using System.Numerics;


namespace ShapeEngineCore
{
    public struct ScreenInitInfo
    {
        public int devWidth = 1920;
        public int devHeight = 1080;
        public float gameSizeFactor = 1.0f;
        public float uiSizeFactor = 1.0f;
        public string windowName = "Shape Engine Game";
        public int fps = 60;
        public bool vsync = true;
        public bool fullscreen = false;
        public int monitor = 0;
        public bool stretch = false;

        public ScreenInitInfo() { }
        public ScreenInitInfo(int devWidth, int devHeight, string windowName) { this.devWidth = devWidth; this.devHeight = devHeight; this.windowName = windowName; }
        public ScreenInitInfo(int devWidth, int devHeight, float gameSizeFactor, float uiSizeFactor, string windowName, int fps, bool vsync, bool fullscreen, int monitor, bool stretch)
        {
            this.devWidth = devWidth;
            this.devHeight = devHeight;
            this.gameSizeFactor = gameSizeFactor;
            this.uiSizeFactor = uiSizeFactor;
            this.windowName = windowName;
            this.fps = fps;
            this.vsync = vsync;
            this.fullscreen = fullscreen;
            this.monitor = monitor;
            this.stretch = stretch;
        }
    }
    public struct DataInitInfo
    {
        public string resourceFolderPath = "";
        public string dataFileName = "";
        public DataResolver dataResolver = new DataResolver();
        public string[] sheetNames = new string[0];

        public DataInitInfo() { }
        public DataInitInfo(string resourceFolderPath, string dataFileName, DataResolver dataResolver, params string[] sheetNames) { this.resourceFolderPath = resourceFolderPath; this.dataFileName = dataFileName; this.dataResolver = dataResolver; this.sheetNames = sheetNames; }
    }

    internal class DeferredInfo
    {
        private Action action;
        private int frames = 0;
        public DeferredInfo(Action action, int frames)
        {
            this.action = action;
            this.frames = frames;
        }

        public bool Call()
        {
            if(frames <= 0)
            {
                action.Invoke();
                return true;
            }
            else
            {
                frames--;
                return false;
            }
        }

    }
    public class GameLoop
    {
        public delegate void Triggered(string trigger, params float[] values);
        public string[] LAUNCH_PARAMS { get; private set; }
        public bool QUIT = false;
        public bool RESTART = false;

        /// <summary>
        /// The delta time of the game. When the game runs at 60fps, DELTA would be 1/60 = 0.016
        /// </summary>
        public float DELTA { get; private set; }
        /// <summary>
        /// DELTA affected by the current slow amount. Equals DELTA * CUR_SLOW_FACTOR
        /// </summary>
        public float GAME_DELTA { get; private set; }
        public Vector2 MOUSE_POS { get; private set; }
        public Vector2 MOUSE_POS_GAME { get; private set; }
        public Vector2 MOUSE_POS_UI { get; private set; }
        public Color backgroundColor = BLACK;
        public Scene? CUR_SCENE { get; private set; }
        private int CUR_SCENE_INDEX = 0;


        private DelegateTimerHandlerNamed delayHandler = new();
        private Dictionary<string, Scene> SCENES = new();
        private BasicTimer stopTimer = new();
        
        //public bool PAUSED { get; private set; } = false;
        
        /// <summary>
        /// By how much the current scene is slowed down.
        /// </summary>
        public float CUR_SLOW_FACTOR { get; private set; } = 1f;
        private BasicTimer slowTimer = new();
        //private int slowCounter = 0;
        //private int slowCount = 0;
        private List<DeferredInfo> deferred = new();

        /// <summary>
        /// Slow down the current scene by the factor. 0.5f means scene runs 2 times slower; 2.0f means scene runs 2 times faster. If factor is <= to 0 Stop(duration) is called instead.
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="duration"></param>
        public void Slow(float factor, float duration, float delay = 0f)
        {
            if (duration <= 0) return;
            if (factor <= 0f) Stop(duration, delay);
            else
            {
                if(delay > 0)
                {
                    delayHandler.Add("slow", delay, () => { slowTimer.Start(duration); CUR_SLOW_FACTOR = factor; }, 0);
                }
                else
                {
                    if (delayHandler.Has("slow")) delayHandler.Remove("slow");
                    slowTimer.Start(duration);
                    //slowCounter = 0;
                    //slowCount = (int)(1f / factor);
                    CUR_SLOW_FACTOR = factor;
                }
            }
        }
        public void EndSlow()
        {
            if (delayHandler.Has("slow")) delayHandler.Remove("slow");
            slowTimer.Stop();
            CUR_SLOW_FACTOR = 1f;
            //slowCount = 0;
            //slowCounter = 0;
        }

        /// <summary>
        /// Stop the current scene for the duration. Only affects Update().
        /// </summary>
        /// <param name="duration"></param>
        public void Stop(float duration, float delay = 0f) 
        {
            if (delay > 0)
            {
                delayHandler.Add("stop", delay, () => { stopTimer.Start(duration); }, 0);
            }
            else
            {
                if (delayHandler.Has("stop")) delayHandler.Remove("stop");
                stopTimer.Start(duration);
            }
            
        }
        public void CancelStop() 
        {
            if (delayHandler.Has("stop")) delayHandler.Remove("stop");
            stopTimer.Stop(); 
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
        //public void Pause() { PAUSED = true; }
        //public void UnPause() { PAUSED = false; }
        //public void TogglePause() { PAUSED = !PAUSED; }

        public Area? GetCurArea()
        {
            if (CUR_SCENE == null) return null;
            return CUR_SCENE.GetCurArea();
        }

        public void AddGameObject(GameObject obj, bool uiDrawing = false)
        {
            Area? area = GetCurArea();
            if (area == null) return;
            area.AddGameObject(obj, uiDrawing);
        }
        public Vector2 GameSize() { return ScreenHandler.GameSize(); }
        public Vector2 UISize() { return ScreenHandler.UISize(); }
        public Vector2 GameCenter() { return ScreenHandler.GameCenter(); }
        public Vector2 UICenter() { return ScreenHandler.UICenter(); }
        public Rectangle GameArea() { return ScreenHandler.GameArea(); }
        public Rectangle UIArea() { return ScreenHandler.UIArea(); }

        
        public void SwitchScene(Scene oldScene, Scene newScene, string key = "")
        {
            if (newScene == null) return;
            AddScene(key, newScene);//add scene takes care if scene is already in the dictionary or key == ""
            CUR_SCENE = newScene;
            CUR_SCENE.Activate(oldScene);
            CUR_SCENE_INDEX = SCENES.Values.ToList().IndexOf(newScene);
        }
        public void GoToScene(Scene newScene)
        {
            if (newScene == null) return;
            if (CUR_SCENE == null)
            {
                CUR_SCENE = newScene;
                CUR_SCENE.Activate(null);
            }
            else
            {
                CUR_SCENE.Deactivate(newScene);
            }
        }
        public void GoToScene(string key)
        {
            if (key == "") return;
            if (!SCENES.ContainsKey(key)) return;
            GoToScene(SCENES[key]);
        }
        public void GoToScene(int index)
        {
            if (index < 0 || index >= SCENES.Count) return;
            GoToScene(SCENES.ElementAt(index).Value);
        }
        public void AddScene(string key, Scene scene)
        {
            if (key == "") return;
            if (scene == null) return;
            if (SCENES.ContainsKey(key))
            {
                SCENES[key] = scene;
            }
            else
            {
                SCENES.Add(key, scene);
            }
            scene.Start();
        }
        public void RemoveScene(string key, int fallbackIndex = 0)
        {
            if (key == "") return;
            if (!SCENES.ContainsKey(key)) return;
            Scene? scene = SCENES[key];
            SCENES.Remove(key);
            if (scene == null) return;

            if (scene == CUR_SCENE)
            {
                if (SCENES.Count > 0 && fallbackIndex >= 0 && fallbackIndex < SCENES.Count)
                {
                    var newScene = SCENES.ElementAt(fallbackIndex).Value;
                    if (newScene == null)
                    {
                        CUR_SCENE = null;
                        return;
                    }
                    CUR_SCENE = newScene;
                    newScene.Activate(scene);
                }
                else
                {
                    CUR_SCENE = null;
                }
            }
            scene.Close();
        }
        public Scene NextScene()
        {
            CUR_SCENE_INDEX += 1;
            if (CUR_SCENE_INDEX >= SCENES.Count) CUR_SCENE_INDEX = 0;
            var scene = SCENES.ElementAt(CUR_SCENE_INDEX).Value;
            GoToScene(scene);
            return scene;
        }
        public Scene PreviousScene()
        {
            CUR_SCENE_INDEX -= 1;
            if (CUR_SCENE_INDEX < 0) CUR_SCENE_INDEX = SCENES.Count - 1;
            var scene = SCENES.ElementAt(CUR_SCENE_INDEX).Value;
            GoToScene(scene);
            return scene;
        }
        public void ClearScenes()
        {
            CUR_SCENE = null;
            CUR_SCENE_INDEX = 0;
            foreach (Scene scene in SCENES.Values)
            {
                scene.Close();
            }
            SCENES.Clear();
        }
        public void Restart()
        {
            RESTART = true;
            QUIT = true;
        }


        public void Initialize(string resourceFolderPath, ScreenInitInfo screenInitInfo, params string[] launchParams)
        {
            LAUNCH_PARAMS = launchParams;
            QUIT = false;
            RESTART = false;

            //Load savegames here to get stored values for fullscreen, monitor etc.

            //needs to be called first!!!
            bool fs = launchParams.Contains("fullscreen") || screenInitInfo.fullscreen;
            //ScreenHandler.Initialize(1920, 1080, 0.25f, 2.0f, "Raylib Template", 60, true, fs, 0, false);
            ScreenHandler.Initialize(screenInitInfo.devWidth, screenInitInfo.devHeight, screenInitInfo.gameSizeFactor, screenInitInfo.uiSizeFactor, screenInitInfo.windowName, screenInitInfo.fps, screenInitInfo.vsync, fs, screenInitInfo.monitor, screenInitInfo.stretch);

            ResourceManager.Initialize(resourceFolderPath);
            PaletteHandler.Initialize();
            UIHandler.Initialize();
            AudioHandler.Initialize();
            ShaderHandler.Initialize();
            InputHandler.Initialize();
            CursorHandler.Initialize();
        }

        public bool Close()
        {
            //it is good to toggle fullscreen when restarting

            bool fullscreen = IsWindowFullscreen();
            if (RESTART && fullscreen) ScreenHandler.ToggleFullscreen();

            ClearScenes();
            InputHandler.Close();
            DataHandler.Close();
            ShaderHandler.Close();
            AudioHandler.Close();
            UIHandler.Close();
            TimerHandler.Close();
            StepHandler.Close();
            CursorHandler.Close();
            ScreenHandler.Close();

            return fullscreen;
        }
        public void Run()
        {
            while (!QUIT)
            {
                DELTA = GetFrameTime();
                //MOUSE_POS_GAME = ScreenHandler.ScalePositionV(Raylib.GetMousePosition(), "game");
                MOUSE_POS = GetMousePosition();
                MOUSE_POS_UI = ScreenHandler.ScalePositionV(MOUSE_POS, false);
                MOUSE_POS_GAME = ScreenHandler.TransformPositionToGame(MOUSE_POS_UI);
                if (WindowShouldClose() && !IsKeyDown(KeyboardKey.KEY_ESCAPE)) QUIT = true;

                delayHandler.Update(DELTA);
                stopTimer.Update(DELTA);
                if (!stopTimer.IsRunning())
                {
                    slowTimer.Update(DELTA);
                    if (CUR_SLOW_FACTOR != 1f && !slowTimer.IsRunning())
                    {
                        CUR_SLOW_FACTOR = 1f;
                        //slowCount = 0;
                        //slowCounter = 0;
                    }
                }
                GAME_DELTA = DELTA * CUR_SLOW_FACTOR;

                PreUpdate(DELTA);
                
                //Input
                HandleInput();
                if (CUR_SCENE != null) CUR_SCENE.HandleInput(DELTA);
                
                //UPDATE
                Update(DELTA);
                
                // DRAW TO MAIN TEXTURE
                Draw();

                ScreenHandler.EndUpdate(DELTA);

                ResolveDeferred();
            }
        }
        protected void Update(float dt)
        {
            InputHandler.Update(dt);
            Ease.Update(dt);
            UIHandler.Update(dt);
            TimerHandler.Update(dt);
            StepHandler.Update(dt);
            AudioHandler.Update(dt);
            ScreenHandler.Update(dt);
            //if (!stopTimer.IsRunning()) ScreenHandler.UpdateCamera(dt * CUR_SLOW_FACTOR);

            if (ScreenHandler.HasMonitorChanged())
            {
                foreach (var scene in SCENES.Values)
                {
                    scene.MonitorHasChanged();
                }
            }

            if (CUR_SCENE != null)
            {
                if (!stopTimer.IsRunning())
                {
                    CUR_SCENE.Update(dt * CUR_SLOW_FACTOR);
                    //if(slowCount <= 0) CUR_SCENE.Update(dt);
                    //else
                    //{
                    //    if (slowCounter >= slowCount)
                    //    {
                    //        slowCounter = 0;
                    //        CUR_SCENE.Update(dt);
                    //    }
                    //    else slowCounter++;
                    //}
                }
            }
        }
        protected void Draw()
        {
            //Draw to game texture
            if (CUR_SCENE != null)
            {
                ScreenHandler.StartDraw(true);
                CUR_SCENE.Draw();
                ScreenHandler.EndDraw(true);
            }


            //Draw to UI texture
            ScreenHandler.StartDraw(false);
            if (CUR_SCENE != null) CUR_SCENE.DrawUI();
            CursorHandler.Draw(MOUSE_POS_UI);
            ScreenHandler.EndDraw(false);

            
            //Draw textures to screen
            BeginDrawing();
            ClearBackground(backgroundColor);
            PreDraw();

            if (ScreenHandler.Cam != null && ScreenHandler.Cam.PIXEL_SMOOTHING_ENABLED)
            {
                BeginMode2D(ScreenHandler.Cam.ScreenSpaceCam);
                ShaderHandler.DrawShaders();
                EndMode2D();
            }
            else ShaderHandler.DrawShaders();
            
            ScreenHandler.UI.Draw();
            PostDraw();
            EndDrawing();
        }

        //public virtual void PreInit() { } //called before initialization -> use for setting specific vars
        public virtual void Start() { } //called after initialization
        public virtual void PreUpdate(float dt) { } //always called before update
        public virtual void HandleInput() { }//called before update to handle global input
        public virtual void PreDraw() { }//called before draw
        public virtual void PostDraw() { }//called after draw
        public virtual void End() { } //called before game closes

    }
}
