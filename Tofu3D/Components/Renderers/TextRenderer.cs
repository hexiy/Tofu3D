using System.IO;

namespace Scripts;

public class TextRenderer : SpriteRenderer
{
	Dictionary<char, int> _fontMappings = new()
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
		                                      {'Z', 58}
	                                      };
	[ShowIf(nameof(IsGradient))]
	public Color GradientColor1 = Tofu3D.Color.White;
	[ShowIf(nameof(IsGradient))]
	public Color GradientColor2 = Tofu3D.Color.White;
	// texture will be font signed distance field texture,
	// render will be basically going through all the characters in Text component and rendering each symbol

	public bool IsGradient = false;

	Vector2 _spritesCount = new(8, 8);
	[Hide]
	public Vector2 SpriteSize;
	// //[LinkableComponent]
	public Text Text;
	public Vector2 SpritesCount
	{
		get { return _spritesCount; }
		set
		{
			_spritesCount = value;
			if (Texture != null)
			{
				SpriteSize = new Vector2(Texture.Size.X / SpritesCount.X, Texture.Size.Y / SpritesCount.Y);
			}
		}
	}

	public override void Awake()
	{
		SpritesCount = SpritesCount;
		SetDefaultTexture(Path.Combine(Folders.Textures, "sdf.png"));
		Text = GetComponent<Text>();
		BoxShape = GetComponent<BoxShape>();


		if (Texture == null)
		{
			Texture = new Texture();
		}
		else
		{
			TextureLoadSettings textureLoadSettings = TextureLoadSettings.DefaultSettingsSpritePixelArt;
			Texture = AssetManager.Load<Texture>(Texture.AssetPath, textureLoadSettings);
		}

		base.Awake();
	}

	public override void SetDefaultMaterial()
	{
		if (Material?.FileName == null)
		{
			Material = MaterialCache.GetMaterial("TextRenderer");
		}

		Material.Additive = false;
	}

	public override void OnNewComponentAdded(Component comp)
	{
		Text = GetComponent<Text>();
		BoxShape = GetComponent<BoxShape>();
	}

	public override void LoadTexture(string texturePath)
	{
		if (texturePath.Contains("Assets") == false)
		{
			texturePath = Path.Combine("Assets", texturePath);
		}

		if (File.Exists(texturePath) == false)
		{
			return;
		}


		Texture = AssetManager.Load<Texture>(texturePath);
	}

	private bool IsInCanvas()
	{
		return Transform.Parent?.GetComponent<Canvas>() != null;
	}

	public override void Render()
	{
		if (OnScreen == false || BoxShape == null || Texture.Loaded == false || Text == null)
		{
			return;
		}

		//Debug.Log("Draw text:" + text?.text);

		ShaderCache.UseShader(Material.Shader);
		Material.Shader.SetVector2("u_resolution", Texture.Size);

		if (IsInCanvas())
		{
			// Material.Shader.SetMatrix4X4("u_mvp", GetModelMatrix() * Matrix4x4.CreateScale(1f/Units.OneWorldUnit));
			Material.Shader.SetMatrix4X4("u_mvp", GetModelMatrixForCanvasObject()); // * Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);
		}
		else
		{
			Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
		}

		Material.Shader.SetColor("u_color", Color.ToVector4());
		Material.Shader.SetVector2("u_scale", BoxShape.Size / Units.OneWorldUnit);
		Material.Shader.SetVector2("zoomAmount", _spritesCount * 2);
		Material.Shader.SetFloat("isGradient", IsGradient ? 1 : 0);
		if (IsGradient)
		{
			Material.Shader.SetVector4("u_color_a", GradientColor1.ToVector4());
			Material.Shader.SetVector4("u_color_b", GradientColor2.ToVector4());
		}

		BoxShape.Size = Vector3.One * 1.2f; // bigger individual characters


		float charSpacing = Transform.WorldScale.X * Text.Size * BoxShape.Size.X + Text.Size * Transform.WorldScale.X;
		charSpacing = charSpacing / Units.OneWorldUnit;
		Vector2 originalPosition = Transform.WorldPosition;

		int symbolInLineIndex = 0;
		int line = 0;
		float lineSpacing = Text.Size * 3 + Transform.WorldScale.Y * Text.Size;

		Vector2 originalScale = Transform.LocalScale;
		Vector2 fontSizeScale = Vector3.One * Mathf.Clamp(Text.Size / 40f, 0, 1000);
		Transform.LocalScale = originalScale * fontSizeScale;

		float textWidth = charSpacing * (Text.Value.Length - 1);

		for (int symbolIndex = 0;
		     symbolIndex < Text.Value.Length;
		     symbolIndex++,
		     symbolInLineIndex++)
		{
			Transform.WorldPosition = new Vector3(originalPosition.X + charSpacing * symbolInLineIndex - textWidth * Transform.Pivot.X,
			                                      originalPosition.Y - line * lineSpacing, Transform.WorldPosition.Z);

			if (GetComponent<TextReactToMouse>() != null && Global.GameRunning)
			{
				Transform.WorldPosition = Transform.WorldPosition + new Vector2(0, (float) MathHelper.Sin(Time.ElapsedTime + symbolIndex * 0.1f) * 1);

				float distanceToCursor = Vector2.Distance(Transform.WorldPosition, MouseInput.WorldPosition);
				Transform.WorldScale = originalScale * fontSizeScale * Mathf.Clamp((0.2f / distanceToCursor + 1f), 1, 1.3f);
			}

			UpdateMvp();
			// Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
			if (IsInCanvas())
			{
				// Material.Shader.SetMatrix4X4("u_mvp", GetModelMatrix() * Matrix4x4.CreateScale(1f/Units.OneWorldUnit));
				Material.Shader.SetMatrix4X4("u_mvp", GetModelMatrixForCanvasObject()); // * Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);
			}
			else
			{
				Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
			}

			char ch = Text.Value[symbolIndex].ToString().ToUpper()[0];
			if (ch == '\n')
			{
				symbolInLineIndex = -1;
				line++;
				//symbolIndex++;
				continue;
			}

			int glyphMappingIndex = 0;

			if (_fontMappings.ContainsKey(ch))
			{
				glyphMappingIndex = _fontMappings[ch];
			}

			int columnIndex = glyphMappingIndex % (int) _spritesCount.X;
			int rowIndex = (int) Math.Floor(glyphMappingIndex / _spritesCount.Y);

			Vector2 drawOffset = new(columnIndex * SpriteSize.X + SpriteSize.X / 2, -rowIndex * SpriteSize.Y - SpriteSize.Y / 2);

			Material.Shader.SetVector2("offset", drawOffset);

			ShaderCache.BindVertexArray(Material.Vao);

			if (Material.Additive)
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
			}
			else
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			}

			GL.ActiveTexture(TextureUnit.Texture0);
			TextureCache.BindTexture(Texture.TextureId);

			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

			Debug.StatAddValue("Draw Calls", 1);
		}

		Transform.WorldPosition = originalPosition;
		Transform.LocalScale = originalScale;
	}
}