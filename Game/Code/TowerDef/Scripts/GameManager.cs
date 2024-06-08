using Assimp;
using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Components.Base.Renderers;
using PGK2.Engine.Core;
using PGK2.Engine.Core.Physics;
using PGK2.Engine.SceneSystem;
using System.Text.Json.Serialization;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	public class GameManager : Component
	{
		[JsonIgnore] public float TimePassed; // seconds
		[JsonIgnore] public float WaveTime;
		[JsonIgnore] public float WaveTimeLeft => MathHelper.Clamp(WaveTime - TimePassed, 0, float.MaxValue);
		bool WaveEnded = false;
		int wave = 0;
		[JsonIgnore] public List<Enemy> SpawnedEnemies = new();
		[JsonIgnore]
		public string TimeString // H:MM:SS
		{
			get
			{
				TimeSpan timeSpan = TimeSpan.FromSeconds(WaveTimeLeft);

				if (timeSpan.TotalHours >= 1)
				{
					return string.Format("{0:D}:{1:D2}:{2:D2}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
				}
				else 
				{
					return string.Format("{0:D}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
				}
			
			}
		}
		private bool _gamestarted;
		[JsonIgnore]
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
		[JsonIgnore] public int Health;
		public int MaxHealth=1000;
		UI_ProgressBar? HealthBar;
		UI_Text? TimeText;
		UI_Text? WaveText;
		UI_Text? MoneyText;

		List<(float, int, float)> EnemiesQueue = new();
		[JsonIgnore] public GameObject? CurrentMouseTarget;
		[JsonIgnore] UI_Text? EnemyHpDisplayText;
		[JsonIgnore] public Enemy? hoveredEnemy;
		[JsonIgnore] public int Money = 0;
		[JsonIgnore] public float TakenDamageTimer;

		private static readonly int TimeBeforeFirstWave = 5;
		private void OnGameStarted()
		{
			Health = MaxHealth;
			CreateHealthBar();
			CreateTimerText();
			CreateWaveText();
			CreateEnemyHpText();
			CreateMoneyText();
			
			Money = 2000;
			WaveTime = TimeBeforeFirstWave;
			WaveEnded = true;
		}
		void CreateWaveQueue()
		{
			EnemiesQueue.Clear();
			switch (wave)
			{
				case 1:
					WaveTime = 25f;
					EnemiesQueue.Add((1f, 70, 0.9f));
					EnemiesQueue.Add((8f, 70, 0.9f));
					break;
				case 2:
					WaveTime = 50f;
					EnemiesQueue.Add((0, 100, 1f));
					EnemiesQueue.Add((5, 100, 1f));
					EnemiesQueue.Add((10, 100, 1f));
					EnemiesQueue.Add((13, 100, 1f));
					EnemiesQueue.Add((15f, 100, 1f));
					EnemiesQueue.Add((20f, 100, 1f));
					EnemiesQueue.Add((25f, 100, 1f));
					EnemiesQueue.Add((30f, 100, 1f));
					EnemiesQueue.Add((32f, 100, 1f));
					EnemiesQueue.Add((34f, 100, 1f));
					EnemiesQueue.Add((35f, 100, 1f));
					EnemiesQueue.Add((36f, 100, 1f));
					EnemiesQueue.Add((37f, 100, 1f));
					EnemiesQueue.Add((40f, 100, 1f));
					break;
				case 3:
					WaveTime = 60f;
					EnemiesQueue.Add((1f, 100, 1f));
					EnemiesQueue.Add((2f, 100, 1f));
					EnemiesQueue.Add((3f, 100, 1f));
					EnemiesQueue.Add((4f, 110, 1f));
					EnemiesQueue.Add((5f, 110, 1f));
					EnemiesQueue.Add((10f, 60, 2f));
					EnemiesQueue.Add((13f, 60, 2f));
					EnemiesQueue.Add((15f, 60, 2f));
					EnemiesQueue.Add((21, 200, 0.8f));
					EnemiesQueue.Add((23, 200, 0.8f));
					EnemiesQueue.Add((25, 200, 0.8f));
					EnemiesQueue.Add((30, 200, 0.8f));
					EnemiesQueue.Add((35, 250, 0.6f));
					EnemiesQueue.Add((35, 250, 0.6f));
					EnemiesQueue.Add((40, 100, 1.0f));
					EnemiesQueue.Add((43, 100, 1.0f));
					EnemiesQueue.Add((45, 100, 1.0f));
					EnemiesQueue.Add((50, 100, 1.0f));
					EnemiesQueue.Add((51, 100, 1.0f));
					EnemiesQueue.Add((52, 100, 1.0f));
					EnemiesQueue.Add((53, 100, 1.0f));
					EnemiesQueue.Add((54, 100, 1.0f));
					EnemiesQueue.Add((55, 100, 1.0f));
					EnemiesQueue.Add((60, 500, 0.3f));
					break;
				default:
					return;
			}
		}
		void WaveLogic()
		{
			if (!_gamestarted) return;
			if (wave == 0 && !WaveEnded) return;
			TimePassed += Time.deltaTime;
			if (EnemiesQueue.Count > 0)
			{
				if (TimePassed >= EnemiesQueue[0].Item1)
				{
					Console.WriteLine($"Spawning Enemy of Time {EnemiesQueue[0].Item1}");
					SpawnEnemy(EnemiesQueue[0].Item2, EnemiesQueue[0].Item3);
					EnemiesQueue.RemoveAt(0);
				}
			}
			else
			{
				if(!WaveEnded && WaveTimeLeft==0 && SpawnedEnemies.Count==0)
				{
					Console.WriteLine("Wave Ended");
					TimePassed = 0;
					WaveEnded = true;
					WaveTime = 20;
				}
				else if(WaveTimeLeft==0 && WaveEnded)
				{
					NextWave();
				}
			}
		}
		void NextWave()
		{
			WaveEnded = false;
			TimePassed = 0;
			if(wave>=3)
			{
				wave = 0;
				IsGameStarted = false;
				return;
			}
			wave++;
			CreateWaveQueue();
			Console.WriteLine($"QUEUE CREATED WITH SIZE: {EnemiesQueue.Count}");
			Console.WriteLine($"STARTING WAVE {wave}");
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
			bar.Pivot = new(0.5f, 0.5f);
			HealthBar = bar;
		}
		private void CreateTimerText()
		{

			GameObject timerText = MyScene.CreateSceneObject("TIMER");
			var text = timerText.AddComponent<UI_Text>();
			text.Color = new(1, 1, 1, 1);
			text.transform.Position = new(0f, 30f, 0f);
			text.FontSize = 2;
			text.UI_Alignment = UI_Renderer.Alignment.CenterUp;
			text.Pivot = new(0.5f, 0);
			TimeText = text;
		}	
		private void CreateWaveText()
		{
			GameObject timerText = MyScene.CreateSceneObject("WAVE TEXT");
			var text = timerText.AddComponent<UI_Text>();
			text.Color = new(1, 1, 1, 1);
			text.transform.Position = new(0f, 60f, 0f);
			text.FontSize = 1.5f;
			text.UI_Alignment = UI_Renderer.Alignment.CenterUp;
			text.Pivot = new(0.5f, 0);
			WaveText = text;
		}
		private void CreateMoneyText()
		{
			GameObject texto = MyScene.CreateSceneObject("MONEY TEXT");
			var text = texto.AddComponent<UI_Text>();
			text.Color = new(1, 1, 1, 1);
			text.transform.Position = new(-45f, 15f, 0f);
			text.FontSize = 2f;
			text.UI_Alignment = UI_Renderer.Alignment.RightUp;
			text.Pivot = new(1f, 0);
			MoneyText = text;
		}
		private void CreateEnemyHpText()
		{
			GameObject texto = MyScene.CreateSceneObject("ENEMY HP TEXT");
			var text = texto.AddComponent<UI_Text>();
			text.Color = new(1, 1, 1, 1);
			text.transform.Position = new(0f, -30f, 0f);
			text.FontSize = 1;
			text.UI_Alignment = UI_Renderer.Alignment.DownCenter;
			text.Pivot = new(0.5f, 1);
			EnemyHpDisplayText = text;
		}
		public override void Update()
		{
			base.Update();
			RaycastTargetLogic();
			WaveLogic();

			if (TakenDamageTimer > 0f)
				TakenDamageTimer -= Time.deltaTime;

			if (MoneyText != null)
				MoneyText.Text = $"$ {Money}";
			if (HealthBar != null)
			{
				HealthBar.Value = Health / (float)MaxHealth;
			}
			if (TimeText != null)
			{

				TimeText.Text = $"{TimeString}";
			}
			if (WaveText != null)
			{
				if (WaveEnded)
					WaveText.Text = $"WAVE COMING";
				else
					WaveText.Text = $"WAVE {wave}";
			}

			if(EnemyHpDisplayText != null)
			{
				if (hoveredEnemy!=null)
				{
					EnemyHpDisplayText.Text = $"HP: {hoveredEnemy.Health}";
				}
				else if (!WaveEnded && _gamestarted)
				{
					EnemyHpDisplayText.Text = $"HOVER ON ENEMY TO SEE THEIR HEALTH.";
				}
				else
				{
					EnemyHpDisplayText.Text = $"";
				}
			}
		}

		private void RaycastTargetLogic()
		{
			var mousePosition = Mouse.MousePosition;
			TagsContainer tc = new();
			tc.Add("enemyhitbox");
			if (Physics.RayCast_Triangle(CameraComponent.activeCamera, mousePosition, 1000f, out RayCastHit hitInfo, tc))
			{
				CurrentMouseTarget = hitInfo.gameObject;
			}else
				CurrentMouseTarget = null;
		}

		public Enemy SpawnEnemy(int hp, float speed)
		{
			var enemy = SceneManager.ActiveScene.CreateSceneObject("ENEMY");
			enemy.transform.Position = new(4.8f, 0.12f, 3.4f);
			//enemy.transform.Scale = 0.001f * Vector3.One;
			var pathfind = enemy.AddComponent<PathFindingAgent>();
			pathfind.Speed = speed;
			var rend = enemy.AddComponent<ModelRenderer>();
			var Enemy = enemy.AddComponent<Enemy>();
			Enemy.Health = hp;
			return Enemy;
		}

		public void EnemyReached(Enemy enemy)
		{
			enemy.gameObject.Destroy();
			Health -= 30;
			TakenDamageTimer = 0.15f;
			if (Health<=0)
			{
				GameOver();
			}
		}
		public void KilledEnemy(Enemy enemy)
		{
			Money += (int)(500 * (MathHelper.Clamp(50f - enemy.TimeLived, 1f, 50f)));
		}
		void GameOver()
		{
			IsGameStarted = false;
		}
		public override void Start()
		{
			base.Start();
			IsGameStarted = true;
		}
	}
}
