using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Screen;

namespace ShapeEngine.Core;

public partial class Game
{
    private void UpdateFlashes(float dt)
    {
        for (int i = shapeFlashes.Count() - 1; i >= 0; i--)
        {
            var flash = shapeFlashes[i];
            flash.Update(dt);
            if (flash.IsFinished())
            {
                shapeFlashes.RemoveAt(i);
            }
        }
    }

    private void OnGamepadButtonReleased(ShapeGamepadDevice gamepad, ShapeGamepadButton button) => ResolveOnButtonReleased(new(gamepad, button));
    private void OnGamepadButtonPressed(ShapeGamepadDevice gamepad, ShapeGamepadButton button) => ResolveOnButtonPressed(new(gamepad, button));
    private void OnMouseButtonReleased(ShapeMouseButton button) => ResolveOnButtonReleased(new(button));
    private void OnMouseButtonPressed(ShapeMouseButton button) => ResolveOnButtonPressed(new(button));
    private void OnKeyboardButtonReleased(ShapeKeyboardButton button) => ResolveOnButtonReleased(new(button));
    private void OnKeyboardButtonPressed(ShapeKeyboardButton button) => ResolveOnButtonPressed(new(button));

    private void ResolveOnButtonPressed(InputEvent e)
    {
        OnButtonPressed(e);
        CurScene.ResolveOnButtonPressed(e);
    }

    private void ResolveOnButtonReleased(InputEvent e)
    {
        OnButtonReleased(e);
        CurScene.ResolveOnButtonReleased(e);
    }

    private void ResolveUpdate()
    {
        TriggerIntervalEventsUpdate(true, Time.Delta);
        Update(Time, GameScreenInfo, GameUiScreenInfo, UIScreenInfo);
        CurScene.ResolveUpdate(Time, GameScreenInfo, GameUiScreenInfo, UIScreenInfo);
        TriggerIntervalEventsUpdate(false, Time.Delta);
    }

    private void ResolvePreFixedUpdate()
    {
        PreFixedUpdate(Time, GameScreenInfo, GameUiScreenInfo, UIScreenInfo);
        CurScene.ResolvePreFixedUpdate(Time, GameScreenInfo, GameUiScreenInfo, UIScreenInfo);
    }

    private void ResolveFixedUpdate()
    {
        FixedUpdate(FixedTime, GameScreenInfo, GameUiScreenInfo, UIScreenInfo);
        CurScene.ResolveFixedUpdate(FixedTime, GameScreenInfo, GameUiScreenInfo, UIScreenInfo);
    }

    private void ResolveInterpolateFixedUpdate(float f)
    {
        InterpolateFixedUpdate(Time, GameScreenInfo, GameUiScreenInfo, UIScreenInfo, f);
        CurScene.ResolveInterpolateFixedUpdate(Time, GameScreenInfo, GameUiScreenInfo, UIScreenInfo, f);
    }

    private void ResolveOnGameTextureResized(int w, int h)
    {
        OnGameTextureResized(w, h);
        CurScene.ResolveGameTextureResized(w, h);
    }

    private void ResolveDrawGame(ScreenInfo game)
    {
        TriggerIntervalEventsDrawGame(true, game);
        DrawGame(game);
        CurScene.ResolveDrawGame(game);
        TriggerIntervalEventsDrawGame(false, game);
    }

    private void ResolveDrawGameUI(ScreenInfo gameUi)
    {
        TriggerIntervalEventsDrawGameUi(true, gameUi);
        ;
        DrawGameUI(gameUi);
        CurScene.ResolveDrawGameUI(gameUi);
        TriggerIntervalEventsDrawGameUi(false, gameUi);
        ;
    }

    private void ResolveDrawUI(ScreenInfo ui)
    {
        TriggerIntervalEventsDrawUi(true, ui);
        DrawUI(ui);
        CurScene.ResolveDrawUI(ui);
        TriggerIntervalEventsDrawUi(false, ui);
    }

    private void ResolveOnWindowSizeChanged(DimensionConversionFactors conversion)
    {
        OnWindowSizeChanged(conversion);
        CurScene.ResolveOnWindowSizeChanged(conversion);
    }

    private void ResolveOnWindowPositionChanged(Vector2 oldPos, Vector2 newPos)
    {
        //Console.WriteLine($"Window Pos: {Raylib.GetWindowPosition()}");
        OnWindowPositionChanged(oldPos, newPos);
        CurScene.ResolveOnWindowPositionChanged(oldPos, newPos);
    }

    private void ResolveOnMonitorChanged(MonitorInfo newMonitor)
    {
        OnMonitorChanged(newMonitor);
        CurScene.ResolveOnMonitorChanged(newMonitor);
    }

    private void ResolveOnPausedChanged(bool newPaused)
    {
        OnPausedChanged(newPaused);
        CurScene.ResolveOnPausedChanged(newPaused);
    }

    private void ResolveOnMouseEnteredScreen()
    {
        OnMouseEnteredScreen();
        CurScene.ResolveOnMouseEnteredScreen();
    }

    private void ResolveOnMouseLeftScreen()
    {
        OnMouseLeftScreen();
        CurScene.ResolveOnMouseLeftScreen();
    }

    private void ResolveOnMouseVisibilityChanged(bool visible)
    {
        OnMouseVisibilityChanged(visible);
        CurScene.ResolveOnMouseVisibilityChanged(visible);
    }

    private void ResolveOnMouseEnabledChanged(bool enabled)
    {
        OnMouseEnabledChanged(enabled);
        CurScene.ResolveOnMouseEnabledChanged(enabled);
    }

    private void ResolveOnWindowFocusChanged(bool focused)
    {
        OnWindowFocusChanged(focused);
        CurScene.ResolveOnWindowFocusChanged(focused);
    }

    private void ResolveOnWindowFullscreenChanged(bool fullscreen)
    {
        OnWindowFullscreenChanged(fullscreen);
        CurScene.ResolveOnWindowFullscreenChanged(fullscreen);
    }

    private void ResolveOnWindowMaximizeChanged(bool maximized)
    {
        OnWindowMaximizeChanged(maximized);
        CurScene.ResolveOnWindowMaximizeChanged(maximized);
    }

    private void ResolveOnWindowMinimizedChanged(bool minimized)
    {
        OnWindowMinimizedChanged(minimized);
        CurScene.ResolveOnWindowMinimizedChanged(minimized);
    }

    private void ResolveOnWindowHiddenChanged(bool hidden)
    {
        OnWindowHiddenChanged(hidden);
        CurScene.ResolveOnWindowHiddenChanged(hidden);
    }

    private void ResolveOnWindowTopmostChanged(bool topmost)
    {
        OnWindowTopmostChanged(topmost);
        CurScene.ResolveOnWindowTopmostChanged(topmost);
    }

}