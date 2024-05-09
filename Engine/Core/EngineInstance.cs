namespace PGK2.Engine.Core
{
	public sealed class EngineInstance
	{
		public static EngineInstance? Instance { get; private set; }
		public EngineWindow? window;

		public static void CreateInstance()
		{
			if (Instance == null) 
			{
				Instance = new EngineInstance();
				Instance.Init();
			}
		}

		private void Init()
		{
			using (window = new EngineWindow(1280, 720, "Application"))
			{
				window.VSync = OpenTK.Windowing.Common.VSyncMode.On;
				window.Run();
			}
		}
	}
}
