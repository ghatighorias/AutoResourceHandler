using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoAssetLoader
{
    public partial class AutoAssetLoader
    {
        static bool loadSettingsFinished = false;
        public static bool MonitorActive = false;
        public static ClassDescriptor ClassDescriptor { get; private set; }

        public static void SaveSettings()
        {
            try
            {
                EditorPrefs.SetString("AutoAssetLoader_ClassDescriptor_Namespace", ClassDescriptor.namespaceName);
                EditorPrefs.SetString("AutoAssetLoader_ClassDescriptor_ClassName", ClassDescriptor.className);
                EditorPrefs.SetString("AutoAssetLoader_ClassDescriptor_SaveLocation", ClassDescriptor.saveLocation);
                EditorPrefs.SetString("AutoAssetLoader_ClassDescriptor_ItemNamePrefix", ClassDescriptor.itemNamePrefix);
                EditorPrefs.SetBool("AutoAssetLoader_ClassDescriptor_ItemNameToUpper", ClassDescriptor.itemNameToUpper);
                EditorPrefs.SetBool("AutoAssetLoader_ClassDescriptor_SeperateEnumForPrefabs", ClassDescriptor.seperateEnumForPrefabs);
                EditorPrefs.SetBool("AutoAssetLoader_ClassDescriptor_SeperateEnumPerFolder", ClassDescriptor.seperateEnumPerFolder);
                EditorPrefs.SetBool("AutoAssetLoader_ClassDescriptor_StaticClass", ClassDescriptor.staticClass);
                EditorPrefs.SetBool("AutoAssetLoader_Monitor_Active", MonitorActive);
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
                namespaceName = EditorPrefs.GetString("AutoAssetLoader_ClassDescriptor_Namespace", "AutoAssetLoader"),
                className = EditorPrefs.GetString("AutoAssetLoader_ClassDescriptor_ClassName", "_ResourHandler"),
                saveLocation = EditorPrefs.GetString("AutoAssetLoader_ClassDescriptor_SaveLocation", "Scripts"),
                itemNamePrefix = EditorPrefs.GetString("AutoAssetLoader_ClassDescriptor_ItemNamePrefix", "ITEM_"),
                itemNameToUpper = EditorPrefs.GetBool("AutoAssetLoader_ClassDescriptor_ItemNameToUpper", true),
                seperateEnumForPrefabs = EditorPrefs.GetBool("AutoAssetLoader_ClassDescriptor_SeperateEnumForPrefabs", true),
                seperateEnumPerFolder = EditorPrefs.GetBool("AutoAssetLoader_ClassDescriptor_SeperateEnumPerFolder", false),
                staticClass = EditorPrefs.GetBool("AutoAssetLoader_ClassDescriptor_StaticClass", false)
            };

            MonitorActive = EditorPrefs.GetBool("AutoAssetLoader_Monitor_Active", false);

            loadSettingsFinished = true;
        }

        public static void Generate()
        {
            try
            {
                var foundItems = GetFileItems("*prefab");

                Assets.Editor.Scripts.ResourceHandlerCodeGenerator.GenerateAndSave(foundItems, ClassDescriptor);

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

            MarkExistingNames(foundFiles);

            return foundFiles;
        }

        /// <summary>
        /// Mark the nameExist for the given items if there are repeatative names
        /// </summary>
        /// <param name="fileItems">List of items to check and mark</param>
        static void MarkExistingNames(List<FileItemDescriptor> fileItems)
        {
            foreach (var fileItem in fileItems)
            {
                fileItem.nameExist = fileItems.Count((item) => item.NormalizedName.ToLower() == fileItem.NormalizedName.ToLower()) > 1;
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
        static Regex nameNormalizerRegex = new Regex("([^a-zA-Z_0-9]+)");

        public string name;
        public string NormalizedName { get { return GetNormalizedName(name); } }
        public string path;
        public string guid;
        public string directory;
        public string NormalizedDirectory { get { return GetNormalizedName(directory); } }
        public bool nameExist;

        static string GetNormalizedName(string input)
        {

            return nameNormalizerRegex
                .Replace(input, "_")
                .Trim('_');
        }
    }
}
