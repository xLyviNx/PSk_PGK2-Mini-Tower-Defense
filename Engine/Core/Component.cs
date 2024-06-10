using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.SceneSystem;
using PGK2.Engine.Serialization.Converters;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Abstrakcyjna klasa bazowa dla komponentów.
	/// </summary>
	[Serializable]
	[JsonDerivedType(typeof(CameraComponent))]
	public abstract class Component
    {
		/// <summary>
		/// Flaga wskazująca, czy metoda Awake została wywołana.
		/// </summary>
		internal bool CalledAwake = false;

		/// <summary>
		/// Obiekt, do którego przypisywany jest komponent przy tworzeniu.
		/// </summary>
		internal static GameObject? assigningComponentTo;

		/// <summary>
		/// Obiekt gry, do którego należy komponent.
		/// </summary>
		[JsonIgnore] public GameObject gameObject;

		/// <summary>
		/// Scena, do której należy obiekt komponentu.
		/// </summary>
		[JsonIgnore] public SceneSystem.Scene MyScene => gameObject.MyScene;

		/// <summary>
		/// Kolekcja komponentów obiektu gry.
		/// </summary>
		[JsonIgnore] public GameObjectComponents Components => gameObject.Components;

		/// <summary>
		/// Komponent transformacji obiektu gry.
		/// </summary>
		[JsonIgnore] public TransformComponent transform => gameObject.transform;

		/// <summary>
		/// Flaga wskazująca, czy komponent jest włączony.
		/// </summary>
		public bool EnabledSelf = true;

		/// <summary>
		/// Flaga wskazująca, czy komponent jest włączony w hierarchii obiektów.
		/// </summary>
		[JsonIgnore]
		public bool EnabledInHierarchy
        {
            get
            {
                return EnabledSelf && gameObject != null && gameObject.IsActive;
            }
        }

		/// <summary>
		/// Delegat wywoływany podczas transferu sceny.
		/// </summary>
		[JsonIgnore] public Action<SceneSystem.Scene?> OnSceneTransfer = delegate { };
		/// <summary>
		/// Pobiera komponent typu T z obiektu gry.
		/// </summary>
		/// <typeparam name="T">Typ komponentu do pobrania.</typeparam>
		/// <returns>Komponent typu T, jeśli istnieje; w przeciwnym razie null.</returns>
		public T? GetComponent<T>() where T : Component
		{
			return gameObject.Components.Get<T>();
		}
		/// <summary>
		/// Konstruktor klasy Component.
		/// </summary>
		public Component()
		{
			if (assigningComponentTo != null)
				gameObject = assigningComponentTo;
			else if (DeserializeContext.CurrentContext != null)
				gameObject = DeserializeContext.CurrentContext.GameObject;

			gameObject.OnSceneTransfer += OnSceneTransfer;
			assigningComponentTo = null;
		}

		/// <summary>
		/// Metoda wywoływana co klatkę.
		/// </summary>
		public virtual void Update()
		{

		}

		/// <summary>
		/// Metoda wywoływana przy inicjalizacji komponentu.
		/// </summary>
		public virtual void Awake()
		{

		}

		/// <summary>
		/// Metoda wywoływana po inicjalizacji wszystkich komponentów.
		/// </summary>
		public virtual void Start()
		{

		}

		/// <summary>
		/// Metoda wywoływana przy niszczeniu komponentu.
		/// </summary>
		public virtual void OnDestroy()
		{

		}

		/// <summary>
		/// Metoda wywoływana, gdy komponent zostaje włączony.
		/// </summary>
		public virtual void OnEnable()
		{

		}

		/// <summary>
		/// Metoda wywoływana, gdy komponent zostaje wyłączony.
		/// </summary>
		public virtual void OnDisable()
		{

		}
	}
}
