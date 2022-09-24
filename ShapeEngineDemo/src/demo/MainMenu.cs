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
            Vector2 center = new(ScreenHandler.UIWidth() / 2, ScreenHandler.UIHeight() / 2);
            Vector2 size = UIHandler.Scale(new Vector2 (350, 100));
            Vector2 offset = UIHandler.Scale(new Vector2(0, 115));
            float fontSize = UIHandler.GetFontSizeScaled(FontSize.HUGE);
            level1Button = new("START", fontSize, "medium", center, size, true);
            optionsButton = new("OPTIONS", fontSize, "medium", center + offset, size, true);
            quitButton = new("QUIT", fontSize, "medium", center + offset * 2, size, true);
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
            level1Button.Update(dt, GAMELOOP.MOUSE_POS_UI);
            optionsButton.Update(dt, GAMELOOP.MOUSE_POS_UI);
            quitButton.Update(dt, GAMELOOP.MOUSE_POS_UI);
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
        public override void DrawUI()
        {
            Rectangle uiArea = ScreenHandler.UIArea();
            DrawRectangleRec(uiArea, PaletteHandler.C("bg1"));
            UIHandler.DrawTextAligned("MAIN MENU", new(uiArea.x + uiArea.width / 2, uiArea.y + uiArea.height * 0.21f), FontSize.HEADER_XL, 15, PaletteHandler.C("bg2"), "bold");
            UIHandler.DrawTextAligned("MAIN MENU", new(uiArea.x + uiArea.width / 2, uiArea.y + uiArea.height * 0.2f), FontSize.HEADER_XL, 15, PaletteHandler.C("header"), "bold");
            UIHandler.DrawTextAligned(ShapeEngine.EDITORMODE == true ? "EDITOR" : "STANDALONE", new Vector2(200, 200), 100, 5, WHITE, Alignement.LEFTCENTER);
            level1Button.Draw();
            optionsButton.Draw();
            quitButton.Draw();
            //Vector2 center = new(SCREEN_AREA_UI.width / 2, SCREEN_AREA_UI.height / 2);
            //Vector2 size = new(750, 300);
            //Rectangle rect = new(center.X - size.X * 0.5f, center.Y - size.Y * 0.5f, size.X, size.Y);
            //if (UI.Button(rect, new()))
            //{
            //    GoToScene("level1");
            //
            //}
            ////RayGui.GuiSetStyle(2, 16, 25);
            //RayGui.GuiSetStyle(0, 16, 50);
            //if (RayGui.GuiButton(new(center.X - size.X * 0.5f, center.Y - size.Y * 0.5f, size.X, size.Y), "START"))
            //{
            //    GoToScene("level1");
            //}

        }

        //public override void MonitorHasChanged()
        //{
        //    base.MonitorHasChanged();
        //    level1Button.MonitorHasChanged();
        //    optionsButton.MonitorHasChanged();
        //    quitButton.MonitorHasChanged();
        //}
    }


}
