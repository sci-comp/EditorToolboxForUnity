using UnityEngine;
using UnityEditor;

namespace EditorToolbox
{
    /// <summary>
    /// Replace specific terms within the names of selected objects or assets. 
    /// The user can input a term to replace and the replacement term, then apply 
    /// these changes to all selected objects.
    /// </summary>

    public class ReplaceTerm : EditorWindow
    {
        private string TermToReplace = "";
        private string ReplaceWith = "";
        private bool isFocused;

        [MenuItem("Editor Toolbox/Asset/Replace term", priority = 100 * (int)LetterAsInteger.A + (int)LetterAsInteger.R)]
        public static void ShowWindow()
        {
            GetWindow(typeof(ReplaceTerm));
        }

        private void OnGUI()
        {
            GUILayout.Label("Replace Term", EditorStyles.boldLabel);

            GUI.SetNextControlName("TermToReplaceField");
            TermToReplace = EditorGUILayout.TextField("Term to Replace", TermToReplace);

            if (!isFocused)
            {
                GUI.FocusControl("TermToReplaceField");
                isFocused = true;
            }

            ReplaceWith = EditorGUILayout.TextField("Replace With", ReplaceWith);

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
            if (string.IsNullOrEmpty(TermToReplace))
            {
                Debug.LogWarning("Term to Replace is empty.");
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
                if (selectedObject.name.Contains(TermToReplace))
                {
                    string newName = selectedObject.name.Replace(TermToReplace, ReplaceWith);
                    string path = AssetDatabase.GetAssetPath(selectedObject);
                    string newPath = path.Replace(TermToReplace, ReplaceWith);

                    if (AssetDatabase.Contains(selectedObject) && path != newPath)
                    {
                        AssetDatabase.MoveAsset(path, newPath);
                    }

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
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Close();
        }


    }
}

