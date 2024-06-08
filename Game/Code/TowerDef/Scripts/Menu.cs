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
		UI_Button? PlayButton;
		public override void Awake()
		{
			base.Awake();
			PlayButton = MyScene.FindObjectByName("PlayButton")?.GetComponent<UI_Button>();
			if (PlayButton!=null)
			{
				PlayButton.OnClick += PlayClicked;
			}
		}

		private void PlayClicked()
		{
			var GameScene = SceneManager.LoadSceneFromFile(GameSceneFile);
			SceneManager.UnloadScene(MyScene);
			SceneManager.LoadScene(GameScene);
		}
	}
}
