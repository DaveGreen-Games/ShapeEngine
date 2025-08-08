
using ShapeEngine.StaticLib;
using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Input;
using ShapeEngine.Text;

namespace Examples.Scenes
{
    public class TextExampleScene : ExampleScene
    {
        #region Members
        protected Vector2 topLeft { get; private set; } = new();
        protected Vector2 bottomRight { get; private set; } = new();
        protected Vector2 topLeftRelative = new();
        protected Vector2 bottomRightRelative = new();

        private bool mouseInsideTopLeft = false;
        private bool mouseInsideBottomRight = false;

        private bool draggingTopLeft = false;
        private bool draggingBottomRight = false;

        //protected TextFont font;
        // protected Font font;
        private int fontIndex = 0;
        private bool textEntryActive => TextInputBox.Active;

        protected readonly TextInputBox TextInputBox = new("Enter Text into this box");

        protected readonly uint accessTagTextBox = ShapeInput.NextAccessTag; // BitFlag.GetFlagUint(12);
        
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
        
        // protected readonly List<InputAction> inputActions;
        protected readonly InputActionTree inputActionTree;
        #endregion
        
        public TextExampleScene()
        {
            Title = "Text Example Scene";
            
            var s = GAMELOOP.UIScreenInfo.Area.Size;
            topLeftRelative = new Vector2(0.1f, 0.2f);
            bottomRightRelative = new Vector2(0.9f, 0.8f);
            topLeft = topLeftRelative * s;
            bottomRight = bottomRightRelative * s;
            
            //font =  new(GAMELOOP.GetFont(fontIndex),1f, ExampleScene.ColorLight);
            InputActionSettings defaultSettings = new();
            var enterTextKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ENTER);
            var enterTextGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
            iaEnterText = new(accessTagTextBox,defaultSettings,enterTextKB, enterTextGP);
            
            var cancelTextKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ESCAPE);
            var cancelTextGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
            iaCancelText = new(accessTagTextBox,defaultSettings,cancelTextKB, cancelTextGP);
            
            var finishTextKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ENTER);
            var finishTextGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            iaFinishText = new(accessTagTextBox,defaultSettings,finishTextKB, finishTextGP);

            var modifierKB = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT);
            var modifierGP = new ModifierKeyGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_BOTTOM);
            
            var modifierKeySetKb = new ModifierKeySet(ModifierKeyOperator.Or, modifierKB);
            var modifierKeySetGp = new ModifierKeySet(ModifierKeyOperator.Or, modifierGP);
            var clearTextKB = new InputTypeKeyboardButton(ShapeKeyboardButton.BACKSPACE, modifierKeySetKb);
            var clearTextGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP, 0.1f, modifierKeySetGp);
            iaClear = new(accessTagTextBox,defaultSettings,clearTextKB, clearTextGP);
            
            var deleteTextKB = new InputTypeKeyboardButton(ShapeKeyboardButton.DELETE);
            var deleteTextGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
            iaDelete = new(accessTagTextBox,defaultSettings,deleteTextKB, deleteTextGP);
            
            var backspaceTextKB = new InputTypeKeyboardButton(ShapeKeyboardButton.BACKSPACE);
            var backspaceTextGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
            iaBackspace = new(accessTagTextBox,defaultSettings,backspaceTextKB, backspaceTextGP);
            
            var caretLeftKB = new InputTypeKeyboardButton(ShapeKeyboardButton.LEFT);
            var caretLeftGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT);
            iaCaretPrev = new(accessTagTextBox,defaultSettings,caretLeftKB, caretLeftGP);

            var caretRightKB = new InputTypeKeyboardButton(ShapeKeyboardButton.RIGHT);
            var caretRightGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT);
            iaCaretNext = new(accessTagTextBox,defaultSettings,caretRightKB, caretRightGP);
            
            var dragMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            var dragKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var dragGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            iaDrag = new(accessTagTextBox,defaultSettings,dragGP, dragKB, dragMB);

            var nextFontKB = new InputTypeKeyboardButton(ShapeKeyboardButton.A);
            var nextFontGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            iaNextFont = new(accessTagTextBox,defaultSettings,nextFontKB, nextFontGP);
            
            inputActionTree =
            [
                iaEnterText, iaCancelText, iaFinishText, iaClear, iaDelete, iaBackspace, iaCaretPrev, iaCaretNext,
                iaDrag, iaNextFont
            ];
        }
        
        #region Virtual
        protected virtual void HandleInputTextEntryInactive(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            
        }
        protected virtual void HandleInputTextEntryActive(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            
        }
        protected virtual void UpdateExampleTextEntryActive(float dt, ScreenInfo game, ScreenInfo ui)
        {
            
        }
        protected virtual void UpdateExampleTextEntryInactive(float dt, ScreenInfo game, ScreenInfo ui)
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
        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
        {
            var gamepad = ShapeInput.GamepadManager.LastUsedGamepad;
            inputActionTree.CurrentGamepad = gamepad;
            inputActionTree.Update(dt);
            TextInputBox.Update(dt);
            
            if (!textEntryActive)
            {
                if (iaEnterText.State.Pressed)
                {
                    BitFlag mask = new(accessTagTextBox);
                    ShapeInput.LockWhitelist(mask);
                    TextInputBox.StartEntry();
                    // InputAction.LockWhitelist(accessTagTextBox);
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
                    TextInputBox.FinishEntry();
                    ShapeInput.Unlock();
                }
                else if (iaCancelText.State.Pressed)
                {
                    TextInputBox.CancelEntry();
                    ShapeInput.Unlock();
                }
                else if (iaClear.State.Pressed)
                {
                    TextInputBox.DeleteEntry();
                }
                else if (iaDelete.State.Down)
                {
                    TextInputBox.DeleteCharacterStart(1);
                }
                else if (iaBackspace.State.Down)
                {
                    TextInputBox.DeleteCharacterStart(-1);
                }
                else if (iaCaretPrev.State.Down)
                {
                    // textBox.MoveCaret(-1, false);
                    TextInputBox.MoveCaretStart(-1, false);
                }
                else if (iaCaretNext.State.Down)
                {
                    // textBox.MoveCaret(1, false);
                    TextInputBox.MoveCaretStart(1, false);
                }
                else
                {
                    TextInputBox.AddCharacters(ShapeInput.Keyboard.GetStreamChar());
                }

                
                if (iaCaretPrev.State.Released || iaCaretNext.State.Released)
                {
                    TextInputBox.MoveCaretEnd();
                }
                else if (iaDelete.State.Released || iaBackspace.State.Released)
                {
                    TextInputBox.DeleteCharacterEnd();
                }
                
                HandleInputTextEntryActive(dt, mousePosGame, mousePosUI);
            }
        }
        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui)
        {
            var uiSize = ui.Area.Size;
            
            if (textEntryActive)
            {
                topLeft = topLeftRelative * uiSize;
                bottomRight = bottomRightRelative * uiSize;
                UpdateExampleTextEntryActive(time.Delta, game, ui);
                return;
            }
            
            float lineThickness = uiSize.Min() * 0.01f;
            float interactionRadius = lineThickness * 4;
            
            if (draggingTopLeft || draggingBottomRight)
            {
                if (draggingTopLeft)
                {
                    topLeftRelative = (ui.MousePos / uiSize);
                }
                else if (draggingBottomRight)
                {
                    bottomRightRelative = (ui.MousePos / uiSize);
                }

                topLeft = topLeftRelative * uiSize;
                bottomRight = bottomRightRelative * uiSize;
            }
            else
            {
                topLeft = topLeftRelative * uiSize;
                bottomRight = bottomRightRelative * uiSize;
                
                float topLeftDisSq = (topLeft - ui.MousePos).LengthSquared();
                mouseInsideTopLeft = topLeftDisSq <= interactionRadius * interactionRadius;

                if (!mouseInsideTopLeft)
                {
                    float bottomRightDisSq = (bottomRight - ui.MousePos).LengthSquared();
                    mouseInsideBottomRight = bottomRightDisSq <= interactionRadius * interactionRadius;
                }
            }
            
            UpdateExampleTextEntryInactive(time.Delta, game, ui);
        }
        
        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            var uiSize = ui.Area.Size;
            Rect r = new(topLeft, bottomRight);
            
            float lineThickness = uiSize.Min() * 0.01f;
            // float fontSize = r.Width * 0.05f;
            float pointRadius = lineThickness * 2f;
            float interactionRadius = lineThickness * 4;
            r.DrawLines(lineThickness, Colors.Medium);
            
            float margin = r.Size.Min() * 0.05f;
            r = r.ApplyMarginsAbsolute(margin, margin, margin, margin);
            
            if (!textEntryActive)
            {
                //font.DrawText(textBox.Text, fontSize, fontSpacing, r.GetPoint(curAlignement), curAlignement, ColorHighlight1);
                
                DrawText(r);
                
                Circle topLeftPoint = new(topLeft, pointRadius);
                Circle topLeftInteractionCircle = new(topLeft, interactionRadius);
                if (draggingTopLeft)
                {
                    topLeftInteractionCircle.Draw(Colors.Highlight);
                }
                else if (mouseInsideTopLeft)
                {
                    topLeftPoint.Draw(Colors.Medium);
                    topLeftInteractionCircle *= 2f;
                    // topLeftInteractionCircle.Radius *= 2f;
                    topLeftInteractionCircle.DrawLines(2f, Colors.Special, 4f);
                }
                else
                {
                    topLeftPoint.Draw(Colors.Medium);
                    topLeftInteractionCircle.DrawLines(2f, Colors.Medium, 4f);
                }

                Circle bottomRightPoint = new(bottomRight, pointRadius);
                Circle bottomRightInteractionCircle = new(bottomRight, interactionRadius);
                if (draggingBottomRight)
                {
                    bottomRightInteractionCircle.Draw(Colors.Special);
                }
                else if (mouseInsideBottomRight)
                {
                    bottomRightPoint.Draw(Colors.Medium);
                    bottomRightInteractionCircle *= 2f;
                    // bottomRightInteractionCircle.Radius *= 2f;
                    bottomRightInteractionCircle.DrawLines(2f, Colors.Special, 4f);
                }
                else
                {
                    bottomRightPoint.Draw(Colors.Medium);
                    bottomRightInteractionCircle.DrawLines(2f, Colors.Medium, 4f);
                }
            }
            else
            {
                DrawTextEntry(r);
            }
            
            var rects = GAMELOOP.UIRects.GetRect("bottom center").SplitV(0.35f);
            DrawDescription(rects.top, rects.bottom);
           
        }
        // public override void OnWindowSizeChanged(DimensionConversionFactors conversionFactors)
        // {
        //     topLeft *= conversionFactors.Factor;
        //     bottomRight *=  conversionFactors.Factor;
        // }
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

                textFont.FontSpacing = 4f;
                textFont.ColorRgba = Colors.Light;
                textFont.DrawTextWrapNone(info, top, new(0.5f));
                // font.DrawText(info, top, 4f, new Vector2(0.5f, 0.5f), ColorLight);

                DrawInputDescriptionBottom(bottom);
                // string alignmentInfo = $"{nextFontText} Font: {GAMELOOP.GetFontName(fontIndex)} | [{decreaseFontSpacingText}/{increaseFontSpacingText}] Font Spacing: {fontSpacing} | {nextAlignementText} Alignment: {curAlignement}";
                // font.DrawText(alignmentInfo, bottom, 4f, new Vector2(0.5f, 0.5f), ColorLight);
            }
            else
            {
                string info = $"Cancel {cancelText} | Accept {finishText} | Clear Text {clearText} | Delete {deleteText} | Backspace {backspaceText}";
                
                textFont.FontSpacing = 4f;
                
                textFont.ColorRgba = Colors.Warm;
                textFont.DrawTextWrapNone($"Text Entry Mode Active |  Caret Position [{caretPrevText}/{caretNextText}] ({TextInputBox.CaretIndex})", top, new(0.5f));
                
                textFont.ColorRgba = Colors.Light;
                textFont.DrawTextWrapNone(info, bottom, new(0.5f));
                
                // font.DrawText($"Text Entry Mode Active |  Caret Position [{caretPrevText}/{caretNextText}] ({textBox.CaretIndex})", top, 4f, new Vector2(0.5f, 0.5f), ColorHighlight3);
                // font.DrawText(info, bottom, 4f, new Vector2(0.5f, 0.5f), ColorLight);
            }
        }
        private void NextFont()
        {
            int fontCount = GAMELOOP.GetFontCount();
            fontIndex++;
            if (fontIndex >= fontCount) fontIndex = 0;
            textFont.Font = GAMELOOP.GetFont(fontIndex);
        }
        #endregion
    }

}
