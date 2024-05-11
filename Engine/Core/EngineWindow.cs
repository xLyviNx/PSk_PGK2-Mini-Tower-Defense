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

namespace PGK2.Engine.Core
{
	public class EngineWindow : GameWindow
	{
		public static EngineWindow? instance;
		Queue<int> frameQueue = new Queue<int>();
		double secTimer = 0d;
		long frames = 0;
		public static Shader shader;
		public static Shader lightShader;
		public static Shader OutlineShader;
		Renderer test;
		private bool changedFocus;
		public float aspectRatio { get; private set; }
		GameObject gobj;
		public static Queue<Component> StartQueue = new();
		public CameraComponent? activeCamera {get=>CameraComponent.activeCamera; }
		float[] vertices = {
				-0.5f, -0.5f, 0.0f, //Bottom-left vertex
				 0.5f, -0.5f, 0.0f, //Bottom-right vertex
				 0.0f,  0.5f, 0.0f  //Top vertex
			};
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
			shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
			lightShader = new Shader("Shaders/lightShader.vert", "Shaders/lightShader.frag");
			OutlineShader = new Shader("Shaders/outline.vert", "Shaders/outline.frag");
			SceneTest();

		}
		void SceneTest()
		{
			SceneSystem.Scene scene = new();
			SceneManager.LoadScene(scene);
			GameObject newObject = scene.CreateSceneObject("CAMERA OBJECT");
			newObject.Components.Add<CameraComponent>();
			newObject.Components.Add<Freecam>();

			GameObject newObject2 = scene.CreateSceneObject("RENDER OBJECT");
			var rend = newObject2.Components.Add<ModelRenderer>();
			rend.Model = new Model("Models/cube.fbx");
			rend.OutlineColor = Color4.Aqua;
			if (rend.Model != null)
			{
				rend.transform.Scale = Vector3.One * 0.005f;
				Console.WriteLine($"Loaded Model: {rend.Model.meshes.Count} MESHES");
				rend.Model.meshes[0].Material.Vector3Values["material.diffuse"] = new Vector3(1f, 1f,1f);
				rend.Model.meshes[0].Material.Vector3Values["material.specular"] = new Vector3(1f, 1f, 1f);
				rend.Model.meshes[0].Material.FloatValues["material.shininess"] =256f;
				rend.Model.meshes[0].Material.FloatValues["material.transparency"] = 1;
			}

			GameObject newObject3 = scene.CreateSceneObject("TRANSPARENT");
			var rend2 = newObject3.Components.Add<ModelRenderer>();
			rend2.Model = new Model("Models/cube.fbx");
			rend2.OutlineColor = Color4.Yellow;
			if (rend2.Model != null)
			{
				rend2.transform.Scale = Vector3.One * 0.005f;
				rend2.transform.Position = new Vector3(-0.5f,-0.5f,-0.5f);
				rend2.Model.meshes[0].Material.Vector3Values["material.diffuse"] = new Vector3(1f,1f,1f);
				rend2.Model.meshes[0].Material.Vector3Values["material.specular"] = new Vector3(1f, 1f, 1f);
				rend2.Model.meshes[0].Material.FloatValues["material.transparency"] = 0.6f;
				rend2.Model.meshes[0].Material.FloatValues["material.shininess"] =256f;
			}
			GameObject lightObj = scene.CreateSceneObject("Light Object");
			Light light = lightObj.Components.Add<Light>();
			lightObj.transform.Position = new Vector3(1.5f, 1f, 1f);
			light.Diffuse = new Vector3(0f, 0.1f, 1f);
			light.Specular = new Vector3(1f,1f,1f);


			GameObject lightObj2 = scene.CreateSceneObject("Light Object 2");
			Light light2 = lightObj2.Components.Add<Light>();
			lightObj2.transform.Position = new Vector3(-1.5f, 1f, -1f);
			light2.Diffuse = new Vector3(1f, 0, 1f);
			light2.Specular = new Vector3(1f, 1f, 1f);

			SceneManager.SaveSceneToFile(scene, "SCENE.lscn");
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
			}
			SwapBuffers();
		}
		protected override void OnUnload()
		{
			base.OnUnload();
			shader.Dispose();
			lightShader.Dispose();
			OutlineShader.Dispose();
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
		}
		protected override void OnFocusedChanged(FocusedChangedEventArgs e)
		{
			base.OnFocusedChanged(e);
			changedFocus = true;
		}
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
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

			while(StartQueue.Count > 0)
			{
				StartQueue.Dequeue().Start();
			}

			if(SceneManager.ActiveScene!=null)
			{
				foreach(GameObject obj in SceneManager.ActiveScene.GameObjects)
				{
					obj.Update();
				}
			}
		}
		public bool IsPointInWindowBounds(Vector2i point)
		{
			return ClientRectangle.Contains(point, true);
		}
	}

}