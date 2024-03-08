using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace PGK2.Engine.Core
{
	public class Texture
	{
		public int Handle { get; private set; }
		public Bitmap Bitmap { get; private set; }

		public Texture(string filePath)
		{
			// Ładowanie tekstury z pliku
			LoadFromFile(filePath);
		}

		public void Use(TextureUnit unit = TextureUnit.Texture0)
		{
			GL.ActiveTexture(unit);
			GL.BindTexture(TextureTarget.Texture2D, Handle);
		}

		private void LoadFromFile(string filePath)
		{
			// Wczytaj obrazek z pliku
			Bitmap = new Bitmap(filePath);

			// Wygeneruj identyfikator tekstury
			Handle = GL.GenTexture();

			// Ustaw parametry tekstury
			GL.BindTexture(TextureTarget.Texture2D, Handle);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
						  Bitmap.Width, Bitmap.Height, 0, PixelFormat.Bgra,
						  PixelType.UnsignedByte, Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height),
						  System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb));
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

			// Ustaw opcje filtrowania
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
		}

		// Dodatkowe metody do obsługi tekstur (np. usuwanie, zmiana parametrów itp.)
	}
}
