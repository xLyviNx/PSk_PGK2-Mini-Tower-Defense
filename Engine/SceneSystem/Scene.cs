using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using System.Text.Json.Serialization;
using PGK2.Engine.Components;
namespace PGK2.Engine.SceneSystem
{
	/// <summary>
	/// Klasa reprezentująca scenę w silniku gry.
	/// </summary>
	[Serializable]
	public class Scene
	{

		/// <summary>
		/// Nazwa sceny.
		/// </summary>
		public string SceneName = "Unnamed Scene";

		/// <summary>
		/// Lista obiektów sceny.
		/// </summary>
		public List<GameObject> GameObjects { get; internal set; }
		/// <summary>
		/// Lista obiektów sceny czekających na dodanie po zakończeniu klatki.
		/// </summary>
		internal List<GameObject> AwaitingGameObjects { get; set; }
		/// <summary>
		/// Lista obiektów sceny czekających na usunięcie po zakończeniu klatki.
		/// </summary>
		internal List<GameObject> RemovingGameObjects { get; set; }

		/// <summary>
		/// Lista kamer w scenie.
		/// </summary>
		[JsonIgnore] public List<CameraComponent> Cameras { get; internal set; }

		/// <summary>
		/// Lista rendererów w scenie.
		/// </summary>
		[JsonIgnore] public List<Renderer> Renderers { get; internal set; }

		/// <summary>
		/// Lista rendererów interfejsu użytkownika (GUI) w scenie.
		/// </summary>
		[JsonIgnore] public List<UI_Renderer> UI_Renderers { get; internal set; }

		/// <summary>
		/// Lista świateł w scenie.
		/// </summary>
		[JsonIgnore] public List<Light> Lights { get; internal set; }

		/// <summary>
		/// Metoda dodająca oczekujące obiekty do listy obiektów sceny.
		/// </summary>
		internal void AddAwaitingObjects()
		{
			List<GameObject> toadd = new();
			foreach (GameObject obj in AwaitingGameObjects)
				if(!obj.isDestroyed && obj!=null)
					toadd.Add(obj);
			GameObjects.InsertRange(GameObjects.Count, toadd);
			AwaitingGameObjects.Clear();
		}
		/// <summary>
		/// Metoda usuwająca oczekujące obiekty z listy obiektów sceny.
		/// </summary>
		internal void RemoveAwaitingObjects()
		{
			foreach (GameObject obj in RemovingGameObjects)
				GameObjects.Remove(obj);
			RemovingGameObjects.Clear();
		}

		/// <summary>
		/// Konstruktor sceny, inicjalizuje listy obiektów, kamer, rendererów, rendererów UI i świateł.
		/// </summary>
		public Scene()
		{
			AwaitingGameObjects = new List<GameObject>();
			RemovingGameObjects = new List<GameObject>();
			GameObjects = new List<GameObject>();
			Cameras = new();
			Renderers = new();
			UI_Renderers = new();
			Lights = new();
		}
		/// <summary>
		/// Metoda tworząca nowy obiekt w scenie.
		/// </summary>
		/// <param name="ObjectName">Nazwa nowego obiektu.</param>
		/// <returns>Nowo utworzony obiekt.</returns>
		public GameObject CreateSceneObject(string ObjectName = "GameObject")
		{
			GameObject gameObject = new GameObject(ObjectName);
			gameObject.MyScene = this;

			return gameObject;
		}
		/// <summary>
		/// Metoda znajdująca pierwszy obiekt o określonym typie komponentu w scenie.
		/// </summary>
		/// <typeparam name="T">Typ komponentu.</typeparam>
		/// <param name="onlyActive">Czy wyszukiwać tylko aktywne obiekty.</param>
		/// <returns>Obiekt komponentu lub null, jeśli nie znaleziono.</returns>
		public T? FindObjectOfType<T>(bool onlyActive = false) where T : Component
		{
			foreach (var gameObject in GameObjects)
			{
				if (onlyActive && !gameObject.IsActive)
					continue;

				var component = gameObject.Components.Get<T>();
				if (component != null && component.EnabledInHierarchy)
					return component;
			}

			return null;
		}
		/// <summary>
		/// Metoda znajdująca obiekt o określonej nazwie w scenie.
		/// </summary>
		/// <param name="name">Nazwa obiektu do znalezienia.</param>
		/// <param name="onlyActive">Czy wyszukiwać tylko aktywne obiekty.</param>
		/// <returns>Obiekt o określonej nazwie lub null, jeśli nie znaleziono.</returns>
		public GameObject FindObjectByName(string name, bool onlyActive = false)
		{
			foreach (var gameObject in GameObjects)
			{
				if (onlyActive && !gameObject.IsActive)
					continue;
				if(gameObject.name.Trim() == name.Trim())
					return gameObject;
			}
			return null;
		}
		/// <summary>
		/// Metoda znajdująca wszystkie obiekty o określonym typie komponentu w scenie.
		/// </summary>
		/// <typeparam name="T">Typ komponentu.</typeparam>
		/// <param name="onlyActive">Czy wyszukiwać tylko aktywne obiekty.</param>
		/// <returns>Lista obiektów o określonym typie komponentu.</returns>
		public List<T> FindAllObjectsOfType<T>(bool onlyActive = true) where T : Component
		{
			List<T> result = new List<T>();

			foreach (var gameObject in GameObjects)
			{
				if (onlyActive && !gameObject.IsActive)
					continue;

				var component = gameObject.Components.Get<T>();
				if (component != null)
					result.Add(component);
			}

			return result;
		}
	}
}
