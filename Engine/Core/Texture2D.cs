using System;
using System.Drawing;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace PGK2.Engine.Core
{
	public class Texture
	{
		public int Handle { get; private set; }
		public TextureMinFilter filtering = TextureMinFilter.Linear;

		public Texture(string filePath)
		{
			// Load the texture from a file
			LoadFromFile(filePath);
		}

		public void Use(TextureUnit unit = TextureUnit.Texture0)
		{
			GL.ActiveTexture(unit);
			GL.BindTexture(TextureTarget.Texture2D, Handle);
		}

		private void LoadFromFile(string filePath)
		{
			StbImage.stbi_set_flip_vertically_on_load(1);

			// Load the image.
			ImageResult image = ImageResult.FromStream(File.OpenRead(filePath), ColorComponents.RedGreenBlueAlpha);
			Handle = GL.GenTexture();
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

			// Set filtering options
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)filtering);
		}
	}
}
