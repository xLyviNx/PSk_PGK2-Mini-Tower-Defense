using OpenTK.Windowing.Desktop;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Główna klasa silnika, odpowiedzialna za zarządzanie instancją silnika.
	/// </summary>
	public sealed class EngineInstance
	{
		/// <summary>
		/// Statyczna instancja silnika (singleton).
		/// </summary>
		public static EngineInstance? Instance { get; private set; }

		/// <summary>
		/// Ścieżka do katalogu silnika.
		/// </summary>
		public static readonly string ENGINE_PATH = ".//..//..//..//Engine";

		/// <summary>
		/// Ścieżka do katalogu gry.
		/// </summary>
		public static readonly string GAME_PATH = ".//..//..//..//Game";

		/// <summary>
		/// Ścieżka do zasobów gry.
		/// </summary>
		public static string ASSETS_PATH => $"{GAME_PATH}/Assets";

		/// <summary>
		/// Okno silnika.
		/// </summary>
		public EngineWindow? window;

		/// <summary>
		/// Tworzy instancję silnika, jeśli jeszcze nie została utworzona.
		/// </summary>
		public static void CreateInstance()
		{
			if (Instance == null)
			{
				Instance = new EngineInstance();
				Instance.Init();
			}
		}

		/// <summary>
		/// Inicjalizuje ustawienia okna i uruchamia główną pętlę gry.
		/// </summary>
		private void Init()
		{
			NativeWindowSettings nWS = new NativeWindowSettings();
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

		/// <summary>
		/// Enum określający różne etapy renderowania.
		/// </summary>
		public enum RenderPass
		{
			/// <summary>
			/// Renderowanie nieprzezroczystych obiektów.
			/// </summary>
			Opaque,

			/// <summary>
			/// Renderowanie przezroczystych obiektów.
			/// </summary>
			Transparent,

			/// <summary>
			/// Renderowanie obrysów obiektów.
			/// </summary>
			Outline,
		}
	}
}
