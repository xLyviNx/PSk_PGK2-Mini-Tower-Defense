using Assimp.Configs;
using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using PGK2.Engine.Core.Physics;
using PGK2.TowerDef.Scripts;
using System;
using System.Text.Json.Serialization;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	public class TurretManager : Component
	{
		GameManager? gameManager;
		GameObject? MouseTarget;
		ModelRenderer? MouseTargetRenderer;
		GameObject TurretZone;
		Material PickerMaterial;
		Material gridMaterial;
		Material ShowRangeMaterial;
		TagsContainer raycastTags;
		[JsonIgnore] public bool IsPlacingTurret = false;
		Dictionary<Vector3, Turret> PlacedTurrets = new();
		GameObject TurretMenuPanel;
		UI_Button BuildButton;
		UI_Button Turret1Button;
		UI_Button Turret2Button;
		UI_Button Turret3Button;
		Model Cube;
		Model Sphere;
		Model Turret1Model;
		Model Turret2Model;
		Model Turret3Model;
		int ChosenTurret;
		public static int[] TurretPrices = { 0, 1000, 500, 800 };
		GameObject RangeDisplayObject;
		public override void Awake()
		{
			base.Awake();
			Sphere = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/sphere.fbx");
			raycastTags = new TagsContainer();
			raycastTags.Add("TurretRegion");
			gameManager = MyScene.FindObjectOfType<GameManager>();
			MouseTarget = MyScene.CreateSceneObject();
			MouseTargetRenderer = MouseTarget.AddComponent<ModelRenderer>();
			Cube = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/cube.fbx");
			MouseTargetRenderer.Model = Cube;
			PickerMaterial = new(EngineWindow.lightShader);
			SetMatColor(new(1, 1, 1));
			TurretZone = MyScene.FindObjectByName("TurretRegion");
			gridMaterial = new(EngineWindow.GridShader);
			gridMaterial.FloatValues["gridWidth"] = 0.03f;
			gridMaterial.FloatValues["gridSpacing"] = 0.5f;
			gridMaterial.Vector3Values["gridColor"] = new(0,1,1);
			TurretZone.GetComponent<ModelRenderer>().OverrideMaterials[0] = gridMaterial;

			TurretMenuPanel = MyScene.FindObjectByName("Turret Main Panel");
			BuildButton = MyScene.FindObjectByName("TurretButton").GetComponent<UI_Button>();
			Turret1Button = MyScene.FindObjectByName("Turret1Button").GetComponent<UI_Button>();
			Turret2Button = MyScene.FindObjectByName("Turret2Button").GetComponent<UI_Button>();
			Turret3Button = MyScene.FindObjectByName("Turret3Button").GetComponent<UI_Button>();
			BuildButton.OnClick += ClickedBuild;
			Turret1Button.OnClick += () => ClickedTurret(1);
			Turret2Button.OnClick += () => ClickedTurret(2);
			Turret3Button.OnClick += () => ClickedTurret(3);

			Turret1Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/turret1.fbx");
			Turret2Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/turret2.fbx");
			Turret3Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/turret3.fbx");
			RangeDisplayObject = MyScene.CreateSceneObject("RANGE DISPLAY");
			var rdm = RangeDisplayObject.AddComponent<ModelRenderer>();
			rdm.Model = Sphere;
			//rdm.InstantiateAllMaterials();
			ShowRangeMaterial = gridMaterial.Instantiate();
			rdm.OverrideMaterials[0] = ShowRangeMaterial;
			ShowRangeMaterial.FloatValues["gridWidth"] = 0.015f;
			ShowRangeMaterial.FloatValues["gridSpacing"] = 0.15f;
		}

		private void ClickedTurret(int v)
		{
			ChosenTurret = v;
		}
		private void ClickedBuild()
		{
			IsPlacingTurret = !IsPlacingTurret;
			ChosenTurret = 0;
		}

		public override void Update()
		{
			Model? oldmodel = MouseTargetRenderer.Model;
			base.Update();
			TurretZone.IsActiveSelf = IsPlacingTurret;
			var mouse = EngineWindow.instance.MouseState;
			BuildButton.Text = IsPlacingTurret ? "CANCEL" : "BUILD";
			TurretMenuPanel.IsActive = IsPlacingTurret;
			if (ChosenTurret != 0 && IsPlacingTurret && UI_Renderer.CurrentHovered==null && !MouseLockController.isLocked && Physics.RayCast_Triangle(CameraComponent.activeCamera, Mouse.MousePosition, 200f, out var hit, raycastTags))
			{
				Model mdl = Cube;
				float range = 0;
				if (ChosenTurret == 1)
				{
					mdl = Turret1Model;
					range = 2f;
				}
				else if (ChosenTurret == 2)
				{
					mdl = Turret2Model;
					range = 3.5f;
				}	
				else if (ChosenTurret == 3)
				{
					mdl = Turret3Model;
					range = 1.25f;
				}
				MouseTargetRenderer.Model = mdl;
				RangeDisplayObject.transform.LocalScale = Vector3.One*range;

				MouseTarget.IsActiveSelf = true;
				Vector3 pos = SnapToGrid(hit.Point, gridMaterial.FloatValues["gridSpacing"]);
				MouseTarget.transform.Position = pos;
				//MouseTarget.transform.Position = hit.Point;
				MouseTargetRenderer.OutlineColor = Color4.Black;
				RangeDisplayObject.transform.Position = pos;

				if (PlacedTurrets.ContainsKey(pos) || gameManager.Money < TurretPrices[ChosenTurret])
				{
					ShowRangeMaterial.Vector3Values["gridColor"] = new(1, 0, 0);
					SetMatColor(new(1,0, 0));
				}
				else
				{
					ShowRangeMaterial.Vector3Values["gridColor"] = new(0, 1, 0);
					SetMatColor(new(0, 1, 0));
					if (mouse.IsButtonPressed(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left))
					{
						PlaceTurret();
					}
				}
		
			}
			else
			{
				MouseTarget.IsActiveSelf = false;
			}
			RangeDisplayObject.IsActiveSelf = MouseTarget.IsActiveSelf;
	

		}
		void SetMatColor(Vector3 color)
		{
			PickerMaterial.Vector3Values["lightcolor"] = color;
			for (int i =0; i<MouseTargetRenderer.OverrideMaterials.Length; i++)
			{
				MouseTargetRenderer.OverrideMaterials[i] = PickerMaterial;
			}
		}
		private void PlaceTurret()
		{
			int price = TurretPrices[ChosenTurret];
			if (price == 0) return;

			if(price>gameManager.Money)
			{
				//nie ma pieniedzy
				return;
			}
			gameManager.Money -= price;
			GameObject turretObject = MyScene.CreateSceneObject("TURRET");
			ModelRenderer? TurretRenderer = turretObject.AddComponent<ModelRenderer>();
			Turret? Turret = turretObject.AddComponent<Turret>();
			var pos = MouseTarget.transform.Position;
			turretObject.transform.Position = pos;
			PlacedTurrets.Add(pos, Turret);
			switch (ChosenTurret)
			{
				case 1:
					Turret.Range = 2f;
					Turret.Damage = 10;
					Turret.ShootingSpeed = 2f;
					Turret.IsRangeTurret = true;
					TurretRenderer.Model = Turret1Model;
					break;
				case 2:
					Turret.Range = 3.5f;
					Turret.Damage = 25;
					Turret.ShootingSpeed = 1.5f;
					TurretRenderer.Model = Turret2Model;
					break;
				case 3:
					Turret.Range = 1.25f;
					Turret.Damage = 70;
					Turret.ShootingSpeed = 3.25f;
					TurretRenderer.Model = Turret3Model;
					break;
			}
		}

		private Vector3 SnapToGrid(Vector3 position, float gridSpacing)
		{
			return new Vector3(
				MathF.Round(position.X / gridSpacing) * gridSpacing,
				MathF.Round(position.Y+0.0f, 2),
				MathF.Round(position.Z / gridSpacing) * gridSpacing
			);
		}
	}
}
