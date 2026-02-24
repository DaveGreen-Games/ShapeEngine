using Examples.PayloadSystem;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.StaticLib;

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
            var circle = new Circle(Location, 15f);
            if (visible)
            {
                circle.Draw(Colors.Cold, 0.2f);
            }

            circle = circle.SetRadius(25f);
            circle.DrawSectorLines(0, 359 * TravelF, 0f, new LineDrawingInfo(4f, Colors.Cold), 0.2f);
        }
        else
        {
            var circle = new Circle(Location, 15f);
            circle.Draw(Colors.Cold, 0.2f);
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