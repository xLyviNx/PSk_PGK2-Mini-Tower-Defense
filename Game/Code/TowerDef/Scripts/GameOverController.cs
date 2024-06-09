using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	public class GameOverController : Component
	{
		UI_Button MenuButton;
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
