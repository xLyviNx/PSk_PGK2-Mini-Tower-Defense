using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Struktura reprezentująca wierzchołek mesha.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct MeshVertex
	{
		/// <summary>
		/// Pozycja wierzchołka w przestrzeni 3D.
		/// </summary>
		public Vector3 Position { get; set; }

		/// <summary>
		/// Wektor normalny do powierzchni w wierzchołku.
		/// </summary>
		public Vector3 Normal { get; set; }

		/// <summary>
		/// Współrzędne tekstury dla wierzchołka.
		/// </summary>
		public Vector2 TexCoords { get; set; }

		/// <summary>
		/// Zwraca rozmiar struktury MeshVertex w bajtach.
		/// </summary>
		public static int SizeInBytes => Marshal.SizeOf<MeshVertex>();

		/// <summary>
		/// Konstruktor inicjujący strukturę MeshVertex.
		/// </summary>
		/// <param name="position">Pozycja wierzchołka.</param>
		/// <param name="normal">Wektor normalny do powierzchni w wierzchołku.</param>
		/// <param name="texCoord">Współrzędne tekstury dla wierzchołka.</param>
		public MeshVertex(Vector3 position, Vector3 normal, Vector2 texCoord)
		{
			Position = position;
			Normal = normal;
			TexCoords = texCoord;
		}
	}
}
