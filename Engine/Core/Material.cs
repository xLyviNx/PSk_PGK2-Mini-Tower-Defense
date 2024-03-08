using Assimp.Unmanaged;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace PGK2.Engine.Core
{
	[Serializable]
	public class Material
	{
		public Shader Shader { get; set; }
		public Dictionary<string, Texture> Textures { get; set; } = new Dictionary<string, Texture>();
		public Vector3 Color { get; set; } = Vector3.One;

		public Material(Shader shader)
		{
			Shader = shader;
		}

		public void Use()
		{
			// Ustaw odpowiednie wartości w shaderze
			Shader.Use();

			int textureIndex = 0;

			foreach (var kvp in Textures)
			{
				GL.ActiveTexture(TextureUnit.Texture0 + textureIndex);
				GL.BindTexture(TextureTarget.Texture2D, kvp.Value.Handle);
				Shader.SetInt(kvp.Key, textureIndex);
				textureIndex++;
			}

			Shader.SetVector3("materialColor", Color);
		}
	}
}
