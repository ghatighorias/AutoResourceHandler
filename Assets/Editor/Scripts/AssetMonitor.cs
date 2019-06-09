using System;
using UnityEngine;
using UnityEditor;

class AssetMonitor : UnityEditor.AssetModificationProcessor
{
    private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
    {
        

        // Perform operations on the asset and set the value of 'assetMoveResult' accordingly.
        return AssetMoveResult.DidNotMove;
    }
}
