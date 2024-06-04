namespace PGK2.Engine.Core
{
    public class Time
    {
        public static long lastTime = 0;
        public static double doubleDeltaTime = 0;
        public static float deltaTime { get => ((float)doubleDeltaTime*1); set { doubleDeltaTime = value; } }

    }
}
