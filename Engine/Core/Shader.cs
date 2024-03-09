using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Shader : IDisposable
{
	public int Handle;

	public Shader(string vertexPath, string fragmentPath)
	{
		int VertexShader, FragmentShader;
		string VertexShaderSource = File.ReadAllText(vertexPath);
		string FragmentShaderSource = File.ReadAllText(fragmentPath);

		VertexShader = GL.CreateShader(ShaderType.VertexShader);
		GL.ShaderSource(VertexShader, VertexShaderSource);

		FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
		GL.ShaderSource(FragmentShader, FragmentShaderSource);

		GL.CompileShader(VertexShader);

		GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
		if (success == 0)
		{
			string infoLog = GL.GetShaderInfoLog(VertexShader);
			Console.WriteLine(infoLog);
		}

		GL.CompileShader(FragmentShader);

		GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
		if (success == 0)
		{
			string infoLog = GL.GetShaderInfoLog(FragmentShader);
			Console.WriteLine(infoLog);
		}

		Handle = GL.CreateProgram();

		GL.AttachShader(Handle, VertexShader);
		GL.AttachShader(Handle, FragmentShader);

		GL.LinkProgram(Handle);

		GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
		if (success == 0)
		{
			string infoLog = GL.GetProgramInfoLog(Handle);
			Console.WriteLine(infoLog);
		}
		GL.DetachShader(Handle, VertexShader);
		GL.DetachShader(Handle, FragmentShader);
		GL.DeleteShader(FragmentShader);
		GL.DeleteShader(VertexShader);
	}

	public void Use()
	{
		GL.UseProgram(Handle);
	}

	private bool disposedValue = false;

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			GL.DeleteProgram(Handle);

			disposedValue = true;
		}
	}

	~Shader()
	{
		if (disposedValue == false)
		{
			Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
		}
	}

	public void SetInt(string name, int value)
	{
		int location = GL.GetUniformLocation(Handle, name);
		if (location != -1)
		{
			GL.Uniform1(location, value);
		}
		else
		{
			Console.WriteLine($"Uniform {name} not found in shader.");
		}
	}

	public void SetFloat(string name, float value)
	{
		int location = GL.GetUniformLocation(Handle, name);
		if (location != -1)
		{
			GL.Uniform1(location, value);
		}
		else
		{
			Console.WriteLine($"Uniform {name} not found in shader.");
		}
	}

	public void SetVector3(string name, Vector3 value)
	{
		int location = GL.GetUniformLocation(Handle, name);
		if (location != -1)
		{
			GL.Uniform3(location, value.X, value.Y, value.Z);
		}
		else
		{
			Console.WriteLine($"Uniform {name} not found in shader.");
		}
	}

	public void SetVector4(string name, Vector4 value)
	{
		int location = GL.GetUniformLocation(Handle, name);
		if (location != -1)
		{
			GL.Uniform4(location, value.X, value.Y, value.Z, value.W);
		}
		else
		{
			Console.WriteLine($"Uniform {name} not found in shader.");
		}
	}
	public void SetMatrix4(string name, Matrix4 value)
	{
		int location = GL.GetUniformLocation(Handle, name);
		if (location != -1)
		{
			GL.UniformMatrix4(location, false, ref value);
		}
		else
		{
			Console.WriteLine($"Uniform {name} not found in shader.");
		}
	}
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	public void Unuse()
	{
		GL.UseProgram(0);
	}
}