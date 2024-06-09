/**
 * @file Enemy.cs
 * @brief Ten plik zawiera definicję klasy Enemy.
 */
using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Core;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	/**
     * @class Enemy
     * @brief Reprezentuje wroga w grze Tower Defense.
     */
	public class Enemy : Component
	{
		/// <summary>
		 /// Prędkość interpolacji dla obrotu wroga.
		 /// </summary>
		private float LerpSpeed = 5f;

		/// <summary>
		/// Agent znajdowania ścieżki dla wroga.
		/// </summary>
		private PathFindingAgent myAgent;

		/// <summary>
		/// Renderer modelu dla wroga.
		/// </summary>
		private ModelRenderer myModelRenderer;

		/// <summary>
		/// Nazwa modelu wroga.
		/// </summary>
		public string ModelName = "enemy1.fbx";

		/// <summary>
		/// Flaga wskazująca, czy wróg osiągnął swój cel.
		/// </summary>
		private bool hasreached = false;

		/// <summary>
		/// Punkty zdrowia wroga.
		/// </summary>
		public int Health = 100;

		/// <summary>
		/// Odwołanie do menedżera gry.
		/// </summary>
		private GameManager? gameManager = null;

		/// <summary>
		/// Flaga wskazująca, czy myszka jest nad wrogiem.
		/// </summary>
		public bool isHovered;

		/// <summary>
		/// Czas życia wroga.
		/// </summary>
		public float TimeLived;

		/// <summary>
		/// Flaga wskazująca, czy materiały (ich kopie) na wrogu zostały zainicjowane.
		/// </summary>
		private bool instantiatedmaterials = false;

		/// <summary>
		/// Flaga wskazująca, czy wróg otrzymał obrażenia.
		/// </summary>
		private bool Damaged;

		/// <summary>
		/// Obiekt hitbox dla wroga (do raycastow myszki).
		/// </summary>
		private GameObject hitboxObject;

		/// <summary>
		/// Token anulowania dla efektu obrażeń.
		/// </summary>
		private CancellationTokenSource damageEffectCancellationTokenSource;

		/// <summary>
		/// Wywoływane, gdy obiekt zostanie aktywowany.
		/// </summary>
		public override void Awake()
		{
			base.Awake();
			myAgent = GetComponent<PathFindingAgent>();
			myModelRenderer = GetComponent<ModelRenderer>();
			myModelRenderer.Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/" + ModelName);
			gameManager = MyScene.FindObjectOfType<GameManager>();
			hitboxObject = MyScene.CreateSceneObject("hitbox");
			hitboxObject.transform.Parent = transform;
			var hitboxrend = hitboxObject.AddComponent<ModelRenderer>();
			hitboxrend.Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/cube.fbx");
			hitboxrend.gameObject.Tags.Add("enemyhitbox");
			hitboxObject.transform.LocalPosition = new(0,0.4f,0);
			hitboxObject.transform.LocalRotation = Vector3.Zero;

		}
		/// <summary>
		/// Wywoływane po aktywacji obiektu.
		/// </summary>
		public override void Start()
		{
			base.Start();
			gameManager.SpawnedEnemies.Add(this);
			myAgent.SetTargetPosition(MyScene.FindObjectByName("ai_target").transform.Position);
		}
		/// <summary>
		/// Wywoływane co klatkę, aktualizuje zachowanie wroga.
		/// </summary>
		public override void Update()
		{
			base.Update();
			if (hasreached)
				return;
			TimeLived += Time.deltaTime;
			isHovered = gameManager.CurrentMouseTarget == hitboxObject;
			DamagedUpdate();
			if (isHovered)
			{
				transform.LocalScale = Vector3.One * 1.1f;
				gameManager.hoveredEnemy = this;
			}
			else
			{
				transform.LocalScale = Vector3.One;
				if (gameManager.hoveredEnemy == this)
					gameManager.hoveredEnemy = null;
			}
			if (myAgent.waypoint < myAgent.Path.Count)
			{
				transform.RotateTowards(myAgent.Path[myAgent.waypoint], LerpSpeed);
			}

			float dist = Vector3.Distance(myAgent.transform.Position, myAgent.TargetPosition);

			if (dist < 0.05f)
			{
				hasreached = true;
				Reached();
			}
		}
		/// <summary>
		/// Aktualizuje efekt obrażeń.
		/// </summary>
		/// <param name="lerp">Określa, czy użyć interpolacji.</param>
		private void DamagedUpdate(bool lerp = true)
		{
			if (!instantiatedmaterials) return;
			for(int i = 0; i<myModelRenderer.Model.meshes.Count; i++)
			{
				Mesh mesh = myModelRenderer.Model.meshes[i];
				int nummats = 1; //bo w aktualnej wersji nie ma wielu materialow na mesh
				for (int j = 0; j<nummats; j++)
				{
					Material defaultmat = myModelRenderer.Model.meshes[i].Material;
					Material mat = myModelRenderer.OverrideMaterials[i + j];
					Vector3 targetcolor = Damaged ? new(1, 0.0f, 0.0f) : defaultmat.Vector3Values["material.diffuse"];
					if(mat!=null)
					{
						if (lerp)
							mat.Vector3Values["material.diffuse"] = Vector3.Lerp(mat.Vector3Values["material.diffuse"], targetcolor, Time.deltaTime * 30f);
						else
							mat.Vector3Values["material.diffuse"] = mat.Vector3Values["material.diffuse"];
					}

				}
			}
		}
		/// <summary>
		/// Wywoływane przed zniszczeniem obiektu.
		/// </summary>
		public override void OnDestroy()
		{
			base.OnDestroy();
			gameManager.SpawnedEnemies.Remove(this);
			if (gameManager.hoveredEnemy == this)
				gameManager.hoveredEnemy = null;
		}
		/// <summary>
		/// Wywoływane po osiągnięciu celu przez wroga.
		/// </summary>
		private void Reached()
		{
			gameManager.EnemyReached(this);
		}
		/// <summary>
		/// Zadaje obrażenia wrogowi.
		/// </summary>
		/// <param name="dmg">Ilość zadanych obrażeń.</param>
		public void Damage(int dmg)
		{
			Health -= dmg;
			Console.WriteLine($"{gameObject.name} TAKEN {dmg} DMG");
			if(Health<=0)
			{
				gameManager.KilledEnemy(this);
			}
			else
			{
				DamageEffect();
			}
		}
		/// <summary>
		/// Inicjuje materiały w momencie zadania obrażeń jeśli jeszcze tego nie zrobiono.
		/// </summary>
		private void InstantiateMaterials()
		{
			if (instantiatedmaterials) return;
			instantiatedmaterials = true;

			myModelRenderer.InstantiateAllMaterials();
		}
		/// <summary>
		/// Wywołuje efekt obrażeń.
		/// </summary>
		private async void DamageEffect()
		{

			damageEffectCancellationTokenSource?.Cancel();
			Damaged = false;
			DamagedUpdate(false);
			damageEffectCancellationTokenSource = new CancellationTokenSource();
			CancellationToken token = damageEffectCancellationTokenSource.Token;

			InstantiateMaterials();
			Damaged = true;
			try
			{
				await Task.Delay(120, token);
				if (!gameObject.isDestroyed)
				{
					Damaged = false;
				}
			}
			catch (TaskCanceledException)
			{
				
			}
		}
	}
}
