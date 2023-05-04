
using ShapeScreen;
using ShapeTiming;
using Raylib_CsLo;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ShapeCore
{
    internal class DelayedAction : ISequenceable
    {
        private Action action;
        private float timer;

        public DelayedAction(float delay, Action action)
        {
            if(delay <= 0)
            {
                this.timer = 0f;
                this.action = action;
                this.action();
            }
            else
            {
                this.timer = delay;
                this.action = action;
            }
        }
        public bool Update(float dt)
        {
            if (timer <= 0f) return true;
            else
            {
                timer -= dt;
                if(timer <= 0f)
                {
                    this.action.Invoke();
                    return true;
                }
            }
            return false;
        }
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
    public abstract class GameLoop
    {
        //private int CUR_SCENE_INDEX = 0;
        //private Dictionary<string, Scene> SCENES = new();
        //private DelegateTimerHandlerNamed delayHandler = new();
        
        public static readonly string CURRENT_DIRECTORY = Environment.CurrentDirectory;
        public static bool EDITORMODE { get; private set; } = Directory.Exists("resources");
        
        public static OSPlatform OS_PLATFORM { get; private set; } =
           RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?    OSPlatform.Windows :
           RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?      OSPlatform.Linux :
           RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ?        OSPlatform.OSX :
                                                                    OSPlatform.FreeBSD;
        public static bool IsWindows() { return OS_PLATFORM == OSPlatform.Windows; }
        public static bool IsLinux() { return OS_PLATFORM == OSPlatform.Linux; }
        public static bool IsOSX() { return OS_PLATFORM == OSPlatform.OSX; }

        public string[] LAUNCH_PARAMS { get; private set; }
        
        public bool QUIT { get; protected set; } = false;
        public bool RESTART { get; protected set; } = false;
        
        public bool CALL_HANDLE_INPUT = true;
        public bool CALL_UPDATE = true;
        public bool CALL_DRAW = true;
        public bool CALL_DRAWUI = true;

        public Color BackgroundColor = BLACK;
        public IScene CUR_SCENE { get; private set; } = new SceneEmpty();
        
        private Sequencer<DelayedAction> delayHandler = new();
        private BasicTimer stopTimer = new();
        private List<DeferredInfo> deferred = new();





        public void GoToScene(IScene newScene)
        {
            if (newScene == null) return;
            if (newScene == CUR_SCENE) return;
            CUR_SCENE.Deactivate();
            newScene.Activate(CUR_SCENE);
            CUR_SCENE = newScene;
        }
        public void Restart()
        {
            RESTART = true;
            QUIT = true;
        }
        public void Quit()
        {
            RESTART = false;
            QUIT = true;
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


        /// <summary>
        /// Stop the current scene for the duration. Only affects Update().
        /// </summary>
        /// <param name="duration"></param>
        public void Stop(float duration, float delay = 0f) 
        {
            if (delay > 0)
            {
                var a = new DelayedAction(delay, () => { stopTimer.Start(duration); });
                delayHandler.StartSequence(a);
            }
            else
            {
                if(delayHandler.HasSequences()) delayHandler.Stop();
                stopTimer.Start(duration);
            }
        }
        public void CancelStop() 
        {
            //if (delayHandler.Has("stop")) delayHandler.Remove("stop");
            stopTimer.Stop(); 
        }

        
       

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
        



        public void Initialize(int devWidth, int devHeight, float gameSizeFactor, float uiSizeFactor, string windowName, bool fixedTexture, bool pixelSmoothing, bool hideCursor, params string[] launchParams)
        {
            LAUNCH_PARAMS = launchParams;
            QUIT = false;
            RESTART = false;

            Raylib.SetExitKey(-1);

            ScreenHandler.Initialize(devWidth, devHeight, gameSizeFactor, uiSizeFactor, windowName, fixedTexture, pixelSmoothing, hideCursor);
            InitAudioDevice();
            //AudioHandler.Initialize();
            //InputHandler.Initialize();
            
            
        }

        
        
        /// <summary>
        /// Runs the game until Quit() or Restart() is called or the Window is closed by the user.
        /// </summary>
        /// <returns>Returns if the application should restart or not.</returns>
        public bool Run()
        {
            Start();
            while (!QUIT)
            {
                //UPDATE
                UpdateGame(GetFrameTime());

                // DRAW TO MAIN TEXTURE
                DrawGame();

                ResolveDeferred();
            }
            
            End();
            Close();
            return RESTART;
        }
        private void UpdateGame(float dt)
        {
            if (CALL_HANDLE_INPUT) PreHandleInput();
            ScreenHandler.Update(dt);

            if (CALL_UPDATE)
            {
                PreUpdate(dt);
                
                if (CUR_SCENE.CallHandleInput) CUR_SCENE.HandleInput();
                //delayHandler.Update(dt);
                //stopTimer.Update(dt);
                if (CUR_SCENE.CallUpdate && !stopTimer.IsRunning) CUR_SCENE.Update(dt);
                
                PostUpdate(dt);
            }
            
            if (CALL_HANDLE_INPUT) PostHandleInput();

            /*
            if (CALL_UPDATE) PreUpdate(dt);

            
            
            if (CUR_SCENE != null)
            {
                if(!CUR_SCENE.IsInputDisabled()) CUR_SCENE.HandleInput();

                if (!CUR_SCENE.IsPaused())
                {
                    delayHandler.Update(dt);
                    stopTimer.Update(dt);
                    
                    if (!stopTimer.IsRunning())
                    {
                        CUR_SCENE.Update(dt);
                    }
                }
            }
            if (CALL_UPDATE) PostUpdate(dt);
            */
        }
        private void DrawGame()
        {
            //Draw to game texture
            if (CALL_DRAW)
            {
                ScreenHandler.StartDraw(true);
                PreDraw();
                if (CUR_SCENE.CallDraw) CUR_SCENE.Draw();
                PostDraw();
                ScreenHandler.EndDraw(true);
            }

            //Draw to ui texture
            if (CALL_DRAWUI)
            {
                //Draw to UI texture
                Vector2 uiSize = ScreenHandler.UISize();
                ScreenHandler.StartDraw(false);
                PreDrawUI(uiSize);
                if (CUR_SCENE.CallDraw) CUR_SCENE.DrawUI(uiSize);
                PostDrawUI(uiSize);
                ScreenHandler.EndDraw(false);
            }
            
            //Draw textures to screen
            BeginDrawing();
            ClearBackground(BackgroundColor);

            if (ScreenHandler.CAMERA != null && ScreenHandler.CAMERA.PIXEL_SMOOTHING_ENABLED)
            {
                BeginMode2D(ScreenHandler.CAMERA.ScreenSpaceCam);
                ScreenHandler.Draw();
                EndMode2D();
            }
            else ScreenHandler.Draw();

            ScreenHandler.DrawUI();
            EndDrawing();
        }
        private void Close()
        {
            ScreenHandler.Close();
            //bool fullscreen = IsWindowFullscreen();
            //if (RESTART && fullscreen) ScreenHandler.ToggleFullscreen();

            //return fullscreen;
        }


        public virtual void Start() { }
        public virtual void PreHandleInput() { }
        public virtual void PostHandleInput() { }
        public virtual void PreUpdate(float dt) { }
        public virtual void PostUpdate(float dt) { }
        public virtual void PreDraw() { }
        public virtual void PostDraw() { }
        public virtual void PreDrawUI(Vector2 uiSize) { }
        public virtual void PostDrawUI(Vector2 uiSize) { }
        public virtual void End() { }


        /*
        public Vector2 GameTextureSize { get { return ScreenHandler.GameSize(); } }
        public Vector2 UITextureSize { get { return ScreenHandler.UISize(); } }
        public Vector2 GameTextureCenter { get { return ScreenHandler.GameCenter(); } }
        public Vector2 UITextureCenter { get { return ScreenHandler.UICenter(); } }
        public Rectangle GameTextureRect { get { return ScreenHandler.GameArea(); } }
        public Rectangle UITextureRect { get { return ScreenHandler.UIArea(); } }
        */
        /*
       public void SwitchScene(Scene oldScene, Scene newScene, string key = "")
       {
           if (newScene == null) return;
           AddScene(key, newScene);//add scene takes care if scene is already in the dictionary or key == ""
           CUR_SCENE = newScene;
           CUR_SCENE.Activate(oldScene);
           CUR_SCENE_INDEX = SCENES.Values.ToList().IndexOf(newScene);
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
       */
        //public virtual void HandleInput() { }//called before update to handle global input
        //public void Pause() { PAUSED = true; }
        //public void UnPause() { PAUSED = false; }
        //public void TogglePause() { PAUSED = !PAUSED; }

    }
}
