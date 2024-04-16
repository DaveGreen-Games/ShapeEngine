using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.UI;


//deprecate? 
//should be replaced by control node or rect node 
public class RectContainer
{
    public readonly string Name;
    private Rect rect = new();
    
    private readonly Dictionary<string, RectContainer> children = new();

    public RectContainer(string name)
    {
        Name = name;
        
    }
    public RectContainer(string name, params RectContainer[] container)
    {
        Name = name;
        foreach (var c in container)
        {
            AddChild(c);
        }
    }
    public void AddChild(RectContainer container) => children[container.Name] = container;
    public RectContainer? RemoveChild(string name)
    {
        bool found = children.TryGetValue(name, out var value);
        if (found) children.Remove(name);
        return value;
    }
        
    public RectContainer? GetChild(string name)
    {
        children.TryGetValue(name, out var child);
        return child;
    }

    public Rect GetRect(string path, char separator = ' ')
    {
        var names = path.Split(separator);
        return GetRect(names);
    }
    public Rect GetRect(params string[] path)
    {
        if (path.Length <= 0) return rect;
        var curChild = this;
        for (var i = 0; i < path.Length; i++)
        {
            string name = path[i];
            if (name == "") return curChild.GetRect();
            var next = curChild.GetChild(name);
            if (next != null) curChild = next;
        }

        return curChild.GetRect();
    }
    public Rect GetRectSingle(string name)
    {
        children.TryGetValue(name, out var container);
        return container?.GetRect() ?? new();
    }
    public Rect GetRect() => rect;
    public void SetRect(Rect newRect)
    {
        rect = OnRectUpdateRequested(newRect);
        foreach (var child in children.Values)
        {
            child.SetRect(rect);
        }
    }

    protected virtual Rect OnRectUpdateRequested(Rect newRect)
    {
        return newRect;
    }

    public void Draw(ColorRgba startColorRgba, ColorRgba colorRgbaShift)
    {
        rect.Draw(startColorRgba);
        var nextColor = startColorRgba + colorRgbaShift; //.Add(colorShift);
        foreach (var child in children.Values)
        {
            child.Draw(nextColor, colorRgbaShift);
        }
    }

}