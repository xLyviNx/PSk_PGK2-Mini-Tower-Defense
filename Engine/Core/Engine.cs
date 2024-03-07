namespace PGK2.Engine.Core
{
	public sealed class Engine
	{
		public static Engine? Instance { get; private set; }
		public EngineWindow? window;

		public static void CreateInstance()
		{
			if (Instance == null) 
			{
				Instance = new Engine();
				Instance.Init();
			}
		}

		private void Init()
		{
			using (window = new EngineWindow(1280, 720, "Application"))
			{
				window.Run();
			}
		}
	}
}
