using System.Text.Json.Serialization;

namespace PGK2.Engine.Components.Base
{
    [Serializable]
    public class ChildrenContainer
    {
        [JsonIgnore] public List<TransformComponent> AllObjects;
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

        public bool Remove(TransformComponent child)
        {
            if (!Has(child)) return false;
            AllObjects.Remove(child);
            return true;
        }
        public bool Add(TransformComponent child)
        {
            if (Has(child)) return false;
            AllObjects.Add(child);
            return true;
        }
        public bool Has(TransformComponent child)
        {
            return AllObjects.Contains(child);
        }
        public ChildrenContainer()
        {
            AllObjects = new();
        }
    }
}
