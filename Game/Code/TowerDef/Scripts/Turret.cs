using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using PGK2.Engine.Core.Physics;
using PGK2.TowerDef.Scripts;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	/**
     * @class Turret
     * @brief Reprezentuje wieżyczkę w grze Tower Defense.
     */
	public class Turret : Component
	{
		/// <summary>
		/// Szybkość strzelania wieżyczki.
		/// </summary>
		public float ShootingSpeed;

		/// <summary>
		/// Obrażenia zadawane przez wieżyczkę.
		/// </summary>
		public int Damage;

		/// <summary>
		/// Zasięg ataku wieżyczki.
		/// </summary>
		public float Range;

		/// <summary>
		/// Obliczony zasięg ataku wieżyczki na podstawie poziomu.
		/// </summary>
		public float LevelRange => Range + (Level * 0.1f);

		/// <summary>
		/// Poziom wieżyczki.
		/// </summary>
		public int Level;

		/// <summary>
		/// Renderer modelu wieżyczki.
		/// </summary>
		public ModelRenderer? mymodel;

		/// <summary>
		/// Referencja do menedżera gry.
		/// </summary>
		GameManager? gameManager;

		/// <summary>
		/// Określa, czy wieżyczka jest wieżyczką zasięgową.
		/// </summary>
		public bool IsRangeTurret;

		/// <summary>
		/// Czas do kolejnego strzału.
		/// </summary>
		float shootCooldown;

		/// <summary>
		/// Bieżący cel wieżyczki.
		/// </summary>
		public Enemy? CurrentTarget;

		/// <summary>
		/// Początkowa skala transformacji wieżyczki.
		/// </summary>
		Vector3 startScale;

		/// <summary>
		/// Metoda wywoływana podczas aktywacji komponentu.
		/// </summary>
		public override void Awake()
		{
			base.Awake();
			gameManager = MyScene.FindObjectOfType<GameManager>();
		}

		/// <summary>
		/// Metoda wywoływana podczas startu gry.
		/// </summary>
		public override void Start()
		{
			base.Start();
			startScale = transform.LocalScale;
			shootCooldown = ShootingSpeed;
		}

		/// <summary>
		/// Metoda wywoływana co klatkę do aktualizacji turreta.
		/// </summary>
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
		/// <summary>
		/// Metoda wywoływana podczas zniszczenia wieżyczki.
		/// </summary>
		public override void OnDestroy()
		{
			base.OnDestroy();
			TurretManager.instance.PlacedTurrets.Remove(transform.Position);
		}
		/// <summary>
		/// Atak w jednego przeciwnika.
		/// </summary>
		private void TargetAttack()
		{
			if (CurrentTarget == null) return;
			CurrentTarget.Damage(Damage + (5 * Level));
		}
		/// <summary>
		/// Atak obszarowy.
		/// </summary>
		private void RangeAttack(Enemy[] enemies)
		{
			foreach(Enemy en in enemies)
			{
				en.Damage(Damage + (5*Level));
			}

		}
		/// <summary>
		/// Znajdź najbliższego wroga w zasięgu.
		/// </summary>
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
		/// <summary>
		/// Znajdź wszystkich wrogów w zasięgu.
		/// </summary>
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
	