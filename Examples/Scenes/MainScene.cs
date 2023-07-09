using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.UI;
using System.Numerics;
using ShapeEngine.Lib;
using Examples.Scenes.ExampleScenes;
using Examples.UIElements;

namespace Examples.Scenes
{
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
            examples.Add(new PolylineCollisionExample());
            examples.Add(new TextScalingExample());
            examples.Add(new TextWrapExample());
            examples.Add(new TextWrapEmphasisExample());
            examples.Add(new WordEmphasisDynamicExample());
            examples.Add(new WordEmphasisStaticExample());
            examples.Add(new TextEmphasisExample());
            examples.Add(new TextRotationExample());
            examples.Add(new TextBoxExample()); 
            titleFont = GAMELOOP.FontDefault; // GAMELOOP.GetFont(GameloopExamples.FONT_IndieFlowerRegular);

            SetupButtons();
        }



        public override void HandleInput(float dt)
        {
            if (IsKeyPressed(KeyboardKey.KEY_ESCAPE)) GAMELOOP.Quit();
            if (IsKeyPressed(KeyboardKey.KEY_R))
            {
                for (int i = 0; i < examples.Count; i++)
                {
                    examples[i].Reset();
                }
            }
            if (IsKeyPressed(KeyboardKey.KEY_M)) GAMELOOP.ToggleWindowMaximize();
            if (IsKeyPressed(KeyboardKey.KEY_F)) GAMELOOP.ToggleFullscreen();

            if (IsKeyPressed(KeyboardKey.KEY_Q)) PrevPage();
            else if (IsKeyPressed(KeyboardKey.KEY_E)) NextPage();

            if (IsKeyPressed(KeyboardKey.KEY_W)) PrevButton();
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
            titleFont.DrawText(text, titleRect, 10, new(0.5f), ExampleScene.ColorLight);

            int pages = GetMaxPages();
            string pagesText = pages <= 1 ? "Page 1/1" : String.Format("[Q] <- Page #{0}/{1} -> [E]", curPageIndex + 1, pages);
            Rect pageRect = new Rect(uiSize * new Vector2(0.01f, 0.07f), uiSize * new Vector2(0.3f, 0.06f), new Vector2(0f, 0f));
            titleFont.DrawText(pagesText, pageRect, 4f, new(0f, 0.5f), ExampleScene.ColorHighlight2);

            Segment s = new(uiSize * new Vector2(0f, 0.17f), uiSize * new Vector2(1f, 0.17f));
            s.Draw(4f, ExampleScene.ColorLight);

            string backText = "Back [ESC]";
            Rect backRect = new Rect(uiSize * new Vector2(0.01f, 0.125f), uiSize * new Vector2(0.2f, 0.04f), new Vector2(0f, 0f));
            titleFont.DrawText(backText, backRect, 4f, new Vector2(0f, 0f), ExampleScene.ColorHighlight2);
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
                if (i < examples.Count) b.SetScene(examples[i]);
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

}
