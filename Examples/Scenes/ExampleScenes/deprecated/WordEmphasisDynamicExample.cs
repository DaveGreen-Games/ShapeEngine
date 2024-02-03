//
//
// using Raylib_CsLo;
// using ShapeEngine.Lib;
// using System.Numerics;
// using ShapeEngine.Core.Interfaces;
// using ShapeEngine.Core.Structs;
// using ShapeEngine.Core.Shapes;
// using ShapeEngine.Input;
//
// namespace Examples.Scenes.ExampleScenes
// {
//     public class WordEmphasisDynamicExample : TextExampleScene
//     {
//
//         private int fontSpacing = 1;
//         private const int maxFontSpacing = 15;
//
//         TextEmphasisType curEmphasisType = TextEmphasisType.Corner;
//         TextEmphasisAlignement curEmphasisAlignement = TextEmphasisAlignement.TopLeft;
//         Vector2 curAlignement = new(0.5f, 0.5f);
//         int curAlignementIndex = 8;
//
//         private readonly InputAction iaNextAlignement;
//         // private readonly InputAction iaIncreaseFontSpacing;
//         private readonly InputAction iaNextEmphasisAlignement;
//         private readonly InputAction iaNextEmphasisType;
//         
//         public WordEmphasisDynamicExample()
//         {
//             Title = "Word Emphasis Dynamic Example";
//             
//             var nextAlignementKB = new InputTypeKeyboardButton(ShapeKeyboardButton.S);
//             var nextAlignementGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN);
//             iaNextAlignement = new(accessTagTextBox,nextAlignementKB, nextAlignementGP);
//             
//             // var increaseFontSpacingKB = new InputTypeKeyboardButton(ShapeKeyboardButton.W);
//             // var increaseFontSpacingGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP);
//             // iaIncreaseFontSpacing = new(accessTagTextBox,increaseFontSpacingGP, increaseFontSpacingKB);
//             
//             var nextEmphasisAlignementKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
//             var nextEmphasisAlignementGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT);
//             iaNextEmphasisAlignement = new(accessTagTextBox,nextEmphasisAlignementKB, nextEmphasisAlignementGP);
//             
//             var nextEmphasisKB = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
//             var nextEmphasisGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT);
//             iaNextEmphasisType = new(accessTagTextBox,nextEmphasisKB, nextEmphasisGP);
//             
//             inputActions.Add(iaNextAlignement);
//             // inputActions.Add(iaIncreaseFontSpacing);
//             inputActions.Add(iaNextEmphasisType);
//             inputActions.Add(iaNextEmphasisAlignement);
//         }
//
//         
//         protected override void HandleInputTextEntryInactive(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
//         {
//             // if (iaIncreaseFontSpacing.State.Pressed) ChangeFontSpacing(1);
//             if (iaNextEmphasisType.State.Pressed) NextTextEmphasisType();
//             if (iaNextEmphasisAlignement.State.Pressed) NextTextEmphasisAlignement();
//             if (iaNextAlignement.State.Pressed) NextAlignement();
//         }
//
//         protected override void DrawText(Rect rect)
//         {
//             DrawCross(rect.GetPoint(curAlignement), 100f);
//
//             WordEmphasis we = new(ColorHighlight1, curEmphasisType, curEmphasisAlignement);
//             font.DrawWord(textBox.Text, rect, fontSpacing, curAlignement, we);
//         }
//
//         protected override void DrawTextEntry(Rect rect)
//         {
//             DrawCross(rect.GetPoint(curAlignement), 100f);
//
//             WordEmphasis we = new(ColorLight, curEmphasisType, curEmphasisAlignement);
//             font.DrawWord(textBox.Text, rect, fontSpacing, curAlignement, we);
//             
//             if(textBox.CaretVisible)
//                 font.DrawCaret(textBox.Text, rect, fontSpacing, curAlignement, textBox.CaretIndex, 5f, ColorHighlight2);
//         }
//
//         protected override void DrawInputDescriptionBottom(Rect rect)
//         {
//             var curInputDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
//             string nextAlignementText = iaNextAlignement.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
//             // string increaseFontSpacingText = iaIncreaseFontSpacing.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
//             string nextEmphasisTypeText = iaNextEmphasisType.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
//             string nextEmphasisAligenmentText = iaNextEmphasisAlignement.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
//             // string text = $"Font Spacing {increaseFontSpacingText} ({fontSpacing}) | Alignment {nextAlignementText} ({curAlignement}) | Emphasis Type {nextEmphasisTypeText} ({curEmphasisType}) | Emphasis Alignment {nextEmphasisAligenmentText} ({curEmphasisAlignement})";
//             string text = $"Emphasis Type {nextEmphasisTypeText} ({curEmphasisType}) | Emphasis Alignment {nextEmphasisAligenmentText} ({curEmphasisAlignement}) | Alignment {nextAlignementText} ({curAlignement})";
//             font.DrawText(text, rect, 4f, new Vector2(0.5f, 0.5f), ColorLight);
//         }
//         
//         // private void ChangeFontSpacing(int amount)
//         // {
//         //     fontSpacing += amount;
//         //     if (fontSpacing < 0) fontSpacing = maxFontSpacing;
//         //     else if (fontSpacing > maxFontSpacing) fontSpacing = 0;
//         // }
//         //
//         private void NextAlignement()
//         {
//             curAlignementIndex++;
//             if (curAlignementIndex > 8) curAlignementIndex = 0;
//             else if (curAlignementIndex < 0) curAlignementIndex = 8;
//
//             if (curAlignementIndex == 0) curAlignement = new Vector2(0f); //top left
//             else if (curAlignementIndex == 1) curAlignement = new Vector2(0.5f, 0f); //top
//             else if (curAlignementIndex == 2) curAlignement = new Vector2(1f, 0f); //top right
//             else if (curAlignementIndex == 3) curAlignement = new Vector2(1f, 0.5f); //right
//             else if (curAlignementIndex == 4) curAlignement = new Vector2(1f, 1f); //bottom right
//             else if (curAlignementIndex == 5) curAlignement = new Vector2(0.5f, 1f); //bottom
//             else if (curAlignementIndex == 6) curAlignement = new Vector2(0f, 1f); //bottom left
//             else if (curAlignementIndex == 7) curAlignement = new Vector2(0f, 0.5f); //left
//             else if (curAlignementIndex == 8) curAlignement = new Vector2(0.5f, 0.5f); //center
//         }
//         private void NextTextEmphasisType()
//         {
//             int cur = (int)curEmphasisType;
//             cur++;
//             if (cur > 2) cur = 0;
//             else if (cur < 0) cur = 2;
//             curEmphasisType = (TextEmphasisType)cur;
//         }
//         private void NextTextEmphasisAlignement()
//         {
//             int cur = (int)curEmphasisAlignement;
//             cur++;
//             if (cur > 11) cur = 0;
//             else if (cur < 0) cur = 11;
//             curEmphasisAlignement = (TextEmphasisAlignement)cur;
//         }
//     }
// }
