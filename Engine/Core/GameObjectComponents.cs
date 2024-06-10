using PGK2.Engine.Serialization.Converters;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Klasa reprezentująca komponenty obiektu gry.
	/// </summary>
	[Serializable]
    public class GameObjectComponents
    {
		/// <summary>
		/// Obiekt gry, do którego należą komponenty.
		/// </summary>
		internal GameObject gameObject;
		/// <summary>
		/// Lista wszystkich komponentów obiektu gry.
		/// </summary>
		[JsonConverter(typeof(ComponentListConverter))]
		public List<Component> All { get; internal set; }
		/// <summary>
		/// Konstruktor domyślny klasy. Inicjuje pustą listę komponentów.
		/// </summary>
		public GameObjectComponents()
		{
			All = new List<Component>();
		}
		/// <summary>
		/// Konstruktor klasy przypisujący obiekt gry do zarządzanych komponentów.
		/// Inicjuje pustą listę komponentów.
		/// </summary>
		/// <param name="gObj">Obiekt gry.</param>
		public GameObjectComponents(GameObject gObj)
		{
			gameObject = gObj;
			All = new List<Component>();
		}

		/// <summary>
		/// Metoda dodająca nowy komponent typu T do obiektu gry.
		/// Ustawia obiekt gry jako obiekt docelowy przypisania komponentu.
		/// Dodaje komponent do listy komponentów obiektu gry.
		/// Wywołuje metodę OnSceneTransfer dla nowo dodanego komponentu (jeśli istnieje).
		/// Dodaje komponent do kolejki uruchamiania komponentów w EngineWindow.
		/// Zwraca nowo utworzony komponent.
		/// </summary>
		/// <typeparam name="T">Typ komponentu do dodania (musi dziedziczyć po klasie Component i mieć konstruktor bezparametryczny).</typeparam>
		/// <returns>Nowo dodany komponent typu T.</returns>
		public T Add<T>() where T : Component, new()
        {
            Component.assigningComponentTo = gameObject;
			T newComponent = new T();
			All.Add(newComponent);
            newComponent.OnSceneTransfer?.Invoke(null);
			EngineWindow.StartQueue.Enqueue(newComponent);
			return newComponent;
        }
		/// <summary>
		/// Metoda pobierająca komponent typu T z obiektu gry.
		/// Przeszukuje listę komponentów obiektu gry w poszukiwaniu komponentu o typie T.
		/// Zwraca znaleziony komponent lub null, jeśli komponent nie istnieje.
		/// </summary>
		/// <typeparam name="T">Typ komponentu do pobrania.</typeparam>
		/// <returns>Znaleziony komponent typu T lub null.</returns>
		public T? Get<T>() where T : Component
        {
            foreach (var component in All)
            {
                if (component is T typedComponent)
                {
                    return typedComponent;
                }
            }
            return null;
        }
    }
}
