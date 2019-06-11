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
        public static ClassDescriptor ClassDescriptor { get; private set; }

        static List<string> exclusionList = null;
        static bool loadSettingsFinished = false;
        const string assetSearchRootDirectory = "Assets";

        /// <summary>
        /// Save current state of ClassDescriptor property of this class
        /// </summary>
        public static void SaveSettings()
        { 
            try
            {
                EditorPrefs.SetString("AutoAssetLoader_ClassDescriptor_FileName", ClassDescriptor.fileName);
                EditorPrefs.SetString("AutoAssetLoader_ClassDescriptor_SaveLocation", ClassDescriptor.saveLocation);
                EditorPrefs.SetString("AutoAssetLoader_ClassDescriptor_EnumPrefix", ClassDescriptor.enumPrefix);

                EditorPrefs.SetString("AutoAssetLoader_ClassDescriptor_EnumGenerationOption", ClassDescriptor.enumGenerationOption.ToString());

                EditorPrefs.SetBool("AutoAssetLoader_ClassDescriptor_CapitalizeAssetNames", ClassDescriptor.capitalizeAssetNames);
                EditorPrefs.SetBool("AutoAssetLoader_ClassDescriptor_TrimItemNameByEnumGenerationOption", ClassDescriptor.trimItemNameByEnumGenerationOption);
            }
            catch (Exception e)
            {
                Debug.Log($"AutoAssetLoader Failed to save settings: {e.Message}");
            }
        }

        /// <summary>
        /// Attempt to load previously saved settings for ClassDescriptor
        /// If no previously saved data available, will set a recommended setting for ClassDescriptor
        /// </summary>
        public static void LoadSettings()
        {
            if (loadSettingsFinished)
                return;

            try
            {
                ClassDescriptor = new ClassDescriptor()
                {
                    fileName = EditorPrefs.GetString("AutoAssetLoader_ClassDescriptor_FileName", "_ResourHandler"),
                    saveLocation = EditorPrefs.GetString("AutoAssetLoader_ClassDescriptor_SaveLocation", "Scripts"),
                    enumPrefix = EditorPrefs.GetString("AutoAssetLoader_ClassDescriptor_EnumPrefix", "AssetLoader_"),
                    capitalizeAssetNames = EditorPrefs.GetBool("AutoAssetLoader_ClassDescriptor_CapitalizeAssetNames", true),
                    trimItemNameByEnumGenerationOption = EditorPrefs.GetBool("AutoAssetLoader_ClassDescriptor_TrimItemNameByEnumGenerationOption", false),
                };

                var loadedEnumGenerationOption = EditorPrefs.GetString("AutoAssetLoader_ClassDescriptor_EnumGenerationOption", ClassDescriptor.EnumGenerationOption.Type.ToString());
                Enum.TryParse<ClassDescriptor.EnumGenerationOption>(loadedEnumGenerationOption, out ClassDescriptor.enumGenerationOption);


                loadSettingsFinished = true;
            }
            catch (Exception e)
            {
                Debug.Log($"AutoAssetLoader Failed to load settings: {e.Message}");
            }
        }

        /// <summary>
        /// Generate the needed Enums using the ClassDescriptor property value
        /// </summary>
        public static void Generate()
        {
            try
            {
                var foundItems = GetFileItems(assetSearchRootDirectory);

                var categorizedItems = GetCategorizeDescriptors(foundItems);

                var filteredCategorizedItems = FilterFileItemDescriptor(categorizedItems, exclusionList);
                
                CodeGenerator.ResourceHandlerCodeGenerator.GenerateAndSave(categorizedItems, ClassDescriptor);
                
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.Log($"AutoAssetLoader Failed: {e.Message}");
            }

            return;
        }

        /// <summary>
        /// Search for assets in the given path. and convert and return them as FileItemDescriptor
        /// Assets of type null will be ignored (e.g. .meta files)
        /// Will throw NullReferenceException if searchDirectory is empty whitespace or null
        /// Will throw FileNotFoundException exception if no asset was found
        /// </summary>
        /// <param name="searchDirectory">Directory to search through</param>
        /// <returns>List of FileItemDescriptor created from found items</returns>
        static List<FileItemDescriptor> GetFileItems(string searchDirectory)
        {
            if (string.IsNullOrWhiteSpace(searchDirectory))
                throw new NullReferenceException("search directory cannot be empty, whitespace or null");

            var foundFiles = Directory.EnumerateFiles(searchDirectory, "*", SearchOption.AllDirectories)
                        .Select((path) => new FileItemDescriptor() {
                            path = path,
                            name = Path.GetFileNameWithoutExtension(path),
                            extention = Path.GetExtension(path),
                            directory = Path.GetDirectoryName(path),
                            guid = AssetDatabase.AssetPathToGUID(path),
                            type = AssetDatabase.GetMainAssetTypeAtPath(path) })
                        .Where((itemDescriptor) => { return itemDescriptor.type != null; })
                        .ToList();

            var markedFiles = GetFileItemDescriptorWithMarkedItems(foundFiles);

            if (markedFiles == null || markedFiles.Count == 0)
                throw new FileNotFoundException($"No asset was found with in {searchDirectory} directory");

            return markedFiles;
        }

        /// <summary>
        /// Update the nameExist of items in the given filItem list if there are repeatative names among them
        /// </summary>
        /// <param name="fileItems">List of items to check and mark</param>
        static List<FileItemDescriptor> GetFileItemDescriptorWithMarkedItems(List<FileItemDescriptor> fileItems)
        {
            /* regardless of the enum generation type there can be two item with the same name
             * or even the same extention and in different folders. it is needed therefore to
             * inform the code generator inorder to create a unique enum name
             */

            if (fileItems == null)
                return null;

            foreach (var fileItem in fileItems)
            {
                fileItem.nameExist = fileItems
                    .Count((item) => item.NormalizedName.ToLower() == fileItem.NormalizedName.ToLower()) > 1;
                fileItem.nameAndDirectoryExist = fileItem.nameExist && fileItems
                    .Count((item) => item.NormalizedDirectory.ToLower() == fileItem.NormalizedDirectory.ToLower()) > 1;
            }

            return fileItems.Select(fileItem =>
            {
                fileItem.nameExist = fileItems
                    .Count((item) => item.NormalizedName.ToLower() == fileItem.NormalizedName.ToLower()) > 1;
                fileItem.nameAndDirectoryExist = fileItem.nameExist && fileItems
                    .Count((item) => item.NormalizedDirectory.ToLower() == fileItem.NormalizedDirectory.ToLower()) > 1;
                return fileItem;
            })
            .ToList();
        }

        /// <summary>
        /// Filter out unwanted  files from the fileItems
        /// </summary>
        /// <param name="fileItems">List of fileItemDescriptor to filter through</param>
        /// <param name="exclusionList">List of types that should be filtered out</param>
        /// <returns>Filtered list</returns>
        static Dictionary<string, List<FileItemDescriptor>> FilterFileItemDescriptor(Dictionary<string, List<FileItemDescriptor>> categorizedItems, List<string> exclusionList)
        {
            var excludedItemDictionary = new Dictionary<string, List<FileItemDescriptor>>();

            if (categorizedItems == null)
                return excludedItemDictionary;
            if (exclusionList == null)
                return categorizedItems;
            

            switch (ClassDescriptor.enumGenerationOption)
            {
                case ClassDescriptor.EnumGenerationOption.Type:
                    excludedItemDictionary = categorizedItems
                    .Where(categorizedItem => !categorizedItem.Value.Any(itemDescriptor => exclusionList.Contains(itemDescriptor.type.Name.ToLower())))
                    .ToDictionary(item => item.Key, item => item.Value);
                    break;
                case ClassDescriptor.EnumGenerationOption.Folder:
                    excludedItemDictionary = categorizedItems
                    .Where(categorizedItem => !categorizedItem.Value.Any(itemDescriptor => exclusionList.Contains(itemDescriptor.directory.ToLower())))
                    .ToDictionary(item => item.Key, item => item.Value);
                    break;
                case ClassDescriptor.EnumGenerationOption.Extention:
                    excludedItemDictionary = categorizedItems
                    .Where(categorizedItem => !categorizedItem.Value.Any(itemDescriptor => exclusionList.Contains(itemDescriptor.NormalizedExtention.ToLower())))
                    .ToDictionary(item => item.Key, item => item.Value);
                    break;
            }

            return excludedItemDictionary;
        }

        /// <summary>
        /// Categorize the FileItemDescriptor list using EnumGenerationOption from ClassDescriptor
        /// </summary>
        /// <param name="fileItemDescriptors">Input list to be categorizes</param>
        /// <returns>Categorized dictionary</returns>
        static Dictionary<string, List<FileItemDescriptor>> GetCategorizeDescriptors(List<FileItemDescriptor> fileItemDescriptors)
        {
            var categorizedDescriptos = new Dictionary<string, List<FileItemDescriptor>>();

            if (fileItemDescriptors == null)
                return categorizedDescriptos;

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
        public EnumGenerationOption enumGenerationOption;
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
    /// This class describes a items that will passed to assetloader code generator
    /// </summary>
    public class FileItemDescriptor
    {
        static Regex nameNormalizerRegex = new Regex("([^a-zA-Z_0-9]+)");

        public Type type;
        public string path;
        public string guid;
        public string name;
        public bool nameExist;
        public bool nameAndDirectoryExist;
        public string NormalizedName { get { return GetNormalizedName(name); } }
        public string directory;
        public string NormalizedDirectory { get { return GetNormalizedName(directory); } }
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
