using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System;

namespace AutoAssetLoader
{
    public partial class AutoAssetLoader : EditorWindow
    {
        static readonly string windowName = "Auto resource generator Settings";
        static readonly Vector2 settingsWindowSize = new Vector2(250, 250);

        [MenuItem("Assets/Autoresoruce/Settings")]
        static void ShowSettings()
        {
            LoadSettings();

            var window = CreateInstance<AutoAssetLoader>();

            if (window)
            {
                window.position = new Rect(Screen.width / 2, Screen.height / 2, settingsWindowSize.x, settingsWindowSize.y);
                window.titleContent = new GUIContent(windowName);
                window.minSize = settingsWindowSize;
                window.maxSize = settingsWindowSize;
                window.ShowUtility();
            }
        }

        [MenuItem("Assets/Autoresoruce/Manual Generate")]
        static void ManualGenerate()
        {
            Generate();
        }

        void OnGUI()
        {
            CreateTextInput("Namespace", ref ClassDescriptor.namespaceName, new Rect(10, 10, 100, 15));
            CreateTextInput("Class name", ref ClassDescriptor.className, new Rect(10, 30, 100, 15));
            CreateTextInput("Save location", ref ClassDescriptor.saveLocation, new Rect(10, 50, 100, 15), dragDrop: true);
            CreateTextInput("Item name prefix", ref ClassDescriptor.itemNamePrefix, new Rect(10, 70, 100, 15));

            ClassDescriptor.itemNameToUpper = GUI.Toggle(new Rect(10, 90, 250, 20), ClassDescriptor.itemNameToUpper, "Change Item names to uppercase");
            // the code is not implemented for this section yet
            GUI.enabled = false;
            ClassDescriptor.seperateEnumForPrefabs = GUI.Toggle(new Rect(10, 110, 250, 20), ClassDescriptor.seperateEnumForPrefabs, "Create a seperate enum for prefabs");
            ClassDescriptor.seperateEnumPerFolder = GUI.Toggle(new Rect(10, 130, 250, 20), ClassDescriptor.seperateEnumPerFolder, "Create seperate enum per each folder");
            GUI.enabled = true;

            ClassDescriptor.staticClass = GUI.Toggle(new Rect(10, 150, 250, 20), ClassDescriptor.staticClass, "Make resource handler static");

            MonitorActive = GUI.Toggle(new Rect(10, 170, 250, 20), MonitorActive, "Auto refresh if resources changed");

            CreateButton("Save and generate", new Rect(50, 190, 150, 20), () => {
                SaveSettings();
                Generate();
                this.Close();
            });

            CreateButton("Save and close", new Rect(50, 220, 150, 20), () => {
                SaveSettings();
                this.Close();
            });
        }

        private void CreateButton(string label, Rect buttonRect, Action action)
        {
            if (GUI.Button(buttonRect, label))
                action?.Invoke();
        }

        void CreateTextInput(string label, ref string value, Rect rect, float inputMargin = 20, bool dragDrop = false)
        {
            Rect labelRect = new Rect(rect.xMin, rect.yMin, rect.width, rect.height);
            GUI.Label(labelRect, label);
            Rect valueRect = new Rect(rect.xMin + rect.width + inputMargin, rect.yMin, rect.width, rect.height);
            value = GUI.TextField(valueRect, value);

            if (dragDrop)
            {
                if (valueRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.DragUpdated)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        Event.current.Use();
                    }
                    if (Event.current.type == EventType.DragPerform)
                    {
                        if (DragAndDrop.paths.Length > 0)
                            ClassDescriptor.saveLocation = DragAndDrop.paths[0];
                    }
                }
            }
        }
    }
}