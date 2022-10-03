using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals.UI
{
    public class Button : UIElementSelectable
    {
        protected Vector2 offsetRelative = new();
        protected Vector2 sizeOffset = new(1f, 1f);
        protected EaseHandler easeHandler = new();
        protected bool centered = false;

        protected UISelectionColors stateColors = new();

        public Button(Vector2 posRelative, Vector2 sizeRelative, bool centered = false)
        {
            this.centered = centered;

            if (centered)
            {
                rect = new(posRelative.X - sizeRelative.X * 0.5f, posRelative.Y - sizeRelative.Y * 0.5f, sizeRelative.X, sizeRelative.Y);
            }
            else
            {
                rect = new(posRelative.X, posRelative.Y, sizeRelative.X, sizeRelative.Y);
            }
        }




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
        public override void Update(float dt, Vector2 mousePos)
        {
            base.Update(dt, mousePos);
            easeHandler.Update(dt);
            if (easeHandler.HasChain("offset"))
            {
                var result = easeHandler.GetVector2("offset");
                offsetRelative = result;
            }
            else { offsetRelative.X = 0f; offsetRelative.Y = 0f; }

            if (easeHandler.HasChain("sizeUp"))
            {
                var result = easeHandler.GetVector2("sizeUp");
                sizeOffset = result;
            }
        }
        public override void Draw()
        {
            Color color = stateColors.baseColor;
            if (disabled) color = stateColors.disabledColor;
            else if (pressed) color = stateColors.pressedColor;
            else if (hovered) color = stateColors.hoveredColor;


            Rectangle offsetRect;
            if (centered)
            {
                Vector2 size = new(rect.width, rect.height);
                Vector2 newSize = new(size.X * sizeOffset.X, size.Y * sizeOffset.Y);
                Vector2 sizeDif = newSize - size;
                offsetRect =
                    new(
                        rect.X + offsetRelative.X - sizeDif.X * 0.5f,
                        rect.Y + offsetRelative.Y - sizeDif.Y * 0.5f,
                        newSize.X,
                        newSize.Y
                        );
            }
            else
            {
                offsetRect = new(rect.X + offsetRelative.X, rect.Y + offsetRelative.Y, rect.width * sizeOffset.X, rect.height * sizeOffset.Y);
            }
            DrawRectangleRec(ToAbsolute(offsetRect), color);
            if (selected) DrawRectangleV(ToAbsolute(new Vector2(offsetRect.x, offsetRect.y + offsetRect.height * 0.9f)), ToAbsolute(new Vector2(offsetRect.width, offsetRect.height * 0.1f)), stateColors.selectedColor);
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
                new EaseOrder(0.1f, new Vector2(0.05f, 0), EasingType.QUAD_OUT),
                new EaseOrder(0.2f, new Vector2(-0.075f, 0), EasingType.BACK_IN),
                new EaseOrder(0.1f, new Vector2(0.025f, 0), EasingType.LINEAR_IN)
                );

        }
        public override void SelectedChanged(bool selected)
        {
            if (!selected) return;

            if (easeHandler.HasChain("offset")) return;
            easeHandler.AddChain(
                "offset",
                new Vector2(0, 0),
                new EaseOrder(0.1f, new Vector2(-0.05f, 0), EasingType.QUAD_OUT),
                new EaseOrder(0.2f, new Vector2(0.075f, 0), EasingType.BACK_IN),
                new EaseOrder(0.1f, new Vector2(-0.025f, 0), EasingType.LINEAR_IN)
                );
        }
    }

}
