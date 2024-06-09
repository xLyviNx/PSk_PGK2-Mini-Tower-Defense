using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	public class WinController : Component
	{
		UI_Button MenuButton;
		UI_Button ReplayButton;
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
