using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection.Emit;

public class SampleMentu : MonoBehaviour
{

    [MenuItem("Resources/Generate Handler")]
    private static void GenerateResourceHandlersToolbar()
    {

    }

    [MenuItem("Assets/Generate Handler")]
    private static void GenerateResourceHandlers()
    {

        EnumDescriptor descriptor = new EnumDescriptor()
        {
            name = "MySampleSet",
            items = new Dictionary<string, string>()
        };

        var prefabs = new List<SelectedPrefab>();

        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            prefabs.Add(new SelectedPrefab()
            {
                name = Selection.gameObjects[i].name.ToUpper(),
                guid = Selection.assetGUIDs[i],
            });
        }

        foreach (var item in prefabs)
        {
            descriptor.items.Add(string.Format("PREFAB_{0}", item.name), item.AssetPath);
        }

        var fileName = string.Format("./Assets/{0}Loader.cs", descriptor.name);
        using (var writer = System.IO.File.CreateText(fileName))
        {
            writer.WriteLine(PrefabAutoResource.GenerateClass(descriptor));
            writer.Flush();
        }

        AssetDatabase.Refresh();
    } 
}



public class SelectedPrefab
{
    public string name;
    public string guid;
    public string AssetPath { get { return AssetDatabase.GUIDToAssetPath(guid); } }

}