using System;
using UnityEditor;
using System.Collections.Generic;

public abstract class ResourceHandlerBase<T>
{
    Dictionary<T, string> _ResourHandlerMapper;

    public int ResourceCount { get { return _ResourHandlerMapper.Count; } }

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