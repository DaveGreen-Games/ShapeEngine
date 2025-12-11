using System.Diagnostics;
using Raylib_cs;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Screen;

namespace ShapeEngine.Core.GameDef;

public partial class Game
{
    private List<ScreenTexture>? customScreenTexturesDrawBefore;
    private List<ScreenTexture>? customScreenTexturesDrawAfter;

    private double frameDelta;
    private int fps;
    private long frameDeltaNanoSeconds;
    
    /// <summary>
    /// Gets the current frames per second (FPS) calculated from the latest frame delta.
    /// </summary>
    /// <value>Frames per second as an integer. Updated each frame.</value>
    public int FramesPerSecond => fps;
    
    /// <summary>
    /// Gets the elapsed time in seconds for the most recent frame.
    /// </summary>
    /// <value>Frame delta time in seconds. Updated each frame and used for time-based updates.</value>
    public double FrameDelta => frameDelta;
    
    /// <summary>
    /// Gets the elapsed time in nanoseconds for the most recent frame.
    /// </summary>
    /// <value>Frame delta time in nanoseconds. Updated each frame and useful for high-resolution timing and profiling.</value>
    public long FrameDeltaNanoSeconds => frameDeltaNanoSeconds;
    
    
    private void StartGameloop()
    {
        Input.Keyboard.OnButtonPressed += KeyboardButtonPressed;
        Input.Keyboard.OnButtonReleased += KeyboardButtonReleased;
        Input.Mouse.OnButtonPressed += MouseButtonPressed;
        Input.Mouse.OnButtonReleased += MouseButtonReleased;
        Input.GamepadManager.OnGamepadButtonPressed += GamepadButtonPressed;
        Input.GamepadManager.OnGamepadButtonReleased += GamepadButtonReleased;

        LoadContent();
        BeginRun();
    }

    private void RunGameloop()
    {
        Stopwatch frameWatch = new();
        long frequency = Stopwatch.Frequency;
        const long  nanoSecPerSecond = 1000L * 1000L * 1000L;
        const long nanoSecPerMilliSec = 1000L * 1000L;
        long nanosecPerTick = nanoSecPerSecond / frequency;
        
        while (!quit)
        {
            frameDeltaNanoSeconds = frameWatch.ElapsedTicks * nanosecPerTick;
            frameDelta = frameDeltaNanoSeconds / (double)nanoSecPerSecond;
            fps = (int)Math.Ceiling(1.0 / frameDelta);
            frameWatch.Restart();
            int targetFps = Window.TargetFps;
            
            if (Raylib.WindowShouldClose())
            {
                Quit();
                continue;
            }

            var dt = (float)frameDelta;
            Time = Time.TickF(dt);

            Window.Update(dt);
            AudioDevice.Update(dt, curCamera);
            Input.Update(dt);

            if (Window.MouseOnScreen)
            {
                if (Input.CurrentInputDeviceType is InputDeviceType.Keyboard or InputDeviceType.Gamepad)
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
                ResolveUpdate(true);
                AdvanceFixedUpdate(dt);
            }
            else ResolveUpdate(false);

            DrawToScreen();

            ResolveDeferred();

            Input.EndFrame();


            if (targetFps > 0)
            {
                long elapsedNanoSec = frameWatch.ElapsedTicks * nanosecPerTick;
                long totalFrameTimeNanoSec = nanoSecPerSecond / targetFps;
                long remainingNanoSec = totalFrameTimeNanoSec - elapsedNanoSec;
                
                long msToWait = remainingNanoSec / nanoSecPerMilliSec;
                if (msToWait > 1)
                {
                    Thread.Sleep((int)(msToWait - 1));
                }
                elapsedNanoSec = frameWatch.ElapsedTicks * nanosecPerTick;
                remainingNanoSec = totalFrameTimeNanoSec - elapsedNanoSec;
                
                while (remainingNanoSec > 0)
                {
                    // if (remainingNanoSec > 1_000_000) Thread.SpinWait(100);
                    // else if(remainingNanoSec > 750_000)Thread.SpinWait(50);
                    // else if(remainingNanoSec > 250_000)Thread.SpinWait(10);
                    // else Thread.SpinWait(1);
                    
                    // if (remainingNanoSec > 1_000_000) Thread.SpinWait(100); //more than 1 ms
                    // else if(remainingNanoSec > 500_000)Thread.SpinWait(50); //more than 0.5 ms
                    // else if(remainingNanoSec > 250_000)Thread.SpinWait(25); //more than 0.25 ms
                    // else if(remainingNanoSec > 100_000)Thread.SpinWait(10); //more than 0.1 ms
                    // else Thread.SpinWait(1); //less than 0.1 ms
                    
                    Thread.SpinWait((int)(remainingNanoSec / 10_000L)); //approximate
                    elapsedNanoSec = frameWatch.ElapsedTicks * nanosecPerTick;
                    remainingNanoSec = totalFrameTimeNanoSec - elapsedNanoSec;
                }
            }
            
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

        //split custom screen textures into textures to draw to the screen before the game texture
        //and textures to draw to the screen after the game texture
        if (customScreenTextures is { Count: > 0 })
        {
            for (var i = 0; i < customScreenTextures.Count; i++)
            {
                //negative draw order means to draw it to screen before the game texture
                if (customScreenTextures[i].DrawToScreenOrder < 0)
                {
                    customScreenTexturesDrawBefore ??= []; //initialize if it has not been initialized yet
                    customScreenTexturesDrawBefore.Add(customScreenTextures[i]);
                }
                //otherwise it will be drawn to screen after the game texture
                else
                {
                    customScreenTexturesDrawAfter ??= []; //initialize if it has not been initialized yet
                    customScreenTexturesDrawAfter.Add(customScreenTextures[i]);
                }
            }
        }

        //draw screen textures to screen before the game texture
        if (customScreenTexturesDrawBefore is { Count: > 0 })
        {
            customScreenTexturesDrawBefore.Sort((a, b) =>
                {
                    if (a.DrawToScreenOrder < b.DrawToScreenOrder) return -1;
                    if (a.DrawToScreenOrder > b.DrawToScreenOrder) return 1;
                    return 0;
                }
            );
            for (var i = 0; i < customScreenTexturesDrawBefore.Count; i++)
            {
                customScreenTexturesDrawBefore[i].DrawToScreen();
            }
        }

        //draw game texture to screen
        gameTexture.DrawToScreen();

        //draw screen textures to screen after the game texture
        if (customScreenTexturesDrawAfter is { Count: > 0 })
        {
            customScreenTexturesDrawAfter.Sort((a, b) =>
                {
                    if (a.DrawToScreenOrder < b.DrawToScreenOrder) return -1;
                    if (a.DrawToScreenOrder > b.DrawToScreenOrder) return 1;
                    return 0;
                }
            );
            for (var i = 0; i < customScreenTexturesDrawAfter.Count; i++)
            {
                customScreenTexturesDrawAfter[i].DrawToScreen();
            }
        }
        
        customScreenTexturesDrawBefore?.Clear();
        customScreenTexturesDrawAfter?.Clear();

        ResolveDrawUI(UIScreenInfo);

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