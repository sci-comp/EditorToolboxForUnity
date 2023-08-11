using UnityEngine;
using UnityEditor;

namespace EditorToolbox
{
    /// <summary>
    /// Editor window class that allows the user to add a prefix or suffix to the names of selected GameObjects or Assets.
    /// </summary>
    public class AddPrefixOrSuffix : EditorWindow
    {
        private string InputText = "";
        private bool isSuffix = false;
        private bool isFocused;

        [MenuItem("Editor Toolbox/Asset/Add prefix or suffix", priority = 100 * (int)LetterAsInteger.A + (int)LetterAsInteger.A)]
        public static void ShowWindow()
        {
            GetWindow(typeof(AddPrefixOrSuffix));
        }

        private void OnGUI()
        {
            GUILayout.Label("Add Suffix/Prefix", EditorStyles.boldLabel);

            GUI.SetNextControlName("InputTextField");
            InputText = EditorGUILayout.TextField("Suffix/Prefix to Add", InputText);

            if (!isFocused)
            {
                GUI.FocusControl("InputTextField");
                isFocused = true;
            }

            isSuffix = EditorGUILayout.Toggle("Suffix", isSuffix);

            if (GUILayout.Button("Apply") || IsEnterKeyPressed())
            {
                RenameSelectedObjects();
            }
        }

        private bool IsEnterKeyPressed()
        {
            return Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
        }

        private void RenameSelectedObjects()
        {
            if (string.IsNullOrEmpty(InputText))
            {
                Debug.LogWarning("Input is empty.");
                return;
            }

            Object[] selectedObjects = Selection.objects;

            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No game objects selected.");
                return;
            }

            foreach (Object selectedObject in selectedObjects)
            {
                string newName = isSuffix ? selectedObject.name + InputText : InputText + selectedObject.name;
                string path = AssetDatabase.GetAssetPath(selectedObject);
                string newPath = path.Replace(selectedObject.name + ".", newName + ".");

                Object oldAtPath = AssetDatabase.LoadAssetAtPath<Object>(newPath);
                if (oldAtPath && oldAtPath != selectedObject)
                    AssetDatabase.RenameAsset(newPath, oldAtPath.name + InputText);

                if (selectedObject is GameObject go)
                {
                    Undo.RecordObject(go, "Rename GameObject");
                    go.name = newName;
                }
                else
                {
                    Undo.RecordObject(selectedObject, "Rename Asset");
                    selectedObject.name = newName;
                }

                if (AssetDatabase.Contains(selectedObject))
                {
                    AssetDatabase.RenameAsset(path, selectedObject.name);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Close();
        }
    }
}

