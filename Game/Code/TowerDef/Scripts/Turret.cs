using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using PGK2.Engine.Core.Physics;
using PGK2.TowerDef.Scripts;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	public class Turret : Component
	{
		public float ShootingSpeed;
		public int Damage;
		public float Range;
		ModelRenderer? mymodel;
		GameManager? gameManager;
		public bool IsRangeTurret;
		float shootCooldown;
		public Enemy? CurrentTarget;

		GameObject test;
		public override void Awake()
		{
			base.Awake();
			mymodel = GetComponent<ModelRenderer>();
			gameManager=MyScene.FindObjectOfType<GameManager>();
			test = MyScene.CreateSceneObject("Test");
			test.AddComponent<ModelRenderer>().Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/cube.fbx");

		}
		public override void Start()
		{
			base.Start();
			shootCooldown = ShootingSpeed;
		}
		public override void Update()
		{
			base.Update();
			var raycastTags = new TagsContainer();
			raycastTags.Add("map");
			if (!MouseLockController.isLocked && Physics.RayCast_Triangle(CameraComponent.activeCamera, Mouse.MousePosition, 200f, out var hit, raycastTags))
			{
				test.transform.Position = hit.Point;
				float dist = Vector3.Distance(transform.Position, hit.Point);
				if (dist < Range)
					test.transform.Scale = Vector3.One * 0.5f;
				else
					test.transform.Scale = Vector3.One * 0.2f;
			}

			if (gameManager == null)
				return;

			if (CurrentTarget!=null)
			{
				float dist = Vector3.Distance(transform.Position, CurrentTarget.transform.Position);
				if(CurrentTarget.gameObject.isDestroyed || dist>Range)
					CurrentTarget = null;


			}
			else 
			{
				if(!IsRangeTurret)
					CurrentTarget = FindClosestEnemyInRange();
			}

			if (shootCooldown > 0)
				shootCooldown -= Time.deltaTime;
			else
			{
				if (IsRangeTurret)
					RangeAttack();
				else
					TargetAttack();
				shootCooldown = ShootingSpeed;
			}
		}

		private void TargetAttack()
		{
			if (CurrentTarget == null) return;
			CurrentTarget.Damage(Damage);
		}

		private void RangeAttack()
		{
			foreach(Enemy en in FindEnemiesInRange())
			{
				en.Damage(Damage);
			}

		}
		private Enemy? FindClosestEnemyInRange()
		{
			if (gameManager == null) return null;
			Enemy? closest = null;
			float closestDist = float.MaxValue;
			foreach(var enemy in gameManager.SpawnedEnemies)
			{
				if (enemy == null) continue;
				float dist = Vector3.Distance(enemy.transform.Position, transform.Position);
				if (dist > Range) continue;
				if (dist<closestDist || closest==null || closest.gameObject.isDestroyed)
				{
					closestDist = dist;
					closest = enemy;
				}
			}
			return closest;
		}
		private Enemy[] FindEnemiesInRange()
		{
			List<Enemy> enemiesfound = new();
			if (gameManager != null)
			{
				foreach (var enemy in gameManager.SpawnedEnemies)
				{
					if (enemy == null || enemy.gameObject.isDestroyed) continue;
					float dist = Vector3.Distance(enemy.transform.Position, transform.Position);
					if (dist > Range) continue;
					enemiesfound.Add(enemy);
				}
			}
			return enemiesfound.ToArray();
		}
	}
}
	