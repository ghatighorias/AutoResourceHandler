using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AutoAssetLoader
{
    /// <summary>
    /// This class is used as code generator input
    /// </summary>
    public class ClassDescriptor
    {
        public string saveLocation;
        public string namespaceName;
        public string className;
        public string EnumName { get { return string.Format("{0}Enum", className); } }
        public string MapperName { get { return string.Format("{0}Mapper", className); } }
        public string itemNamePrefix;
        public bool itemNameToUpper;
        public bool seperateEnumForPrefabs;
        public bool seperateEnumPerFolder;
        public bool staticClass;
    }

    /// <summary>
    /// This class describes the selected item from the editor
    /// </summary>
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

    public class FileItemDescriptor
    {
        public string name;
        public string path;
        public string directory;
        public string guid;
    }
}