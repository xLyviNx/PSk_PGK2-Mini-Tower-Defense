using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace PGK2.Engine.Core
{
	[Serializable]
	public class Material
	{
		public Shader Shader { get; set; }
		public Dictionary<string, Texture> Textures { get; set; } = new Dictionary<string, Texture>();
		public Dictionary<string, float> FloatValues { get; set; } = new Dictionary<string, float>();
		public Dictionary<string, Vector3> Vector3Values { get; set; } = new Dictionary<string, Vector3>();
		public Dictionary<string, Color4> ColorValues { get; set; } = new Dictionary<string, Color4>();

		public Material(Shader shader)
		{
			Shader = shader;
		}

		public Material(Assimp.Material material)
		{
			// Implementacja przekształcania Assimp.Material na Material
		}

		public void Use()
		{
			Shader.Use();

			// Ustawienia dla tekstur
			int textureUnit = 0;
			foreach (var kvp in Textures)
			{
				GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
				GL.BindTexture(TextureTarget.Texture2D, kvp.Value.Handle);
				Shader.SetInt(kvp.Key, textureUnit);
				textureUnit++;
			}

			// Ustawienia dla wartości liczbowych
			foreach (var kvp in FloatValues)
			{
				Shader.SetFloat(kvp.Key, kvp.Value);
			}

			// Ustawienia dla wartości wektorowych
			foreach (var kvp in Vector3Values)
			{
				Shader.SetVector3(kvp.Key, kvp.Value);
			}

			// Ustawienia dla kolorów
			foreach (var kvp in ColorValues)
			{
				Shader.SetVector4(kvp.Key, (Vector4)kvp.Value);
			}
		}
		public void Unuse()
		{
			Shader.Unuse();
		}
	}
}