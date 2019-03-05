using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using AutoPrefabLoader;
using System.Text;

public static class PrefabAutoResource
{
    static ClassDescriptor classDescriptor;

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
        return string.Format("{0}Loader", descriptor.name);
    }

    static string GetMapperName(EnumDescriptor descriptor)
    {
        return string.Format("{0}_Value_Mapper", descriptor.name);
    }

    static string GenerateNameSpace(EnumDescriptor descriptor)
    {
        return string.Empty;
    }

    static string GenerateEnumCode(EnumDescriptor descriptor)
    {
        var content = CodeGenerator.AddEnumContent(descriptor.elements);

        var enumbuilder = CodeGenerator.AddEnum(true, GetEnumName(descriptor), content);

        return enumbuilder.ToString();
    }

    static string GenerateEnumMappeDictionary(EnumDescriptor descriptor)
    {
        var fieldType = string.Format("Dictionary<{0},string>", GetEnumName(descriptor));

        var fixedElementList = descriptor.elements.Select((element) => new EnumItemDescriptor()
        {
            name = string.Format("{0}.{1}", GetEnumName(descriptor), element.name),
            mappedValue = element.mappedValue,
            itemValue = element.itemValue
        }).ToList();

        var mapperCode = CodeGenerator.AddDictionaryField(true, true, fieldType, GetMapperName(descriptor), fixedElementList);

        return mapperCode.ToString();
    }

    static string GenerateGenericEnumLoader(EnumDescriptor descriptor)
    {
        var content = string.Format("return PrefabAutoResource.AutoLoad<T>({0}[resource]);", GetMapperName(descriptor));
        var methodCode = CodeGenerator.AddMethod(true, true, "T", "Load<T>", string.Format("{0} resource", GetEnumName(descriptor)), content);

        return methodCode.ToString();
    }

    static string GenerateGameobjectEnumLoader(EnumDescriptor descriptor)
    {
        var content = "return Load<GameObject>(resource);";
        var methodCode = CodeGenerator.AddMethod(true, true, "GameObject", "Load", string.Format("{0} resource", GetEnumName(descriptor)), content);

        return methodCode.ToString();
    }

    public static string GenerateClass(EnumDescriptor descriptor)
    {
        var usingContent = CodeGenerator.AddUsing(new string[] {
            "UnityEngine",
            "System.Collections.Generic" })
            .ToString();

        var enumCode = GenerateEnumCode(descriptor);
        var dictionaryMapper = GenerateEnumMappeDictionary(descriptor);
        var genericLoader = GenerateGenericEnumLoader(descriptor);
        var gameobjectLoader = GenerateGameobjectEnumLoader(descriptor);

        var ClassContent = CodeGenerator.MergeContent(new string[]{
            dictionaryMapper,
            gameobjectLoader,
            genericLoader
        });

        var classCode = CodeGenerator.AddClass(true, true, GetClassName(descriptor), ClassContent).ToString();

        return CodeGenerator.MergeContent(usingContent, classCode, enumCode).ToString();
    }

    public static string GenerateClass(EnumDescriptor descriptor, ClassDescriptor generatedClassDescriptor)
    {
        return string.Empty;
    }
}

public struct ClassDescriptor
{
    public string saveLocation;
    public string nameSpaceName;
    public string className;
    public string enumName;
    public string MapperName;
}

public struct EnumDescriptor
{
    public string name;
    public Dictionary<string, string> items;
    public List<EnumItemDescriptor> elements;
}

public struct EnumItemDescriptor
{
    public string name;
    public string mappedValue;
    public int itemValue;
}