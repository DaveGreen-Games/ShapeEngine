using System.Numerics;
using System.Reflection.Metadata;
using Raylib_CsLo;
using ShapeColor;
using ShapeLib;
using ShapeUI;

namespace ShapeAchievements
{
    public class AchievementStat
    {
        public string apiName = "";
        public string displayName = "";

        public int value = 0;
        public int defaultValue = 0;
        public int maxValue = int.MaxValue;
        public int minValue = int.MinValue;


        public event Action<int, int>? OnValueChanged;

        public AchievementStat(string apiName, string displayName, int defaultValue, int maxValue = int.MaxValue, int minValue = int.MinValue)
        {
            this.apiName = apiName;
            this.displayName = displayName;
            this.maxValue = maxValue;
            this.minValue = minValue;
            this.defaultValue = defaultValue;
            if (this.defaultValue < minValue) this.defaultValue = minValue;
            else if (this.defaultValue > maxValue) this.defaultValue = maxValue;
            this.value = this.defaultValue;
        }

        public void Reset() { SetStat(defaultValue); }
        public void ChangeStat(int change)
        {
            if (change == 0) return;

            int oldValue = value;
            value += change;
            if (value < minValue) value = minValue;
            else if (value > maxValue) value = maxValue;

            if (oldValue != value) OnValueChanged?.Invoke(oldValue, value);
        }

        public void SetStat(int newValue) { ChangeStat(newValue - value); }

    }
    
    public class Achievement
    {
        public event Action<Achievement>? Achieved;
        public event Action<Achievement>? IncrementNotification;

        public string apiName = "";
        public string displayName = "";

        protected bool achieved = false;
        protected bool hidden = false; //doesnt show description and display name

        protected AchievementStat stat;
        protected int start;
        protected int end;
        protected int notificationIncrement = 1;

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


        public bool IsGoalActive() { return stat.value >= start; }
        public bool IsGoalFinished() { return stat.value >= end; }
        public float GetGoalPercentage() { return Clamp( (float)(stat.value - start) / (float)(end - start), 0f, 1f); }

        public bool IsHidden() { return hidden; }
        public bool IsAchieved() { return achieved; }
        public void Achieve() 
        {
            if (!achieved) Achieved?.Invoke(this);
            achieved = true; 
        }
        public virtual void Draw(Rectangle rect, Color bgColor, Color textColor, Color progressColor, Color achievedColor)
        {
            Rectangle left = new(rect.x, rect.y, rect.width * 0.25f, rect.height);
            Rectangle leftTop = new(left.x, left.y, left.width, left.height * 0.5f);
            Rectangle leftBottom = new(left.x, left.y +left.height * 0.5f, left.width, left.height * 0.5f);
            Rectangle right = new(rect.x + rect.width * 0.28f, rect.y, rect.width * 0.72f, rect.height);
            SDrawing.DrawBar(rect, GetGoalPercentage(), progressColor, bgColor);
            if (achieved) SDrawing.DrawRectangeLinesPro(new(rect.x, rect.y), new(rect.width, rect.height), new(0f), new(0f), 0f, 3f, achievedColor);
            int value = stat.value;
            int max = end;
            SDrawing.DrawTextAligned(String.Format("{0}", value), leftTop, 1f, textColor, new(0.5f));
            SDrawing.DrawTextAligned(String.Format("{0}", max), leftBottom, 1f, textColor, new(0.5f));
            if (hidden)
            {
                if(achieved) SDrawing.DrawTextAligned(displayName, right, 1f, achieved ? achievedColor : textColor, new(0.5f));
                else SDrawing.DrawTextAligned("???", right, 1f, textColor, new(0.5f));
            }
            else SDrawing.DrawTextAligned(displayName, right, 1f, achieved ? achievedColor : textColor, new(0.5f));
        }
    }


    internal class AchievmentDrawStack
    {
        public float duration;
        public Achievement achievement;
        public AchievmentDrawStack(float duration, Achievement achievement)
        {
            this.duration = duration;
            this.achievement = achievement;
        }
        
        public void Update(float dt)
        {
            if (duration <= 0f) return;

            duration -= dt;
        }
        public bool IsFinished() { return duration <= 0f; }
    }


    public static class AchievementHandler
    {
        private static Dictionary<string,AchievementStat> stats = new();
        private static List<Achievement> achievements = new();

        private static List<AchievmentDrawStack> achievementDrawStack = new();
        

        private static Color bgColor = GRAY;
        private static Color achievedColor = YELLOW;
        private static Color textColor = WHITE;
        private static Color progressColor = BLUE;

        public static float achievedDisplayDuration = 5f;
        public static float notificationDuration = 3f;
        public static void Initialize()
        {

        }

        public static void SetColors(Color bgColor, Color textColor, Color progressColor, Color achievedColor)
        {

        }
        public static void Update(float dt)
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
        public static void Draw(Rectangle achievementRect) 
        {
            if (achievementDrawStack.Count > 0)
            {
                achievementDrawStack[0].achievement.Draw(achievementRect, bgColor, textColor, progressColor, achievedColor);
            }
        }

        public static void AddStat(AchievementStat stat)
        {
            if(stats.ContainsKey(stat.apiName)) return;
            stats.Add(stat.apiName, stat);
        }
        //public static void RemoveStat(AchievementStat stat)
        //{
        //    stats.Remove(stat.apiName);
        //}
        //public static void ClearStats() { stats.Clear(); }
        
        public static int GetStatValue(string stat)
        {
            if(!stats.ContainsKey(stat)) return -1;
            else return stats[stat].value;
        }
        public static void UpdateStatValue(string stat, int change)
        {
            if (!stats.ContainsKey(stat)) return;
            stats[stat].ChangeStat(change);
        }

        public static void AddAchievement(Achievement achievement)
        {
            if(achievements.Contains(achievement)) return;
            achievements.Add(achievement);
            if(!achievement.IsAchieved()) 
            { 
                achievement.Achieved += OnAchievementAchieved;
                achievement.IncrementNotification += OnAchievementIncrementNotification;
            }
        }
        public static void RemoveAchievment(Achievement achievement) 
        { 
            achievements.Remove(achievement);
            achievement.Achieved -= OnAchievementAchieved;
            achievement.IncrementNotification -= OnAchievementIncrementNotification;
        }
        public static void ClearAchievements() { achievements.Clear(); }

        private static void OnAchievementAchieved(Achievement achievement)
        {
            achievementDrawStack.Add(new(achievedDisplayDuration, achievement));
            achievement.Achieved -= OnAchievementAchieved;
            achievement.IncrementNotification -= OnAchievementIncrementNotification;
        }
        private static void OnAchievementIncrementNotification(Achievement achievement)
        {
            achievementDrawStack.Add(new(notificationDuration, achievement));
        }
    }
}
