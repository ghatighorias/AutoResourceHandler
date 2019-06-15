using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;

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
            ClassDescriptor.fileName = CreateTextInput("File Name",
                ClassDescriptor.fileName, new Rect(10, 10, 110, 15), inputMargin: 10);

            ClassDescriptor.saveLocation = CreateTextInput("Save Location",
                ClassDescriptor.saveLocation, new Rect(10, 30, 110, 15), dragDrop: true, inputMargin: 10);

            ClassDescriptor.enumPrefix = CreateTextInput("Enum Name Prefix",
                ClassDescriptor.enumPrefix, new Rect(10, 50, 110, 15), inputMargin: 10);

            ClassDescriptor.capitalizeAssetNames = GUI.Toggle(new Rect(10, 70, 250, 20),
                ClassDescriptor.capitalizeAssetNames, "Capitalize asset names");

            ClassDescriptor.trimItemNameByEnumGenerationOption = GUI.Toggle(new Rect(10, 90, 250, 20), 
                ClassDescriptor.trimItemNameByEnumGenerationOption, "Trim Item Name By Generation Option");

            ClassDescriptor.enumGenerationOption = CreateEnumDropdown("Generation mode",
                ClassDescriptor.enumGenerationOption, new Rect(10, 110, 110, 20), inputMargin: 10);


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

        /// <summary>
        /// Creates a button and call its call back when pressed
        /// </summary>
        /// <param name="label">button label</param>
        /// <param name="buttonRect">rectangle describing the display area for the button</param>
        /// <param name="action">callback to be called when button is pressed</param>
        void CreateButton(string label, Rect buttonRect, Action action)
        {
            if (GUI.Button(buttonRect, label))
                action?.Invoke();
        }

        /// <summary>
        /// Creates a textfield and returns its edited value
        /// </summary>
        /// <param name="label">label to show on the left of textfield</param>
        /// <param name="value">current value of the field</param>
        /// <param name="rect">rectangle representing the display area of label and textfield</param>
        /// <param name="inputMargin">horizental margin between label and textfield</param>
        /// <param name="dragDrop">describes whether field accepts drag and drop or not</param>
        /// <returns>new value of the text field</returns>
        string CreateTextInput(string label, string value, Rect rect, float inputMargin = 20, bool dragDrop = false)
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
                            value = DragAndDrop.paths[0];
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Creates a drop down to select a value from an enum and return the selected value
        /// </summary>
        /// <typeparam name="T">requested enum type</typeparam>
        /// <param name="label">label to show on the left of dropdown</param>
        /// <param name="selectedEntry">currently selected value</param>
        /// <param name="rect">rectangle representing the display area of label and dropdown</param>
        /// <param name="inputMargin">horizental margin between label and dropdown</param>
        /// <returns>selected value from the enum</returns>
        T CreateEnumDropdown<T>(string label, T selectedEntry, Rect rect, float inputMargin = 20)
        {
            Rect labelRect = new Rect(rect.xMin, rect.yMin, rect.width, rect.height);
            GUI.Label(labelRect, label);

            var enumType = typeof(T);
            var options = Enum.GetNames(enumType);
            var selectedEntryIndex = Array.FindIndex(options, entry => entry == selectedEntry.ToString());
            Rect valueRect = new Rect(rect.xMin + rect.width + inputMargin, rect.yMin, rect.width, rect.height);
            selectedEntryIndex = EditorGUI.Popup(valueRect, selectedEntryIndex, options);

            return (T)Enum.Parse(enumType, options[selectedEntryIndex], true);
        }
    }
}