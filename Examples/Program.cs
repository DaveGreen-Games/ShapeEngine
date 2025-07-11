global using static Examples.Program;
using ShapeEngine.Input;

namespace Examples;
public static class Program
{
    public static GameloopExamples GAMELOOP = new();
    public static void Main(string[] args)
    {
        GAMELOOP.Run(args);
    }
}
