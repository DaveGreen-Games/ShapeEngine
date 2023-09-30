﻿using System.Numerics;
using System.Runtime.Serialization.Json;
using Raylib_CsLo;
using ShapeEngine.Lib;
using ShapeEngine.Persistent;
using ShapeEngine.Core;
using Examples.Scenes;
using ShapeEngine.Screen;

namespace Examples
{
    internal class SimpleCursor : ICursor
    {
        public uint GetID()
        {
            return 0;
        }

        public void Draw(ScreenInfo ui)
        {
            Vector2 center = ui.MousePos;
            float size = ui.Area.Size.Min() * 0.02f;
            Vector2 a = center;
            Vector2 b = center + new Vector2(0, size);
            Vector2 c = center + new Vector2(size, size);
            //SDrawing.DrawTriangle(a, b, c, RED);
            Triangle cursor = new(a, b, c);
            cursor.Draw(ExampleScene.ColorHighlight2);
            cursor.DrawLines(1f, ExampleScene.ColorHighlight1);
            //SDrawing.DrawCircle(center, 4, RED);
        }

        public void Update(float dt, ScreenInfo ui)
        {
            
        }

        public void Deactivate()
        {
            
        }

        public void Activate(ICursor oldCursor)
        {
            
        }
    }
    public class GameloopExamples : ShapeLoop
    {
        //public BasicCamera GameCam { get; private set; }
        public Font FontDefault { get; private set; }


        private Dictionary<int, Font> fonts = new();
        private List<string> fontNames = new();
        private MainScene? mainScene = null;

        private uint crtShaderID = SID.NextID;
        private Vector2 crtCurvature = new(6, 4);
        
        public GameloopExamples() : base(new(1920, 1080), true, true)
        {
            
            BackgroundColor = ExampleScene.ColorDark;
        }
        protected override void LoadContent()
        {
            fonts.Add(FontIDs.GruppoRegular, ContentLoader.LoadFont("Resources/Fonts/Gruppo-Regular.ttf", 100));
            fonts.Add(FontIDs.IndieFlowerRegular, ContentLoader.LoadFont("Resources/Fonts/IndieFlower-Regular.ttf", 100));
            fonts.Add(FontIDs.OrbitRegular, ContentLoader.LoadFont("Resources/Fonts/Orbit-Regular.ttf", 100));
            fonts.Add(FontIDs.OrbitronBold, ContentLoader.LoadFont("Resources/Fonts/Orbitron-Bold.ttf", 100));
            fonts.Add(FontIDs.OrbitronRegular, ContentLoader.LoadFont("Resources/Fonts/Orbitron-Regular.ttf", 100));
            fonts.Add(FontIDs.PromptLightItalic, ContentLoader.LoadFont("Resources/Fonts/Prompt-LightItalic.ttf", 100));
            fonts.Add(FontIDs.PromptRegular, ContentLoader.LoadFont("Resources/Fonts/Prompt-Regular.ttf", 100));
            fonts.Add(FontIDs.PromptThin, ContentLoader.LoadFont("Resources/Fonts/Prompt-Thin.ttf", 100));
            fonts.Add(FontIDs.TekoMedium, ContentLoader.LoadFont("Resources/Fonts/Teko-Medium.ttf", 100));
            fonts.Add(FontIDs.JetBrains, ContentLoader.LoadFont("Resources/Fonts/JetBrainsMono.ttf", 100));
            
            fontNames.Add("Gruppo Regular");
            fontNames.Add("Indie Flower Regular");
            fontNames.Add("Orbit Regular");
            fontNames.Add("Orbitron Bold");
            fontNames.Add("Orbitron Regular");
            fontNames.Add("Prompt Light Italic");
            fontNames.Add("Prompt Regular");
            fontNames.Add("Prompt Thin");
            fontNames.Add("Teko Medium");
            fontNames.Add("Jet Brains Mono");


            Shader crt = ContentLoader.LoadFragmentShader("Resources/Shaders/CRTShader.fs");
            ShapeShader crtShader = new(crt, crtShaderID, true, 1);
            ShapeShader.SetValueFloat(crtShader.Shader, "renderWidth", CurScreenSize.Width);
            ShapeShader.SetValueFloat(crtShader.Shader, "renderHeight", CurScreenSize.Height);
            var bgColor = BackgroundColor;
            ShapeShader.SetValueColor(crtShader.Shader, "cornerColor", bgColor);// 1, 0, 0, 1);
            ShapeShader.SetValueFloat(crtShader.Shader, "vignetteOpacity", 0.35f);
            ShapeShader.SetValueVector2(crtShader.Shader, "curvatureAmount", crtCurvature.X, crtCurvature.Y);//smaller values = bigger curvature
            ScreenShaders.Add(crtShader);
            
            FontDefault = GetFont(FontIDs.JetBrains);
            this.VSync = false;
            this.FrameRateLimit = 60;

            Raylib.HideCursor();
            SwitchCursor(new SimpleCursor());
        }
        protected override void UnloadContent()
        {
            ContentLoader.UnloadFonts(fonts.Values);
        }
        protected override void BeginRun()
        {
            mainScene = new MainScene();
            GoToScene(mainScene);
        }

        protected override void WindowSizeChanged(DimensionConversionFactors conversionFactors)
        {
            var crtShader = ScreenShaders.Get(crtShaderID);
            if (crtShader != null)
            {
                ShapeShader.SetValueFloat(crtShader.Shader, "renderWidth", CurScreenSize.Width);
                ShapeShader.SetValueFloat(crtShader.Shader, "renderHeight", CurScreenSize.Height);
            }
        }

        //protected override void HandleInput(float dt)
        //{
        //    if (IsKeyPressed(KeyboardKey.KEY_F))
        //    {
        //        GAMELOOP.ToggleWindowMaximize();
        //    }
        //    base.HandleInput(dt);
        //}
        protected override void Update(float dt)
        {
            UpdateScene();

            int speed = 2;
            int movement = 0;
            if (IsKeyDown(KeyboardKey.KEY_J))
            {
                movement = 1;
            }
            else if (IsKeyDown(KeyboardKey.KEY_K))
            {
                movement = -1;
            }


            if (movement != 0)
            {
                float change = movement * speed * dt;
                crtCurvature = (crtCurvature + new Vector2(change)).Clamp(new Vector2(1.5f, 1f), new Vector2(12, 8));
                
                var crtShader = ScreenShaders.Get(crtShaderID);
                if (crtShader != null)
                {
                    ShapeShader.SetValueVector2(crtShader.Shader, "curvatureAmount", crtCurvature.X, crtCurvature.Y);
                }
            }
            
            
            
        }

        protected override void DrawGame(ScreenInfo game)
        {
            DrawGameScene();
        }

        protected override void DrawUI(ScreenInfo ui)
        {
            DrawUIScene();
        }

        public int GetFontCount() { return fonts.Count; }
        public Font GetFont(int id) { return fonts[id]; }
        public string GetFontName(int id) { return fontNames[id]; }
        public Font GetRandomFont()
        {
            Font? randFont = SRNG.randCollection<Font>(fonts.Values.ToList(), false);
            return randFont != null ? (Font)randFont : FontDefault;
        }
        public void GoToMainScene()
        {
            if (mainScene == null) return;
            if (CurScene == mainScene) return;
            GoToScene(mainScene);
        }
    }

}
