

using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;

namespace Examples.Scenes.ExampleScenes
{
    // public class TextEmphasisExample : ExampleScene
    // {
    //     Vector2 topLeft = new();
    //     Vector2 bottomRight = new();
    //
    //     bool mouseInsideTopLeft = false;
    //     bool mouseInsideBottomRight = false;
    //
    //     bool draggingTopLeft = false;
    //     bool draggingBottomRight = false;
    //
    //     float pointRadius = 8f;
    //     float interactionRadius = 24f;
    //
    //     string text = "While HP is below {0}%, Attack Speed is doubled.";
    //     int fontSpacing = 0;
    //     int fontSpacingIncrement = 2;
    //     int maxFontSpacing = 100;
    //     Font font;
    //     int fontIndex = 0;
    //
    //     int fontSize = 30;
    //     int fontSizeIncrement = 10;
    //     int maxFontSize = 100;
    //
    //     float timer = 0f;
    //     float interval = 2f;
    //     float percentage = 50f;
    //
    //     float rotDeg = 0f;
    //
    //     TextEmphasisType curEmphasisType = TextEmphasisType.Corner;
    //     TextEmphasisAlignement curEmphasisAlignement = TextEmphasisAlignement.TopLeft;
    //     Vector2 curAlignement = new(0.5f);
    //     int curAlignementIndex = 8;
    //
    //     public TextEmphasisExample()
    //     {
    //         Title = "Text Multi Color Example";
    //         var s = GAMELOOP.UI.Area.Size;
    //         topLeft = s * new Vector2(0.1f, 0.1f);
    //         bottomRight = s * new Vector2(0.9f, 0.8f);
    //         font = GAMELOOP.GetFont(fontIndex);
    //     }
    //     public override void OnWindowSizeChanged(DimensionConversionFactors conversionFactors)
    //     {
    //         topLeft *= conversionFactors.Factor;
    //         bottomRight *= conversionFactors.Factor;
    //     }
    //
    //     protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
    //     {
    //         if (IsKeyPressed(KeyboardKey.KEY_W)) NextFont();
    //
    //         if (IsKeyPressed(KeyboardKey.KEY_D)) ChangeFontSize();
    //         if (IsKeyPressed(KeyboardKey.KEY_A)) ChangeFontSpacing();
    //
    //         if (IsKeyPressed(KeyboardKey.KEY_Q)) NextTextEmphasisType();
    //         if (IsKeyPressed(KeyboardKey.KEY_E)) NextTextEmphasisAlignement();
    //         if (IsKeyPressed(KeyboardKey.KEY_S)) NextAlignement();
    //
    //         if (mouseInsideTopLeft)
    //         {
    //             if (draggingTopLeft)
    //             {
    //                 if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) draggingTopLeft = false;
    //             }
    //             else
    //             {
    //                 if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) draggingTopLeft = true;
    //             }
    //
    //         }
    //         else if (mouseInsideBottomRight)
    //         {
    //             if (draggingBottomRight)
    //             {
    //                 if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) draggingBottomRight = false;
    //             }
    //             else
    //             {
    //                 if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) draggingBottomRight = true;
    //             }
    //         }
    //
    //
    //     }
    //     protected override void UpdateExample(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
    //     {
    //         timer += dt;
    //         if (timer >= interval)
    //         {
    //             timer = 0f;
    //             percentage = ShapeRandom.randI(10, 90);
    //         }
    //
    //         if (draggingTopLeft || draggingBottomRight)
    //         {
    //             if (draggingTopLeft) topLeft = ui.MousePos;
    //             else if (draggingBottomRight) bottomRight = ui.MousePos;
    //         }
    //         else
    //         {
    //             float topLeftDisSq = (topLeft - ui.MousePos).LengthSquared();
    //             mouseInsideTopLeft = topLeftDisSq <= interactionRadius * interactionRadius;
    //
    //             if (!mouseInsideTopLeft)
    //             {
    //                 float bottomRightDisSq = (bottomRight - ui.MousePos).LengthSquared();
    //                 mouseInsideBottomRight = bottomRightDisSq <= interactionRadius * interactionRadius;
    //             }
    //         }
    //
    //     }
    //     protected override void DrawGameUIExample(ScreenInfo ui)
    //     {
    //         Vector2 uiSize = ui.Area.Size;
    //         rotDeg = ShapeVec.AngleDeg((uiSize * 0.5f) - ui.MousePos);
    //
    //         Rect r = new(topLeft, bottomRight);
    //         r.DrawLines(6f, ColorMedium);
    //
    //         DrawCross(r.GetPoint(curAlignement), 100f);
    //
    //         WordEmphasis basic = new(ColorHighlight1);
    //         WordEmphasis red = new(ColorHighlight2, TextEmphasisType.Line, TextEmphasisAlignement.Bottom, 1, 4);
    //         WordEmphasis yellow = new(ColorLight, curEmphasisType, curEmphasisAlignement, 5, 6);
    //         string curText = String.Format(text, percentage);
    //         font.DrawText(curText, fontSize, fontSpacing, r.GetPoint(curAlignement), curAlignement, basic, red, yellow);
    //         Circle topLeftPoint = new(topLeft, pointRadius);
    //         Circle topLeftInteractionCircle = new(topLeft, interactionRadius);
    //         if (draggingTopLeft)
    //         {
    //             topLeftInteractionCircle.Draw(ColorHighlight2);
    //         }
    //         else if (mouseInsideTopLeft)
    //         {
    //             topLeftPoint.Draw(ColorMedium);
    //             topLeftInteractionCircle.Radius *= 2f;
    //             topLeftInteractionCircle.DrawLines(2f, ColorHighlight2, 4f);
    //         }
    //         else
    //         {
    //             topLeftPoint.Draw(ColorMedium);
    //             topLeftInteractionCircle.DrawLines(2f, ColorMedium, 4f);
    //         }
    //
    //         Circle bottomRightPoint = new(bottomRight, pointRadius);
    //         Circle bottomRightInteractionCircle = new(bottomRight, interactionRadius);
    //         if (draggingBottomRight)
    //         {
    //             bottomRightInteractionCircle.Draw(ColorHighlight2);
    //         }
    //         else if (mouseInsideBottomRight)
    //         {
    //             bottomRightPoint.Draw(ColorMedium);
    //             bottomRightInteractionCircle.Radius *= 2f;
    //             bottomRightInteractionCircle.DrawLines(2f, ColorHighlight2, 4f);
    //         }
    //         else
    //         {
    //             bottomRightPoint.Draw(ColorMedium);
    //             bottomRightInteractionCircle.DrawLines(2f, ColorMedium, 4f);
    //         }
    //     }
    //
    //     protected override void DrawUIExample(ScreenInfo ui)
    //     {
    //         Vector2 uiSize = ui.Area.Size;
    //         
    //         string info2 =
    //             $"[S] Text Align: {curAlignement} | [Q] Type: {curEmphasisType} | [E] Align: {curEmphasisAlignement}";
    //         Rect infoRect2 = new(uiSize * new Vector2(0.5f, 0.95f), uiSize * new Vector2(0.95f, 0.075f), new Vector2(0.5f, 1f));
    //         font.DrawText(info2, infoRect2, 4f, new Vector2(0.5f, 0.5f), ColorLight);
    //
    //         string info =
    //             $"[W] Font: {GAMELOOP.GetFontName(fontIndex)} | [A] Spacing: {fontSpacing} | [D] Size: {fontSize}";
    //         Rect infoRect = new(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.075f), new Vector2(0.5f, 1f));
    //         font.DrawText(info, infoRect, 4f, new Vector2(0.5f, 0.5f), ColorLight);
    //     }
    //
    //     private void ChangeFontSpacing()
    //     {
    //         fontSpacing += fontSpacingIncrement;
    //         if (fontSpacing < 0) fontSpacing = maxFontSpacing;
    //         else if (fontSpacing > maxFontSpacing) fontSpacing = 0;
    //     }
    //     private void NextFont()
    //     {
    //         int fontCount = GAMELOOP.GetFontCount();
    //         fontIndex++;
    //         if (fontIndex >= fontCount) fontIndex = 0;
    //         font = GAMELOOP.GetFont(fontIndex);
    //     }
    //     private void ChangeFontSize()
    //     {
    //         fontSize += fontSizeIncrement;
    //         if (fontSize < 30) fontSize = maxFontSize;
    //         else if (fontSize > maxFontSize) fontSize = 30;
    //     }
    //
    //     private void NextAlignement()
    //     {
    //         curAlignementIndex++;
    //         if (curAlignementIndex > 8) curAlignementIndex = 0;
    //         else if (curAlignementIndex < 0) curAlignementIndex = 8;
    //
    //         if (curAlignementIndex == 0) curAlignement = new Vector2(0f); //top left
    //         else if (curAlignementIndex == 1) curAlignement = new Vector2(0.5f, 0f); //top
    //         else if (curAlignementIndex == 2) curAlignement = new Vector2(1f, 0f); //top right
    //         else if (curAlignementIndex == 3) curAlignement = new Vector2(1f, 0.5f); //right
    //         else if (curAlignementIndex == 4) curAlignement = new Vector2(1f, 1f); //bottom right
    //         else if (curAlignementIndex == 5) curAlignement = new Vector2(0.5f, 1f); //bottom
    //         else if (curAlignementIndex == 6) curAlignement = new Vector2(0f, 1f); //bottom left
    //         else if (curAlignementIndex == 7) curAlignement = new Vector2(0f, 0.5f); //left
    //         else if (curAlignementIndex == 8) curAlignement = new Vector2(0.5f, 0.5f); //center
    //     }
    //     private void NextTextEmphasisType()
    //     {
    //         int cur = (int)curEmphasisType;
    //         cur++;
    //         if (cur > 2) cur = 0;
    //         else if (cur < 0) cur = 2;
    //         curEmphasisType = (TextEmphasisType)cur;
    //     }
    //     private void NextTextEmphasisAlignement()
    //     {
    //         int cur = (int)curEmphasisAlignement;
    //         cur++;
    //         if (cur > 11) cur = 0;
    //         else if (cur < 0) cur = 11;
    //         curEmphasisAlignement = (TextEmphasisAlignement)cur;
    //     }
    // }
    //
}
