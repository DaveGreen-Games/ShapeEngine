/*
using System.Numerics;
using ShapeLib;

namespace ShapeUI.Container
{
    public class GridDisplayContainer : GridContainer
    {
        protected int displayCount = 0;
        protected int curIndex = 0;
        protected int movementDir = 0;
        protected bool vContainer = true;
        public GridDisplayContainer(int columns, int rows, bool vContainer) : base(columns, rows)
        {
            this.displayCount = columns * rows;
            this.vContainer = vContainer;
            RegisterEvents();
        }
        public GridDisplayContainer(int columns, int rows, bool vContainer, List<UIElement> children)
            : base(columns, rows, children)
        {
            this.displayCount = columns * rows;
            this.vContainer = vContainer;
            RegisterEvents();
        }
        public GridDisplayContainer(int columns, int rows, bool vContainer, params UIElement[] children)
            : base(columns, rows, children)
        {
            this.displayCount = columns * rows;
            this.vContainer = vContainer;
            RegisterEvents();
        }
        public GridDisplayContainer(int columns, int rows, float hGapRelative, float vGapRelative, bool vContainer)
            : base(columns, rows, hGapRelative, vGapRelative)
        {
            this.displayCount = columns * rows;
            this.vContainer = vContainer;
            RegisterEvents();
        }
        public GridDisplayContainer(int columns, int rows, float hGapRelative, float vGapRelative, bool vContainer, List<UIElement> children)
            : base(columns, rows, hGapRelative, vGapRelative, children)
        {
            this.displayCount = columns * rows;
            this.vContainer = vContainer;
            RegisterEvents();
        }
        public GridDisplayContainer(int columns, int rows, float hGapRelative, float vGapRelative, bool vContainer, params UIElement[] children)
            : base(columns, rows, hGapRelative, vGapRelative, children)
        {
            this.displayCount = columns * rows;
            this.vContainer = vContainer;
            RegisterEvents();
        }
        protected void RegisterEvents()
        {
            UIHandler.OnDirectionInput += OnDirectionInput;
            UIHandler.OnSelectedItemUnregistered += OnSelectedItemUnregistered;
        }
        protected virtual void OnDirectionInput(UINeighbors.NeighborDirection dir, UIElement? selected, UIElement? nextSelected)
        {
            if (nextSelected == null) return;

            if (vContainer)
            {
                if (dir == UINeighbors.NeighborDirection.TOP)
                {
                    var firstElements = GetFirstElements();
                    if (firstElements.Contains(nextSelected))
                    {
                        ChangeCurIndex(-1);
                    }
                }
                else if (dir == UINeighbors.NeighborDirection.BOTTOM)
                {
                    var lastElements = GetLastElements();
                    if (lastElements.Contains(nextSelected))
                    {
                        ChangeCurIndex(1);
                    }
                }

            }
            else
            {
                if (dir == UINeighbors.NeighborDirection.LEFT)
                {
                    var firstElements = GetFirstElements();
                    if (firstElements.Contains(nextSelected))
                    {
                        ChangeCurIndex(-1);
                    }
                }
                else if (dir == UINeighbors.NeighborDirection.RIGHT)
                {
                    var lastElements = GetLastElements();
                    if (lastElements.Contains(nextSelected))
                    {
                        ChangeCurIndex(1);
                    }
                }
            }
        }
        protected virtual void OnSelectedItemUnregistered(UIElement element)
        {
            if (movementDir > 0)
            {
                var selectable = GetFirstElement() as UIElementSelectable;
                if (selectable != null) UIHandler.SelectUIElement(selectable);
            }
            else if (movementDir < 0)
            {
                var selectable = GetLastElement() as UIElementSelectable;
                if (selectable != null) UIHandler.SelectUIElement(selectable);
            }
        }

        public bool IsVContainer() { return vContainer; }
        public void SetVContainer(bool isVContainer) { vContainer = isVContainer; }
        public List<UIElement> GetFirstElements()
        {
            if (children.Count <= 0) return new();
            if (vContainer)
            {
                int start = curIndex * columns;
                int end = start + columns;
                if (end > children.Count) end = children.Count;
                int count = end - start;
                return children.GetRange(start, count);
            }
            else
            {
                int start = curIndex * rows;
                int end = start + rows;
                if (end > children.Count) end = children.Count;
                int count = end - start;
                return children.GetRange(start, count);
            }

        }
        public List<UIElement> GetLastElements()
        {

            if (children.Count <= 0) return new();
            if (vContainer)
            {
                int endIndex = curIndex + rows - 1;
                int start = endIndex * columns;
                int end = start + columns;
                if (end > children.Count) end = children.Count;
                int count = end - start;
                return children.GetRange(start, count);
            }
            else
            {
                int endIndex = curIndex + columns - 1;
                int start = endIndex * rows;
                int end = start + rows;
                if (end > children.Count) end = children.Count;
                int count = end - start;
                return children.GetRange(start, count);
            }

        }
        public override void AddChild(UIElement newChild)
        {
            if (displayCount > children.Count)
            {
                RegisterChild(newChild);
            }
            children.Add(newChild);
        }
        public override void AddChildren(List<UIElement> newChildren)
        {
            foreach (var child in newChildren)
            {
                AddChild(child);
            }
        }
        public override void AddChildren(params UIElement[] newChildren)
        {
            foreach (var child in newChildren)
            {
                AddChild(child);
            }
        }

        public void SetIndex(int newIndex)
        {
            int prevCurIndex = curIndex;
            int prevEndIndex = GetEndIndex();

            if (vContainer)
            {
                int maxRowIndex = GetMaxRows() - rows;
                if (maxRowIndex < 0) maxRowIndex = 0;
                if (newIndex < 0)
                {
                    curIndex = 0;
                }
                else if (newIndex > maxRowIndex)
                {
                    curIndex = maxRowIndex;
                }
                else curIndex = newIndex;

                movementDir = curIndex - prevCurIndex;

                if ((int)MathF.Abs(movementDir * columns) >= displayCount)
                {
                    int start = prevCurIndex * columns;
                    int end = GetEndIndex(prevEndIndex);
                    for (int i = start; i < end; i++)
                    {
                        var selectable = children[i] as UIElementSelectable;
                        if (selectable != null) UnregisterChild(selectable);
                    }
                    RegisterChildren();
                }
                else
                {
                    if (movementDir > 0)
                    {
                        int unregisterStart = prevCurIndex * columns;
                        int unregisterEnd = curIndex * columns;
                        for (int i = unregisterStart; i < unregisterEnd; i++)
                        {
                            var selectable = children[i] as UIElementSelectable;
                            if (selectable != null) UnregisterChild(selectable);
                        }

                        int registerStart = GetEndIndex(prevEndIndex) * columns;
                        int registerEnd = GetEndIndex() * columns;
                        if (registerEnd > children.Count) registerEnd = children.Count;
                        for (int i = registerStart; i < registerEnd; i++)
                        {
                            var selectable = children[i] as UIElementSelectable;
                            if (selectable != null) RegisterChild(selectable);
                        }
                    }
                    else if (movementDir < 0)
                    {
                        int unregisterStart = GetEndIndex() * columns;
                        int unregisterEnd = GetEndIndex(prevEndIndex) * columns;
                        if (unregisterEnd > children.Count) unregisterEnd = children.Count;
                        for (int i = unregisterStart; i < unregisterEnd; i++)
                        {
                            var selectable = children[i] as UIElementSelectable;
                            if (selectable != null) UnregisterChild(selectable);
                        }

                        int registerStart = curIndex * columns;
                        int registerEnd = prevCurIndex * columns;
                        for (int i = registerStart; i < registerEnd; i++)
                        {
                            var selectable = children[i] as UIElementSelectable;
                            if (selectable != null) RegisterChild(selectable);
                        }
                    }
                }

            }
            else
            {
                int maxColumnIndex = GetMaxColumns() - columns;
                if (maxColumnIndex < 0) maxColumnIndex = 0;
                if (newIndex < 0)
                {
                    curIndex = 0;
                }
                else if (newIndex > maxColumnIndex)
                {
                    curIndex = maxColumnIndex;
                }
                else curIndex = newIndex;

                movementDir = curIndex - prevCurIndex;

                if ((int)MathF.Abs(movementDir * columns) >= displayCount)
                {
                    int start = prevCurIndex * rows;
                    int end = GetEndIndex(prevEndIndex);
                    for (int i = start; i < end; i++)
                    {
                        var selectable = children[i] as UIElementSelectable;
                        if (selectable != null) UnregisterChild(selectable);
                    }
                    RegisterChildren();
                }
                else
                {
                    if (movementDir > 0)
                    {
                        int unregisterStart = prevCurIndex * rows;
                        int unregisterEnd = curIndex * rows;
                        for (int i = unregisterStart; i < unregisterEnd; i++)
                        {
                            var selectable = children[i] as UIElementSelectable;
                            if (selectable != null) UnregisterChild(selectable);
                        }

                        int registerStart = GetEndIndex(prevEndIndex) * rows;
                        int registerEnd = GetEndIndex() * rows;
                        if (registerEnd > children.Count) registerEnd = children.Count;
                        for (int i = registerStart; i < registerEnd; i++)
                        {
                            var selectable = children[i] as UIElementSelectable;
                            if (selectable != null) RegisterChild(selectable);
                        }
                    }
                    else if (movementDir < 0)
                    {
                        int unregisterStart = GetEndIndex() * rows;
                        int unregisterEnd = GetEndIndex(prevEndIndex) * rows;
                        if (unregisterEnd > children.Count) unregisterEnd = children.Count;
                        for (int i = unregisterStart; i < unregisterEnd; i++)
                        {
                            var selectable = children[i] as UIElementSelectable;
                            if (selectable != null) UnregisterChild(selectable);
                        }

                        int registerStart = curIndex * rows;
                        int registerEnd = prevCurIndex * rows;
                        for (int i = registerStart; i < registerEnd; i++)
                        {
                            var selectable = children[i] as UIElementSelectable;
                            if (selectable != null) RegisterChild(selectable);
                        }
                    }
                }
            }
        }

        public void SetIndex(UIElement child)
        {
            if (!children.Contains(child)) return;
            int index = children.IndexOf(child);
            if (vContainer)
            {
                var coords = SUtils.TransformIndexToCoordinates(index, GetMaxRows(), columns, true);
                SetIndex(coords.row);
            }
            else
            {
                var coords = SUtils.TransformIndexToCoordinates(index, rows, GetMaxColumns(), false);
                SetIndex(coords.col);
            }

        }
        public int GetCurIndex() { return curIndex; }
        public int GetDisplayCount() { return displayCount; }
        public void ChangeCurIndex(int amount)
        {
            SetIndex(curIndex + amount);
        }
        public int GetEndIndex()
        {
            if (vContainer)
            {
                int index = curIndex + rows;
                int count = index * columns;
                int childCount = children.Count;

                if (count > childCount)
                {
                    var coords = SUtils.TransformIndexToCoordinates(childCount - 1, GetMaxRows(), columns, true);
                    index = coords.row + 1;
                }
                return index;
            }
            else
            {
                int index = curIndex + columns;
                int count = index * rows;
                int childCount = children.Count;

                if (count > childCount)
                {
                    var coords = SUtils.TransformIndexToCoordinates(childCount - 1, rows, GetMaxColumns(), false);
                    index = coords.col + 1;
                }
                return index;
            }

        }
        public int GetEndIndex(int endIndex)
        {
            if (vContainer)
            {
                int index = endIndex;// + rows;
                int count = index * columns;
                int childCount = children.Count;

                if (count > childCount)
                {
                    var coords = SUtils.TransformIndexToCoordinates(childCount - 1, GetMaxRows(), columns, true);
                    index = coords.row + 1;
                }
                return index;
            }
            else
            {
                int index = endIndex;
                int count = index * rows;
                int childCount = children.Count;

                if (count > childCount)
                {
                    var coords = SUtils.TransformIndexToCoordinates(childCount - 1, rows, GetMaxColumns(), false);
                    index = coords.col + 1;
                }
                return index;
            }

        }

        public (int start, int end) GetDisplayedItemRange()
        {
            if (vContainer)
            {
                int start = curIndex * columns;
                int end = GetEndIndex() * columns;
                if (start > children.Count) start = children.Count;
                if (end > children.Count) end = children.Count;
                return (start, end);
            }
            else
            {
                int start = curIndex * rows;
                int end = GetEndIndex() * rows;
                if (start > children.Count) start = children.Count;
                if (end > children.Count) end = children.Count;
                return (start, end);
            }
        }
        public int GetMaxRows()
        {
            float r = (float)children.Count / (float)columns;
            return (int)MathF.Ceiling(r);
        }
        public int GetMaxColumns()
        {
            float r = (float)children.Count / (float)rows;
            return (int)MathF.Ceiling(r);
        }
        public override List<UIElement> GetDisplayedItems()
        {
            var range = GetDisplayedItemRange();
            return children.GetRange(range.start, range.end - range.start);
        }

        public override void Update(float dt, Vector2 mousePosUI)
        {
            movementDir = 0;
            var range = GetDisplayedItemRange();
            UpdateRects(GetRect(new(0f)), range.start, range.end, vContainer);
            UpdateChildren(dt, mousePosUI, range.start, range.end);
        }

        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            if (HasBackground()) DrawBackground(GetRect(new(0f)), bgColor);
            var range = GetDisplayedItemRange();
            DrawChildren(uiSize, stretchFactor, range.start, range.end);
        }


        public override void RegisterChildren()
        {
            var range = GetDisplayedItemRange();
            for (int i = range.start; i < range.end; i++)
            {
                var child = children[i];
                RegisterChild(child);
            }
        }

        public override void UnregisterChildren()
        {
            var range = GetDisplayedItemRange();
            for (int i = range.start; i < range.end; i++)
            {
                var child = children[i];
                UnregisterChild(child);
            }
        }
    }

}
*/