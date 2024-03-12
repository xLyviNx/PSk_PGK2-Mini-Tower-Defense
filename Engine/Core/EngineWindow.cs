using Assimp;
using Game.Engine.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PGK2.Engine.Components;
using PGK2.Engine.SceneSystem;

namespace PGK2.Engine.Core
{
    public class EngineWindow : GameWindow
	{
		Queue<int> frameQueue = new Queue<int>();
		double secTimer = 0d;
		long frames = 0;
		int VertexBufferObject;
		int VertexArrayObject;
		public static Shader shader;
		Renderer test;
		public float aspectRatio { get; private set; }
		public CameraComponent? activeCamera {get=>CameraComponent.activeCamera; }
		float[] vertices = {
				-0.5f, -0.5f, 0.0f, //Bottom-left vertex
				 0.5f, -0.5f, 0.0f, //Bottom-right vertex
				 0.0f,  0.5f, 0.0f  //Top vertex
			};
		public EngineWindow(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings()
			{ ClientSize = (width, height), Title = title })
		{
		
		}

		protected override void OnLoad()
		{
			base.OnLoad();
			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
			GL.Enable(EnableCap.DepthTest);
			VertexBufferObject = GL.GenBuffer();

			//Code goes here
			shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
			var vertexLocation = shader.GetAttribLocation("aPosition");
			GL.EnableVertexAttribArray(vertexLocation);
			GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

			var texCoordLocation = shader.GetAttribLocation("aTexCoord");
			GL.EnableVertexAttribArray(texCoordLocation);
			GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

			VertexArrayObject = GL.GenVertexArray();
			VertexBufferObject = GL.GenBuffer();
			SceneTest();

		}
		void SceneTest()
		{
			SceneSystem.Scene scene = new();
			SceneManager.LoadScene(scene);
			GameObject newObject = new("TEST OBJECT");
			newObject.Components.Add<CameraComponent>();
			newObject.Components.Add<TestComponent>();
			scene.GameObjects.Add(newObject);

			Mesh mesh = Mesh.LoadFromFile("Models/cube.fbx");
			Console.WriteLine($"Loaded Mesh: {mesh.Vertices.Count} VERTS");

			GameObject newObject2 = new("RENDER OBJECT");
			newObject2.Components.Add<TestComponent>();
			test = newObject2.Components.Add<MeshRenderer>();
			//newObject2.Components.Get<MeshRenderer>().Mesh = mesh;
			//Console.WriteLine("MATERIALS : " + newObject2.Components.Get<MeshRenderer>().Materials.Count);
			scene.GameObjects.Add(newObject2);

			SceneManager.SaveSceneToFile(scene, "SCENE.lscn");
		}
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);
			aspectRatio = (float)ClientSize.X / ClientSize.Y;
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);          
			//Console.WriteLine($"CAMERA: {(activeCamera != null ? activeCamera.gameObject.name : "NULL")}");

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
						r.CallRender(activeCamera);
					}
				}
			}
			shader.Use();

			shader.SetMatrix4("model", test.transform.GetModelMatrix());
			shader.SetMatrix4("view", activeCamera.ViewMatrix);
			shader.SetMatrix4("projection", activeCamera.ProjectionMatrix);
			DrawTest();

			SwapBuffers();
		}
		private void DrawTest()
		{
			GL.BindVertexArray(VertexArrayObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

			// BufferData only needs to be called once, assuming vertices don't change
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);

			GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 3);
		}

		protected override void OnUnload()
		{
			base.OnUnload();
			shader.Dispose();
			GL.DeleteVertexArray(VertexArrayObject);
			GL.DeleteBuffer(VertexBufferObject);
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);
			
			GL.Viewport(0, 0, e.Width, e.Height);
		}
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			frames++;
			Time.deltaTime = e.Time;
			if (secTimer < 1f)
			{
				secTimer += Time.deltaTime;
			}
			else
			{
				Console.WriteLine($"FPS: {frames}");
				frames = 0;
				secTimer = 0f;
			}


			if(SceneManager.ActiveScene!=null)
			{
				foreach(GameObject obj in SceneManager.ActiveScene.GameObjects)
				{
					obj.Update();
				}
			}

		}
	}
}