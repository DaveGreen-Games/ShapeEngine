using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore;
using ShapeEngineCore.Globals.UI;
using ShapeEngineCore.Globals.Screen;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Cursor;
using ShapeEngineCore.Globals.Audio;

namespace ShapeEngineDemo
{
    public class MainMenu : Scene
    {
        //MazeGenerator.Maze maze;
        //MazeGenerator.MazeDrawer mazeDrawer;
        ButtonLabel level1Button, quitButton, optionsButton;
        public MainMenu()
        {
            //maze = new(16, 9);
            //mazeDrawer = new(maze, new(SCREEN_AREA.X + SCREEN_AREA.width * 0.1f, SCREEN_AREA.Y + SCREEN_AREA.height * 0.1f), new(SCREEN_AREA.width * 0.8f, SCREEN_AREA.height * 0.8f), 2);
            //maze.GenerateDeadCells(50);
            //maze.Generate(MazeGenerator.Maze.GenerationType.PRIM_SIMPLE, -1, 1);
            Vector2 center = new(ScreenHandler.DEVELOPMENT_RESOLUTION.width / 2, ScreenHandler.DEVELOPMENT_RESOLUTION.height / 2);
            Vector2 size = new Vector2 (300, 100);
            Vector2 offset = new Vector2(0, 110);
            level1Button = new("START", "medium", center, size, true);
            optionsButton = new("OPTIONS", "medium", center + offset, size, true);
            quitButton = new("QUIT", "medium", center + offset * 2, size, true);
            quitButton.AddShortcut("UI Cancel");

            level1Button.SetStateColors(new(PaletteHandler.C("bg2"), PaletteHandler.C("energy"), PaletteHandler.C("player"), PaletteHandler.C("sepcial1"), PaletteHandler.C("neutral")));
            level1Button.SetTextStateColors(new(PaletteHandler.C("text"), PaletteHandler.C("bg2"), PaletteHandler.C("player"), PaletteHandler.C("b1"), PaletteHandler.C("enemy")));

            optionsButton.SetStateColors(new(PaletteHandler.C("bg2"), PaletteHandler.C("energy"), PaletteHandler.C("player"), PaletteHandler.C("sepcial1"), PaletteHandler.C("neutral")));
            optionsButton.SetTextStateColors(new(PaletteHandler.C("text"), PaletteHandler.C("bg2"), PaletteHandler.C("player"), PaletteHandler.C("b1"), PaletteHandler.C("enemy")));

            quitButton.SetStateColors(new(PaletteHandler.C("bg2"), PaletteHandler.C("energy"), PaletteHandler.C("player"), PaletteHandler.C("sepcial1"), PaletteHandler.C("neutral")));
            quitButton.SetTextStateColors(new(PaletteHandler.C("text"), PaletteHandler.C("bg2"), PaletteHandler.C("player"), PaletteHandler.C("b1"), PaletteHandler.C("enemy")));

            optionsButton.Disable();
            //startButton.AddShortcut(KeyboardKey.KEY_Q);
            //startButton.SetNeighbor(quitButton, UINeighbors.NeighborDirection.BOTTOM);
            //startButton.SetNeighbor(quitButton, UINeighbors.NeighborDirection.TOP);
            //quitButton.SetNeighbor(startButton, UINeighbors.NeighborDirection.BOTTOM);
            //quitButton.SetNeighbor(startButton, UINeighbors.NeighborDirection.TOP);
            UIHandler.SelectUIElement(level1Button);
        }

        public override void Activate(Scene? oldScene)
        {
            base.Activate(oldScene);
            UIHandler.RegisterUIElement(level1Button);
            UIHandler.RegisterUIElement(optionsButton);
            UIHandler.RegisterUIElement(quitButton);
            CursorHandler.Switch("ui");
            GAMELOOP.backgroundColor = PaletteHandler.C("bg2");
            GAMELOOP.RemoveScene("level1");
            //AudioHandler.SwitchPlaylist("menu");
        }
        public override void Deactivate(Scene? newScene)
        {
            base.Deactivate(newScene);
            UIHandler.UnregisterUIElement(level1Button);
            UIHandler.UnregisterUIElement(optionsButton);
            UIHandler.UnregisterUIElement(quitButton);
        }
        public override void Update(float dt)
        {
            level1Button.Update(dt, GAMELOOP.MOUSE_POS_UI_RAW);
            optionsButton.Update(dt, GAMELOOP.MOUSE_POS_UI_RAW);
            quitButton.Update(dt, GAMELOOP.MOUSE_POS_UI_RAW);
            if (level1Button.Clicked())
            {
                GAMELOOP.AddScene("level1", new Level());
                GAMELOOP.GoToScene("level1");
            }
            //if (optionsButton.Clicked()) GAMELOOP.GoToScene("level2");
            if (quitButton.Clicked()) GAMELOOP.QUIT = true;
        }
        public override void Draw()
        {
            //mazeDrawer.Draw();
        }
        public override void DrawUI(Vector2 devRes, Vector2 stretchFactor)
        {
            Rectangle uiArea = ScreenHandler.UIArea();
            DrawRectangleRec(uiArea, PaletteHandler.C("bg1"));
            
            UIHandler.DrawTextAligned("MAIN MENU", devRes * new Vector2(0.5f, 0.21f) * stretchFactor, devRes * new Vector2(0.5f, 0.5f) * stretchFactor, 1, PaletteHandler.C("bg2"), "bold");
            UIHandler.DrawTextAligned("MAIN MENU", devRes * new Vector2(0.5f, 0.2f) * stretchFactor, devRes * new Vector2(0.5f, 0.5f) * stretchFactor, 1, PaletteHandler.C("header"), "bold");
            
            
            
            UIHandler.DrawTextAligned2(ShapeEngine.EDITORMODE == true ? "EDITOR" : "STANDALONE", 
                devRes * new Vector2(0.05f, 0.05f) * stretchFactor, devRes.X * 0.05f * stretchFactor.X, 5, WHITE, Alignement.LEFTCENTER);

            //UIHandler.DrawTextAligned(String.Format("Size: {0}", ScreenHandler.UISize()), new Vector2(100f, 180f), 50, 5, WHITE, Alignement.LEFTCENTER);
            //UIHandler.DrawTextAligned(String.Format("Dev Res: {0}", devRes), new Vector2(100f, 260f), 50, 5, WHITE, Alignement.LEFTCENTER);
            //UIHandler.DrawTextAligned(String.Format("Target Res: {0}", ScreenHandler.UI.TARGET_RESOLUTION), new Vector2(100f, 340f), 50, 5, WHITE, Alignement.LEFTCENTER);
            //UIHandler.DrawTextAligned(String.Format("Stretch F: {0}", stretchFactor), new Vector2(100f, 420f), 50, 5, WHITE, Alignement.LEFTCENTER);
            //
            ////UIHandler.DrawTextAligned(String.Format("Area F: {0}", ScreenHandler.UI.STRETCH_AREA_FACTOR), new Vector2(100f, 420f), 50, 5, WHITE, Alignement.LEFTCENTER);
            ////UIHandler.DrawTextAligned(String.Format("Area Side F: {0}", ScreenHandler.UI.STRETCH_AREA_SIDE_FACTOR), new Vector2(100f, 500f), 50, 5, WHITE, Alignement.LEFTCENTER);
            //
            //UIHandler.DrawTextAligned(String.Format("Button Pos: {0}", level1Button.GetCenter()), new Vector2(100f, 580f), 50, 5, WHITE, Alignement.LEFTCENTER);
            //UIHandler.DrawTextAligned(String.Format("Button Scaled Pos: {0}", level1Button.GetCenter() * stretchFactor), new Vector2(100f, 660f), 50, 5, WHITE, Alignement.LEFTCENTER);
            level1Button.Draw(devRes, stretchFactor);
            optionsButton.Draw(devRes, stretchFactor);
            quitButton.Draw(devRes, stretchFactor);


            //Vector2 barSize = devRes * new Vector2(0.5f, 0.05f) * stretchFactor;
            //Vector2 center = devRes * new Vector2(0.5f, 0.3f) * stretchFactor;
            //Vector2 topleft = center - barSize / 2;
            //UIHandler.DrawBar(topleft, barSize, RNG.randF(), RED, DARKPURPLE, BarType.LEFTRIGHT);


        }
    }


}
