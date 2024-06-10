using PGK2.Engine.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PGK2.Engine.Components.Base;
using PGK2.Engine.SceneSystem;
using PGK2.TowerDef.Scripts;
using TP_IMGUI;
using PGK2.Engine.Components.Base.Renderers;
using PGK2.Game.Code.TowerDef.Scripts;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Główna klasa reprezentująca okno gry i zarządzająca jej wyświetlaniem oraz logiką.
	/// </summary>
	public class EngineWindow : GameWindow
	{
		/// <summary>
		/// Instancja okna gry.
		/// </summary>
		public static EngineWindow? instance;

		/// <summary>
		/// Kontroler interfejsu użytkownika ImGui.
		/// </summary>
		private TP_IMGUI.ImGuiController _imGuiController;

		/// <summary>
		/// Kolejka ramek.
		/// </summary>
		Queue<int> frameQueue = new Queue<int>();

		/// <summary>
		/// Licznik czasu w sekundach.
		/// </summary>
		double secTimer = 0d;

		/// <summary>
		/// Licznik klatek na sekundę.
		/// </summary>
		long frames = 0;

		/// <summary>
		/// Podstawowy Shader dla elementów graficznych.
		/// </summary>
		public static Shader shader;

		/// <summary>
		/// Shader dla światła (jeden kolor).
		/// </summary>
		public static Shader lightShader;

		/// <summary>
		/// Shader dla obrysu obiektów.
		/// </summary>
		public static Shader OutlineShader;

		/// <summary>
		/// Shader dla siatki.
		/// </summary>
		public static Shader GridShader;

		/// <summary>
		/// Flaga informująca o zmianie fokusu okna.
		/// </summary>
		private bool changedFocus;

		/// <summary>
		/// Współczynnik proporcji okna.
		/// </summary>
		public float aspectRatio { get; private set; }

		/// <summary>
		/// Kolejka komponentów do uruchomienia.
		/// </summary>
		public static Queue<Component> StartQueue = new();

		/// <summary>
		/// Zdarzenie wywoływane na końcu każdej klatki.
		/// </summary>
		public event EventHandler EndOfFrame;

		/// <summary>
		/// Aktywna kamera w grze.
		/// </summary>
		public CameraComponent? activeCamera { get => CameraComponent.activeCamera; }

		/// <summary>
		/// Konstruktor klasy EngineWindow.
		/// </summary>
		/// <param name="gws">Ustawienia okna gry.</param>
		/// <param name="nws">Ustawienia natywne okna gry.</param>
		public EngineWindow(GameWindowSettings gws, NativeWindowSettings nws) : base(gws,nws)
		{
			instance = this;
		}
		/// <summary>
		/// Metoda wywoływana po załadowaniu okna gry.
		/// </summary>
		protected override void OnLoad()
		{
			base.OnLoad();
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.Enable(EnableCap.StencilTest);
			GL.Enable(EnableCap.FragmentLightingSgix);
			GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
			GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);
			shader = new Shader($"{EngineInstance.ENGINE_PATH}/Shaders/shader.vert", $"{EngineInstance.ENGINE_PATH}/Shaders/shader.frag");
			lightShader = new Shader($"{EngineInstance.ENGINE_PATH}/Shaders/lightShader.vert", $"{EngineInstance.ENGINE_PATH}/Shaders/lightShader.frag");
			OutlineShader = new Shader($"{EngineInstance.ENGINE_PATH}/Shaders/outline.vert", $"{EngineInstance.ENGINE_PATH}/Shaders/outline.frag");
			GridShader = new Shader($"{EngineInstance.ENGINE_PATH}/Shaders/grid.vert", $"{EngineInstance.ENGINE_PATH}/Shaders/grid.frag");
			GridShader.EnforceTransparencyPass = true;
			_imGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);


			LoadStartScene();
			//nie usuwaj tych linijek Krzysztof. Co najwyzej tu jak napisalem zebys nie usuwal to tylko to xd
			//MakeGameScene();
			//MakeGameOverScene();
			//MakeWinScene();
			//MakeMenuScene();

		}
		/// <summary>
		/// Metoda wczytująca początkową scenę gry.
		/// </summary>
		void LoadStartScene()
		{
			var scene = SceneManager.LoadSceneFromFile($"{EngineInstance.ASSETS_PATH}/Scenes/MENU.lscn");
			SceneManager.LoadScene(scene);
		}
		#region MAKE_SCENES
		void MakeGameScene()
		{
			SceneSystem.Scene scene = new();
			scene.SceneName = "Game Scene";
			GameObject LockController = scene.CreateSceneObject("LOCK CONTROLLER");
			LockController.AddComponent<MouseLockController>();

			GameObject CamParent = scene.CreateSceneObject("CAMERA PARENT");
			GameObject CameraObject = scene.CreateSceneObject("CAMERA OBJECT");
			CameraObject.transform.Parent=CamParent.transform;

			var maincam = CameraObject.Components.Add<CameraComponent>();
			maincam.ExcludeTags.Add("enemyhitbox");
			maincam.ExcludeTags.Add("hide");
			CameraObject.Components.Add<CameraController>();

			GameObject newObject2 = scene.CreateSceneObject("MAP OBJECT");
			var rend = newObject2.Components.Add<ModelRenderer>();
			rend.Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/Level1.fbx");
			rend.gameObject.Tags.Add("map");
			rend.OutlineColor = Color4.Transparent;
			//rend.transform.Pitch = -90f;
			rend.transform.LocalScale = Vector3.One * 1;


			GameObject obstacle = scene.CreateSceneObject("MAP OBSTACLE");
			var obstaclerend = obstacle.Components.Add<ModelRenderer>();
			obstaclerend.Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/obstacle.fbx");
			obstacle.Tags.Add("map");
			obstacle.Tags.Add("hide");

			GameObject lightObj = scene.CreateSceneObject("Light Object");
			Light light = lightObj.Components.Add<Light>();
			lightObj.transform.Position = new Vector3(1.5f, 10f, 1f);
			light.Diffuse = new Vector3(1, 1, 1f);
			light.Specular = new Vector3(1f,1f,1f);

			GameObject lightObj2 = scene.CreateSceneObject("Light Object 2");
			Light light2 = lightObj2.Components.Add<Light>();
			lightObj2.transform.Position = new Vector3(8, 0.5f, 0f);
			light2.Diffuse = new Vector3(0.5f, 0.5f, 1f);
			light2.Specular = new Vector3(1f, 1f, 1f);		
			
			GameObject lightObj3 = scene.CreateSceneObject("Light 3");
			Light light3 = lightObj3.Components.Add<Light>();
			light3.transform.Position = new Vector3(0, 1f, 10f);
			light3.Diffuse = new Vector3(0.6f, 0.6f, 0.6f);
			light3.Specular = new Vector3(0.6f, 0.6f, 0.6f);
			light3.Ambient = new Vector3(0.1f, 0.1f, 0.1f);

			var ai_target = scene.CreateSceneObject("ai_target");
			ai_target.transform.Position = new(-5, 0.12f, -4.25f);
			ai_target.transform.LocalScale = 0.001f * Vector3.One;

			var Manager = scene.CreateSceneObject("Game Manager").AddComponent<GameManager>();

			var TurretPlaceRegion = scene.CreateSceneObject("TurretRegion").AddComponent<ModelRenderer>();
			TurretPlaceRegion.gameObject.Tags.Add("TurretRegion");
			TurretPlaceRegion.transform.LocalPosition = new(0, 0.01f, 0f);
			TurretPlaceRegion.Model= Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/turretzone.fbx");
			TurretPlaceRegion.transform.LocalScale = Vector3.One;


			var TurretPanel = scene.CreateSceneObject("Turret Main Panel").AddComponent<UI_Panel>();
			TurretPanel.Z_Index = -1;
			TurretPanel.Size = new(210, 200);
			TurretPanel.UI_Alignment = UI_Renderer.Alignment.DownLeft;
			TurretPanel.transform.Position = new(10, -10, 0);
			TurretPanel.Color = new(0, 0, 0, 0.5f);
			TurretPanel.Pivot = new(0, 1);


						
			
			var Turret1Button = scene.CreateSceneObject("Turret1Button").AddComponent<UI_Button>();
			Turret1Button.Text = "Area Turret ($1000)";
			Turret1Button.UI_Alignment = UI_Renderer.Alignment.DownLeft;
			Turret1Button.Pivot = new(0.5f, 1);
			Turret1Button.Padding = new(30, 10);
			
			var Turret2Button = scene.CreateSceneObject("Turret2Button").AddComponent<UI_Button>();
			Turret2Button.Text = "Standard Turret ($500)";
			Turret2Button.UI_Alignment = UI_Renderer.Alignment.DownLeft;
			Turret2Button.Pivot = new(0.5f, 1);
			Turret2Button.Padding = new(20, 10);
			
			var Turret3Button = scene.CreateSceneObject("Turret3Button").AddComponent<UI_Button>();
			Turret3Button.Text = "Short-Range Turret ($800)";
			Turret3Button.UI_Alignment = UI_Renderer.Alignment.DownLeft;
			Turret3Button.Pivot = new(0.5f, 1);
			Turret3Button.Padding = new(10, 10);

			Turret1Button.transform.Parent = TurretPanel.transform;
			Turret2Button.transform.Parent = TurretPanel.transform;
			Turret3Button.transform.Parent = TurretPanel.transform;

			Turret1Button.transform.Position = new(116, -170, 0);
			Turret2Button.transform.Position = new(116, -132, 0);
			Turret3Button.transform.Position = new(116, -94, 0);

			var PlaceTurretsButton = scene.CreateSceneObject("TurretButton").AddComponent<UI_Button>();
			PlaceTurretsButton.Text = "BUILD";
			PlaceTurretsButton.FontSize = 1.5f;
			PlaceTurretsButton.UI_Alignment = UI_Renderer.Alignment.DownLeft;
			PlaceTurretsButton.Pivot = new(0, 1);
			PlaceTurretsButton.transform.Position = new(75, -25, 0);
			PlaceTurretsButton.Z_Index = 1;
			var TurretManager = scene.CreateSceneObject("Turret Manager").AddComponent<TurretManager>();

			scene.AddAwaitingObjects();
			SceneManager.SaveSceneToFile(scene, $"{EngineInstance.ASSETS_PATH}/Scenes/GAME.lscn");

			SceneManager.LoadScene(scene);
		}
		void MakeMenuScene()
		{
			SceneSystem.Scene scene = new();
			scene.SceneName = "Menu Scene";
			GameObject CameraObject = scene.CreateSceneObject("CAMERA OBJECT");
			var maincam = CameraObject.Components.Add<CameraComponent>();
			maincam.transform.Position = new Vector3(0, 0, -1.5f);
			maincam.BackgroundColor = new(0.2f, 0.5f, 0f, 1f);
			maincam.FieldOfView = 20;

			GameObject lightObj = scene.CreateSceneObject("Light Object");
			Light light = lightObj.Components.Add<Light>();
			lightObj.transform.Position = new Vector3(0f, 1f, -1f);
			light.Diffuse = new Vector3(1, 0.8f, 0.8f);
			light.Specular = new Vector3(1f, 0.9f, 0.9f);


			var enemyO = scene.CreateSceneObject("enemy");
			var enemyModel = enemyO.AddComponent<ModelRenderer>();
			enemyModel.Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/enemy1.fbx");
			enemyModel.transform.Position = new(0.4f, -0.6f, 0.7f);
			enemyModel.transform.Yaw = 190;

			var titleO = scene.CreateSceneObject("Title");
			var title = titleO.AddComponent<UI_Text>();
			title.UI_Alignment = UI_Renderer.Alignment.CenterUp;
			title.transform.Position = new Vector3(0, 30f, 0);
			title.Text = "Basic Tower Defense Game";
			title.FontSize = 3f;
			title.Pivot = new(0.5f, 0f);	
			
			
			var creditsO = scene.CreateSceneObject("Title");
			var credits = creditsO.AddComponent<UI_Text>();
			credits.UI_Alignment = UI_Renderer.Alignment.DownLeft;
			credits.transform.Position = new Vector3(0, 0, 0);
			credits.Text = "2ID14B:\n" +
				"Sygut Grzegorz\n" +
				"Synowiec Adrian\n" +
				"Szylinski Krzysztof\n" +
				"Sieczkowski Dawid\n";
			credits.FontSize = 1f;
			credits.Pivot = new(0f, 1f);
					
			var btnO = scene.CreateSceneObject("PlayButton");
			var btn = btnO.AddComponent<UI_Button>();
			btn.UI_Alignment = UI_Renderer.Alignment.Center;
			btn.transform.Position = new(0, -22.5f, 0);
			btn.Text = "Play";
			btn.FontSize = 1.5f;
			btn.Padding.X = 70f;
			btn.Pivot = new(0.5f, 0.5f);
						
			var QbtnO = scene.CreateSceneObject("QuitButton");
			var Qbtn = QbtnO.AddComponent<UI_Button>();
			Qbtn.UI_Alignment = UI_Renderer.Alignment.Center;
			Qbtn.Text = "Quit";
			Qbtn.transform.Position = new(0, 22.5f, 0);
			Qbtn.FontSize = 1.5f;
			Qbtn.Padding.X = 70f;
			Qbtn.Pivot = new(0.5f, 0.5f);

			var controller = scene.CreateSceneObject("Menu Controller").AddComponent<Menu>();


			scene.AddAwaitingObjects();
			SceneManager.SaveSceneToFile(scene, $"{EngineInstance.ASSETS_PATH}/Scenes/MENU.lscn");

			SceneManager.LoadScene(scene);
		}
		void MakeGameOverScene()
		{
			SceneSystem.Scene scene = new();
			scene.SceneName = "Game Over Scene";

			GameObject CameraObject = scene.CreateSceneObject("CAMERA OBJECT");
			var maincam = CameraObject.Components.Add<CameraComponent>();
			var fc = CameraObject.Components.Add<Freecam>();
			maincam.BackgroundColor = new(0.1f, 0.1f, 0.1f, 1f); 
			maincam.FieldOfView = 20;

			// Game Over Title
			var titleO = scene.CreateSceneObject("Title");
			var title = titleO.AddComponent<UI_Text>();
			title.UI_Alignment = UI_Renderer.Alignment.CenterUp;
			title.transform.Position = new Vector3(0, 30f, 0);
			title.Text = "Game Over";
			title.FontSize = 3f;
			title.Pivot = new(0.5f, 0f);

			// Return to Menu Button
			var btnO = scene.CreateSceneObject("ReturnButton");
			var btn = btnO.AddComponent<UI_Button>();
			btn.UI_Alignment = UI_Renderer.Alignment.Center;
			btn.transform.Position = new(0, -22.5f, 0);
			btn.Text = "Return to Menu";
			btn.FontSize = 1.5f;
			btn.Padding.X = 70f;
			btn.Pivot = new(0.5f, 0.5f);

			var controller = scene.CreateSceneObject("GameOver Controller").AddComponent<GameOverController>();

			scene.AddAwaitingObjects();
			SceneManager.SaveSceneToFile(scene, $"{EngineInstance.ASSETS_PATH}/Scenes/GAMEOVER.lscn");

			SceneManager.LoadScene(scene);
		}
		void MakeWinScene()
		{
			SceneSystem.Scene scene = new();
			scene.SceneName = "Win Scene";
			GameObject CameraObject = scene.CreateSceneObject("CAMERA OBJECT");
			var maincam = CameraObject.Components.Add<CameraComponent>();
			var fc = CameraObject.Components.Add<Freecam>();
			maincam.BackgroundColor = new(0.2f, 0.7f, 0.2f, 1f);  // Greenish background for winning
			maincam.FieldOfView = 20;

			// Win Title
			var titleO = scene.CreateSceneObject("Title");
			var title = titleO.AddComponent<UI_Text>();
			title.UI_Alignment = UI_Renderer.Alignment.CenterUp;
			title.transform.Position = new Vector3(0, 30f, 0);
			title.Text = "You Win!";
			title.FontSize = 3f;
			title.Pivot = new(0.5f, 0f);

			// Return to Menu Button
			var menuBtnO = scene.CreateSceneObject("MenuButton");
			var menuBtn = menuBtnO.AddComponent<UI_Button>();
			menuBtn.UI_Alignment = UI_Renderer.Alignment.Center;
			menuBtn.transform.Position = new(0, -22.5f, 0);
			menuBtn.Text = "Return to Menu";
			menuBtn.FontSize = 1.5f;
			menuBtn.Padding.X = 70f;
			menuBtn.Pivot = new(0.5f, 0.5f);


			// Replay Game Button
			var replayBtnO = scene.CreateSceneObject("ReplayButton");
			var replayBtn = replayBtnO.AddComponent<UI_Button>();
			replayBtn.UI_Alignment = UI_Renderer.Alignment.Center;
			replayBtn.transform.Position = new(0, 22.5F, 0);
			replayBtn.Text = "Replay";
			replayBtn.FontSize = 1.5f;
			replayBtn.Padding.X = 70f;
			replayBtn.Pivot = new(0.5f, 0.5f);

			var controller = scene.CreateSceneObject("Win Controller").AddComponent<WinController>();


			scene.AddAwaitingObjects();
			SceneManager.SaveSceneToFile(scene, $"{EngineInstance.ASSETS_PATH}/Scenes/WIN.lscn");
			SceneManager.LoadScene(scene);
		}
		#endregion
		/// <summary>
		/// Metoda wywoływana podczas renderowania klatki gry.
		/// </summary>
		/// <param name="e">Argument zawierający dane o klatce renderowania.</param>
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);
			aspectRatio = (float)ClientSize.X / ClientSize.Y;
			GL.ClearColor(CameraComponent.activeCamera != null? CameraComponent.activeCamera.BackgroundColor : Color4.Black);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			GL.Enable(EnableCap.DepthTest);

			if (Mouse.framesSinceLastMove > 0)
				Mouse.Delta = Vector2.Zero;
			else
				Mouse.framesSinceLastMove++;

			if (SceneManager.ActiveScene != null)
			{
				bool interrupted = false;
				do
				{
					foreach (CameraComponent cam in SceneManager.ActiveScene.Cameras)
					{
						if (cam == null)
						{
							SceneManager.ActiveScene.Cameras.RemoveAll(item => item == null);
							interrupted = true;
							break;
						}
						cam.RenderUpdate();
					}
				} while (interrupted);

				if (activeCamera != null)
				{
					foreach (Renderer r in SceneManager.ActiveScene.Renderers)
					{
						r.CallRender(activeCamera, EngineInstance.RenderPass.Opaque);
					}
	
					TransparentPass();

					foreach (Renderer r in SceneManager.ActiveScene.Renderers)
					{
						r.CallRenderOutline(activeCamera);
					}
				}

				foreach (UI_Renderer uir in SceneManager.ActiveScene.UI_Renderers.OrderBy(u => u.Z_Index))
				{
					uir.CallDraw();
				}
			}

			_imGuiController.Render();
			SwapBuffers();
			OnEndOfFrame();
		}
		/// <summary>
		/// Metoda wykonująca render przezroczystych obiektów podczas renderowania wraz z sortowaniem (nie jest idealne).
		/// </summary>
		private void TransparentPass()
		{
			List<ModelRenderer> modelRenderers = SceneManager.ActiveScene.Renderers
						.OfType<ModelRenderer>()
						.ToList();

			List<(Mesh, int, ModelRenderer, float)> modelRendererDistances = new();

			foreach (ModelRenderer modelRenderer in modelRenderers)
			{
				if (modelRenderer.Model == null) continue;
				int i = 0;
				foreach (Mesh m in modelRenderer.Model.meshes)
				{
					Material mat = modelRenderer.OverrideMaterials[i] != null ? modelRenderer.OverrideMaterials[i] : m.Material;
					if (!mat.HasTransparency) continue;
					BoundingBox bb = m.BoundingBox;
					bb = TransformBoundingBox(bb, modelRenderer.transform.GetModelMatrix());
					float distance = CalculateDistanceToCamera(bb, activeCamera);
					modelRendererDistances.Add((m, i, modelRenderer, distance));
					i++;
				}
			}
			modelRendererDistances.Sort((x, y) => y.Item4.CompareTo(x.Item4));
			foreach ((Mesh m, int i, ModelRenderer mr, float dist) in modelRendererDistances)
			{
				if (mr.CanDraw(activeCamera))
				{
					bool overrided = (mr.OverrideMaterials != null && mr.OverrideMaterials.Length >= i && mr.OverrideMaterials[i] != null);
					Material? mat = overrided ? mr.OverrideMaterials[i] : m.Material;
					m.Draw(mr.transform.GetModelMatrix(), activeCamera.ViewMatrix, activeCamera.ProjectionMatrix, mr.MyScene.Lights, activeCamera, overrided ? mat : null);
				}
			}
		}
		/// <summary>
		/// Metoda transformująca granice prostokątne obiektu na podstawie jego macierzy transformacji modelu.
		/// </summary>
		/// <param name="boundingBox">Granice prostokątne obiektu.</param>
		/// <param name="ModelMatrix">Macierz transformacji modelu.</param>
		/// <returns>Transformowany bounding box.</returns>
		BoundingBox TransformBoundingBox(BoundingBox boundingBox, Matrix4 ModelMatrix)
		{
			Vector3 min = Vector3.TransformPosition(boundingBox.Min, ModelMatrix);
			Vector3 max = Vector3.TransformPosition(boundingBox.Max, ModelMatrix);
			return new BoundingBox(min, max);
		}
		/// <summary>
		/// Metoda obliczająca odległość od obiektu do kamery w grze.
		/// </summary>
		/// <param name="boundingBox">Granice prostokątne obiektu.</param>
		/// <param name="camera">Komponent kamery w grze.</param>
		/// <returns>Odległość od obiektu do kamery.</returns>
		float CalculateDistanceToCamera(BoundingBox boundingBox, CameraComponent camera)
		{
			Vector3 cameraPosition = camera.gameObject.transform.Position;
			Vector3 center = (boundingBox.Max + boundingBox.Min) / 2.0f;
			Vector3 toCenter = center - cameraPosition;
			return toCenter.LengthFast;
		}
		/// <summary>
		/// Metoda wywoływana na końcu każdej klatki renderowania.
		/// </summary>
		protected virtual void OnEndOfFrame()
		{
			EndOfFrame?.Invoke(this, EventArgs.Empty);
		}
		/// <summary>
		/// Asynchronicznie oczekuje na zakończenie klatki renderowania.
		/// </summary>
		/// <returns>Zadanie reprezentujące oczekiwanie na zakończenie klatki.</returns>
		public async Task WaitForEndOfFrame()
		{
			var tcs = new TaskCompletionSource<bool>();

			EventHandler handler = null;
			handler = (sender, args) =>
			{
				EndOfFrame -= handler;
				tcs.SetResult(true);
			};

			EndOfFrame += handler;

			await tcs.Task;
		}

		/// <summary>
		/// Metoda wywoływana podczas usuwania zasobów aplikacji.
		/// </summary>
		protected override void OnUnload()
		{
			base.OnUnload();
			shader.Dispose();
			lightShader.Dispose();
			OutlineShader.Dispose();
			_imGuiController.Dispose();
			foreach(var m in Model.LoadedModels.Values)
			{
				foreach(Mesh mesh in m.meshes)
				{
					mesh.indices.Clear();
					mesh.vertices.Clear();
				}
				m.meshes.Clear();
			}
			Model.LoadedModels.Clear();
		}
		/// <summary>
		/// Metoda wywoływana podczas przesuwania myszy.
		/// </summary>
		/// <param name="e">Dane zdarzenia myszy.</param>
		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			base.OnMouseMove(e);
			if (Mouse.IgnoreDelta)
			{
				Mouse.IgnoreDelta = false;
				return;
			}
			Mouse.Delta = e.Delta;
			Mouse.MousePosition = e.Position;
			Mouse.framesSinceLastMove = 0;
		}
		/// <summary>
		/// Metoda wywoływana podczas zmiany rozmiaru okna aplikacji.
		/// </summary>
		/// <param name="e">Dane zdarzenia zmiany rozmiaru okna.</param>
		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);
			
			GL.Viewport(0, 0, e.Width, e.Height);
			_imGuiController.WindowResized(ClientSize.X, ClientSize.Y);
		}
		/// <summary>
		/// Metoda wywoływana podczas zmiany stanu aktywności aplikacji.
		/// </summary>
		/// <param name="e">Dane zdarzenia zmiany stanu aktywności.</param>
		protected override void OnFocusedChanged(FocusedChangedEventArgs e)
		{
			base.OnFocusedChanged(e);
			changedFocus = true;
		}
		/// <summary>
		/// Metoda wywoływana podczas aktualizacji klatki gry.
		/// </summary>
		/// <param name="e">Argumenty klatki aktualizacji.</param>
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
			_imGuiController.Update(this, (float)e.Time);
			CursorState = Mouse.IsLocked ? CursorState.Grabbed : CursorState.Normal;
			if (Mouse.IsLocked)
			{
				if (!changedFocus)
				{
					unsafe
					{
						Cursor* cursor = GLFW.CreateStandardCursor(CursorShape.Arrow);
					}
					//Mouse.LockDelta = Mouse.ScreenCenter - Mouse.MousePosition;
				}
				else
					changedFocus=false;
			}
			frames++;
			Time.doubleDeltaTime = e.Time;
			if (secTimer < 1f)
			{
				secTimer += Time.deltaTime;
			}
			else
			{
				//Console.WriteLine($"FPS: {frames}");
				frames = 0;
				secTimer = 0f;
			}

			while (StartQueue.Count > 0)
			{
				var component = StartQueue.Dequeue();
				if (!component.CalledAwake)
				{
					component.Awake();
					component.CalledAwake = true;
				}
				component.Start();
			}

			if (SceneManager.ActiveScene!=null)
			{
				SceneManager.ActiveScene.AddAwaitingObjects();
				foreach(GameObject obj in SceneManager.ActiveScene.GameObjects)
				{
					obj.Update();
				}
				SceneManager.ActiveScene.RemoveAwaitingObjects();
			}
		}
	}

}