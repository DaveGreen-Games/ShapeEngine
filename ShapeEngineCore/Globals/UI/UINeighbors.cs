using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeEngineCore.Globals.UI
{
    public class UINeighbors
    {
        public enum NeighborDirection { TOP = 0, RIGHT = 1, BOTTOM = 2, LEFT = 3 };
        private UIElementSelectable? top = null;
        private UIElementSelectable? right = null;
        private UIElementSelectable? bottom = null;
        private UIElementSelectable? left = null;

        public UINeighbors() { }
        public UINeighbors(UIElementSelectable top, UIElementSelectable right, UIElementSelectable bottom, UIElementSelectable left)
        {
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.left = left;
        }
        public UIElementSelectable? SelectNeighbor(NeighborDirection dir, UIElementSelectable current)
        {
            if (!HasNeighbor(dir)) return null;
            var neighbor = GetNeighbor(dir);
            if (neighbor.IsDisabled()) return null;
            if (current != null) current.Deselect();
            neighbor.Select();

            return neighbor;
        }
        public bool HasNeighbor(NeighborDirection dir)
        {
            switch (dir)
            {
                case NeighborDirection.TOP: return top != null;
                case NeighborDirection.RIGHT: return right != null;
                case NeighborDirection.BOTTOM: return bottom != null;
                case NeighborDirection.LEFT: return left != null;
                default: return false;
            }
        }
        public UIElementSelectable? GetNeighbor(NeighborDirection dir)
        {
            switch (dir)
            {
                case NeighborDirection.TOP: return top;
                case NeighborDirection.RIGHT: return right;
                case NeighborDirection.BOTTOM: return bottom;
                case NeighborDirection.LEFT: return left;
                default: return null;
            }
        }
        public void SetNeighbor(UIElementSelectable neighbor, NeighborDirection dir)
        {
            switch (dir)
            {
                case NeighborDirection.TOP:
                    top = neighbor;
                    break;
                case NeighborDirection.RIGHT:
                    right = neighbor;
                    break;
                case NeighborDirection.BOTTOM:
                    bottom = neighbor;
                    break;
                case NeighborDirection.LEFT:
                    left = neighbor;
                    break;
                default:
                    break;
            }
        }
    }


}
