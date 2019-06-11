using System;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

using System.Linq;

namespace AutoAssetLoader
{
    // TO-DO : throw exception when an enum is passed that has no description or its first description not guid of an asset
    public static class ResourceHandlerBase<T>
    {
        static string GetEnumDescription(T value)
        {
            return value
                .GetType()
                .GetField(value.ToString())
                .GetCustomAttribute<DescriptionAttribute>(false)
                .Description;
        }

        public static bool ContainsGuid(string guid)
        {
            return typeof(T)
                .GetType()
                .GetCustomAttributes<DescriptionAttribute>()
                .Any((attribute) => attribute.Description == guid);
        }

        /// <summary>
        /// Loads a game object from using its mapped enum
        /// </summary>
        public static UnityEngine.GameObject Load(T resource)
        {
            return Load<UnityEngine.GameObject>(resource);
        }

        /// <summary>
        /// Loads an asset from using its mapped enum
        /// </summary>    
        public static G Load<G>(T resource)
        {
            return AssetLoad<G>(GetEnumDescription(resource));
        }

        /// <summary>
        /// Load an asset using its guid
        /// </summary>
        static G AssetLoad<G>(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var value = AssetDatabase.LoadAssetAtPath(path, typeof(G));
            return (G)Convert.ChangeType(value, typeof(G));
        }
    }
}