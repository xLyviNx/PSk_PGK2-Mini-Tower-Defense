using PGK2.Engine.Components.Base;
using PGK2.Engine.SceneSystem;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Klasa reprezentująca obiekt gry w silniku.
	/// </summary>
	[Serializable]
    public class GameObject
	{
		/// <summary>
		/// Unikalny identyfikator obiektu gry.
		/// </summary>
		[JsonInclude] public Guid Id;
		/// <summary>
		/// Nazwa obiektu gry.
		/// </summary>
		[JsonIgnore] public string name;
		/// <summary>
		/// Scena, do której należy obiekt gry.
		/// </summary>
		[JsonIgnore] private SceneSystem.Scene? _myscene;
		/// <summary>
		/// Scena, do której należy obiekt gry.
		/// Ustawienie sceny powoduje przeniesienie obiektu gry pomiędzy scenami.
		/// </summary>
		[JsonIgnore]
        public SceneSystem.Scene? MyScene
        {
            get => _myscene;
            set
            {
                if(ReferenceEquals(_myscene, value) == false)
                {
                    Scene? oldScene = _myscene;
                    if(_myscene!=null)
                    {
                        _myscene.GameObjects.Remove(this);
                    }
                    if (value != null)
                        if (!value.GameObjects.Contains(this) && !value.AwaitingGameObjects.Contains(this))
                        {
                            value.AwaitingGameObjects.Add(this);
                        }
					_myscene = value;
					OnSceneTransfer.Invoke(oldScene);
				}
			}
		}
		/// <summary>
		/// Flaga informująca czy obiekt gry został zniszczony (właściwość wewnętrzna).
		/// </summary>
		private bool _isdestroyed = false;
		/// <summary>
		/// Delegata wywoływana podczas przenoszenia obiektu gry pomiędzy scenami.
		/// </summary>
		[JsonIgnore] public Action<SceneSystem.Scene?> OnSceneTransfer = delegate { };
		/// <summary>
		/// Właściwość tylko do odczytu informująca czy obiekt gry został zniszczony.
		/// </summary>
		[JsonIgnore] public bool isDestroyed { get => _isdestroyed; }

		/// <summary>
		/// Komponenty obiektu.
		/// </summary>
		public GameObjectComponents Components;
		/// <summary>
		/// Komponent transformacji obiektu.
		/// </summary>
		public TransformComponent transform;
		/// <summary>
		/// Kontener tagów obiektu gry.
		/// </summary>
		public TagsContainer Tags { get; internal set; }
		/// <summary>
		/// Właściwość mówiąca czy obiekt jest aktywny (tylko ten obiekt)
		/// </summary>
		public bool IsActiveSelf { get; internal set; } = true;
		/// <summary>
		/// Właściwość mówiąca czy obiekt jest aktywny w scenie (biorąc pod uwagę jego rodzica)
		/// </summary>
		[JsonIgnore] public bool IsActive
        {
            get
            {
                return IsActiveSelf && transform != null && (transform.Parent == null || transform.Parent.gameObject != null && transform.Parent.gameObject.IsActive);
            }
            set
            {
                IsActiveSelf = value;
            }
        }
		/// <summary>
		/// Metoda pobierająca komponent typu T z obiektu gry.
		/// Przekazuje żądanie do komponentów obiektu gry.
		/// </summary>
		/// <typeparam name="T">Typ komponentu do pobrania.</typeparam>
		/// <returns>Znaleziony komponent lub null jeżeli nie istnieje.</returns>
		public T? GetComponent<T>() where T : Component
		{
			return Components.Get<T>();
		}
		/// <summary>
		/// Metoda dodająca komponent typu T do obiektu gry.
		/// Przekazuje żądanie do komponentów obiektu gry.
		/// </summary>
		/// <typeparam name="T">Typ komponentu do dodania (musi dziedziczyć po klasie Component i mieć konstruktor bezparametryczny).</typeparam>
		/// <returns>Nowo dodany komponent.</returns>
		public T? AddComponent<T>() where T : Component, new()
		{
			return Components.Add<T>();
		}
		/// <summary>
		/// Konstruktor domyślny obiektu gry. Tworzy obiekt o nazwie "GameObject".
		/// </summary>
		public GameObject() : this("GameObject") { }
		/// <summary>
		/// Konstruktor obiektu gry. Tworzy obiekt o podanej nazwie.
		/// </summary>
		/// <param name="name">Nazwa obiektu gry.</param>
		public GameObject(string name) : this(name, Guid.NewGuid()) { }
		/// <summary>
		/// Konstruktor obiektu gry. Tworzy obiekt o podanej nazwie i identyfikatorze Guid.
		/// </summary>
		/// <param name="name">Nazwa obiektu gry.</param>
		/// <param name="GUID">Unikalny identyfikator Guid obiektu gry.</param>
		public GameObject(string name, Guid GUID)
        {
            Components = new GameObjectComponents(this);
            transform = Components.Add<TransformComponent>();
            Tags = new();
            this.name = name;
			Id = GUID;
            if (MyScene == null)
                MyScene = SceneManager.ActiveScene;
		}
		/// <summary>
		/// Metoda aktualizująca obiekt gry (wywoływana w każdej klatce).
		/// Jeżeli obiekt gry jest nieaktywny metoda nie wykonuje żadnej akcji.
		/// Przechodzi po wszystkich komponentach obiektu i wywołuje dla nich metodę Update.
		/// </summary>
		public void Update()
        {
            if (!IsActive) return;
            foreach(Component c in Components.All)
            {
                if (!c.EnabledInHierarchy) continue;
                if (isDestroyed) return;
                c.Update();
				if (isDestroyed) return;
			}
		}
		/// <summary>
		/// Metoda niszcząca obiekt gry.
		/// Jeżeli obiekt jest już zniszczony metoda nie wykonuje żadnej akcji.
		/// Wyłącza wszystkie komponenty obiektu i wywołuje dla nich metodę OnDestroy.
		/// Usuwa wszystkie komponenty z obiektu.
		/// Przenosi obiekt na listę obiektów do usunięcia w scenie.
		/// Niszczy wszystkie obiekty potomne transform obiektu.
		/// Flaguje obiekt jako zniszczony.
		/// </summary>
		public void Destroy()
        {
            if (_isdestroyed || Components == null) return;
            foreach (var component in Components.All)
            {
                component.EnabledSelf = false;
                component.OnDestroy();
            }
            Components.All.Clear();
            if (MyScene != null)
            {
				MyScene.RemovingGameObjects.Add(this);
            }
            foreach(TransformComponent child in transform.Children.AllObjects)
            {
                child.gameObject.Destroy();
            }
            _isdestroyed = true;
        }
		/// <summary>
		/// Metoda zwracająca tekstową reprezentację obiektu gry.
		/// Jeżeli obiekt nie jest null zwraca informacje o nazwie i identyfikatorze Guid.
		/// Jeżeli obiekt jest zniszczony zwraca ciąg "null (destroyed)".
		/// W przypadku innych przypadków zwraca ciąg "null".
		/// </summary>
		/// <returns>Tekstowa reprezentacja obiektu gry.</returns>
		public override string ToString()
		{
            if (this != null)
                return $"GameObject ({name} - {Id})";
            else if (this._isdestroyed)
                return "null (destroyed)";
            else
                return "null";
        }
		/// <summary>
		/// Przeciążenie operatora == dla porównywania obiektów gry.
		/// Porównuje obiekty pod kątem referencji i identyfikatora Guid.
		/// </summary>
		/// <param name="x">Lewy operand.</param>
		/// <param name="y">Prawy operand.</param>
		/// <returns>True jeżeli obiekty są takie same, False w przeciwnym wypadku.</returns>
		public static bool operator ==(GameObject? x, GameObject? y)
		{
			//Console.WriteLine($"EQUALS CHECK\n - X null? {ReferenceEquals(x, null)}\n - Y null? {ReferenceEquals(y, null)}");

            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null)) //x==null
            {
                if (!ReferenceEquals(y, null)) //y!=null
					return y._isdestroyed;
            }
            else if (ReferenceEquals(y, null))
                    return x._isdestroyed;

            if (ReferenceEquals(x, null) && !ReferenceEquals(y, null))
                return false;  
            if (ReferenceEquals(y, null) && !ReferenceEquals(x, null))
                return false;

			if (x.isDestroyed && y.isDestroyed)
				return true;

			return x.Id == y.Id;
		}
		/// <summary>
		/// Przeciążenie operatora != dla porównywania obiektów gry.
		/// Neguje wynik operatora ==.
		/// </summary>
		/// <param name="x">Lewy operand.</param>
		/// <param name="y">Prawy operand.</param>
		/// <returns>True jeżeli obiekty są różne, False w przeciwnym wypadku.</returns>
		public static bool operator !=(GameObject? x, GameObject? y)
		{
			return !(x == y);
		}

		/// <summary>
		/// Metoda zwracająca kod hash obiektu gry.
		/// Zwraca kod hash identyfikatora Guid obiektu gry, jeżeli nie jest null i nie został zniszczony.
		/// W przeciwnym wypadku zwraca 0.
		/// </summary>
		/// <param name="obj">Obiekt gry.</param>
		/// <returns>Kod hash obiektu gry.</returns>
		public int GetHashCode(GameObject? obj)
		{
			if (ReferenceEquals(obj, null) || obj.isDestroyed)
				return 0;

			return obj.Id.GetHashCode();
		}
		/// <summary>
		/// Przeciążenie metody Equals dla porównywania obiektów gry.
		/// Wywołuje operator ==.
		/// </summary>
		/// <param name="obj">Obiekt do porównania.</param>
		/// <returns>True jeżeli obiekty są takie same, False w przeciwnym wypadku.</returns>
		public override bool Equals(object? obj) => this == obj;
		/// <summary>
		/// Przeciążenie metody GetHashCode dla pobrania kodu hash obiektu gry.
		/// Wywołuje statyczną metodę GetHashCode.
		/// </summary>
		/// <returns>Kod hash obiektu gry.</returns>
		public override int GetHashCode()
		{
			if (this==null)
				return 0;

			return Id.GetHashCode();
		}
	}
}
