using Game.Engine.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using PGK2.Engine.SceneSystem;

namespace PGK2.Engine.Core
{
    public class EngineWindow : GameWindow
	{
		Queue<int> frameQueue = new Queue<int>();
		double secTimer = 0d;
		long frames = 0;
		float fps;
		int VertexBufferObject;
		Shader shader;
		public float aspectRatio { get; private set; }
		public CameraComponent? activeCamera {get=>CameraComponent.activeCamera; }
		public EngineWindow(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings()
			{ ClientSize = (width, height), Title = title })
		{
		
		}

		protected override void OnLoad()
		{
			base.OnLoad();

			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
			VertexBufferObject = GL.GenBuffer();

			//Code goes here
			shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

			Scene scene = new Scene();
			GameObject newObject = new("TEST OBJECT");
			newObject.Components.Add<CameraComponent>();
			scene.GameObjects.Add(newObject);
			SceneManager.SaveSceneToFile(scene, "SCENE.lscn");

		}
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);
			aspectRatio = (float)ClientSize.X / ClientSize.Y;
			GL.Clear(ClearBufferMask.ColorBufferBit);
			Console.WriteLine($"CAMERA: {(activeCamera != null ? activeCamera.gameObject.name : "NULL")}");
			shader.Use();

			if (activeCamera != null)
			{
				activeCamera.Update();
				int viewLocation = GL.GetUniformLocation(shader.Handle, "view");
				Matrix4 viewMatrix = activeCamera.ViewMatrix;
				GL.UniformMatrix4(viewLocation, false, ref viewMatrix);

				int projectionLocation = GL.GetUniformLocation(shader.Handle, "projection");
				Matrix4 projectionMatrix = activeCamera.ProjectionMatrix;
				GL.UniformMatrix4(projectionLocation, false, ref projectionMatrix);
			}

			DrawTest();

			SwapBuffers();
		}
		private void DrawTest()
		{
			float[] vertices = {
				-0.5f, -0.5f, 0.0f, //Bottom-left vertex
				 0.5f, -0.5f, 0.0f, //Bottom-right vertex
				 0.0f,  0.5f, 0.0f  //Top vertex
			};
			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
			int VertexArrayObject = GL.GenVertexArray();
			GL.BindVertexArray(VertexArrayObject);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);

			GL.BindVertexArray(VertexArrayObject);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

		}

		protected override void OnUnload()
		{
			base.OnUnload();
			shader.Dispose();
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
			long now = DateTime.Now.Ticks;
			double dT = (now - Time.lastTime) / 10_000_000f;
			Time.lastTime = now;
			Time.deltaTime = dT;
			if (secTimer < 1f)
			{
				secTimer += dT;
			}
			else
			{
				Console.WriteLine($"FPS: {frames}");
				frames = 0;
				secTimer = 0f;
			}
		}
	}
}