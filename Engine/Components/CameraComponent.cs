using OpenTK;
using OpenTK.Mathematics;
using PGK2.Engine.Core;
using PGK2.Engine.Main;
using System.Collections.Generic;

namespace Game.Engine.Components
{
    /// <summary>
    /// Represents a camera component that defines the view and projection matrices for rendering scenes.
    /// </summary>
    public class CameraComponent : Component
	{
		private Matrix4 viewMatrix;
		private Matrix4 projectionMatrix;
		public static CameraComponent? activeCamera;

		/// <summary>
		/// Gets or sets the view matrix of the camera.
		/// </summary>
		public Matrix4 ViewMatrix
		{
			get { return viewMatrix; }
			set { viewMatrix = value; }
		}

		/// <summary>
		/// Gets or sets the projection matrix of the camera.
		/// </summary>
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
		public CameraComponent()
		{
			viewMatrix = Matrix4.Identity;
			projectionMatrix = Matrix4.Identity;
			IsOrthographic = false;
			FieldOfView = 45.0f;
			BackgroundColor = Color4.Black;
			RenderTags = new TagsContainer();
			Console.WriteLine("CAMERA CREATED");
		}
		
		/// <summary>
		/// Updates the camera's view and projection matrices based on its transform component.
		/// </summary>
		/// <param name="aspectRatio">The aspect ratio of the camera (width / height).</param>
		public void Update()
		{
			if (Engine.Instance == null || Engine.Instance.window == null)
			{
				throw new Exception("[CAMERA] NO GAME ENGINE INSTANCE OR WINDOW REGISTERED");
			}
			gameObject.transform.Position += Vector3.One * (float)Time.deltaTime * 10f;
			Console.WriteLine($"POS: {gameObject.transform.Position}");
			if (activeCamera == null && enabled)
			{
				activeCamera = this;
			}
			if (gameObject != null && gameObject.transform != null)
			{
				float aspectRatio = Engine.Instance.window.aspectRatio;
				Vector3 eye = gameObject.transform.Position;
				Vector3 target = eye + gameObject.transform.Forward;
				Vector3 up = gameObject.transform.Up;

				if (IsOrthographic)
				{
					float orthoSize = 10.0f; // You may adjust this value based on your scene size

					projectionMatrix = Matrix4.CreateOrthographic(orthoSize * aspectRatio, orthoSize, 0.1f, 1000.0f);
				}
				else
				{
					projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FieldOfView), aspectRatio, 0.1f, 1000.0f);
				}

				viewMatrix = Matrix4.LookAt(eye, target, up);
			}
		}
	}
}
