using ShapeEngine.Color;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.StaticLib;
using ShapeEngine.Text;

namespace ShapeEngine.Achievements;

/// <summary>
/// Represents an achievement that tracks progress based on an associated <see cref="AchievementStat"/>.
/// Handles notifications for achievement completion and progress increments.
/// </summary>
public class Achievement
{
    /// <summary>
    /// Occurs when the achievement is achieved.
    /// </summary>
    public event Action<Achievement>? Achieved;
    /// <summary>
    /// Occurs when the achievement's progress reaches a notification increment.
    /// </summary>
    public event Action<Achievement>? IncrementNotification;

    /// <summary>
    /// The API name of the achievement.
    /// </summary>
    public string apiName;
    /// <summary>
    /// The display name of the achievement.
    /// </summary>
    public string displayName;

    /// <summary>
    /// Indicates whether the achievement has been achieved.
    /// </summary>
    protected bool achieved;
    /// <summary>
    /// Indicates whether the achievement is hidden (does not show description and display name).
    /// </summary>
    protected bool hidden;

    /// <summary>
    /// The stat associated with this achievement.
    /// </summary>
    protected AchievementStat stat;
    /// <summary>
    /// The starting value for progress tracking.
    /// </summary>
    protected int start;
    /// <summary>
    /// The end value required to achieve this achievement.
    /// </summary>
    protected int end;
    /// <summary>
    /// The increment value at which to notify progress.
    /// </summary>
    protected int notificationIncrement;

    /// <summary>
    /// Initializes a new instance of the <see cref="Achievement"/> class.
    /// </summary>
    /// <param name="apiName">The API name of the achievement.</param>
    /// <param name="displayName">The display name of the achievement.</param>
    /// <param name="hidden">Whether the achievement is hidden.</param>
    /// <param name="stat">The associated stat.</param>
    /// <param name="start">The starting value for progress.</param>
    /// <param name="end">The end value required to achieve.</param>
    /// <param name="notificationIncrement">The increment value for notifications.</param>
    public Achievement(string apiName, string displayName, bool hidden, AchievementStat stat, int start, int end, int notificationIncrement = 1)
    {
        this.apiName = apiName;
        this.displayName = displayName;
        this.hidden = hidden;

        this.stat = stat;
        this.start = start;
        this.end = end;
        this.notificationIncrement = notificationIncrement;

        if(IsGoalFinished())
        {
            achieved = true;
        }
        else
        {
            this.stat.OnValueChanged += OnStatValueChanged;
        }
    }

    /// <summary>
    /// Handles stat value changes to determine achievement progress and notifications.
    /// </summary>
    /// <param name="oldValue">The previous stat value.</param>
    /// <param name="newValue">The new stat value.</param>
    protected void OnStatValueChanged(int oldValue, int newValue)
    { 
        if(newValue >= end)
        {
            this.stat.OnValueChanged-= OnStatValueChanged;
            Achieve();
        }
        else
        {
            if(newValue >= start && notificationIncrement > 0 && newValue != oldValue && newValue % notificationIncrement == 0)
            {
                IncrementNotification?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Determines if the achievement's goal is currently active.
    /// </summary>
    /// <returns>True if the stat value is greater than or equal to the start value; otherwise, false.</returns>
    public bool IsGoalActive() { return stat.value >= start; }

    /// <summary>
    /// Determines if the achievement's goal has been finished.
    /// </summary>
    /// <returns>True if the stat value is greater than or equal to the end value; otherwise, false.</returns>
    public bool IsGoalFinished() { return stat.value >= end; }

    /// <summary>
    /// Gets the percentage of progress towards the achievement's goal.
    /// </summary>
    /// <returns>A value between 0 and 1 representing progress.</returns>
    public float GetGoalPercentage() { return ShapeMath.Clamp( (stat.value - start) / (float)(end - start), 0f, 1f); }

    /// <summary>
    /// Determines if the achievement is hidden.
    /// </summary>
    /// <returns>True if hidden; otherwise, false.</returns>
    public bool IsHidden() { return hidden; }

    /// <summary>
    /// Determines if the achievement has been achieved.
    /// </summary>
    /// <returns>True if achieved; otherwise, false.</returns>
    public bool IsAchieved() { return achieved; }

    /// <summary>
    /// Marks the achievement as achieved and triggers the <see cref="Achieved"/> event if not already achieved.
    /// </summary>
    public void Achieve() 
    {
        if (!achieved) Achieved?.Invoke(this);
        achieved = true; 
    }
    /// <summary>
    /// Draws the achievement's visual representation.
    /// </summary>
    /// <param name="textFont">The font used for text rendering.</param>
    /// <param name="rect">The rectangle area to draw in.</param>
    /// <param name="bgColorRgba">The background color.</param>
    /// <param name="textColorRgba">The text color.</param>
    /// <param name="progressColorRgba">The color representing progress.</param>
    /// <param name="achievedColorRgba">The color used when achieved.</param>
    public virtual void Draw(TextFont textFont, Rect rect, ColorRgba bgColorRgba, ColorRgba textColorRgba, ColorRgba progressColorRgba, ColorRgba achievedColorRgba)
    {
        Rect left = new(rect.X, rect.Y, rect.Width * 0.25f, rect.Height);
        Rect leftTop = new(left.X, left.Y, left.Width, left.Height * 0.5f);
        Rect leftBottom = new(left.X, left.Y +left.Height * 0.5f, left.Width, left.Height * 0.5f);
        Rect right = new(rect.X + rect.Width * 0.28f, rect.Y, rect.Width * 0.72f, rect.Height);
        rect.DrawBar(GetGoalPercentage(), progressColorRgba, bgColorRgba);
        if (achieved) rect.DrawLines(3f, achievedColorRgba);// SDrawing.DrawRect(rect, new(0f), 0f, 3f, achievedColor);
        int value = stat.value;
        int max = end;

        textFont.ColorRgba = textColorRgba;
        textFont.DrawTextWrapNone($"{value}", leftTop, new(0.5f));
        textFont.DrawTextWrapNone($"{max}", leftBottom, new(0.5f));
        // ShapeText.DrawText(font, String.Format("{0}", value), leftTop, 1f, new(0.5f) ,textColor);
        // ShapeText.DrawText(font, String.Format("{0}", max), leftBottom, 1f, new(0.5f),textColor);
        if (hidden)
        {
                
            if(achieved) textFont.DrawTextWrapNone(displayName, right, new(0.5f), achieved ? achievedColorRgba : textColorRgba); // ShapeText.DrawText(font, displayName, right, 1f, new(0.5f), achieved ? achievedColor : textColor);
            else textFont.DrawTextWrapNone("???", right, new(0.5f), textColorRgba); //ShapeText.DrawText(font, "???", right, 1f, new(0.5f), textColor);
        }
        else
        {
            textFont.ColorRgba = achieved ? achievedColorRgba : textColorRgba;
            textFont.DrawTextWrapNone(displayName, leftBottom, new(0.5f));
        }
    }
}