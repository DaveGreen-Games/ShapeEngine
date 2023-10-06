using Raylib_CsLo;
using ShapeEngine.Core;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace Examples.Scenes.ExampleScenes
{
    public class InputExample : ExampleScene
    {
        Font font;
        
        public InputExample()
        {
            Title = "Input Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);
                
        }

        
        public override void Activate(IScene oldScene)
        {
            
        }

        public override void Deactivate()
        {
            
        }
        public override GameObjectHandler? GetGameObjectHandler()
        {
            return null;
        }
        public override void Reset()
        {
            

        }
        
        
        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            

        }
        protected override void UpdateExample(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            
        }
        protected override void DrawGameExample(ScreenInfo game)
        {
            
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            
        }
        protected override void DrawUIExample(ScreenInfo ui)
        {
            Vector2 uiSize = ui.Area.Size;
            
            // Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            // string infoText = $"{moveText} | {rotText} | {scaleText} | {shakeText}";
            // font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);

        }
    }

}
