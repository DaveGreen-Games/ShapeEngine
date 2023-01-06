using Raylib_CsLo;
using System.Numerics;

namespace ShapeUI.Container
{
    public class GridContainer : UIContainer
    {
        protected float hGapRelative = 0f;
        protected float vGapRelative = 0f;
        protected int columns = 1;
        protected int rows = 1;

        public GridContainer() { }
        public GridContainer(int columns, int rows)
        {
            this.columns = columns;
            this.rows = rows;
        }
        public GridContainer(int columns, int rows, List<UIElement> children)
            : base(children)
        {
            this.columns = columns;
            this.rows = rows;
        }
        public GridContainer(int columns, int rows, params UIElement[] children)
            : base(children)
        {
            this.columns = columns;
            this.rows = rows;
        }
        public GridContainer(int columns, int rows, float hGapRelative, float vGapRelative)
        {
            this.columns = columns;
            this.rows = rows;
            this.hGapRelative = hGapRelative;
            this.vGapRelative = vGapRelative;
        }

        public GridContainer(int columns, int rows, float hGapRelative, float vGapRelative, List<UIElement> children)
            : base(children)
        {
            this.columns = columns;
            this.rows = rows;
            this.hGapRelative = hGapRelative;
            this.vGapRelative = vGapRelative;
        }
        public GridContainer(int columns, int rows, float hGapRelative, float vGapRelative, params UIElement[] children)
            : base(children)
        {
            this.columns = columns;
            this.rows = rows;
            this.hGapRelative = hGapRelative;
            this.vGapRelative = vGapRelative;
        }

        public void SetHGap(float newHGapRelative)
        {
            hGapRelative = newHGapRelative;
        }
        public void SetVGap(float newVGapRelative)
        {
            vGapRelative = newVGapRelative;
        }
        public void SetColumns(int newColumns) { columns = newColumns; }
        public void SetRows(int newRows) { rows = newRows; }
        public override void Update(float dt, Vector2 mousePosUI)
        {
            UpdateRects(GetRect(), 0, children.Count);
            UpdateChildren(dt, mousePosUI);
        }

        protected virtual void UpdateRects(Rectangle rect, int startIndex, int endIndex, bool leftToRight = true)
        {
            Vector2 startPos = new(rect.x, rect.y);
            //int count = children.Count;

            int hGaps = columns - 1;
            float totalWidth = rect.width;
            float hGapSize = totalWidth * hGapRelative;
            float elementWidth = (totalWidth - hGaps * hGapSize) / columns;
            Vector2 hGap = new(hGapSize + elementWidth, 0);

            int vGaps = rows - 1;
            float totalHeight = rect.height;
            float vGapSize = totalHeight * vGapRelative;
            float elementHeight = (totalHeight - vGaps * vGapSize) / rows;
            Vector2 vGap = new(0, vGapSize + elementHeight);

            Vector2 elementSize = new(elementWidth, elementHeight);
            int displayedItems = endIndex - startIndex;

            for (int i = 0; i < displayedItems; i++)
            //for (int i = startIndex; i < endIndex; i++)
            {
                var item = children[i + startIndex];
                var coords = Utils.TransformIndexToCoordinates(i, rows, columns, leftToRight);

                //int col = i / rows;
                //int row = i % rows;
                item.UpdateRect(startPos + hGap * coords.col + vGap * coords.row, elementSize, Alignement.TOPLEFT);
            }
        }
    }

}
