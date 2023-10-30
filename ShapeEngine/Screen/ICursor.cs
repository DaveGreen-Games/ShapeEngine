using ShapeEngine.Lib;
using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Screen
{
    public interface ICursor
    {
        public uint GetID();
        public void DrawGameUI(ScreenInfo ui);
        public void DrawUI(ScreenInfo ui);
        public void Update(float dt, ScreenInfo ui);
        public void TriggerEffect(string effect);
        public void Deactivate();
        public void Activate(ICursor oldCursor);
    }

    public class NullCursor : ICursor 
    {
        private readonly uint id = ShapeID.NextID;
        public NullCursor() { }

        public void Activate(ICursor oldCursor) { }

        public void TriggerEffect(string effect)
        {
        }

        public void Deactivate() { }

        public void DrawGameUI(ScreenInfo ui) { }
        public void DrawUI(ScreenInfo ui) { }

        public uint GetID() { return id; }

        public void Update(float dt, ScreenInfo ui) { }
    }


    /*
    public class CursorHandler
    {
        private Dictionary<uint, ICursor> cursors = new();
        //private CursorBasic nullCursor;
        private ICursor? curCursor = null;
        public bool Hidden { get; protected set; } = false;

        public CursorHandler(bool hidden = false) 
        {
            this.Hidden = hidden;
        }

        public void Draw(Vector2 uiSize, Vector2 mousePos)
        {
            if (Hidden) return;
            curCursor?.Draw(uiSize, mousePos);
        }
        public void Close()
        {
            cursors.Clear();
            curCursor = null;// nullCursor;
        }


        public void Hide()
        {
            if (Hidden) return;
            Hidden = true;
        }
        public void Show()
        {
            if (!Hidden) return;
            Hidden = false;
        }
        public bool Switch(uint id)
        {
            if (!cursors.ContainsKey(id)) return false;
            curCursor = cursors[id];
            return true;
        }
        public bool Remove(uint id)
        {
            if (!cursors.ContainsKey(id)) return false;
            if (cursors[id] == curCursor) curCursor = null;
            cursors.Remove(id);
            return true;
        }
        public void Add(ICursor cursor)
        {
            if (cursors.ContainsKey(cursor.ID)) cursors[cursor.ID] = cursor;
            else cursors.Add(cursor.ID, cursor);
        }
    }
    */
}
