using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace UK_V1LevelSpeak
{
    [BepInPlugin("ccobaltdev.v1levelspeak", "V1LevelSpeak", "1.0.0")]
    public class V1LevelSpeak : BaseUnityPlugin
    {
        static ManualLogSource log;
        static AssetBundle assets;
        static Dictionary<string, AudioClip[]> clips = new Dictionary<string, AudioClip[]>();
        static Dictionary<string, string> levelMap = new Dictionary<string, string>
        {
            { "0-1", "PRELUDE /// FIRST" },
            { "PRELUDE_FIRST", "PRELUDE /// FIRST" },
            { "0-2", "PRELUDE /// SECOND" },
            { "PRELUDE_SECOND", "PRELUDE /// SECOND" },
            { "0-3", "PRELUDE /// THIRD" },
            { "PRELUDE_THIRD", "PRELUDE /// THIRD" },
            { "0-4", "PRELUDE /// FOURTH" },
            { "PRELUDE_FOURTH", "PRELUDE /// FOURTH" },
            { "0-5", "PRELUDE /// CLIMAX" },
            { "PRELUDE_CLIMAX", "PRELUDE /// CLIMAX" },
            { "1-1", "LIMBO /// FIRST" },
            { "LIMBO_FIRST", "LIMBO /// FIRST" },
            { "1-2", "LIMBO /// SECOND" },
            { "LIMBO_SECOND", "LIMBO /// SECOND" },
            { "1-3", "LIMBO /// THIRD" },
            { "LIMBO_THIRD", "LIMBO /// THIRD" },
            { "1-4", "LIMBO /// CLIMAX" },
            { "LIMBO_CLIMAX", "LIMBO /// CLIMAX" },
            { "2-1", "LUST /// FIRST" },
            { "LUST_FIRST", "LUST /// FIRST" },
            { "2-2", "LUST /// SECOND" },
            { "LUST_SECOND", "LUST /// SECOND" },
            { "2-3", "LUST /// THIRD" },
            { "LUST_THIRD", "LUST /// THIRD" },
            { "2-4", "LUST /// CLIMAX" },
            { "LUST_CLIMAX", "LUST /// CLIMAX" },
            { "3-1", "GLUTTONY /// ACT I CRESCENDO" },
            { "GLUTTONY_ACT_1_CRESCENDO", "GLUTTONY /// ACT I CRESCENDO" },
            { "3-2", "GLUTTONY /// ACT I CLIMAX" },
            { "GLUTTONY_ACT_1_CLIMAX", "GLUTTONY /// ACT I CLIMAX" },
            { "4-1", "GREED /// FIRST" },
            { "GREED_FIRST", "GREED /// FIRST" },
            { "4-2", "GREED /// SECOND" },
            { "GREED_SECOND", "GREED /// SECOND" },
            { "4-3", "GREED /// THIRD" },
            { "GREED_THIRD", "GREED /// THIRD" },
            { "4-4", "GREED /// CLIMAX" },
            { "GREED_CLIMAX", "GREED /// CLIMAX" },
            { "5-1", "WRATH /// FIRST" },
            { "WRATH_FIRST", "WRATH /// FIRST" },
            { "5-2", "WRATH /// SECOND" },
            { "WRATH_SECOND", "WRATH /// SECOND" },
            { "5-3", "WRATH /// THIRD" },
            { "WRATH_THIRD", "WRATH /// THIRD" },
            { "5-4", "WRATH /// CLIMAX" },
            { "WRATH_CLIMAX", "WRATH /// CLIMAX" },
            { "6-1", "HERESY /// ACT II CRESCENDO" },
            { "HERESY_ACT_2_CRESCENDO", "HERESY /// ACT II CRESCENDO" },
            { "6-2", "HERESY /// ACT II CLIMAX" },
            { "HERESY_ACT_2_CLIMAX", "HERESY /// ACT II CLIMAX" },
            { "7-1", "VIOLENCE /// FIRST" },
            { "VIOLENCE_FIRST", "VIOLENCE /// FIRST" },
            { "7-2", "VIOLENCE /// SECOND" },
            { "VIOLENCE_SECOND", "VIOLENCE /// SECOND" },
            { "7-3", "VIOLENCE /// THIRD" },
            { "VIOLENCE_THIRD", "VIOLENCE /// THIRD" },
            { "7-4", "VIOLENCE /// CLIMAX" },
            { "VIOLENCE_CLIMAX", "VIOLENCE /// CLIMAX" },
            { "P-1", "PRIME /// FIRST" },
            { "PRIME_FIRST", "PRIME /// FIRST" },
            { "P-2", "PRIME /// SECOND" },
            { "PRIME_SECOND", "PRIME /// SECOND" }
        };
        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(LevelNamePopup_Patch));
            assets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "leveltitles"));

            //Inefficient - but it works
            foreach (var f in assets.LoadAllAssets<AudioClip>())
            {
                if (levelMap.ContainsKey(f.name)) Add(f.name, f);
            }

            log = Logger;
        }

        private static void Add(string name, AudioClip clip)
        {
            if (!clips.ContainsKey(levelMap[name])) clips.Add(levelMap[name], new AudioClip[2]);
            if (name.Length == 3) clips[levelMap[name]][1] = clip;
            else clips[levelMap[name]][0] = clip;
        }

        [HarmonyPatch]
        public static class LevelNamePopup_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(LevelNamePopup), "ShowLayerText")]
            public static void patch_ShowLayerText(LevelNamePopup __instance)
            {
                if (clips.ContainsKey(MapInfoBase.Instance.layerName))
                {
                    AudioSource src = Camera.main.gameObject.AddComponent<AudioSource>();
                    src.clip = clips[MapInfoBase.Instance.layerName][0];
                    src.volume = 1f;
                    src.Play();
                    Destroy(src, 10f);
                    __instance.StartCoroutine(RunInABit(src.clip.length));
                }
            }

            static IEnumerator RunInABit(float delay)
            {
                yield return new WaitForSeconds(delay);
                if (clips.ContainsKey(MapInfoBase.Instance.layerName))
                {
                    AudioSource src = Camera.main.gameObject.AddComponent<AudioSource>();
                    src.clip = clips[MapInfoBase.Instance.layerName][1];
                    src.volume = 1f;
                    src.Play();
                    Destroy(src, 10f);
                }
            }
        }
    }
}
