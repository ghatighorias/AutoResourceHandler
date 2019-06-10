using System;
using UnityEngine;
using UnityEditor;

namespace AutoAssetLoader
{
    class AssetMonitor : UnityEditor.AssetModificationProcessor
    {
        static void OnWillCreateAsset(string assetName)
        {
            if (AutoResourceHandler.MonitorActive)
            {
                var guid = AssetDatabase.AssetPathToGUID(assetName);
                if (!string.IsNullOrWhiteSpace(guid))
                {
                    AutoResourceHandler.LoadSettings();
                    AutoResourceHandler.Generate();
                }
            }
        }

        static AssetDeleteResult OnWillDeleteAsset(string assetName, RemoveAssetOptions removeAssetOptions)
        {
            if (AutoResourceHandler.MonitorActive)
            {
                var guid = AssetDatabase.AssetPathToGUID(assetName);
                if (!string.IsNullOrWhiteSpace(guid))
                {
                    AutoResourceHandler.LoadSettings();
                    AutoResourceHandler.Generate();
                }
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
