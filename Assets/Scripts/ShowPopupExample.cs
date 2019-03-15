using UnityEngine;
using UnityEditor;

public class ShowPopupExample : EditorWindow
{
    static readonly string windowName = "Auto handler generator";

    private bool showSelectedElements;

    ClassDescriptor classDescriptor;


    [MenuItem("Assets/Generate Handler")]
    static void Init()
    {
        ShowPopupExample window = CreateInstance<ShowPopupExample>();
        if (window)
        {
            window.classDescriptor = new ClassDescriptor()
            {
                className = "PrefabHandler",
                saveLocation = "Asset"
            };

            window.UpdateSelectedItemList();

            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            window.titleContent = new GUIContent(windowName);
            window.ShowUtility();
        }
    }

    void OnGUI()
    {
        Rect classNameLabelRect = new Rect(5, 10, 100, 15); 
        Rect classNameRect = new Rect(110, 10, 135, 15);

        GUI.Label(classNameLabelRect, "Class name");
        classDescriptor.className = GUI.TextField(classNameRect, classDescriptor.className);

        Rect saveLocationLabelRect = new Rect(5, 30, 100, 15);
        Rect saveLocationRect = new Rect(110, 30, 135, 15);

        if (saveLocationRect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
            if (Event.current.type == EventType.DragPerform)
            {
                if(DragAndDrop.paths.Length>0)
                    classDescriptor.saveLocation = DragAndDrop.paths[0];
            }
        }

        GUI.Label(saveLocationLabelRect, "Save location");
        classDescriptor.saveLocation = GUI.TextField(saveLocationRect, classDescriptor.saveLocation);

        showSelectedElements = EditorGUILayout.Foldout(showSelectedElements, "Selected Prefabs");
        if (showSelectedElements)
        {
            GUILayout.BeginArea(new Rect(5,40,240,150));

            GUILayout.BeginHorizontal();
            GUILayout.VerticalScrollbar(0, 0, 0, 0);
            GUILayout.BeginVertical();
            for (int index = 0; index < classDescriptor.items.Count; index++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("item-{0}", index));

                var newName = GUILayout.TextField(classDescriptor.items[index].name);
                classDescriptor.items[index].name = newName;

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        Rect buttonRect = new Rect(50, 120, 150, 20);
        if (GUI.Button(buttonRect, "Generate"))
        {
            Assets.Scripts.ResourceHandlerCodeGenerator.GenerateAndSave(classDescriptor);
            
            AssetDatabase.Refresh();

            this.Close();
        }
    }

    private void UpdateSelectedItemList()
    {
        classDescriptor.items.Clear();

        for (int index = 0; index < Selection.gameObjects.Length; index++)
        {
            classDescriptor.items.Add(new SelectedItem()
            {
                item = Selection.gameObjects[index],
                guid = Selection.assetGUIDs[index],
                name = Selection.gameObjects[index].name,
            });
        }
    }
}