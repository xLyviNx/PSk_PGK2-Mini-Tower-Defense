using Assimp.Configs;
using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Components.Base.Renderers;
using PGK2.Engine.Core;
using PGK2.Engine.Core.Physics;
using PGK2.TowerDef.Scripts;
using System;
using System.Text.Json.Serialization;

namespace PGK2.Game.Code.TowerDef.Scripts
{
	/**
     * @class TurretManager
     * @brief Zarządza wieżyczkami w grze Tower Defense.
     */
	public class TurretManager : Component
	{
		/// <summary>
		/// Instancja TurretManager.
		/// </summary>
		public static TurretManager instance;

		/// <summary>
		/// Referencja do menedżera gry.
		/// </summary>
		GameManager? gameManager;

		/// <summary>
		/// Obiekt gdzie kładziemy wieżyczki.
		/// </summary>
		GameObject? MouseTarget;

		/// <summary>
		/// Renderer celu myszy.
		/// </summary>
		ModelRenderer? MouseTargetRenderer;

		/// <summary>
		/// Strefa, w której można umieszczać wieżyczki.
		/// </summary>
		GameObject TurretZone;

		/// <summary>
		/// Materiał dla podglądu kładzenia wieżyczki.
		/// </summary>
		Material PickerMaterial;

		/// <summary>
		/// Materiał siatki.
		/// </summary>
		Material gridMaterial;

		/// <summary>
		/// Materiał do wyświetlania zasięgu.
		/// </summary>
		Material ShowRangeMaterial;

		/// <summary>
		/// Tagi do wykrywania kolizji.
		/// </summary>
		TagsContainer raycastTags;

		/// <summary>
		/// Flaga określająca, czy gracz umieszcza wieżyczkę.
		/// </summary>
		[JsonIgnore]
		public bool IsPlacingTurret = false;

		/// <summary>
		/// Słownik przechowujący umieszczone wieżyczki.
		/// Klucz: pozycja wieżyczki, Wartość: obiekt wieżyczki
		/// </summary>
		public Dictionary<Vector3, Turret> PlacedTurrets = new();

		/// <summary>
		/// Panel menu wieżyczek.
		/// </summary>
		GameObject TurretMenuPanel;

		/// <summary>
		/// Przycisk budowania wieżyczki.
		/// </summary>
		UI_Button BuildButton;

		/// <summary>
		/// Przycisk wyboru pierwszej wieżyczki.
		/// </summary>
		UI_Button Turret1Button;

		/// <summary>
		/// Przycisk wyboru drugiej wieżyczki.
		/// </summary>
		UI_Button Turret2Button;

		/// <summary>
		/// Przycisk wyboru trzeciej wieżyczki.
		/// </summary>
		UI_Button Turret3Button;

		/// <summary>
		/// Model sześcianu.
		/// </summary>
		Model Cube;

		/// <summary>
		/// Model sfery.
		/// </summary>
		Model Sphere;

		/// <summary>
		/// Model pierwszej wieżyczki.
		/// </summary>
		Model Turret1Model;

		/// <summary>
		/// Model drugiej wieżyczki.
		/// </summary>
		Model Turret2Model;

		/// <summary>
		/// Model trzeciej wieżyczki.
		/// </summary>
		Model Turret3Model;

		/// <summary>
		/// Numer wybranej wieżyczki do budowania.
		/// </summary>
		int ChosenTurret;

		/// <summary>
		/// Tablica zawierająca ceny wieżyczek. Indeks odpowiada typowi wieżyczki.
		/// </summary>
		public static int[] TurretPrices = { 0, 1000, 500, 800 };

		/// <summary>
		/// Obiekt wyświetlający zasięg.
		/// </summary>
		GameObject RangeDisplayObject;

		/// <summary>
		/// Panel informacyjny o wybranej wieżyczce.
		/// </summary>
		UI_Panel? TurretInfoPanel;

		/// <summary>
		/// Wybrana wieżyczka.
		/// </summary>
		Turret? SelectedTurret;

		/// <summary>
		/// Tekst wyświetlający poziom wieżyczki.
		/// </summary>
		UI_Text? LevelText;

		/// <summary>
		/// Przycisk ulepszania wybranej wieżyczki.
		/// </summary>
		UI_Button? UpgradeButton;

		/// <summary>
		/// Metoda wywoływana przy inicjalizacji obiektu.
		/// </summary>
		public override void Awake()
		{
			base.Awake();
			instance = this;
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

			CreateTurretInfoPanel();

		}
		/// <summary>
		/// Tworzy panel informacyjny o wieżyczce wyświetlany po naciśnięciu na nią.
		/// </summary>
		void CreateTurretInfoPanel()
		{
			// Create the Turret Info Panel
			var turretInfoPanel = MyScene.CreateSceneObject("TurretInfoPanel").AddComponent<UI_Panel>();
			turretInfoPanel.UI_Alignment = UI_Renderer.Alignment.Left;
			turretInfoPanel.Size = new(212, 150);
			turretInfoPanel.Color = new(0, 0, 0, 0.7f);
			turretInfoPanel.transform.Position = new(10, 0, 0);
			turretInfoPanel.Pivot = new (0, 0.5f);
			turretInfoPanel.Z_Index = 100; // Ensure it's on top of other UI elements
			turretInfoPanel.gameObject.IsActiveSelf = (false); // Initially hidden

			// Create the Level Text
			var levelText = MyScene.CreateSceneObject("LevelText").AddComponent<UI_Text>();
			levelText.UI_Alignment = UI_Renderer.Alignment.Left;
			levelText.Text = "Level: 1";
			levelText.FontSize = 1.5f;
			levelText.transform.Parent = turretInfoPanel.transform;
			levelText.transform.Position = new Vector3(116, 50, 0);
			levelText.Pivot = new (0.5f, 0.5f);

			// Create the Upgrade Button
			var upgradeButton = MyScene.CreateSceneObject("UpgradeButton").AddComponent<UI_Button>();
			upgradeButton.UI_Alignment = UI_Renderer.Alignment.Left;
			upgradeButton.Text = "Upgrade";
			upgradeButton.FontSize = 1.5f;
			upgradeButton.Padding = new (20, 10);
			upgradeButton.Pivot = new (0.5f, 0.5f);
			upgradeButton.transform.Parent = turretInfoPanel.transform;
			upgradeButton.transform.Position = new Vector3(116, 0, 0);
			upgradeButton.OnClick += UpgradeSelectedTurret;

			// Create the Destroy Button
			var destroyButton = MyScene.CreateSceneObject("DestroyButton").AddComponent<UI_Button>();
			destroyButton.UI_Alignment = UI_Renderer.Alignment.Left;
			destroyButton.Text = "Destroy";
			destroyButton.FontSize = 1.5f;
			destroyButton.Padding = new (20, 10);
			destroyButton.Pivot = new (0.5f, 0.5f);
			destroyButton.transform.Parent = turretInfoPanel.transform;
			destroyButton.transform.Position = new Vector3(116, -50, 0);
			destroyButton.OnClick += DestroyTurret;

			TurretInfoPanel = turretInfoPanel;
			UpgradeButton = upgradeButton;
			LevelText = levelText;
		}
		/// <summary>
		/// Metoda wywoływana podczas niszczenia wieżyczki.
		/// </summary>
		private void DestroyTurret()
		{
			if (SelectedTurret != null)
			{
				SelectedTurret.gameObject.Destroy();
				SelectedTurret = null;
			}
			TurretInfoPanel.gameObject.IsActiveSelf = (false);
		}
		/// <summary>
		/// Metoda zwracająca cenę ulepszenia wieżyczki.
		/// </summary>
		int UpgradePrice(Turret turr)
		{
			int p = 1000;
			p += turr.Level * 250;
			return p;
		}
		/// <summary>
		/// Metoda obsługująca ulepszanie wybranej wieżyczki.
		/// </summary>
		private void UpgradeSelectedTurret()
		{
			if (SelectedTurret.Level < 5)
			{
				if(gameManager.Money >= UpgradePrice(SelectedTurret))
				{
					gameManager.Money-= UpgradePrice(SelectedTurret);
					SelectedTurret.Level++;
					SelTurretChanged(ref SelectedTurret);
				}
			}
		}
		/// <summary>
		/// Metoda obsługująca kliknięcie na wybrany typ wieżyczki do budowania w menu.
		/// </summary>
		private void ClickedTurret(int v)
		{
			ChosenTurret = v;
		}
		/// <summary>
		/// Metoda obsługująca kliknięcie na przycisk budowania wieżyczki (otwiera menu).
		/// </summary>
		private void ClickedBuild()
		{
			IsPlacingTurret = !IsPlacingTurret;
			ChosenTurret = 0;
		}
		/// <summary>
		/// Metoda aktualizująca logikę wieżyczki.
		/// </summary>
		public override void Update()
		{
			base.Update();
			SelectedTurretLogic();
			TurretZone.IsActiveSelf = IsPlacingTurret;
			var mouse = EngineWindow.instance.MouseState;
			BuildButton.Text = IsPlacingTurret ? "CANCEL" : "BUILD";
			TurretMenuPanel.IsActive = IsPlacingTurret;
			if (ChosenTurret != 0 && IsPlacingTurret && UI_Renderer.CurrentHovered==null && !MouseLockController.IsLocked && Physics.RayCast_Triangle(CameraComponent.activeCamera, Mouse.MousePosition, 200f, out var hit, raycastTags))
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
			RangeDisplayObject.IsActiveSelf = MouseTarget.IsActiveSelf || SelectedTurret!=null;
			if (SelectedTurret!=null)
			{
				RangeDisplayObject.transform.Position = SelectedTurret.transform.Position;
				RangeDisplayObject.transform.LocalScale = Vector3.One * SelectedTurret.LevelRange;
			}

		}
		/// <summary>
		/// Metoda aktualizująca klikanie w wieżyczki.
		/// </summary>
		private void SelectedTurretLogic()
		{
			Turret oldSel = SelectedTurret;

			if (EngineWindow.instance.MouseState.IsButtonPressed(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left))
			{
				TagsContainer tags = new();
				tags.Add("turret");
				if (UI_Renderer.CurrentHovered == null)
				{
					if (Physics.RayCast_Triangle(CameraComponent.activeCamera, Mouse.MousePosition, 300f, out var hit, tags))
					{
						SelectedTurret = hit.gameObject.GetComponent<Turret>();
					}
					else
					{
						SelectedTurret = null;
					}
				}
			}
			if(oldSel!=SelectedTurret)
				SelTurretChanged(ref oldSel);
		}

		/// <summary>
		/// Metoda obsługująca aktualizację wybranej wieżyczki.
		/// </summary>
		private void SelTurretChanged(ref Turret old)
		{
			TurretInfoPanel.gameObject.IsActiveSelf = SelectedTurret != null;
			if (SelectedTurret == null) return;
			LevelText.Text = $"Level: {SelectedTurret.Level}";
			if (SelectedTurret.Level < 5)
			{
				UpgradeButton.Text = $"UGPRADE (${UpgradePrice(SelectedTurret)})";
			}
			else
			{
				UpgradeButton.Text = $"MAX LEVEL";
			}

		}
		/// <summary>
		/// Ustawia kolor materiału podglądu.
		/// </summary>
		void SetMatColor(Vector3 color)
		{
			PickerMaterial.Vector3Values["lightcolor"] = color;
			for (int i =0; i<MouseTargetRenderer.OverrideMaterials.Length; i++)
			{
				MouseTargetRenderer.OverrideMaterials[i] = PickerMaterial;
			}
		}
		/// <summary>
		/// Umieszcza wieżyczkę na scenie.
		/// </summary>
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
			turretObject.Tags.Add("turret");
			ModelRenderer? TurretRenderer = turretObject.AddComponent<ModelRenderer>();
			Turret? Turret = turretObject.AddComponent<Turret>();
			var pos = MouseTarget.transform.Position;
			turretObject.transform.Position = pos;
			PlacedTurrets.Add(pos, Turret);
			Turret.Level = 1;
			Turret.mymodel = TurretRenderer;
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
		// <summary>
		/// Zaokrągla pozycję do siatki.
		/// </summary>
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
