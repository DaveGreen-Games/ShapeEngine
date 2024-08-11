using System.Data;
using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Screen;

public enum ScreenTextureMode
{
    Stretch = 1,
    Pixelation = 2,
    Fixed = 4
}

public enum ShaderSupportType
{
    None = 1,
    Single = 2,
    Multi = 4
}

public sealed class ScreenTexture2
{
    
    #region Events
    public delegate void DrawToRenderTexture(ScreenInfo screenInfo);
    
    /// <summary>
    /// Draw to the render texture
    /// Is called before shaders are applied
    /// </summary>
    public event DrawToRenderTexture? OnDrawGame;
    
    /// <summary>
    /// Draw to the render texture
    /// Is called after shaders are applied
    /// </summary>
    public event DrawToRenderTexture? OnDrawUI;
    #endregion

    #region Members

    #region Public 
    
    public ScreenInfo GameScreenInfo { get; private set; } = new();
    public ScreenInfo GameUiScreenInfo { get; private set; } = new();
    public bool Loaded { get; private set; } = false;
    public int Width { get; private set; } = 0;
    public int Height { get; private set; } = 0;
    public ShapeCamera? Camera = null;
    public ColorRgba Tint = new ColorRgba(255, 255, 255, 255);
    
    public readonly ScreenTextureMode Mode;
    public readonly Dimensions FixedDimensions;
    public readonly float PixelationFactor;
    
    public readonly TextureFilter TextureFilter;
    public readonly ShaderSupportType ShaderSupport;
    public readonly ShaderContainer? Shaders = null;
    
    #endregion
    
    #region Private
    private RenderTexture2D renderTexture = new();
    private RenderTexture2D shaderBuffer = new();
    // private ShapeShader? singleShader = null;
    private Rect textureRect = new();
    private Dimensions screenDimensions = new();
    #endregion
    
    #endregion

    #region Constructors
    
    public ScreenTexture2(ShaderSupportType shaderSupportType, TextureFilter textureFilter = TextureFilter.Bilinear)
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
    }
    public ScreenTexture2(float pixelationFactor, ShaderSupportType shaderSupportType, TextureFilter textureFilter = TextureFilter.Bilinear)
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
    }
    public ScreenTexture2(Dimensions fixedDimensions, ShaderSupportType shaderSupportType, TextureFilter textureFilter = TextureFilter.Bilinear)
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
    }
    
    #endregion

    #region Public Functions


    public void Initialize(Dimensions screenSize, Vector2 mousePosition, ShapeCamera? camera = null)
    {
        if(camera != null) Camera = camera;
        
        screenDimensions = screenSize;
        UpdateTexture(screenSize);
        textureRect = CalculateTextureRect();
        Camera?.SetSize(new Dimensions(Width, Height));

        var scaledMousePostionGame = mousePosition;
        var scaledMousePositionUi = mousePosition;
        if (Mode == ScreenTextureMode.Pixelation)
        {
            scaledMousePositionUi = mousePosition / PixelationFactor;
            if (Camera != null)
            {
                scaledMousePostionGame = Camera.ScreenToWorld(mousePosition);
            }
            else
            {
                scaledMousePostionGame = scaledMousePositionUi;
            }
        }
        else if (Mode == ScreenTextureMode.Fixed)
        {
            if (textureRect.X > 0)
            {
                var f = screenSize.Height / textureRect.Height;
                float xOffset = textureRect.X;
                float mouseX = (mousePosition.X - xOffset) / f;
                scaledMousePositionUi = new Vector2(mouseX, mousePosition.Y / f);
            }
            else if (textureRect.Y > 0)
            {
                var f = screenSize.Width / textureRect.Width;
                float yOffset = textureRect.Y;
                float mouseY = (mousePosition.Y - yOffset) / f;
                scaledMousePositionUi = new Vector2(mousePosition.X / f, mouseY);
            }
            else
            {
                var f = screenSize.Width / textureRect.Width;
                scaledMousePositionUi = mousePosition / f;
            }
            if (Camera != null)
            {
                scaledMousePostionGame = Camera.ScreenToWorld(mousePosition);
            }
            else
            {
                scaledMousePostionGame = scaledMousePositionUi;
            }
        }
        
        GameScreenInfo = new(Camera?.Area ?? textureRect, scaledMousePostionGame);
        GameUiScreenInfo = new(textureRect, scaledMousePositionUi);
    }
    
    public void Update(float dt, Dimensions screenSize, Vector2 mousePosition, bool paused)
    {
        
        if (screenDimensions != screenSize)
        {
            screenDimensions = screenSize;
            UpdateTexture(screenSize);
        }
        
        textureRect = CalculateTextureRect();
        if (Camera != null)
        {
            Camera.SetSize(new Dimensions(Width, Height));
            if(!paused) Camera.Update(dt);
        }

        var scaledMousePostionGame = mousePosition;
        var scaledMousePositionUi = mousePosition;
        if (Mode == ScreenTextureMode.Pixelation)
        {
            scaledMousePositionUi = mousePosition / PixelationFactor;
            if (Camera != null)
            {
                scaledMousePostionGame = Camera.ScreenToWorld(mousePosition);
            }
            else
            {
                scaledMousePostionGame = scaledMousePositionUi;
            }
        }
        else if (Mode == ScreenTextureMode.Fixed)
        {
            if (textureRect.X > 0)
            {
                var f = screenSize.Height / textureRect.Height;
                float xOffset = textureRect.X;
                float mouseX = (mousePosition.X - xOffset) / f;
                scaledMousePositionUi = new Vector2(mouseX, mousePosition.Y / f);
            }
            else if (textureRect.Y > 0)
            {
                var f = screenSize.Width / textureRect.Width;
                float yOffset = textureRect.Y;
                float mouseY = (mousePosition.Y - yOffset) / f;
                scaledMousePositionUi = new Vector2(mousePosition.X / f, mouseY);
            }
            else
            {
                var f = screenSize.Width / textureRect.Width;
                scaledMousePositionUi = mousePosition / f;
            }
            
            if (Camera != null)
            {
                scaledMousePostionGame = Camera.ScreenToWorld(mousePosition);
            }
            else
            {
                scaledMousePostionGame = scaledMousePositionUi;
            }
        }
        else
        {
            if (Camera != null)
            {
                scaledMousePostionGame = Camera.ScreenToWorld(mousePosition);
            }
            else
            {
                scaledMousePostionGame = scaledMousePositionUi;
            }
        }
        GameScreenInfo = new(Camera?.Area ?? textureRect, scaledMousePostionGame);
        GameUiScreenInfo = new(textureRect, scaledMousePositionUi);
    }
    
    public void DrawOnTexture()
    {
        bool shaderMode = ShaderSupport != ShaderSupportType.None && Shaders != null && Shaders.HasActiveShaders();

        if (shaderMode)
        {
            Raylib.BeginTextureMode(renderTexture);
            Raylib.ClearBackground(new(0,0,0,0));

            if (Camera != null)
            {
                Raylib.BeginMode2D(Camera.Camera);
                OnDrawGame?.Invoke(GameScreenInfo);
                Raylib.EndMode2D();
                
                // OnDrawGameUI?.Invoke(ScreenInfo);
            }
            else
            {
                OnDrawGame?.Invoke(GameScreenInfo);
                // OnDrawGameUI?.Invoke(ScreenInfo);
            }
            
            Raylib.EndTextureMode();
            
            ApplyShaders(); // we know that we have shaders active
            
            Raylib.BeginTextureMode(renderTexture);
            OnDrawUI?.Invoke(GameUiScreenInfo);
            Raylib.EndTextureMode();
            
            //DrawToScreen();
        }
        else
        {
            Raylib.BeginTextureMode(renderTexture);
            Raylib.ClearBackground(new(0,0,0,0));

            if (Camera != null)
            {
                Raylib.BeginMode2D(Camera.Camera);
                OnDrawGame?.Invoke(GameScreenInfo);
                Raylib.EndMode2D();
            }
            else OnDrawGame?.Invoke(GameScreenInfo);
            
            // OnDrawGameUI?.Invoke(ScreenInfo);
            OnDrawUI?.Invoke(GameUiScreenInfo);
            
            Raylib.EndTextureMode();
            //DrawToScreen();
            
        }
    }
    public void DrawToScreen()
    {
        // if (singleShader != null)
        // {
        //     Raylib.BeginShaderMode(singleShader.Shader);
        //     DrawTextureToScreen(RenderTexture.Texture);
        //     Raylib.EndShaderMode();
        // }
        // else
        // {
        //     DrawTextureToScreen(RenderTexture.Texture);
        // }
        
        DrawTextureToScreen(renderTexture.Texture);
    }
    public void Unload()
    {
        if (!Loaded) return;
        Loaded = false;
        Raylib.UnloadRenderTexture(renderTexture);
        if(ShaderSupport != ShaderSupportType.None) Raylib.UnloadRenderTexture(shaderBuffer);
    }
    
    #endregion

    #region Private Functions
    
    private Rect CalculateTextureRect()
    {
        return new Rect(0, 0, Width, Height);
        // if(Mode is ScreenTextureMode.Stretch or ScreenTextureMode.Pixelation) return new Rect(0, 0, Width, Height);
        //
        // var wDif = ShapeMath.AbsInt(screenDimensions.Width - Width);
        // var hDif = ShapeMath.AbsInt(screenDimensions.Height - Height);
        // if (wDif == hDif)
        // {
        //     return new Rect(0, 0, Width, Height);
        // }
        //
        // if (wDif < hDif)
        // {
        //     //scale to height
        //     float hFactor = (float)screenDimensions.Height / Height;
        //     var newW = (int)(Width * hFactor);
        //     return new Rect((screenDimensions.Width - newW) / 2f, 0f, newW, Height);
        //     
        // }
        //
        // //scale to width
        // float wFactor = (float)screenDimensions.Width / Width;
        // var newH = (int)(Height * wFactor);
        // return new Rect((screenDimensions.Width - newH) / 2f, 0f, Width, newH);
    }
   
    private void ApplyShaders()
    {
        if (ShaderSupport == ShaderSupportType.None) return;
        if (Shaders == null) return;
        
        var activeScreenShaders = Shaders.GetActiveShaders();
        if (activeScreenShaders.Count <= 0) return;

        if (activeScreenShaders.Count == 1)
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
            X = Width * 0.5f,
            Y = Height * 0.5f,
            Width = Width,
            Height = Height
        };
        Vector2 origin = new()
        {
            X = Width * 0.5f,
            Y = Height * 0.5f
        };
        
        var sourceRec = new Rectangle(0, 0, Width, -Height);
        
        Raylib.DrawTexturePro(texture, sourceRec, destRec, origin, 0f, new ColorRgba(System.Drawing.Color.White).ToRayColor());
    }
    private void UpdateTexture(Dimensions screenSize)
    {
        if (Mode == ScreenTextureMode.Fixed) return;

        int w = screenSize.Width;
        int h = screenSize.Height;
        
        if (Mode == ScreenTextureMode.Pixelation)
        {
            w = (int)(w * PixelationFactor);
            h = (int)(h * PixelationFactor);
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
    }
    private void DrawTextureToScreen(Texture2D texture)
    {
        if (Mode == ScreenTextureMode.Stretch)
        {
            var w = Width;
            var h = Height;
            var destRec = new Rectangle
            {
                X = w * 0.5f,
                Y = h * 0.5f,
                Width = w,
                Height = h
            };
            Vector2 origin = new()
            {
                X = w * 0.5f,
                Y = h * 0.5f
            };
        
            var sourceRec = new Rectangle(0, 0, w, -h);
        
            Raylib.DrawTexturePro(texture, sourceRec, destRec, origin, 0f, Tint.ToRayColor());
        }
        else if (Mode == ScreenTextureMode.Pixelation)
        {
            var w = screenDimensions.Width;
            var h = screenDimensions.Height;
            var destRec = new Rectangle
            {
                X = w * 0.5f,
                Y = h * 0.5f,
                Width = w,
                Height = h
            };
            Vector2 origin = new()
            {
                X = w * 0.5f,
                Y = h * 0.5f
            };
        
            var sourceRec = new Rectangle(0, 0, Width, -Height);
        
            Raylib.DrawTexturePro(texture, sourceRec, destRec, origin, 0f, Tint.ToRayColor());
        }
        else // Fixed Mode
        {
            float virtualRatioW = (float)screenDimensions.Width/ Width;
            float virtualRatioH = (float)screenDimensions.Height/ Height;
            Rectangle destRec;
            var originX = 0f;
            var originY = 0f;
            if (virtualRatioW < virtualRatioH)
            {
                var w = Width * virtualRatioW;
                var h = Width * virtualRatioW;
                originY = screenDimensions.Height / 2f - h / 2f;
                destRec = new Rectangle(originX, originY, w, h);
            }
            else
            {
                var w = Height * virtualRatioH;
                var h = Height * virtualRatioH;
                originX = screenDimensions.Width / 2f - w / 2f;
                destRec = new Rectangle(originX, originY, w, h);
            }
            
            var origin = new Vector2(0, 0);
            var sourceRec = new Rectangle(0, 0, Width, -Height);
            
            Raylib.DrawTexturePro(texture, sourceRec, destRec, origin, 0f, Tint.ToRayColor());
        }
        
    }
   
    #endregion
    
}

internal sealed class ScreenTexture
{
    public bool Loaded { get; private set; } = false;
    public RenderTexture2D RenderTexture { get; private set; } = new();
    public int Width { get; private set; } = 0;
    public int Height { get; private set; } = 0;
    
    
    /// <summary>
    /// Requires to unload and load the texture to take effect!
    /// </summary>
    public TextureFilter TextureFilter = TextureFilter.Bilinear;

    public ScreenTexture(){}

    public void Load(Dimensions dimensions)
    {
        if (Loaded) return;
        Loaded = true;
        SetTexture(dimensions);
    }
    public void Unload()
    {
        if (!Loaded) return;
        Loaded = false;
        Raylib.UnloadRenderTexture(RenderTexture);
    }
    public void UpdateDimensions(Dimensions dimensions)
    {
        if (!Loaded) return;

        if (Width == dimensions.Width && Height == dimensions.Height) return;
        
        Raylib.UnloadRenderTexture(RenderTexture);
        SetTexture(dimensions);
    }
    public void Draw()
    {
        var destRec = new Rectangle
        {
            X = Width * 0.5f,
            Y = Height * 0.5f,
            Width = Width,
            Height = Height
        };
        Vector2 origin = new()
        {
            X = Width * 0.5f,
            Y = Height * 0.5f
        };
        
        var sourceRec = new Rectangle(0, 0, Width, -Height);
        
        Raylib.DrawTexturePro(RenderTexture.Texture, sourceRec, destRec, origin, 0f, new ColorRgba(System.Drawing.Color.White).ToRayColor());
    }
    
    private void SetTexture(Dimensions dimensions)
    {
        Width = dimensions.Width;
        Height = dimensions.Height;
        RenderTexture = Raylib.LoadRenderTexture(Width, Height);
        Raylib.SetTextureFilter(RenderTexture.Texture, TextureFilter);
    }
    
    //public void DrawTexture(int targetWidth, int targetHeight)
    //{
    //var destRec = new Rectangle
    //{
    //    x = targetWidth * 0.5f,
    //    y = targetHeight * 0.5f,
    //    width = targetWidth,
    //    height = targetHeight
    //};
    //Vector2 origin = new()
    //{
    //    X = targetWidth * 0.5f,
    //    Y = targetHeight * 0.5f
    //};
    //
    //
    //
    //var sourceRec = new Rectangle(0, 0, Width, -Height);
    //
    //DrawTexturePro(RenderTexture.texture, sourceRec, destRec, origin, 0f, WHITE);
    //
}