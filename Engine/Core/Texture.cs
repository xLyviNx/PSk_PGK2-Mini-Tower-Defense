using Assimp;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using System.IO;
using System.Numerics;

namespace PGK2.Engine.Core
{
	public struct Texture
	{
		public static Dictionary<string,Texture> textures_loaded = new();
		public int id;
		public string type;
		public string path;
		public bool transparency;
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