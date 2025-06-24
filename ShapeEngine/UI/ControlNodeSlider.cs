using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.StaticLib;

namespace ShapeEngine.UI;

/// <summary>
/// A slider control node for UI, supporting horizontal or vertical orientation, value range, and user interaction.
/// </summary>
/// <remarks>
/// Supports value changes via mouse and keyboard/gamepad navigation.
/// The slider can be configured for snapping, step increments, and orientation.
/// </remarks>
public class ControlNodeSlider : ControlNode
{
    /// <summary>
    /// Occurs when the slider value changes.
    /// </summary>
    public event Action<float, float>? OnValueChanged;
    
    /// <summary>
    /// Gets or sets whether the slider is horizontal (true) or vertical (false).
    /// </summary>
    public bool Horizontal;
    /// <summary>
    /// The minimum value of the slider.
    /// </summary>
    public float MinValue;
    /// <summary>
    /// The maximum value of the slider.
    /// </summary>
    public float MaxValue;
    /// <summary>
    /// Gets the current value of the slider, interpolated between MinValue and MaxValue.
    /// </summary>
    public float CurValue => ShapeMath.LerpFloat(MinValue, MaxValue, CurF);
    /// <summary>
    /// Gets the current normalized value (0-1) of the slider.
    /// </summary>
    public float CurF { get; private set; } = 0f;
    /// <summary>
    /// The step size (normalized) for keyboard/gamepad value changes.
    /// </summary>
    public float StepF = 0.05f;
    /// <summary>
    /// Snap value for mouse changes (normalized). Set to 0 or less to disable snapping.
    /// </summary>
    public float MouseSnapF = 0f;
    /// <summary>
    /// Gets the fill rectangle representing the current value.
    /// </summary>
    public Rect Fill { get; private set; }

    /// <summary>
    /// Initializes a new horizontal slider with default range 0-100.
    /// </summary>
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
    /// <summary>
    /// Initializes a new slider with specified orientation and default range 0-100.
    /// </summary>
    /// <param name="horizontal">If true, slider is horizontal; otherwise, vertical.</param>
    public ControlNodeSlider(bool horizontal)
    {
        MinValue = 0;
        MaxValue = 100;
        Horizontal = horizontal;
        SetCurValue(0);
        Fill = Rect;
    }
    /// <summary>
    /// Initializes a new slider with specified maximum value and orientation.
    /// </summary>
    /// <param name="maxValue">Maximum value of the slider.</param>
    /// <param name="horizontal">If true, slider is horizontal; otherwise, vertical.</param>
    public ControlNodeSlider(float maxValue, bool horizontal = true)
    {
        MinValue = 0;
        MaxValue = maxValue;
        Horizontal = horizontal;
        SetCurValue(0);
        Fill = Rect;
    }
    /// <summary>
    /// Initializes a new slider with specified minimum, maximum value and orientation.
    /// </summary>
    /// <param name="minValue">Minimum value of the slider.</param>
    /// <param name="maxValue">Maximum value of the slider.</param>
    /// <param name="horizontal">If true, slider is horizontal; otherwise, vertical.</param>
    public ControlNodeSlider(float minValue, float maxValue, bool horizontal = true)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        Horizontal = horizontal;
        SetCurValue(minValue);
        Fill = Rect;
    }
    /// <summary>
    /// Initializes a new slider with specified start, min, max value and orientation.
    /// </summary>
    /// <param name="startValue">Initial value of the slider.</param>
    /// <param name="minValue">Minimum value of the slider.</param>
    /// <param name="maxValue">Maximum value of the slider.</param>
    /// <param name="horizontal">If true, slider is horizontal; otherwise, vertical.</param>
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

    /// <summary>
    /// Sets the normalized value (0-1) of the slider.
    /// </summary>
    /// <param name="f">The normalized value to set (clamped between 0 and 1).</param>
    /// <returns>True if the value was changed; otherwise, false.</returns>
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
    /// <summary>
    /// Sets the slider value.
    /// </summary>
    /// <param name="value">The value to set (clamped between MinValue and MaxValue).</param>
    /// <returns>True if the value was changed; otherwise, false.</returns>
    public bool SetCurValue(float value)
    {
        if (!Active) return false;
        var f = ShapeMath.LerpInverseFloat(MinValue, MaxValue, value);
        return SetCurF(f);
    }
    /// <summary>
    /// Changes the slider value by a specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the value by.</param>
    /// <returns>True if the value was changed; otherwise, false.</returns>
    public bool ChangeValue(float amount)
    {
        if (!Active) return false;

        var v = ShapeMath.Clamp(CurValue + amount, MinValue, MaxValue);
        SetCurValue(v);
        
        return true;
    }
    /// <summary>
    /// Called to check if the decrease value input is pressed. Override to provide input logic.
    /// </summary>
    /// <returns>True if decrease input is pressed; otherwise, false.</returns>
    protected virtual bool GetDecreaseValuePressed() => false;
    /// <summary>
    /// Called to check if the increase value input is pressed. Override to provide input logic.
    /// </summary>
    /// <returns>True if increase input is pressed; otherwise, false.</returns>
    protected virtual bool GetIncreaseValuePressed() => false;
    /// <summary>
    /// Called every update to handle slider logic and fill calculation.
    /// </summary>
    /// <param name="dt">Delta time.</param>
    /// <param name="mousePos">Current mouse position.</param>
    /// <param name="mousePosValid">Whether the mouse position is valid.</param>
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
    /// <summary>
    /// Handles updating the fill rectangle based on the current value and mouse interaction.
    /// </summary>
    /// <param name="mousePos">Current mouse position.</param>
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
    /// <summary>
    /// Called when the value changes. Override to handle value change events.
    /// </summary>
    /// <param name="prevValue">Previous value.</param>
    /// <param name="curValue">Current value.</param>
    protected virtual void ValueHasChanged(float prevValue, float curValue) { }
    
    private void ResolveValueChange(float prevValue, float curValue)
    {
        ValueHasChanged(prevValue, curValue);
        OnValueChanged?.Invoke(prevValue, curValue);
    }
    
    private Vector2 GetSizeFactor() => Horizontal ? new Vector2(CurF, 1f) : new Vector2(1f, CurF);
}