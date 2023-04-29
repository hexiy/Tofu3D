﻿namespace Tofu3D.Rendering;

public class RenderPassUI : RenderPass
{
	public static RenderPassUI I { get; private set; }

	public RenderPassUI() : base(RenderPassType.UI)
	{
		I = this;
	}

	public override void Initialize()
	{
		SetupRenderTexture();

		base.Initialize();
	}

	protected override void SetupRenderTexture()
	{
		if (PassRenderTexture != null)
		{
			PassRenderTexture.Size = RenderPassSystem.ViewSize;
			PassRenderTexture.Invalidate(generateBrandNewTextures: false);
			return;
		}

		PassRenderTexture = new RenderTexture(size: RenderPassSystem.ViewSize, colorAttachment: true, depthAttachment: true, hasStencil: true);

		base.SetupRenderTexture();
	}
}