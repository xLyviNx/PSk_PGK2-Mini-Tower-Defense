using PGK2.Engine.Components.Base.Renderers;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	/// <summary>
	/// Kontroluje scene w przypadku wygranej w grze.
	/// </summary>
	public class WinController : Component
	{
		/// <summary>
		/// Przycisk umożliwiający przejście do menu głównego.
		/// </summary>
		private UI_Button MenuButton;

		/// <summary>
		/// Przycisk umożliwiający ponowne uruchomienie gry.
		/// </summary>
		private UI_Button ReplayButton;

		/// <summary>
		/// Metoda wywoływana po przebudzeniu obiektu.
		/// </summary>
		public override void Awake()
		{
			base.Awake();

			MenuButton = MyScene.FindObjectByName("MenuButton").GetComponent<UI_Button>();
			ReplayButton = MyScene.FindObjectByName("ReplayButton").GetComponent<UI_Button>();

			ReplayButton.OnClick += () =>
			{
				var scene = SceneManager.LoadSceneFromFile($"{EngineInstance.ASSETS_PATH}/Scenes/GAME.lscn");
				SceneManager.ChangeSceneAsync(scene);
			};

			MenuButton.OnClick += () =>
			{
				var scene = SceneManager.LoadSceneFromFile($"{EngineInstance.ASSETS_PATH}/Scenes/MENU.lscn");
				SceneManager.ChangeSceneAsync(scene);
			};
		}
	}
}
