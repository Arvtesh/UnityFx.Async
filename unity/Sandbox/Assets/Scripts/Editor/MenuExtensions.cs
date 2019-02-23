using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class MenuExtensions
{
	[MenuItem("Sandbox/Build Asset Bundles")]
	public static void BuildAssetBundles()
	{
		BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
	}
}
