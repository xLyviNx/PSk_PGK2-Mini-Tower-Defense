using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	public class Enemy : Component
	{
		float LerpSpeed = 5f;
		private PathFindingAgent myAgent;
		private ModelRenderer myModelRenderer;
		public string ModelName = "enemy1.fbx";
		bool hasreached = false;
		public int Health = 100;
		GameManager? gameManager = null;
		public bool isHovered;
		public float TimeLived;
		bool instantiatedmaterials = false;
		bool Damaged;
		GameObject hitboxObject;
		private CancellationTokenSource damageEffectCancellationTokenSource;
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
		public override void Start()
		{
			base.Start();
			gameManager.SpawnedEnemies.Add(this);
			myAgent.SetTargetPosition(MyScene.FindObjectByName("ai_target").transform.Position);
		}
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
			//Console.WriteLine(dist);

			var kb = EngineWindow.instance.KeyboardState;
			if(kb.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.L))
			{
				Console.WriteLine("DAMAGE");
				Damage(0);
			}

			if (dist < 0.05f)
			{
				hasreached = true;
				Reached();
			}
		}

		private void DamagedUpdate(bool lerp = true)
		{
			if (!instantiatedmaterials) return;
			for(int i = 0; i<myModelRenderer.Model.meshes.Count; i++)
			{
				Mesh mesh = myModelRenderer.Model.meshes[i];
				int nummats = 1; //bo w aktualnej wersji nie ma wielu materialow na mesh
				for (int j = 0; j<nummats; j++)
				{
					//Console.WriteLine("DAMAGED UPDATE 3");
					Material defaultmat = myModelRenderer.Model.meshes[i].Material;
					Material mat = myModelRenderer.OverrideMaterials[i + j];
					Vector3 targetcolor = Damaged ? new(1, 0.0f, 0.0f) : defaultmat.Vector3Values["material.diffuse"];
					if(mat!=null)
					{
						if (lerp)
							mat.Vector3Values["material.diffuse"] = Vector3.Lerp(mat.Vector3Values["material.diffuse"], targetcolor, Time.deltaTime * 20f);
						else
							mat.Vector3Values["material.diffuse"] = mat.Vector3Values["material.diffuse"];
					}

				}
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			gameManager.SpawnedEnemies.Remove(this);
		}
		private void Reached()
		{
			gameManager.EnemyReached(this);
		}
		public void Damage(int dmg)
		{
			DamageEffect();
		}
		private void InstantiateMaterials()
		{
			if (instantiatedmaterials) return;
			instantiatedmaterials = true;

			myModelRenderer.InstantiateAllMaterials();
		}
		private async void DamageEffect()
		{

			damageEffectCancellationTokenSource?.Cancel();
			Damaged = false;
			DamagedUpdate(false);
			damageEffectCancellationTokenSource = new CancellationTokenSource();
			CancellationToken token = damageEffectCancellationTokenSource.Token;

			InstantiateMaterials();
			Damaged = true;
			Console.WriteLine("SETTING DAMAGED TO TRUE");
			try
			{
				await Task.Delay(200, token);
				if (!gameObject.isDestroyed)
				{
					Console.WriteLine("SETTING DAMAGED TO FALSE");
					Damaged = false;
				}
			}
			catch (TaskCanceledException)
			{
				
			}
		}
	}
}
