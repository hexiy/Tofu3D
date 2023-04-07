namespace Scripts;

public class ParticleSystemRenderer : SpriteRenderer
{
	public new bool AllowMultiple = false;

	private int _particlesInBatcher;
	public ParticleSystem ParticleSystem;
	public bool Additive = true;

	public override void Render()
	{
		if (OnScreen == false)
		{
			return;
		}

		if (BoxShape == null)
		{
			return;
		}

		if (Texture.Loaded == false)
		{
			return;
		}

		while (_particlesInBatcher < ParticleSystem.Particles.Count)
		{
			//BatchingManager.AddObjectToBatcher(texture.id, this, particlesInBatcher);
			_particlesInBatcher++;
		}

		ShaderCache.UseShader(Material.Shader);
		Material.Shader.SetVector2("u_repeats", Tiling);
		TextureCache.BindTexture(Texture.TextureId);

		foreach (Particle particle in ParticleSystem.Particles)
		{
			Material.Shader.SetMatrix4X4("u_mvp", GetParticleMvpMatrix(particle));
			Material.Shader.SetColor("u_color", particle.Color.ToVector4());

			ShaderCache.BindVertexArray(Material.Vao);


			// GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.ConstantColor); cool
			if (Additive)
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			}
			else
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			}


			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

			Debug.StatAddValue("Draw Calls", 1);
		}
		//BatchingManager.UpdateAttribs(texture.id, gameObjectID, particleSystem.particles[i].worldPosition, new Vector2(particleSystem.particles[i].radius),
		//                              particleSystem.particles[i].color, i);

		Debug.StatSetValue("Particles", ParticleSystem.Particles.Count);
	}

	public Matrix4x4 GetParticleMvpMatrix(Particle particle)
	{
		Vector3 pivotOffset = -(particle.Radius * Transform.WorldScale) / 2
		                    + particle.Radius * Transform.WorldScale * Transform.Pivot;

		Matrix4x4 pivot = Matrix4x4.CreateTranslation(-pivotOffset.X, -pivotOffset.Y, -pivotOffset.Z);
		Matrix4x4 translation = Matrix4x4.CreateTranslation(particle.WorldPosition + BoxShape.Offset * Transform.WorldScale) * Matrix4x4.CreateScale(1, 1, -1);

		Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y / 180 * Mathf.Pi,
		                                                      -Transform.Rotation.X / 180 * Mathf.Pi,
		                                                      -Transform.Rotation.Z / 180 * Mathf.Pi);
		Matrix4x4 scale = Matrix4x4.CreateScale(particle.Radius * Transform.WorldScale);
		return scale * Matrix4x4.Identity * pivot * rotation * translation * Matrix4x4.CreateScale(Units.OneWorldUnit) * Camera.I.ViewMatrix * Camera.I.ProjectionMatrix;
	}
}