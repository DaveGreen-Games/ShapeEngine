﻿
using ShapeLib;
using ShapeScreen;
using ShapeAudio;
using ShapeInput;
using ShapeUI;
using ShapeTiming;
using ShapeShaders;
using ShapePersistent;
using ShapeCursor;
using Raylib_CsLo;
using System.Numerics;
using ShapeEase;

namespace ShapeCore
{
    
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
        //public static DelegateTimerHandler TIMER = new();
        //public static AlternatorContainer ALTERNATOR = new();
        //public static EaseHandler EASE = new();
        //public static StepHandler STEP = new();
        //public static FontHandler FONT = new();


        public delegate void Triggered(string trigger, params float[] values);
        public string[] LAUNCH_PARAMS { get; private set; }
        public bool QUIT = false;
        public bool RESTART = false;
        //public bool CALL_HANDLE_INPUT = true;
        public bool CALL_GAMELOOP_HANDLE_INPUT = true;
        public bool CALL_GAMELOOP_UPDATE = true;
        public bool CALL_GAMELOOP_DRAW = true;
        public bool CALL_GAMELOOP_DRAWUI = true;
        /// <summary>
        /// The delta time of the game. When the game runs at 60fps, DELTA would be 1/60 = 0.016
        /// </summary>
        public float DELTA { get; private set; }
        /// <summary>
        /// DELTA affected by the current slow amount. Equals DELTA * CUR_SLOW_FACTOR
        /// </summary>
        //public float GAME_DELTA { get; private set; }
        public Vector2 MOUSE_POS { get; private set; } = new(0f);
        public Vector2 MOUSE_POS_GAME { get; private set; } = new(0f);
        public Vector2 MOUSE_POS_UI { get; private set; } = new(0f);
        //public Vector2 MOUSE_POS_UI_RAW { get; private set; }
        public Color backgroundColor = BLACK;
        public Scene? CUR_SCENE { get; private set; }
        private int CUR_SCENE_INDEX = 0;
        private Dictionary<string, Scene> SCENES = new();


        private DelegateTimerHandlerNamed delayHandler = new();
        private BasicTimer stopTimer = new();
        private List<DeferredInfo> deferred = new();
        
        
        ///// <summary>
        ///// By how much the current scene is slowed down.
        ///// </summary>
        //public float CUR_SLOW_FACTOR { get; private set; } = 1f;
        //private BasicTimer slowTimer = new();
        //
        ///// <summary>
        ///// Slow down the current scene by the factor. 0.5f means scene runs 2 times slower; 2.0f means scene runs 2 times faster. If factor is <= to 0 Stop(duration) is called instead.
        ///// </summary>
        ///// <param name="factor"></param>
        ///// <param name="duration"></param>
        //public void Slow(float factor, float duration, float delay = 0f)
        //{
        //    if (duration <= 0) return;
        //    if (factor <= 0f) Stop(duration, delay);
        //    else
        //    {
        //        if(delay > 0)
        //        {
        //            delayHandler.Add("slow", delay, () => { slowTimer.Start(duration); CUR_SLOW_FACTOR = factor; }, 0);
        //        }
        //        else
        //        {
        //            if (delayHandler.Has("slow")) delayHandler.Remove("slow");
        //            slowTimer.Start(duration);
        //            //slowCounter = 0;
        //            //slowCount = (int)(1f / factor);
        //            CUR_SLOW_FACTOR = factor;
        //        }
        //    }
        //}
        //public void EndSlow()
        //{
        //    if (delayHandler.Has("slow")) delayHandler.Remove("slow");
        //    slowTimer.Stop();
        //    CUR_SLOW_FACTOR = 1f;
        //    //slowCount = 0;
        //    //slowCounter = 0;
        //}

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


        public void Initialize(int devWidth, int devHeight, float gameSizeFactor, float uiSizeFactor, string windowName, bool fixedTexture, bool pixelSmoothing, bool hideCursor, params string[] launchParams)
        {
            LAUNCH_PARAMS = launchParams;
            QUIT = false;
            RESTART = false;

            Raylib.SetExitKey(-1);

            ScreenHandler.Initialize(devWidth, devHeight, gameSizeFactor, uiSizeFactor, windowName, fixedTexture, pixelSmoothing, hideCursor);
            AudioHandler.Initialize();
            InputHandler.Initialize();
            
            
        }

        public bool Close()
        {
            //it is good to toggle fullscreen when restarting

            bool fullscreen = IsWindowFullscreen();
            if (RESTART && fullscreen) ScreenHandler.ToggleFullscreen();

            ClearScenes();
            InputHandler.Close();
            AudioHandler.Close();
            UIHandler.Close();
            ScreenHandler.Close();

            //STEP.Close();
            //TIMER.Close();
            //ALTERNATOR.Close();
            //EASE.Close();


            return fullscreen;
        }
        public void Run()
        {
            while (!QUIT)
            {
                DELTA = GetFrameTime();

                Vector2 curMousePos = GetMousePosition();
                if(!SVec.IsNan(curMousePos))
                {
                    MOUSE_POS = curMousePos;

                    //implement mouse pos raw
                    Vector2 curMousePosUI = ScreenHandler.UI.ScalePositionV(MOUSE_POS);
                    if (!SVec.IsNan(curMousePosUI)) MOUSE_POS_UI = curMousePosUI;
                    //MOUSE_POS_UI_RAW =  ScreenHandler.UI.ScalePositionRawV(MOUSE_POS);

                    Vector2 curMousePosGame = ScreenHandler.TransformPositionToGame(MOUSE_POS_UI);
                    if (!SVec.IsNan(curMousePosGame)) MOUSE_POS_GAME = curMousePosGame;
                    //if (WindowShouldClose() && !InputHandler.QuitPressed()) QUIT = true; // IsKeyDown(KeyboardKey.KEY_ESCAPE)) QUIT = true;
                }



                //UPDATE
                UpdateGame(DELTA);

                // DRAW TO MAIN TEXTURE
                DrawGame();

                ResolveDeferred();
            }
        }
        private void UpdateGame(float dt)
        {
            if (CALL_GAMELOOP_UPDATE) PreUpdate(dt);

            InputHandler.Update(dt);
            AudioHandler.Update(dt);
            UIHandler.Update(dt);
            ScreenHandler.Update(dt);

            //STEP.Update(dt);
            //EASE.Update(dt);
            //ALTERNATOR.Update(dt);
            //TIMER.Update(dt);

            if (CALL_GAMELOOP_HANDLE_INPUT) PreHandleInput();
            if (CUR_SCENE != null)
            {
                if(!CUR_SCENE.IsInputDisabled()) CUR_SCENE.HandleInput();

                if (!CUR_SCENE.IsPaused())
                {
                    delayHandler.Update(DELTA);
                    stopTimer.Update(DELTA);
                    //if (!stopTimer.IsRunning())
                    //{
                    //    slowTimer.Update(DELTA);
                    //    if (CUR_SLOW_FACTOR != 1f && !slowTimer.IsRunning())
                    //    {
                    //        CUR_SLOW_FACTOR = 1f;
                    //    }
                    //}
                    
                    if (!stopTimer.IsRunning())
                    {
                        //CUR_SCENE.Update(dt * CUR_SLOW_FACTOR);
                        CUR_SCENE.Update(dt);
                    }
                }
                //GAME_DELTA = DELTA * CUR_SLOW_FACTOR;
            }
            if (CALL_GAMELOOP_HANDLE_INPUT) PostHandleInput();
            if (CALL_GAMELOOP_UPDATE) PostUpdate(dt);
        }
        private void DrawGame()
        {
            //Draw to game texture
            ScreenHandler.StartDraw(true);
            if (CALL_GAMELOOP_DRAW) PreDraw();
            if (CUR_SCENE != null && !CUR_SCENE.IsHidden()) CUR_SCENE.Draw();
            if (CALL_GAMELOOP_DRAW) PostDraw();
            ScreenHandler.EndDraw(true);

            //Draw to UI texture
            Vector2 uiSize = ScreenHandler.UISize();
            Vector2 stretchFactor = ScreenHandler.UI.STRETCH_FACTOR;
            
            ScreenHandler.StartDraw(false);
            if (CALL_GAMELOOP_DRAWUI) PreDrawUI(uiSize, stretchFactor);
            if (CUR_SCENE != null && !CUR_SCENE.IsHidden()) CUR_SCENE.DrawUI(uiSize, stretchFactor);
            //CursorHandler.Draw(uiSize, MOUSE_POS_UI);
            if (CALL_GAMELOOP_DRAWUI) PostDrawUI(uiSize, stretchFactor);
            ScreenHandler.EndDraw(false);

            
            //Draw textures to screen
            BeginDrawing();
            ClearBackground(backgroundColor);

            //List<ScreenShader> activeShaders = CUR_SHADER_HANDLER != null ? CUR_SHADER_HANDLER.GetCurActiveShaders() : new();
            //var shaders = ShaderHandler.GetCurActiveShaders();
            if (ScreenHandler.CAMERA != null && ScreenHandler.CAMERA.PIXEL_SMOOTHING_ENABLED)
            {
                BeginMode2D(ScreenHandler.CAMERA.ScreenSpaceCam);
                ScreenHandler.Draw();
                //ShaderHandler.DrawShaders();
                EndMode2D();
            }
            else ScreenHandler.Draw(); // ShaderHandler.DrawShaders();

            ScreenHandler.DrawUI();

            
            EndDrawing();
        }

        //public virtual void PreInit() { } //called before initialization -> use for setting specific vars
        public virtual void Start() { } //called after initialization
        public virtual void PreHandleInput() { }
        public virtual void PostHandleInput() { }
        public virtual void PreUpdate(float dt) { } //always called before update
        public virtual void PostUpdate(float dt) { }
        //public virtual void HandleInput() { }//called before update to handle global input
        public virtual void PreDraw() { }//called before draw
        public virtual void PostDraw() { }//called after draw
        public virtual void PreDrawUI(Vector2 uiSize, Vector2 stretchFactor) { }//called before draw
        public virtual void PostDrawUI(Vector2 uiSize, Vector2 stretchFactor) { }//called after draw
        public virtual void End() { } //called before game closes

    }
}
