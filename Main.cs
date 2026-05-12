using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using GameConsole.pcon;
using HarmonyLib;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace V1LevelSpeak;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Main : BaseUnityPlugin
{
	static ManualLogSource log;
	static AssetBundle assets;
	static Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
	static readonly Dictionary<string, string> layerToNumber = new Dictionary<string, string>()
	{
		{ "PRELUDE", "0" },
		{ "LIMBO", "1" },
		{ "LUST", "2" },
		{ "GLUTTONY", "3" },
		{ "GREED", "4" },
		{ "WRATH", "5" },
		{ "HERESY", "6" },
		{ "VIOLENCE", "7" },
		{ "FRAUD", "8" },
        // Future-proofing
        { "TREACHERY", "9" }
	};
	static readonly Dictionary<string, string> levelToNumber = new Dictionary<string, string>()
	{
		{ "FIRST", "1" },
		{ "SECOND", "2" },
		{ "THIRD", "3" },
		{ "FOURTH", "4" },
		{ "FIFTH", "5" },
		{ "CLIMAX", "4" },
		{ "ENCORE", "E" },
		{ "ACT I CRESCENDO", "1" },
		{ "ACT I CLIMAX", "2" },
		{ "ACT II CRESCENDO", "1" },
		{ "ACT II CLIMAX", "2" },
		// Future-proofing
		{ "ACT III CRESCENDO", "1" },
		{ "ACT III CLIMAX", "2" }
	};

	private void Awake()
	{
		Harmony.CreateAndPatchAll(typeof(LevelNamePopup_Patch));
		assets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "leveltitles"));

		foreach (var f in assets.LoadAllAssets<AudioClip>())
		{
			clips.Add(f.name, f);
		}

		log = Logger;
	}

	public static string GetLevelID(string layerName)
	{
		string[] sections = layerName.Split(' ');

		string key = string.Join(" ", sections.Skip(2));

		if (!levelToNumber.ContainsKey(key)) return null;

		string postfix = levelToNumber[key];

		if (sections[0] == "PRELUDE" && sections[2] == "CLIMAX") postfix = "5";

		if (!layerToNumber.ContainsKey(sections[0])) return null;

		return layerToNumber[sections[0]] + "-" + postfix;
	}

	[HarmonyPatch]
	public static class LevelNamePopup_Patch
	{
		private static readonly FieldRef<LevelNamePopup, string> layerStringField = FieldRefAccess<LevelNamePopup, string>("layerString");

		[HarmonyPrefix]
		[HarmonyPatch(typeof(LevelNamePopup), "ShowLayerText")]
		public static void patch_ShowLayerText(LevelNamePopup __instance)
		{
			string baseKey = GetLevelID(layerStringField(__instance));
			if (baseKey != null)
			{
				string soundKey = baseKey + "-1";
				if (clips.ContainsKey(soundKey))
				{
					AudioSource src = Camera.main.gameObject.AddComponent<AudioSource>();
					src.clip = clips[soundKey];
					src.volume = 1f;
					src.Play();
					Destroy(src, 10f);
					__instance.StartCoroutine(RunInABit(baseKey, src.clip.length));
				}
			}
		}

		static IEnumerator RunInABit(string baseKey, float delay)
		{
			yield return new WaitForSeconds(delay);
			var soundKey = baseKey + "-2";
			if (clips.ContainsKey(soundKey))
			{
				AudioSource src = Camera.main.gameObject.AddComponent<AudioSource>();
				src.clip = clips[soundKey];
				src.volume = 1f;
				src.Play();
				Destroy(src, 10f);
			}
		}
	}
}