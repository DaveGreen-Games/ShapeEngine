namespace ShapeEngineCore.Globals.Screen
{
    public class MonitorHandler
    {
        public struct MonitorInfo
        {
            public bool available = false;
            public string name = "";
            public int width = -1;
            public int height = -1;
            public int refreshrate = -1;

            public MonitorInfo()
            {
                available = false;
                name = "";
                width = -1;
                height = -1;
                refreshrate = -1;
            }
            public MonitorInfo(string name, int w, int h, int refreshrate)
            {
                available = true;
                this.name = name;
                width = w;
                height = h;
                this.refreshrate = refreshrate;
            }
        }


        private List<MonitorInfo> monitors = new();
        private int curIndex = 0;
        private int monitorCount = 0;


        public MonitorHandler()
        {
            GenerateInfo();
        }

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
                //Temporary Fix!!!
                if (w > 2000) w = 1920;
                if (h > 1250) h = 1080;
                int rr = GetMonitorRefreshRate(i);
                monitors.Add(new(name, w, h, rr));
            }
        }
        public (bool changed, int oldIndex, int newIndex) HasIndexChanged()
        {
            int index = GetCurrentMonitor();
            if (curIndex != index && IsValidIndex(index))
            {
                int oldIndex = curIndex;
                curIndex = index;
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
                return new();
            }
            else //monitors removed
            {
                string currentMonitorName = GetName();
                GenerateInfo();
                if (currentMonitorName != GetName()) return Get();//the current monitor was removed
                else return new();//current monitor still exists so it was not removed
            }
        }

        public void SetCurIndex(int index)
        {
            if (!IsValidIndex(index)) return;
            curIndex = index;
        }
        public int GetCurIndex() { return curIndex; }
        public (int width, int height) GetSize(int monitorIndex)
        {
            if (monitorIndex < 0 || monitorIndex >= monitors.Count) return (-1, -1);
            return (monitors[monitorIndex].width, monitors[monitorIndex].height);
        }
        public (int width, int height) GetSize() { return GetSize(curIndex); }
        public int GetRefreshrate(int monitorIndex)
        {
            if (monitorIndex < 0 || monitorIndex >= monitors.Count) return -1;
            return monitors[monitorIndex].refreshrate;
        }
        public int GetRefreshrate() { return GetRefreshrate(curIndex); }
        public string GetName(int monitorIndex)
        {
            if (monitorIndex < 0 || monitorIndex >= monitors.Count) return "";
            return monitors[monitorIndex].name;
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
        public MonitorInfo Next()
        {
            int oldIndex = curIndex;
            curIndex += 1;
            if (curIndex >= monitors.Count) curIndex = 0;
            if (oldIndex == curIndex) return new();
            return Get();
        }
        public MonitorInfo Prev()
        {
            int oldIndex = curIndex;
            curIndex -= 1;
            if (curIndex < 0) curIndex = monitors.Count - 1;
            if (oldIndex == curIndex) return new();
            return Get();
        }
    }

}
