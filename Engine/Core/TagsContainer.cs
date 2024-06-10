using System.Text.Json.Serialization;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Struktura TagsContainer przechowuje kolekcję tagów (etykiet) powiązanych z obiektem.
	/// Tagi służą do kategoryzowania i wyszukiwania obiektów, albo do raycastowania lub rysowania.
	/// </summary>
	public struct TagsContainer
	{
		/// <summary>
		/// Lista wszystkich tagów przypisanych do obiektu.
		/// </summary>
		[JsonPropertyName("All")]
		public List<string> All { get; internal set; }

		/// <summary>
		/// Konstruktor domyślny inicjujący pustą listę tagów.
		/// </summary>
		public TagsContainer()
		{
			All = new List<string>();
		}

		/// <summary>
		/// Sprawdza, czy obiekt posiada dany tag.
		/// </summary>
		/// <param name="name">Nazwa tagu do sprawdzenia.</param>
		/// <returns>True jeżeli obiekt posiada tag, False w przeciwnym wypadku.</returns>
		public bool Has(string name)
		{
			return All.Contains(name);
		}

		/// <summary>
		/// Dodaje nowy tag do obiektu.
		/// </summary>
		/// <param name="tag">Nazwa tagu do dodania.</param>
		/// <returns>True jeżeli tag został dodany pomyślnie, False jeżeli obiekt już posiada taki tag.</returns>
		public bool Add(string tag)
		{
			if (Has(tag))
			{
				return false;
			}
			All.Add(tag);
			return true;
		}

		/// <summary>
		/// Sprawdza, czy bieżący kontener tagów posiada co najmniej jeden wspólny tag z innym kontenerem.
		/// </summary>
		/// <param name="tags">Inny kontener tagów do porównania.</param>
		/// <returns>True jeżeli istnieje co najmniej jeden wspólny tag, False w przeciwnym wypadku.</returns>
		public bool HasAny(TagsContainer tags)
		{
			foreach (string tag in All)
			{
				foreach (string otherTag in tags.All)
				{
					if (tag.Equals(otherTag))
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Liczba tagów przypisanych do obiektu (właściwość tylko do odczytu).
		/// </summary>
		[JsonIgnore]
		public int Count => All.Count;

		/// <summary>
		/// Liczba tagów przypisanych do obiektu (właściwość tylko do odczytu - alias dla Count).
		/// </summary>
		[JsonIgnore]
		public int Length => All.Count;

		/// <summary>
		/// Sprawdza, czy obiekt posiada jakiekolwiek tagi (właściwość tylko do odczytu).
		/// </summary>
		[JsonIgnore]
		public bool isEmpty => (Count == 0);
	}
}
