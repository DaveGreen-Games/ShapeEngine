using Raylib_CsLo;
using System.Numerics;

namespace ShapeUI.Container
{
    public class UIContainer : UIElement
    {
        protected List<UIElement> children = new();
        protected Color bgColor = new(0, 0, 0, 0);
        protected string containerTitle = "";

        public UIContainer() { }
        public UIContainer(List<UIElement> children)
        {
            this.children = children;
        }
        public UIContainer(params UIElement[] children)
        {
            this.children = children.ToList();
        }


        public List<UIElement> FindChildren(Predicate<UIElement> match) { return children.FindAll(match); }

        public string GetContainerTitle() { return containerTitle; }
        public void SetContainerTitle(string newTitle) { containerTitle = newTitle; }
        public void SetColors(Color newBgColor)
        {
            bgColor = newBgColor;
        }
        public bool HasBackground() { return bgColor.a > 0; }

        public bool HasChildren() { return children != null && children.Count > 0; }

        public virtual List<UIElement> GetDisplayedItems() { return children; }

        /// <summary>
        /// Try to select the children at the designated index.
        /// Index of < 0 tries to select the first element, Index of >= child count tries to select the last element.
        /// If the item inherits from UIElementSelectable it is selected. If the item inherits from UIContainer 
        /// than Select() is called on the UIContainer etc.
        /// </summary>
        /// <param name="index">Item to try to select.</param>
        /// <returns></returns>
        public virtual bool Select(int index = -1)
        {
            var items = GetDisplayedItems();
            if (items != null && items.Count > 0)
            {
                int i = 0;
                if (index < 0) i = 0;
                else if (index >= items.Count) i = items.Count - 1;
                else i = index;
                var element = items[i];
                if (element != null)
                {
                    var selectable = element as UIElementSelectable;
                    if (selectable != null)
                    {
                        UIHandler.SelectUIElement(selectable);
                        return true;
                    }
                    else
                    {
                        var container = element as UIContainer;
                        if (container != null) return container.Select(index);
                    }
                }
            }
            return false;
        }

        public UIElement? GetChild(int index)
        {
            if (children == null || children.Count <= 0) return null;
            if (index < 0) index = 0;
            else if (index >= children.Count) index = children.Count - 1;

            return children[index];
        }
        public UIElement? GetDisplayedItem(int index)
        {
            var items = GetDisplayedItems();
            if (items == null || items.Count <= 0) return null;
            if (index < 0) index = 0;
            else if (index >= items.Count) index = items.Count - 1;
            return items[index];
        }
        /*
        public bool SelectFirstElement()
        {
            var element = GetFirstElement();
            if (element != null)
            {
                var selectable = element as UIElementSelectable;
                if (selectable != null)
                {
                    UIHandler.SelectUIElement(selectable);
                    return true;
                }
                else
                {
                    var container = element as UIContainer;
                    if (container != null) return container.SelectFirstElement();
                }
            }
            return false;
        }
        public bool SelectLastElement()
        {
            var element = GetLastElement();
            if (element != null)
            {
                var selectable = element as UIElementSelectable;
                if (selectable != null)
                {
                    UIHandler.SelectUIElement(selectable);
                    return true;
                }
                else
                {
                    var container = element as UIContainer;
                    if (container != null) return container.SelectLastElement();
                }
            }
            return false;
        }
        */

        public UIElement? GetFirstElement()
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return null;
            return displayedItems[0];
        }
        public UIElement? GetLastElement()
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return null;
            return displayedItems[displayedItems.Count - 1];
        }
        public bool IsFirstItemSelected()
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return false;
            return IsElementSelected(displayedItems[0]);
        }
        public bool IsLastElementSelected()
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return false;
            return IsElementSelected(displayedItems[displayedItems.Count - 1]);
        }
        public bool IsElementSelected(UIElement element)
        {
            var selectable = element as UIElementSelectable;
            if (selectable != null) return selectable.IsSelected();
            else return false;
        }
        public bool IsSelected(bool deep = false)
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return false;
            if (deep)
            {
                foreach (var child in displayedItems)
                {
                    var selectable = child as UIElementSelectable;
                    if (selectable != null)
                    {
                        if (selectable.IsSelected()) return true;
                    }
                    else
                    {
                        var container = child as UIContainer;
                        if (container != null)
                        {
                            bool selected = container.IsSelected();
                            if (selected) return true;
                        }
                    }
                }
            }
            else
            {
                foreach (var item in displayedItems)
                {
                    if (IsElementSelected(item)) return true;
                }
            }

            return false;
        }

        public virtual void AddChild(UIElement newChild)
        {
            RegisterChild(newChild);
            children.Add(newChild);
        }

        public virtual void AddChildren(List<UIElement> newChildren)
        {
            foreach (var child in newChildren)
            {
                RegisterChild(child);
            }
            children.AddRange(newChildren);
        }
        public virtual void AddChildren(params UIElement[] newChildren)
        {
            foreach (var child in newChildren)
            {
                RegisterChild(child);
            }
            children.AddRange(newChildren);
        }
        public virtual void RemoveChild(UIElement child)
        {
            UnregisterChild(child);
            children.Remove(child);
        }

        public void RemoveChildren(List<UIElement> childrenToRemove)
        {
            foreach (var child in childrenToRemove)
            {
                RemoveChild(child);
            }
        }
        public void RemoveChildren(params UIElement[] childrenToRemove)
        {
            foreach (var child in childrenToRemove)
            {
                RemoveChild(child);
            }
        }
        public void RemoveAll(Predicate<UIElement> match)
        {
            UnregisterChildren();
            children.RemoveAll(match);
            RegisterChildren();
        }
        public void ClearChildren()
        {
            UnregisterChildren();
            children.Clear();
        }

        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            if (HasBackground()) DrawBackground(GetRect(), bgColor);
            DrawChildren(uiSize, stretchFactor);
        }

        public override void Update(float dt, Vector2 mousePosUI)
        {
            foreach (var child in children)
            {
                child.UpdateRect(GetPos(Alignement.CENTER), GetSize(), Alignement.CENTER);
                child.Update(dt, mousePosUI);
            }
        }

        public virtual void RegisterChildren()
        {
            foreach (var child in GetDisplayedItems())
            {
                RegisterChild(child);
            }
        }

        public virtual void UnregisterChildren()
        {
            foreach (var child in GetDisplayedItems())
            {
                UnregisterChild(child);
            }
        }

        protected void RegisterChild(UIElement child)
        {
            if (child is UIContainer)
            {
                var container = (UIContainer)child;
                container.RegisterChildren();
            }
            else if (child is UIElementSelectable)
            {
                var selectable = (UIElementSelectable)child;
                UIHandler.RegisterUIElement(selectable);
            }
        }

        protected void UnregisterChild(UIElement child)
        {
            if (child is UIContainer)
            {
                var container = (UIContainer)child;
                container.UnregisterChildren();
            }
            else if (child is UIElementSelectable)
            {
                var selectable = (UIElementSelectable)child;
                UIHandler.UnregisterUIElement(selectable);
            }
        }
        protected virtual void DrawChildren(Vector2 uiSize, Vector2 stretchFactor, int startIndex = -1, int endIndex = -1)
        {
            if (startIndex < 0 || endIndex < 0)
            {
                foreach (var child in children)
                {
                    child.Draw(uiSize, stretchFactor);
                }
            }
            else
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    children[i].Draw(uiSize, stretchFactor);
                }
            }

        }
        protected virtual void UpdateChildren(float dt, Vector2 mousePosUI, int startIndex = -1, int endIndex = -1)
        {
            if (startIndex < 0 || endIndex < 0)
            {
                foreach (var child in children)
                {
                    child.Update(dt, mousePosUI);
                }
            }
            else
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    children[i].Update(dt, mousePosUI);
                }
            }
        }

        protected virtual void DrawBackground(Rectangle rect, Color color)
        {
            SDrawing.DrawRectangle(rect, color);
        }
    }

}
