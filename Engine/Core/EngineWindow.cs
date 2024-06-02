using Assimp;
using PGK2.Engine.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PGK2.Engine.Components.Base;
using PGK2.Engine.SceneSystem;
using System.Reflection;
using PGK2.TowerDef.Scripts;
using ImGuiNET;
using TP_IMGUI;
using PGK2.Engine.Components.Base.Renderers;

namespace PGK2.Engine.Core
{
	public class EngineWindow : GameWindow
	{
		public static EngineWindow? instance;
		private TP_IMGUI.ImGuiController _imGuiController;
		Queue<int> frameQueue = new Queue<int>();
		double secTimer = 0d;
		long frames = 0;
		public static Shader shader;
		public static Shader lightShader;
		public static Shader OutlineShader;
		private bool changedFocus;
		public float aspectRatio { get; private set; }
		public static Queue<Component> StartQueue = new();
		public CameraComponent? activeCamera {get=>CameraComponent.activeCamera; }
		public EngineWindow(GameWindowSettings gws, NativeWindowSettings nws) : base(gws,nws)
		{
			instance = this;
		}

		protected override void OnLoad()
		{
			base.OnLoad();
			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.Enable(EnableCap.StencilTest);
			GL.Enable(EnableCap.FragmentLightingSgix);
			GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
			GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
			shader = new Shader($"{EngineInstance.ENGINE_PATH}/Shaders/shader.vert", $"{EngineInstance.ENGINE_PATH}/Shaders/shader.frag");
			lightShader = new Shader($"{EngineInstance.ENGINE_PATH}/Shaders/lightShader.vert", $"{EngineInstance.ENGINE_PATH}/Shaders/lightShader.frag");
			OutlineShader = new Shader($"{EngineInstance.ENGINE_PATH}/Shaders/outline.vert", $"{EngineInstance.ENGINE_PATH}/Shaders/outline.frag");
			_imGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);


			//SceneLoadTest();
			SceneMakeTest();

		}
		void SceneLoadTest()
		{
			var scene = SceneManager.LoadSceneFromFile($"{EngineInstance.ASSETS_PATH}/Scenes/GAME.lscn");
			SceneManager.LoadScene(scene);
		}
		void SceneMakeTest()
		{
			SceneSystem.Scene scene = new();
			scene.SceneName = "Game Scene";
			SceneManager.LoadScene(scene);
			GameObject LockController = scene.CreateSceneObject("LOCK CONTROLLER");
			LockController.AddComponent<MouseLockController>();

			GameObject CamParent = scene.CreateSceneObject("CAMERA PARENT");
			GameObject CameraObject = scene.CreateSceneObject("CAMERA OBJECT");
			CameraObject.transform.Parent=CamParent.transform;

			CameraObject.Components.Add<CameraComponent>();
			CameraObject.Components.Add<CameraController>();

			GameObject newObject2 = scene.CreateSceneObject("MAP OBJECT");
			var rend = newObject2.Components.Add<ModelRenderer>();
			rend.Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/Level1.fbx");
			rend.RenderTags.Add("map");
			rend.OutlineColor = Color4.Transparent;
			rend.transform.Pitch = -90f;
			rend.transform.Scale = Vector3.One * 1;


			GameObject lightObj = scene.CreateSceneObject("Light Object");
			Light light = lightObj.Components.Add<Light>();
			lightObj.transform.Position = new Vector3(1.5f, 10f, 1f);
			light.Diffuse = new Vector3(1, 1, 1f);
			light.Specular = new Vector3(1f,1f,1f);

			GameObject TestText = scene.CreateSceneObject("UI TEXT TEST");
			var text = TestText.AddComponent<UI_Text>();
			text.Text = "TESTOWY TEKST";
			text.Color = new(0, 1, 0, 0.5f);
			text.transform.Position = new(50,50,0);
			var temp = scene.CreateSceneObject("temp");
			temp.AddComponent<TemporaryCube>();
			temp.transform.Position = new(0, 2, 0);	
			
			var ai_target = scene.CreateSceneObject("AI TARGET");
			ai_target.AddComponent<ModelRenderer>().Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/cube.fbx");
			ai_target.transform.Position = new(-5, 0.12f, -4.25f);
			ai_target.transform.Scale = 0.001f * Vector3.One;
			
			var ai_test = scene.CreateSceneObject("AI TEST");
			ai_test.AddComponent<ModelRenderer>().Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/cube.fbx");
			ai_test.GetComponent<ModelRenderer>().OutlineColor = Color4.Red;
			ai_test.transform.Position = new(5, 0.12f, 3.4f);
			ai_test.transform.Scale = 0.001f * Vector3.One;
			var pathfind = ai_test.AddComponent<PathFindingAgent>();
			pathfind.SetTargetPosition(ai_target.transform.Position);

			scene.AddAwaitingObjects();

			SceneManager.SaveSceneToFile(scene, $"{EngineInstance.ASSETS_PATH}/Scenes/GAME.lscn");
			foreach(var obj in scene.GameObjects)
			{
				Console.WriteLine(obj.name);
			}
			SceneManager.LoadScene(scene);
		}
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);
			aspectRatio = (float)ClientSize.X / ClientSize.Y;
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
			if (Mouse.framesSinceLastMove>0)
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

					SceneManager.ActiveScene.Renderers.Sort((r1, r2) =>
					{
						float distance1 = (r1.gameObject.transform.Position - activeCamera.transform.Position).LengthSquared;
						float distance2 = (r2.gameObject.transform.Position - activeCamera.transform.Position).LengthSquared;
						return distance2.CompareTo(distance1);
					});

					foreach (Renderer r in SceneManager.ActiveScene.Renderers)
					{
						r.CallRender(activeCamera, EngineInstance.RenderPass.Transparent);
					}

					foreach (Renderer r in SceneManager.ActiveScene.Renderers)
					{
						r.CallRenderOutline(activeCamera);
					}
				}
				foreach (UI_Renderer uir in SceneManager.ActiveScene.UI_Renderers)
				{
					uir.CallDraw();
				}
			}
			_imGuiController.Render();
			SwapBuffers();
		}
		protected override void OnUnload()
		{
			base.OnUnload();
			shader.Dispose();
			lightShader.Dispose();
			OutlineShader.Dispose();
			_imGuiController.Dispose();

		}
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
		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);
			
			GL.Viewport(0, 0, e.Width, e.Height);
			_imGuiController.WindowResized(ClientSize.X, ClientSize.Y);
		}
		protected override void OnFocusedChanged(FocusedChangedEventArgs e)
		{
			base.OnFocusedChanged(e);
			changedFocus = true;
		}
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
					//Console.WriteLine($"POS: {Mouse.MousePosition}, CENTER: {Mouse.ScreenCenter}, DELTA: {Mouse.Delta}");
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
		public bool IsPointInWindowBounds(Vector2i point)
		{
			return ClientRectangle.Contains(point, true);
		}
	}

}