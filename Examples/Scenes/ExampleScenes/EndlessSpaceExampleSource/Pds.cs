using System.Numerics;
using Examples.PayloadSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.StaticLib;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class Pds : PayloadDeliverySystem
{
    private readonly List<PayloadKey> keys;
    private readonly List<KeyDirection> sequence = new();
    public bool SequenceFailed { get; private set; } = false;
    
    public Pds(PdsInfo info, Vector2 position, IPayloadTargetingSystem targetingSystem, CollisionHandler colHandler, BitFlag castMask, params KeyDirection[] keyDirections) : base(info, targetingSystem, new PayloadConstructor(colHandler, castMask), position)
    {
        keys = new();
        foreach (var key in keyDirections)
        {
            keys.Add(new(key));
        }
    }

    protected override void WasDrawn()
    {
        if (CallInF > 0f && curMarker != null)
        {
            TargetingSystem.DrawTargetArea(CallInF, Colors.PcSpecial.ColorRgba);
            // ShapeDrawing.DrawCircleLines(curMarker.Location, 500 * CallInF, 6f, Colors.Special);
        }
    }

    public void ResetSequence()
    {
        sequence.Clear();
        SequenceFailed = false;
    }
    public bool KeyPressed(KeyDirection direction)
    {
        if (!IsReady) return false;
        if (SequenceFailed) return false;
        
        sequence.Add(direction);
        var index = sequence.Count - 1;
        var correct = keys[index].Direction == direction;
        if (!correct)
        {
            SequenceFailed = true;
            return false;
        }

        if (sequence.Count < keys.Count) return false;
        
        return true;
    }
    public override void DrawUI(Rect rect)
    {

        var split = rect.SplitV(0.6f);
        var topRect = split.top.ApplyMargins(0f, 0f, 0f, 0.1f);
        var bottomRect = split.bottom.ApplyMargins(0f, 0f, 0.1f, 0f);

        var dirBaseColor = Colors.PcMedium;
        if (!IsReady || SequenceFailed) dirBaseColor = Colors.PcDark;
        var rects = topRect.SplitH(keys.Count);
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            if (sequence.Count > 0 && !SequenceFailed)
            {
                if(i < sequence.Count) key.Draw(rects[i], Colors.PcSpecial.ColorRgba);
                else key.Draw(rects[i], dirBaseColor.ColorRgba);
            }
            else key.Draw(rects[i], dirBaseColor.ColorRgba);
        }
        
        if (CooldownF > 0f)
        {
            var marginRect = bottomRect.ApplyMargins(0f, CooldownF, 0f, 0f);
            marginRect.Draw(Colors.Warm.ChangeBrightness(-0.5f));
            bottomRect.DrawLines(2f, Colors.Warm);
        }
        else if(CallInF > 0f)
        {
            var c = Colors.PcSpecial.ColorRgba;
            float f = 1f - CallInF;
            var marginRect = bottomRect.ApplyMargins(0f, f, 0f, 0f);
            marginRect.Draw(c.ChangeBrightness(-0.5f));
            bottomRect.DrawLines(2f, c);
        }
        else if (curMarker != null && curMarker.TravelF > 0f)
        {
            var c = Colors.PcMedium.ColorRgba;
            float f = 1f - curMarker.TravelF;
            var marginRect = bottomRect.ApplyMargins(0f, f, 0f, 0f);
            marginRect.Draw(c.ChangeBrightness(-0.5f));
            bottomRect.DrawLines(2f, c);
        }
        else if (ActiveF > 0f)
        {
            var c = Colors.PcMedium.ColorRgba;
            float f = 1f - ActiveF;
            var marginRect = bottomRect.ApplyMargins(0f, f, 0f, 0f);
            marginRect.Draw(c.ChangeBrightness(-0.5f));
            bottomRect.DrawLines(2f, c);
        }
        else
        {
            bottomRect.Draw(Colors.Cold.ChangeBrightness(-0.5f));
            bottomRect.DrawLines(2f, Colors.Cold);
        }
    }
}