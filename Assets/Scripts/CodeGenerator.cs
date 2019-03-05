using System;
using System.Collections.Generic;
using System.Text;

namespace AutoPrefabLoader
{
    public static class CodeGenerator
    {
        private static StringBuilder AddIndentation(StringBuilder content)
        {
            var indentedContent = new StringBuilder();

            string[] delim = { Environment.NewLine, "\n" }; // "\n" added in case you manually appended a newline
            string[] lines = content.ToString().Split(delim, StringSplitOptions.None);
            foreach (string line in lines)
            {
                indentedContent.AppendLine(string.Format("\t{0}", line));
            }

            return indentedContent;
        }

        private static StringBuilder AddScope(StringBuilder content, bool close = false, bool newLineAfter = false)
        {
            var closeChar = close ? ";" : String.Empty;
            var newLineAfterChar = newLineAfter ? "\n" : string.Empty;

            return AddIndentation(content)
               .Insert(0, "\n{\n")
               .Append(string.Format("}}{0}{1}",closeChar, newLineAfterChar));
        }

        public static StringBuilder AddDictionaryField(bool isPublic, bool isStatic, string type, string name, List<EnumItemDescriptor> elements)
        {
            string accessModifier = isPublic ? "public" : "private";
            string staticText = isStatic ? "static" : string.Empty;

            var content = new StringBuilder();

            for (int index = 0; index < elements.Count; index++)
            {
                if (index + 1 == elements.Count)
                {
                    content.Append(string.Format("{{{0}, \"{1}\"}},", elements[index].name, elements[index].mappedValue));
                }
                else
                {
                    content.AppendLine(string.Format("{{{0}, \"{1}\"}},", elements[index].name, elements[index].mappedValue));
                }
            }

            return AddScope(content, true, true)
                   .Insert(0, string.Format("{0} {1} {2} {3} = new {2}()", accessModifier, staticText, type, name));
        }

        public static StringBuilder AddClass(bool isPublic, bool isStatic, string name, StringBuilder content)
        {
            string accessModifier = isPublic ? "public" : "private";
            string staticText = isStatic ? "static" : string.Empty;

            return AddScope(content, newLineAfter: true)
                .Insert(0, string.Format("{0} {1} partial class {2}", accessModifier, staticText, name));
        }

        public static StringBuilder AddMethod(bool isPublic,bool isStatic, string returnType, string name, string parameters, string content)
        {
            string accessModifier = isPublic ? "public" : "private";
            string staticText = isStatic ? "static" : string.Empty;

            return AddScope(new StringBuilder(content))
                .Insert(0, string.Format("{0} {1} {2} {3}({4})", accessModifier, staticText, returnType, name, parameters));
        }

        public static StringBuilder AddEnum(bool isPublic, string name, StringBuilder content)
        {
            string accessModifier = isPublic ? "public" : "private";

            return AddScope(content)
                .Insert(0, string.Format("{0} enum {1}", accessModifier, name));
        }

        public static StringBuilder AddEnumContent(List<EnumItemDescriptor> elements)
        {
            var content = new StringBuilder();

            for (int index = 0; index < elements.Count; index++)
            {
                if (index + 1 == elements.Count)
                {
                    content.Append(string.Format("{0},", elements[index].name));
                }
                else
                {
                    content.AppendLine(string.Format("{0},", elements[index].name));
                }
            }

            return content;
        }

        public static StringBuilder AddUsing(params string[] usings)
        {
            var content = new StringBuilder();

            foreach (var item in usings)
            {
                content.AppendLine(string.Format("using {0};", item));
            }

            return content;
        }

        public static StringBuilder AddEmptyLines(int number, bool append,  StringBuilder content)
        {
            for (int counter = 0; counter < number; counter++)
            {
                if (append)
                {
                    content.AppendLine();
                }
                else
                {
                    content.Insert(0, Environment.NewLine);
                }
            }

            return content;
        }

        public static StringBuilder MergeContent(params string[] content)
        {
            var mergedContent = new StringBuilder();

            foreach (var item in content)
            {
                mergedContent.AppendLine(item);
            }

            return mergedContent;
        }

        public static StringBuilder AddNameSpace(string name, StringBuilder content)
        {
            return AddScope(content)
                .Insert(0, string.Format("namespace {0}", name));
        }
    }
}
