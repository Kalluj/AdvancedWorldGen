namespace AdvancedWorldGen.UI;

public class VanillaWorldGenConfigurator : UIState
{
	public readonly WorldGenConfiguration Configuration;

	public VanillaWorldGenConfigurator()
	{
		Configuration =
			WorldGenConfiguration.FromEmbeddedPath("Terraria.GameContent.WorldBuilding.Configuration.json");
		WorldGen.Hooks.ProcessWorldGenConfig(ref Configuration);
		WorldGen.Hooks.OnWorldGenConfigProcess += SetConfig;

		SetupUI();
	}

	public void Dispose()
	{
		WorldGen.Hooks.OnWorldGenConfigProcess -= SetConfig;
	}

	private void SetupUI()
	{
		UIPanel uiPanel = new()
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
			Width = new StyleDimension(0, 0.5f),
			Height = new StyleDimension(0, 0.5f),
			BackgroundColor = UICommon.MainPanelBackground
		};
		Append(uiPanel);

		UIText uiTitle = new("Worldgen Config", 0.75f, true) { HAlign = 0.5f };
		uiTitle.Height = uiTitle.MinHeight;
		uiPanel.Append(uiTitle);
		uiPanel.Append(new UIHorizontalSeparator
		{
			Width = new StyleDimension(0f, 1f),
			Top = new StyleDimension(43f, 0f),
			Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f
		});


		UIScrollbar uiScrollbar = new()
		{
			Height = new StyleDimension(-60f, 1f),
			Top = new StyleDimension(50, 0f),
			HAlign = 1f
		};
		UIList uiList = new()
		{
			Height = new StyleDimension(-60f, 1f),
			Width = new StyleDimension(-20f, 1f),
			Top = new StyleDimension(50, 0f)
		};
		uiList.SetScrollbar(uiScrollbar);
		uiPanel.Append(uiScrollbar);
		uiPanel.Append(uiList);

		int index = 0;

		foreach (JObject jObject in typeof(WorldGenConfiguration)
			         .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
			         .Select(fieldInfo => (JObject)fieldInfo.GetValue(Configuration)!))
			TransformJsonToUI(uiList, ref index, jObject);


		UITextPanel<string> goBack = new(Language.GetTextValue("UI.Back"))
		{
			Width = new StyleDimension(0f, 0.1f),
			Top = new StyleDimension(0f, 0.75f),
			HAlign = 0.5f
		};
		goBack.OnLeftClick += GoBack;
		goBack.OnMouseOver += UIChanger.FadedMouseOver;
		goBack.OnMouseOut += UIChanger.FadedMouseOut;
		Append(goBack);
		Recalculate();
	}

	private static void TransformJsonToUI(UIList uiPanel, ref int index, JToken jToken)
	{
		const string localizationPath = "Mods.AdvancedWorldGen.UI.Vanilla";
		switch (jToken.Type)
		{
			case JTokenType.Object:
				JObject jObject = (JObject)jToken;
				foreach ((string? name, JToken? child) in jObject)
					if (child is not null && name != "CorruptionPitCount")
						TransformJsonToUI(uiPanel, ref index, child);
				break;
			case JTokenType.Integer:
				//Create an long number text box.
				NumberTextBox<long> longInput =
					new JsonNumberTextBox<long>((JValue)jToken, 0, ushort.MaxValue, localizationPath)
					{
						Order = index++
					};
				uiPanel.Add(longInput);
				break;
			case JTokenType.Float:
				//Create a double number text box.
				NumberTextBox<double> doubleInput =
					new JsonNumberTextBox<double>((JValue)jToken, 0, double.PositiveInfinity, localizationPath)
					{
						Order = index++
					};
				uiPanel.Add(doubleInput);
				break;
			case JTokenType.String:
				//Create a world scaling text box.
				EnumInputListBox<JsonRange.ScalingMode> enumInput = new((JValue)jToken, localizationPath)
				{
					Order = index++
				};
				uiPanel.Add(enumInput);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(jToken),
					$"{jToken.Type} not implemented for serialization");
		}
	}

	// ReSharper disable once RedundantAssignment
	private void SetConfig(ref WorldGenConfiguration configuration)
	{
		configuration = Configuration;
	}

	private static void GoBack(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose);
		Main.MenuUI.SetState(new CustomSizeUI());
	}
}