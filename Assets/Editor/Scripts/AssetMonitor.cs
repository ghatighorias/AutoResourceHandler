using System;
using UnityEngine;
using UnityEditor;

namespace AutoAssetLoader
{
    class AssetMonitor : UnityEditor.AssetModificationProcessor
    {
        static void OnWillCreateAsset(string assetName)
        {
            if (AutoAssetLoader.MonitorActive)
            {
                var guid = AssetDatabase.AssetPathToGUID(assetName);
                if (!string.IsNullOrWhiteSpace(guid))
                {
                    AutoAssetLoader.LoadSettings();
                    AutoAssetLoader.Generate();
                }
            }
        }

        static AssetDeleteResult OnWillDeleteAsset(string assetName, RemoveAssetOptions removeAssetOptions)
        {
            if (AutoAssetLoader.MonitorActive)
            {
                var guid = AssetDatabase.AssetPathToGUID(assetName);
                if (!string.IsNullOrWhiteSpace(guid))
                {
                    AutoAssetLoader.LoadSettings();
                    AutoAssetLoader.Generate();
                }
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
