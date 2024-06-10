using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Klasa Material reprezentuje materiał używany do renderowania obiektów 3D.
	/// </summary>
	[Serializable]
	public class Material
	{
		/// <summary>
		/// Shader przypisany do materiału.
		/// </summary>
		public Shader Shader { get; set; }

		/// <summary>
		/// Słownik tekstur materiału, gdzie kluczem jest nazwa tekstury, a wartością obiekt Texture.
		/// </summary>
		public Dictionary<string, Texture> Textures { get; set; } = new Dictionary<string, Texture>();

		/// <summary>
		/// Słownik wartości zmiennoprzecinkowych materiału, gdzie kluczem jest nazwa uniforma, a wartością jej wartość.
		/// </summary>
		public Dictionary<string, float> FloatValues { get; set; } = new Dictionary<string, float>();

		/// <summary>
		/// Słownik wartości wektorowych 3D materiału, gdzie kluczem jest nazwa uniforma, a wartością wektor Vector3.
		/// </summary>
		public Dictionary<string, Vector3> Vector3Values { get; set; } = new Dictionary<string, Vector3>();

		/// <summary>
		/// Słownik wartości całkowitych materiału, gdzie kluczem jest nazwa uniforma, a wartością jej wartość.
		/// </summary>
		public Dictionary<string, int> IntValues { get; set; } = new Dictionary<string, int>();

		/// <summary>
		/// Słownik kolorów materiału w formacie RGBA, gdzie kluczem jest nazwa koloru, a wartością obiekt Color4.
		/// </summary>
		public Dictionary<string, Color4> ColorValues { get; set; } = new Dictionary<string, Color4>();

		/// <summary>
		/// Właściwość określająca czy materiał jest przezroczysty. 
		/// </summary>
		public bool HasTransparency
		{
			get
			{
				if (Shader.EnforceTransparencyPass) return true;
				if (FloatValues.ContainsKey("material.transparency") && FloatValues["material.transparency"] < 1f)
				{
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Konstruktor Materiału, który przyjmuje shader jako parametr.
		/// </summary>
		/// <param name="shader">Shader przypisywany do materiału.</param>
		public Material(Shader shader)
		{
			Shader = shader;
		}

		/// <summary>
		/// Metoda statyczna konwertująca kolor Assimp.Color4D do postaci Vector3.
		/// </summary>
		/// <param name="col">Kolor w formacie Assimp.Color4D.</param>
		/// <returns>Zwracany jest wektor Vector3 reprezentujący kolor.</returns>
		public static Vector3 AssimpColorToVec3(Assimp.Color4D col)
		{
			return new Vector3(col.R, col.G, col.B);
		}

		/// <summary>
		/// Konstruktor Materiału, który przyjmuje materiał Assimp jako parametr. Tworzy materiał na podstawie danych z materiału Assimp.
		/// </summary>
		/// <param name="material">Materiał Assimp.</param>
		public Material(Assimp.Material material)
		{
			Shader = EngineWindow.shader;

			this.Vector3Values.Add("material.ambient", material.HasColorAmbient ? AssimpColorToVec3(material.ColorAmbient) : Vector3.Zero);
			this.Vector3Values.Add("material.diffuse", material.HasColorDiffuse ? AssimpColorToVec3(material.ColorDiffuse) : Vector3.Zero);
			this.Vector3Values.Add("material.specular", material.HasColorSpecular ? AssimpColorToVec3(material.ColorSpecular) : Vector3.Zero);
			this.FloatValues.Add("material.shininess", material.HasShininess ? material.Shininess : 0f);
			this.FloatValues.Add("material.transparency", material.HasOpacity ? material.Opacity : 1f);
			this.IntValues.Add("specularAlwaysVisible", 1);
		}

		/// <summary>
		/// Metoda aktywująca materiał. Ustawia shader i parametry materiału w shaderze.
		/// </summary>
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

		/// <summary>
		/// Metoda dezaktywująca materiał. Wywołuje odpowiadającą metodę dla shadera.
		/// </summary>
		public void Unuse()
		{
			Shader.Unuse();
		}

		/// <summary>
		/// Metoda tworząca kopię materiału. Kopiuje wszystkie słowniki parametrów materiału.
		/// </summary>
		/// <returns>Zwracany jest nowy obiekt Material, będący kopią materiału.</returns>
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