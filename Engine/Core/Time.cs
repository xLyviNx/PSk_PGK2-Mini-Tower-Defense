namespace PGK2.Engine.Core
{
    public class Time
    {
        public static long lastTime = 0;
        public static double doubleDeltaTime = 0;
        public static float timeScale = 1f;
        public static float unscaledDeltaTime { get => ((float)doubleDeltaTime); set { doubleDeltaTime = value; } }
        public static float deltaTime { get => unscaledDeltaTime * timeScale; }

    }
}
