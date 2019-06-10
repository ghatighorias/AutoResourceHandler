using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System;

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
            return Directory.EnumerateFiles("Assets/Resources", $"{searchTerm}", SearchOption.AllDirectories)
                        .ToList()
                        .ConvertAll<FileItemDescriptor>((path) => new FileItemDescriptor()
                        {
                            path = path,
                            name = Path.GetFileNameWithoutExtension(path),
                            directory = Path.GetDirectoryName(path),
                            guid = AssetDatabase.AssetPathToGUID(path)
                        })
                        .ToList();
        }
    }
}
