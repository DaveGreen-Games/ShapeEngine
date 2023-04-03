using System.Numerics;

namespace ShapeUI.Container
{
    public class TabContainer : UIContainer
    {
        protected int curTabIndex = 0;

        public TabContainer() { }
        public TabContainer(int startTab, List<UIElement> children)
        {
            this.children = children;
            SetTab(startTab);
        }
        public TabContainer(int startTab, params UIElement[] children)
        {
            this.children = children.ToList();
            SetTab(startTab);
        }
        public TabContainer(List<UIElement> children) : base(children) { }
        public TabContainer(params UIElement[] children) : base(children) { }

        public UIElement? GetCurTab()
        {
            if (curTabIndex < 0 || curTabIndex >= children.Count) return null;
            return children[curTabIndex];
        }
        public override List<UIElement> GetDisplayedItems()
        {
            List<UIElement> list = new();
            var tab = GetCurTab();
            if (tab != null) list.Add(tab);
            return list;
        }
        public int SetTab(int newTab)
        {
            if (newTab == curTabIndex) return curTabIndex;
            UnregisterChildren();
            if (newTab < 0) curTabIndex = children.Count - 1;
            else if (newTab >= children.Count) curTabIndex = 0;
            else curTabIndex = newTab;
            RegisterChildren();
            return curTabIndex;
        }
        public int NextTab() { return SetTab(curTabIndex + 1); }
        public int PrevTab() { return SetTab(curTabIndex + 1); }

        public int GetCurTabIndex() { return curTabIndex; }
        public override void Update(float dt, Vector2 mousePosUI)
        {
            var curTab = GetCurTab();
            if (curTab != null)
            {
                curTab.UpdateRect(GetPos(new(0.5f)), GetSize(), new(0.5f));
                curTab.Update(dt, mousePosUI);
            }
        }
        public override void Draw()
        {
            //if (HasBackground()) DrawBackground(GetRect(new(0f)), bgColor);

            var curTab = GetCurTab();
            if (curTab != null)
            {
                curTab.Draw();
            }
        }


    }

}
