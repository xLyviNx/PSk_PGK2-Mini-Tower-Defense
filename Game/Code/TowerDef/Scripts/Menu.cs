using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	public class Menu : Component
	{
		public static string GameSceneFile =>  $"{EngineInstance.ASSETS_PATH}/Scenes/GAME.lscn";
		public static string MenuSceneFile =>  $"{EngineInstance.ASSETS_PATH}/Scenes/MENU.lscn";
		public static string GameOverScene =>  $"{EngineInstance.ASSETS_PATH}/Scenes/GAMEOVER.lscn";
		public static string WinScene =>  $"{EngineInstance.ASSETS_PATH}/Scenes/WIN.lscn";
		UI_Button? PlayButton;
		UI_Button? QuitButton;
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

		private void QuitClicked()
		{
			EngineWindow.instance.Close();
		}

		private void PlayClicked()
		{
			var GameScene = SceneManager.LoadSceneFromFile(GameSceneFile);
			SceneManager.UnloadScene(MyScene);
			SceneManager.LoadScene(GameScene);
		}
	}
}
