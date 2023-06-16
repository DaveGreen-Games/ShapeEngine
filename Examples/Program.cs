global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
global using static Examples.Program;

using Raylib_CsLo;
using ShapeEngine;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.UI;
using ShapeEngine.Persistent;
//put scenes in their own files!!!
namespace Examples
{
    public static class Program
    {
        public static GameloopExamples GAMELOOP = new GameloopExamples();
        public static void Main(string[] args)
        {
            GAMELOOP.SetupWindow("Shape Engine Examples", false, true);
            GAMELOOP.Run(args);
        }
    }
    public static class FontIDs
    {
        public static readonly int AbelRegular = 0;
        public static readonly int GruppoRegular = 1;
        public static readonly int IndieFlowerRegular = 2;
        public static readonly int OrbitRegular = 3;
        public static readonly int OrbitronBold = 4;
        public static readonly int OrbitronRegular = 5;
        public static readonly int PromptLightItalic = 6;
        public static readonly int PromptRegular = 7;
        public static readonly int PromptThin = 8;
        public static readonly int TekoMedium = 9;
    }
    public class GameloopExamples : GameLoopScene
    {
        public Font FontDefault { get; private set; }

        

        private Dictionary<int, Font> fonts = new();
        private List<string> fontNames = new();
        private MainScene? mainScene = null;
        
        public GameloopExamples() : base(960, 540, 1920, 1080) 
        {
            
        }
        protected override void LoadContent()
        {
            fonts.Add(FontIDs.AbelRegular, ContentLoader.LoadFont("fonts/Abel-Regular.ttf", 200));
            fonts.Add(FontIDs.GruppoRegular, ContentLoader.LoadFont("fonts/Gruppo-Regular.ttf", 200));
            fonts.Add(FontIDs.IndieFlowerRegular, ContentLoader.LoadFont("fonts/IndieFlower-Regular.ttf", 200));
            fonts.Add(FontIDs.OrbitRegular, ContentLoader.LoadFont("fonts/Orbit-Regular.ttf", 200));
            fonts.Add(FontIDs.OrbitronBold, ContentLoader.LoadFont("fonts/Orbitron-Bold.ttf", 200));
            fonts.Add(FontIDs.OrbitronRegular, ContentLoader.LoadFont("fonts/Orbitron-Regular.ttf", 200));
            fonts.Add(FontIDs.PromptLightItalic, ContentLoader.LoadFont("fonts/Prompt-LightItalic.ttf", 200));
            fonts.Add(FontIDs.PromptRegular, ContentLoader.LoadFont("fonts/Prompt-Regular.ttf", 200));
            fonts.Add(FontIDs.PromptThin, ContentLoader.LoadFont("fonts/Prompt-Thin.ttf", 200));
            fonts.Add(FontIDs.TekoMedium, ContentLoader.LoadFont("fonts/Teko-Medium.ttf", 200));

            fontNames.Add("Abel Regular");
            fontNames.Add("Gruppo Regular");
            fontNames.Add("Indie Flower Regular");
            fontNames.Add("Orbit Regular");
            fontNames.Add("Orbitron Bold");
            fontNames.Add("Orbitron Regular");
            fontNames.Add("Prompt Light Italic");
            fontNames.Add("Prompt Regular");
            fontNames.Add("Prompt Thin");
            fontNames.Add("Teko Medium");

            FontDefault = GetFont(FontIDs.IndieFlowerRegular);

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

        protected override void HandleInput(float dt)
        {
            if (IsKeyPressed(KeyboardKey.KEY_F))
            {
                GAMELOOP.ToggleWindowMaximize();
            }
            base.HandleInput(dt);
        }

        public int GetFontCount() { return  fonts.Count; }
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
    
    
    //implement for back button in example scene
    public class BasicButton : UIElement { }
    //rename to scene button
    public class ExampleSelectionButton : UIElement
    {
        //public string Text { get; set; } = "Button";
        private Vector2 prevMousePos = new();
        public ExampleScene? Scene { get; private set; } = null;
        private Font font;
        public ExampleSelectionButton() 
        { 
            Hidden = true; 
            DisabledSelection = true;
            font = GAMELOOP.FontDefault; // GAMELOOP.GetFont(GameloopExamples.FONT_IndieFlowerRegular);

        }

        public void SetScene(ExampleScene? newScene)
        {
            Scene = newScene;
            Hidden = newScene == null;
            DisabledSelection = Hidden;
            
        }
        protected override bool CheckMousePressed()
        {
            if (Hidden || Scene == null) return false;
            return IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT);
        }

        protected override bool CheckPressed()
        {
            if (Hidden || Scene == null) return false;
            return IsKeyReleased(KeyboardKey.KEY_SPACE) || IsKeyPressed(KeyboardKey.KEY_ENTER);
        }
        public override void Update(float dt, Vector2 mousePosUI)
        {
            Check(prevMousePos, mousePosUI, false, 5f);
            prevMousePos = mousePosUI;
        }
        public override void Draw()
        {
            if (Hidden || Scene == null) return;
            
            var r = GetRect();
            var text = Scene.Title;

            if (Selected)
            {
                //r.DrawLines(4f, GREEN);
                text.Draw(r, 5f, GREEN, font, new(0f));
            }
            else if (Pressed)
            {
                //r.DrawLines(4f, YELLOW);
                text.Draw(r, 5f, YELLOW, font, new(0f));
            }
            else 
            {
                //r.DrawLines(4f, WHITE);
                text.Draw(r, 5f, WHITE, font, new(0f));
            } 
            

        }
        protected override void PressedChanged(bool pressed)
        {
            if(Hidden || Scene == null) return;
            if (pressed)
            {
                GAMELOOP.GoToScene(Scene);
            }
        }
    }
    
    public class MainScene : Scene
    {
        //should display names/description of all examples
        //left half lists all examples vertical / right half displays short info
        //up/ down navigates through all examples scenes + mouse click
        //esc goes back to main scene
        //r always resets cur example scene
        //r in main scene resets all example scenes

        private int curPageIndex = 0;
        private List<ExampleScene> examples = new();
        private List<ExampleSelectionButton> buttons = new();
        private UIElement curButton;
        private Font titleFont;
        public MainScene()
        {
            for (int i = 0; i < 10; i++)
            {
                ExampleSelectionButton b = new ExampleSelectionButton();
                buttons.Add(b);
                b.WasSelected += OnButtonSelected;
            }
            curButton = buttons[0];
            examples.Add(new PolylineInflationExample());
            examples.Add(new TextScalingExample());

            titleFont = GAMELOOP.FontDefault; // GAMELOOP.GetFont(GameloopExamples.FONT_IndieFlowerRegular);

            SetupButtons();
        }

        

        public override void HandleInput(float dt)
        {
            if (IsKeyPressed(KeyboardKey.KEY_R))
            {
                for (int i = 0; i < examples.Count; i++)
                {
                    examples[i].Reset();
                }
            }

            if (IsKeyPressed(KeyboardKey.KEY_Q)) PrevPage();
            else if (IsKeyPressed(KeyboardKey.KEY_E)) NextPage();

            if(IsKeyPressed(KeyboardKey.KEY_W)) PrevButton();
            else if (IsKeyPressed(KeyboardKey.KEY_S)) NextButton();
        }

        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            foreach (var b in buttons)
            {
                b.Update(dt, GAMELOOP.UI.MousePos);
            }
        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            Vector2 start = uiSize * new Vector2(0.02f, 0.25f);
            Vector2 size = uiSize * new Vector2(0.45f, 0.05f);
            Vector2 gap = uiSize * new Vector2(0f, 0.07f);
            for (int i = 0; i < buttons.Count; i++)
            {
                var b = buttons[i];
                b.UpdateRect(start + gap * i, size, new Vector2(0f));
                b.Draw();
            }


            string text = "Shape Engine Examples";
            Rect titleRect = new Rect(uiSize * new Vector2(0.5f, 0.01f), uiSize * new Vector2(0.75f, 0.1f), new Vector2(0.5f, 0f));
            text.Draw(titleRect, 10f, WHITE, titleFont, new(0.5f));

            int pages = GetMaxPages();
            string pagesText = pages <= 1 ? "Page 1/1" : String.Format("[Q] <- Page #{0}/{1} -> [E]", curPageIndex + 1, pages);
            Rect pageRect = new Rect(uiSize * new Vector2(0.02f, 0.1f), uiSize * new Vector2(0.3f, 0.05f), new Vector2(0f, 0f));
            pagesText.Draw(pageRect, 4f, RED, titleFont, new(0f, 0.5f));

            Segment s = new(uiSize * new Vector2(0f, 0.17f), uiSize * new Vector2(1f, 0.17f));
            s.Draw(4f, RED);
        }
        private void OnButtonSelected(UIElement button)
        {
            if (curButton != button)
            {
                curButton.Deselect();
                curButton = button;
            }
        }
        private int GetCurButtonIndex()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                var b = buttons[i];
                if (b.Selected) return i;
            }
            return -1;
        }
        private int GetVisibleButtonCount()
        {
            int count = 0;
            foreach (var b in buttons)
            {
                if (!b.Hidden) count++;
            }
            return count;
        }
        private int GetMaxPages() 
        {
            if (buttons.Count <= 0 || examples.Count <= 0) return 1;
            int pages = (examples.Count - 1) / buttons.Count;
            if (pages < 1) return 1;
            else return pages + 1;
        }
        private void SetupButtons()
        {
            int startIndex = curPageIndex * buttons.Count;
            int endIndex = startIndex + buttons.Count;
            int buttonIndex = 0;
            for (int i = curPageIndex * buttons.Count; i < endIndex; i++)
            {
                var b = buttons[buttonIndex];
                buttonIndex++;
                if(i < examples.Count) b.SetScene(examples[i]);
                else b.SetScene(null);
            }
            
            buttons[0].Select();
        }
        private void NextButton() 
        {
            int curButtonIndex = GetCurButtonIndex();
            if (curButtonIndex < 0) return;
            curButtonIndex++;
            int buttonCount = GetVisibleButtonCount();
            if (curButtonIndex >= buttonCount) curButtonIndex = 0;
            buttons[curButtonIndex].Select();
        }
        private void PrevButton() 
        {
            int curButtonIndex = GetCurButtonIndex();
            if (curButtonIndex < 0) return;
            curButtonIndex--;
            int buttonCount = GetVisibleButtonCount();
            if (curButtonIndex < 0) curButtonIndex = buttonCount - 1;
            buttons[curButtonIndex].Select();
        }
        private void NextPage()
        {
            int maxPages = GetMaxPages();
            if (maxPages <= 1) return;

            curPageIndex++;
            if (curPageIndex >= maxPages) curPageIndex = 0;
            SetupButtons();
        }
        private void PrevPage()
        {
            int maxPages = GetMaxPages();
            if (maxPages <= 1) return;

            curPageIndex--;
            if (curPageIndex < 0) curPageIndex = maxPages - 1;
            SetupButtons();
        }
    }
    public class ExampleScene : Scene
    {
        public string Title { get; protected set; } = "Title Goes Here";
        public string Description { get; protected set; } = "No Description Yet.";

        protected Font titleFont;

        public ExampleScene()
        {
            titleFont = GAMELOOP.FontDefault;// GAMELOOP.GetFont(GameloopExamples.FONT_IndieFlowerRegular);
        }

        public virtual void Reset() { }

        public override void HandleInput(float dt)
        {
            if (IsKeyPressed(KeyboardKey.KEY_R)) Reset();
            if (IsKeyPressed(KeyboardKey.KEY_ESCAPE)) GAMELOOP.GoToMainScene();
        }

        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            Segment s = new(uiSize * new Vector2(0f, 0.07f), uiSize * new Vector2(1f, 0.07f));
            s.Draw(2f, WHITE);
            
            Rect r = new Rect(uiSize * new Vector2(0.5f, 0.02f), uiSize * new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0f));
            Title.Draw(r, 10f, WHITE, titleFont, new(0.5f));

            string backText = "Back [ESC]";
            Rect backRect = new Rect(uiSize * new Vector2(0.02f, 0.02f), uiSize * new Vector2(0.25f, 0.03f), new Vector2(0f));
            backText.Draw(backRect, 4f, RED, titleFont, new Vector2(0.5f));
        }
    }
    
    public class TextScalingExample : ExampleScene
    {
        Vector2 topLeft = new();
        Vector2 bottomRight = new();

        bool mouseInsideTopLeft = false;
        bool mouseInsideBottomRight = false;

        bool draggingTopLeft = false;
        bool draggingBottomRight = false;

        float pointRadius = 8f;
        float interactionRadius = 24f;

        string text = "Longer Test Text.";
        int fontSpacing = 1;
        int maxFontSpacing = 25;
        Font font;
        int fontIndex = 0;

        public TextScalingExample()
        {
            Title = "Text Scaling Example";
            var s = GAMELOOP.UI.GetSize();
            topLeft = s * new Vector2(0.1f, 0.1f);
            bottomRight = s * new Vector2(0.9f, 0.9f);
            font = GAMELOOP.GetFont(fontIndex);
        }

        public override void HandleInput(float dt)
        {
            if (IsKeyPressed(KeyboardKey.KEY_W)) NextFont();
            else if (IsKeyPressed(KeyboardKey.KEY_S)) PrevFont();

            if (IsKeyPressed(KeyboardKey.KEY_D)) ChangeFontSpacing(1);
            else if (IsKeyPressed(KeyboardKey.KEY_A)) ChangeFontSpacing(-1);

            if (mouseInsideTopLeft)
            {
                if (draggingTopLeft)
                {
                    if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) draggingTopLeft = false;
                }
                else
                {
                    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) draggingTopLeft = true;
                }
                
            }
            else if (mouseInsideBottomRight)
            {
                if (draggingBottomRight)
                {
                    if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) draggingBottomRight = false;
                }
                else
                {
                    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) draggingBottomRight = true;
                }
            }
        }
        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            if(draggingTopLeft || draggingBottomRight)
            {
                if (draggingTopLeft) topLeft = mousePosUI;
                else if (draggingBottomRight) bottomRight = mousePosUI;
            }
            else
            {
                float topLeftDisSq = (topLeft - mousePosUI).LengthSquared();
                mouseInsideTopLeft = topLeftDisSq <= interactionRadius * interactionRadius;

                if (!mouseInsideTopLeft)
                {
                    float bottomRightDisSq = (bottomRight - mousePosUI).LengthSquared();
                    mouseInsideBottomRight = bottomRightDisSq <= interactionRadius * interactionRadius;
                }
            }
            
        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            

            Rect r = new(topLeft, bottomRight);
            r.DrawLines(8f, new Color(255, 0, 0, 150));
            text.Draw(r, fontSpacing, WHITE, font, new Vector2(0.5f, 0.5f));

            Circle topLeftPoint = new(topLeft, pointRadius);
            Circle topLeftInteractionCircle = new(topLeft, interactionRadius);

            Circle bottomRightPoint = new(bottomRight, pointRadius);
            Circle bottomRightInteractionCircle = new(bottomRight, interactionRadius);

            if (draggingTopLeft)
            {
                topLeftInteractionCircle.Draw(GREEN);
            }
            else if (mouseInsideTopLeft)
            {
                topLeftPoint.Draw(WHITE);
                topLeftInteractionCircle.radius *= 2f;
                topLeftInteractionCircle.DrawLines(2f, GREEN, 4f);
            }
            else
            {
                topLeftPoint.Draw(WHITE);
                topLeftInteractionCircle.DrawLines(2f, WHITE, 4f);
            }

            if (draggingBottomRight)
            {
                bottomRightInteractionCircle.Draw(GREEN);
            }
            else if (mouseInsideBottomRight)
            {
                bottomRightPoint.Draw(WHITE);
                bottomRightInteractionCircle.radius *= 2f;
                bottomRightInteractionCircle.DrawLines(2f, GREEN, 4f);
            }
            else
            {
                bottomRightPoint.Draw(WHITE);
                bottomRightInteractionCircle.DrawLines(2f, WHITE, 4f);
            }
            //float fontSize = font.baseSize;
            //float fontSpacingWidth = (text.Length - 1) * fontSpacing;
            //Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, 0);
            //Vector2 size = r.Size;
            ////Vector2 f = size / fontDimensions;
            //float f = (size.X - fontSpacingWidth) / fontDimensions.X;
            ////float difX = fontDimensions.X - size.X;
            ////float difY = fontDimensions.Y - size.Y;
            ////if (difX > 0 && difY > 0) f = difX > difY ? size.X / fontDimensions.X : size.Y / fontDimensions.Y;
            ////else if (difX > 0) f = size.X / fontDimensions.X;
            ////else if (difY > 0) f = size.Y / fontDimensions.Y;
            ////else f = difX < difY ? size.X / fontDimensions.X : size.Y / fontDimensions.Y;
            //
            //Rect rectFontDimensionsBasic = new(topLeft, fontDimensions, new Vector2(0f));
            //rectFontDimensionsBasic.DrawLines(2f, GREEN);
            //
            //Rect rectFontDimensionsScaled = new(topLeft, fontDimensions * f, new Vector2(0f));
            //rectFontDimensionsScaled.DrawLines(2f, RED);
            //
            //Raylib.DrawTextEx(font, text, topLeft, fontSize, fontSpacing, GREEN);
            //Raylib.DrawTextEx(font, text, topLeft, fontSize * f, fontSpacing, RED);


            string info = String.Format("[{0}] - Spacing [{1}]", GAMELOOP.GetFontName(fontIndex), fontSpacing);
            Rect infoRect = new(uiSize * new Vector2(0.5f, 0.98f), uiSize * new Vector2(0.4f, 0.1f), new Vector2(0.5f, 1f));
            info.Draw(infoRect, 4f, YELLOW, font, new Vector2(0.5f, 0.5f));
        }
        private void ChangeFontSpacing(int amount)
        {
            fontSpacing += amount;
            if (fontSpacing < 1) fontSpacing = maxFontSpacing;
            else if (fontSpacing > maxFontSpacing) fontSpacing = 1;
        }
        private void NextFont()
        {
            int fontCount = GAMELOOP.GetFontCount();
            fontIndex++;
            if (fontIndex >= fontCount) fontIndex = 0;
            font = GAMELOOP.GetFont(fontIndex);
        }
        private void PrevFont()
        {
            int fontCount = GAMELOOP.GetFontCount();
            fontIndex--;
            if (fontIndex < 0) fontIndex = fontCount - 1;
            font = GAMELOOP.GetFont(fontIndex);
        }
    }
    public class PolylineInflationExample : ExampleScene
    {
        ScreenTexture game;
        Polyline polyline = new();
        int dragIndex = -1;
        float offsetDelta = 0f;
        float lerpOffsetDelta = 0f;
        
        public PolylineInflationExample()
        {
            Title = "Polyline Inflation Example";
            game = GAMELOOP.Game;
            game.SetCamera(new BasicCamera(new Vector2(0f), new Vector2(1920, 1080), new Vector2(0.5f), 1f, 0f));
        }
        public override void Reset()
        {
            polyline = new();
            dragIndex = -1;
            offsetDelta = 0f;
            lerpOffsetDelta = 0f;
        }
        public override void HandleInput(float dt)
        {
            base.HandleInput(dt);
            if (IsKeyPressed(KeyboardKey.KEY_R)) { polyline = new(); }

            if (GetMouseWheelMove() > 0)
            {
                offsetDelta += 10f;
            }
            else if (GetMouseWheelMove() < 0)
            {
                offsetDelta -= 10f;
            }

            lerpOffsetDelta = Lerp(lerpOffsetDelta, offsetDelta, dt * 2f);

            offsetDelta = Clamp(offsetDelta, 0f, 300f);
        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            base.DrawUI(uiSize, mousePosUI);
            Vector2 mousePos = mousePosUI;

            float vertexRadius = 8f;
            int pickedVertex = -1;

            bool isMouseOnLine = false; // polyline.OverlapShape(new Circle(mousePos, vertexRadius * 2f));
            var closest = polyline.GetClosestPoint(mousePos);
            int closestIndex = polyline.GetClosestIndex(mousePos);
            bool drawClosest = true;


            for (int i = 0; i < polyline.Count; i++)
            {
                var p = polyline[i];
                float disSq = (mousePos - p).LengthSquared();
                if (pickedVertex == -1 && disSq < (vertexRadius * vertexRadius) * 2f)
                {
                    DrawCircleV(p, vertexRadius * 2f, GREEN);
                    pickedVertex = i;
                }
                else DrawCircleV(p, vertexRadius, GRAY);
                if (drawClosest)
                {
                    disSq = (closest - p).LengthSquared();
                    if (disSq < (vertexRadius * vertexRadius) * 4f)
                    {
                        drawClosest = false;
                    }
                }

            }

            if (drawClosest)
            {
                float disSq = (closest - mousePos).LengthSquared();

                float tresholdSq = 30 * 30;
                if (disSq > tresholdSq)
                {
                    drawClosest = false;
                }
                else isMouseOnLine = true;
            }


            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                if (pickedVertex == -1)
                {
                    if (isMouseOnLine)
                    {
                        polyline.Insert(closestIndex + 1, mousePos);
                    }
                    else polyline.Add(mousePos);

                }
                else
                {
                    dragIndex = pickedVertex;
                }
            }
            else if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dragIndex = -1;
            }
            else if (dragIndex == -1 && IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                if (pickedVertex > -1)
                {
                    polyline.RemoveAt(pickedVertex);
                }
            }

            if (dragIndex > -1) polyline[dragIndex] = mousePos;

            //polyline.Draw(4f, WHITE);
            var segments = polyline.GetEdges();
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];

                if (drawClosest)
                {
                    if (closestIndex == i) segment.Draw(4f, BLUE);
                    else segment.Draw(4f, WHITE);
                }
                else segment.Draw(4f, WHITE);



            }

            if (drawClosest) DrawCircleV(closest, vertexRadius, RED);

            if (lerpOffsetDelta > 10f)
            {
                var polygons = SClipper.Inflate(polyline, lerpOffsetDelta).ToPolygons();
                foreach (var polygon in polygons)
                {
                    polygon.DrawLines(3f, GOLD);
                }
            }

        }
    }
    
    /*
    public class GameloopPolylineInflationTest : GameLoop
    {
        ScreenTexture game;
        Polyline polyline = new();
        int dragIndex = -1;
        float offsetDelta = 0f;
        float lerpOffsetDelta = 0f;
        public GameloopPolylineInflationTest() : base()
        {
            game = AddScreenTexture(1920, 1080, 0);
            game.SetCamera(new BasicCamera(new Vector2(0f), new Vector2(1920, 1080), new Vector2(0.5f), 1f, 0f));
        }

        protected override void HandleInput(float dt)
        {
            if(IsKeyPressed(KeyboardKey.KEY_F))
            {
                ToggleWindowMaximize();
            }
            if(IsKeyPressed(KeyboardKey.KEY_R)) { polyline = new(); }

            if (GetMouseWheelMove() > 0)
            {
                offsetDelta += 10f;
            }
            else if (GetMouseWheelMove() < 0)
            {
                offsetDelta -= 10f;
            }

            lerpOffsetDelta = Lerp(lerpOffsetDelta, offsetDelta, dt * 2f);

            offsetDelta = Clamp(offsetDelta, 0f, 300f);
        }

        protected override void Draw(ScreenTexture screenTexture)
        {
            Vector2 mousePos = screenTexture.MousePos;

            float vertexRadius = 8f;
            int pickedVertex = -1;

            bool isMouseOnLine = false; // polyline.OverlapShape(new Circle(mousePos, vertexRadius * 2f));
            var closest = polyline.GetClosestPoint(mousePos);
            int closestIndex = polyline.GetClosestIndex(mousePos);
            bool drawClosest = true;

            

            


            for (int i = 0; i < polyline.Count; i++)
            {
                var p = polyline[i];
                float disSq = (mousePos - p).LengthSquared();
                if (pickedVertex == -1 && disSq < (vertexRadius * vertexRadius) * 2f)
                {
                    DrawCircleV(p, vertexRadius * 2f, GREEN);
                    pickedVertex = i;
                }
                else DrawCircleV(p, vertexRadius, GRAY);
                if (drawClosest)
                {
                    disSq = (closest - p).LengthSquared();
                    if (disSq < (vertexRadius * vertexRadius) * 4f)
                    {
                        drawClosest = false;
                    }
                }
                
            }

            if (drawClosest)
            {
                float disSq = (closest - mousePos).LengthSquared();

                float tresholdSq = 30 * 30;
                if (disSq > tresholdSq)
                {
                    drawClosest = false;
                }
                else isMouseOnLine = true;
            }


            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                if(pickedVertex == -1)
                {
                    if(isMouseOnLine)
                    {
                        polyline.Insert(closestIndex + 1, mousePos);
                    }
                    else polyline.Add(mousePos);

                }
                else
                {
                    dragIndex = pickedVertex;
                }
            }
            else if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dragIndex = -1;
            }
            else if(dragIndex == -1 && IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                if (pickedVertex > -1)
                {
                    polyline.RemoveAt(pickedVertex);
                }
            }

            if(dragIndex > -1) polyline[dragIndex] = mousePos;

            //polyline.Draw(4f, WHITE);
            var segments = polyline.GetEdges();
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];

                if (drawClosest)
                {
                    if (closestIndex == i) segment.Draw(4f, BLUE);
                    else segment.Draw(4f, WHITE);
                }
                else segment.Draw(4f, WHITE);
                
                

            }
            
            if(drawClosest) DrawCircleV(closest, vertexRadius, RED);

            if(lerpOffsetDelta > 10f)
            {
                var polygons = SClipper.Inflate(polyline, lerpOffsetDelta).ToPolygons();
                foreach (var polygon in polygons)
                {
                    polygon.DrawLines(3f, GOLD);
                }
            }
            
        }

    }

    public class GameloopTriangulationTest : GameLoopScene
    {
        Polygon shape;
        Triangulation triangles = new();
        float fractureAreaFactor = 0.03f;
        //List<Vector2> randomPoints = new();
        //Triangle t;

        public GameloopTriangulationTest(int gameTextureWidth, int gameTextureHeight, int uiTextureWidth, int uiTextureHeight) : base(gameTextureWidth, gameTextureHeight, uiTextureWidth, uiTextureHeight)
        {
            shape = GeneratePolygon();

            //extra func that uses min area instead of subdivision -> subdivide triangles until all triangles have an area <= minArea.
            triangles = shape.Fracture(fractureAreaFactor);
            UI.SetCamera(new BasicCamera(new Vector2(0f), new Vector2(gameTextureWidth, gameTextureHeight), new Vector2(0.5f), 1f, 0f));

            //triangles.RemoveNarrow(0.01f);
            //Vector2 center = UI.GetSize() / 2;
            //Vector2 a = center + new Vector2(0, -250f);
            //Vector2 b = center + new Vector2(-250, 100f);
            //Vector2 c = center + new Vector2(250, 100f);
            //
            //t = new(a, b, c);
            //randomPoints = t.GetRandomPointsOnEdge(120);
        }
        protected override void HandleInput(float dt)
        {
            base.HandleInput(dt);
            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                shape = GeneratePolygon();
                triangles = shape.Fracture(fractureAreaFactor);
                //randomPoints = shape.GetRandomPoints(480);

                //randomPoints = t.GetRandomPoints(90);

            }
            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                triangles = shape.Fracture(fractureAreaFactor);
                //triangles.RemoveNarrow(0.1f);
                //randomPoints = t.GetRandomPointsOnEdge(120);
            }
            if (IsKeyPressed(KeyboardKey.KEY_F))
            {
                ToggleWindowMaximize();

                //if(IsWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE))
                //{
                //    ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
                //    SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
                //    ResizeWindow(1920, 1080);
                //}
                //else
                //{
                //    ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
                //    SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
                //    ResizeWindow(1920/2, 1080/2);
                //}
                
                //ToggleFullscreen();
            }
        }
        private Polygon GeneratePolygon() 
        { 
            var poly = SPoly.Generate(new Vector2(0f), 24, 100, 400);
            return SClipper.Simplify(poly, 10f, false).ToPolygon();
        }
        //private Polygon GeneratePolygon() { return SPoly.Generate(UI.GetSize() / 2, 48, 100, 400); }
        protected override void Draw(ScreenTexture screenTexture)
        {
            base.Draw(screenTexture);
            if(screenTexture == UI)
            {
                Vector2 mousePos = UI.MousePos;
                Vector2 center = new Vector2(0f); // UI.GetSize() / 2;
                float f = Clamp(mousePos.X / UI.GetSize().X, 0f, 1f);
                
                Triangulation exploded = new();
                foreach(var tri in triangles)
                {
                    Vector2 centroid = tri.GetCentroid();
                    Vector2 dir = (centroid - center);//.Normalize();
                    exploded.Add(tri.Move(dir * f * 2));
                }


                if (exploded.Count > 0 && f > 0.01f) 
                {
                    //exploded.DrawLines(2f * f, RED);
                    //Color c = SColor.Lerp(new(255, 0, 0, 150), new(255, 0, 0, 255), f);
                    //exploded.Draw(c);
                    exploded.DrawLines(3f, RED);

                }

                if(f < 0.1f)
                {
                    shape.DrawLines(4f, WHITE);
                    //shape.DrawVertices(8f, YELLOW);

                }

                //Vector2 a = mousePos;
                //Vector2 b = center + new Vector2(-250, 100f);
                //Vector2 c = center + new Vector2(250, 100f);
                //
                //t = new(a, b, c);
                //
                //t.DrawLines(2f, WHITE);
                
                //foreach(var p in randomPoints)
                //{
                //    DrawCircleV(p, 6f, GREEN);
                //}

                





                //shape.DrawLines(2f, WHITE);
                //
                //var r = shape.GetBoundingBox();
                //r.DrawLines(4f, GREEN);
                //
                //var t = shape.GetBoundingTriangle(0f);
                //t.DrawLines(4f, RED);
                //
                //var c = t.GetCircumCircle();
                //c.DrawLines(4f, BLUE);
            }
        }
        protected override void DrawToScreen(Vector2 screenSize)
        {

            //var shape = SPoly.GetShape(basePoly, screenSize / 2, 0f, new(1f));
            //shape.DrawLines(4f, WHITE);
        }
    }

    public class GameloopCollisionTest : GameLoopScene
    {

        int moverShapeIndex = 0;
        int subjectShapeIndex = 0;
        int maxShapes = 6;
        Vector2 randDirMover = SRNG.randVec2();
        Vector2 randDirSubject = SRNG.randVec2();

        Polygon subjectPoints = SPoly.Generate(12, 125, 200);
        Polygon moverPoints = SPoly.Generate(12, 25, 50);

        public GameloopCollisionTest(int gameTextureWidth, int gameTextureHeight, int uiTextureWidth, int uiTextureHeight) : base(gameTextureWidth, gameTextureHeight, uiTextureWidth, uiTextureHeight)
        {

        }
        protected override void HandleInput(float dt)
        {
            base.HandleInput(dt);
            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                moverShapeIndex = (moverShapeIndex + 1) % maxShapes;
                randDirMover = SRNG.randVec2();
                moverPoints = SPoly.Generate(12, 25, 50);
            }
            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                subjectShapeIndex = (subjectShapeIndex + 1) % maxShapes;
                randDirSubject = SRNG.randVec2();
                subjectPoints = SPoly.Generate(12, 125, 200);
            }

        }
        private IShape GetShape(int shapeIndex, Vector2 pos, float size, bool mover)
        {
            if (shapeIndex == 0)//Segment
            {
                Vector2 dir = mover ? randDirMover : randDirSubject;
                return new Segment(pos - dir * size, pos + dir * size);
            }
            else if (shapeIndex == 1)//Circle
            {
                return new Circle(pos, size);
            }
            else if (shapeIndex == 2)//Triangle
            {
                return new Triangle(pos + new Vector2(0, -size), pos + new Vector2(-size / 2, size), pos + new Vector2(size / 2, size));
            }
            else if (shapeIndex == 3)//Rect
            {
                return new Rect(pos, new Vector2(size, size) * 2, new Vector2(0.5f, 0.5f));
            }
            else if (shapeIndex == 4)//Polygon
            {
                return SPoly.GetShape(mover ? moverPoints : subjectPoints, pos, 0f, new(1f)); // SPoly.Generate(pos, 12, size / 2, size);
            }
            else if (shapeIndex == 5)//Polyline
            {
                return SPoly.GetShape(mover ? moverPoints : subjectPoints, pos, 0f, new(1f)).ToPolyline();
            }
            else return new Circle(pos, size);
        }
        protected override void DrawToScreen(Vector2 screenSize)
        {
            IShape mover = GetShape(moverShapeIndex, MousePos, 50f, true);
            IShape subject = GetShape(subjectShapeIndex, screenSize / 2, 200f, false);
            
            

            bool overlapping = mover.Overlap(subject);
            if (overlapping)
            {
                var intersection = mover.Intersect(subject);
                if (intersection.valid)
                {
                    mover.DrawShape(2f, GREEN);
                    subject.DrawShape(2f, GREEN);
                    foreach (var i in intersection.points)
                    {
                        DrawCircleV(i.p, 5f, RED);
                        DrawLineEx(i.p, i.p + i.n * 50f, 1f, RED);
                    }
                }
                else
                {
                    mover.DrawShape(2f, BLUE);
                    subject.DrawShape(2f, BLUE);
                }
                
            }
            else
            {
                mover.DrawShape(2f, WHITE);
                subject.DrawShape(2f, WHITE);
            }


            //Circle mover = new Circle(MousePos, 75);
            //Circle subject = new(screenSize / 2, 150);
            //bool overlapping = mover.OverlapShape(subject);
            //if(overlapping)
            //{
            //    var intersection = mover.IntersectShape(subject);
            //    if (intersection.valid)
            //    {
            //        subject.DrawLines(2f, GREEN);
            //        mover.DrawLines(2f, GREEN);
            //        foreach (var i in intersection.points)
            //        {
            //            DrawCircleV(i.p, 5f, RED);
            //            DrawLineEx(i.p, i.p + i.n * 50f, 1f, RED);
            //        }
            //    }
            //    else
            //    {
            //        subject.DrawLines(2f, BLUE);
            //        mover.DrawLines(2f, BLUE);
            //    }
            //}
            //else
            //{
            //    subject.DrawLines(2f, WHITE);
            //    mover.DrawLines(2f, WHITE);
            //}

            
            

        }
    }
    */
    /*
    public class GameloopTest : GameLoop
    {
        ScreenTexture uiTexture;
        ScreenTexture gameTexture;
        BasicCamera gameCamera;
        BasicCamera uiCamera;
        Vector2 worldPos = new(0f);
        public GameloopTest()
        {
            uiTexture = AddScreenTexture(1920, 1080, 1);
            uiCamera = new BasicCamera(new(0f), new(1920, 1080), new(0.5f), 1f, 0f);
            uiTexture.SetCamera(uiCamera);
            

            gameTexture = AddScreenTexture(1920/4, 1080/4, 0);
            //gameTexture.BackgroundColor = new(50, 0, 0, 50);
            gameCamera = new BasicCamera(new(0f), gameTexture.GetSize(), new(0.5f, 0.5f), 1f, 0f);
            gameTexture.SetCamera(gameCamera);
        }

        protected override void BeginRun()
        {
            //var st = AddScreenTexture(1920/4, 1080/4, 0);
            //st = AddScreenTexture(400, 400, 0);
            //st.BackgroundColor = new(100, 0, 0, 100);
        }
        protected override void HandleInput(float dt)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_Z))
            {
                gameCamera.Zoom = 1f;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_X))
            {
                gameCamera.Zoom -= 0.1f;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_C))
            {
                gameCamera.Zoom += 0.1f;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_B))
            {
                uiCamera.Zoom = 1f;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_N))
            {
                uiCamera.Zoom -= 0.1f;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_M))
            {
                uiCamera.Zoom += 0.1f;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_E))
            {
                //gameCamera.Translation = new Vector2(0f);
                gameCamera.AdjustOrigin(new(0.5f, 0.5f));
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_UP))
            {
                //gameCamera.Translation += new Vector2(0, 100f) * dt;
                //gameCamera.AdjustOrigin(gameCamera.Origin + new Vector2(0, 0.1f) * dt);
                gameCamera.AdjustOrigin(new Vector2(gameCamera.Origin.X, 0.25f));
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
            {
                //gameCamera.Translation += new Vector2(-100, 0f) * dt;
                //gameCamera.AdjustOrigin(gameCamera.Origin + new Vector2(-0.1f, 0f) * dt);
                gameCamera.AdjustOrigin(new Vector2(0.25f, gameCamera.Origin.Y));
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
            {
                //gameCamera.Translation += new Vector2(0, -100f) * dt;
                //gameCamera.AdjustOrigin(gameCamera.Origin + new Vector2(0, -0.1f) * dt);
                gameCamera.AdjustOrigin(new Vector2(gameCamera.Origin.X, 0.75f));
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
            {
                //gameCamera.Translation += new Vector2(100, 0f) * dt;
                //gameCamera.AdjustOrigin(gameCamera.Origin + new Vector2(0.1f, 0f) * dt);
                gameCamera.AdjustOrigin(new Vector2(0.75f, gameCamera.Origin.Y));
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_Q))
            {
                worldPos = new Vector2(0f);
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
            {
                worldPos += new Vector2(0, -100f) * dt;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
            {
                worldPos += new Vector2(-100, 0f) * dt;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
            {
                worldPos += new Vector2(0, 100f) * dt;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
            {
                worldPos += new Vector2(100, 0f) * dt;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_F))
            {
                ToggleFullscreen();
            }
        }
        protected override void Update(float dt)
        {
            gameCamera.Position = worldPos;
            //gameCamera.Update(dt);
            //uiCamera.Update(dt);
        }
        protected override void Draw(ScreenTexture screenTexture)
        {

            if(screenTexture == gameTexture)
            {
                DrawRectangleLinesEx(new(-screenTexture.GetTextureWidth()/2, -screenTexture.GetTextureHeight()/2, screenTexture.GetTextureWidth(), screenTexture.GetTextureHeight()), 8f, new(150, 0, 0, 150));
                gameCamera.GetArea().DrawLines(4f, RED);
                DrawCircleV(screenTexture.MousePos, 10, RED);
                DrawCircleV(worldPos, 20, RED);
                
                //DrawCircleV(screenTexture.WorldToScreen(worldPos), 15, GREEN);
                //DrawCircleV(screenTexture.ScreenToWorld(worldPos), 15, BLUE);
            }
            else
            {
                uiCamera.GetArea().DrawLines(4f, BLUE); // DrawRectangleLinesEx(new(0, 0, screenTexture.GetTextureWidth(), screenTexture.GetTextureHeight()), 2f, BLUE);
                DrawCircleV(screenTexture.MousePos, 25, BLUE);
            
                Vector2 uiPos = gameTexture.ScalePosition(worldPos, uiTexture);
                //uiPos += new Vector2(0, -20*gameTexture.GetScaleFactorTo(uiTexture)*gameCamera.GetZoomFactor());
                uiPos += gameTexture.ScaleVector(new(0, -20), uiTexture);
                //25 * gameCamera.GetZoomFactor() * uiCamera.GetZoomFactorInverse()
                DrawCircleV(uiPos, uiTexture.AdjustToZoom(25, gameTexture), BLUE);

                DrawCircleV(new(400, 400), 50, GOLD);
                //DrawCircleV(new(400f,100f), 50, GREEN);
            }
            
        }
        protected override void DrawToScreen(Vector2 screenSize)
        {
            DrawCircleV(MousePos, 5, GREEN);

            //DrawCircleV(screenSize * new Vector2(0.2f, 0.7f), 50, PURPLE);
        }

    }

    */
    /*
    public class CollisionTest : Scene
    {

        private Area area = new Area(0, 0, 1000, 1000, 12, 12);
        private CameraBasic camera;
        
        public CollisionTest()
        {
            camera = new(new(0f), Gameloop.GFX.GameSize(), 1f, 0f);
            Gameloop.GFX.Camera = camera;
            
        }

        public override Area? GetCurArea() { return area; }

        public override void HandleInput() 
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_Q))
            {
                camera.ResetTranslation();
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_W))
            {
                camera.Translation += new Vector2(0, -10f);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_A))
            {
                camera.Translation += new Vector2(-10, 0f);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
            {
                camera.Translation += new Vector2(0, 10f);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_D))
            {
                camera.Translation += new Vector2(10, 0f);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE))
            {
                camera.ZoomFactor += 0.25f;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_TWO))
            {
                camera.ZoomFactor -= 0.25f;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_THREE))
            {
                camera.ResetZoom();
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_FOUR))
            {
                camera.RotationDeg += 22.5f;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_FIVE))
            {
                camera.RotationDeg -= 22.5f;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SIX))
            {
                camera.ResetRotation();
            }
        }
        public override void Update(float dt, Vector2 mousePosGame) { }

        public override void Draw(Vector2 mousePosGame)
        {
            area.Draw();
            SDrawing.DrawCircleLines(new Vector2(0f), 150, 5f, GREEN, 8f);
            SDrawing.DrawCircle(mousePosGame, 150, GREEN);

            camera.GetArea().DrawLines(4f, BLUE);
        }

        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI) 
        {
            area.DrawUI(uiSize);
            SDrawing.DrawCircleLines(new(400, 400), 100, 5f, RED, 8f);
            SDrawing.DrawCircle(mousePosUI, 100, RED);
        }
    }
    public class Game : GameLoop
    {
        public override void BeginRun()
        {
            //HideCursor();
            this.GoToScene(new CollisionTest());
        }
    }
    */
}