namespace AdvancedWorldGen.Base;

public class API
{
	/// <summary>
	///     Registers a new option
	///     To be well drawn, an entry Mods.{mod.Name}.{internalName} defining the displayed option name and an entry
	///     Mods.{mod.Name}.{internalName}.Description giving a short description must be defined on a localization file
	/// </summary>
	/// <param name="mod">The mod featuring the given option</param>
	/// <param name="internalName">The internal name of the option, it must be unique and with no space</param>
	/// <param name="hidden">Set to true if you don't want this option to be shown by default</param>
	[Obsolete("Please use Mod.Call", true)]
	public static void RegisterOption(Mod mod, string internalName, bool hidden = false)
	{
		OptionHelper.OptionDict.Add(internalName, new Option
		{
			Hidden = hidden,
			ModName = mod.Name,
			Name = internalName
		});
	}

	[Obsolete("Please use Mod.Call", true)]
	public static bool OptionsContains(params string[] options)
	{
		return OptionHelper.OptionsContains(options[0]);
	}
}