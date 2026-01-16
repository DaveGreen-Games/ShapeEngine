using System.Numerics;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Input;
using ShapeEngine.Pathfinding;
using ShapeEngine.Screen;

namespace ShapeEngine.Core;

/// <summary>
/// A scene is the core object for your game logic.
/// A scene can be a level, a main menu, a pause screen, etc.
/// Only one scene can be active at a time.
/// Use "Game.GoToScene()" method to switch scenes.
/// Use "Game.CurScene" property to get the current active scene.
/// </summary>
public abstract class Scene
{

    #region Members
    
    /// <summary>
    /// Is true when the scene is the current active scene of the game class.
    /// If "Game.CurScene" equals this scene, active will be true, otherwise active will be false.
    /// Use "Game.GoToScene()" method to switch scenes.
    /// </summary>
    public bool Active { get; private set; }
    /// <summary>
    /// Is set to the current game instance.
    /// </summary>
    public Game Parent => Game.Instance;
    
    /// <summary>
    /// Provides access to the <see cref="InputSystem"/> instance from the parent <see cref="Game"/>.
    /// </summary>
    public InputSystem Input => Parent.Input;
    
    /// <summary>
    /// The current SpawnArea of this scene.
    /// SpawnArea handles game objects.
    /// SpawnArea is null per default.
    /// Call "InitSpawnArea()" to set SpawnArea.
    /// </summary>
    public SpawnArea? SpawnArea { get; private set; }
    /// <summary>
    /// The current CollisionHandler of this scene.
    /// CollisionHandler handles collision objects and reports overlaps/intersections..
    /// CollisionHandler is null per default.
    /// Call "InitCollisionHandler()" to set CollisionHandler.
    /// </summary>
    public CollisionHandler? CollisionHandler { get; private set; }
    /// <summary>
    /// The current Pathfinder of this scene.
    /// Pathfinder handles pathfinding on a 2d grid.
    /// Pathfinder is null per default.
    /// Call "InitPathfinder()" to set Pathfinder.
    /// </summary>
    public Pathfinder? Pathfinder { get; private set; }

    #endregion

    #region Init Handlers
    /// <summary>
    /// Initializes a new SpawnArea.
    /// Only creates a new SpawnArea if one  doesn't exist.
    /// </summary>
    /// <param name="bounds">The bounds of the area.</param>
    /// <returns>Returns if a new SpawnArea was created.</returns>
    protected bool InitSpawnArea(Rect bounds)
    {
        if (SpawnArea != null) return false;

        SpawnArea = new(bounds);
            
        return true;
    }
    
    /// <summary>
    /// Initializes a new SpawnArea.
    /// Only creates a new SpawnArea if one  doesn't exist.
    /// </summary>
    /// <param name="x">The x coordinate of the top left corner of the bounds rect.</param>
    /// <param name="y">The y coordinate of the top left corner of the bounds rect.</param>
    /// <param name="w">The width of the bounds.</param>
    /// <param name="h">The height of the bounds.</param>
    /// <returns>Returns if a new SpawnArea was created.</returns>
    protected bool InitSpawnArea(float x, float y, float w, float h)
    {
        if (SpawnArea != null) return false;

        SpawnArea = new(x, y, w, h);
            
        return true;
    }
    
    /// <summary>
    /// Initializes a new SpawnArea.
    /// Only creates a new SpawnArea if one  doesn't exist.
    /// </summary>
    /// <param name="area">An already created spawn area.</param>
    /// <returns>Returns if a new SpawnArea was created.</returns>
    protected bool InitSpawnArea(SpawnArea area)
    {
        if (SpawnArea != null) return false;
        if (SpawnArea == area) return false;

        SpawnArea = area;
            
        return true;
    }
    private bool RemoveSpawnArea()
    {
        if (SpawnArea == null) return false;

        SpawnArea.Close();
        SpawnArea = null;
            
        return true;
    }
    
    /// <summary>
    /// Initializes a new <see cref="CollisionHandler"/> for this scene using the specified broadphase algorithm and starting capacity.
    /// Only creates a new <see cref="CollisionHandler"/> if one does not already exist.
    /// </summary>
    /// <param name="broadphase">The broadphase algorithm to use for collision detection.</param>
    /// <param name="startingCapacity">The initial capacity for the collision handler. Default is 1024.</param>
    /// <returns>Returns true if a new <see cref="CollisionHandler"/> was created; false if one already exists.</returns>
    protected bool InitCollisionHandler(IBroadphase broadphase, int startingCapacity = 1024)
    {
        if (CollisionHandler != null) return false;
        CollisionHandler = new(broadphase, startingCapacity);
        
        return true;
    }
    /// <summary>
    /// Initializes a new CollisionHandler with the provided CollisionHandler instance.
    /// </summary>
    /// <param name="collisionHandler">The CollisionHandler instance to be used for this scene.</param>
    /// <returns>
    /// Returns true if a new CollisionHandler was successfully assigned to this scene.
    /// Returns false if a CollisionHandler already exists, in which case no new CollisionHandler is assigned.
    /// </returns>
    protected bool InitCollisionHandler(CollisionHandler collisionHandler)
    {
        if (CollisionHandler != null) return false;
        if (CollisionHandler == collisionHandler) return false;
        CollisionHandler = collisionHandler; // new(Bounds, rows, cols);
        return true;
    }
    private bool RemoveCollisionHandler()
    {
        if (CollisionHandler == null) return false;
        CollisionHandler.Close();
        CollisionHandler = null;
        return true;
    }
    /// <summary>
    /// Initializes a new Pathfinder with the specified grid dimensions.
    /// </summary>
    /// <param name="bounds">The bounds of the pathfinding grid.</param>
    /// <param name="rows">The number of rows in the grid.</param>
    /// <param name="cols">The number of columns in the grid.</param>
    /// <returns>
    /// Returns true if a new Pathfinder was created successfully.
    /// Returns false if a Pathfinder already exists, in which case no new Pathfinder is created.
    /// </returns>
    protected bool InitPathfinder(Rect bounds, int rows, int cols)
    {
        if (Pathfinder != null) return false;
        Pathfinder = new(bounds, rows, cols);
        return true;
    }
    /// <summary>
    /// Initializes a new Pathfinder with the provided Pathfinder instance.
    /// </summary>
    /// <param name="pathfinder">The Pathfinder instance to be used for this scene.</param>
    /// <returns>
    /// Returns true if a new Pathfinder was successfully assigned to this scene.
    /// Returns false if a Pathfinder already exists, in which case no new Pathfinder is assigned.
    /// </returns>
    protected bool InitPathfinder(Pathfinder pathfinder)
    {
        if (Pathfinder != null) return false;
        if (Pathfinder == pathfinder) return false;
        Pathfinder = pathfinder;
        return true;
    }
    private bool RemovePathfinder()
    {
        if (Pathfinder == null) return false;
        Pathfinder.Clear();
        Pathfinder = null;
        return true;
    }
    #endregion
    
    #region Internal

    // internal void SetGameReference(GameDef.Game? game) => Parent = game;

    internal void ResolveActivate(Scene oldScene)
    {
        Active = true;
        OnActivate(oldScene);
    }
    internal void ResolveDeactivate()
    {
        Active = false;
        OnDeactivate();
    }

    /// <summary>
    /// Used for cleanup. Should be called once right before the scene gets deleted.
    /// </summary>
    public void Close()
    {
        RemoveSpawnArea();
        RemoveCollisionHandler();
        RemovePathfinder();
        OnClose();
    }
    internal void ResolveUpdate(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        if (time.FixedMode)
        {
            SpawnArea?.Update(time, game, gameUi, ui);
        }
        else
        {
            SpawnArea?.Update(time, game, gameUi, ui, false);
            CollisionHandler?.Update(time.Delta);
            Pathfinder?.Update(time.Delta);
        }
        OnUpdate(time, game, gameUi, ui);
    }
    internal void ResolveFixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        SpawnArea?.FixedUpdate(fixedTime, game, gameUi, ui);
        CollisionHandler?.Update(fixedTime.Delta);
        Pathfinder?.Update(fixedTime.Delta);
        OnFixedUpdate(fixedTime, game, gameUi, ui);
    }
    internal void ResolveInterpolateFixedUpdate(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui, float f)
    {
        SpawnArea?.InterpolateFixedUpdate(time, game, gameUi, ui, f);
        OnInterpolateFixedUpdate(time, game, gameUi, ui, f);
    }
    internal void ResolveGameTextureResized(int w, int h)
    {
        OnGameTextureResized(w, h);
    }

    internal void ResolveDrawGame(ScreenInfo game)
    {
        OnPreDrawGame(game);
        SpawnArea?.DrawGame(game);
        OnDrawGame(game);
    }
    internal void ResolveDrawGameUI(ScreenInfo gameUi)
    {
        OnPreDrawGameUI(gameUi);
        SpawnArea?.DrawGameUI(gameUi);
        OnDrawGameUI(gameUi);
    }
    internal void ResolveDrawUI(ScreenInfo ui)
    {
        OnDrawUI(ui);
    }
    internal void ResolveOnWindowSizeChanged(DimensionConversionFactors conversionFactors)
    {
        OnWindowSizeChanged(conversionFactors);
    }
    internal void ResolveOnWindowPositionChanged(Vector2 oldPos, Vector2 newPos)
    {
        OnWindowPositionChanged(oldPos, newPos);
    }
    internal void ResolveOnMonitorChanged(MonitorInfo newMonitor)
    {
        OnMonitorChanged(newMonitor);   
    }
    internal void ResolveOnGamepadConnected(GamepadDevice gamepad)
    {
        OnGamepadConnected(gamepad);
    }
    internal void ResolveOnGamepadDisconnected(GamepadDevice gamepad)
    {
        OnGamepadDisconnected(gamepad);
    }
    internal void ResolveOnGamepadClaimed(GamepadDevice gamepad)
    {
        OnGamepadClaimed(gamepad);
    }
    internal void ResolveOnGamepadFreed(GamepadDevice gamepad)
    {
        OnGamepadFreed(gamepad);
    }
    internal void ResolveOnInputDeviceChanged(InputDeviceType prevDeviceType, InputDeviceType curDeviceType)
    {
        OnInputDeviceChanged(prevDeviceType, curDeviceType);
    }
    internal void ResolveOnPausedChanged(bool newPaused)
    {
        OnPausedChanged(newPaused);
    }
    internal void ResolveOnMouseEnteredScreen()
    {
        OnMouseEnteredScreen();
    }
    internal void ResolveOnMouseLeftScreen()
    {
        OnMouseLeftScreen();
    }
    internal void ResolveOnMouseVisibilityChanged(bool visible)
    {
        OnMouseVisibilityChanged(visible);
    }
    internal void ResolveOnMouseEnabledChanged(bool enabled)
    {
        OnMouseEnabledChanged(enabled);
    }
    internal void ResolveOnWindowFocusChanged(bool focused)
    {
        OnWindowFocusChanged(focused);
    }
    internal void ResolveOnWindowFullscreenChanged(bool fullscreen)
    {
        OnWindowFullscreenChanged(fullscreen);
    }
    internal void ResolveOnWindowMaximizeChanged(bool maximized)
    {
        OnWindowMaximizeChanged(maximized);
    }

    internal void ResolveOnWindowMinimizedChanged(bool minimized)
    {
        OnWindowMinimizedChanged(minimized);
    }

    internal void ResolveOnWindowHiddenChanged(bool hidden)
    {
        OnWindowHiddenChanged(hidden);
    }

    internal void ResolveOnWindowTopmostChanged(bool topmost)
    {
        OnWindowTopmostChanged(topmost);
    }
    /// <summary>
    /// Resolves the OnButtonPressed event by calling the protected virtual OnButtonPressed method.
    /// The game class calls this if necessary.
    /// You can call this function, but it is not recommended.
    /// </summary>
    /// <param name="e">The InputEvent object containing information about the button press event.</param>
    public void ResolveOnButtonPressed(InputEvent e)
    {
        OnButtonPressed(e);
    }
    /// <summary>
    /// Resolves the OnButtonReleased event by calling the protected virtual OnButtonReleased method.
    /// The game class calls this if necessary.
    /// You can call this function, but it is not recommended.
    /// </summary>
    /// <param name="e">The InputEvent object containing information about the button release event.</param>
    public void ResolveOnButtonReleased(InputEvent e)
    {
        OnButtonReleased(e);
    }
    #endregion
    
    #region Protected Virtual
    
    /// <summary>
    /// Called when the scene becomes active.
    /// </summary>
    /// <param name="oldScene">The scene that was previously active.</param>
    protected abstract void OnActivate(Scene oldScene);
    
    /// <summary>
    /// Called when the scene becomes inactive.
    /// </summary>
    protected abstract void OnDeactivate();

    /// <summary>
    /// Used for cleanup. Should be called once right before the scene gets deleted.
    /// </summary>
    protected virtual void OnClose()
    {
        
    }
    
    /// <summary>
    /// Called when the game texture is resized.
    /// </summary>
    /// <param name="w">The new width of the game texture.</param>
    /// <param name="h">The new height of the game texture.</param>
    protected virtual void OnGameTextureResized(int w, int h) { }
    
    /// <summary>
    /// Called every frame. Called before FixedUpdate if fixed framerate is enabled.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="game"></param>
    /// <param name="gameUi"></param>
    /// <param name="ui"></param>
    protected virtual void OnUpdate(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui) { }
    
    
    /// <summary>
    /// Only called when fixed framerate is enabled. Called in a fixed interval.
    /// </summary>
    /// <param name="fixedTime"></param>
    /// <param name="game"></param>
    /// <param name="gameUi"></param>
    /// <param name="ui"></param>
    protected virtual void OnFixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui) { }
    
    /// <summary>
    /// Only called when fixed framerate is enabled. Called every frame after all fixed update calls.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="game"></param>
    /// <param name="gameUi"></param>
    /// <param name="ui"></param>
    /// <param name="f"></param>
    protected virtual void OnInterpolateFixedUpdate(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui, float f) { }
    /// <summary>
    /// Called before SpawnArea DrawGame is called.
    /// </summary>
    /// <param name="game"></param>
    protected virtual void OnPreDrawGame(ScreenInfo game) { }
    /// <summary>
    /// Draw your game. Is affected by screen shaders and the camera.
    /// </summary>
    /// <param name="game"></param>
    protected virtual void OnDrawGame(ScreenInfo game) { }
        
    /// <summary>
    /// Called before SpawnArea DrawGameUI is called
    /// </summary>
    /// <param name="ui"></param>
    protected virtual void OnPreDrawGameUI(ScreenInfo ui) { }
    /// <summary>
    /// Draw your game ui. Is not affected by screen shaders and not by the camera but uses the same ScreenTextureMode as DrawGame.
    /// </summary>
    /// <param name="gameUi"></param>
    protected virtual void OnDrawGameUI(ScreenInfo gameUi) { }
        
    /// <summary>
    /// Draw your main ui. Is NOT affected by screen shaders and NOT affected by the camera.
    /// </summary>
    protected virtual void OnDrawUI(ScreenInfo ui) { }
    /// <summary>
    /// Called when the window size is changed.
    /// </summary>
    /// <param name="conversionFactors">The conversion factors for the new window size.</param>
    protected virtual void OnWindowSizeChanged(DimensionConversionFactors conversionFactors) { }

    /// <summary>
    /// Called when the window position is changed.
    /// </summary>
    /// <param name="oldPos">The old position of the window.</param>
    /// <param name="newPos">The new position of the window.</param>
    protected virtual void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos) { }

    /// <summary>
    /// Called when the monitor the window is on is changed.
    /// </summary>
    /// <param name="newMonitor">The new monitor the window is on.</param>
    protected virtual void OnMonitorChanged(MonitorInfo newMonitor) { }

    /// <summary>
    /// Called when a gamepad is connected.
    /// </summary>
    /// <param name="gamepad">The gamepad that was connected.</param>
    protected virtual void OnGamepadConnected(GamepadDevice gamepad) { }

    /// <summary>
    /// Called when a gamepad is disconnected.
    /// </summary>
    /// <param name="gamepad">The gamepad that was disconnected.</param>
    protected virtual void OnGamepadDisconnected(GamepadDevice gamepad) { }
    /// <summary>
    /// Called when a gamepad is claimed by the scene (e.g., assigned to a player or reserved for use).
    /// </summary>
    /// <param name="gamepad">The gamepad that was claimed.</param>
    protected virtual void OnGamepadClaimed(GamepadDevice gamepad) { }

    /// <summary>
    /// Called when a gamepad is freed by the scene (e.g., unassigned from a player or released for general use).
    /// </summary>
    /// <param name="gamepad">The gamepad that was freed.</param>
    protected virtual void OnGamepadFreed(GamepadDevice gamepad) { }

    /// <summary>
    /// Called when the input device type is changed.
    /// </summary>
    /// <param name="prevDeviceType">The previous input device type.</param>
    /// <param name="curDeviceType">The current input device type.</param>
    protected virtual void OnInputDeviceChanged(InputDeviceType prevDeviceType, InputDeviceType curDeviceType) { }

    /// <summary>
    /// Called when the game is paused or unpaused.
    /// </summary>
    /// <param name="newPaused">True if the game is paused, false otherwise.</param>
    protected virtual void OnPausedChanged(bool newPaused) { }
    /// <summary>
    /// Called when the mouse cursor enters the screen area.
    /// </summary>
    protected virtual void OnMouseEnteredScreen() { }

    /// <summary>
    /// Called when the mouse cursor leaves the screen area.
    /// </summary>
    protected virtual void OnMouseLeftScreen() { }

    /// <summary>
    /// Called when the mouse cursor visibility changes.
    /// </summary>
    /// <param name="visible">True if the mouse cursor is visible, false otherwise.</param>
    protected virtual void OnMouseVisibilityChanged(bool visible) { }

    /// <summary>
    /// Called when the mouse cursor input is enabled or disabled.
    /// </summary>
    /// <param name="enabled">True if the mouse cursor input is enabled, false otherwise.</param>
    protected virtual void OnMouseEnabledChanged(bool enabled) { }

    /// <summary>
    /// Called when the window gains or loses focus.
    /// </summary>
    /// <param name="focused">True if the window has focus, false otherwise.</param>
    protected virtual void OnWindowFocusChanged(bool focused) { }

    /// <summary>
    /// Called when the window changes between fullscreen and windowed mode.
    /// </summary>
    /// <param name="fullscreen">True if the window is in fullscreen mode, false otherwise.</param>
    protected virtual void OnWindowFullscreenChanged(bool fullscreen) { }
    /// <summary>
    /// Called when the window changes between maximized and non-maximized mode.
    /// </summary>
    /// <param name="maximized">True if the window is maximized, false otherwise.</param>
    protected virtual void OnWindowMaximizeChanged(bool maximized) { }

    /// <summary>
    /// Called when the window changes between minimized and non-minimized mode.
    /// </summary>
    /// <param name="minimized">True if the window is minimized, false otherwise.</param>
    protected virtual void OnWindowMinimizedChanged(bool minimized) { }

    /// <summary>
    /// Called when the window changes between hidden and non-hidden mode.
    /// </summary>
    /// <param name="hidden">True if the window is hidden, false otherwise.</param>
    protected virtual void OnWindowHiddenChanged(bool hidden) { }

    /// <summary>
    /// Called when the window changes between topmost and non-topmost mode.
    /// </summary>
    /// <param name="topmost">True if the window is topmost, false otherwise.</param>
    protected virtual void OnWindowTopmostChanged(bool topmost) { }

    /// <summary>
    /// Called when a button is pressed.
    /// </summary>
    /// <param name="e">The InputEvent object containing information about the button press event.</param>
    protected virtual void OnButtonPressed(InputEvent e) { }

    /// <summary>
    /// Called when a button is released.
    /// </summary>
    /// <param name="e">The InputEvent object containing information about the button release event.</param>
    protected virtual void OnButtonReleased(InputEvent e) { }
    
    #endregion
    
}