
using ShapeEngine.Lib;
using Raylib_CsLo;
using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

namespace Examples.Scenes
{
    public class TextExampleScene : ExampleScene
    {
        #region Members
        protected Vector2 topLeft = new();
        protected Vector2 bottomRight = new();

        private bool mouseInsideTopLeft = false;
        private bool mouseInsideBottomRight = false;

        private bool draggingTopLeft = false;
        private bool draggingBottomRight = false;

        protected Font font;
        private int fontIndex = 0;
        private bool textEntryActive => textBox.Active;

        protected readonly ShapeTextBox textBox = new("Enter Text into this box");
        
        protected const uint accessTagTextBox = 2345;
        private readonly InputAction iaEnterText;
        private readonly InputAction iaCancelText;
        private readonly InputAction iaFinishText;
        private readonly InputAction iaDelete;
        private readonly InputAction iaBackspace;
        private readonly InputAction iaClear;
        private readonly InputAction iaCaretPrev;
        private readonly InputAction iaCaretNext;
        
        private readonly InputAction iaNextFont;
        private readonly InputAction iaDrag;
        
        protected readonly List<InputAction> inputActions;
        #endregion
        
        public TextExampleScene()
        {
            Title = "Text Example Scene";
            var s = GAMELOOP.UI.Area.Size;
            topLeft = s * new Vector2(0.1f, 0.2f);
            bottomRight = s * new Vector2(0.9f, 0.8f);
            font = GAMELOOP.GetFont(fontIndex);

            var enterTextKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ENTER);
            var enterTextGP = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_RIGHT);
            iaEnterText = new(accessTagTextBox,enterTextKB, enterTextGP);
            
            var cancelTextKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ESCAPE);
            var cancelTextGP = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_LEFT);
            iaCancelText = new(accessTagTextBox,cancelTextKB, cancelTextGP);
            
            var finishTextKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ENTER);
            var finishTextGP = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_RIGHT);
            iaFinishText = new(accessTagTextBox,finishTextKB, finishTextGP);

            var modifierKB = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT);
            var modifierGP = new ModifierKeyGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_BOTTOM);
            var clearTextKB = new InputTypeKeyboardButton(ShapeKeyboardButton.BACKSPACE, ModifierKeyOperator.Or, modifierKB);
            var clearTextGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP, 0.1f, ModifierKeyOperator.Or, modifierGP);
            iaClear = new(accessTagTextBox,clearTextKB, clearTextGP);
            
            var deleteTextKB = new InputTypeKeyboardButton(ShapeKeyboardButton.DELETE);
            var deleteTextGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
            iaDelete = new(accessTagTextBox,deleteTextKB, deleteTextGP);
            
            var backspaceTextKB = new InputTypeKeyboardButton(ShapeKeyboardButton.BACKSPACE);
            var backspaceTextGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
            iaBackspace = new(accessTagTextBox,backspaceTextKB, backspaceTextGP);
            
            var caretLeftKB = new InputTypeKeyboardButton(ShapeKeyboardButton.LEFT);
            var caretLeftGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT);
            iaCaretPrev = new(accessTagTextBox,caretLeftKB, caretLeftGP);

            var caretRightKB = new InputTypeKeyboardButton(ShapeKeyboardButton.RIGHT);
            var caretRightGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT);
            iaCaretNext = new(accessTagTextBox,caretRightKB, caretRightGP);
            
            var dragMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            var dragKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var dragGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            iaDrag = new(accessTagTextBox,dragGP, dragKB, dragMB);

            var nextFontKB = new InputTypeKeyboardButton(ShapeKeyboardButton.A);
            var nextFontGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            iaNextFont = new(accessTagTextBox,nextFontKB, nextFontGP);
            
            inputActions = new()
            {
                iaEnterText, iaCancelText, iaFinishText, iaClear, iaDelete, iaBackspace, iaCaretPrev, iaCaretNext,
                iaDrag, iaNextFont
            };
        }
        
        #region Virtual
        protected virtual void HandleInputTextEntryInactive(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            
        }
        protected virtual void HandleInputTextEntryActive(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            
        }
        protected virtual void UpdateExampleTextEntryActive(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            
        }
        protected virtual void UpdateExampleTextEntryInactive(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            
        }
        protected virtual void DrawText(Rect rect)
        {
            
        }
        protected virtual void DrawTextEntry(Rect rect)
        {
            
        }
        protected virtual void DrawInputDescriptionBottom(Rect rect)
        {
            
        }
        #endregion

        #region Base Class
        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            var gamepad = GAMELOOP.CurGamepad;
            foreach (var action in inputActions)
            {
                action.Gamepad = gamepad;
                action.Update(dt);
            }
            textBox.Update(dt);
            
            if (!textEntryActive)
            {
                if (iaEnterText.State.Pressed)
                {
                    textBox.StartEntry();
                    InputAction.LockWhitelist(accessTagTextBox);
                    draggingBottomRight = false;
                    draggingTopLeft = false;
                    mouseInsideBottomRight = false;
                    mouseInsideTopLeft = false;
                    // prevText = text;
                }
                if (iaNextFont.State.Pressed) NextFont();

                if (mouseInsideTopLeft)
                {
                    if (draggingTopLeft)
                    {
                        if (iaDrag.State.Released) draggingTopLeft = false;
                    }
                    else
                    {
                        if (iaDrag.State.Pressed) draggingTopLeft = true;
                    }

                }
                else if (mouseInsideBottomRight)
                {
                    if (draggingBottomRight)
                    {
                        if (iaDrag.State.Released) draggingBottomRight = false;
                    }
                    else
                    {
                        if (iaDrag.State.Pressed) draggingBottomRight = true;
                    }
                }
                
                HandleInputTextEntryInactive(dt, mousePosGame, mousePosUI);
            }
            else
            {
                if (iaFinishText.State.Pressed)
                {
                    textBox.FinishEntry();
                    InputAction.Unlock();
                }
                else if (iaCancelText.State.Pressed)
                {
                    textBox.CancelEntry();
                    InputAction.Unlock();
                }
                else if (iaClear.State.Pressed)
                {
                    textBox.DeleteEntry();
                }
                else if (iaDelete.State.Down)
                {
                    textBox.DeleteCharacterStart(1);
                }
                else if (iaBackspace.State.Down)
                {
                    textBox.DeleteCharacterStart(-1);
                }
                else if (iaCaretPrev.State.Down)
                {
                    // textBox.MoveCaret(-1, false);
                    textBox.MoveCaretStart(-1, false);
                }
                else if (iaCaretNext.State.Down)
                {
                    // textBox.MoveCaret(1, false);
                    textBox.MoveCaretStart(1, false);
                }
                else
                {
                    textBox.AddCharacters(ShapeInput.KeyboardDevice.GetStreamChar());
                }

                
                if (iaCaretPrev.State.Released || iaCaretNext.State.Released)
                {
                    textBox.MoveCaretEnd();
                }
                else if (iaDelete.State.Released || iaBackspace.State.Released)
                {
                    textBox.DeleteCharacterEnd();
                }
                
                HandleInputTextEntryActive(dt, mousePosGame, mousePosUI);
            }
        }
        protected override void UpdateExample(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            if (textEntryActive)
            {
                UpdateExampleTextEntryActive(dt, deltaSlow, game, ui);
                return;
            }
            
            float lineThickness = ui.Area.Size.Min() * 0.01f;
            float interactionRadius = lineThickness * 4;
            
            if (draggingTopLeft || draggingBottomRight)
            {
                if (draggingTopLeft) topLeft = ui.MousePos;
                else if (draggingBottomRight) bottomRight = ui.MousePos;
            }
            else
            {
                float topLeftDisSq = (topLeft - ui.MousePos).LengthSquared();
                mouseInsideTopLeft = topLeftDisSq <= interactionRadius * interactionRadius;

                if (!mouseInsideTopLeft)
                {
                    float bottomRightDisSq = (bottomRight - ui.MousePos).LengthSquared();
                    mouseInsideBottomRight = bottomRightDisSq <= interactionRadius * interactionRadius;
                }
            }
            
            UpdateExampleTextEntryInactive(dt, deltaSlow, game, ui);
        }
        protected override void DrawGameUIExample(ScreenInfo ui) 
        {
            Rect r = new(topLeft, bottomRight);
            float lineThickness = ui.Area.Size.Min() * 0.01f;
            // float fontSize = r.Width * 0.05f;
            float pointRadius = lineThickness * 2f;
            float interactionRadius = lineThickness * 4;
            r.DrawLines(lineThickness, ColorMedium);

            if (!textEntryActive)
            {
                //font.DrawText(textBox.Text, fontSize, fontSpacing, r.GetPoint(curAlignement), curAlignement, ColorHighlight1);
                
                DrawText(r);
                
                Circle topLeftPoint = new(topLeft, pointRadius);
                Circle topLeftInteractionCircle = new(topLeft, interactionRadius);
                if (draggingTopLeft)
                {
                    topLeftInteractionCircle.Draw(ColorHighlight2);
                }
                else if (mouseInsideTopLeft)
                {
                    topLeftPoint.Draw(ColorMedium);
                    topLeftInteractionCircle.Radius *= 2f;
                    topLeftInteractionCircle.DrawLines(2f, ColorHighlight2, 4f);
                }
                else
                {
                    topLeftPoint.Draw(ColorMedium);
                    topLeftInteractionCircle.DrawLines(2f, ColorMedium, 4f);
                }

                Circle bottomRightPoint = new(bottomRight, pointRadius);
                Circle bottomRightInteractionCircle = new(bottomRight, interactionRadius);
                if (draggingBottomRight)
                {
                    bottomRightInteractionCircle.Draw(ColorHighlight2);
                }
                else if (mouseInsideBottomRight)
                {
                    bottomRightPoint.Draw(ColorMedium);
                    bottomRightInteractionCircle.Radius *= 2f;
                    bottomRightInteractionCircle.DrawLines(2f, ColorHighlight2, 4f);
                }
                else
                {
                    bottomRightPoint.Draw(ColorMedium);
                    bottomRightInteractionCircle.DrawLines(2f, ColorMedium, 4f);
                }
            }
            else
            {
                DrawTextEntry(r);
                // font.DrawText(textBox.Text, fontSize, fontSpacing, r.GetPoint(curAlignement), curAlignement, ColorLight);
                
                // if(textBox.CaretVisible)
                    // font.DrawCaret(textBox.Text, r, fontSize, fontSpacing, curAlignement, textBox.CaretIndex, 5f, ColorHighlight2);
            }

        }
        protected override void DrawUIExample(ScreenInfo ui)
        {
            var rects = GAMELOOP.UIRects.GetRect("bottom center").SplitV(0.35f);
            DrawDescription(rects.top, rects.bottom);
           
        }
        public override void OnWindowSizeChanged(DimensionConversionFactors conversionFactors)
        {
            topLeft *= conversionFactors.Factor;
            bottomRight *=  conversionFactors.Factor;
        }
        protected override bool IsCancelAllowed()
        {
            return !textEntryActive;
        }

        private void DrawDescription(Rect top, Rect bottom)
        {
            var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
            var curInputDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;

            
            string dragText = iaDrag.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string nextFontText = iaNextFont.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string enterText = iaEnterText.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string finishText = iaFinishText.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string cancelText = iaCancelText.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string backspaceText = iaBackspace.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string deleteText = iaDelete.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string clearText = iaClear.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string caretNextText = iaCaretNext.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            string caretPrevText = iaCaretPrev.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            
            
            if (!textEntryActive)
            {
                string info =
                    $"Write Custom Text {enterText} | Drag Rect Corners {dragText} | Change Font {nextFontText} ({GAMELOOP.GetFontName(fontIndex)})";
                font.DrawText(info, top, 4f, new Vector2(0.5f, 0.5f), ColorLight);

                DrawInputDescriptionBottom(bottom);
                // string alignmentInfo = $"{nextFontText} Font: {GAMELOOP.GetFontName(fontIndex)} | [{decreaseFontSpacingText}/{increaseFontSpacingText}] Font Spacing: {fontSpacing} | {nextAlignementText} Alignment: {curAlignement}";
                // font.DrawText(alignmentInfo, bottom, 4f, new Vector2(0.5f, 0.5f), ColorLight);
            }
            else
            {
                string info = $"Cancel {cancelText} | Accept {finishText} | Clear Text {clearText} | Delete {deleteText} | Backspace {backspaceText}";
                font.DrawText($"Text Entry Mode Active |  Caret Position [{caretPrevText}/{caretNextText}] ({textBox.CaretIndex})", top, 4f, new Vector2(0.5f, 0.5f), ColorHighlight3);
                font.DrawText(info, bottom, 4f, new Vector2(0.5f, 0.5f), ColorLight);
            }
        }
        private void NextFont()
        {
            int fontCount = GAMELOOP.GetFontCount();
            fontIndex++;
            if (fontIndex >= fontCount) fontIndex = 0;
            font = GAMELOOP.GetFont(fontIndex);
        }
        #endregion
    }

}
