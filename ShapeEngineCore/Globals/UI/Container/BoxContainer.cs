using Raylib_CsLo;
using System.Numerics;

namespace ShapeEngineCore.Globals.UI.Container
{
    public class BoxContainer : UIContainer
    {
        protected float gapRelative = 0f;
        protected bool vContainer = true;
        public Vector2 maxElementSizeRel = new(0f);
        public BoxContainer() { }
        public BoxContainer(float gapRelative)
        {
            this.gapRelative = gapRelative;
        }
        public BoxContainer(float gapRelative, List<UIElement> children) : base(children)
        {
            this.gapRelative = gapRelative;
        }

        public BoxContainer(float gapRelative, params UIElement[] children) : base(children)
        {
            this.gapRelative = gapRelative;
        }
        public BoxContainer(float gapRelative, bool vContainer)
        {
            this.gapRelative = gapRelative;
            this.vContainer = vContainer;
        }
        public BoxContainer(float gapRelative, bool vContainer, List<UIElement> children) : base(children)
        {
            this.gapRelative = gapRelative;
            this.vContainer = vContainer;
        }

        public BoxContainer(float gapRelative, bool vContainer, params UIElement[] children) : base(children)
        {
            this.gapRelative = gapRelative;
            this.vContainer = vContainer;
        }


        public bool IsVContainer = true;
        public void SetVContainer(bool vContainer) { this.vContainer = vContainer; }
        public void SetGap(float newGapRelative)
        {
            gapRelative = newGapRelative;
        }
        public override void Update(float dt, Vector2 mousePosUI)
        {
            UpdateRects(GetRect(), 0, children.Count);
            UpdateChildren(dt, mousePosUI);
        }


        protected virtual void UpdateRects(Rectangle rect, int startIndex, int endIndex)
        {
            Vector2 startPos = new(rect.x, rect.y);
            float stretchFactorTotal = 0f;
            for (int i = startIndex; i < endIndex; i++)
            {
                stretchFactorTotal += children[i].GetStretchFactor();
            }
            int count = endIndex - startIndex;
            int gaps = count - 1;

            if (vContainer)
            {
                float totalHeight = rect.height;
                float gapSize = totalHeight * gapRelative;
                float elementHeight = (totalHeight - gaps * gapSize) / stretchFactorTotal;
                Vector2 offset = new(0f, 0f);
                for (int i = startIndex; i < endIndex; i++)
                {
                    var item = children[i];
                    float height = elementHeight * item.GetStretchFactor();
                    Vector2 size = new(rect.width, height);
                    Vector2 maxSize = maxElementSizeRel * new Vector2(rect.width, rect.height);
                    if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
                    if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
                    item.UpdateRect(startPos + offset, size, Alignement.TOPLEFT);
                    offset += new Vector2(0, gapSize + size.Y);
                }
            }
            else
            {
                float totalWidth = rect.width;
                float gapSize = totalWidth * gapRelative;
                float elementWidth = (totalWidth - gaps * gapSize) / stretchFactorTotal;
                Vector2 offset = new(0f, 0f);
                for (int i = startIndex; i < endIndex; i++)
                {
                    var item = children[i];
                    float width = elementWidth * item.GetStretchFactor();
                    Vector2 size = new(width, rect.height);
                    Vector2 maxSize = maxElementSizeRel * new Vector2(rect.width, rect.height);
                    if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
                    if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
                    item.UpdateRect(startPos + offset, size, Alignement.TOPLEFT);
                    offset += new Vector2(gapSize + width, 0f);
                }
            }

        }
    }

}
