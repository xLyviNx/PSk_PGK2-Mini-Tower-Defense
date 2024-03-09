namespace PGK2.Engine.Core
{
	public class TagsContainer
	{
		protected List<string> All;
		public TagsContainer()
		{
			All = new();
		}
		public bool Has(string name)
		{ return All.Contains(name); }

		public bool Add(string tag)
		{
			if (Has(tag)) return false;
			All.Add(tag);
			return true;
		}
		public bool HasAny(TagsContainer tags)
		{
			foreach(string tag in All)
			{
				foreach (string otherTag in tags.All)
				{
					if(tag.Equals(otherTag)) return true;
				}
			}
			return false;
		}
		public int Count => All.Count;
		public int Length => All.Count;
		public bool isEmpty => (Count == 0);
	}
}
