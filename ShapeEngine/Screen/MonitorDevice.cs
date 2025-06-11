
using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Screen;

/// <summary>
/// Manages monitor information and handles monitor-related events in the application.
/// Provides functionality to detect and respond to monitor changes, such as 
/// disconnection, display setup changes, or switching the active monitor.
/// </summary>
public sealed class MonitorDevice
{
    /// <summary>
    /// Delegate for monitor change events, providing information about the old and new monitor.
    /// </summary>
    /// <param name="oldMonitor">Information about the previous monitor.</param>
    /// <param name="newMonitor">Information about the new monitor.</param>
    public delegate void MonitorChanged(MonitorInfo oldMonitor, MonitorInfo newMonitor);
    
    /// <summary>
    /// Event triggered when the current monitor changes.
    /// </summary>
    public event MonitorChanged? OnMonitorChanged;

    /// <summary>
    /// Delegate for monitor setup change events, providing information about the new monitor configuration.
    /// </summary>
    /// <param name="newSetup">List of all monitors in the updated setup.</param>
    public delegate void MonitorSetupChanged(List<MonitorInfo> newSetup);
    
    /// <summary>
    /// Event triggered when the monitor setup changes (monitors added or removed).
    /// </summary>
    public event MonitorSetupChanged? OnMonitorSetupChanged;
    
    /// <summary>
    /// List of all detected monitors.
    /// </summary>
    private List<MonitorInfo> monitors = [];
    
    /// <summary>
    /// Index of the current active monitor.
    /// </summary>
    private int curIndex;
    
    /// <summary>
    /// Total number of detected monitors.
    /// </summary>
    private int monitorCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitorDevice"/> class and populates initial monitor information.
    /// </summary>
    public MonitorDevice()
    {
        GenerateInfo();
    }

    /// <summary>
    /// Gets the current active monitor's information.
    /// </summary>
    /// <returns>Information about the current monitor, or an empty <see cref="MonitorInfo"/> if no monitors are available.</returns>
    public MonitorInfo CurMonitor()
    {
        if (monitors.Count <= 0) return new();
        else return monitors[curIndex];
    }

    /// <summary>
    /// Gets the total number of connected monitors.
    /// </summary>
    /// <returns>The count of connected monitors.</returns>
    public int MonitorCount() { return monitors.Count; }

    /// <summary>
    /// Gets information about all connected monitors.
    /// </summary>
    /// <returns>A list containing information about all monitors.</returns>
    public List<MonitorInfo> GetAllMonitorInfo() { return monitors; }

    /// <summary>
    /// Checks if the specified monitor index is valid.
    /// </summary>
    /// <param name="index">The monitor index to check.</param>
    /// <returns><c>true</c> if the index is valid; otherwise, <c>false</c>.</returns>
    public bool IsValidIndex(int index)
    {
        return index >= 0 && index < monitors.Count;
    }

    /// <summary>
    /// Refreshes and regenerates information about all connected monitors.
    /// </summary>
    private void GenerateInfo()
    {
        curIndex = Raylib.GetCurrentMonitor();
        monitorCount = Raylib.GetMonitorCount();
        monitors.Clear();
        for (int i = 0; i < monitorCount; i++)
        {
            string name =Raylib.GetMonitorName_(i);
            int w =  Raylib.GetMonitorWidth(i);
            int h =  Raylib.GetMonitorHeight(i);
            Vector2 pos = Raylib.GetMonitorPosition(i) + new Vector2(1, 1);
            
            int rr = Raylib.GetMonitorRefreshRate(i);
            monitors.Add(new(name, w, h, pos, rr, i));
        }
    }

    /// <summary>
    /// Checks if the current monitor index has changed and updates accordingly.
    /// </summary>
    /// <returns>A tuple containing: whether the index changed, the old index, and the new index.</returns>
    public (bool changed, int oldIndex, int newIndex) HasIndexChanged()
    {
        int index = Raylib.GetCurrentMonitor();
        if (curIndex != index && IsValidIndex(index))
        {
            int oldIndex = curIndex;
            curIndex = index;
            OnMonitorChanged?.Invoke(monitors[oldIndex], monitors[index]);
            return (true, oldIndex, curIndex);
        }
        return (false, -1, -1);
    }

    /// <summary>
    /// Checks if any monitor configuration has changed and updates monitor information if needed.
    /// Triggers relevant events when monitor changes are detected.
    /// </summary>
    /// <returns>Information about the new monitor if a change was detected, or an empty <see cref="MonitorInfo"/> if no change occurred.</returns>
    public MonitorInfo HasMonitorChanged()
    {
        MonitorInfo nextMonitor = CurMonitor();
        int currentMonitorCount = Raylib.GetMonitorCount();
        if (currentMonitorCount != monitorCount)
        {
            int dif = currentMonitorCount - monitorCount;
            if (dif > 0) //new monitors added -> only update monitor list
            {
                GenerateInfo();
                OnMonitorSetupChanged?.Invoke(monitors);
                // return new();
            }
            else //monitors removed
            {
                //string currentMonitorName = GetName();
                var oldMonitor = Get();
                GenerateInfo();
                OnMonitorSetupChanged?.Invoke(monitors);

                var monitor = monitors.Find(mi => mi.Name == oldMonitor.Name);
                if (!monitor.Available) //current monitor was removed
                {
                    var newMonitor = Get();
                    // OnMonitorChanged?.Invoke(oldMonitor, newMonitor);
                    nextMonitor = newMonitor;
                }
                else//current monitor is still in the list
                {
                    if(monitor.Index != oldMonitor.Index)//just update the index in case the monitors index has changed
                    {
                        curIndex = monitor.Index;
                    }
                    //return new();
                }
            }

        }
        
        int curMonitorIndex = Raylib.GetCurrentMonitor();
        if (curMonitorIndex != nextMonitor.Index)
        {
            nextMonitor = Get(curMonitorIndex);
        }

        if (nextMonitor.Index != CurMonitor().Index)
        {
            OnMonitorChanged?.Invoke(CurMonitor(), nextMonitor);
            curIndex = nextMonitor.Index;
            return nextMonitor;
        }
    
        return new();
    }

    /// <summary>
    /// Sets the current monitor index if the specified index is valid.
    /// </summary>
    /// <param name="index">The monitor index to set as current.</param>
    /// <returns><c>true</c> if the index was valid and set successfully; otherwise, <c>false</c>.</returns>
    public bool SetCurIndex(int index)
    {
        if (!IsValidIndex(index)) return false;
        curIndex = index;
        return true;
    }

    /// <summary>
    /// Gets the current monitor index.
    /// </summary>
    /// <returns>The index of the current active monitor.</returns>
    public int GetCurIndex() { return curIndex; }

    /// <summary>
    /// Gets the dimensions of the specified monitor.
    /// </summary>
    /// <param name="monitorIndex">The index of the monitor.</param>
    /// <returns>The dimensions of the specified monitor, or (-1, -1) if the index is invalid.</returns>
    public Dimensions GetSize(int monitorIndex)
    {
        if (monitorIndex < 0 || monitorIndex >= monitors.Count) return new(-1, -1);
        return monitors[monitorIndex].Dimensions;
        //return (monitors[monitorIndex].width, monitors[monitorIndex].height);
    }

    /// <summary>
    /// Gets the dimensions of the current monitor.
    /// </summary>
    /// <returns>The dimensions of the current monitor.</returns>
    public Dimensions GetSize() { return GetSize(curIndex); }

    /// <summary>
    /// Gets the refresh rate of the specified monitor.
    /// </summary>
    /// <param name="monitorIndex">The index of the monitor.</param>
    /// <returns>The refresh rate in Hz, or -1 if the index is invalid.</returns>
    public int GetRefreshrate(int monitorIndex)
    {
        if (monitorIndex < 0 || monitorIndex >= monitors.Count) return -1;
        return monitors[monitorIndex].Refreshrate;
    }

    /// <summary>
    /// Gets the refresh rate of the current monitor.
    /// </summary>
    /// <returns>The refresh rate of the current monitor in Hz.</returns>
    public int GetRefreshrate() { return GetRefreshrate(curIndex); }

    /// <summary>
    /// Gets the name of the specified monitor.
    /// </summary>
    /// <param name="monitorIndex">The index of the monitor.</param>
    /// <returns>The name of the specified monitor, or an empty string if the index is invalid.</returns>
    public string GetName(int monitorIndex)
    {
        if (monitorIndex < 0 || monitorIndex >= monitors.Count) return "";
        return monitors[monitorIndex].Name;
    }

    /// <summary>
    /// Gets the name of the current monitor.
    /// </summary>
    /// <returns>The name of the current monitor.</returns>
    public string GetName() { return GetName(curIndex); }

    /// <summary>
    /// Gets the monitor information for the specified monitor.
    /// </summary>
    /// <param name="monitorIndex">The index of the monitor.</param>
    /// <returns>Information about the specified monitor, or an empty <see cref="MonitorInfo"/> if the index is invalid.</returns>
    public MonitorInfo Get(int monitorIndex)
    {
        if (monitorIndex < 0 || monitorIndex >= monitors.Count) return new();
        return monitors[monitorIndex];
    }

    /// <summary>
    /// Gets information about the current monitor.
    /// </summary>
    /// <returns>Information about the current monitor.</returns>
    public MonitorInfo Get() { return Get(curIndex); }

    /// <summary>
    /// Sets the specified monitor as the current monitor.
    /// </summary>
    /// <param name="newMonitor">The index of the monitor to set as current.</param>
    /// <returns>Information about the newly set monitor, or an empty <see cref="MonitorInfo"/> if the index is invalid or already current.</returns>
    public MonitorInfo SetMonitor(int newMonitor)
    {
        if (!IsValidIndex(newMonitor)) return new();
        if (newMonitor == curIndex) return new();
        curIndex = newMonitor;
        return Get();
    }

    /// <summary>
    /// Switches to the next monitor in the sequence.
    /// </summary>
    /// <returns>Information about the new monitor, or an empty <see cref="MonitorInfo"/> if no change occurred.</returns>
    public MonitorInfo NextMonitor()
    {
        int oldIndex = curIndex;
        curIndex += 1;
        if (curIndex >= monitors.Count) curIndex = 0;
        if (oldIndex == curIndex) return new();
        return Get();
    }

    /// <summary>
    /// Switches to the previous monitor in the sequence.
    /// </summary>
    /// <returns>Information about the new monitor, or an empty <see cref="MonitorInfo"/> if no change occurred.</returns>
    public MonitorInfo PrevMonitor()
    {
        int oldIndex = curIndex;
        curIndex -= 1;
        if (curIndex < 0) curIndex = monitors.Count - 1;
        if (oldIndex == curIndex) return new();
        return Get();
    }
}
