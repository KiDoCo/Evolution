using System.Collections;
using UnityEngine;

#pragma warning disable
/// <summary>
/// Possibly will be removed
/// </summary>
public static class AssetLoader {

    /// <summary>
    /// Loads asset from parameter path
    /// </summary>
    /// <param name="assetBundleName"></param>
    /// <param name="objectNameToLoad"></param>
    /// <returns></returns>
  public static IEnumerator loadAsset(string assetBundleName, string objectNameToLoad)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        filePath = System.IO.Path.Combine(filePath, assetBundleName);
        Debug.Log("loading" + filePath);
        var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
        yield return assetBundleCreateRequest;

        AssetBundle asseBundle = assetBundleCreateRequest.assetBundle;

        AssetBundleRequest asset = asseBundle.LoadAssetAsync<GameObject>(objectNameToLoad);
        Debug.Log("loading" + asset);
        yield return asset;

        GameObject loadedAsset = asset.asset as GameObject;
        Debug.Log(asset.asset);
        yield return asset.asset;
        Asset = asset.asset;
        //Do something with the loaded loadedAsset  object
    }
    public static object Asset { get; set; }
}

