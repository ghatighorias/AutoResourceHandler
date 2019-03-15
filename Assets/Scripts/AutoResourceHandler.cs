using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class AutoResourceHandler
{
    public AutoResourceHandler()
    {
    }
}

public class ClassDescriptor
{
    public string saveLocation;
    public string namespaceName;
    public string className;
    public string EnumName { get { return string.Format("{0}Enum", className); } }
    public string MapperName { get { return string.Format("{0}Mapper", className); } }

    public List<SelectedItem> items = new List<SelectedItem>();
}

public class SelectedItem
{
    public string name;
    public string guid;
    public GameObject item;
    public string AssetPath
    {
        get
        {
            return AssetDatabase.GUIDToAssetPath(guid);
        }
    }
}