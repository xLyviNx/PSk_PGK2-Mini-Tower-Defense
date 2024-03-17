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

				GameObject testgo = new("TEST OBJ");
				Console.WriteLine(testgo);
				Console.WriteLine($"Is null? {testgo==null}");
				testgo.Destroy();
				Console.WriteLine(testgo);
				Console.WriteLine($"Is null? {testgo == null}");
				Console.WriteLine($"Is null? {null == testgo}");
				Console.WriteLine(testgo);

			}
		}
	}
}
