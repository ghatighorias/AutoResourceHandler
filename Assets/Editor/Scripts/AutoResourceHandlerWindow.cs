using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

namespace AutoAssetLoader
{
    public class AutoResourceHandlerWindow : EditorWindow
    {
        static readonly string windowName = "Auto resource generator Settings";
        static readonly Vector2 settingsWindowSize = new Vector2(250, 250);

        [MenuItem("Assets/Autoresoruce/Settings")]
        static void ShowSettings()
        {
            AutoResourceHandler.LoadSettings();
            
            AutoResourceHandlerWindow window = CreateInstance<AutoResourceHandlerWindow>();

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
        static async Task ManualGenerate()
        {
            await AutoResourceHandler.Generate();
        }

        async void OnGUI()
        {
            CreateTextInput("Namespace", ref AutoResourceHandler.ClassDescriptor.namespaceName, new Rect(10, 10, 100, 15));
            CreateTextInput("Class name", ref AutoResourceHandler.ClassDescriptor.className, new Rect(10, 30, 100, 15));
            CreateTextInput("Save location", ref AutoResourceHandler.ClassDescriptor.saveLocation, new Rect(10, 50, 100, 15), dragDrop: true);
            CreateTextInput("Item name prefix", ref AutoResourceHandler.ClassDescriptor.itemNamePrefix, new Rect(10, 70, 100, 15));

            AutoResourceHandler.ClassDescriptor.itemNameToUpper = GUI.Toggle(new Rect(10, 90, 250, 20), AutoResourceHandler.ClassDescriptor.itemNameToUpper, "Change Item names to uppercase");
            // the code is not implemented for this section yet
            GUI.enabled = false;
            AutoResourceHandler.ClassDescriptor.seperateEnumForPrefabs = GUI.Toggle(new Rect(10, 110, 250, 20), AutoResourceHandler.ClassDescriptor.seperateEnumForPrefabs, "Create a seperate enum for prefabs");
            AutoResourceHandler.ClassDescriptor.seperateEnumPerFolder = GUI.Toggle(new Rect(10, 130, 250, 20), AutoResourceHandler.ClassDescriptor.seperateEnumPerFolder, "Create seperate enum per each folder");
            GUI.enabled = true;

            AutoResourceHandler.Active = GUI.Toggle(new Rect(10, 150, 250, 20), AutoResourceHandler.Active, "Auto refresh if resources changed");

            Rect buttonRect = new Rect(50, 170, 150, 20);
            if (GUI.Button(buttonRect, "Update and Generate"))
            {
                await AutoResourceHandler.Generate();
                
                this.Close();
            }
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
                            AutoResourceHandler.ClassDescriptor.saveLocation = DragAndDrop.paths[0];
                    }
                }
            }
        }
    }
}