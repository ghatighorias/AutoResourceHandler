using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using AutoAssetLoader.CodeGenerator;
using System.Text.RegularExpressions;

namespace AutoAssetLoader
{
    //TO-DO : remove save and load. it is not that useful
    public partial class AutoAssetLoader
    {
        static bool loadSettingsFinished = false;
        public static ClassDescriptor ClassDescriptor { get; private set; }
        const string assetSearchRootDirectory = "Assets";
        static List<Type> exclusionList = new List<Type>()
        { typeof(DefaultAsset) };

        public static void SaveSettings()
        { 
            try
            {
                EditorPrefs.SetString("AutoAssetLoader_ClassDescriptor_FileName", ClassDescriptor.fileName);
                EditorPrefs.SetString("AutoAssetLoader_ClassDescriptor_SaveLocation", ClassDescriptor.saveLocation);
                EditorPrefs.SetString("AutoAssetLoader_ClassDescriptor_EnumPrefix", ClassDescriptor.enumPrefix);
                EditorPrefs.SetBool("AutoAssetLoader_ClassDescriptor_CapitalizeAssetNames", ClassDescriptor.capitalizeAssetNames);
                EditorPrefs.SetBool("AutoAssetLoader_ClassDescriptor_TrimItemNameByEnumGenerationOption", ClassDescriptor.trimItemNameByEnumGenerationOption);
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
                fileName = EditorPrefs.GetString("AutoAssetLoader_ClassDescriptor_FileName", "_ResourHandler"),
                saveLocation = EditorPrefs.GetString("AutoAssetLoader_ClassDescriptor_SaveLocation", "Scripts"),
                enumPrefix = EditorPrefs.GetString("AutoAssetLoader_ClassDescriptor_EnumPrefix", "AssetLoader_"),
                capitalizeAssetNames = EditorPrefs.GetBool("AutoAssetLoader_ClassDescriptor_CapitalizeAssetNames", true),
                trimItemNameByEnumGenerationOption = EditorPrefs.GetBool("AutoAssetLoader_ClassDescriptor_TrimItemNameByEnumGenerationOption", false),
            };
            
            loadSettingsFinished = true;
        }

        public static void Generate()
        {
            try
            {
                var foundItems = GetFileItems("*prefab");

                var categorizedItems = GetCategorizeDescriptors(foundItems);

                ResourceHandlerCodeGenerator.GenerateAndSave(categorizedItems, ClassDescriptor);
                
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
            var foundFiles = Directory.EnumerateFiles(assetSearchRootDirectory, $"*", SearchOption.AllDirectories)
                        .Select((path) => new FileItemDescriptor() {
                            path = path,
                            name = Path.GetFileNameWithoutExtension(path),
                            extention = Path.GetExtension(path),
                            directory = Path.GetDirectoryName(path),
                            guid = AssetDatabase.AssetPathToGUID(path),
                            type = AssetDatabase.GetMainAssetTypeAtPath(path) })
                        .Where((itemDescriptor) => { return itemDescriptor.type != null; })
                        .ToList();

            if (foundFiles.Count == 0)
                throw new FileNotFoundException($"No asset was found with the search term: {searchTerm}");

            var excludedList = AssetExclusion(foundFiles, exclusionList);

            MarkExistingNames(excludedList);

            return excludedList;
        }

        /// <summary>
        /// Update the nameExist of items in the given filItem list if there are repeatative names among them
        /// </summary>
        /// <param name="fileItems">List of items to check and mark</param>
        static void MarkExistingNames(List<FileItemDescriptor> fileItems)
        {
            /* regardless of the enum generation type there can be two item with the same name in a folder
             * only when the seperation is by extension such won't happen. since os won't allow two files
             *  with the same name and extention in a folder
             */

            switch (ClassDescriptor.enumGenerationOption)
            {
                case ClassDescriptor.EnumGenerationOption.Type:
                case ClassDescriptor.EnumGenerationOption.Folder:
                    foreach (var fileItem in fileItems)
                        fileItem.nameExist = fileItems
                            .Count((item) => item.NormalizedName.ToLower() == fileItem.NormalizedName.ToLower()) > 1;
                    break;
                default:
                    break;
            }
            
        }

        /// <summary>
        /// Filter out unwanted  files from the fileItems
        /// </summary>
        /// <param name="fileItems">List of fileItemDescriptor to filter through</param>
        /// <param name="exclusionList">List of types that should be filtered out</param>
        /// <returns>Filtered list</returns>
        static List<FileItemDescriptor> AssetExclusion(List<FileItemDescriptor> fileItems, List<Type> exclusionList)
        {
            return fileItems.FindAll((fileItem) => { return !exclusionList.Contains(fileItem.type); });
        }

        static Dictionary<string, List<FileItemDescriptor>> GetCategorizeDescriptors(List<FileItemDescriptor> fileItemDescriptors)
        {
            var categorizedDescriptos = new Dictionary<string, List<FileItemDescriptor>>();

            if (fileItemDescriptors != null)
            {
                switch (ClassDescriptor.enumGenerationOption)
                {
                    case ClassDescriptor.EnumGenerationOption.Type:
                        categorizedDescriptos = fileItemDescriptors
                            .GroupBy(fileItemDescriptor => fileItemDescriptor.type.Name.ToUpper())
                            .ToDictionary(item => item.Key, item => item.ToList());
                        break;
                    case ClassDescriptor.EnumGenerationOption.Folder:
                        categorizedDescriptos = fileItemDescriptors
                            .GroupBy(fileItemDescriptor => fileItemDescriptor.NormalizedDirectory.ToUpper())
                            .ToDictionary(item => item.Key, item => item.ToList());
                        break;
                    case ClassDescriptor.EnumGenerationOption.Extention:
                        categorizedDescriptos = fileItemDescriptors
                            .GroupBy(fileItemDescriptor => fileItemDescriptor.NormalizedExtention.ToUpper())
                            .ToDictionary(item => item.Key, item => item.ToList());
                        break;
                }
            }

            return categorizedDescriptos;
        }
    }

    /// <summary>
    /// This class describes the settings of the asset loader code generator
    /// </summary>
    public class ClassDescriptor
    {
        public string saveLocation;
        public string fileName;
        public string enumPrefix;
        public bool capitalizeAssetNames;
        public EnumGenerationOption enumGenerationOption = EnumGenerationOption.Extention; //TO-DO serialized for save and load
        public bool trimItemNameByEnumGenerationOption;

        /// <summary>
        /// Describes how items should be seperated in different enums
        /// </summary>
        public enum EnumGenerationOption
        {
            Type,
            Folder,
            Extention
        }
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
        public Type type;
        public string extention;
        public string NormalizedExtention { get { return GetNormalizedName(extention); } }

        static string GetNormalizedName(string input)
        {

            return nameNormalizerRegex
                .Replace(input, "_")
                .Trim('_');
        }
    }

    /// <summary>
    /// This class translate a class name using predefined set
    /// </summary>
    public class TypeNameTranslator
    {
        readonly Dictionary<Type, string> typeMatch;

        public TypeNameTranslator()
        {
            typeMatch = new Dictionary<Type, string>()
            {
                {typeof(GameObject), "Prefab" },
                {typeof(MonoBehaviour), "Script" }
            };
        }

        /// <summary>
        /// Attempt to get mapped name for the given type.
        /// will return Name from the given type if no match was found in the class
        /// </summary>
        /// <param name="type">Input to find the match name for</param>
        /// <returns>matched translation name</returns>
        public string GetName(Type type)
        {
            try
            {
                return typeMatch[type];
            }
            catch
            {
                return type.Name;
            }
        }
    }
}
