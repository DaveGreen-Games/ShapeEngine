using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.UI;

public class ControlNodeSlider : ControlNode
{
    public event Action<float, float>? OnValueChanged;
    
    public bool Horizontal;
    public float MinValue;
    public float MaxValue;
    
    public float CurValue => ShapeMath.LerpFloat(MinValue, MaxValue, CurF);
    public float CurF { get; private set; } = 0f;
    
    /// <summary>
    /// How much to increase/decrease CurF if Decreased/Increased input is pressed
    /// </summary>
    public float StepF = 0.05f;

    /// <summary>
    /// Snap value for changing the value of the slider via mouse. <= 0 means disabled.
    /// </summary>
    public float MouseSnapF = 0f;
    
    public Rect Fill { get; private set; }

    public ControlNodeSlider()
    {
        MinValue = 0;
        MaxValue = 100;
        Horizontal = true;
        SetCurValue(0);
        Fill = Rect;
        
        SelectionFilter = SelectFilter.All;
        MouseFilter = MouseFilter.Pass;
        InputFilter = InputFilter.All;
    }
    public ControlNodeSlider(bool horizontal)
    {
        MinValue = 0;
        MaxValue = 100;
        Horizontal = horizontal;
        SetCurValue(0);
        Fill = Rect;
    }
    public ControlNodeSlider(float maxValue, bool horizontal = true)
    {
        MinValue = 0;
        MaxValue = maxValue;
        Horizontal = horizontal;
        SetCurValue(0);
        Fill = Rect;
    }
    public ControlNodeSlider(float minValue, float maxValue, bool horizontal = true)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        Horizontal = horizontal;
        SetCurValue(minValue);
        Fill = Rect;
    }
    public ControlNodeSlider(float startValue, float minValue, float maxValue, bool horizontal = true)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        Horizontal = horizontal;
        SetCurValue(startValue);
        Fill = Rect;
        
        SelectionFilter = SelectFilter.All;
        MouseFilter = MouseFilter.Pass;
        InputFilter = InputFilter.All;
    }

    
    public bool SetCurF(float f)
    {
        if (!Active) return false;
        var prevF = CurF;
        var newF = ShapeMath.Clamp(f, 0f, 1f);
        
        if (Math.Abs(prevF - newF) > 0.0001f)
        {
            ResolveValueChange(prevF, newF);
        }

        CurF = newF;
        return true;
    }
    public bool SetCurValue(float value)
    {
        if (!Active) return false;
        var f = ShapeMath.LerpInverseFloat(MinValue, MaxValue, value);
        return SetCurF(f);
    }

    public bool ChangeValue(float amount)
    {
        if (!Active) return false;

        var v = ShapeMath.Clamp(CurValue + amount, MinValue, MaxValue);
        SetCurValue(v);
        
        return true;
    }
    
    protected virtual bool GetDecreaseValuePressed() => false;
    protected virtual bool GetIncreaseValuePressed() => false;
    protected override void OnUpdate(float dt, Vector2 mousePos, bool mousePosValid)
    {
        if (!Active) return;
        
        if (StepF > 0 && Selected)
        {
            if (GetDecreaseValuePressed()) SetCurF(CurF - StepF);
            else if (GetIncreaseValuePressed()) SetCurF(CurF + StepF);
        }
        
        HandleFill(mousePos);
    }

    protected virtual void HandleFill(Vector2 mousePos)
    {
        // if (!Active) return;
        if (MouseInside)
        {
            if (Pressed)
            {
                float f = Horizontal ? Rect.GetWidthFactor(mousePos.X) : Rect.GetHeightFactor(mousePos.Y);
                if (MouseSnapF > 0)
                {
                    var snap = (f % MouseSnapF);
                    if (snap > MouseSnapF / 2) //snap to next bigger value
                    {
                        f = (f - snap) + MouseSnapF;
                    }
                    else //snap to lower value
                    {
                        f -= snap;
                    }
                }
                SetCurF(f);
            }
        }
        Fill = Rect.SetSize(Rect.Size * GetSizeFactor());
    }

    protected virtual void ValueHasChanged(float prevValue, float curValue) { }

    private void ResolveValueChange(float prevValue, float curValue)
    {
        ValueHasChanged(prevValue, curValue);
        OnValueChanged?.Invoke(prevValue, curValue);
    }
    
    private Vector2 GetSizeFactor() => Horizontal ? new Vector2(CurF, 1f) : new Vector2(1f, CurF);
}