namespace Tofu3D.Rendering;

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
			PassRenderTexture.Size = Camera.MainCamera.Size;
			PassRenderTexture.Invalidate(generateBrandNewTextures: false);
			return;
		}

		PassRenderTexture = new RenderTexture(size: Camera.MainCamera.Size, colorAttachment: true, depthAttachment: false, stencilAttachment: true);

		base.SetupRenderTexture();
	}
}