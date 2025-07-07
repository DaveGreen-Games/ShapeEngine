using Raylib_cs;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Screen;

namespace ShapeEngine.Core;

public partial class Game
{
    private void StartGameloop()
    {
        ShapeInput.KeyboardDevice.OnButtonPressed += OnKeyboardButtonPressed;
        ShapeInput.KeyboardDevice.OnButtonReleased += OnKeyboardButtonReleased;
        ShapeInput.MouseDevice.OnButtonPressed += OnMouseButtonPressed;
        ShapeInput.MouseDevice.OnButtonReleased += OnMouseButtonReleased;
        ShapeInput.GamepadDeviceManager.OnGamepadButtonPressed += OnGamepadButtonPressed;
        ShapeInput.GamepadDeviceManager.OnGamepadButtonReleased += OnGamepadButtonReleased;

        LoadContent();
        BeginRun();
    }

    private void RunGameloop()
    {
        while (!quit)
        {
            if (Raylib.WindowShouldClose())
            {
                Quit();
                continue;
            }

            var dt = Raylib.GetFrameTime();
            Time = Time.TickF(dt);

            Window.Update(dt);
            AudioDevice.Update(dt, curCamera);
            ShapeInput.Update();

            if (Window.MouseOnScreen)
            {
                if (ShapeInput.CurrentInputDeviceType is InputDeviceType.Keyboard or InputDeviceType.Gamepad)
                {
                    Window.MoveMouse(ChangeMousePos(dt, Window.MousePosition, Window.ScreenArea));
                }
            }

            var mousePosUI = Window.MousePosition;
            gameTexture.Update(dt, Window.CurScreenSize, mousePosUI, Paused);

            if (customScreenTextures is { Count: > 0 })
            {
                for (var i = 0; i < customScreenTextures.Count; i++)
                {
                    customScreenTextures[i].Update(dt, Window.CurScreenSize, mousePosUI, Paused);
                }
            }

            GameScreenInfo = gameTexture.GameScreenInfo;
            GameUiScreenInfo = gameTexture.GameUiScreenInfo;
            UIScreenInfo = new(Window.ScreenArea, mousePosUI);

            if (!Paused)
            {
                UpdateFlashes(dt);
            }

            UpdateCursor(dt, GameScreenInfo, GameUiScreenInfo, UIScreenInfo);

            if (FixedPhysicsEnabled)
            {
                ResolvePreFixedUpdate();
                AdvanceFixedUpdate(dt);
            }
            else ResolveUpdate();

            DrawToScreen();

            ResolveDeferred();
        }
    }

    private void DrawToScreen()
    {
        gameTexture.DrawOnTexture();
        if (customScreenTextures is { Count: > 0 })
        {
            for (var i = 0; i < customScreenTextures.Count; i++)
            {
                customScreenTextures[i].DrawOnTexture();
            }
        }

        Raylib.BeginDrawing();
        Raylib.ClearBackground(BackgroundColorRgba.ToRayColor());

        if (DeferredDrawingBeforeGame.Count > 0)
        {
            foreach (var action in DeferredDrawingBeforeGame.Values)
            {
                action.Invoke();
            }
        }

        //split custom screen textures into textures to draw to the screen before the game texture
        //and textures to draw to the screen after the game texture
        List<ScreenTexture>? drawBefore = null;
        List<ScreenTexture>? drawAfter = null;
        if (customScreenTextures is { Count: > 0 })
        {
            for (var i = 0; i < customScreenTextures.Count; i++)
            {
                //negative draw order means to draw it to screen before the game texture
                if (customScreenTextures[i].DrawToScreenOrder < 0)
                {
                    drawBefore ??= new List<ScreenTexture>(); //initialize if it has not been initialized yet
                    drawBefore.Add(customScreenTextures[i]);
                }
                //otherwise it will be drawn to screen after the game texture
                else
                {
                    drawAfter ??= new List<ScreenTexture>(); //initialize if it has not been initialized yet
                    drawAfter.Add(customScreenTextures[i]);
                }
            }
        }

        //draw screen textures to screen before the game texture
        if (drawBefore is { Count: > 0 })
        {
            drawBefore.Sort((a, b) =>
                {
                    if (a.DrawToScreenOrder < b.DrawToScreenOrder) return -1;
                    if (a.DrawToScreenOrder > b.DrawToScreenOrder) return 1;
                    return 0;
                }
            );
            for (var i = 0; i < drawBefore.Count; i++)
            {
                drawBefore[i].DrawToScreen();
            }
        }

        //draw game texture to screen
        gameTexture.DrawToScreen();

        if (DeferredDrawingAfterGame.Count > 0)
        {
            foreach (var action in DeferredDrawingAfterGame.Values)
            {
                action.Invoke();
            }
        }

        //draw screen textures to screen after the game texture
        if (drawAfter is { Count: > 0 })
        {
            drawAfter.Sort((a, b) =>
                {
                    if (a.DrawToScreenOrder < b.DrawToScreenOrder) return -1;
                    if (a.DrawToScreenOrder > b.DrawToScreenOrder) return 1;
                    return 0;
                }
            );
            for (var i = 0; i < drawAfter.Count; i++)
            {
                drawAfter[i].DrawToScreen();
            }
        }

        ResolveDrawUI(UIScreenInfo);

        if (DeferredDrawingAfterUI.Count > 0)
        {
            foreach (var action in DeferredDrawingAfterUI.Values)
            {
                action.Invoke();
            }
        }

        if (Window.MouseOnScreen) DrawCursorUi(UIScreenInfo);

        Raylib.EndDrawing();
    }

    private void EndGameloop()
    {
        EndRun();
        UnloadContent();
        Window.Close();
        gameTexture.Unload();
    }

    private void GameTextureOnDrawGame(ScreenInfo gameScreenInfo, ScreenTexture texture)
    {
        ResolveDrawGame(gameScreenInfo);
        if (Window.MouseOnScreen) DrawCursorGame(gameScreenInfo);
    }

    private void GameTextureOnDrawUI(ScreenInfo gameUiScreenInfo, ScreenTexture texture)
    {
        ResolveDrawGameUI(gameUiScreenInfo);
        if (Window.MouseOnScreen) DrawCursorGameUi(gameUiScreenInfo);
    }

    private void GameTextureOnTextureResized(int w, int h)
    {
        ResolveOnGameTextureResized(w, h);
    }

    private void AdvanceFixedUpdate(float dt)
    {
        const float maxFrameTime = 1f / 30f;
        float frameTime = dt;
        // var t = 0.0f;

        if (frameTime > maxFrameTime) frameTime = maxFrameTime;

        physicsAccumulator += frameTime;
        while (physicsAccumulator >= FixedPhysicsTimestep)
        {
            FixedTime = FixedTime.TickF(FixedPhysicsFramerate);
            ResolveFixedUpdate();
            // t += FixedPhysicsTimestep;
            physicsAccumulator -= FixedPhysicsTimestep;
        }

        float alpha = physicsAccumulator / FixedPhysicsTimestep;
        ResolveInterpolateFixedUpdate(alpha);
    }

}