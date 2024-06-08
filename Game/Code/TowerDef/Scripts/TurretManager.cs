using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using PGK2.Engine.Core.Physics;
using PGK2.TowerDef.Scripts;
using System;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	public class TurretManager : Component
	{
		GameManager? gameManager;
		GameObject? MouseTarget;
		ModelRenderer? MouseTargetRenderer;
		GameObject TurretZone;
		Material gridMaterial;
		TagsContainer raycastTags;
		public override void Awake()
		{
			base.Awake();
			raycastTags = new TagsContainer();
			raycastTags.Add("TurretRegion");
			gameManager = MyScene.FindObjectOfType<GameManager>();
			MouseTarget = MyScene.CreateSceneObject();
			MouseTarget.transform.LocalScale = Vector3.One * 0.15f;
			MouseTargetRenderer = MouseTarget.AddComponent<ModelRenderer>();
			MouseTargetRenderer.Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/cube.fbx");
			MouseTargetRenderer.InstantiateAllMaterials();
			MouseTargetRenderer.OverrideMaterials[0].Vector3Values["material.diffuse"] = new(10, 10, 10);
			TurretZone = MyScene.FindObjectByName("TurretRegion");
			gridMaterial = new(EngineWindow.GridShader);
			gridMaterial.FloatValues["gridWidth"] = 0.03f;
			gridMaterial.FloatValues["gridSpacing"] = 0.4f;
			gridMaterial.Vector3Values["gridColor"] = new(0,1,1);
			//TurretZone.GetComponent<ModelRenderer>().InstantiateAllMaterials();
			TurretZone.GetComponent<ModelRenderer>().OverrideMaterials[0] = gridMaterial;

		}
		public override void Update()
		{
			base.Update();

			if ( !MouseLockController.isLocked && Physics.RayCast_Triangle(CameraComponent.activeCamera, Mouse.MousePosition, 200f, out var hit, raycastTags))
			{
				MouseTarget.IsActiveSelf = true;
				MouseTarget.transform.Position = SnapToGrid(hit.Point, gridMaterial.FloatValues["gridSpacing"]);
				//MouseTarget.transform.Position = hit.Point;
			}
			else
			{
				MouseTarget.IsActiveSelf = false;
			}
		}

		private Vector3 SnapToGrid(Vector3 position, float gridSpacing)
		{
			return new Vector3(
				MathF.Round(position.X / gridSpacing) * gridSpacing,
				position.Y+0.05f,
				MathF.Round(position.Z / gridSpacing) * gridSpacing
			);
		}
	}
}
