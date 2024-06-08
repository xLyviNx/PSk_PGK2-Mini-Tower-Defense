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
		public Dictionary<string, int> IntValues { get; set; } = new Dictionary<string, int>();
		public Dictionary<string, Color4> ColorValues { get; set; } = new Dictionary<string, Color4>();
		public bool HasTransparency
		{
			get
			{
				if(Shader.EnforceTransparencyPass) return true;
				if (FloatValues.ContainsKey("material.transparency") && FloatValues["material.transparency"] < 1f)
				{
					return true;
				}
				return false;
			}
		}
		public Material(Shader shader)
		{
			Shader = shader;
		}

		public static Vector3 AssimpColorToVec3(Assimp.Color4D col)
		{
			return new Vector3(col.R, col.G, col.B);
		}
		public Material(Assimp.Material material)
		{
			Shader = EngineWindow.shader;

			this.Vector3Values.Add("material.ambient", material.HasColorAmbient? AssimpColorToVec3(material.ColorAmbient) : Vector3.Zero);
			this.Vector3Values.Add("material.diffuse", material.HasColorDiffuse? AssimpColorToVec3(material.ColorDiffuse) : Vector3.Zero);
			this.Vector3Values.Add("material.specular", material.HasColorSpecular? AssimpColorToVec3(material.ColorSpecular) : Vector3.Zero);
			this.FloatValues.Add("material.shininess", material.HasShininess? material.Shininess : 0f);
			this.FloatValues.Add("material.transparency", material.HasOpacity? material.Opacity : 1f);
			this.IntValues.Add("specularAlwaysVisible", 1);
		}

		public void Use()
		{
			Shader.Use();

			// Ustawienia dla tekstur
			int textureUnit = 0;
			foreach (var kvp in Textures)
			{
				GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
				GL.BindTexture(TextureTarget.Texture2D, kvp.Value.id);
				Shader.SetInt(kvp.Key, textureUnit);
				textureUnit++;
			}

			// Ustawienia dla wartości liczbowych
			foreach (var kvp in FloatValues)
			{
				Shader.SetFloat(kvp.Key, kvp.Value);
			}
				
			foreach (var kvp in IntValues)
			{
				Shader.SetInt(kvp.Key, kvp.Value);
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
		public Material Instantiate()
		{
			Material newMaterial = new Material(this.Shader);

			foreach (var kvp in this.Textures)
			{
				newMaterial.Textures.Add(kvp.Key, kvp.Value);
			}

			foreach (var kvp in this.FloatValues)
			{
				newMaterial.FloatValues.Add(kvp.Key, kvp.Value);
			}

			foreach (var kvp in this.Vector3Values)
			{
				newMaterial.Vector3Values.Add(kvp.Key, kvp.Value);
			}

			foreach (var kvp in this.IntValues)
			{
				newMaterial.IntValues.Add(kvp.Key, kvp.Value);
			}

			foreach (var kvp in this.ColorValues)
			{
				newMaterial.ColorValues.Add(kvp.Key, kvp.Value);
			}

			return newMaterial;
		}
	}
}