using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AutoAssetLoader
{
    public class AutoResourceHandler
    {
        static bool loadSettingsFinished = false;

        public static bool MonitorActive = false;
        public static ClassDescriptor ClassDescriptor { get; private set; }



        public static void SaveSettings()
        {
            try
            {
                PlayerPrefs.SetString("AutoAssetLoader_ClassDescriptor_Namespace", ClassDescriptor.namespaceName);
                PlayerPrefs.SetString("AutoAssetLoader_ClassDescriptor_ClassName", ClassDescriptor.className);
                PlayerPrefs.SetString("AutoAssetLoader_ClassDescriptor_SaveLocation", ClassDescriptor.saveLocation);
                PlayerPrefs.SetString("AutoAssetLoader_ClassDescriptor_ItemNamePrefix", ClassDescriptor.itemNamePrefix);
                PlayerPrefs.SetInt("AutoAssetLoader_ClassDescriptor_ItemNameToUpper", Convert.ToInt32(ClassDescriptor.itemNameToUpper));
                PlayerPrefs.SetInt("AutoAssetLoader_ClassDescriptor_SeperateEnumForPrefabs", Convert.ToInt32(ClassDescriptor.seperateEnumForPrefabs));
                PlayerPrefs.SetInt("AutoAssetLoader_ClassDescriptor_SeperateEnumPerFolder", Convert.ToInt32(ClassDescriptor.seperateEnumPerFolder));
                PlayerPrefs.SetInt("AutoAssetLoader_ClassDescriptor_StaticClass", Convert.ToInt32(ClassDescriptor.staticClass));
                PlayerPrefs.SetInt("AutoAssetLoader_Monitor_Active", Convert.ToInt32(MonitorActive));
            }
            catch (Exception e)
            {
                Debug.Log($"AutoAssetLoader Failed to save settings: {e.Message}");
            }
        }

        public static void LoadSettings()
        {
            if (loadSettingsFinished)
                return;

            ClassDescriptor = new ClassDescriptor()
            {
                namespaceName = PlayerPrefs.GetString("AutoAssetLoader_ClassDescriptor_Namespace", "AutoAssetLoader"),
                className = PlayerPrefs.GetString("AutoAssetLoader_ClassDescriptor_ClassName", "_ResourHandler"),
                saveLocation = PlayerPrefs.GetString("AutoAssetLoader_ClassDescriptor_SaveLocation", "Scripts"),
                itemNamePrefix = PlayerPrefs.GetString("AutoAssetLoader_ClassDescriptor_ItemNamePrefix", "ITEM_"),
                itemNameToUpper = Convert.ToBoolean(PlayerPrefs.GetInt("AutoAssetLoader_ClassDescriptor_ItemNameToUpper", 1)),
                seperateEnumForPrefabs = Convert.ToBoolean(PlayerPrefs.GetInt("AutoAssetLoader_ClassDescriptor_SeperateEnumForPrefabs", 1)),
                seperateEnumPerFolder = Convert.ToBoolean(PlayerPrefs.GetInt("AutoAssetLoader_ClassDescriptor_SeperateEnumPerFolder", 0)),
                staticClass = Convert.ToBoolean(PlayerPrefs.GetInt("AutoAssetLoader_ClassDescriptor_StaticClass", 0))
            };

            MonitorActive = Convert.ToBoolean(PlayerPrefs.GetInt("AutoAssetLoader_Monitor_Active", 0));

            loadSettingsFinished = true;
        }

        public static void Generate()
        {
            try
            {
                var foundItems = GetFileItems("*prefab");

                Assets.Editor.Scripts.ResourceHandlerCodeGenerator.GenerateAndSave(foundItems);

                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.Log($"AutoAssetLoader Failed: {e.Message}");
            }

            return;
        }
        
        static List<FileItemDescriptor> GetFileItems(string searchTerm)
        {
            var foundFiles = Directory.EnumerateFiles("Assets/Resources", $"{searchTerm}", SearchOption.AllDirectories)
                        .ToList()
                        .ConvertAll<FileItemDescriptor>((path) => new FileItemDescriptor()
                        {
                            path = path,
                            name = Path.GetFileNameWithoutExtension(path),
                            directory = Path.GetDirectoryName(path),
                            guid = AssetDatabase.AssetPathToGUID(path)
                        })
                        .ToList();

            if (foundFiles.Count == 0)
                throw new FileNotFoundException($"No asset was found with the search term: {searchTerm}");

            return foundFiles;
        }
    }
}

/// <summary>
/// This class describes the settings of the asset loader code generator
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
/// This class describes a candidate item for assetloader code generator
/// </summary>
public class FileItemDescriptor
{
    public string name;
    public string path;
    public string directory;
    public string guid;
}
