
using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Screen
{
    /*public interface IMonitorDevice
    {
        public delegate void MonitorChanged(MonitorInfo oldMonitor, MonitorInfo newMonitor);
        public event MonitorChanged? OnMonitorChanged;

        public delegate void MonitorSetupChanged(List<MonitorInfo> newSetup);
        public event MonitorSetupChanged? OnMonitorSetupChanged;

        public MonitorInfo CurMonitor();
        public int MonitorCount();
        public List<MonitorInfo> GetAllMonitorInfo();
        public MonitorInfo HasMonitorSetupChanged();
        public MonitorInfo SetMonitor(int newMonitor);
        public MonitorInfo NextMonitor();
        public MonitorInfo PrevMonitor();

    }*/
    public sealed class MonitorDevice // : IMonitorDevice
    {
        public delegate void MonitorChanged(MonitorInfo oldMonitor, MonitorInfo newMonitor);
        public event MonitorChanged? OnMonitorChanged;

        public delegate void MonitorSetupChanged(List<MonitorInfo> newSetup);
        public event MonitorSetupChanged? OnMonitorSetupChanged;
        
        private List<MonitorInfo> monitors = new();
        private int curIndex = 0;
        private int monitorCount = 0;

        public MonitorDevice()
        {
            GenerateInfo();
        }
        public MonitorInfo CurMonitor()
        {
            if (monitors.Count <= 0) return new();
            else return monitors[curIndex];
        }
        public int MonitorCount() { return monitors.Count; }
        public List<MonitorInfo> GetAllMonitorInfo() { return monitors; }
        public bool IsValidIndex(int index)
        {
            return index >= 0 && index < monitors.Count;
        }
        private void GenerateInfo()
        {
            curIndex = GetCurrentMonitor();
            monitorCount = GetMonitorCount();
            monitors.Clear();
            for (int i = 0; i < monitorCount; i++)
            {
                string name = GetMonitorName_(i);
                int w =  GetMonitorWidth(i);
                int h =  GetMonitorHeight(i);
                Vector2 pos = GetMonitorPosition(i) + new Vector2(1, 1);
                
                int rr = GetMonitorRefreshRate(i);
                monitors.Add(new(name, w, h, pos, rr, i));
            }
        }
        public (bool changed, int oldIndex, int newIndex) HasIndexChanged()
        {
            int index = GetCurrentMonitor();
            if (curIndex != index && IsValidIndex(index))
            {
                int oldIndex = curIndex;
                curIndex = index;
                OnMonitorChanged?.Invoke(monitors[oldIndex], monitors[index]);
                return (true, oldIndex, curIndex);
            }
            return (false, -1, -1);
        }

        public MonitorInfo HasMonitorSetupChanged()
        {

            int currentMonitorCount = GetMonitorCount();
            if (currentMonitorCount == monitorCount) return new();
            int dif = currentMonitorCount - monitorCount;
            if (dif > 0) //new monitors added -> only update monitor list
            {
                GenerateInfo();
                OnMonitorSetupChanged?.Invoke(monitors);
                return new();
            }
            else //monitors removed
            {
                //string currentMonitorName = GetName();
                var oldMonitor = Get();
                GenerateInfo();
                OnMonitorSetupChanged?.Invoke(monitors);

                var monitor = monitors.Find((MonitorInfo mi) => mi.Name == oldMonitor.Name);
                if (!monitor.Available) //current monitor was removed
                {
                    var newMonitor = Get();
                    OnMonitorChanged?.Invoke(oldMonitor, newMonitor);
                    return newMonitor;
                }
                else//current monitor is still in the list
                {
                    if(monitor.Index != oldMonitor.Index)//just update the index in case the monitors index has changed
                    {
                        curIndex = monitor.Index;
                    }
                    return new();
                }

                //if (currentMonitorName != GetName()) return Get();//the current monitor was removed
                //else return new();//current monitor still exists so it was not removed
            }
        }

        public bool SetCurIndex(int index)
        {
            if (!IsValidIndex(index)) return false;
            curIndex = index;
            return true;
        }
        public int GetCurIndex() { return curIndex; }
        public Dimensions GetSize(int monitorIndex)
        {
            if (monitorIndex < 0 || monitorIndex >= monitors.Count) return new(-1, -1);
            return monitors[monitorIndex].Dimensions;
            //return (monitors[monitorIndex].width, monitors[monitorIndex].height);
        }
        public Dimensions GetSize() { return GetSize(curIndex); }
        public int GetRefreshrate(int monitorIndex)
        {
            if (monitorIndex < 0 || monitorIndex >= monitors.Count) return -1;
            return monitors[monitorIndex].Refreshrate;
        }
        public int GetRefreshrate() { return GetRefreshrate(curIndex); }
        public string GetName(int monitorIndex)
        {
            if (monitorIndex < 0 || monitorIndex >= monitors.Count) return "";
            return monitors[monitorIndex].Name;
        }
        public string GetName() { return GetName(curIndex); }
        public MonitorInfo Get(int monitorIndex)
        {
            if (monitorIndex < 0 || monitorIndex >= monitors.Count) return new();
            return monitors[monitorIndex];
        }
        public MonitorInfo Get() { return Get(curIndex); }

        public MonitorInfo SetMonitor(int newMonitor)
        {
            if (!IsValidIndex(newMonitor)) return new();
            if (newMonitor == curIndex) return new();
            curIndex = newMonitor;
            return Get();
        }
        public MonitorInfo NextMonitor()
        {
            int oldIndex = curIndex;
            curIndex += 1;
            if (curIndex >= monitors.Count) curIndex = 0;
            if (oldIndex == curIndex) return new();
            return Get();
        }
        public MonitorInfo PrevMonitor()
        {
            int oldIndex = curIndex;
            curIndex -= 1;
            if (curIndex < 0) curIndex = monitors.Count - 1;
            if (oldIndex == curIndex) return new();
            return Get();
        }
    
    }
    public struct MonitorInfo
    {
        public bool Available = false;
        public string Name = "";
        public Dimensions Dimensions = new();
        public int Refreshrate = -1;
        public int Index = -1;
        public Vector2 Position = new();

        public int Width { get { return Dimensions.Width; } }
        public int Height { get { return Dimensions.Height; } }

        public MonitorInfo()
        {
            this.Available = false;
            this.Name = "";
            this.Dimensions = new(-1, -1);
            this.Refreshrate = -1;
            this.Index = -1;
            this.Position = new();
        }
        public MonitorInfo(string name, int w, int h, Vector2 pos, int refreshrate, int index)
        {
            this.Available = true;
            this.Name = name;
            this.Dimensions = new(w, h);
            this.Refreshrate = refreshrate;
            this.Index = index;
            this.Position = pos;
        }

        public void WriteDebugInfo()
        {
            Console.WriteLine("---Monitor Info---");
            Console.WriteLine($"{Name}: x({Dimensions.Width}) y({Dimensions.Height}) i({Index}) rr({Refreshrate}) ");
            Console.WriteLine("------------------");
        }
    }

}
