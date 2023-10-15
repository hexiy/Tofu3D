﻿using Tofu3D.Rendering;

namespace Tofu3D;

public class RenderPassSkybox : RenderPass
{
	public static RenderPassSkybox I { get; private set; }

	public RenderPassSkybox() : base(RenderPassType.Skybox)
	{
		I = this;
	}
	// protected override bool CanRender()
	// {
	// 	return _directionalLight?.IsActive == true;
	// }

	public override void Initialize()
	{
		base.Initialize();
		SetupRenderTexture();
	}

	protected override void SetupRenderTexture()
	{
		PassRenderTexture = new RenderTexture(size: Tofu.RenderPassSystem.ViewSize, colorAttachment: true);
	}
}