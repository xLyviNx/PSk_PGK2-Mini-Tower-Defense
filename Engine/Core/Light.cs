using OpenTK.Mathematics;

namespace PGK2.Engine.Core
{
	public struct Light
	{
		public Vector3 Position;
		public Vector3 Ambient;
		public Vector3 Diffuse;
		public Vector3 Specular;

		public Light(Vector3 position, Vector3 ambient, Vector3 diffuse, Vector3 specular)
		{
			Position = position;
			Ambient = ambient;
			Diffuse = diffuse;
			Specular = specular;
		}
	}

}