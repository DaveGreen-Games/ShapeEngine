using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Text;

namespace ShapeEngine.Achievements;

/// <summary>
/// Handles the management, updating, and display of achievements and their stats.
/// </summary>
public class AchievementHandler
{
    /// <summary>
    /// Dictionary of achievement stats, keyed by API name.
    /// </summary>
    private Dictionary<string,AchievementStat> stats = new();
    /// <summary>
    /// List of all achievements managed by this handler.
    /// </summary>
    private List<Achievement> achievements = new();

    /// <summary>
    /// Stack of achievements to be drawn as notifications.
    /// </summary>
    private List<AchievmentDrawStack> achievementDrawStack = new();
    /// <summary>
    /// The font used for achievement text rendering.
    /// </summary>
    private TextFont textFont;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementHandler"/> class.
    /// </summary>
    /// <param name="textFont">The font used for achievement text rendering.</param>
    public AchievementHandler(TextFont textFont)
    {
        this.textFont = textFont;
    }

    /// <summary>
    /// Duration (in seconds) to display achieved notifications.
    /// </summary>
    public float achievedDisplayDuration = 5f;
    /// <summary>
    /// Duration (in seconds) to display increment notifications.
    /// </summary>
    public float notificationDuration = 3f;

    /// <summary>
    /// Updates the achievement notification stack.
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
    public void Update(float dt)
    {
        if(achievementDrawStack.Count > 0)
        {
            achievementDrawStack[0].Update(dt);
            if (achievementDrawStack[0].IsFinished())
            {
                achievementDrawStack.RemoveAt(0);
            }

        }
    }
    /// <summary>
    /// Draws the current achievement notification, if any.
    /// </summary>
    /// <param name="achievementRect">The rectangle area for drawing.</param>
    /// <param name="background">Background color.</param>
    /// <param name="text">Text color.</param>
    /// <param name="progress">Progress color.</param>
    /// <param name="achieved">Achieved color.</param>
    public void Draw(Rect achievementRect, ColorRgba background, ColorRgba text, ColorRgba progress, ColorRgba achieved) 
    {
        if (achievementDrawStack.Count > 0)
        {
            achievementDrawStack[0].achievement.Draw(textFont, achievementRect, background, text, progress, achieved);
        }
    }
    /// <summary>
    /// Clears all stats, achievements, and notification stacks.
    /// </summary>
    public void Close()
    {
        stats.Clear();
        achievements.Clear();
        achievementDrawStack.Clear();
    }
    /// <summary>
    /// Adds a new achievement stat if it does not already exist.
    /// </summary>
    /// <param name="stat">The achievement stat to add.</param>
    public  void AddStat(AchievementStat stat)
    {
        if(stats.ContainsKey(stat.apiName)) return;
        stats.Add(stat.apiName, stat);
    }
    /// <summary>
    /// Gets the value of a specified stat.
    /// </summary>
    /// <param name="stat">The API name of the stat.</param>
    /// <returns>The stat value, or -1 if not found.</returns>
    public  int GetStatValue(string stat)
    {
        if(!stats.TryGetValue(stat, out var stat1)) return -1;
        return stat1.value;
    }
    /// <summary>
    /// Updates the value of a specified stat by a given change.
    /// </summary>
    /// <param name="stat">The API name of the stat.</param>
    /// <param name="change">The value to add to the stat.</param>
    public  void UpdateStatValue(string stat, int change)
    {
        if (!stats.ContainsKey(stat)) return;
        stats[stat].ChangeStat(change);
    }
    /// <summary>
    /// Adds an achievement to the handler if not already present.
    /// Subscribes to achievement events if not achieved.
    /// </summary>
    /// <param name="achievement">The achievement to add.</param>
    public  void AddAchievement(Achievement achievement)
    {
        if(achievements.Contains(achievement)) return;
        achievements.Add(achievement);
        if(!achievement.IsAchieved()) 
        { 
            achievement.Achieved += OnAchievementAchieved;
            achievement.IncrementNotification += OnAchievementIncrementNotification;
        }
    }
    /// <summary>
    /// Removes an achievement from the handler and unsubscribes from its events.
    /// </summary>
    /// <param name="achievement">The achievement to remove.</param>
    public  void RemoveAchievment(Achievement achievement) 
    { 
        achievements.Remove(achievement);
        achievement.Achieved -= OnAchievementAchieved;
        achievement.IncrementNotification -= OnAchievementIncrementNotification;
    }
    /// <summary>
    /// Clears all achievements from the handler.
    /// </summary>
    public  void ClearAchievements() { achievements.Clear(); }

    /// <summary>
    /// Handles the event when an achievement is achieved, adding it to the notification stack.
    /// </summary>
    /// <param name="achievement">The achieved achievement.</param>
    private void OnAchievementAchieved(Achievement achievement)
    {
        achievementDrawStack.Add(new(achievedDisplayDuration, achievement));
        achievement.Achieved -= OnAchievementAchieved;
        achievement.IncrementNotification -= OnAchievementIncrementNotification;
    }
    /// <summary>
    /// Handles the event when an achievement increment notification is triggered.
    /// </summary>
    /// <param name="achievement">The achievement with increment notification.</param>
    private void OnAchievementIncrementNotification(Achievement achievement)
    {
        achievementDrawStack.Add(new(notificationDuration, achievement));
    }
}