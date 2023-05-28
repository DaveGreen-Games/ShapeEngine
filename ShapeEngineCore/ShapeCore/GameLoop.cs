global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
using ShapeScreen;
using ShapeTiming;
using Raylib_CsLo;
using System.Numerics;
using System.Runtime.InteropServices;
using ShapeLib;

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

        public string[] LAUNCH_PARAMS { get; private set; } = new string[0];
        
        public bool CallHandleInput = true;
        public bool CallUpdate = true;
        public bool CallDraw = true;
        public bool CallDrawUI = true;
        public Color BackgroundColor = BLACK;

        public Vector2 MousePos { get; private set; } = new(0f);
        public Vector2 MousePosUI { get; private set; } = new(0f);
        public Vector2 MousePosGame { get; private set; } = new(0f);

        public IScene CUR_SCENE { get; private set; } = new SceneEmpty();
        
        private Sequencer<DelayedAction> delayHandler = new();
        private BasicTimer stopTimer = new();
        private List<DeferredInfo> deferred = new();
        private bool quit = false;
        private bool restart = false;

        public GraphicsDevice GFX { get; private set; }

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
        public void StopScene(float duration, float delay = 0f) 
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
        public void CancelStopScene() 
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

        //public GameLoop(GraphicsDevice graphicsDevice, params string[] launchParameters)
        //{
        //    this.GFX = graphicsDevice;
        //    this.LAUNCH_PARAMS = launchParameters;
        //    
        //    quit = false;
        //    restart = false;
        //    Raylib.SetExitKey(-1);
        //}
        //public GameLoop(int devWidth, int devHeight, float gameFactor, float uiFactor, string windowName = "Raylib Game")
        //{
        //    this.GFX = new(devWidth, devHeight, gameFactor, uiFactor, windowName);
        //
        //    quit = false;
        //    restart = false;
        //    Raylib.SetExitKey(-1);
        //}
        

        public void CreateWindow(string windowName, bool undecorated, bool resizable)
        {
            InitWindow(0, 0, windowName);

            if (undecorated) SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            else ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);

            if (resizable) SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            else ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            
            ClearWindowState(ConfigFlags.FLAG_VSYNC_HINT);
        }

        /// <summary>
        /// Starts the gameloop. Runs until Quit() or Restart() is called or the Window is closed by the user.
        /// </summary>
        /// <returns>Returns an exit code for information how the application was quit. Restart has to be handled seperately.</returns>
        public ExitCode Run(GraphicsDevice graphicsDevice, params string[] launchParameters)
        {
            this.LAUNCH_PARAMS = launchParameters;

            this.GFX = graphicsDevice;
            quit = false;
            restart = false;
            Raylib.SetExitKey(-1);

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
                var mp = GetMousePosition();
                if (!mp.IsNan())
                {
                    MousePos = mp;
                    MousePosUI = GFX.UITexture.ScalePosition(mp,GFX.CurWindowSize.width, GFX.CurWindowSize.height);
                    MousePosGame = GFX.TransformPositionToGame(MousePosUI);
                }

                HandleInput();

                UpdateGame(GetFrameTime());

                DrawGame();

                ResolveDeferred();
            }
        }
        private void HandleInput()
        {
            if(!CallHandleInput) return;

            if (BeginHandleInput()) CUR_SCENE.HandleInput();
            EndHandleInput();
        }
        private void UpdateGame(float dt)
        {
            if (!CallUpdate) return;

            GFX.Update(dt);

            if (BeginUpdate(dt))
            {
                if (CUR_SCENE.CallUpdate && !stopTimer.IsRunning) CUR_SCENE.Update(dt, MousePosGame);
            }
            EndUpdate(dt);
        }
        private void DrawGame()
        {
            DrawToCustomTexture();

            //Draw to game texture
            if (CallDraw)
            {
                GFX.BeginDraw();
                if (BeginDraw()) 
                { 
                    if (CUR_SCENE.CallDraw) CUR_SCENE.Draw(MousePosGame);
                }
                EndDraw();
                GFX.EndDraw();
            }

            //Draw to ui texture
            if (CallDrawUI)
            {
                Vector2 uiSize = GFX.UISize();
                GFX.BeginDrawUI();
                if (BeginDrawUI(uiSize))
                {
                    if (CUR_SCENE.CallDraw) CUR_SCENE.DrawUI(uiSize, MousePosUI);
                    
                }
                EndDrawUI(uiSize);
                GFX.EndDrawUI();
            }
            
            //Draw textures to screen
            BeginDrawing();
            ClearBackground(BackgroundColor);

            DrawCustomToScreenFirst();
            
            if(GFX.Camera != null)
            {
                if (GFX.Camera.IsPixelSmoothingCameraEnabled())
                {
                    BeginMode2D(GFX.Camera.GetPixelSmoothingCamera());
                    GFX.DrawGameToScreen();
                    EndMode2D();
                }
                else GFX.DrawGameToScreen();
            }
            else GFX.DrawGameToScreen();

            DrawCustomToScreenMiddle();

            GFX.DrawUIToScreen();

            DrawCustomToScreenLast();


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

        /// <summary>
        /// Use to draw things onto your custom IScreenTextures. Use the graphics device BeginDrawCustom(IScreenTexture texture) & EndDrawCustom(IScreenTexture texture).
        /// </summary>
        public virtual void DrawToCustomTexture() { }

        /// <summary>
        /// Is called before the game texture is drawn to the screen. 
        /// Can be used to draw directly to the screen or to draw custom textures to the screen with the graphics device DrawCustomToScreen(IScreenTexture texture) function.
        /// </summary>
        public virtual void DrawCustomToScreenFirst() { }
        /// <summary>
        /// Is called after the game texture was drawn to screen but before the ui texture is draw to the screen.
        /// Can be used to draw directly to the screen or to draw custom textures to the screen with the graphics device DrawCustomToScreen(IScreenTexture texture) function.
        /// </summary>
        public virtual void DrawCustomToScreenMiddle() { }
        /// <summary>
        /// Is called after the ui texture was drawn to the screen.
        /// Can be used to draw directly to the screen or to draw custom textures to the screen with the graphics device DrawCustomToScreen(IScreenTexture texture) function.
        /// </summary>
        public virtual void DrawCustomToScreenLast() { }





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

    
    //test mouse pos transformation with an an active camera
    //test transformation of positions between 2 textures (with and without cameras)
    
    public interface ICamera2
    {
        public Vector2 WorldToScreen(Vector2 absolutePos);
        public Vector2 ScreenToWorld(Vector2 relativePos);

        public Camera2D GetCamera();
        public bool IsPixelSmoothingCameraEnabled();
        public Camera2D GetPixelSmoothingCamera();

    }
    public class CameraBasic2 : ICamera2
    {
        public float BaseRotationDeg { get; private set; } = 0f;
        public Vector2 BaseOffset { get; private set; } = new(0f);
        public float BaseZoom { get; private set; } = 1f;
        public Vector2 BaseSize { get; private set; } = new(0f);
        public Vector2 Translation { get; set; } = new(0f);
        public float RotationDeg { get; set; } = 0f;
        public float ZoomFactor { get; set; } = 1f;
        //public float ZoomStretchFactor { get; private set; } = 1f;

        public Camera2D WorldCamera { get; private set; }



        public CameraBasic2(Vector2 pos, Vector2 size, Vector2 origin, float baseZoom, float rotation)//, float zoomStretchFactor)
        {
            //ChangeSize(size);//, zoomStretchFactor);
            this.BaseSize = size;
            this.BaseOffset = size * origin; // size / 2;
            this.BaseZoom = baseZoom;
            this.BaseRotationDeg = rotation;
            this.WorldCamera = new() { offset = BaseOffset, rotation = BaseRotationDeg, zoom = baseZoom * ZoomFactor, target = pos };
        }

        public void Update(float dt)
        {
            Vector2 rawCameraOffset = BaseOffset;// + Translation;
            float rawCameraRotationDeg = BaseRotationDeg + RotationDeg;
            float rawCameraZoom = (BaseZoom /* *ZoomStretchFactor */) * ZoomFactor;
            Vector2 rawCameraTarget = Translation; //   WorldCamera.target;

            var c = new Camera2D();
            c.target = rawCameraTarget;
            c.offset = rawCameraOffset;
            c.zoom = rawCameraZoom;
            c.rotation = rawCameraRotationDeg;
            WorldCamera = c;

        }
        //public void ChangeSize(Vector2 newSize)//, float factor)
        //{
        //    BaseOffset = newSize/ 2;
        //    //ZoomStretchFactor = factor;
        //}

        public void ResetZoom() { ZoomFactor = 1f; }
        public void ResetRotation() { RotationDeg = 0f; }
        public void ResetTranslation() { Translation = new(0f); }


        public Vector2 WorldToScreen(Vector2 pos)
        {
            return Raylib.GetWorldToScreen2D(pos, GetCamera());
            //float zoomFactor = 1 / WorldCamera.zoom;
            //Vector2 cPos = WorldCamera.target - WorldCamera.offset * zoomFactor;
            //Vector2 p = (absolutePos - cPos) * WorldCamera.zoom;
            //return p;
        }
        public Vector2 ScreenToWorld(Vector2 pos)
        {
            return Raylib.GetScreenToWorld2D(pos, GetCamera());
            //float zoomFactor = 1 / WorldCamera.zoom;
            //Vector2 p = relativePos * zoomFactor;
            //Vector2 cPos = WorldCamera.target - WorldCamera.offset * zoomFactor;
            //p += cPos;
            //return p;
        }


        public Rect GetArea()
        {
            float zoomFactor = 1 / WorldCamera.zoom;
            Vector2 cPos = WorldCamera.target - WorldCamera.offset * zoomFactor;
            return new(cPos.X, cPos.Y, BaseSize.X * zoomFactor, BaseSize.Y * zoomFactor); // WorldCamera.offset.X * zoomFactor * 2f, WorldCamera.offset.Y * zoomFactor * 2f);
        }
        public bool IsPixelSmoothingCameraEnabled() { return false; }
        public Camera2D GetPixelSmoothingCamera() { return WorldCamera; }
        public Camera2D GetCamera() { return WorldCamera; }
    }
    public class ScreenTexture2
    {
        public uint ID { get; private set; }
        public int DrawOrder { get; set; } = 0;

        public Vector2 Offset { get; set; } = new(0, 0);
        public float Rotation { get; set; } = 0.0f;
        public float Scale { get; set; } = 1.0f;

        public int BlendMode { get; set; } = -1;

        public Vector2 MousePos { get; set; } = new(0f);

        public Color BackgroundColor { get; set; } = new(0, 0, 0, 0);

        public Color Tint { get; set; } = WHITE;

        public IShaderDevice? ShaderDevice { private get; set; } = null;
        public ICamera2? Camera { private get; set; } = null;

        private RenderTexture texture;
        private Rectangle sourceRec;
        private ScreenBuffer[] screenBuffers = new ScreenBuffer[0];
        private List<ScreenFlash> screenFlashes = new List<ScreenFlash>();

        public ScreenTexture2(int width, int height, int drawOrder = 0)
        {
            this.ID = SID.NextID;
            this.Load(width, height);
            this.DrawOrder = drawOrder;
        }
        private void Load(int width, int height)
        {
            float textureSizeFactor = 1f;
            int textureWidth = (int)(width * textureSizeFactor);
            int textureHeight = (int)(height * textureSizeFactor);
            this.texture = LoadRenderTexture(textureWidth, textureHeight);
            this.sourceRec = new Rectangle(0, 0, textureWidth, -textureHeight);
        }

        public void Update(float dt)
        {
            for (int i = screenFlashes.Count() - 1; i >= 0; i--)
            {
                var flash = screenFlashes[i];
                flash.Update(dt);
                if (flash.IsFinished()) { screenFlashes.RemoveAt(i); }

            }
        }
        public void DrawTexture(int targetWidth, int targetHeight, int blendMode = -1)
        {

            float s = Scale;
            var destRec = new Rectangle();
            //destRec.x = targetWidth * 0.5f / s + Offset.X;
            //destRec.y = targetHeight * 0.5f / s + Offset.Y;
            destRec.x = targetWidth * 0.5f + Offset.X;
            destRec.y = targetHeight * 0.5f + Offset.Y;
            
            Vector2 size = AdjustToAspectRatio(targetWidth, targetHeight, GetTextureWidth(), GetTextureHeight());
            float w = size.X * s;
            float h = size.Y * s;
            
            destRec.width = w;
            destRec.height = h;
            
            Vector2 origin = new();
            origin.X = w * 0.5f;
            origin.Y = h * 0.5f;



            /*
            float s = Scale;
            float w = targetWidth * s;
            float h = targetHeight * s;
            var destRec = new Rectangle();
            destRec.x = w * 0.5f / s + Offset.X;
            destRec.y = h * 0.5f / s + Offset.Y;
            destRec.width = w;
            destRec.height = h;


            //var curSize = GetSize() * Scale; // AdjustToAspectRatio(targetWidth, targetHeight) * Scale;
            //var sourceRec = new Rectangle(0, 0, curSize.X, -curSize.Y);
            //var adjustedSize = new Vector2(targetWidth, targetHeight);// AdjustToAspectRatio(targetWidth, targetHeight);
            //var dif = adjustedSize - curSize;
            //var destRec = new Rectangle
            //    (
            //        adjustedSize.X * 0.5f + Offset.X,
            //        adjustedSize.Y * 0.5f + Offset.Y,
            //        adjustedSize.X,
            //        adjustedSize.Y
            //    );

            Vector2 origin = new();
            origin.X = destRec.width * 0.5f;
            origin.Y = destRec.height * 0.5f;
            */

            if (blendMode < 0)
            {
                DrawTexturePro(texture.texture, sourceRec, destRec, origin, Rotation, Tint);
            }
            else
            {
                BeginBlendMode(blendMode);
                DrawTexturePro(texture.texture, sourceRec, destRec, origin, Rotation, Tint);
                EndBlendMode();
            }
        }
        public void Close()
        {
            UnloadRenderTexture(texture);
            foreach (ScreenBuffer screenBuffer in screenBuffers)
            {
                screenBuffer.Unload();
            }
            screenBuffers = new ScreenBuffer[0];
        }

        public void BeginTextureMode()
        {
            if(Camera != null)
            {
                Raylib.BeginTextureMode(texture);
                BeginMode2D(Camera.GetCamera());
                ClearBackground(BackgroundColor);
            }
            else
            {
                Raylib.BeginTextureMode(texture);
                ClearBackground(BackgroundColor);
            }
        }
        public void EndTextureMode()
        {
            if(Camera != null)
            {
                var camera = Camera.GetCamera();
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
        public void DrawToScreen(int targetWidth, int targetHeight)
        {
            if (Camera != null && Camera.IsPixelSmoothingCameraEnabled()) 
                BeginMode2D(Camera.GetPixelSmoothingCamera());
            

            List<ScreenShader> shadersToApply = ShaderDevice != null ? ShaderDevice.GetCurActiveShaders() : new();
            if (shadersToApply.Count <= 0)
            {
                DrawTexture(targetWidth, targetHeight, BlendMode);
                return;
            }
            else if (shadersToApply.Count == 1)
            {
                ScreenShader s = shadersToApply[0];
                BeginShaderMode(s.GetShader());
                DrawTexture(targetWidth, targetHeight, BlendMode);
                EndShaderMode();
            }
            else if (shadersToApply.Count == 2)
            {
                ScreenShader s = shadersToApply[0];
                screenBuffers[0].StartTextureMode();
                BeginShaderMode(s.GetShader());
                DrawTexture(GetTextureWidth(), GetTextureHeight());
                EndShaderMode();
                screenBuffers[0].EndTextureMode();

                s = shadersToApply[1];

                BeginShaderMode(s.GetShader());
                screenBuffers[0].DrawTexture(targetWidth, targetHeight, BlendMode);
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
                DrawTexture(GetTextureWidth(), GetTextureHeight());
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
                    current.DrawTexture(GetTextureWidth(), GetTextureHeight());
                    EndShaderMode();
                    next.EndTextureMode();
                    currentIndex = currentIndex == 0 ? 1 : 0;
                }

                BeginShaderMode(endshader.GetShader());
                screenBuffers[nextIndex].DrawTexture(targetWidth, targetHeight, BlendMode);
                EndShaderMode();
            }


            if (Camera != null && Camera.IsPixelSmoothingCameraEnabled()) EndMode2D();
        }
        
        public Vector2 GetSize() { return new(texture.texture.width, texture.texture.height); }
        public int GetTextureWidth() { return texture.texture.width; }
        public int GetTextureHeight() { return texture.texture.height; }
        //public float GetTextureSizeFactor() { return textureSizeFactor; }

        public Vector2 GetSizeFactor(Vector2 targetSize)
        {
            float x = GetTextureWidth() / targetSize.X;
            float y = GetTextureHeight() / targetSize.Y;
            return new(x, y);
        }
        private Vector2 AdjustToAspectRatio(float fromWidth, float fromHeight, float toWidth, float toHeight)
        {
            float w, h;
            float fWidth = fromWidth / toWidth;
            float fHeight = fromHeight / toHeight;
            if ((fWidth >=1f && fWidth <= fHeight) || (fWidth < 1f && fWidth > fHeight))
            {
                w = fromWidth;
                float f = toHeight / toWidth;
                h = w * f;
            }
            else
            {
                h = fromHeight;
                float f = toWidth / toHeight;
                w = h * f;
            }
            return new(w, h);
        }

        /// <summary>
        /// Transforms a position relative to the screen to a position relative to this texture.
        /// </summary>
        /// <param name="screenPos"></param>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        /// <returns></returns>
        public Vector2 TransformFromScreen(Vector2 screenPos, int screenWidth, int screenHeight)
        {
            Vector2 size = AdjustToAspectRatio(screenWidth, screenHeight, GetTextureWidth(), GetTextureHeight()) * Scale;
            Vector2 dif = new Vector2(screenWidth, screenHeight) - size;
            dif *= 0.5f;
            screenPos -= dif;
            screenPos -= Offset;
            float fWidth = GetTextureWidth() / size.X;
            float fHeight = GetTextureHeight() / size.Y;
            screenPos.X = Clamp(screenPos.X * fWidth, 0, GetTextureWidth());
            screenPos.Y = Clamp(screenPos.Y * fHeight, 0, GetTextureHeight());
            return ScreenToWorld(screenPos);
        }
        /// <summary>
        /// Transforms a position relative to this texture to position relative to the screen.
        /// </summary>
        /// <param name="screenPos"></param>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        /// <returns></returns>
        public Vector2 TransformToScreen(Vector2 screenPos, int screenWidth, int screenHeight)
        {
            screenPos = WorldToScreen(screenPos);
            Vector2 size = AdjustToAspectRatio(GetTextureWidth() / Scale, GetTextureHeight() / Scale, screenWidth, screenHeight);
            Vector2 dif = GetSize() - size;
            dif *= 0.5f;
            screenPos -= dif;
            float fWidth = screenWidth / size.X;
            float fHeight = screenHeight / size.Y;
            screenPos.X = Clamp(screenPos.X * fWidth, 0, screenWidth);
            screenPos.Y = Clamp(screenPos.Y * fHeight, 0, screenHeight);
            screenPos += Offset;
            return screenPos;
        }
        
        /// <summary>
        /// Transforms a 
        /// </summary>
        /// <param name="screenPos"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Vector2 TransformScreenPos(Vector2 screenPos, ScreenTexture2 from, ScreenTexture2 to) 
        {
            //return TransformFromScreen(screenPos, other.GetTextureWidth(), other.GetTextureHeight()); 
            return new();
        }
        public static Vector2 TransformWorldPos(Vector2 worldPos, ScreenTexture2 from, ScreenTexture2 to) 
        {
            var screenPos = from.WorldToScreen(worldPos);
            var transformed = new Vector2();//do the magic here
            return to.ScreenToWorld(transformed);
        }

        /// <summary>
        /// Transforms a world position to a relative screen position if Camera != null.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector2 WorldToScreen(Vector2 pos)
        {
            if (Camera != null) return Camera.WorldToScreen(pos);
            else return pos;
        }
        /// <summary>
        /// Transforms a relative screen position to a world position if Camera != null.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector2 ScreenToWorld(Vector2 pos)
        {
            if (Camera != null) return Camera.ScreenToWorld(pos);
            else return pos;
        }
        

        /// <summary>
        /// Adjusts the width and height to match the aspect ratio of this screen texture.
        /// If you want to transform from a 400x400 to 200x400 texture, this function would return 200x200. (aspect ration 1:1 like the original)
        /// </summary>
        /// <param name="width">The width of the target.</param>
        /// <param name="height">The height of the target. </param>
        /// <returns>Returns the adjusted size in the aspect ratio of this screen texture.</returns>
        
        public void Flash(float duration, Color startColor, Color endColor)
        {
            if (duration <= 0.0f) return;
            ScreenFlash flash = new(duration, startColor, endColor);
            screenFlashes.Add(flash);
        }
        public void StopFlash() { screenFlashes.Clear(); }


        /*
        public Vector2 TransformAbsolutePosition(Vector2 absolutePos, int targetWidth, int targetHeight)
        {
            var rel = TransformToRelative(absolutePos);
            rel = TransformRelativePosition(rel, targetWidth, targetHeight);
            return TransformToAbsolute(rel);
        }
        public Vector2 TransformAbsolutePosition(Vector2 absolutePos, ScreenTexture2 other)
        {
            var rel = other.TransformToRelative(absolutePos);
            rel = TransformRelativePosition(rel, other);
            return TransformToAbsolute(rel);
        }
        */
    }

    public abstract class GameLoop2
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

        private Dictionary<uint,ScreenTexture2> screenTextures = new();
        public Vector2 MousePos { get; private set; } = new(0f);

        public Color BackgroundColor = BLACK;

        protected bool quit = false;
        protected bool restart = false;
        
        private List<DeferredInfo> deferred = new();



        public delegate void WindowSizeChanged(int w, int h);
        public event WindowSizeChanged? OnWindowSizeChanged;

        public float ScreenEffectIntensity = 1.0f;

        public int FrameRateLimit { get; private set; } = 60;
        public int Fps { get; private set; }
        public bool VSync { get; private set; } = true;
        public (int width, int height) CurWindowSize { get; private set; } = (0, 0);
        public (int width, int height) WindowedWindowSize { get; private set; } = (0, 0);
        public (int width, int height) WindowMinSize { get; } = (128, 128);
        //public (int width, int height) DevResolution { get; private set; } = (0, 0);

        public MonitorDevice Monitor { get; private set; }



        public ICursor Cursor { get; private set; } = new NullCursor();

        public GameLoop2()
        {
            InitWindow(0, 0, "");

            ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);

            ClearWindowState(ConfigFlags.FLAG_VSYNC_HINT);

            Monitor = new MonitorDevice();

            SetupWindowDimensions();

            FrameRateLimit = 60;
            SetVsync(true);
            Raylib.SetWindowMinSize(WindowMinSize.width, WindowMinSize.height);
        }
        public void SetupWindow(string windowName, bool undecorated, bool resizable)
        {
            SetWindowTitle(windowName);
            if (undecorated) SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            else ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);

            if (resizable) SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            else ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
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

        public void Flash(uint screenTextureID, float duration, Color startColor, Color endColor)
        {
            if (!screenTextures.ContainsKey(screenTextureID)) return;

            byte startColorAlpha = (byte)(startColor.a * ScreenEffectIntensity);
            startColor.a = startColorAlpha;
            byte endColorAlpha = (byte)(endColor.a * ScreenEffectIntensity);
            endColor.a = endColorAlpha;

            var screenTexture = screenTextures[screenTextureID];

            screenTexture.Flash(duration, startColor, endColor);
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


        public ScreenTexture2 AddScreenTexture(int width, int height, int drawOrder)
        {
            var newScreenTexture = new ScreenTexture2(width, height, drawOrder);

            AddScreenTexture(newScreenTexture);

            return newScreenTexture;
        }
        public bool AddScreenTexture(ScreenTexture2 newScreenTexture) 
        {
            if (screenTextures.ContainsKey(newScreenTexture.ID)) return false;
            screenTextures.Add(newScreenTexture.ID, newScreenTexture);
            return true;
        }
        public bool RemoveScreenTexture(ScreenTexture2 screenTexture) { return screenTextures.Remove(screenTexture.ID); }
        public bool RemoveScreenTexture(uint id) { return screenTextures.Remove(id); }


        private void StartGameloop()
        {
            LoadContent();
            BeginRun();
        }
        private void RunGameloop()
        {
            while (!quit)
            {
                CheckWindowSizeChanged();

                var mp = GetMousePosition();
                if (!mp.IsNan()) SetMousePos(mp);


                HandleInput();

                float dt = GetFrameTime();
                UpdateMonitorDevice(dt);
                Update(dt);
                var sortedTextures = SortScreenTextures(screenTextures.Values.ToList());
                UpdateScreenTextures(sortedTextures, dt);

                DrawGameloopToTextures(sortedTextures);
                DrawGameloopToScreen(sortedTextures);

                ResolveDeferred();
            }
        }
        private void EndGameloop()
        {
            EndRun();
            UnloadContent();
            foreach (var st in screenTextures.Values)
            {
                st.Close();
            }
        }
        private void UpdateMonitorDevice(float dt)
        {
            
            var newMonitor = Monitor.HasMonitorSetupChanged();
            if (newMonitor.available)
            {
                MonitorChanged(newMonitor);
            }
        }
        private void UpdateScreenTextures(List<ScreenTexture2> sortedTextures, float dt)
        {
            foreach (var st in sortedTextures)
            {
                st.MousePos = st.TransformFromScreen(MousePos, CurWindowSize.width, CurWindowSize.height);
                //st.MousePos = st.TransformAbsolutePosition(MousePos, CurWindowSize.width, CurWindowSize.height);
                st.Update(dt);
            }
        }
        private void DrawGameloopToTextures(List<ScreenTexture2> sortedTextures) 
        {
            foreach (var st in sortedTextures)
            {
                st.BeginTextureMode();
                Draw(st);
                st.EndTextureMode();
            }
        }
        private void DrawGameloopToScreen(List<ScreenTexture2> sortedTextures) 
        {
            Vector2 curScreenSize = new(CurWindowSize.width, CurWindowSize.height);
            BeginDrawing();
            ClearBackground(BackgroundColor);

            foreach (var st in sortedTextures)
            {
                st.DrawToScreen(CurWindowSize.width, CurWindowSize.height);
            }

            DrawToScreen(curScreenSize);

            DrawCursor(curScreenSize, MousePos);

            EndDrawing();
        }
        private List<ScreenTexture2> SortScreenTextures(List<ScreenTexture2> textures)
        {
            textures.Sort(delegate (ScreenTexture2 x, ScreenTexture2 y)
            {
                if (x == null || y == null) return 0;

                if (x.DrawOrder < y.DrawOrder) return -1;
                else if (x.DrawOrder > y.DrawOrder) return 1;
                else return 0;
            });
            return textures;
        }
        private void SetMousePos(Vector2 newPos)
        {
            MousePos = newPos;
        }
        private void CheckWindowSizeChanged()
        {
            if (IsFullscreen()) return;

            int w = GetScreenWidth();
            int h = GetScreenHeight();
            
            var monitor = Monitor.CurMonitor();
            int maxW = monitor.width;
            int maxH = monitor.height;
            
            if (CurWindowSize.width != w || CurWindowSize.height != h)
            {
                int newW = SUtils.Clamp(w, WindowMinSize.width, maxW);
                int newH = SUtils.Clamp(h, WindowMinSize.height, maxH);
                CurWindowSize = (newW, newH);

                WindowedWindowSize = CurWindowSize;
            }
        }


        //how to do that??? how to do drawing? call draw for every screen texture? call draw on the cur scene for every screen texture?
        //adding screen texture returns id -> no access to screen texture just adding and removing
        //scene should have a draw function for directly drawing to the screen if the user does not want to use screen textures
        //how to know wich screen texture is which -> do they store their id or do the have a name?
        //screen textures should either be drawn in the order of the list or the have a draworder that sorts them (sorted list)
        //implement screen texture interface with only functions that the gameloop uses
        //probably gameloop light and advanced are not necessary anymore
        //protected virtual void HandleInputGameloop() 
        //{
        //}
        //protected virtual void UpdateGameloop(float dt) 
        //{
        //}
        //protected virtual void DrawGameloop()
        //{
        //}


        /// <summary>
        /// Called first after starting the gameloop.
        /// </summary>
        protected virtual void LoadContent() { }
        /// <summary>
        /// Called after LoadContent but before the main loop has started.
        /// </summary>
        protected virtual void BeginRun() { }
        
        protected virtual void HandleInput() { }
        protected virtual void Update(float dt) { }
        protected virtual void Draw(ScreenTexture2 screenTexture) { }
        protected virtual void DrawToScreen(Vector2 screenSize) { }

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
            if (monitor.available)
            {
                MonitorChanged(monitor);
                return true;
            }
            return false;
        }
        public void NextMonitor()
        {
            var nextMonitor = Monitor.NextMonitor();
            if (nextMonitor.available)
            {
                MonitorChanged(nextMonitor);
            }
        }
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
            Fps = newFps;
            SetTargetFPS(Fps);
        }
        public void SetVsync(bool enabled)
        {
            if (enabled)
            {
                VSync = true;
                SetFPS(Monitor.CurMonitor().refreshrate);
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
        public void ResizeWindow(int newWidth, int newHeight)
        {
            ChangeWindowDimensions(newWidth, newHeight, false);
        }
        public void ResetWindow()
        {
            if (IsWindowFullscreen())
            {
                Raylib.ToggleFullscreen();
            }
            var monitor = Monitor.CurMonitor();
            ChangeWindowDimensions(monitor.width / 2, monitor.height / 2, false);
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
                if (WindowedWindowSize.width > monitor.width || WindowedWindowSize.height > monitor.height)
                {
                    WindowedWindowSize = (monitor.width / 2, monitor.height / 2);
                }

                ChangeWindowDimensions(WindowedWindowSize.width, WindowedWindowSize.height, false);
                ChangeWindowDimensions(WindowedWindowSize.width, WindowedWindowSize.height, false);//needed for some monitors ...
            }
            else
            {
                ChangeWindowDimensions(monitor.width, monitor.height, true);
                SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            }

            return IsFullscreen();
        }

        
        private void MonitorChanged(MonitorInfo monitor)
        {
            int prevWidth = CurWindowSize.width;
            int prevHeight = CurWindowSize.height;

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
                if (windowWidth > monitor.width || windowHeight > monitor.height)
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
            if (VSync)
            {
                SetFPS(monitor.refreshrate);
            }
        }
        private void ChangeWindowDimensions(int newWidth, int newHeight, bool fullscreenChange = false)
        {
            //if (newWidth == CUR_WINDOW_SIZE.width && newHeight == CUR_WINDOW_SIZE.height) return;

            CurWindowSize = (newWidth, newHeight);
            if (!fullscreenChange) WindowedWindowSize = (newWidth, newHeight);
            //GAME.ChangeWindowSize(newWidth, newHeight);
            //UI.ChangeWindowSize(newWidth, newHeight);

            SetWindowSize(newWidth, newHeight);
            var monitor = Monitor.CurMonitor();

            int winPosX = monitor.width / 2 - newWidth / 2;
            int winPosY = monitor.height / 2 - newHeight / 2;
            //SetWindowPosition(winPosX + (int)MONITOR_OFFSET.X, winPosY + (int)MONITOR_OFFSET.Y);
            SetWindowPosition(winPosX + (int)monitor.position.X, winPosY + (int)monitor.position.Y);

            OnWindowSizeChanged?.Invoke(newWidth, newHeight);
        }
        private void SetupWindowDimensions()
        {
            var monitor = Monitor.CurMonitor();
            int newWidth = monitor.width / 2;
            int newHeight = monitor.height / 2;

            if (newWidth == CurWindowSize.width && newHeight == CurWindowSize.height) return;

            CurWindowSize = (newWidth, newHeight);
            WindowedWindowSize = (newWidth, newHeight);

            SetWindowSize(newWidth, newHeight);
            int winPosX = monitor.width / 2 - newWidth / 2;
            int winPosY = monitor.height / 2 - newHeight / 2;
            //SetWindowPosition(winPosX + (int)MONITOR_OFFSET.X, winPosY + (int)MONITOR_OFFSET.Y);
            SetWindowPosition(winPosX + (int)monitor.position.X, winPosY + (int)monitor.position.Y);

            OnWindowSizeChanged?.Invoke(newWidth, newHeight);
        }

    }

    
    
    
    
    //add a lot of the stuff from graphics device to screen texture and the rest to gameloop
    //-> gameloop can have any number of screen textures, always has 1
    //screentextures have their own shader device and camera
    //to transform coordinates from one screen texture to another just figure out the factor and if a camera is used public Vector2 TransformPos(this ScreenTexture self, ScreenTexture other, Vector2 pos)?
    //-> maybe screen textures have always a basic camera that can shake (flash and shake would then be supported by every screen texture)
    
    //make the most basic gameloop into gl (if not possible, than the 2 gameloop variants are just completely seperate)
    //implement a light gameloop -> only has 1 texture (no factors)
    //implement a basic gameloop with 2 textures for game and ui
    //combine graphics device and gameloop? -> there is a lot of unnecessary duplication of functions etc.
    /*
    public class GameloopLight : GameLoop2
    {
        public ScreenTexture2 ScreenTexture { get; private set; }

        public GameloopLight()
        {

        }
        protected override void HandleInputGameloop()
        {
            HandleInput();
        }
        protected override void DrawGameloop()
        {
            this.ScreenTexture.BeginTextureMode();
            Draw(this.ScreenTexture);
            this.ScreenTexture.EndTextureMode();

            BeginDrawing();
            ClearBackground(BackgroundColor);
            this.ScreenTexture.DrawToScreen(CurWindowSize.width, CurWindowSize.height);
            EndDrawing();

        }
        protected override void UpdateGameloop(float dt)
        {
            this.ScreenTexture.Update(dt);
            Update(dt);
        }
        protected override void SetMousePos(Vector2 newPos)
        {
            base.SetMousePos(newPos);
            this.ScreenTexture.MousePos = this.ScreenTexture.TransformRelativePosition(MousePos, CurWindowSize.width, CurWindowSize.height);
        }

        protected virtual void HandleInput() { }
        protected virtual void Update(float dt) { }
        protected virtual void Draw(ScreenTexture2 screenTexture) { }
    }

    public class GameloopBackup : GameLoop2
    {
        
        public bool CallHandleInput = true;
        public bool CallUpdate = true;
        public bool CallDraw = true;
        public bool CallDrawUI = true;
        

        
        public Vector2 MousePosUI { get; private set; } = new(0f);
        public Vector2 MousePosGame { get; private set; } = new(0f);

        public IScene CUR_SCENE { get; private set; } = new SceneEmpty();

        private Sequencer<DelayedAction> delayHandler = new();
        private BasicTimer stopTimer = new();
        
        


        public void GoToScene(IScene newScene)
        {
            if (newScene == null) return;
            if (newScene == CUR_SCENE) return;
            CUR_SCENE.Deactivate();
            newScene.Activate(CUR_SCENE);
            CUR_SCENE = newScene;
        }


        /// <summary>
        /// Stop the current scene for the duration. Only affects Update().
        /// </summary>
        /// <param name="duration"></param>
        public void StopScene(float duration, float delay = 0f)
        {
            if (delay > 0)
            {
                var a = new DelayedAction(delay, () => { stopTimer.Start(duration); });
                delayHandler.StartSequence(a);
            }
            else
            {
                if (delayHandler.HasSequences()) delayHandler.Stop();
                stopTimer.Start(duration);
            }
        }
        public void CancelStopScene()
        {
            //if (delayHandler.Has("stop")) delayHandler.Remove("stop");
            stopTimer.Stop();
        }



    }

    public class GameloopBasic : GameLoop2
    {

    }
    */
}
