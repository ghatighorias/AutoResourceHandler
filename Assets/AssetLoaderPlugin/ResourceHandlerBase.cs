using System;
using UnityEditor;
using System.Collections.Generic;

namespace AutoAssetLoader
{
    public abstract class ResourceHandlerBase<T>
    {
        protected abstract Dictionary<T, string> _ResourHandlerMapper { get; set; }

        public int ResourceCount { get { return _ResourHandlerMapper.Count; } }

        public ResourceHandlerBase()
        {
            _ResourHandlerMapper = new Dictionary<T, string>();
        }

        public bool ContainsGuid(string guid)
        {
            return _ResourHandlerMapper.ContainsValue(guid);
        }

        /// <summary>
        /// Loads a game object from using its mapped enum
        /// </summary>
        public UnityEngine.GameObject Load(T resource)
        {
            return Load<UnityEngine.GameObject>(resource);
        }

        /// <summary>
        /// Loads an asset from using its mapped enum
        /// </summary>    
        public G Load<G>(T resource)
        {
            return AssetLoad<G>(_ResourHandlerMapper[resource]);
        }

        /// <summary>
        /// Load an asset using its path
        /// </summary>
        private static G AssetLoad<G>(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var value = UnityEngine.Resources.Load(path);
            return (G)Convert.ChangeType(value, typeof(T));
        }
    }
}