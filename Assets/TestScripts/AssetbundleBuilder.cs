using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


public class AssetbundleBuilder
{

    [MenuItem ("Examble/BuildABs")]
    static void BuildABs()
    {

        string assetBundleDirectory = "Assets/AssetBundles";
        if(!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None,EditorUserBuildSettings.activeBuildTarget);
    }
}
