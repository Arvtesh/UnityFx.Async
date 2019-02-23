using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityFx.Async;

public class MainBehaviour : MonoBehaviour
{
	private void Start()
	{
		DontDestroyOnLoad(gameObject);
		Test();
	}

	private async void Test()
	{
		await LoadQuad();
		await LoadScene();
		await LoadScene2();
	}

	private async Task LoadQuad()
	{
		try
		{
			Debug.Log("GetAssetBundlePrefabAsync()");
			var prefab = await AsyncWww.GetAssetBundlePrefabAsync(Path.Combine(Application.streamingAssetsPath, "quad"), null, null);
			Instantiate(prefab);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}

	private async Task<Scene> LoadScene()
	{
		try
		{
			Debug.Log("GetAssetBundleSceneAsync()");
			return await AsyncWww.GetAssetBundleSceneAsync(Path.Combine(Application.streamingAssetsPath, "test_scene"), null, LoadSceneMode.Additive, null);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
			throw;
		}
	}

	private async Task<Scene> LoadScene2()
	{
		try
		{
			Debug.Log("GetAssetBundleAsync()");
			var assetBundle = await AsyncWww.GetAssetBundleAsync(Path.Combine(Application.streamingAssetsPath, "test_scene"));
			Debug.Log("GetAssetBundleSceneAsync()");
			return await AsyncWww.GetAssetBundleSceneAsync(assetBundle, null, LoadSceneMode.Additive, null);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
			throw;
		}
	}
}
