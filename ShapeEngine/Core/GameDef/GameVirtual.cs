using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Input;
using ShapeEngine.Screen;

namespace ShapeEngine.Core.GameDef;

public partial class Game
{
    /// <summary>
    /// Called first after starting the gameloop.
    /// </summary>
    protected virtual void LoadContent()
    {
    }

    /// <summary>
    /// Called after LoadContent but before the main loop has started.
    /// </summary>
    protected virtual void BeginRun()
    {
    }

    /// <summary>
    /// Updates game state when the fixed framerate is disabled.
    /// This is the standard update method
    /// called every frame at variable intervals.
    /// </summary>
    /// <param name="time">Contains timing information for the current frame.</param>
    /// <param name="game">Screen information for the main game area.</param>
    /// <param name="gameUi">Screen information for the game's UI elements.</param>
    /// <param name="ui">Screen information for the global UI.</param>
    protected virtual void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui) { }

    /// <summary>
    /// Updates the game at a fixed time interval when fixed framerate is enabled. This method
    /// ensures consistent physics and game logic calculations independent of frame rate.
    /// </summary>
    /// <param name="fixedTime">Contains timing information for the fixed update cycle.</param>
    /// <param name="game">Screen information for the main game area.</param>
    /// <param name="gameUi">Screen information for the game's UI elements.</param>
    /// <param name="ui">Screen information for the global UI.</param>
    protected virtual void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
    }

    /// <summary>
    /// Interpolates between fixed updates when fixed framerate is enabled. Called every frame
    /// to provide smooth rendering between physics/logic steps.
    /// </summary>
    /// <param name="time">Contains timing information for the current frame.</param>
    /// <param name="game">Screen information for the main game area.</param>
    /// <param name="gameUi">Screen information for the game's UI elements.</param>
    /// <param name="ui">Screen information for the global UI.</param>
    /// <param name="f">Interpolation factor (0.0 to 1.0) between the current and next fixed update.</param>
    protected virtual void InterpolateFixedUpdate(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui, float f)
    {
    }

    /// <summary>
    /// Renders the main game content to the specified screen.
    /// </summary>
    /// <param name="game">The screen information for rendering the game content.</param>
    protected virtual void DrawGame(ScreenInfo game)
    {
    }

    /// <summary>
    /// Renders the game user interface elements to the specified screen.
    /// </summary>
    /// <param name="gameUi">The screen information for rendering the game UI elements.</param>
    protected virtual void DrawGameUI(ScreenInfo gameUi)
    {
    }

    /// <summary>
    /// Renders the general user interface elements to the specified screen.
    /// </summary>
    /// <param name="ui">The screen information for rendering the UI elements.</param>
    protected virtual void DrawUI(ScreenInfo ui)
    {
    }

    /// <summary>
    /// Called before UnloadContent is called after the main gameloop has been exited.
    /// </summary>
    protected virtual void EndRun()
    {
    }

    /// <summary>
    /// Called after EndRun before the application terminates.
    /// </summary>
    protected virtual void UnloadContent()
    {
    }

    /// <summary>
    /// Called when the game texture is resized.
    /// </summary>
    /// <param name="w">The new width of the game texture.</param>
    /// <param name="h">The new height of the game texture.</param>
    protected virtual void OnGameTextureResized(int w, int h)
    {
    }

    /// <summary>
    /// Called when the window size changes.
    /// </summary>
    /// <param name="conversion">The dimension conversion factors between window and game coordinates.</param>
    protected virtual void OnWindowSizeChanged(DimensionConversionFactors conversion)
    {
    }

    /// <summary>
    /// Called when the window position changes.
    /// </summary>
    /// <param name="oldPos">The previous window position.</param>
    /// <param name="newPos">The new window position.</param>
    protected virtual void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos)
    {
    }

    /// <summary>
    /// Called when the game window moves to a different monitor.
    /// </summary>
    /// <param name="newMonitor">Information about the new monitor.</param>
    protected virtual void OnMonitorChanged(MonitorInfo newMonitor)
    {
    }

    /// <summary>
    /// Called when the game's paused state changes.
    /// </summary>
    /// <param name="newPaused">The new paused state.</param>
    protected virtual void OnPausedChanged(bool newPaused)
    {
    }

    /// <summary>
    /// Called when the active input device type changes.
    /// </summary>
    /// <param name="prevDeviceType">The previous input device type.</param>
    /// <param name="newDeviceType">The new input device type.</param>
    protected virtual void OnInputDeviceChanged(InputDeviceType prevDeviceType, InputDeviceType newDeviceType)
    {
    }

    /// <summary>
    /// Called when a gamepad is connected to the system.
    /// </summary>
    /// <param name="gamepad">The gamepad device that was connected.</param>
    protected virtual void OnGamepadConnected(GamepadDevice gamepad)
    {
    }

    /// <summary>
    /// Called when a gamepad is disconnected from the system.
    /// </summary>
    /// <param name="gamepad">The gamepad device that was disconnected.</param>
    protected virtual void OnGamepadDisconnected(GamepadDevice gamepad)
    {
    }

    /// <summary>
    /// Called when the mouse cursor enters the game window.
    /// </summary>
    protected virtual void OnMouseEnteredScreen()
    {
    }

    /// <summary>
    /// Called when the mouse cursor leaves the game window.
    /// </summary>
    protected virtual void OnMouseLeftScreen()
    {
    }

    /// <summary>
    /// Called when the mouse cursor visibility changes.
    /// </summary>
    /// <param name="visible">Whether the mouse cursor is now visible.</param>
    protected virtual void OnMouseVisibilityChanged(bool visible)
    {
    }

    /// <summary>
    /// Called when the mouse input enabled state changes.
    /// </summary>
    /// <param name="enabled">Whether mouse input is now enabled.</param>
    protected virtual void OnMouseEnabledChanged(bool enabled)
    {
    }

    /// <summary>
    /// Called when the window focus state changes.
    /// </summary>
    /// <param name="focused">Whether the window is now focused.</param>
    protected virtual void OnWindowFocusChanged(bool focused)
    {
    }

    /// <summary>
    /// Called when the window fullscreen state changes.
    /// </summary>
    /// <param name="fullscreen">Whether the window is now in fullscreen mode.</param>
    protected virtual void OnWindowFullscreenChanged(bool fullscreen)
    {
    }

    /// <summary>
    /// Called when the window maximize state changes.
    /// </summary>
    /// <param name="maximized">Whether the window is now maximized.</param>
    protected virtual void OnWindowMaximizeChanged(bool maximized)
    {
    }

    /// <summary>
    /// Called when the window minimized state changes.
    /// </summary>
    /// <param name="minimized">Whether the window is now minimized.</param>
    protected virtual void OnWindowMinimizedChanged(bool minimized)
    {
    }

    /// <summary>
    /// Called when the window hidden state changes.
    /// </summary>
    /// <param name="hidden">Whether the window is now hidden.</param>
    protected virtual void OnWindowHiddenChanged(bool hidden)
    {
    }

    /// <summary>
    /// Called when the window topmost state changes.
    /// </summary>
    /// <param name="topmost">Whether the window is now in topmost (always on top) mode.</param>
    protected virtual void OnWindowTopmostChanged(bool topmost)
    {
    }

    /// <summary>
    /// Allows modification of the mouse position before it's used for input processing.
    /// </summary>
    /// <param name="dt">Delta time since the last frame.</param>
    /// <param name="mousePos">The current mouse position.</param>
    /// <param name="screenArea">The screen area rectangle.</param>
    /// <returns>The modified mouse position.</returns>
    protected virtual Vector2 ChangeMousePos(float dt, Vector2 mousePos, Rect screenArea) => mousePos;

    /// <summary>
    /// Called when an input button is pressed.
    /// </summary>
    /// <param name="e">The input event containing information about the button press.</param>
    protected virtual void OnButtonPressed(InputEvent e)
    {
    }

    /// <summary>
    /// Called when an input button is released.
    /// </summary>
    /// <param name="e">The input event containing information about the button release.</param>
    protected virtual void OnButtonReleased(InputEvent e)
    {
    }

}