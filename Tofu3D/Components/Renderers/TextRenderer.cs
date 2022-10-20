using System.IO;
using Tofu3D.Components.Renderers;

namespace Scripts;

public class TextRenderer : SpriteRenderer
{
	private Dictionary<char, int> fontMappings = new Dictionary<char, int>()
	                                             {
		                                             {' ', 0},
		                                             {'0', 16},
		                                             {'1', 17},
		                                             {'2', 18},
		                                             {'3', 19},
		                                             {'4', 20},
		                                             {'5', 21},
		                                             {'6', 22},
		                                             {'7', 23},
		                                             {'8', 24},
		                                             {'9', 25},

		                                             {'A', 33},
		                                             {'B', 34},
		                                             {'C', 35},
		                                             {'D', 36},
		                                             {'E', 37},
		                                             {'F', 38},
		                                             {'G', 39},
		                                             {'H', 40},
		                                             {'I', 41},
		                                             {'J', 42},
		                                             {'K', 43},
		                                             {'L', 44},
		                                             {'M', 45},
		                                             {'N', 46},
		                                             {'O', 47},
		                                             {'P', 48},
		                                             {'Q', 49},
		                                             {'R', 50},
		                                             {'S', 51},
		                                             {'T', 52},
		                                             {'U', 53},
		                                             {'V', 54},
		                                             {'W', 55},
		                                             {'X', 56},
		                                             {'Y', 57},
		                                             {'Z', 58},
	                                             };
	[LinkableComponent]
	public Text text;
	// texture will be font signed distance field texture,
	// render will be basically going through all the characters in Text component and rendering each symbol

	public bool isGradient = true;
	[ShowIf(nameof(isGradient))]
	public Color gradientColor1;
	[ShowIf(nameof(isGradient))]
	public Color gradientColor2;

	private Vector2 spritesCount = new Vector2(1, 1);
	[Hide]
	public Vector2 spriteSize;
	public Vector2 SpritesCount
	{
		get { return spritesCount; }
		set
		{
			spritesCount = value;
			if (texture != null)
			{
				spriteSize = new Vector2(texture.size.X / SpritesCount.X, texture.size.Y / SpritesCount.Y);
			}
		}
	}

	public override void CreateMaterial()
	{
		if (material == null)
		{
			material = MaterialCache.GetMaterial("TextRenderer");
		}

		material.additive = false;
		base.CreateMaterial();
	}

	public override void OnNewComponentAdded(Component comp)
	{
	}

	public override void LoadTexture(string _texturePath)
	{
		if (_texturePath.Contains("Assets") == false)
		{
			_texturePath = Path.Combine("Assets", _texturePath);
		}

		if (File.Exists(_texturePath) == false)
		{
			return;
		}

		texture.Load(_texturePath, smooth: true);
	}

	public override void Render()
	{
		if (onScreen == false || boxShape == null || texture.loaded == false || text == null)
		{
			return;
		}

		//Debug.Log("Draw text:" + text?.text);

		ShaderCache.UseShader(material.shader);
		material.shader.SetVector2("u_resolution", texture.size);
		material.shader.SetMatrix4x4("u_mvp", LatestModelViewProjection);
		material.shader.SetColor("u_color", color.ToVector4());
		material.shader.SetVector2("u_scale", boxShape.size / Units.OneWorldUnit);
		material.shader.SetVector2("zoomAmount", spritesCount);
		material.shader.SetFloat("isGradient", isGradient ? 1 : 0);
		if (isGradient)
		{
			material.shader.SetVector4("u_color_a", gradientColor1.ToVector4());
			material.shader.SetVector4("u_color_b", gradientColor2.ToVector4());
		}

		float charSpacing = text.size * 3f + transform.scale.X * text.size;
		charSpacing = charSpacing / Units.OneWorldUnit;
		Vector2 originalPosition = transform.position;

		int symbolInLineIndex = 0;
		int line = 0;
		float lineSpacing = text.size * 3 + transform.scale.Y * text.size;

		Vector2 originalScale = transform.scale;
		transform.scale = Vector3.One * Mathf.Clamp(text.size / 40f, 0, 1000);

		for (int symbolIndex = 0;
		     symbolIndex < text.text.Length;
		     symbolIndex++,
		     symbolInLineIndex++)
		{
			transform.position = new Vector3(originalPosition.X + charSpacing * symbolInLineIndex, originalPosition.Y - line * lineSpacing, transform.position.Z);
			//transform.position = originalPosition;

			if (GetComponent<TextReactToMouse>() != null && Global.GameRunning)
			{
				float distanceToCursor = Vector2.Distance(transform.position, MouseInput.WorldPosition);
				transform.scale = Vector3.One * Mathf.Clamp(originalScale.X * ((0.2f / distanceToCursor) + 1f), 1, 2);

				transform.position = transform.position + new Vector2(0, (float) MathHelper.Sin(Time.elapsedTime + symbolIndex * 0.1f) * 1);
			}

			UpdateMVP();
			material.shader.SetMatrix4x4("u_mvp", LatestModelViewProjection);

			char ch = text.text[symbolIndex].ToString().ToUpper()[0];
			if (ch == '\n')
			{
				symbolInLineIndex = -1;
				line++;
				//symbolIndex++;
				continue;
			}

			int glyphMappingIndex = 0;

			if (fontMappings.ContainsKey(ch))
			{
				glyphMappingIndex = fontMappings[ch];
			}

			int columnIndex = glyphMappingIndex % (int) spritesCount.X;
			int rowIndex = (int) Math.Floor(glyphMappingIndex / spritesCount.Y);

			Vector2 drawOffset = new Vector2(columnIndex * spriteSize.X + spriteSize.X / 2, -rowIndex * spriteSize.Y - spriteSize.Y / 2);

			material.shader.SetVector2("offset", drawOffset);

			ShaderCache.BindVAO(material.vao);

			if (material.additive)
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
			}
			else
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			}

			TextureCache.BindTexture(texture.id);

			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

			Debug.CountStat("Draw Calls", 1);
		}

		transform.position = originalPosition;
		transform.scale = originalScale;
	}
}