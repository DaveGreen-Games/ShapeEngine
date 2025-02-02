using Examples.PayloadSystem;
using ShapeEngine.Lib;
using ShapeEngine.Lib.Drawing;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class PayloadMarkerSimple : PayloadMarker
{
    private bool visible = false;
    private float blinkTimer = 0f;
    private float blinkInterval = 0.1f;
    
    
    public override void Draw()
    {
        if (!Launched) return;

        if (TravelF > 0f)
        {
            if (visible)
            {
                CircleDrawing.DrawCircle(Location, 15, Colors.Cold, 24);
            }  
            CircleDrawing.DrawCircleSectorLines(Location, 25f, 0, 359 * TravelF, 4f, Colors.Cold, false, 4f);
        }
        else
        {
            CircleDrawing.DrawCircle(Location, 15, Colors.Cold, 24);
        }
        
        
    }

    protected override void TargetWasReached()
    {
        
    }

    protected override void WasDismissed()
    {
       
    }

    protected override void WasLaunched()
    {
        blinkTimer = 0f;
        blinkInterval = travelTime / 8;
        visible = true;
    }

    protected override void OnUpdate(float dt)
    {
        if (TravelF > 0f)
        {
            blinkTimer += dt;
            visible = ShapeMath.Blinking(blinkTimer, blinkInterval);
        }
        
    }
}