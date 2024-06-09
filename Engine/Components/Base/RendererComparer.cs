using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.Engine.Components.Base
{
	public class RendererComparer : IComparer<ModelRenderer>
	{
		private Vector3 cameraPosition;

		public RendererComparer(Vector3 cameraPosition)
		{
			this.cameraPosition = cameraPosition;
		}

		public int Compare(ModelRenderer r1, ModelRenderer r2)
		{
			var bb1 = r1.GetBoundingBox();
			var bb2 = r2.GetBoundingBox();

			// Obliczanie centroidu dla każdego bounding boxa
			Vector3 centroid1 = (bb1.Min + bb1.Max) * 0.5f;
			Vector3 centroid2 = (bb2.Min + bb2.Max) * 0.5f;

			// Sprawdzenie, czy jeden bounding box zawiera się wewnątrz drugiego
			if (IsBoundingBoxInside(bb1, bb2))
			{
				return -1; // r1 jest wewnątrz r2, więc powinien być renderowany pierwszy
			}
			else if (IsBoundingBoxInside(bb2, bb1))
			{
				return 1; // r2 jest wewnątrz r1, więc powinien być renderowany pierwszy
			}
			else
			{
				// W przeciwnym razie porównujemy odległość od kamery
				float distance1 = (centroid1 - cameraPosition).LengthSquared;
				float distance2 = (centroid2 - cameraPosition).LengthSquared;
				return distance1.CompareTo(distance2);
			}
		}

		// Metoda pomocnicza do sprawdzania, czy jeden bounding box zawiera się w drugim
		private bool IsBoundingBoxInside(BoundingBox bb1, BoundingBox bb2)
		{
			return bb1.Min.X >= bb2.Min.X &&
				   bb1.Min.Y >= bb2.Min.Y &&
				   bb1.Min.Z >= bb2.Min.Z &&
				   bb1.Max.X <= bb2.Max.X &&
				   bb1.Max.Y <= bb2.Max.Y &&
				   bb1.Max.Z <= bb2.Max.Z;
		}
	}

}
