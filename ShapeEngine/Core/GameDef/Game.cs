using Raylib_cs;
using ShapeEngine.Audio;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Screen;

namespace ShapeEngine.Core.GameDef;

/// <summary>
/// The core game class.
/// Inherit this class, create a new instance of this class and call Run() to start the game.
/// </summary>
/// <remarks>
/// Main Game loop call order:
/// <list type="bullet">
/// <item><see cref="LoadContent"/> - Called once.</item>
/// <item><see cref="BeginRun"/> - Called once.</item>
/// <item><see cref="Update"/> - Called every frame with variable timing.</item>
/// <item><see cref="DrawGame"/>- Called every frame.</item>
/// <item><see cref="DrawGameUI"/> - Called every frame.</item>
/// <item><see cref="DrawUI"/> - Called every frame.</item>
/// <item><see cref="EndRun"/> - Called once.</item>
/// <item><see cref="UnloadContent"/> - Called once.</item>
/// </list>
/// Fixed Game loop call order:
/// <list type="bullet">
/// <item><see cref="LoadContent"/> - Called once.</item>
/// <item><see cref="BeginRun"/> - Called once.</item>
/// <item><see cref="PreFixedUpdate"/> - Called every frame with variable timing.</item>
/// <item><see cref="FixedUpdate"/> - Called in a fixed interval with fixed timing.</item>
/// <item><see cref="InterpolateFixedUpdate"/> - Called every frame with variable timing.</item>
/// <item><see cref="DrawGame"/> - Called every frame.</item>
/// <item><see cref="DrawGameUI"/> - Called every frame.</item>
/// <item><see cref="DrawUI"/> - Called every frame.</item>
/// <item><see cref="EndRun"/> - Called once.</item>
/// <item><see cref="UnloadContent"/> - Called once.</item>
/// </list>
/// </remarks>
public partial class Game
{
    #region Public Members
    /// <summary>
    /// Gets or sets the command-line arguments passed to the application at launch.
    /// </summary>
    /// <remarks>
    /// This property stores any parameters provided when starting the application.
    /// It can be used to configure the game behavior based on launch arguments.
    /// </remarks>
    public string[] LaunchParams { get; protected set; } = [];

    /// <summary>
    /// Gets whether the fixed physics update system is enabled.
    /// </summary>
    /// <remarks>
    /// When true, the fixed update functions will be called at the FixedPhysicsFramerate.
    /// Fixed Update call order:
    /// <list type="bullet">
    /// <item><see cref="PreFixedUpdate"/> with variable timing.</item>
    /// <item><see cref="FixedUpdate"/> with fixed timing.</item>
    /// <item><see cref="InterpolateFixedUpdate"/> with variable timing.</item>
    /// <item><see cref="DrawGame"/></item>
    /// <item><see cref="DrawGameUI"/></item>
    /// <item><see cref="DrawUI"/></item>
    /// </list>
    /// Unlocked Update call order:
    /// <list type="bullet">
    /// <item><see cref="Update"/> with variable timing.</item>
    /// <item><see cref="DrawGame"/></item>
    /// <item><see cref="DrawGameUI"/></item>
    /// <item><see cref="DrawUI"/></item>
    /// </list>
    /// </remarks>
    public bool FixedPhysicsEnabled { get; private set; }

    /// <summary>
    /// Gets the target framerate for fixed physics updates.
    /// </summary>
    /// <remarks>
    /// This value determines how many physics updates will be performed per second.
    /// Higher values provide more accurate physics but require more processing power.
    /// </remarks>
    public int FixedPhysicsFramerate { get; private set; }

    /// <summary>
    /// Gets the time interval in seconds between fixed physics updates.
    /// </summary>
    /// <remarks>
    /// This value is calculated as 1.0 / FixedPhysicsFramerate and represents
    /// the duration of each physics step in seconds.
    /// </remarks>
    public float FixedPhysicsTimestep { get; private set; }

    /// <summary>
    /// Gets the game time information for the variable update loop.
    /// </summary>
    /// <remarks>
    /// Contains timing data such as elapsed time, delta time, and frame count
    /// for the main game loop that runs at variable framerates.
    /// </remarks>
    public GameTime Time { get; private set; } = new GameTime();

    /// <summary>
    /// Gets the game time information for the fixed update loop.
    /// </summary>
    /// <remarks>
    /// Contains timing data for the physics update loop that runs at a fixed timestep.
    /// Only relevant when FixedPhysicsEnabled is true.
    /// </remarks>
    public GameTime FixedTime { get; private set; } = new GameTime();

    /// <summary>
    /// Gets or sets the background color of the game window.
    /// </summary>
    /// <remarks>
    /// This color is used to clear the screen before rendering each frame.
    /// </remarks>
    public ColorRgba BackgroundColorRgba = ColorRgba.Black;

    /// <summary>
    /// Gets or sets the intensity of screen effects like flashes.
    /// </summary>
    /// <remarks>
    /// Values range from 0.0 (no effect) to 1.0 (full effect).
    /// This can be used to reduce the intensity of visual effects.
    /// </remarks>
    public float ScreenEffectIntensity = 1.0f;

    /// <summary>
    /// Gets the shader container for the game's screen effects.
    /// </summary>
    /// <remarks>
    /// Provides access to the shaders that can be applied to the game's render texture.
    /// May be null if the game texture doesn't support shaders.
    /// </remarks>
    public ShaderContainer? ScreenShaders => gameTexture.Shaders;

    /// <summary>
    /// Gets or sets the active camera used for rendering the game.
    /// </summary>
    /// <remarks>
    /// When setting a new camera, the current camera is deactivated,
    /// the new camera is set as active, and its size is adjusted to match the current window.
    /// </remarks>
    public ShapeCamera Camera
    {
        get => curCamera;
        set
        {
            if (value == curCamera) return;
            curCamera.Deactivate();
            curCamera = value;
            gameTexture.Camera = curCamera;
            curCamera.Activate();
            curCamera.SetSize(Window.CurScreenSize);
        }
    }

    /// <summary>
    /// Gets information about the game's rendering area.
    /// </summary>
    /// <remarks>
    /// Contains details about the game's viewport, dimensions, and mouse position
    /// in game coordinates.
    /// </remarks>
    public ScreenInfo GameScreenInfo { get; private set; }

    /// <summary>
    /// Gets information about the game's UI rendering area.
    /// </summary>
    /// <remarks>
    /// Contains details about the game UI's viewport, dimensions, and mouse position
    /// in game UI coordinates.
    /// </remarks>
    public ScreenInfo GameUiScreenInfo { get; private set; }

    /// <summary>
    /// Gets information about the window's UI rendering area.
    /// </summary>
    /// <remarks>
    /// Contains details about the window UI's viewport, dimensions, and mouse position
    /// in window coordinates.
    /// </remarks>
    public ScreenInfo UIScreenInfo { get; private set; }

    private bool paused;
    /// <summary>
    /// Gets or sets whether the game is currently paused.
    /// </summary>
    /// <remarks>
    /// When set to true, time-dependent game updates may be suspended.
    /// Setting this property triggers the OnPausedChanged event.
    /// </remarks>
    public bool Paused
    {
        get => paused;
        set
        {
            if (value != paused)
            {
                paused = value;
                ResolveOnPausedChanged(paused);
            }
        }
    }

    /// <summary>
    /// Gets the game window that contains the rendering surface.
    /// </summary>
    /// <remarks>
    /// Provides access to window-related functionality such as size, position,
    /// fullscreen state, and input handling.
    /// </remarks>
    public GameWindow Window { get; private set; }

    /// <summary>
    /// Gets the audio device used for sound playback.
    /// </summary>
    /// <remarks>
    /// Provides access to audio functionality such as playing sounds and music,
    /// adjusting volume, and managing audio resources.
    /// </remarks>
    public readonly AudioDevice AudioDevice;

    /// <summary>
    /// Gets the currently active scene.
    /// </summary>
    /// <remarks>
    /// The current scene handles game-specific logic, rendering, and input.
    /// Use GoToScene method to change the active scene.
    /// </remarks>
    public Scene CurScene { get; private set; } = new SceneEmpty();

    /// <summary>
    /// Gets the main render texture used for the game.
    /// </summary>
    /// <remarks>
    /// This texture is where the game world is rendered before being drawn to the screen.
    /// It may apply scaling, filtering, or other effects to the final output.
    /// </remarks>
    public ScreenTexture GameTexture => gameTexture;
    
    /// <summary>
    /// Gets the current game texture used for rendering.
    /// </summary>
    /// <returns>The current ScreenTexture instance used by the game.</returns>
    public ScreenTexture GetGameTexture() => gameTexture;

    #endregion
    
    #region Private Members

    private ScreenTexture gameTexture;
    private readonly ShapeCamera basicCamera = new();
    private ShapeCamera curCamera;
    
    private bool quit;
    private bool restart;
    
    private readonly List<ShapeFlash> shapeFlashes = [];
    private readonly List<DeferredInfo> deferred = [];

    private float physicsAccumulator;

    private List<ScreenTexture>? customScreenTextures;
    #endregion
    
    #region Constructor
    /// <summary>
    /// Initializes a new instance of the Game class with the specified game settings and window settings.
    /// </summary>
    /// <remarks>
    /// Creating a new instance of the game class will set <see cref="CurrentGameInstance"/> to the newly created class!
    /// You should never create more than one instance of the game class per application!
    /// </remarks>
    /// <param name="gameSettings">The settings for the game, including fixed framerate, screen texture mode, and rendering options.</param>
    /// <param name="windowSettings">The settings for the window, including size, position, and display properties.</param>
    public Game(GameSettings gameSettings, WindowSettings windowSettings)
    {
        CurrentGameInstance = this;
        
        #if DEBUG
        DebugMode = true;
        ReleaseMode = false;
        #endif
        
        // this.DevelopmentDimensions = gameSettings.DevelopmentDimensions;
        Window = new(windowSettings);
        Window.OnWindowSizeChanged += ResolveOnWindowSizeChanged;
        Window.OnWindowPositionChanged += ResolveOnWindowPositionChanged;
        Window.OnMonitorChanged += ResolveOnMonitorChanged;
        Window.OnMouseVisibilityChanged += ResolveOnMouseVisibilityChanged;
        Window.OnMouseEnabledChanged += ResolveOnMouseEnabledChanged;
        Window.OnMouseEnteredScreen += ResolveOnMouseEnteredScreen;
        Window.OnMouseLeftScreen += ResolveOnMouseLeftScreen;
        
        
        Window.OnWindowFocusChanged += ResolveOnWindowFocusChanged;
        Window.OnWindowFullscreenChanged += ResolveOnWindowFullscreenChanged;
        Window.OnWindowMaximizeChanged += ResolveOnWindowMaximizeChanged;
        Window.OnWindowMinimizedChanged += ResolveOnWindowMinimizedChanged;
        Window.OnWindowHiddenChanged += ResolveOnWindowHiddenChanged;
        Window.OnWindowTopmostChanged += ResolveOnWindowTopmostChanged;

        AudioDevice = new AudioDevice();

        var fixedFramerate = gameSettings.FixedFramerate;
        if (fixedFramerate <= 0)
        {
            FixedPhysicsFramerate = -1;
            FixedPhysicsTimestep = -1;
            FixedPhysicsEnabled = false;
        }
        else
        {
            if (fixedFramerate < 30) fixedFramerate = 30;
            FixedPhysicsFramerate = fixedFramerate;
            FixedPhysicsTimestep = 1f / FixedPhysicsFramerate;
            FixedPhysicsEnabled = true;
        }
        
        curCamera = basicCamera;
        curCamera.Activate();
        curCamera.SetSize(Window.CurScreenSize);
        
        var mousePosUI = Window.MousePosition;

        var screenTextureMode = gameSettings.ScreenTextureMode;
        if (screenTextureMode == ScreenTextureMode.Stretch)
        {
            gameTexture = new(gameSettings.ShaderSupportType, gameSettings.TextureFilter);
        }
        else if (screenTextureMode == ScreenTextureMode.Fixed)
        {
            gameTexture = new(gameSettings.FixedDimensions, gameSettings.ShaderSupportType, gameSettings.TextureFilter);
        }
        else if (screenTextureMode == ScreenTextureMode.NearestFixed)
        {
            gameTexture = new(gameSettings.FixedDimensions, gameSettings.ShaderSupportType, gameSettings.TextureFilter, true);
        }
        else
        {
            gameTexture = new(gameSettings.PixelationFactor, gameSettings.ShaderSupportType, gameSettings.TextureFilter);
        }
        
        gameTexture.OnTextureResized += GameTextureOnTextureResized;
        gameTexture.Initialize(Window.CurScreenSize, mousePosUI, curCamera);
        gameTexture.OnDrawGame += GameTextureOnDrawGame;
        gameTexture.OnDrawUI += GameTextureOnDrawUI;
        
        GameScreenInfo = gameTexture.GameScreenInfo;
        GameUiScreenInfo = gameTexture.GameUiScreenInfo;
        UIScreenInfo = new(Window.ScreenArea, mousePosUI);

        ShapeInput.OnInputDeviceChanged += ResolveOnInputDeviceChanged;
        ShapeInput.GamepadDeviceManager.OnGamepadConnectionChanged += ResolveOnGamepadConnectionChanged;
        
        //This sets the current directory to the executable's folder, enabling double-click launches.
        //without this, the executable has to be launched from the command line
        if (IsOSX())
        {
            string exeDir = AppContext.BaseDirectory;
            if (!string.IsNullOrEmpty(exeDir))
            {
                Directory.SetCurrentDirectory(exeDir);
            }
            else Console.WriteLine("Failed to set current directory to executable's folder in macos.");
        }
    }
    #endregion

    #region Custom Screen Textures

    /// <summary>
    /// Adds a custom screen texture to the game's rendering pipeline.
    /// </summary>
    /// <param name="texture">The screen texture to add to the game's collection of custom textures.</param>
    /// <returns>
    /// Returns true if the texture was successfully added to the collection.
    /// Returns false if the texture was already present in the collection.
    /// </returns>
    public bool AddScreenTexture(ScreenTexture texture)
    {
        if (customScreenTextures == null)
        {
            customScreenTextures = new(2) { texture };
            return true;
        }

        if (customScreenTextures.Contains(texture)) return false;

        customScreenTextures.Add(texture);
        return true;

    }
    
    /// <summary>
    /// Checks if a specific screen texture is already in the game's custom texture collection.
    /// </summary>
    /// <param name="texture">The screen texture to check for.</param>
    /// <returns>
    /// Returns true if the texture is present in the collection.
    /// Returns false if the texture is not in the collection or if the collection is null.
    /// </returns>
    public bool HasScreenTexture(ScreenTexture texture) => customScreenTextures?.Contains(texture) ?? false;
   
    /// <summary>
    /// Removes a specific screen texture from the game's custom texture collection.
    /// </summary>
    /// <param name="texture">The screen texture to remove.</param>
    /// <returns>
    /// Returns true if the texture was successfully removed from the collection.
    /// Returns false if the texture was not found in the collection or if the collection is null.
    /// </returns>
    public bool RemoveScreenTexture(ScreenTexture texture)
    {
        if (customScreenTextures == null) return false;
        return customScreenTextures.Remove(texture);
    }
    
    /// <summary>
    /// Removes all screen textures from the game's custom texture collection.
    /// </summary>
    /// <returns>
    /// Returns the number of screen textures that were removed from the collection.
    /// Returns 0 if the collection was already empty or null.
    /// </returns>
    public int ClearScreenTextures()
    {
        if (customScreenTextures == null) return 0;

        int count = customScreenTextures.Count;
        customScreenTextures.Clear();
        return count;
    }
    
    #endregion

    #region Cursor
    /// <summary>
    /// Updates the cursor based on the current state of the game.
    /// This method is called every frame and can be overridden in derived classes to implement custom cursor behavior.
    /// </summary>
    /// <param name="dt">The time elapsed since the last frame in seconds.</param>
    /// <param name="gameInfo">Information about the game screen area.</param>
    /// <param name="gameUiInfo">Information about the game UI screen area.</param>
    /// <param name="uiInfo">Information about the general UI screen area.</param>

    protected virtual void UpdateCursor(float dt, ScreenInfo gameInfo, ScreenInfo gameUiInfo, ScreenInfo uiInfo)
    {
        
    }
    /// <summary>
    /// Draws the cursor in the game world space.
    /// </summary>
    /// <param name="gameInfo">Information about the game screen area, including dimensions and mouse position in game coordinates.</param>
    /// <remarks>
    /// This method is called every frame when the mouse is on screen and can be overridden in derived classes 
    /// to implement custom cursor rendering in the game world. If not overridden, no cursor will be drawn in the game world.
    /// </remarks>
    protected virtual void DrawCursorGame(ScreenInfo gameInfo)
    {
        
    }
    /// <summary>
    /// Draws the cursor on the game UI. This method is called during the UI rendering phase when the mouse is on the screen.
    /// </summary>
    /// <param name="gameUiInfo">Information about the current game UI screen.</param>
    /// <remarks>
    /// This is a virtual method intended to be overridden by derived classes to implement custom cursor rendering.
    /// The base implementation does not draw anything.
    /// </remarks>

    protected virtual void DrawCursorGameUi(ScreenInfo gameUiInfo)
    {
        
    }
    /// <summary>
    /// Draws cursor UI elements on the screen.
    /// </summary>
    /// <param name="uiInfo">Screen information for UI rendering.</param>
    /// <remarks>
    /// This is a virtual method that can be overridden by derived classes to implement custom cursor drawing.
    /// It's called during the rendering process when the mouse is detected on the screen.
    /// </remarks>

    protected virtual void DrawCursorUi(ScreenInfo uiInfo)
    {
        
    }

    #endregion
    
    #region Public
    
    /// <summary>
    /// Restarts the game by setting both restart and quit flags.
    /// </summary>
    public void Restart()
    {
        restart = true;
        quit = true;
    }
    
    /// <summary>
    /// Quits the game by setting the quit flag while ensuring restart is not triggered.
    /// </summary>
    public void Quit()
    {
        restart = false;
        quit = true;
    }

    /// <summary>
    /// Switches to the new scene. Deactivates the current scene and activates the new scene.
    /// </summary>
    /// <param name="newScene">The new scene to switch to.</param>
    public void GoToScene(Scene newScene)
    {
        if (newScene == CurScene) return;
        
        CurScene.ResolveDeactivate();
        CurScene.SetGameReference(null);
        
        newScene.SetGameReference(this);
        newScene.ResolveActivate(CurScene);
       
        CurScene = newScene;
    }

    /// <summary>
    /// Schedules an action to be executed after a specified number of frames.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <param name="afterFrames">The number of frames to wait before executing the action.
    /// The default is 0 (next frame).</param>
    public void CallDeferred(Action action, int afterFrames = 0)
    {
        deferred.Add(new(action, afterFrames));
    }
    
    /// <summary>
    /// Processes all deferred actions, executing those whose wait time has elapsed.
    /// </summary>
    private void ResolveDeferred()
    {
        for (int i = deferred.Count - 1; i >= 0; i--)
        {
            var info = deferred[i];
            if (info.Call()) deferred.RemoveAt(i);
        }
    }

    /// <summary>
    /// Creates a screen flash effect that transitions from start color to end color over the specified duration.
    /// </summary>
    /// <param name="duration">The duration of the flash effect in seconds.</param>
    /// <param name="startColorRgba">The starting color of the flash.</param>
    /// <param name="endColorRgba">The ending color of the flash.</param>
    public void Flash(float duration, ColorRgba startColorRgba, ColorRgba endColorRgba)
    {
        if (duration <= 0.0f) return;
        if (ScreenEffectIntensity <= 0f) return;
        startColorRgba = startColorRgba.SetAlpha((byte)(startColorRgba.A * ScreenEffectIntensity));
        endColorRgba = endColorRgba.SetAlpha((byte)(endColorRgba.A * ScreenEffectIntensity));
        // byte startColorAlpha = (byte)(startColor.A * ScreenEffectIntensity);
        // startColor.A = startColorAlpha;
        // byte endColorAlpha = (byte)(endColor.A * ScreenEffectIntensity);
        // endColor.A = endColorAlpha;

        ShapeFlash flash = new(duration, startColorRgba, endColorRgba);
        shapeFlashes.Add(flash);
    }

    /// <summary>
    /// Removes all active flash effects.
    /// </summary>
    public void ClearFlashes() => shapeFlashes.Clear();

    /// <summary>
    /// Resets the current camera to the basic default camera.
    /// </summary>
    public void ResetCamera() => Camera = basicCamera;

    /// <summary>
    /// Changes the game's rendering texture to a new one.
    /// </summary>
    /// <param name="newScreenTexture">The new screen texture to use for game rendering.</param>
    /// <returns>Returns the old game texture or null if the newScreenTexture is the same as the game texture.
    /// The old ScreenTexture should be unloaded and disposed of if no longer needed!</returns>
    public ScreenTexture? ChangeGameTexture(ScreenTexture newScreenTexture)
    {
        if (gameTexture == newScreenTexture) return null;

        gameTexture.OnTextureResized -= GameTextureOnTextureResized;
        gameTexture.OnDrawGame -= GameTextureOnDrawGame;
        gameTexture.OnDrawUI -= GameTextureOnDrawUI;
        
        newScreenTexture.OnTextureResized += GameTextureOnTextureResized;
        if(!newScreenTexture.Initialized) newScreenTexture.Initialize(Window.CurScreenSize, Window.MousePosition, curCamera);
        newScreenTexture.OnDrawGame += GameTextureOnDrawGame;
        newScreenTexture.OnDrawUI += GameTextureOnDrawUI;
        
        var old = gameTexture;
        gameTexture = newScreenTexture;
        
        return old;
    }
    
    /// <summary>
    /// Starts the game loop and runs the game until it is terminated.
    /// </summary>
    /// <param name="launchParameters">Command-line arguments or parameters to configure the game at launch.</param>
    /// <returns>An ExitCode object indicating whether the game should restart.</returns>
    public ExitCode Run(params string[] launchParameters)
    {
        this.LaunchParams = launchParameters;

        quit = false;
        restart = false;
        Raylib.SetExitKey(KeyboardKey.Null);

        StartGameloop();
        RunGameloop();
        EndGameloop();
        Raylib.CloseWindow();

        return new ExitCode(restart);
    }

    #endregion
    
    #region Gamepad Connection
    private void ResolveOnGamepadConnectionChanged(ShapeGamepadDevice gamepad, bool connected)
    {
        if (connected)
        {
            OnGamepadConnected(gamepad);
            CurScene.ResolveOnGamepadConnected(gamepad);
        }
        else
        {
            OnGamepadDisconnected(gamepad);
            CurScene.ResolveOnGamepadDisconnected(gamepad);
        }
    }
    private void ResolveOnInputDeviceChanged(InputDeviceType prevDeviceType, InputDeviceType newDeviceType)
    {
        OnInputDeviceChanged(prevDeviceType, newDeviceType);
        CurScene.ResolveOnInputDeviceChanged(prevDeviceType, newDeviceType);
    }
    #endregion
}




