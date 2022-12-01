using System.ComponentModel;
using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals.UI;

namespace ShapeEngineCore.Globals.Achievements
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
        public string apiName = "";
        public string displayName = "";
        public string description = "";

        public bool achieved = false;
        public bool hidden = false; //doesnt show description and display name

        public List<AchievementGoal> goals = new();


        //progress, dt, return remaining
        public Func<float, float, float>? iconAchieved = null;
        public Func<float, float, float>? iconUnachieved = null;

        public Achievement(string apiName, string displayName, string description, bool achieved = false, bool hidden = false)
        {
            this.apiName = apiName;
            this.displayName = displayName;
            this.description = description;
            this.achieved = achieved;
            this.hidden = hidden;
        }

        //public Achievement(
        //    string apiName, string displayName, string description, AchievementStat stat,
        //    bool achieved = false, bool hidden = false)
        //{
        //    this.apiName = apiName;
        //    this.displayName = displayName;
        //    this.description = description;
        //    this.achieved = achieved;
        //    this.hidden = hidden;
        //    this.stat = stat;
        //}
        //public Achievement(
        //    string apiName, string displayName, string description,
        //    AchievementStat stat,
        //    Func<float, float, float> iconUnachieved, Func<float, float, float> iconAchieved,
        //    bool achieved = false, bool hidden = false)
        //{
        //    this.apiName = apiName;
        //    this.displayName = displayName;
        //    this.description = description;
        //    this.achieved = achieved;
        //    this.hidden = hidden;
        //    this.stat = stat;
        //    this.iconAchieved = iconAchieved;
        //    this.iconUnachieved = iconUnachieved;
        //}
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
