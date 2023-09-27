
global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
using System.Numerics;
using Raylib_CsLo;


namespace ScreenSystem2
{
    public class ShapeTexture
    {
        public static readonly float MinResolutionFactor = 0.25f;
        public static readonly float MaxResolutionFactor = 4f;
        
        public bool Valid { get; private set; } = false;
        public RenderTexture RenderTexture { get; private set; } = new();
        public int Width { get; private set; } = 0;
        public int Height { get; private set; } = 0;

        public ShapeTexture(){}

        public void Load(int w, int h)
        {
            if (Valid) return;
            Valid = true;
            SetTexture(w, h);
        }

        public void Unload()
        {
            if (!Valid) return;
            Valid = false;
            UnloadRenderTexture(RenderTexture);
        }
        
        public void Update(int w, int h)
        {
            if (!Valid) return;

            if (Width == w && Height == h) return;
            
            UnloadRenderTexture(RenderTexture);
            SetTexture(w, h);
        }
        public void Draw()
        {
            var destRec = new Rectangle
            {
            x = Width * 0.5f,
            y = Height * 0.5f,
            width = Width,
            height = Height
            };
            Vector2 origin = new()
            {
            X = Width * 0.5f,
            Y = Height * 0.5f
            };
            
            var sourceRec = new Rectangle(0, 0, Width, -Height);
            
            DrawTexturePro(RenderTexture.texture, sourceRec, destRec, origin, 0f, WHITE);
        }
        
        private void SetTexture(int w, int h)
        {
            Width = w;
            Height = h;
            RenderTexture = LoadRenderTexture(Width, Height);
        }
        //public void DrawTexture(int targetWidth, int targetHeight)
        //{
            //var destRec = new Rectangle
            //{
            //    x = targetWidth * 0.5f,
            //    y = targetHeight * 0.5f,
            //    width = targetWidth,
            //    height = targetHeight
            //};
            //Vector2 origin = new()
            //{
            //    X = targetWidth * 0.5f,
            //    Y = targetHeight * 0.5f
            //};
            //
            //
            //
            //var sourceRec = new Rectangle(0, 0, Width, -Height);
            //
            //DrawTexturePro(RenderTexture.texture, sourceRec, destRec, origin, 0f, WHITE);
        //
    }
    public class ShapeCamera
    {
        public static float MinZoomLevel = 0.1f;
        public static float MaxZoomLevel = 10f;
        
        
        public Vector2 Position { get; set; }= new();
        public Vector2 Size { get; private set; }= new();
        public Vector2 Alignement{ get; private set; } = new(0.5f);
        public Vector2 Offset => Size * Alignement;
        public float ZoomLevel { get; private set; }= 1f;
        public float RotationDeg { get; private set; }= 0f;

        
        public ShapeCamera() { }
        public ShapeCamera(Vector2 pos)
        {
            this.Position = pos;
        }
        public ShapeCamera(Vector2 pos, Vector2 alignement)
        {
            this.Position = pos;
            this.SetAlignement(alignement);
        }
        public ShapeCamera(Vector2 pos, Vector2 alignement, float zoomLevel)
        {
            this.Position = pos;
            this.SetAlignement(alignement);
            this.SetZoom(zoomLevel);
        }
        public ShapeCamera(Vector2 pos, Vector2 alignement, float zoomLevel, float rotationDeg)
        {
            this.Position = pos;
            this.SetAlignement(alignement);
            this.SetZoom(zoomLevel);
            this.SetRotation(rotationDeg);
        }
        public ShapeCamera(Vector2 pos, float zoomLevel)
        {
            this.Position = pos;
            this.SetZoom(zoomLevel);
        }

        public Rectangle Area => new
        (
            Position.X - Offset.X * ZoomFactor, 
            Position.Y - Offset.Y * ZoomFactor, 
            Size.X * ZoomFactor,
            Size.Y * ZoomFactor
        );
        public Camera2D Camera => new()
        {
            target = Position,
            offset = Offset,
            zoom = ZoomLevel,
            rotation = RotationDeg
        };

        public float ZoomFactor => 1f / ZoomLevel;

        
        public void Update(float dt, int screenWidth, int screenHeight)
        {
            Size = new(screenWidth, screenHeight);
        }
        
        public void Reset()
        {
            Position = new();
            Alignement = new(0.5f);
            ZoomLevel = 1f;
            RotationDeg = 0f;
        }
        
        public void Zoom(float change) => SetZoom(ZoomLevel + change);
        public void SetZoom(float zoomLevel)
        {
            ZoomLevel = zoomLevel;
            if (ZoomLevel > MaxZoomLevel) ZoomLevel = MaxZoomLevel;
            else if (ZoomLevel < MinZoomLevel) ZoomLevel = MinZoomLevel;
        }

        public void Rotate(float deg) => SetRotation(RotationDeg + deg);
        public void SetRotation(float deg)
        {
            RotationDeg = deg;
            RotationDeg = Wrap(RotationDeg, 0f, 360f);
        }

        public void SetAlignement(Vector2 newAlignement) => Alignement = Vector2.Clamp(newAlignement, Vector2.Zero, Vector2.One);

        public Vector2 ScreenToWorld(Vector2 pos) => GetScreenToWorld2D(pos, Camera);
        public Vector2 WorldToScreen(Vector2 pos) => GetWorldToScreen2D(pos, Camera);
        private static float Wrap(float value, float min, float max) => value - (max - min) * MathF.Floor((float) (( value -  min) / ( max -  min)));
    }

    public class Player
    {
        private Vector2 pos;
        private Vector2 dir;
        private float size;
        private float speed = 150f;
        
        public Player(Vector2 pos, float size)
        {
            this.pos = pos;
            this.size = size;
        }

        public void Update(float dt, Rectangle cameraArea)
        {
            Vector2 movement = new();
            if (IsKeyDown(KeyboardKey.KEY_A)) movement.X -= 1f;
            else if (IsKeyDown(KeyboardKey.KEY_D)) movement.X += 1f;

            if (IsKeyDown(KeyboardKey.KEY_W)) movement.Y -= 1f;
            else if (IsKeyDown(KeyboardKey.KEY_S)) movement.Y += 1f;

            if (movement.LengthSquared() > 0f)
            {
                movement = Vector2.Normalize(movement);
                dir = movement;
                pos += movement * speed * dt;
            }

            Program.Camera.Position = pos;
        }

        public void Draw(Rectangle cameraArea, Vector2 mousePos)
        {
            float thickness = 2f;
            DrawPolyLinesEx(pos, 16, size, 0f, thickness, RED);
            DrawCircleV(pos, thickness, RED);
            Vector2 thruster = pos + dir * size;
            DrawCircleV(thruster, size / 2, RED);
        }
    }
    
    public static class Program
    {
        public static (int width, int height) CurScreenSize = (800, 800);
        public static ShapeCamera Camera = new();
        public static ShapeTexture GameTexture = new();
        
        public static Player Player = new(new(), 14);
        public static Vector2[] Stars = CreateStars(10000, 5000, 5000);
        
        public static void CalculateCurScreenSize()
        {
            if (IsWindowFullscreen())
            {
                var monitor = GetCurrentMonitor();
                var mw = GetMonitorWidth(monitor);
                var mh = GetMonitorHeight(monitor);
                var scaleFactor = GetWindowScaleDPI();
                int scaleX = (int)scaleFactor.X;
                int scaleY = (int)scaleFactor.Y;
                CurScreenSize = new(mw * scaleX, mh * scaleY);
            }
            else
            {
                var w = GetScreenWidth();
                var h = GetScreenHeight();
                CurScreenSize = new(w, h);
            }
        }

        
        public static int Main(string[] args)
        {
            InitWindow(CurScreenSize.width, CurScreenSize.height, "Screen System 2.0");
            ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            
            GameTexture.Load(CurScreenSize.width, CurScreenSize.height);
            
            while (!WindowShouldClose())
            {
                var dt = GetFrameTime();
                CalculateCurScreenSize();
                
                Camera.Update(dt, CurScreenSize.width, CurScreenSize.height);
                
                var mousePosScreen = GetMousePosition();
                var mousePosGame = Camera.ScreenToWorld(mousePosScreen);
                Rectangle screenArea = new(0, 0, CurScreenSize.width, CurScreenSize.height);
                Rectangle cameraArea = Camera.Area;
                
                Update(dt, cameraArea);

                BeginTextureMode(GameTexture.RenderTexture);
                ClearBackground(new(0,0,0,0));
                
                BeginMode2D(Camera.Camera);
                DrawGame(cameraArea, mousePosGame);
                EndMode2D();
                
                EndTextureMode();
                
                BeginDrawing();
                ClearBackground(BLACK);

                GameTexture.Draw();
                DrawUI(screenArea, mousePosScreen);
                
                EndDrawing();
            }
            
            CloseWindow();
            return 0;
        }

        public static void Update(float dt, Rectangle cameraArea)
        {
            if(IsKeyPressed(KeyboardKey.KEY_F)) ToggleFullscreenMode();
            if (IsKeyPressed(KeyboardKey.KEY_M))
            {
                if (IsWindowMaximized()) ClearWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
                else SetWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
            }
            
            if(IsKeyPressed(KeyboardKey.KEY_C)) Camera.Zoom(0.25f);
            else if(IsKeyPressed(KeyboardKey.KEY_X)) Camera.Zoom(-0.25f);
            
            Player.Update(dt, cameraArea);
        }
        public static void DrawGame(Rectangle cameraArea, Vector2 mousePos)
        {
            for (var i = 0; i < Program.Stars.Length; i++)
            {
                var pos = Stars[i];
                var x = pos.X - 2;
                var y = pos.Y - 2;
                Rectangle r = new(x, y, 4, 4);
                if(CheckCollisionRecs(cameraArea, r)) DrawRectangleRec(r, WHITE);
            }
            Player.Draw(cameraArea, mousePos);
            
            DrawRectangleLinesEx(cameraArea, 6f * Camera.ZoomFactor, GRAY);
            DrawCircleV(mousePos, 10 * Camera.ZoomFactor, GRAY);
        }
        public static void DrawUI(Rectangle screenArea, Vector2 mousePos)
        {
            DrawRectangleLinesEx(screenArea, 2f, WHITE);
            DrawCircleV(mousePos, 5, WHITE);
        }

        private static Vector2[] CreateStars(int amount, int targetAreaWidth, int targetAreaHeight)
        {
            Random rand = new Random();
            Vector2[] points = new Vector2[amount];
            
            for (int i = 0; i < amount; i++)
            {
                int x = rand.Next(-targetAreaWidth, targetAreaWidth);
                int y = rand.Next(-targetAreaHeight, targetAreaHeight);
                points[i] = new Vector2(x, y);
            }

            return points;
        }
        private static void ToggleFullscreenMode()
        {
            ToggleFullscreen();
            CalculateCurScreenSize();
        }
    }
}