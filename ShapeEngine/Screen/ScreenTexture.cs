using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Screen;

public sealed class ScreenTexture
{
    
    #region Events
    public delegate void DrawToRenderTexture(ScreenInfo screenInfo, ScreenTexture texture);
    public delegate void TextureResized(int w, int h);
    
    /// <summary>
    /// Draw to the render texture.
    /// Is called before shaders are applied.
    /// </summary>
    public event DrawToRenderTexture? OnDrawGame;
    
    /// <summary>
    /// Draw to the render texture
    /// Is called after shaders are applied
    /// </summary>
    public event DrawToRenderTexture? OnDrawUI;

    public event TextureResized? OnTextureResized;
    #endregion

    #region Members

    #region Public

    /// <summary>
    /// This is set automatically if custom mode is used!
    /// </summary>
    public Func<(ColorRgba color, bool clear)>? CustomClearBackgroundFunction
    {
        get => customClearBackgroundFunction;
        set
        {
            if (Mode == ScreenTextureMode.Custom) return;
            customClearBackgroundFunction = value;
        } 
    }
    private Func<(ColorRgba color, bool clear)>? customClearBackgroundFunction = null;
    
    public ColorRgba BackgroundColor = ColorRgba.Clear;
    public ScreenInfo GameScreenInfo { get; private set; } = new();
    public ScreenInfo GameUiScreenInfo { get; private set; } = new();
    
    /// <summary>
    /// The order in which screen textures are drawn to the screen each frame. Lower numbers will be draw first.
    /// Negative draw orders will be drawn to screen before the game texture.
    /// Positive draw orders will be drawn to screen after the game texture (this includes 0).
    /// If the draw order is the same the order in which the screen textures were added is taken into account.
    /// </summary>
    public int DrawToScreenOrder { get; set; } = 0;
    
    /// <summary>
    /// Disable/Enable draw to texture. If disable OnDrawGame & OnDrawGameUi event will not fire.
    /// </summary>
    public bool DrawToTextureEnabled { get; set; } = true;
    public bool Loaded { get; private set; } = false;
    public int Width { get; private set; } = 0;
    public int Height { get; private set; } = 0;
    public ShapeCamera? Camera = null;
    public ColorRgba Tint = new ColorRgba(255, 255, 255, 255);
    
    public readonly ScreenTextureMode Mode;
    public readonly Dimensions FixedDimensions;
    public float PixelationFactor { get; private set; }
    public Vector2 AnchorStretch { get; private set; }
    public Vector2 AnchorPosition { get; private set; }
    
    public readonly TextureFilter TextureFilter;
    public readonly ShaderSupportType ShaderSupport;
    public readonly ShaderContainer? Shaders = null;
    public bool Initialized { get; private set; } = false;
    #endregion
    
    #region Private
    private RenderTexture2D renderTexture = new();
    private RenderTexture2D shaderBuffer = new();
    private Rect textureRect = new();
    private Dimensions screenDimensions = new();
    private float nearestFixedFactor = 1f;
    private bool textureReloadRequired = false;
    private CustomScreenTextureHandler? customScreenTextureHandler = null;
    private float macOSHighDpiScaleFactor = 1f;
    #endregion
    
    #endregion

    #region Constructors
    
    /// <summary>
    /// Create a screen texture in stretch mode.
    /// </summary>
    /// <param name="shaderSupportType"></param>
    /// <param name="textureFilter"></param>
    public ScreenTexture(ShaderSupportType shaderSupportType, TextureFilter textureFilter = TextureFilter.Bilinear)
    {
        TextureFilter = textureFilter;
        ShaderSupport = shaderSupportType;
        
        if (shaderSupportType != ShaderSupportType.None)
        {
            Shaders = new();
        }
        
        FixedDimensions = new Dimensions(-1, -1);
        PixelationFactor = 1f;
        
        Mode = ScreenTextureMode.Stretch;
        AnchorPosition = new(-1, -1);
        AnchorStretch = new(-1, -1);
    }
    
    /// <summary>
    /// Create a screen texture in anchor mode.
    /// </summary>
    /// <param name="anchorStretch"> The factors for the size of the resulting screen texture. 0.5/0.5 would result
    /// in a screen texture with half the width and half the height of the screen.</param>
    /// <param name="anchorPosition"> The factors for the position of the screen texture on the screen.
    /// 0/0 is the topleft corner, 1/1 is the bottom right corner.</param>
    /// <param name="shaderSupportType"></param>
    /// <param name="textureFilter"></param>
    public ScreenTexture(Vector2 anchorStretch, Vector2 anchorPosition, ShaderSupportType shaderSupportType, TextureFilter textureFilter = TextureFilter.Bilinear)
    {
        TextureFilter = textureFilter;
        ShaderSupport = shaderSupportType;
        
        if (shaderSupportType != ShaderSupportType.None)
        {
            Shaders = new();
        }

        if (anchorStretch.X <= 0f || anchorStretch.Y <= 0f || anchorStretch.X >= 1f || anchorStretch.Y >= 1f ||
            anchorPosition.X < 0f || anchorPosition.Y < 0f || anchorPosition.X > 1f || anchorPosition.Y > 1f)
        {
            Mode = ScreenTextureMode.Stretch;
            anchorPosition = new(-1, -1);
            anchorStretch = new(-1, -1);
        }
        else
        {
            Mode = ScreenTextureMode.Anchor;
            AnchorPosition = anchorPosition;
            AnchorStretch = anchorStretch;
        }
        
        FixedDimensions = new Dimensions(-1, -1);
        PixelationFactor = 1f;
    }
    
    /// <summary>
    /// Create a screen texture in pixelation mode.
    /// </summary>
    /// <param name="pixelationFactor">The pixelation factor has to be bigger than 0 and smaller than 1 otherwise
    /// the screen texture will be create in stretch mode!</param>
    /// <param name="shaderSupportType"></param>
    /// <param name="textureFilter"></param>
    public ScreenTexture(float pixelationFactor, ShaderSupportType shaderSupportType, TextureFilter textureFilter = TextureFilter.Bilinear)
    {
        TextureFilter = textureFilter;
        ShaderSupport = shaderSupportType;
        if (shaderSupportType != ShaderSupportType.None)
        {
            Shaders = new();
        }
        
        FixedDimensions = new Dimensions(-1, -1);
        PixelationFactor = pixelationFactor;
        if (pixelationFactor is <= 0f or > 1f)
        {
            PixelationFactor = 1f;
            Mode = ScreenTextureMode.Stretch;
        }
        else Mode = ScreenTextureMode.Pixelation;
        
        AnchorPosition = new(-1, -1);
        AnchorStretch = new(-1, -1);
    }
    
    /// <summary>
    /// Create a screen texture in fixed mode or nearest fixed mode.
    /// </summary>
    /// <param name="fixedDimensions">The fixed dimensions to use for the screen texture. Dimensions have to be valid.</param>
    /// <param name="shaderSupportType"></param>
    /// <param name="textureFilter"></param>
    /// <param name="nearest">If true nearest fixed mode is used.</param>
    public ScreenTexture(Dimensions fixedDimensions, ShaderSupportType shaderSupportType, TextureFilter textureFilter = TextureFilter.Bilinear, bool nearest = false)
    {
        TextureFilter = textureFilter;
        ShaderSupport = shaderSupportType;
        if (shaderSupportType != ShaderSupportType.None)
        {
            Shaders = new();
        }
        
        FixedDimensions = fixedDimensions;
        PixelationFactor = 1f;
        if (!fixedDimensions.IsValid())
        {
            FixedDimensions = new(-1, -1);
            Mode = ScreenTextureMode.Stretch;
        }
        else
        {
            if (!nearest)
            {
                Loaded = true;
                Mode = ScreenTextureMode.Fixed;
                Width = FixedDimensions.Width;
                Height = FixedDimensions.Height;

                renderTexture = Raylib.LoadRenderTexture(Width, Height);
                Raylib.SetTextureFilter(renderTexture.Texture, TextureFilter);

                if (shaderSupportType != ShaderSupportType.None)
                {
                    shaderBuffer = Raylib.LoadRenderTexture(Width, Height);
                    Raylib.SetTextureFilter(shaderBuffer.Texture, TextureFilter);
                }
            }
            else Mode = ScreenTextureMode.NearestFixed;

        }
        
        AnchorPosition = new(-1, -1);
        AnchorStretch = new(-1, -1);
    }
    
    /// <summary>
    /// Create a screen texture in custom mode.
    /// </summary>
    /// <param name="customDimensions">The width and height of the screen texture. Has to be positive otherwise screen texture will be
    /// created in stretch mode. Can be changed with ChangeCustomTextureSize function later.</param>
    /// <param name="handler"></param>
    /// <param name="shaderSupportType"></param>
    /// <param name="textureFilter"></param>
    public ScreenTexture(Dimensions customDimensions, CustomScreenTextureHandler handler, ShaderSupportType shaderSupportType, TextureFilter textureFilter = TextureFilter.Bilinear)
    {
        TextureFilter = textureFilter;
        ShaderSupport = shaderSupportType;
        if (shaderSupportType != ShaderSupportType.None)
        {
            Shaders = new();
        }

        if (customDimensions.Width <= 0 || customDimensions.Height <= 0)
        {
            Mode = ScreenTextureMode.Stretch;
        }
        else
        {
            Loaded = true;
            customClearBackgroundFunction = handler.GetBackgroundClearColor;
            customScreenTextureHandler = handler;
            Mode = ScreenTextureMode.Custom;
            Width = customDimensions.Width;
            Height = customDimensions.Height;

            renderTexture = Raylib.LoadRenderTexture(Width, Height);
            Raylib.SetTextureFilter(renderTexture.Texture, TextureFilter);

            if (shaderSupportType != ShaderSupportType.None)
            {
                shaderBuffer = Raylib.LoadRenderTexture(Width, Height);
                Raylib.SetTextureFilter(shaderBuffer.Texture, TextureFilter);
            }
        }
        
        FixedDimensions = Dimensions.GetInvalidDimension();
        PixelationFactor = 1f;
        AnchorPosition = new(-1, -1);
        AnchorStretch = new(-1, -1);
    }

    #endregion

    #region Public Functions
    public void Initialize(Dimensions screenSize, Vector2 mousePosition, ShapeCamera? camera = null)
    {
        if (Initialized) return;
        Initialized = true;
        
        SetMacOsScalingFactor();
        
        if(camera != null) Camera = camera;
        
        screenDimensions = screenSize;
        UpdateTexture(screenSize);
        
        textureRect = GetTextureRect();
        Camera?.SetSize(new Dimensions(Width, Height));

        Vector2 scaledMousePostionGame;
        var scaledMousePositionUi = mousePosition * macOSHighDpiScaleFactor;
        if (Mode == ScreenTextureMode.Pixelation)
        {
            var f = GameWindow.Instance.ScreenToMonitor.AreaSideFactor / macOSHighDpiScaleFactor;
            scaledMousePositionUi = mousePosition * PixelationFactor * f * macOSHighDpiScaleFactor;
        }
        else if (Mode == ScreenTextureMode.NearestFixed)
        {
            scaledMousePositionUi = mousePosition / nearestFixedFactor;
        }
        else if (Mode == ScreenTextureMode.Anchor)
        {
            var w = screenDimensions.Width * AnchorStretch.X;
            var h = screenDimensions.Height * AnchorStretch.Y;

            var size = new Vector2(w, h);
            var topLeft = screenDimensions.ToVector2() * AnchorPosition - size * AnchorPosition;
            scaledMousePositionUi = mousePosition * macOSHighDpiScaleFactor - topLeft * macOSHighDpiScaleFactor;
        }
        else if (Mode == ScreenTextureMode.Fixed)
        {
            float virtualRatioW = (float)screenSize.Width/ Width;
            float virtualRatioH = (float)screenSize.Height/ Height;
            if (virtualRatioW < virtualRatioH)
            {
                var h = Height * virtualRatioW;
                var topLeft = new Vector2(0f, screenDimensions.Height / 2f - h / 2f);
                scaledMousePositionUi = (mousePosition - topLeft) / virtualRatioW;
            }
            else
            {
                var w = Width * virtualRatioH;
                var topLeft = new Vector2(screenDimensions.Width / 2f - w / 2f, 0f);
                scaledMousePositionUi = (mousePosition - topLeft) / virtualRatioH;
            }
        }
        else if (Mode == ScreenTextureMode.Custom)
        {
            if (customScreenTextureHandler != null)
            {
                var textureDimensions = new Dimensions(Width, Height);
                scaledMousePositionUi = customScreenTextureHandler.GetScaledMousePosition(mousePosition, screenDimensions, textureDimensions);
                textureRect = customScreenTextureHandler.GetTextureRect(screenDimensions, textureDimensions);
            }
        }
        if (Camera != null)
        {
            scaledMousePostionGame = Camera.ScreenToWorld(scaledMousePositionUi);
        }
        else
        {
            scaledMousePostionGame = scaledMousePositionUi;
        }
        
        GameScreenInfo = new(Camera?.Area ?? textureRect, scaledMousePostionGame);
        GameUiScreenInfo = new(textureRect, scaledMousePositionUi);
    }

    
    public void Update(float dt, Dimensions screenSize, Vector2 mousePosition, bool paused)
    {
        SetMacOsScalingFactor();
        
        if (screenDimensions != screenSize || textureReloadRequired)
        {
            textureReloadRequired = false;
            screenDimensions = screenSize;
            UpdateTexture(screenSize);
        }
        
        textureRect = GetTextureRect();
        if (Camera != null)
        {
            Camera.SetSize(new Dimensions(Width, Height));
            if(!paused) Camera.Update(dt);
        }

        Vector2 scaledMousePostionGame;
        var scaledMousePositionUi = mousePosition * macOSHighDpiScaleFactor;
        if (Mode == ScreenTextureMode.Pixelation)
        {
            var f = GameWindow.Instance.ScreenToMonitor.AreaSideFactor / macOSHighDpiScaleFactor;
            scaledMousePositionUi = mousePosition * PixelationFactor * f * macOSHighDpiScaleFactor;
        }
        else if (Mode == ScreenTextureMode.NearestFixed)
        {
            // scaledMousePositionUi = (mousePosition  * macOSHighDpiScaleFactor) / nearestFixedFactor;
            scaledMousePositionUi = mousePosition / nearestFixedFactor;
        }
        else if (Mode == ScreenTextureMode.Anchor)
        {
            var w = screenDimensions.Width * AnchorStretch.X;
            var h = screenDimensions.Height * AnchorStretch.Y;

            var size = new Vector2(w, h);
            var topleft = screenDimensions.ToVector2() * AnchorPosition - size * AnchorPosition;
            scaledMousePositionUi = mousePosition * macOSHighDpiScaleFactor - topleft * macOSHighDpiScaleFactor;
        }
        else if (Mode == ScreenTextureMode.Fixed)
        {
            float virtualRatioW = (float)screenSize.Width/ Width;
            float virtualRatioH = (float)screenSize.Height/ Height;
            if (virtualRatioW < virtualRatioH)
            {
                var h = Height * virtualRatioW;
                var topLeft = new Vector2(0f, screenDimensions.Height / 2f - h / 2f);
                scaledMousePositionUi = (mousePosition - topLeft) / virtualRatioW;
            }
            else
            {
                var w = Width * virtualRatioH;
                var topLeft = new Vector2(screenDimensions.Width / 2f - w / 2f, 0f);
                scaledMousePositionUi = (mousePosition - topLeft) / virtualRatioH;
            }
        }
        else if (Mode == ScreenTextureMode.Custom)
        {
            if (customScreenTextureHandler != null)
            {
                var textureDimensions = new Dimensions(Width, Height);
                scaledMousePositionUi = customScreenTextureHandler.GetScaledMousePosition(mousePosition, screenDimensions, textureDimensions);
                textureRect = customScreenTextureHandler.GetTextureRect(screenDimensions, textureDimensions);
            }
        }
        if (Camera != null)
        {
            scaledMousePostionGame = Camera.ScreenToWorld(scaledMousePositionUi);
        }
        else
        {
            scaledMousePostionGame = scaledMousePositionUi;
        }
        
        GameScreenInfo = new(Camera?.Area ?? textureRect, scaledMousePostionGame);
        GameUiScreenInfo = new(textureRect, scaledMousePositionUi);
    }
    
    /// <summary>
    /// Will invoke events to draw to the texture via OnDrawGame & OnDrawGameUi.
    /// </summary>
    public void DrawOnTexture()
    {
        if (!DrawToTextureEnabled) return;
        
        bool shaderMode = ShaderSupport != ShaderSupportType.None && Shaders != null && Shaders.HasActiveShaders();

        if (shaderMode)
        {
            Raylib.BeginTextureMode(renderTexture);
            if(customClearBackgroundFunction == null) Raylib.ClearBackground(BackgroundColor.ToRayColor());
            else
            {
                var result = customClearBackgroundFunction();
                if(result.clear) Raylib.ClearBackground(result.color.ToRayColor());
            }

            if (Camera != null)
            {
                Raylib.BeginMode2D(Camera.Camera);
                OnDrawGame?.Invoke(GameScreenInfo, this);
                Raylib.EndMode2D();
                
                // OnDrawGameUI?.Invoke(ScreenInfo);
            }
            else
            {
                OnDrawGame?.Invoke(GameScreenInfo, this);
                // OnDrawGameUI?.Invoke(ScreenInfo);
            }
            
            Raylib.EndTextureMode();
            
            ApplyShaders(); // we know that we have shaders active
            
            Raylib.BeginTextureMode(renderTexture);
            OnDrawUI?.Invoke(GameUiScreenInfo, this);
            Raylib.EndTextureMode();
            
            //DrawToScreen();
        }
        else
        {
            Raylib.BeginTextureMode(renderTexture);
            if(customClearBackgroundFunction == null) Raylib.ClearBackground(BackgroundColor.ToRayColor());
            else
            {
                var result = customClearBackgroundFunction();
                if(result.clear) Raylib.ClearBackground(result.color.ToRayColor());
            }

            if (Camera != null)
            {
                Raylib.BeginMode2D(Camera.Camera);
                OnDrawGame?.Invoke(GameScreenInfo, this);
                Raylib.EndMode2D();
            }
            else OnDrawGame?.Invoke(GameScreenInfo, this);
            
            // OnDrawGameUI?.Invoke(ScreenInfo);
            OnDrawUI?.Invoke(GameUiScreenInfo, this);
            
            Raylib.EndTextureMode();
            //DrawToScreen();
            
        }
    }
    /// <summary>
    /// Is called by the game class to draw the texture to the screen. Should not be used otherwise.
    /// </summary>
    public void DrawToScreen()
    {
        DrawTextureToScreen(renderTexture.Texture);
    }
    public void Unload()
    {
        if (!Loaded) return;
        Loaded = false;
        Raylib.UnloadRenderTexture(renderTexture);
        if(ShaderSupport != ShaderSupportType.None) Raylib.UnloadRenderTexture(shaderBuffer);
    }
    public Rect GetDestinationRect()
    {
        if( Mode == ScreenTextureMode.Stretch ) return new(0, 0, screenDimensions.Width, screenDimensions.Height);
        if( Mode == ScreenTextureMode.Pixelation ) return new(0, 0, screenDimensions.Width, screenDimensions.Height);
        if( Mode == ScreenTextureMode.NearestFixed ) return new(0, 0, screenDimensions.Width, screenDimensions.Height);
        if( Mode == ScreenTextureMode.Anchor )
        {
            var w = screenDimensions.Width * AnchorStretch.X;
            var h = screenDimensions.Height * AnchorStretch.Y;

            var size = new Vector2(w, h);
            var topLeft = screenDimensions.ToVector2() * AnchorPosition - size * AnchorPosition;
            return new Rect(topLeft.X, topLeft.Y, w, h);
        }
        float virtualRatioW = (float)screenDimensions.Width/ Width;
        float virtualRatioH = (float)screenDimensions.Height/ Height;
        Rect destRec;
        var originX = 0f;
        var originY = 0f;
        if (virtualRatioW < virtualRatioH)
        {
            var w = Width * virtualRatioW;
            var h = Height * virtualRatioW;
            originY = screenDimensions.Height / 2f - h / 2f;
            destRec = new Rect(originX, originY, w, h);
        }
        else
        {
            var w = Width * virtualRatioH;
            var h = Height * virtualRatioH;
            originX = screenDimensions.Width / 2f - w / 2f;
            destRec = new Rect(originX, originY, w, h);
        }

        return destRec;
    }
    public Rect GetTextureRect() => new(0, 0, Width, Height);

    public bool ChangeAnchorPosition(Vector2 newAnchorPosition)
    {
        if(Mode != ScreenTextureMode.Anchor) return false;
        if (newAnchorPosition.X < 0f || newAnchorPosition.Y < 0f || 
            newAnchorPosition.X > 1f || newAnchorPosition.Y > 1f ) return false;
        
        AnchorPosition = newAnchorPosition;
        
        return true;

    }

    /// <summary>
    /// Requires a reload of the texture!
    /// Reloads happens next frame!
    /// </summary>
    /// <param name="newAnchorStretch">Value Range: Bigger than 0, 0 and smaller than 1, 1. </param>
    /// <returns></returns>
    public bool ChangeAnchorStretch(Vector2 newAnchorStretch)
    {
        if(Mode != ScreenTextureMode.Anchor) return false;
        
        if (newAnchorStretch.X <= 0f || newAnchorStretch.Y <= 0f || 
            newAnchorStretch.X >= 1f || newAnchorStretch.Y >= 1f ) return false;

        textureReloadRequired = true;
        AnchorStretch = newAnchorStretch;
        
        return true;
    }

    /// <summary>
    /// Requires a reload of the texture!
    /// Reloads happens next frame!
    /// </summary>
    /// <param name="newPixelationFactor"> Value Range: Bigger than 0 and smaller than 1!</param>
    /// <returns></returns>
    public bool ChangePixelationFactor(float newPixelationFactor)
    {
        if(Mode != ScreenTextureMode.Pixelation) return false;
        if(newPixelationFactor <= 0f || newPixelationFactor >= 1f ) return false;
        
        textureReloadRequired = true;
        PixelationFactor = newPixelationFactor;
        
        return true;
    }
    #endregion

    #region Custom Texture Mode Functions

    /// <summary>
    /// Change the dimensions of the screen texture. This functions only works in custom mode!
    /// </summary>
    /// <param name="newDimensions">The new dimensions to use. Only works if newDimensions are different
    /// than the current dimensions and if the newDimensions are valid!.</param>
    public void ChangeCustomTextureSize(Dimensions newDimensions)
    {
        if (Mode != ScreenTextureMode.Custom) return;
        if (!newDimensions.IsValid()) return;
        if (newDimensions.Width == Width && newDimensions.Height == Height) return;
        
        ReloadTexture(newDimensions.Width, newDimensions.Height);
    }

    
    #endregion
    
    #region Private Functions
    private void SetMacOsScalingFactor()
    {
        if (Game.IsOSX()) //for fixing blurry screen texture on high dpi moitors on macOS
        {
            bool noScaleFactor = Mode == ScreenTextureMode.Fixed || Mode == ScreenTextureMode.Custom || Mode == ScreenTextureMode.NearestFixed;
           
            if (noScaleFactor || Raylib.GetWindowScaleDPI().X < 2 || GameWindow.Instance.IsWindowFullscreen())
            {
                macOSHighDpiScaleFactor = 1f;
            }
            else macOSHighDpiScaleFactor = 2f;
        }
    }
    private float GetNearestFixedFactor()
    {
        var xF = screenDimensions.Width / (float)FixedDimensions.Width;
        var yF = screenDimensions.Height / (float)FixedDimensions.Height;
        return (xF + yF) / 2f;
    }
    private void ApplyShaders()
    {
        if (ShaderSupport == ShaderSupportType.None) return;
        if (Shaders == null) return;
        
        var activeScreenShaders = Shaders.GetActiveShaders();
        if (activeScreenShaders.Count <= 0) return;

        if (activeScreenShaders.Count == 1 || ShaderSupport == ShaderSupportType.Single)
        {
            var singleShader = activeScreenShaders[0];
            Raylib.BeginTextureMode(shaderBuffer);
            Raylib.ClearBackground(new(0,0,0,0));
            Raylib.BeginShaderMode(singleShader.Shader);
            ApplyShaderTexture(renderTexture.Texture);//draws texture to target
            Raylib.EndShaderMode();
            Raylib.EndTextureMode();
            (renderTexture, shaderBuffer) = (shaderBuffer, renderTexture);
            return;
        }
        
        var source = renderTexture;
        var target = shaderBuffer;

        foreach (var shader in activeScreenShaders)
        {
            Raylib.BeginTextureMode(target);
            Raylib.ClearBackground(new(0,0,0,0));
            Raylib.BeginShaderMode(shader.Shader);
            ApplyShaderTexture(source.Texture);//draws texture to target
            Raylib.EndShaderMode();
            Raylib.EndTextureMode();
            (source, target) = (target, source);
        }

        renderTexture = source;
        shaderBuffer = target;
    }
    private void ApplyShaderTexture(Texture2D texture)
    {
        var destRec = new Rectangle
        {
            X = 0, //Width * 0.5f,
            Y = 0, //Height * 0.5f,
            Width = Width,
            Height = Height
        };
        Vector2 origin = new()
        {
            X = 0, //Width * 0.5f,
            Y = 0 //Height * 0.5f
        };
        
        var sourceRec = new Rectangle(0, 0, Width, -Height);
        
        Raylib.DrawTexturePro(texture, sourceRec, destRec, origin, 0f, new ColorRgba(System.Drawing.Color.White).ToRayColor());
    }
    private void UpdateTexture(Dimensions screenSize)
    {
        if (Mode == ScreenTextureMode.Fixed) return;
        
        if (Mode == ScreenTextureMode.Custom)
        {
            if (customScreenTextureHandler != null)
            {
                var newTextureDimensions = customScreenTextureHandler.OnScreenDimensionsChanged(screenSize);
                if (newTextureDimensions.IsValid() && (newTextureDimensions.Width != Width || newTextureDimensions.Height != Height))
                {
                    ReloadTexture(newTextureDimensions.Width, newTextureDimensions.Height);
                }
            }

            return;
        }
        
        if(macOSHighDpiScaleFactor > 1) screenSize *= macOSHighDpiScaleFactor;
        
        int w = screenSize.Width;
        int h = screenSize.Height;
        
        if (Mode == ScreenTextureMode.Pixelation)
        {
            //screen to monitor factor is also wrong on macOS so we divide by it here 
            var f = GameWindow.Instance.ScreenToMonitor.AreaSideFactor / macOSHighDpiScaleFactor;
            
            w = (int)(w * PixelationFactor * f);
            h = (int)(h * PixelationFactor * f);
        }
        else if (Mode == ScreenTextureMode.NearestFixed)
        {
            nearestFixedFactor = GetNearestFixedFactor();
            w = (int)(w / nearestFixedFactor);
            h = (int)(w / screenSize.RatioW);
        }
        else if (Mode == ScreenTextureMode.Anchor)
        {
            w = (int) (w * AnchorStretch.X);
            h = (int) (h * AnchorStretch.Y);
        }
        ReloadTexture(w, h);
    }
    private void ReloadTexture(int w, int h)
    {
        if (!Loaded) Loaded = true;
        
        Width = w;
        Height = h;
        
        if (ShaderSupport == ShaderSupportType.None)
        {
            Raylib.UnloadRenderTexture(renderTexture);
                
            renderTexture = Raylib.LoadRenderTexture(Width, Height);
            Raylib.SetTextureFilter(renderTexture.Texture, TextureFilter);
        }
        else
        {
            Raylib.UnloadRenderTexture(renderTexture);
            Raylib.UnloadRenderTexture(shaderBuffer);
            
            renderTexture = Raylib.LoadRenderTexture(Width, Height);
            Raylib.SetTextureFilter(renderTexture.Texture, TextureFilter);
                
            shaderBuffer = Raylib.LoadRenderTexture(Width, Height);
            Raylib.SetTextureFilter(shaderBuffer.Texture, TextureFilter);
        }
        
        OnTextureResized?.Invoke(w, h);
    }
    private void DrawTextureToScreen(Texture2D texture)
    {
        if (Mode == ScreenTextureMode.Custom)
        {
            if (customScreenTextureHandler != null)
            {
                var h = customScreenTextureHandler;
                var sd = screenDimensions;
                var td = new Dimensions(Width, Height);
                Raylib.DrawTexturePro(
                    texture, 
                    h.GetSourceRect(sd, td).Rectangle, 
                    h.GetDestinationRect(sd, td).Rectangle, 
                    h.GetOrigin(sd, td), 
                    h.GetRotation(), 
                    Tint.ToRayColor());
            }
            
            return;
        }
        var sourceRec = new Rectangle(0, 0, Width, -Height);
        var destRect = GetDestinationRect().Rectangle;
        var origin = new Vector2(0, 0);
        Raylib.DrawTexturePro(texture, sourceRec, destRect, origin, 0f, Tint.ToRayColor());
    }
   
    #endregion
    
}

/*
  /// <summary>
      /// Used to draw to custom screen textures. Should only be used once or once a frame. Will apply shaders if shader mode is enabled
      /// and screen shaders are active. Will only run on custom screen textures.
      /// </summary>
      /// <param name="backgroundColor"></param>
      /// <param name="clearBackground"></param>
      /// <param name="drawGameFunction"></param>
      /// <param name="drawGameUiFunction"></param>
      public void DrawOnCustomTexture(ColorRgba backgroundColor, bool clearBackground, Action<ScreenInfo> drawGameFunction, Action<ScreenInfo> drawGameUiFunction)
      {
          if (Mode != ScreenTextureMode.Custom) return;
          
          bool shaderMode = ShaderSupport != ShaderSupportType.None && Shaders != null && Shaders.HasActiveShaders();

          if (shaderMode)
          {
              Raylib.BeginTextureMode(renderTexture);
              if(clearBackground) Raylib.ClearBackground(backgroundColor.ToRayColor());

              if (Camera != null)
              {
                  Raylib.BeginMode2D(Camera.Camera);
                  drawGameFunction(GameScreenInfo);
                  Raylib.EndMode2D();
              }
              else drawGameFunction(GameScreenInfo);
              
              Raylib.EndTextureMode();
              
              ApplyShaders(); // we know that we have shaders active
              
              Raylib.BeginTextureMode(renderTexture);
              drawGameUiFunction(GameUiScreenInfo);
              Raylib.EndTextureMode();
              
          }
          else
          {
              Raylib.BeginTextureMode(renderTexture);
              if(clearBackground) Raylib.ClearBackground(backgroundColor.ToRayColor());

              if (Camera != null)
              {
                  Raylib.BeginMode2D(Camera.Camera);
                  drawGameFunction(GameScreenInfo);
                  Raylib.EndMode2D();
              }
              else drawGameFunction(GameScreenInfo);
              
              drawGameUiFunction(GameUiScreenInfo);
              
              Raylib.EndTextureMode();
          }
      }
      
 */