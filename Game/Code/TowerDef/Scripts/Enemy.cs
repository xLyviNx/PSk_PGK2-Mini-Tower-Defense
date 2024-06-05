using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
		public override void Awake()
		{
			base.Awake();
			myAgent = GetComponent<PathFindingAgent>();
			myModelRenderer = GetComponent<ModelRenderer>();
			myModelRenderer.Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/" + ModelName);
			gameManager = MyScene.FindObjectOfType<GameManager>();

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
			isHovered = gameManager.CurrentMouseTarget == gameObject;
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
				var targetrot = TransformComponent.LookAtRotation(transform.Position, myAgent.Path[myAgent.waypoint]);
				transform.LocalRotation = Vector3.Lerp(transform.LocalRotation, targetrot, Time.deltaTime*LerpSpeed);
			}

			float dist = Vector3.Distance(myAgent.transform.Position, myAgent.TargetPosition);
			//Console.WriteLine(dist);
			if (dist < 0.05f)
			{
				hasreached = true;
				Reached();
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
	}
}
