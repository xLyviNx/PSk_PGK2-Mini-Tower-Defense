using OpenTK.Windowing.Desktop;

namespace PGK2.Engine.Core
{
	public sealed class EngineInstance
	{
		public static EngineInstance? Instance { get; private set; }
		public static readonly string ENGINE_PATH = ".//..//..//..//Engine";
		public static readonly string GAME_PATH = ".//..//..//..//Game";
		public static string ASSETS_PATH => $"{GAME_PATH}/Assets";
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
			NativeWindowSettings nWS = new NativeWindowSettings() ;
			GameWindowSettings gWS = GameWindowSettings.Default;
			nWS.ClientSize = new(1280, 720);
			nWS.Title = "Application";
			nWS.APIVersion = System.Version.Parse("4.1");
			using (window = new EngineWindow(gWS, nWS))
			{
				window.VSync = OpenTK.Windowing.Common.VSyncMode.On;

				window.Run();
			}
		}
        public enum RenderPass
        {
            Opaque,
			Transparent,
			Outline,

        }
    }
}
