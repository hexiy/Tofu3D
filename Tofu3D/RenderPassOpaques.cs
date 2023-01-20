namespace Tofu3D;

public class RenderPassOpaques : RenderPass
{
	public static RenderPassOpaques I { get; private set; }

	public RenderPassOpaques() : base(RenderPassType.Opaques)
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
		PassRenderTexture = new RenderTexture(size: Camera.I.Size, colorAttachment: true, depthAttachment: true);

		base.SetupRenderTexture();
	}

	protected override void PreRender()
	{
		base.PreRender();
	}

	protected override void PostRender()
	{
		base.PostRender();
	}
}