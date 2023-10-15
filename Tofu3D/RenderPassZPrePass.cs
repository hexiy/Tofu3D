﻿namespace Tofu3D.Rendering;

public class RenderPassZPrePass : RenderPass
{
	public static RenderPassZPrePass I { get; private set; }

	public RenderPassZPrePass() : base(RenderPassType.ZPrePass)
	{
		I = this;
	}

	public override void Initialize()
	{
		SetupRenderTexture();

		base.Initialize();
	}

	protected override void PreRender()
	{
		GL.Enable(EnableCap.DepthTest);

		GL.DepthMask(true);

		GL.ClearDepth(1);
		GL.Clear(ClearBufferMask.DepthBufferBit);
		// GL.DepthRange(0, Camera.MainCamera.FarPlaneDistance);
		GL.DepthRange(0, 1);
		GL.DepthFunc(DepthFunction.Lequal);
	}

	protected override void PostUnbindFrameBuffer()
	{
		GL.DepthMask(false);
	}

	protected override void SetupRenderTexture()
	{
		if (PassRenderTexture != null)
		{
			PassRenderTexture.Size = Tofu.RenderPassSystem.ViewSize;
			PassRenderTexture.Invalidate(generateBrandNewTextures: false);
			return;
		}

		PassRenderTexture = new RenderTexture(size: Tofu.RenderPassSystem.ViewSize, colorAttachment: false, depthAttachment: true, hasStencil: false);
	}
}