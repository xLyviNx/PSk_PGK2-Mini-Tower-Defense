using OpenTK.Mathematics;

namespace PGK2.Engine.Components.Base
{
	public class BoundingBox
	{
		public Vector3 Min { get; internal set; }
		public Vector3 Max { get; internal set; }

		public BoundingBox(Vector3 min, Vector3 max)
		{
			Min = min;
			Max = max;
		}

		public void Update(Vector3 point)
		{
			Min = Vector3.ComponentMin(Min, point);
			Max = Vector3.ComponentMax(Max, point);
		}
	}
}
