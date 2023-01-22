using System.Numerics;

namespace ShapeCore
{
    public class Scene
    {
        protected bool paused = false;


        public Scene() { }


        public virtual Area? GetCurArea() { return null; }


        public virtual void Activate(Scene? oldScene) { }
        public virtual void Deactivate(Scene? newScene) { if (newScene != null) GAMELOOP.SwitchScene(this, newScene); }

        public virtual void Start() { }
        public virtual void Close() { }
        public virtual void Draw() { }
        /// <summary>
        /// Used for drawing UI elements like buttons, progress bars and text.
        /// </summary>
        /// <param name="uiSize"> The actual size of the UI texture. Can be used to make position and size of ui elements relative.</param>
        /// <param name="stretchFactor">Should be used for ui elements like buttons where the position and size is set a start but 
        /// the button should still be relative to the current screen size. Just multiply your raw position and size with the stretch factor.
        /// ONLY needed if texture mode is not fixed! </param>
        public virtual void DrawUI(Vector2 uiSize, Vector2 stretchFactor) { }
        //public virtual void HandleInput(float dt) { }
        public virtual void Update(float dt) { }


        public virtual bool IsPaused() { return paused; }
        public virtual void Pause()
        {
            if (paused) return;
            paused = true;
        }
        public virtual void Resume()
        {
            if (!paused) return;
            paused = false;
        }
        public virtual void TogglePause() { paused = !paused; }
        //public virtual void MonitorHasChanged()
        //{
        //    var area = GetCurArea();
        //    if (area != null) area.MonitorHasChanged();
        //}
    }
}
