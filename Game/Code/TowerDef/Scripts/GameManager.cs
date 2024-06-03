using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	public class GameManager : Component
	{
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
		}
	}
}
