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

        public static bool Active = false;
        public static ClassDescriptor ClassDescriptor { get; private set; }

        public void SaveSettings()
        {
            //save the updated data to a jsonfile for next times that is being loaded
        }

        public static void LoadSettings()
        {
            if (loadSettingsFinished)
                return;
            
            //check if there is any previous data saved
            // load if there is any

            ClassDescriptor = new ClassDescriptor()
            {
                namespaceName = "AutoAssetLoader",
                className = "_ResourHandler",
                saveLocation = "Scripts",
                itemNamePrefix = "ITEM_",
                itemNameToUpper = true,
                seperateEnumForPrefabs = true,
                seperateEnumPerFolder = false,
                staticClass = false
            };

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
