using Game.Engine.Components;
using OpenTK;
using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;
using PGK2.Engine.Serialization.Converters;
using System.Drawing;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Components
{
	/// <summary>
	/// Represents a camera component that defines the view and projection matrices for rendering scenes.
	/// </summary>
	/// 
	[Serializable]
	public class CameraComponent : Component
	{
		private Matrix4 viewMatrix;
		private Matrix4 projectionMatrix;
		public byte Priority = 0;
		public float OrthoSize = 10f;
		public float NearClip = 0.1f;
		public float FarClip = 1000f;
		[JsonIgnore] public static CameraComponent? activeCamera;
		Vector3 oldrot;

		/// <summary>
		/// Gets or sets the view matrix of the camera.
		/// </summary>
		[JsonIgnore]
		public Matrix4 ViewMatrix
		{
			get { return viewMatrix; }
			set { viewMatrix = value; }
		}

		/// <summary>
		/// Gets or sets the projection matrix of the camera.
		/// </summary>
		[JsonIgnore]
		public Matrix4 ProjectionMatrix
		{
			get { return projectionMatrix; }
			set { projectionMatrix = value; }
		}

		/// <summary>
		/// Gets or sets whether the camera uses an orthographic projection.
		/// </summary>
		public bool IsOrthographic { get; set; }

		/// <summary>
		/// Gets or sets the field of view for perspective projection (in degrees).
		/// </summary>
		public float FieldOfView { get; set; }

		/// <summary>
		/// Gets or sets the background color of the camera.
		/// </summary>
		public Color4 BackgroundColor { get; set; }

		/// <summary>
		/// Gets or sets the tags that the camera should render.
		/// </summary>
		public TagsContainer RenderTags { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CameraComponent"/> class with default values.
		/// </summary>
		public CameraComponent() : base()
		{
			viewMatrix = Matrix4.Identity;
			projectionMatrix = Matrix4.Identity;
			IsOrthographic = false;
			FieldOfView = 45.0f;
			BackgroundColor = Color4.Black;
			RenderTags = new TagsContainer();
			Console.WriteLine("CAMERA CREATED");

			if (SceneManager.ActiveScene != null)
			{
				SceneManager.ActiveScene.Cameras.Add(this);
				SceneManager.ActiveScene.Cameras.Sort((x, y) => x.Priority.CompareTo(y.Priority));
			}
			transform.Position = new Vector3(0.0f, 0.0f, 3.0f);
		}
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (SceneManager.ActiveScene != null)
			{
				SceneManager.ActiveScene.Cameras.Remove(this);
				SceneManager.ActiveScene.Cameras.Sort((x, y) => x.Priority.CompareTo(y.Priority));
			}
			if (activeCamera == this)
				activeCamera = null;
		}
		public override void Update()
		{
			base.Update();
			//double tempX = Time.deltaTime;
			//Console.WriteLine($"TEMP X: {Time.deltaTime}");
			//tempX += Time.deltaTime;
			//Console.WriteLine($"NOW TEMP X: {tempX}");

			//gameObject.transform.Position -= transform.Forward * 0.5f;
			//transform.Rotation = Quaternion.FromEulerAngles(transform.Rotation.ToEulerAngles() + new Vector3(0,1f,0f));

			KeyboardState input = EngineInstance.Instance.window.KeyboardState;
			if (input.IsKeyDown(Keys.W))
			{
				transform.Position += transform.Forward * 2f * (float)Time.deltaTime; //Forward 
			}

			if (input.IsKeyDown(Keys.S))
			{
				transform.Position -= transform.Forward * 2f * (float)Time.deltaTime; //Backwards
			}

			if (input.IsKeyDown(Keys.A))
			{
				transform.Position -= transform.Right * 2f * (float)Time.deltaTime; //Left
			}

			if (input.IsKeyDown(Keys.D))
			{
				transform.Position += transform.Right * 2f * (float)Time.deltaTime; //Right
			}

			if (input.IsKeyDown(Keys.Space))
			{
				transform.Position += transform.Up * 5f * (float)Time.deltaTime; //Up 
			}

			if (input.IsKeyDown(Keys.LeftShift))
			{
				transform.Position -= transform.Up * 5f * (float)Time.deltaTime; //Down
			}

			if(input.IsKeyDown(Keys.Left))
			{
				transform.Rotation += new Vector3(0, 50, 0) * (float)Time.deltaTime;
			}
			if (input.IsKeyDown(Keys.Right))
			{
				transform.Rotation += new Vector3(0, -50, 0) * (float)Time.deltaTime;
			}
			if (input.IsKeyDown(Keys.Down))
			{
				transform.Rotation += new Vector3(50, 0, 0) * (float)Time.deltaTime;
			}
			if (input.IsKeyDown(Keys.Up))
			{
				transform.Rotation += new Vector3(-50, 0, 0) * (float)Time.deltaTime;
			}
			if (oldrot != transform.Position) 
				Console.WriteLine($"POS: {gameObject.transform.Position}");
			oldrot = transform.Position;
		}
		/// <summary>
		/// Updates the camera's view and projection matrices based on its transform component.
		/// </summary>
		/// <param name="aspectRatio">The aspect ratio of the camera (width / height).</param>
		public void RenderUpdate()
		{
			if (EngineInstance.Instance == null || EngineInstance.Instance.window == null)
			{
				throw new Exception("[CAMERA] NO GAME ENGINE INSTANCE OR WINDOW REGISTERED");
			}
			if (activeCamera == null && Enabled)
			{
				activeCamera = this;
			}
			if (gameObject != null && gameObject.transform != null)
			{
				float aspectRatio = EngineInstance.Instance.window.aspectRatio;
				Vector3 eye = gameObject.transform.Position;
				Vector3 target = eye + gameObject.transform.Forward;
				Vector3 up = gameObject.transform.Up;

				if (IsOrthographic)
				{
					projectionMatrix = Matrix4.CreateOrthographicOffCenter(
						-OrthoSize * aspectRatio, OrthoSize * aspectRatio,
						-OrthoSize, OrthoSize,
						NearClip, FarClip);
				}
				else
				{
					projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
						MathHelper.DegreesToRadians(FieldOfView), aspectRatio, NearClip, FarClip);
				}


				viewMatrix = Matrix4.LookAt(eye, target, up);
			}
		}
	}
}
