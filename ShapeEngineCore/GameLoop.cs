
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

    public class GameLoop
    {
        public delegate void Triggered(string trigger, params float[] values);
        public string[] LAUNCH_PARAMS { get; private set; }
        public bool QUIT = false;
        public bool RESTART = false;
        public float DELTA { get; private set; }


        public Vector2 MOUSE_POS_GAME { get; private set; }
        public Vector2 MOUSE_POS_UI { get; private set; }
        public Color backgroundColor = BLACK;
        public Scene? CUR_SCENE { get; private set; }
        private int CUR_SCENE_INDEX = 0;
        private Dictionary<string, Scene> SCENES = new();
        private BasicTimer stopTimer = new();




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

        public void Stop(float duration)
        {
            stopTimer.Start(duration);
        }
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


        public void Initialize(ScreenInitInfo screenInitInfo, DataInitInfo dataInitInfo, params string[] launchParams)
        {
            LAUNCH_PARAMS = launchParams;
            QUIT = false;
            RESTART = false;

            //Load savegames here to get stored values for fullscreen, monitor etc.

            //needs to be called first!!!
            bool fs = launchParams.Contains("fullscreen") || screenInitInfo.fullscreen;
            //ScreenHandler.Initialize(1920, 1080, 0.25f, 2.0f, "Raylib Template", 60, true, fs, 0, false);
            ScreenHandler.Initialize(screenInitInfo.devWidth, screenInitInfo.devHeight, screenInitInfo.gameSizeFactor, screenInitInfo.uiSizeFactor, screenInitInfo.windowName, screenInitInfo.fps, screenInitInfo.vsync, fs, screenInitInfo.monitor, screenInitInfo.stretch);

            //DataHandler.Initialize("data/test-properties.json", new DataResolver());
            ResourceManager.Initialize(dataInitInfo.resourceFolderPath);
            DataHandler.Initialize(dataInitInfo.dataFileName, dataInitInfo.dataResolver, dataInitInfo.sheetNames);
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
                MOUSE_POS_UI = ScreenHandler.ScalePositionV(GetMousePosition(), false);
                MOUSE_POS_GAME = ScreenHandler.TransformPositionToGame(MOUSE_POS_UI);
                if (WindowShouldClose() && !IsKeyDown(KeyboardKey.KEY_ESCAPE)) QUIT = true;

                stopTimer.Update(DELTA);

                if (!stopTimer.IsRunning())
                {
                    PreUpdate(DELTA);

                    //Input
                    HandleInput();

                    //UPDATE
                    Update(DELTA);

                }
                // DRAW TO MAIN TEXTURE
                Draw();

                ScreenHandler.EndUpdate(DELTA);
            }
        }
        protected void Update(float dt)
        {
            InputHandler.Update(dt);
            //UI.UpdateMousePos(MOUSE_POS_UI);
            Ease.Update(dt);
            UIHandler.Update(dt);
            TimerHandler.Update(dt);
            StepHandler.Update(dt);
            AudioHandler.Update(dt);
            ScreenHandler.Update(dt);

            if (ScreenHandler.HasMonitorChanged())
            {
                foreach (var scene in SCENES.Values)
                {
                    scene.MonitorHasChanged();
                }
            }

            if (CUR_SCENE != null) CUR_SCENE.Update(dt);
        }
        protected void Draw()
        {
            //Draw to game texture
            ScreenHandler.StartDraw(true);
            if (CUR_SCENE != null) CUR_SCENE.Draw();
            ScreenHandler.EndDraw(true);


            //Draw to UI texture
            ScreenHandler.StartDraw(false);
            //DrawFPS(20, 20);
            if (CUR_SCENE != null) CUR_SCENE.DrawUI();
            CursorHandler.Draw(MOUSE_POS_UI);
            ScreenHandler.EndDraw(false);


            //Draw textures to screen
            BeginDrawing();
            ClearBackground(backgroundColor);
            PreDraw();
            ShaderHandler.DrawShaders();
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
