using Raylib_CsLo;
using ShapeEngineCore.Globals.Screen;

namespace ShapeEngineCore.Globals.Shaders
{

    public static class ShaderHandler
    {
        private static Dictionary<string, ScreenShader> screenShaders = new Dictionary<string, ScreenShader>();
        private static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
        private static ScreenBuffer[] screenBuffers = new ScreenBuffer[0];
        //{
        //    new(ScreenHandler.GameWidth(), ScreenHandler.GameHeight(), ScreenHandler.GameWidth(), ScreenHandler.GameHeight()),
        //    new(ScreenHandler.GameWidth(), ScreenHandler.GameHeight(), ScreenHandler.GameWidth(), ScreenHandler.GameHeight())
        //};



        public static void Initialize()
        {
            screenBuffers = new ScreenBuffer[]
            {
                new(ScreenHandler.GameWidth(), ScreenHandler.GameHeight(), ScreenHandler.GameWidth(), ScreenHandler.GameHeight()),
                new(ScreenHandler.GameWidth(), ScreenHandler.GameHeight(), ScreenHandler.GameWidth(), ScreenHandler.GameHeight())
            };
            //ScreenHandler.CreateGameTexture("screenshader");
        }
        public static void Close()
        {
            foreach (ScreenBuffer screenBuffer in screenBuffers)
            {
                screenBuffer.Unload();
            }
            foreach (Shader shader in shaders.Values)
            {
                UnloadShader(shader);
            }
            foreach (ScreenShader screenShader in screenShaders.Values)
            {
                screenShader.Unload();
            }
            screenShaders.Clear();
            shaders.Clear();
            screenBuffers = new ScreenBuffer[0];
        }
        public static void DrawShaders()
        {
            //BeginMode2D(ScreenHandler.curCamera);
            //ScreenHandler.GetTexture(screenName).Draw();
            //EndMode2D();
            //return;

            if (screenShaders.Count <= 0)
            {
                ScreenHandler.Game.Draw();
                return;
            }
            List<ScreenShader> shadersToApply = screenShaders.Values.ToList().FindAll(s => s.IsEnabled());
            shadersToApply.Sort(delegate (ScreenShader a, ScreenShader b)
            {
                if (a.GetOrder() < b.GetOrder()) return -1;
                else if (a.GetOrder() > b.GetOrder()) return 1;
                else return 0;
            });

            if (shadersToApply.Count <= 0)
            {
                ScreenHandler.Game.Draw();
                return;
            }
            else if (shadersToApply.Count == 1)
            {
                ScreenShader s = shadersToApply[0];
                BeginShaderMode(s.GetShader());
                ScreenHandler.Game.Draw();
                EndShaderMode();
            }
            else if (shadersToApply.Count == 2)
            {
                ScreenShader s = shadersToApply[0];
                //ScreenHandler.StartDraw("screenshader");
                screenBuffers[0].StartTextureMode();
                BeginShaderMode(s.GetShader());
                ScreenHandler.Game.DrawPro(ScreenHandler.GameWidth(), ScreenHandler.GameHeight());
                EndShaderMode();
                screenBuffers[0].EndTextureMode();
                //ScreenHandler.EndDraw("screenshader");

                s = shadersToApply[1];

                BeginShaderMode(s.GetShader());
                screenBuffers[0].DrawPro(ScreenHandler.GetCurWindowSizeWidth(), ScreenHandler.GetCurWindowSizeHeight());
                //ScreenHandler.GetTexture("screenshader").Draw();
                EndShaderMode();
            }
            else
            {
                ScreenShader s = shadersToApply[0];
                shadersToApply.RemoveAt(0);

                ScreenShader endshader = shadersToApply[shadersToApply.Count - 1];
                shadersToApply.RemoveAt(shadersToApply.Count - 1);

                //draw game texture to first screenbuffer and first shader is already applied
                screenBuffers[0].StartTextureMode();
                BeginShaderMode(s.GetShader());
                ScreenHandler.Game.DrawPro(ScreenHandler.GameWidth(), ScreenHandler.GameHeight());
                EndShaderMode();
                screenBuffers[0].EndTextureMode();

                int currentIndex = 0;
                int nextIndex = 0;
                for (int i = 0; i < shadersToApply.Count; i++)
                {
                    s = shadersToApply[i];
                    nextIndex = currentIndex == 0 ? 1 : 0;
                    ScreenBuffer current = screenBuffers[currentIndex];
                    ScreenBuffer next = screenBuffers[nextIndex];
                    next.StartTextureMode();
                    BeginShaderMode(s.GetShader());
                    current.DrawPro(ScreenHandler.GameWidth(), ScreenHandler.GameHeight());
                    EndShaderMode();
                    next.EndTextureMode();
                    currentIndex = currentIndex == 0 ? 1 : 0;
                }

                //draw to screen
                //ScreenHandler.StartDraw("screenshader");
                BeginShaderMode(endshader.GetShader());
                screenBuffers[nextIndex].DrawPro(ScreenHandler.GetCurWindowSizeWidth(), ScreenHandler.GetCurWindowSizeHeight());
                EndShaderMode();
                //ScreenHandler.EndDraw("screenshader");

                //BeginShaderMode(endshader1.GetShader());
                //ScreenHandler.GetTexture("screenshader").Draw();
                //EndShaderMode();
            }
        }


        public static bool HasScreenShader(string name) { return screenShaders.ContainsKey(name); }
        public static void AddScreenShader(string name, string path, bool enabled = true, int order = 0)
        {
            if (name == "" || path == "") return;
            if (screenShaders.ContainsKey(name)) return;
            screenShaders[name] = new ScreenShader(path, name, enabled);
        }
        public static void RemoveScreenShader(string name)
        {
            //if (!screenShaders.ContainsKey(name)) return;
            screenShaders.Remove(name);
        }
        public static ScreenShader? GetScreenShader(string name)
        {
            if (!screenShaders.ContainsKey(name)) return null;
            return screenShaders[name];
        }

        public static void AddShader(string name, string path)
        {
            if (name == "" || path == "") return;
            if (shaders.ContainsKey(name)) return;
            shaders[name] = LoadShader("330", path);
        }
        public static void RemoveShader(string name)
        {
            shaders.Remove(name);
        }
        public static Shader? GetShader(string name)
        {
            if (!shaders.ContainsKey(name)) return null;
            return shaders[name];
        }

        public static bool IsScreenShaderEnabled(string name)
        {
            if (!screenShaders.ContainsKey(name)) return false;
            return screenShaders[name].IsEnabled();
        }
        public static void EnableScreenShader(string name)
        {
            if (!screenShaders.ContainsKey(name)) return;
            screenShaders[name].Enable();
        }
        public static void DisableScreenShader(string name)
        {
            if (!screenShaders.ContainsKey(name)) return;
            screenShaders[name].Disable();
        }
        public static void SetScreenShaderValueFloat(string shaderName, string propertyName, float value)
        {
            if (!screenShaders.ContainsKey(shaderName)) return;
            Shader shader = screenShaders[shaderName].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        }
        public static void SetScreenShaderValueVec(string shaderName, string propertyName, float[] values, ShaderUniformDataType dataType)
        {
            if (!screenShaders.ContainsKey(shaderName)) return;
            Shader shader = screenShaders[shaderName].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, values, dataType);
        }
        public static void SetShaderValueFloat(string shaderName, string propertyName, float value)
        {
            if (!shaders.ContainsKey(shaderName)) return;
            Shader shader = shaders[shaderName];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        }
        public static void SetShaderValueVec(string shaderName, string propertyName, float[] values, ShaderUniformDataType dataType)
        {
            if (!shaders.ContainsKey(shaderName)) return;
            Shader shader = shaders[shaderName];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, values, dataType);
        }
    }
}
