﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


public class AssetbundleBuilder : MonoBehaviour
{

    [MenuItem ("Examble/BuildABs")]
    static void BuildABs()
    {
        string assetBundleDirectory = Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        if(!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.UncompressedAssetBundle,EditorUserBuildSettings.activeBuildTarget);
    }
}