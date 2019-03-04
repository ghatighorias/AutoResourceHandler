using System;
using UnityEngine;
using System.Collections.Generic;

public static class PrefabAutoResource
{
    public static T AutoLoad<T>(string path)
    {
        var value = Resources.Load(path);
        return (T)Convert.ChangeType(value, typeof(T));
    }

    static string GetEnumName(EnumDescriptor descriptor)
    {
        return string.Format("{0}Items", descriptor.name);
    }

    static string GetClassName(EnumDescriptor descriptor)
    {
        return string.Format("{0}Items", descriptor.name);
    }

    static string GetMapperName(EnumDescriptor descriptor)
    {
        return string.Format("{0}Items", descriptor.name);
    }

    public static string GenerateEnumCode(EnumDescriptor descriptor)
    {
        var enumHeader = string.Format("public enum {0}\n{{\n", GetEnumName(descriptor));
        var enumBody = string.Empty;
        var enumEnd = "}";

        foreach (var item in descriptor.items)
        {
            enumBody += string.Format("\t{0},\n", item.Key);
        }

        return string.Format("{0}{1}{2}", enumHeader, enumBody, enumEnd);
    }

    public static string GenerateEnumMappeDictionary(EnumDescriptor descriptor)
    {
        var mapperHeader = string.Format("public static Dictionary<{0},string> {0}_Value_Mapper = new Dictionary<{0},string>()\n{{\n", GetEnumName(descriptor));
        var mapperBody = string.Empty;
        var mapperClosure = "};";

        foreach (var item in descriptor.items)
        {
            mapperBody += string.Format("\t{{{0}.{1}, \"{2}\"}},\n", GetEnumName(descriptor), item.Key, item.Value);
        }

        return string.Format("{0}{1}{2}", mapperHeader, mapperBody, mapperClosure);
    }

    public static string GenerateEnumLoader(EnumDescriptor descriptor)
    {
        var header = string.Format("public static T Load<T>({0} resource)\n{{\n", GetEnumName(descriptor));
        var body = string.Format("\treturn PrefabAutoResource.AutoLoad<T>({0}_Value_Mapper[resource]);", GetEnumName(descriptor));
        var closure = "\n}";

        return string.Format("{0}{1}{2}", header, body, closure);
    }

    public static string GenerateClass(EnumDescriptor descriptor)
    {
        string usings = "using System.Collections.Generic;\n";
        string classHeader = string.Format("public static class {0}Loader\n{{\n", descriptor.name);
        string classClosure = "\n}";

        var enumCode = PrefabAutoResource.GenerateEnumCode(descriptor);
        var mapperCode = PrefabAutoResource.GenerateEnumMappeDictionary(descriptor);
        var loaderCode = PrefabAutoResource.GenerateEnumLoader(descriptor);

        return string.Format("{0}{1}\n{2}\n{3}{4}\n{5}", usings, classHeader, mapperCode, loaderCode, classClosure, enumCode);
    }
}

public struct EnumDescriptor
{
    public string name;
    public Dictionary<string, string> items;
}