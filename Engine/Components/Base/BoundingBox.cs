using OpenTK.Mathematics;

namespace PGK2.Engine.Components.Base
{
	/// <summary>
	/// Klasa reprezentująca bounding box.
	/// </summary>
	public class BoundingBox
	{
		/// <summary>
		/// Minimalny punkt bounding boxa.
		/// </summary>
		public Vector3 Min { get; internal set; }

		/// <summary>
		/// Maksymalny punkt bounding boxa.
		/// </summary>
		public Vector3 Max { get; internal set; }

		/// <summary>
		/// Konstruktor klasy <see cref="BoundingBox"/>.
		/// </summary>
		/// <param name="min">Minimalny punkt.</param>
		/// <param name="max">Maksymalny punkt.</param>
		public BoundingBox(Vector3 min, Vector3 max)
		{
			Min = min;
			Max = max;
		}

		/// <summary>
		/// Aktualizuje bounding box na podstawie nowego punktu.
		/// </summary>
		/// <param name="point">Nowy punkt.</param>
		public void Update(Vector3 point)
		{
			Min = Vector3.ComponentMin(Min, point);
			Max = Vector3.ComponentMax(Max, point);
		}
	}
}
