using System.Text.Json.Serialization;

namespace PGK2.Engine.Components.Base
{
	/// <summary>
	/// Klasa przechowująca potomków.
	/// </summary>
	[Serializable]
    public class ChildrenContainer
    {
        [JsonIgnore] public List<TransformComponent> AllObjects;
        [JsonInclude]
        internal List<Guid> _loaded = new();

		/// <summary>
		/// Lista identyfikatorów wszystkich potomków.
		/// </summary>
		[JsonInclude]
        public List<Guid> All
        {
            get
            {
                List<Guid> guids = new();
                foreach (TransformComponent transform in AllObjects)
                {
                    Console.WriteLine($"{transform.gameObject}");
                    guids.Add(transform.gameObject.Id);
                }
                return guids;
            }
        }
		/// <summary>
		/// Usuwa potomka z listy.
		/// </summary>
		public bool Remove(TransformComponent child)
        {
            if (!Has(child)) return false;
            AllObjects.Remove(child);
            return true;
        }
		/// <summary>
		/// Dodaje potomka do listy.
		/// </summary>
		public bool Add(TransformComponent child)
        {
            if (Has(child)) return false;
            AllObjects.Add(child);
            return true;
        }
		/// <summary>
		/// Sprawdza, czy dany potomek znajduje się na liście.
		/// </summary>
		public bool Has(TransformComponent child)
        {
            return AllObjects.Contains(child);
        }
		/// <summary>
		/// Konstruktor klasy.
		/// </summary>
		public ChildrenContainer()
        {
            AllObjects = new();
        }
    }
}
