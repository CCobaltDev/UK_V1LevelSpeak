using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildAssets : MonoBehaviour
{
    [MenuItem("Assets/Build AssetBundle")]
    static void BuildBundles()
    {
        AssetBundleBuild[] buildDef = new AssetBundleBuild[1];
        buildDef[0].assetBundleName = "LevelTitles";
        buildDef[0].assetNames = Directory.GetFiles("Assets/Audio", "*.wav", SearchOption.AllDirectories);
        BuildPipeline.BuildAssetBundles("AssetBundleOut", buildDef, BuildAssetBundleOptions.AssetBundleStripUnityVersion | BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows);
    }
}