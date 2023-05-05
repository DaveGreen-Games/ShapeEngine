
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


    public struct ExitCode
    {
        public bool restart = false;
        public ExitCode(bool restart) { this.restart = restart; }

    }
    public class GameLoop
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
        
        public bool CALL_HANDLE_INPUT = true;
        public bool CALL_UPDATE = true;
        public bool CALL_DRAW = true;
        public bool CALL_DRAWUI = true;

        public Color BackgroundColor = BLACK;
        public IScene CUR_SCENE { get; private set; } = new SceneEmpty();
        
        private Sequencer<DelayedAction> delayHandler = new();
        private BasicTimer stopTimer = new();
        private List<DeferredInfo> deferred = new();
        private bool quit = false;
        private bool restart = false;

        public GraphicsDevice GFX { get; protected set; }



        public void GoToScene(IScene newScene)
        {
            if (newScene == null) return;
            if (newScene == CUR_SCENE) return;
            CUR_SCENE.Deactivate();
            newScene.Activate(CUR_SCENE);
            CUR_SCENE = newScene;
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

        
       
        /*
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
        */

        public GameLoop(GraphicsDevice graphicsDevice, params string[] launchParameters)
        {
            this.GFX = graphicsDevice;
            this.LAUNCH_PARAMS = launchParameters;
            
            quit = false;
            restart = false;
            Raylib.SetExitKey(-1);
        }


        /// <summary>
        /// Starts the gameloop. Runs until Quit() or Restart() is called or the Window is closed by the user.
        /// </summary>
        /// <returns>Returns an exit code for information how the application was quit. Restart has to be handled seperately.</returns>
        public ExitCode Run()
        {
            Start();
            RunGameloop();
            End();
            
            GFX.Close();
            
            return new ExitCode(restart);
        }
        private void RunGameloop()
        {
            while (!quit)
            {
                HandleInput();

                UpdateGame(GetFrameTime());

                DrawGame();

                ResolveDeferred();
            }
        }
        private void HandleInput()
        {
            if(!CALL_HANDLE_INPUT) return;

            if (BeginHandleInput()) CUR_SCENE.HandleInput();
            EndHandleInput();
        }
        private void UpdateGame(float dt)
        {
            if (!CALL_UPDATE) return;

            GFX.Update(dt);

            if (BeginUpdate(dt))
            {
                if (CUR_SCENE.CallUpdate && !stopTimer.IsRunning) CUR_SCENE.Update(dt);
            }
            EndUpdate(dt);
        }
        private void DrawGame()
        {
            //Draw to game texture
            if (CALL_DRAW)
            {
                GFX.BeginDraw();
                if (BeginDraw()) 
                { 
                    if (CUR_SCENE.CallDraw) CUR_SCENE.Draw(); 
                }
                EndDraw();
                GFX.EndDraw();
            }

            //Draw to ui texture
            if (CALL_DRAWUI)
            {
                Vector2 uiSize = GFX.UISize();// GetUITextureSize();// ScreenHandler.UISize();
                GFX.BeginDrawUI();
                if (BeginDrawUI(uiSize))
                {
                    if (CUR_SCENE.CallDraw) CUR_SCENE.DrawUI(uiSize);
                }
                EndDrawUI(uiSize);
                GFX.EndDrawUI();
            }
            
            //Draw textures to screen
            BeginDrawing();
            ClearBackground(BackgroundColor);

            if (GFX.CAMERA.IsPixelSmoothingCameraEnabled())
            {
                BeginMode2D(GFX.CAMERA.GetPixelSmoothingCamera());
                GFX.DrawGameToScreen();
                EndMode2D();
            }
            else GFX.DrawGameToScreen();

            GFX.DrawUIToScreen();
            EndDrawing();
        }
        private void Start() 
        {
            GFX.Start();
            LoadContent();
            BeginRun();
        }
        private void End() 
        {
            EndRun();
            UnloadContent();
        }

        //public virtual void HandleLaunchParameters() { }
        /// <summary>
        /// Called first after starting the gameloop.
        /// </summary>
        public virtual void LoadContent() { }
        /// <summary>
        /// Called after LoadContent but before the main loop has started.
        /// </summary>
        public virtual void BeginRun() { }
        /// <summary>
        /// Called within the main gameloop before HandleInput is called on the cur scene.
        /// </summary>
        /// <returns>Return if HandleInput should be called on the cur scene.</returns>
        public virtual bool BeginHandleInput() { return true; }
        /// <summary>
        /// Called within the main gameloop after HandleInput was called on the cur scene.
        /// </summary>
        public virtual void EndHandleInput() { }
        /// <summary>
        /// Called within the main gameloop before Update is called on the cur scene.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>Return if Update should be called on the cur scene.</returns>
        public virtual bool BeginUpdate(float dt) { return true; }
        /// <summary>
        /// Called within the main gameloop after Update was called on the cur scene.
        /// </summary>
        /// <param name="dt"></param>
        public virtual void EndUpdate(float dt) { }
        /// <summary>
        /// Called within the main gameloop before Draw is called on the cur scene.
        /// </summary>
        /// <returns>Return if Draw should be called on the cur scene.</returns>
        public virtual bool BeginDraw() { return true; }
        /// <summary>
        /// Called within the main gameloop after Draw was called on the cur scene.
        /// </summary>
        public virtual void EndDraw() { }
        /// <summary>
        /// Called within the main gameloop before DrawUI is called on the cur scene.
        /// </summary>
        /// <param name="uiSize"></param>
        /// <returns>Return if DrawUI should be called on the cur scene</returns>
        public virtual bool BeginDrawUI(Vector2 uiSize) { return true; }
        /// <summary>
        /// Called within the main gameloop after DrawUI was called on the cur scene.
        /// </summary>
        /// <param name="uiSize"></param>
        public virtual void EndDrawUI(Vector2 uiSize) { }
        /// <summary>
        /// Called before UnloadContent is called after the main gameloop has been exited.
        /// </summary>
        public virtual void EndRun() { }
        /// <summary>
        /// Called after EndRun before the application terminates.
        /// </summary>
        public virtual void UnloadContent() { }








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
