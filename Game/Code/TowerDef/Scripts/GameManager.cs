using Assimp;
using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Components.Base.Renderers;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	public class GameManager : Component
	{
		private bool _gamestarted;
		public bool IsGameStarted
		{
			get => _gamestarted;
			set
			{
				if (!_gamestarted && value)
				{
					_gamestarted = true;
					OnGameStarted();
				}

			}
		}
		public int Health;
		public int MaxHealth=1000;
		UI_ProgressBar HealthBar;
		private void OnGameStarted()
		{
			Health = MaxHealth;
			CreateHealthBar();
		}

		private void CreateHealthBar()
		{
			GameObject barobject = MyScene.CreateSceneObject("HEALTH BAR");
			var bar = barobject.AddComponent<UI_ProgressBar>();
			bar.Color = new(0, 0.5f, 0, 0.5f);
			bar.transform.Position = new(0f, 15f, 0f);
			bar.BarColor = new(0, 1f, 0, 0.5f);
			bar.BarWidth = 200f;
			bar.BarHeight = 20f;
			bar.UI_Alignment = UI_Renderer.Alignment.CenterUp;
			HealthBar = bar;
		}
		public override void Update()
		{
			base.Update();
			if(HealthBar!=null)
			{
				HealthBar.Value = Health / (float)MaxHealth;
			}
		}

		public Enemy SpawnEnemy()
		{
			var enemy = SceneManager.ActiveScene.CreateSceneObject("ENEMY");
			enemy.transform.Position = new(4.8f, 0.12f, 3.4f);
			//enemy.transform.Scale = 0.001f * Vector3.One;
			var pathfind = enemy.AddComponent<PathFindingAgent>();
			var rend = enemy.AddComponent<ModelRenderer>();
			var Enemy = enemy.AddComponent<Enemy>();
			return Enemy;
		}

		public void EnemyReached(Enemy enemy)
		{
			enemy.gameObject.Destroy();
			Health -= 30;
		}
		public override void Start()
		{
			base.Start();
			IsGameStarted = true;
		}
	}
}
