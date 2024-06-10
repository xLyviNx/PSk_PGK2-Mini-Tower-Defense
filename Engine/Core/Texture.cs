using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Struktura reprezentująca teksturę w aplikacji.
	/// </summary>
	public struct Texture
	{
		/// <summary>
		/// Słownik przechowujący załadowane tekstury.
		/// </summary>
		public static Dictionary<string, Texture> textures_loaded = new();

		/// <summary>
		/// Identyfikator tekstury.
		/// </summary>
		public int id;

		/// <summary>
		/// Typ tekstury.
		/// </summary>
		public string type;

		/// <summary>
		/// Ścieżka do pliku tekstury.
		/// </summary>
		public string path;

		/// <summary>
		/// Flaga określająca, czy tekstura zawiera przezroczystość.
		/// </summary>
		public bool transparency;

		/// <summary>
		/// Sprawdza, czy tekstura została już załadowana.
		/// </summary>
		/// <param name="path">Ścieżka do pliku tekstury.</param>
		/// <param name="texture">Znaleziony obiekt tekstury.</param>
		/// <returns>Zwraca <c>true</c> jeśli tekstura została już załadowana, w przeciwnym razie <c>false</c>.</returns>
		public static bool FindLoaded(string path, out Texture texture)
		{
			if (textures_loaded.ContainsKey(path))
			{
				texture = textures_loaded[path];
				return true;
			}
			texture = new();
			return false;
		}

		/// <summary>
		/// Ładuje teksturę z pliku.
		/// </summary>
		/// <param name="path">Ścieżka do pliku tekstury.</param>
		/// <param name="typeName">Typ tekstury.</param>
		/// <returns>Załadowana tekstura.</returns>
		public static Texture LoadFromFile(string path, string typeName)
		{
			bool exists = FindLoaded(path, out Texture texture);
			if (!exists)
			{
				Console.WriteLine($"Trying to load an image from ({path})");
				byte[] imageData = File.ReadAllBytes(path);
				ImageResult imageResult = ImageResult.FromMemory(imageData, ColorComponents.RedGreenBlueAlpha);

				// Check for transparency
				bool hasTransparency = false;
				for (int i = 0; i < imageResult.Data.Length; i += 4) // Assuming RGBA format
				{
					if (imageResult.Data[i + 3] < 255) // Alpha channel is less than 1
					{
						hasTransparency = true;
						break;
					}
				}

				if (hasTransparency)
				{
					Console.WriteLine("The texture has transparency.");
				}
				else
				{
					Console.WriteLine("The texture does not have transparency.");
				}
				texture.transparency = hasTransparency;
				texture.id = GL.GenTexture();
				texture.type = typeName;
				texture.path = path;
				GL.BindTexture(TextureTarget.Texture2D, texture.id);

				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, imageResult.Width, imageResult.Height, 0,
					OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, imageResult.Data);

				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			}
			return texture;
		}

	};
}