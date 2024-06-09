using OpenTK.Mathematics;
using PGK2.Engine.Core;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Components.Base
{
	/// <summary>
	/// Reprezentuje komponent kamery, który definiuje macierze widoku i projekcji do renderowania scen.
	/// </summary>
	[Serializable]
    public class CameraComponent : Component
    {
        private Matrix4 viewMatrix;
        private Matrix4 projectionMatrix;
		/// <summary>
		/// Priorytet kamery.
		/// </summary>
		public byte Priority = 0;

		/// <summary>
		/// Rozmiar obszaru rzutowania dla projekcji ortogonalnej. Określa wielkość obszaru widzenia kamery.
		/// </summary>
		public float OrthoSize = 10f;

		/// <summary>
		/// Minimalna odległość od kamery, do której obiekty będą renderowane. Określa, jak blisko obiekt musi znajdować się przy kamerze, aby był renderowany.
		/// </summary>
		public float NearClip = 0.1f;

		/// <summary>
		/// Maksymalna odległość od kamery, do której obiekty będą renderowane. Określa, jak daleko obiekt może znajdować się od kamery i nadal być renderowany.
		/// </summary>
		public float FarClip = 1000f;

		/// <summary>
		/// Aktywna kamera używana do renderowania sceny.
		/// </summary>
		[JsonIgnore]
		public static CameraComponent? activeCamera;

		/// <summary>
		/// Macierz widoku kamery.
		/// </summary>
		[JsonIgnore]
		public Matrix4 ViewMatrix
		{
			get { return viewMatrix; }
			set { viewMatrix = value; }
		}

		/// <summary>
		/// Macierz projekcji kamery.
		/// </summary>
		[JsonIgnore]
		public Matrix4 ProjectionMatrix
		{
			get { return projectionMatrix; }
			set { projectionMatrix = value; }
		}

		/// <summary>
		/// Określa, czy kamera używa projekcji ortogonalnej.
		/// </summary>
		public bool IsOrthographic { get; set; }

		/// <summary>
		/// Pole widzenia dla projekcji perspektywicznej (w stopniach).
		/// </summary>
		public float FieldOfView { get; set; }

		/// <summary>
		/// Kolor tła kamery.
		/// </summary>
		public Color4 BackgroundColor { get; set; }

		/// <summary>
		/// Obiekty, które kamera powinna uwzględniać podczas renderowania.
		/// </summary>
		public TagsContainer IncludeTags { get; internal set; }

		/// <summary>
		/// Obiekty, które kamera powinna pomijać podczas renderowania.
		/// </summary>
		public TagsContainer ExcludeTags { get; internal set; }

		/// <summary>
		/// Inicjalizuje nową instancję klasy <see cref="CameraComponent"/> z domyślnymi wartościami.
		/// </summary>
		public CameraComponent() : base()
        {
            viewMatrix = Matrix4.Identity;
            projectionMatrix = Matrix4.Identity;
            IsOrthographic = false;
            FieldOfView = 45.0f;
            BackgroundColor = Color4.Black;
			IncludeTags = new TagsContainer();
			ExcludeTags = new TagsContainer();
            Console.WriteLine("CAMERA CREATED");
            transform.Position = new Vector3(0.0f, 0.0f, 3.0f);
			OnSceneTransfer += sceneTransfer;
		}
		/// <summary>
		/// Metoda wywoływana przy przenoszeniu kamery między scenami.
		/// </summary>
		private void sceneTransfer(SceneSystem.Scene OldScene)
		{
            Console.WriteLine("SCENE TRANSFER");
			if (OldScene != null)
			{
				OldScene.Cameras.Remove(this);
			}
			if (MyScene != null && !MyScene.Cameras.Contains(this))
			{
				MyScene.Cameras.Add(this);
			}
			MyScene?.Cameras.Sort((x, y) => x.Priority.CompareTo(y.Priority));
		}

		/// <summary>
		/// Metoda wywoływana podczas niszczenia kamery.
		/// </summary>
		public override void OnDestroy()
        {
            base.OnDestroy();
			gameObject.MyScene = null;
			if (activeCamera == this)
                activeCamera = null;
        }
		/// <summary>
		/// Aktualizuje macierze widoku i projekcji kamery na podstawie jej komponentu transformacji oraz ustawień kamery.
		/// </summary>
		public void RenderUpdate()
        {
            if (EngineInstance.Instance == null || EngineInstance.Instance.window == null)
            {
                throw new Exception("[CAMERA] NO GAME ENGINE INSTANCE OR WINDOW REGISTERED");
            }
            if (activeCamera == null && EnabledInHierarchy)
            {
                activeCamera = this;
            }
            if (gameObject != null && gameObject.transform != null)
            {
                float aspectRatio = EngineInstance.Instance.window.aspectRatio;
                Vector3 eye = gameObject.transform.Position;
                Vector3 target = eye + gameObject.transform.Forward;
                Vector3 up = transform.Up;

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
