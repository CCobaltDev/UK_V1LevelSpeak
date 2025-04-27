using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace UK_V1LevelSpeak
{
    [BepInPlugin("ccobaltdev.v1levelspeak", "V1LevelSpeak", "1.1.0")]
    public class V1LevelSpeak : BaseUnityPlugin
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
            // Future-proofing
            { "FRAUD", "8" },
            { "TREACHERY", "9" }
        };
        static readonly Dictionary<string, string> numberToWord = new Dictionary<string, string>()
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
        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(LevelNamePopup_Patch));
            assets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "leveltitles"));

            foreach (var f in assets.LoadAllAssets<AudioClip>())
            {
                clips.Add(f.name, f);
            }

            log = Logger;
        }

        public static string GetCurrentLevel()
        {
            string[] sections = MapInfoBase.Instance.layerName.Split(' ');

            string postfix = numberToWord[string.Join(" ", sections.Skip(2))];

            if (sections[0] == "PRELUDE" && sections[2] == "CLIMAX") postfix = "5";

            return layerToNumber[sections[0]] + "-" + postfix;
        }

        [HarmonyPatch]
        public static class LevelNamePopup_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(LevelNamePopup), "ShowLayerText")]
            public static void patch_ShowLayerText(LevelNamePopup __instance)
            {
                var clip = clips[GetCurrentLevel() + "-1"];
                if (clip != null)
                {
                    AudioSource src = Camera.main.gameObject.AddComponent<AudioSource>();
                    src.clip = clip;
                    src.volume = 1f;
                    src.Play();
                    Destroy(src, 10f);
                    __instance.StartCoroutine(RunInABit(src.clip.length));
                }
            }

            static IEnumerator RunInABit(float delay)
            {
                yield return new WaitForSeconds(delay);
                var clip = clips[GetCurrentLevel() + "-2"];
                if (clip != null)
                {
                    AudioSource src = Camera.main.gameObject.AddComponent<AudioSource>();
                    src.clip = clip;
                    src.volume = 1f;
                    src.Play();
                    Destroy(src, 10f);
                }
            }
        }
    }
}
