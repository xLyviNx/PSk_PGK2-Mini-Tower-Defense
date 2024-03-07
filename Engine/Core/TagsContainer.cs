namespace PGK2.Engine.Core
{
	public class TagsContainer
	{
		private List<string> All;
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
	}
}
