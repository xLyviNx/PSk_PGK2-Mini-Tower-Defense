/**
 * @file GameManager.cs
 * @brief Plik zawiera definicje klasy GameManager.
 */
using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Components.Base.Renderers;
using PGK2.Engine.Core;
using PGK2.Engine.Core.Physics;
using PGK2.Engine.SceneSystem;
using PGK2.TowerDef.Scripts;
using System.Text.Json.Serialization;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	/**
     * @class GameManager
     * @brief Zarzadza stanem gry, falami i elementami interfejsu uzytkownika.
     */
	public class GameManager : Component
	{
		/// <summary>
		/// Czas, który upłynął od rozpoczęcia fali.
		/// </summary>
		[JsonIgnore] public float TimePassed; // seconds
		/// <summary>
		/// Czas trwania aktualnej fali.
		/// </summary>
		[JsonIgnore] public float WaveTime;

		/// <summary>
		/// Pozostały czas aktualnej fali.
		/// </summary>
		[JsonIgnore] public float WaveTimeLeft => MathHelper.Clamp(WaveTime - TimePassed, 0, float.MaxValue);

		/// <summary>
		/// Flaga określająca, czy aktualna fala zakończyła się.
		/// </summary>
		bool WaveEnded = false;

		/// <summary>
		/// Numer aktualnej fali.
		/// </summary>
		int wave = 0;

		/// <summary>
		/// Lista przeciwników, którzy zostali zespawnowani.
		/// </summary>
		[JsonIgnore] public List<Enemy> SpawnedEnemies = new();

		/// <summary>
		/// Licznik zespawnowanych przeciwników.
		/// </summary>
		[JsonIgnore]
		int everenemies = 0;

		/// <summary>
		/// Czas gry w postaci łańcucha znaków w formacie H:MM:SS.
		/// </summary>
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

		/// <summary>
		/// Flaga określająca, czy gra została rozpoczęta.
		/// </summary>
		private bool _gamestarted;

		/// <summary>
		/// Właściwość określająca, czy gra została rozpoczęta.
		/// </summary>
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

		/// <summary>
		/// Aktualna ilość zdrowia gracza.
		/// </summary>
		[JsonIgnore] public int Health;

		/// <summary>
		/// Maksymalna ilość zdrowia gracza.
		/// </summary>
		public int MaxHealth = 1000;

		/// <summary>
		/// Pasek zdrowia gracza w interfejsie użytkownika.
		/// </summary>
		UI_ProgressBar? HealthBar;

		/// <summary>
		/// Tekst z czasem pozostałym do końca fali.
		/// </summary>
		UI_Text? TimeText;

		/// <summary>
		/// Tekst z informacją o aktualnej fali.
		/// </summary>
		UI_Text? WaveText;

		/// <summary>
		/// Tekst z aktualną ilością pieniędzy gracza.
		/// </summary>
		UI_Text? MoneyText;

		/// <summary>
		/// Kolejka przeciwników do zespawnowania w aktualnej fali.
		/// </summary>
		List<(float, int, float)> EnemiesQueue = new();

		/// <summary>
		/// Aktualny obiekt wskazywany przez kursor myszy.
		/// </summary>
		[JsonIgnore] public GameObject? CurrentMouseTarget;

		/// <summary>
		/// Tekst z informacją o zdrowiu aktualnie wybranego przeciwnika.
		/// </summary>
		[JsonIgnore] UI_Text? EnemyHpDisplayText;

		/// <summary>
		/// Aktualnie wybrany przeciwnik, na którym znajduje się kursor myszy.
		/// </summary>
		[JsonIgnore] public Enemy? hoveredEnemy;

		/// <summary>
		/// Aktualna ilość pieniędzy gracza.
		/// </summary>
		[JsonIgnore] public int Money = 0;

		/// <summary>
		/// Timer określający czas, przez który gracz widzi informację o otrzymanym zadaniu obrażeniu.
		/// </summary>
		[JsonIgnore] public float TakenDamageTimer;

		/// <summary>
		/// Czas, który musi upłynąć przed rozpoczęciem pierwszej fali.
		/// </summary>
		private static readonly int TimeBeforeFirstWave = 30;

		/// <summary>
		/// Panel menu pauzy.
		/// </summary>
		UI_Panel pauseMenuPanel;

		/// <summary>
		/// Metoda wywoływana po rozpoczęciu gry.
		/// </summary>
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
		/// <summary>
		/// Tworzy kolejkę przeciwników na podstawie numeru fali.
		/// </summary>
		void CreateWaveQueue()
		{
			EnemiesQueue.Clear();
			switch (wave)
			{
				case 1:
					WaveTime = 8;
					EnemiesQueue.Add((1f, 70, 0.9f));
					EnemiesQueue.Add((8f, 70, 0.9f));
					break;
				case 2:
					WaveTime = 40f;
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
				case 4:
					WaveTime = 70;
					EnemiesQueue.Add((1f, 130, 1f));
					EnemiesQueue.Add((2f, 130, 1f));
					EnemiesQueue.Add((3f, 130, 1f));

					EnemiesQueue.Add((5, 500, 0.7f));

					EnemiesQueue.Add((8f, 150, 0.9f));
					EnemiesQueue.Add((9f, 150, 0.9f));
					EnemiesQueue.Add((10f, 150, 0.9f));

					EnemiesQueue.Add((14f, 50, 4));
					EnemiesQueue.Add((14.5f, 50, 4));
					EnemiesQueue.Add((15, 50, 4));

					EnemiesQueue.Add((20f, 100, 2));
					EnemiesQueue.Add((21f, 100, 2));
					EnemiesQueue.Add((22f, 100, 2));
					EnemiesQueue.Add((23f, 100, 2));
					EnemiesQueue.Add((24f, 100, 2));
					EnemiesQueue.Add((25f, 100, 2));

					EnemiesQueue.Add((30f, 100, 1.8f));
					EnemiesQueue.Add((32f, 120, 1.8f));
					EnemiesQueue.Add((34f, 120, 1.8f));
					EnemiesQueue.Add((36f, 120, 1.8f));
					EnemiesQueue.Add((38f, 120, 1.8f));
					EnemiesQueue.Add((40f, 120, 1.8f));

					for (int i = 42; i < 68; i++)
						EnemiesQueue.Add((i, 130, 1.5f));

					EnemiesQueue.Add((69, 800, 1f));
					break;
				case 5:
					WaveTime = 25;
					EnemiesQueue.Add((1f, 300, 1.5f));
					EnemiesQueue.Add((3f, 300, 1.5f));
					EnemiesQueue.Add((5f, 300, 1.5f));
					EnemiesQueue.Add((10f, 350, 1.6f));
					EnemiesQueue.Add((12f, 360, 1.7f));
					EnemiesQueue.Add((15f, 370, 1.8f));
					EnemiesQueue.Add((20f, 400, 2f));
					EnemiesQueue.Add((21f, 400, 2f));
					EnemiesQueue.Add((22f, 400, 2f));
					EnemiesQueue.Add((23f, 400, 2f));

					EnemiesQueue.Add((25f, 1000, 1f));

					break;
				default:
					return;
			}
		}
		/// <summary>
		/// Logika przebiegu fali.
		/// </summary>
		void WaveLogic()
		{
			if (!_gamestarted) return;
			if (wave == 0 && !WaveEnded) return;
			TimePassed += Time.deltaTime;
			if (EnemiesQueue.Count > 0)
			{
				if (TimePassed >= EnemiesQueue[0].Item1)
				{
					SpawnEnemy(EnemiesQueue[0].Item2, EnemiesQueue[0].Item3);
					EnemiesQueue.RemoveAt(0);
				}
			}
			else
			{
				if(!WaveEnded && WaveTimeLeft==0 && SpawnedEnemies.Count==0)
				{
					if (wave < 5)
					{
						Console.WriteLine("Wave Ended");
						TimePassed = 0;
						WaveEnded = true;
						WaveTime = 20;
					}
					else
					{
						Win();
					}
				}
				else if(WaveTimeLeft==0 && WaveEnded)
				{
					NextWave();
				}
			}
		}
		/// <summary>
		/// Przechodzi do następnej fali gry.
		/// </summary>
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
			Console.WriteLine($"STARTING WAVE {wave}");
		}
		/// <summary>
		/// Tworzy pasek zdrowia.
		/// </summary>
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
		/// <summary>
		/// Tworzy tekst timera.
		/// </summary>
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
		/// <summary>
		/// Tworzy tekst informujący o fali.
		/// </summary>
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
		/// <summary>
		/// Tworzy tekst informujący o ilości pieniędzy.
		/// </summary>
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
		/// <summary>
		/// Tworzy tekst informujący o zdrowiu przeciwnika.
		/// </summary>
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
		/// <summary>
		/// Metoda wywoływana przy starcie obiektu.
		/// </summary>
		public override void Awake()
		{
			base.Awake();
			CreatePauseMenu(MyScene);
		}
		/// <summary>
		/// Metoda aktualizacji logiki gry.
		/// </summary>
		public override void Update()
		{
			base.Update();
			RaycastTargetLogic();
			WaveLogic();
			if(EngineWindow.instance.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
			{
				pauseMenuPanel.gameObject.IsActiveSelf = !pauseMenuPanel.gameObject.IsActiveSelf;
				if (pauseMenuPanel.gameObject.IsActiveSelf)
					Time.timeScale = 0;
				else
					Time.timeScale = 1;
			}
			CameraController.instance.blockMovement = pauseMenuPanel.gameObject.IsActiveSelf;
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
		/// <summary>
		/// Logika odświeżania celu raycastowania.
		/// </summary>
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
		/// <summary>
		/// Tworzy przeciwnika.
		/// </summary>
		/// <param name="hp">Punkty życia przeciwnika.</param>
		/// <param name="speed">Prędkość przeciwnika.</param>
		/// <returns>Stworzony przeciwnik.</returns>
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
			Enemy.gameObject.name = $"ENEMY {everenemies}";
			everenemies++;
			return Enemy;
		}
		/// <summary>
		/// Obsługuje sytuację, gdy przeciwnik dotrze do końca trasy.
		/// </summary>
		/// <param name="enemy">Przeciwnik, który dotarł do końca trasy.</param>
		public void EnemyReached(Enemy enemy)
		{
			enemy.gameObject.Destroy();
			Health -= 75;
			TakenDamageTimer = 0.15f;
			if (Health<=0)
			{
				GameOver();
			}
		}
		/// <summary>
		/// Obsługuje sytuację, gdy gracz zabije przeciwnika.
		/// </summary>
		/// <param name="enemy">Przeciwnik, który został zabity.</param>
		public void KilledEnemy(Enemy enemy)
		{
			Money += (int)(3 * (MathHelper.Clamp(50f - enemy.TimeLived, 1f, 50f)));
			enemy.gameObject.Destroy();
		}
		/// <summary>
		/// Metoda wywoływana po zakończeniu gry.
		/// </summary>
		void GameOver()
		{
			var GameOver = SceneManager.LoadSceneFromFile(Menu.GameOverScene);
			SceneManager.ChangeSceneAsync(GameOver);
		}
		/// <summary>
		/// Metoda wywoływana w przypadku zwycięstwa gracza.
		/// </summary>
		void Win()
		{
			var scene = SceneManager.LoadSceneFromFile(Menu.WinScene);
			SceneManager.ChangeSceneAsync(scene);
		}
		/// <summary>
		/// Metoda wywoływana przy starcie obiektu.
		/// </summary>
		public override void Start()
		{
			base.Start();
			IsGameStarted = true;
		}
		/// <summary>
		/// Tworzy menu pauzy.
		/// </summary>
		/// <param name="scene">Scena, do której dodawane jest menu pauzy.</param>
		void CreatePauseMenu(Scene scene)
		{
			pauseMenuPanel = scene.CreateSceneObject("PauseMenuPanel").AddComponent<UI_Panel>();
			pauseMenuPanel.UI_Alignment = UI_Renderer.Alignment.Center;
			pauseMenuPanel.Size = new (300, 200);
			pauseMenuPanel.Color = new (0, 0, 0, 0.5f);
			pauseMenuPanel.transform.Position = new Vector3(0, 0, 0);
			pauseMenuPanel.Pivot = new (0.5f, 0.5f);
			pauseMenuPanel.Z_Index = 100; // Ensure it's on top of other UI elements

			var exitButton = scene.CreateSceneObject("ExitButton").AddComponent<UI_Button>();
			exitButton.UI_Alignment = UI_Renderer.Alignment.Center;
			exitButton.Text = "Exit to Menu";
			exitButton.FontSize = 1.5f;
			exitButton.Padding = new (20, 10);
			exitButton.Pivot = new (0.5f, 0.5f);
			exitButton.transform.Parent = pauseMenuPanel.transform;
			exitButton.transform.Position = new Vector3(0, -50, 0);
			exitButton.OnClick += () =>
			{
				var scene = SceneManager.LoadSceneFromFile($"{EngineInstance.ASSETS_PATH}/Scenes/MENU.lscn");
				SceneManager.ChangeSceneAsync(scene);
			};

			var resumeButton = scene.CreateSceneObject("ResumeButton").AddComponent<UI_Button>();
			resumeButton.UI_Alignment = UI_Renderer.Alignment.Center;
			resumeButton.Text = "Resume";
			resumeButton.FontSize = 1.5f;
			resumeButton.Padding = new (20, 10);
			resumeButton.Pivot = new (0.5f, 0.5f);
			resumeButton.transform.Parent = pauseMenuPanel.transform;
			resumeButton.transform.Position = new Vector3(0, 50, 0);
			resumeButton.OnClick += () =>
			{
				Time.timeScale = 1;
				pauseMenuPanel.gameObject.IsActiveSelf = (false);
			};

			pauseMenuPanel.gameObject.IsActiveSelf = (false);
		}
	}
}
