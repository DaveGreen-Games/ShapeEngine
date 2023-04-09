using System.Numerics;
using Raylib_CsLo;
using ShapeUI;
using ShapeScreen;
using ShapeCore;
using ShapeLib;
using ShapeInput;

namespace ShapeEngineDemo
{
    public class VContainer : BoxContainer
    {
        public VContainer(float gapRelative, params UIElement[] elements) : base(elements)
        {
            this.GapRelative = gapRelative;
            this.VContainer = true;
        }
        public override void Draw()
        {
            if(Selected && !hidden) SDrawing.DrawRectangeLinesPro(GetRect(), new(0f), 0f, 8f, RED);
            base.Draw();
        }
    }
    public class HContainer : BoxContainer
    {
        public HContainer(float gapRelative, params UIElement[] elements) : base(elements)
        {
            this.GapRelative = gapRelative;
            this.VContainer = false;
        }
        public override void Draw()
        {
            if (Selected && !hidden) SDrawing.DrawRectangeLinesPro(GetRect(), new(0f), 0f, 8f, RED);
            base.Draw();
        }
    }
    public class MainMenu : Scene
    {
        //TestButton b1, b2, b3;
        //UINavigator nav;


        ///List<TestButton> b1, b2, b3;
        VContainer c1, c2, c3;
        BoxContainer mainContainer;

        public MainMenu()
        {
            var font = Demo.FONT.GetFont(Demo.FONT_Medium);

            //b1 = new("START",   font, InputIDs.UI_Pressed, InputIDs.UI_MousePressed, -1);
            //b2 = new("OPTIONS", font, InputIDs.UI_Pressed, InputIDs.UI_MousePressed, -1);
            //b3 = new("QUIT",    font, InputIDs.UI_Pressed, InputIDs.UI_MousePressed, InputIDs.UI_Cancel);
            //b1.Neighbors.SetNeighbor(b3, UINeighbors.NeighborDirection.TOP);
            //b3.Neighbors.SetNeighbor(b1, UINeighbors.NeighborDirection.BOTTOM);
            //nav = new(b1, b2, b3);
            //nav.StartNavigation();

            List<TestButton> b1 = new();
            for (int i = 0; i < 24; i++)
            {
                TestButton b = new(String.Format("A{0}", i + 1), font, InputIDs.UI_Pressed, InputIDs.UI_MousePressed, - 1);
                if (SRNG.chance(0.25f))
                {
                    //b.Hidden = true;
                    b.DisabledSelection = true;
                }
                b1.Add(b);
            }
            c1 = new(0.01f, b1.ToArray());
            c1.DisplayCount = 12;

            List<TestButton> b2 = new();
            for (int i = 0; i < 24; i++)
            {
                TestButton b = new(String.Format("B{0}", i + 1), font, InputIDs.UI_Pressed, InputIDs.UI_MousePressed, -1);
                if (SRNG.chance(0.25f))
                {
                    //b.Hidden = true;
                    b.DisabledSelection = true;
                }
                b2.Add(b);
            }
            c2 = new(0.01f, b2.ToArray());
            c2.DisplayCount = 12;

            List<TestButton> b3 = new();
            for (int i = 0; i < 24; i++)
            {
                TestButton b = new(String.Format("C{0}", i + 1), font, InputIDs.UI_Pressed, InputIDs.UI_MousePressed, -1);
                if (SRNG.chance(0.25f))
                {
                    //b.Hidden = true;
                    b.DisabledSelection = true;
                }
                b3.Add(b);
            }
            c3 = new(0.01f, b3.ToArray());
            c3.DisplayCount = 12;

            mainContainer = new(c1, c2, c3);
            mainContainer.GapRelative = 0.05f;
            mainContainer.VContainer = false;
            mainContainer.Select();
        }


        public override void Start()
        {

        }
        public override void Activate(Scene? oldScene)
        {
            base.Activate(oldScene);
            Demo.CURSOR.Switch("ui");
            GAMELOOP.backgroundColor = Demo.PALETTES.C(ColorIDs.Background2);
            GAMELOOP.RemoveScene("level1");
        }
        public override void Deactivate(Scene? newScene)
        {
            base.Deactivate(newScene);
        }
        public override void Update(float dt)
        {
            Vector2 uiSize = ScreenHandler.UISize();
            Rectangle r = SRect.ConstructRect(uiSize *0.5f, uiSize * new Vector2(0.8f, 0.8f), new Vector2(0.5f, 0.5f));
            //UIContainer.AlignUIElementsHorizontal(r, mainContainer.DisplayedElements, mainContainer.DisplayCount, 0.01f, 1, 1);
            mainContainer.UpdateRect(r);
            mainContainer.Update(dt, GAMELOOP.MOUSE_POS_UI);
            //UIContainer.AlignUIElementsGrid(r, mainContainer.DisplayedElements, 5, 5, 0.01f, 0.01f, true);

            UINeighbors.NeighborDirection dir = UINeighbors.NeighborDirection.NONE;
            if (InputHandler.IsDown(0, InputIDs.UI_Down)) dir = UINeighbors.NeighborDirection.BOTTOM;
            else if (InputHandler.IsDown(0, InputIDs.UI_Up)) dir = UINeighbors.NeighborDirection.TOP;
            else if (InputHandler.IsDown(0, InputIDs.UI_Left))  dir = UINeighbors.NeighborDirection.LEFT;
            else if (InputHandler.IsDown(0, InputIDs.UI_Right))  dir = UINeighbors.NeighborDirection.RIGHT;
            bool used = mainContainer.Navigate(dir);
            if(!used && mainContainer.SelectedElement is UIContainer c)
            {
                c.Navigate(dir);
                if (InputHandler.IsReleased(0, 2000))       c.MoveNext();
                else if (InputHandler.IsReleased(0, 2001))  c.MovePrevious();
                else if (InputHandler.IsReleased(0, 2002))  c.MoveNextPage();
                else if (InputHandler.IsReleased(0, 2003))  c.MovePreviousPage();

                if(c.SelectedElement != null)
                {
                    if (c.SelectedElement.Released)
                    {
                        c.SelectedElement.Hidden = true;
                    }
                }
            }

            //if (c2.Released) c2.Hidden = true;
            

            //for (int i = 0; i < testButtons.Count; i++)
            //{
            //    var b = testButtons[i];
            //    if(b.Released)
            //    {
            //        
            //        //List<TestButton> disabled = testButtons.FindAll(m => m.Disabled);
            //        //if(disabled.Count > 0) disabled[SRNG.randI(0, disabled.Count)].Disabled = false;
            //        //b.Disabled = true;
            //        b.Hidden = true;
            //    }
            //}
            //Vector2 uiSize = ScreenHandler.UISize();
            //Vector2 center = uiSize * 0.5f;
            //Vector2 size = uiSize * new Vector2(0.2f, 0.1f);
            //Vector2 offset = new Vector2(0, size.Y * 1.1f);
            //
            //b1.UpdateRect(center, size, new(0.5f));
            //b2.UpdateRect(center + offset, size, new(0.5f));
            //b3.UpdateRect(center + offset * 2, size, new(0.5f));
            //b1.Update(dt, GAMELOOP.MOUSE_POS_UI);
            //b2.Update(dt, GAMELOOP.MOUSE_POS_UI);
            //b3.Update(dt, GAMELOOP.MOUSE_POS_UI);
            //
            //UINeighbors.NeighborDirection dir = UINeighbors.NeighborDirection.NONE;
            //if (InputHandler.IsDown(0, InputIDs.UI_Down)) dir = UINeighbors.NeighborDirection.BOTTOM;
            //else if (InputHandler.IsDown(0, InputIDs.UI_Up)) dir = UINeighbors.NeighborDirection.TOP;
            //nav.Navigate(dir);
            //nav.Update(dt);
            //
            //if (b1.Released)
            //{
            //    GAMELOOP.AddScene("level1", new Level());
            //    GAMELOOP.GoToScene("level1");
            //}
            //if (b3.Released)
            //{
            //    GAMELOOP.QUIT = true;
            //}

        }
        public override void Draw()
        {
            //mazeDrawer.Draw();
        }
        public override void DrawUI(Vector2 uiSize, Vector2 stretchFactor)
        {

            Rectangle uiArea = ScreenHandler.UIArea();
            DrawRectangleRec(uiArea, Demo.PALETTES.C(ColorIDs.Background1));
            //Rectangle r = SRect.ConstructRect(uiSize * 0.5f, uiSize * new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            //DrawRectangleRec(r, RED);
            
            SDrawing.DrawTextAligned("MAIN MENU", uiSize * new Vector2(0.5f, 0.21f), uiSize * new Vector2(0.5f, 0.5f), 1, Demo.PALETTES.C(ColorIDs.Background2), Demo.FONT.GetFont(Demo.FONT_Huge), new(0.5f));
            SDrawing.DrawTextAligned("MAIN MENU", uiSize * new Vector2(0.5f, 0.2f), uiSize * new Vector2(0.5f, 0.5f), 1, Demo.PALETTES.C(ColorIDs.Header), Demo.FONT.GetFont(Demo.FONT_Huge), new(0.5f));
            
            
            
            SDrawing.DrawTextAligned2(ShapeEngine.EDITORMODE == true ? "EDITOR" : "STANDALONE", 
                uiSize * new Vector2(0.01f, 0.03f), uiSize.X * 0.05f, 1, WHITE, Demo.FONT.GetFont(Demo.FONT_Medium), new(0,0.5f));

            Vector2 start = uiSize * new Vector2(0.01f, 0.08f);
            Vector2 gap = uiSize * new Vector2(0, 0.04f);
            Vector2 textSize = uiSize * new Vector2(0.2f, 0.05f);
            var font = Demo.FONT.GetFont(Demo.FONT_Medium);
            SDrawing.DrawTextAligned(String.Format("UI Size: {0}", ScreenHandler.UISize()), start, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("Dev Res: {0}", ScreenHandler.DEVELOPMENT_RESOLUTION), start + gap, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("Target Res: {0}", ScreenHandler.UI.TARGET_RESOLUTION), start + gap * 2, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("Win Size: {0}", ScreenHandler.CUR_WINDOW_SIZE), start + gap * 3, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("Stretch F: {0}", stretchFactor), start + gap * 4, textSize, 1, WHITE, font, new(0, 0.5f));

            if(ShapeEngine.IsWindows()) SDrawing.DrawTextAligned("Windows", start + gap * 5, textSize, 1, WHITE, font, new(0, 0.5f));
            if (ShapeEngine.IsLinux()) SDrawing.DrawTextAligned("Linux", start + gap * 5, textSize, 1, WHITE, font, new(0, 0.5f));
            if (ShapeEngine.IsOSX()) SDrawing.DrawTextAligned("OSX", start + gap * 5, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("FPS: {0}", GetFPS()), start + gap * 6, textSize, 1, GREEN, font, new(0, 0.5f));
            mainContainer.Draw();
            //b1.Draw();
            //b2.Draw();
            //b3.Draw();
        }
    }


}
