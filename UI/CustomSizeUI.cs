using System;
using System.Reflection;
using AdvancedWorldGen.Base;
using AdvancedWorldGen.BetterVanillaWorldGen;
using AdvancedWorldGen.BetterVanillaWorldGen.Interface;
using AdvancedWorldGen.CustomSized;
using AdvancedWorldGen.Helper;
using AdvancedWorldGen.UI.InputUI;
using AdvancedWorldGen.UI.InputUI.Number;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace AdvancedWorldGen.UI;

public class CustomSizeUI : UIState
{
	public WorldSettings WorldSettings;

	public CustomSizeUI()
	{
		WorldSettings = ModifiedWorld.Instance.OptionHelper.WorldSettings;
		CreateCustomSizeUI();
	}

	public void CreateCustomSizeUI()
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

		UIText uiTitle = new("Size options", 0.75f, true) { HAlign = 0.5f };
		uiTitle.Height = uiTitle.MinHeight;
		uiPanel.Append(uiTitle);
		uiPanel.Append(new UIHorizontalSeparator
		{
			Width = new StyleDimension(0f, 1f),
			Top = new StyleDimension(43f, 0f),
			Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f
		});

		float top = 50;
		NumberTextBox<int> sizeXInput =
			new ConfigNumberTextBox<int>(WorldSettings.Params, nameof(Params.SizeX), 100, ushort.MaxValue);
		sizeXInput.Top.Pixels = top;
		top += sizeXInput.Height.Pixels + 4;
		uiPanel.Append(sizeXInput);

		NumberTextBox<int> sizeYInput =
			new ConfigNumberTextBox<int>(WorldSettings.Params, nameof(Params.SizeY), 100, ushort.MaxValue);
		sizeYInput.Top.Pixels = top;
		top += sizeYInput.Height.Pixels + 4;
		uiPanel.Append(sizeYInput);

		NumberTextBox<float> templeModifier =
			new ConfigNumberTextBox<float>(WorldSettings.Params, nameof(Params.TempleMultiplier), 0,
				float.PositiveInfinity);
		templeModifier.Top.Pixels = top;
		top += templeModifier.Height.Pixels + 4;
		uiPanel.Append(templeModifier);

		if (WorldgenSettings.Revamped)
		{
			NumberTextBox<float> dungeonModifier =
				new ConfigNumberTextBox<float>(WorldSettings.Params, nameof(Params.DungeonMultiplier), 0,
					float.MaxValue);
			dungeonModifier.Top.Pixels = top;
			top += dungeonModifier.Height.Pixels + 4;
			uiPanel.Append(dungeonModifier);

			NumberTextBox<float> beachModifier = new ConfigNumberTextBox<float>(WorldSettings.Params,
				nameof(Params.BeachMultiplier), 0,
				float.PositiveInfinity);
			beachModifier.Top.Pixels = top;
			top += beachModifier.Height.Pixels + 4;
			uiPanel.Append(beachModifier);
		}

		UITextPanel<string> gotoConfig = new(Language.GetTextValue("UI.Config"))
		{
			Width = new StyleDimension(0f, 1f),
			Top = new StyleDimension(top, 0f)
		};
		uiPanel.Append(gotoConfig);
		gotoConfig.OnMouseDown += ConfigWorldGen;
		gotoConfig.OnMouseOver += UiChanger.FadedMouseOver;
		gotoConfig.OnMouseOut += UiChanger.FadedMouseOut;

		UITextPanel<string> goBack = new(Language.GetTextValue("UI.Back"))
		{
			Width = new StyleDimension(0f, 0.1f),
			Top = new StyleDimension(0f, 0.75f),
			HAlign = 0.5f
		};
		goBack.OnMouseDown += GoBack;
		goBack.OnMouseOver += UiChanger.FadedMouseOver;
		goBack.OnMouseOut += UiChanger.FadedMouseOut;
		Append(goBack);
	}

	public static void ConfigWorldGen(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuOpen);
		Main.MenuUI.SetState(AdvancedWorldGenMod.Instance.UiChanger.WorldGenConfigurator);
	}

	public void GoBack(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose);
		int size = WorldSettings.Params.SizeX switch
		{
			4200 when WorldSettings.Params.SizeY == 1200 => 0,
			6400 when WorldSettings.Params.SizeY == 1800 => 1,
			8400 when WorldSettings.Params.SizeY == 2400 => 2,
			_ => -1
		};

		VanillaAccessor<int> optionSize = VanillaInterface.OptionSize(WorldSettings.UIWorldCreation);
		optionSize.Value = size;

		object[] sizeButtons = VanillaInterface.SizeButtons(WorldSettings.UIWorldCreation).Value;

		Type groupOptionButtonType = sizeButtons.GetType().GetElementType()!;
		MethodInfo setCurrentOptionMethod =
			groupOptionButtonType.GetMethod("SetCurrentOption", BindingFlags.Instance | BindingFlags.Public)!;

		foreach (object groupOptionButton in sizeButtons)
			setCurrentOptionMethod.Invoke(groupOptionButton, new object[] { size });

#if !SPECIALDEBUG
		int oldSizeX = Main.tile.Width;
		int oldSizeY = Main.tile.Height;
		if (oldSizeX < WorldSettings.Params.SizeX || oldSizeY < WorldSettings.Params.SizeY)
		{
			int newSizeX = Math.Max(WorldSettings.Params.SizeX, 8400);
			int newSizeY = Math.Max(WorldSettings.Params.SizeY, 2100);

			if (KnownLimits.WillCrashMissingEwe(newSizeX, newSizeY))
			{
				Main.MenuUI.SetState(new ErrorUI(Language.GetTextValue(
					"Mods.AdvancedWorldGen.InvalidSizes.TooBigFromRAM", newSizeX, newSizeY)));
				return;
			}
		}

		if (WorldgenSettings.Revamped)
		{
			if (WorldSettings.Params.SizeX < KnownLimits.OverhauledMinX)
			{
				Main.MenuUI.SetState(new ErrorUI(Language.GetTextValue(
					"Mods.AdvancedWorldGen.InvalidSizes.OverhauledMinX", KnownLimits.OverhauledMinX)));
				return;
			}

			if (WorldSettings.Params.SizeY < KnownLimits.OverhauledMinY)
			{
				Main.MenuUI.SetState(new ErrorUI(Language.GetTextValue(
					"Mods.AdvancedWorldGen.InvalidSizes.OverhauledMinY", KnownLimits.OverhauledMinY)));
				return;
			}
		}
		else
		{
			if (WorldSettings.Params.SizeX < KnownLimits.NormalMinX)
			{
				Main.MenuUI.SetState(new ErrorUI(Language.GetTextValue(
					"Mods.AdvancedWorldGen.InvalidSizes.NormalMinX", KnownLimits.NormalMinX)));
				return;
			}

			if (WorldSettings.Params.SizeY > KnownLimits.ComfortNormalMaxX)
			{
				Main.MenuUI.SetState(new ErrorUI(Language.GetTextValue(
					"Mods.AdvancedWorldGen.InvalidSizes.ComfortNormalMaxX")));
				return;
			}
		}
#endif

		Main.MenuUI.SetState(AdvancedWorldGenMod.Instance.UiChanger.OptionsSelector);
	}
}