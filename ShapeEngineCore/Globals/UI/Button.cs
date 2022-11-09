using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals.UI
{
    public class Button : UIElementSelectable
    {
        protected Vector2 offset = new();
        protected Vector2 sizeOffset = new(1f, 1f);
        protected EaseHandler easeHandler = new();
        protected bool centered = false;

        protected UISelectionColors stateColors = new();

        //public Button(Vector2 pos, Vector2 size, bool centered = false)
        //{
        //    this.centered = centered;
        //
        //    if (centered)
        //    {
        //        UpdateRect(pos, size);
        //        //rect = new(pos.X - size.X * 0.5f, pos.Y - size.Y * 0.5f, size.X, size.Y);
        //    }
        //    else
        //    {
        //        UpdateRect(new(pos.X, pos.Y, size.X, size.Y));
        //    }
        //}

        public void SetStateColors(UISelectionColors newColors) { this.stateColors = newColors; }

        public override bool IsAutomaticDetectionDirectionEnabled(UINeighbors.NeighborDirection dir)
        {
            switch (dir)
            {
                case UINeighbors.NeighborDirection.TOP: return true;
                case UINeighbors.NeighborDirection.RIGHT: return true;
                case UINeighbors.NeighborDirection.BOTTOM: return true;
                case UINeighbors.NeighborDirection.LEFT: return true;
                default: return true;
            }
        }
        public override void Update(float dt, Vector2 mousePosUI)
        {
            base.Update(dt, mousePosUI);
            easeHandler.Update(dt);
            if (easeHandler.HasChain("offset"))
            {
                var result = easeHandler.GetVector2("offset");
                offset = result;
            }
            else { offset.X = 0f; offset.Y = 0f; }

            if (easeHandler.HasChain("sizeUp"))
            {
                var result = easeHandler.GetVector2("sizeUp");
                sizeOffset = result;
            }
        }
        public override void Draw(Vector2 uisSize, Vector2 stretchFactor)
        {
            Color color = stateColors.baseColor;
            if (disabled) color = stateColors.disabledColor;
            else if (pressed) color = stateColors.pressedColor;
            else if (hovered) color = stateColors.hoveredColor;


            Rectangle offsetRect;
            Rectangle rect = GetRect(Alignement.TOPLEFT);
            if (centered)
            {
                Vector2 size = GetSize();
                Vector2 newSize = new Vector2 (size.X * sizeOffset.X, size.Y * sizeOffset.Y);
                Vector2 sizeDif = newSize - size;
                offsetRect =
                    new(
                        rect.X + offset.X - sizeDif.X * 0.5f,
                        rect.Y + offset.Y - sizeDif.Y * 0.5f,
                        newSize.X,
                        newSize.Y
                        );
            }
            else
            {
                offsetRect = new Rectangle(rect.X + offset.X, rect.Y + offset.Y, rect.width * sizeOffset.X, rect.height * sizeOffset.Y);
            }
            DrawRectangleRec(offsetRect, color);
            if (selected) DrawRectangleV(new Vector2(offsetRect.X, offsetRect.y + offsetRect.height * 0.9f), new Vector2(offsetRect.width, offsetRect.height * 0.1f), stateColors.selectedColor);
            
        }

        public override void PressedChanged(bool pressed)
        {
            if (pressed)
            {
                if (easeHandler.HasChain("sizeUp")) return;
                easeHandler.AddChain(
                    "sizeUp",
                    new Vector2(1, 1),
                    new EaseOrder(0.1f, new Vector2(0.2f, 0), EasingType.QUAD_IN),
                    new EaseOrder(0.1f, new Vector2(-0.2f, 0), EasingType.QUAD_OUT)
                    //new EaseOrder(0.05f, new Vector2(0.05f, 0.05f), EasingType.QUAD_OUT),
                    //new EaseOrder(0.1f, new Vector2(-0.1f, -0.1f), EasingType.BACK_IN),
                    //new EaseOrder(0.05f, new Vector2(0.02f, 0.02f), EasingType.LINEAR_IN)
                    );
            }
            else
            {
                sizeOffset.X = 1f;
                sizeOffset.Y = 1f;
            }
        }
        public override void MousePressedChanged(bool pressed)
        {
            if (pressed)
            {
                if (easeHandler.HasChain("sizeUp")) return;
                easeHandler.AddChain(
                    "sizeUp",
                    new Vector2(1, 1),
                    new EaseOrder(0.1f, new Vector2(0.2f, 0), EasingType.QUAD_IN),
                    new EaseOrder(0.1f, new Vector2(-0.2f, 0), EasingType.QUAD_OUT)
                    //new EaseOrder(0.05f, new Vector2(0.05f, 0.05f), EasingType.QUAD_OUT),
                    //new EaseOrder(0.1f, new Vector2(-0.1f, -0.1f), EasingType.BACK_IN),
                    //new EaseOrder(0.05f, new Vector2(0.02f, 0.02f), EasingType.LINEAR_IN)
                    );
            }
            else
            {
                sizeOffset.X = 1f;
                sizeOffset.Y = 1f;
            }
        }
        public override void HoveredChanged(bool hovered)
        {
            if (!hovered) return;

            if (easeHandler.HasChain("offset")) return;
            easeHandler.AddChain(
                "offset",
                new Vector2(0, 0),
                new EaseOrder(0.1f, new Vector2(50, 0), EasingType.QUAD_OUT),
                new EaseOrder(0.2f, new Vector2(-75, 0), EasingType.BACK_IN),
                new EaseOrder(0.1f, new Vector2(25, 0), EasingType.LINEAR_IN)
                );

        }
        public override void SelectedChanged(bool selected)
        {
            if (!selected) return;

            if (easeHandler.HasChain("offset")) return;
            easeHandler.AddChain(
                "offset",
                new Vector2(0, 0),
                new EaseOrder(0.1f, new Vector2(-50, 0), EasingType.QUAD_OUT),
                new EaseOrder(0.2f, new Vector2(75, 0), EasingType.BACK_IN),
                new EaseOrder(0.1f, new Vector2(-25, 0), EasingType.LINEAR_IN)
                );
        }
    }

}
