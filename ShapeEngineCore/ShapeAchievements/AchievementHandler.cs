using System.Numerics;
using Raylib_CsLo;
using ShapeUI;

namespace ShapeAchievements
{
    public class AchievementGoal
    {
        public AchievementStat stat;
        public int goal = 0;
        public int updateIncrement = -1;
        public bool finished = false;

        public AchievementGoal(AchievementStat stat, int goal, bool finished)
        {
            this.stat = stat;
            this.goal = goal;
            this.finished = finished;
        }
    }


    public class Achievement
    {

        public event Action<Achievement>? Achieved;
        public delegate float IconDrawer(Vector2 pos, Vector2 size, Vector2 alignement, float progress, float dt);

        public string apiName = "";
        public string displayName = "";
        public string description = "";

        protected bool achieved = false;
        protected bool hidden = false; //doesnt show description and display name

        public List<AchievementGoal> goals = new();


        protected IconDrawer? iconAchieved = null;
        protected IconDrawer? iconUnachieved = null;

        public Achievement(string apiName, string displayName, string description, bool hidden)
        {
            this.apiName = apiName;
            this.displayName = displayName;
            this.description = description;
            this.hidden = hidden;
        }

        public void AddGoal(AchievementStat stat, int goal, bool finished) { AddGoal(new(stat, goal, finished)); }
        public void AddGoal(AchievementGoal goal) { goals.Add(goal); }
        public void AddGoals(params AchievementGoal[] goals) { this.goals.AddRange(goals); }

        public void SetIconAchieved(IconDrawer iconAchieved) { this.iconAchieved = iconAchieved; }
        public void SetIconUnachieved(IconDrawer iconUnachieved) { this.iconUnachieved = iconUnachieved; }

        public bool IsHidden() { return hidden; }
        public bool IsAchieved() { return achieved; }
        public void Achieve() 
        {
            if (!achieved) Achieved?.Invoke(this);
            achieved = true; 
        }

        //public void Update(float dt)
        //{
        //
        //}

        public void Draw(Vector2 pos, Vector2 size, Vector2 alignement, float dt)
        {

        }
    }


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

        //private int Abs(int value)
        //{
        //    if (value >= 0) return value;
        //    else return value * -1;
        //}
    }




    public static class AchievementHandler
    {
        
    }
}
