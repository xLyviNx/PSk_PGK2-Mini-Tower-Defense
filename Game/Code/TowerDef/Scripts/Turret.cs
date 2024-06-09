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
		public float LevelRange => Range + (Level * 0.1f);
		public int Level;
		public ModelRenderer? mymodel;
		GameManager? gameManager;
		public bool IsRangeTurret;
		float shootCooldown;
		public Enemy? CurrentTarget;
		Vector3 startScale;
		public override void Awake()
		{
			base.Awake();
			gameManager=MyScene.FindObjectOfType<GameManager>();
		}
		public override void Start()
		{
			base.Start();
			 startScale = transform.LocalScale;
			shootCooldown = ShootingSpeed;
		}
		public override void Update()
		{
			base.Update();
			float LevelMod = ((Level - 1) * 0.1f);
			transform.LocalScale = startScale + Vector3.One*LevelMod;
			if (CurrentTarget != null)
			{
				transform.LocalRotation = TransformComponent.LookAtRotation(transform.Position, CurrentTarget.transform.Position);
			}

			if (gameManager == null)
				return;

			if (CurrentTarget!=null)
			{
				float dist = Vector3.Distance(transform.Position, CurrentTarget.transform.Position);
				if(CurrentTarget.gameObject.isDestroyed || dist>LevelRange)
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
			CurrentTarget.Damage(Damage + (5 * Level));
		}

		private void RangeAttack(Enemy[] enemies)
		{
			foreach(Enemy en in enemies)
			{
				en.Damage(Damage + (5*Level));
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
				if (dist > LevelRange) continue;
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
					if (dist > LevelRange) continue;
					enemiesfound.Add(enemy);
				}
			}
			return enemiesfound.ToArray();
		}
	}
}
	