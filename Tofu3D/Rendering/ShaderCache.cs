namespace Tofu3D;

public static class ShaderCache
{
	public static int ShaderInUse = -1;
	public static int VaoInUse = -100;

	public static void BindVao(int vao)
	{
		if (vao == VaoInUse)
		{
			return;
		}

		VaoInUse = vao;
		GL.BindVertexArray(vao);
	}

	public static void UseShader(Shader shader)
	{
		UseShader(shader.ProgramId);
	}

	public static void UseShader(int programId)
	{
		if (programId == ShaderInUse)
		{
			return;
		}

		ShaderInUse = programId;
		GL.UseProgram(programId);
	}
}