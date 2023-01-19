using System.Numerics;
using Raylib_CsLo;
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
    public class AchievementGoal
    {
        protected AchievementStat stat;
        protected int goal = 0;
        protected bool finished = false;
        public event Action<AchievementGoal, AchievementStat, int>? GoalFinished;

        public AchievementGoal(AchievementStat stat, int goal)
        {
            this.stat = stat;
            this.goal = goal;

            if(stat.value >= goal)
            {
                finished= true;
            }
            else this.stat.OnValueChanged += OnStatValueChanged;
        }

        public bool IsFinished() { return finished; }
        protected void OnStatValueChanged(int oldValue, int newValue) 
        { 
            if(newValue >= goal)
            {
                this.stat.OnValueChanged-= OnStatValueChanged;
                finished = true;
                GoalFinished?.Invoke(this, stat, goal);
            }
        }
    }
    public class Achievement
    {

        public event Action<Achievement>? Achieved;
        public delegate float IconDrawer(Rectangle rect, float progress, Color color, float dt);

        public string apiName = "";
        public string displayName = "";
        public string description = "";

        protected bool achieved = false;
        protected bool hidden = false; //doesnt show description and display name

        protected List<AchievementGoal> goals = new();
        protected int achievedGoals = 0;

        protected IconDrawer? icon = null;

        public Achievement(string apiName, string displayName, string description, bool hidden, params AchievementGoal[] goals)
        {
            this.apiName = apiName;
            this.displayName = displayName;
            this.description = description;
            this.hidden = hidden;
            
            this.goals = goals.ToList();
            foreach (var goal in goals)
            {
                if (!goal.IsFinished())
                {
                    goal.GoalFinished += OnGoalFinished;
                }
                else achievedGoals++;
            }

            if(achievedGoals >= goals.Length)
            {
                Achieve();
            }
        }

        protected void OnGoalFinished(AchievementGoal goal, AchievementStat stat, int goalValue)
        {
            goal.GoalFinished -= OnGoalFinished;
            achievedGoals++;

            if (achievedGoals >= goals.Count)
            {
                Achieve();
            }
        }

        public void SetIconDrawer(IconDrawer icon) { this.icon = icon; }

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

        public virtual void Draw(Rectangle rect, Color bgColor, Color iconAchieved, Color iconUnachieved, Color textColor, Color progressColor, float dt)
        {

            //draw background
            SDrawing.DrawRectangle(rect, bgColor);


            float margin = 0.05f;
            Rectangle inside = new(rect.x + rect.width * margin, rect.y + rect.height * margin, rect.width * (1f - margin * 2f), rect.height * (1f - margin * 2f));
            Rectangle iconRect = new(inside.x, inside.y, inside.width * 0.25f, inside.height);

            //Draw Icon
            if (icon != null) icon.Invoke(rect, 1f, achieved ? iconAchieved: iconUnachieved, dt);

            //draw title
            


            //draw description

            //draw progress bar
        }
    }


    


    public static class AchievementHandler
    {
        
    }
}
