using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	/**
     * @class GameOverController
     * @brief Reprezentuje kontroler ekranu końca gry.
     */
	public class GameOverController : Component
	{
		/// <summary>
		/// Przycisk menu.
		/// </summary>
		UI_Button MenuButton;

		/// <summary>
		/// Metoda wywoływana podczas uruchamiania komponentu.
		/// </summary>
		public override void Awake()
		{
			base.Awake();

			MenuButton = MyScene.FindObjectByName("ReturnButton").GetComponent<UI_Button>();
			MenuButton.OnClick += () =>
			{
				var scene = SceneManager.LoadSceneFromFile($"{EngineInstance.ASSETS_PATH}/Scenes/MENU.lscn");
				SceneManager.ChangeSceneAsync(scene);
			};
		}
	}
}
