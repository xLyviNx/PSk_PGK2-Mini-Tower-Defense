using PGK2.Engine.Components.Base.Renderers;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.Game.Code.TowerDef.Scripts
{ 
	/**
     * @class Menu
     * @brief Reprezentuje menu gry Tower Defense.
     */
	public class Menu : Component
{
	/// <summary>
	/// Ścieżka do pliku sceny gry.
	/// </summary>
	public static string GameSceneFile => $"{EngineInstance.ASSETS_PATH}/Scenes/GAME.lscn";

	/// <summary>
	/// Ścieżka do pliku sceny menu.
	/// </summary>
	public static string MenuSceneFile => $"{EngineInstance.ASSETS_PATH}/Scenes/MENU.lscn";

	/// <summary>
	/// Ścieżka do pliku sceny z ekranem końca gry.
	/// </summary>
	public static string GameOverScene => $"{EngineInstance.ASSETS_PATH}/Scenes/GAMEOVER.lscn";

	/// <summary>
	/// Ścieżka do pliku sceny z ekranem wygranej.
	/// </summary>
	public static string WinScene => $"{EngineInstance.ASSETS_PATH}/Scenes/WIN.lscn";

	/// <summary>
	/// Przycisk rozpoczęcia gry.
	/// </summary>
	UI_Button? PlayButton;

	/// <summary>
	/// Przycisk wyjścia z gry.
	/// </summary>
	UI_Button? QuitButton;

	/// <summary>
	/// Metoda wywoływana podczas uruchamiania komponentu.
	/// </summary>
	public override void Awake()
		{
			base.Awake();
			PlayButton = MyScene.FindObjectByName("PlayButton")?.GetComponent<UI_Button>();
			QuitButton = MyScene.FindObjectByName("QuitButton")?.GetComponent<UI_Button>();
			if (PlayButton!=null)
				PlayButton.OnClick += PlayClicked;
			if (QuitButton!=null)
				QuitButton.OnClick += QuitClicked;
		}
		/// <summary>
		/// Obsługa kliknięcia przycisku "Wyjście".
		/// </summary>
		private void QuitClicked()
		{
			EngineWindow.instance.Close();
		}
		/// <summary>
		/// Obsługa kliknięcia przycisku "Play".
		/// </summary>
		private void PlayClicked()
		{
			var GameScene = SceneManager.LoadSceneFromFile(GameSceneFile);
			SceneManager.UnloadScene(MyScene);
			SceneManager.LoadScene(GameScene);
		}
	}
}
