namespace ShapeEngineCore
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
        public virtual void DrawUI() { }
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

        public virtual void MonitorHasChanged()
        {
            var area = GetCurArea();
            if (area != null) area.MonitorHasChanged();
        }
    }
}
