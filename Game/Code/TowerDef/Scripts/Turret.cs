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
		public int Level;
		ModelRenderer? mymodel;
		GameManager? gameManager;
		public bool IsRangeTurret;
		float shootCooldown;
		public Enemy? CurrentTarget;
		public override void Awake()
		{
			base.Awake();
			mymodel = GetComponent<ModelRenderer>();
			gameManager=MyScene.FindObjectOfType<GameManager>();
		}
		public override void Start()
		{
			base.Start();
			shootCooldown = ShootingSpeed;
		}
		public override void Update()
		{
			base.Update();
			transform.LocalScale = Vector3.One*( 1f + (Level * 0.1f));
			if (CurrentTarget != null)
			{
				transform.LocalRotation = TransformComponent.LookAtRotation(transform.Position, CurrentTarget.transform.Position);
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
			{

				shootCooldown -= Time.deltaTime;
			}
			else
			{
				if (IsRangeTurret)
				{
					var enemies = FindEnemiesInRange();
					if (enemies.Length > 0)
					{
						RangeAttack(enemies);
						shootCooldown = ShootingSpeed;

					}
				}
				else if (CurrentTarget != null)
				{
					TargetAttack();
					shootCooldown = ShootingSpeed;
				}
			}
		}
		public override void OnDestroy()
		{
			base.OnDestroy();
			TurretManager.instance.PlacedTurrets.Remove(transform.Position);
		}

		private void TargetAttack()
		{
			if (CurrentTarget == null) return;
			CurrentTarget.Damage(Damage);
		}

		private void RangeAttack(Enemy[] enemies)
		{
			foreach(Enemy en in enemies)
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
	