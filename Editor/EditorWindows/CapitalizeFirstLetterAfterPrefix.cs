using UnityEditor;
using UnityEngine;

namespace EditorToolbox
{
    /// <summary>
    /// Capitalize the first letter after a specified prefix for selected assets in the project view.
    /// </summary>
    public class CapitalizeFirstLetterAfterPrefix : EditorWindow
    {
        private string prefix = "";

        [MenuItem("Editor Toolbox/Asset/Capitalize first letter after prefix", priority = 100 * (int)LetterAsInteger.A + (int)LetterAsInteger.C)]
        public static void ShowWindow()
        {
            GetWindow<CapitalizeFirstLetterAfterPrefix>("Capitalize First Letter After Prefix");
        }

        private void OnGUI()
        {
            GUILayout.Label("Prefix:", EditorStyles.boldLabel);
            prefix = EditorGUILayout.TextField(prefix);

            if (GUILayout.Button("Rename"))
            {
                RenameSelectedObjects();
            }
        }

        private void RenameSelectedObjects()
        {
            if (string.IsNullOrEmpty(prefix))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a prefix.", "OK");
                return;
            }

            Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            Undo.RecordObjects(selectedObjects, "Rename Objects");

            foreach (Object obj in selectedObjects)
            {
                string oldName = obj.name;
                string newName = CapitalizeFirstLetterAfterPrefix_(oldName);

                if (!string.IsNullOrEmpty(newName))
                {
                    obj.name = newName;
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(obj), newName);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private string CapitalizeFirstLetterAfterPrefix_(string name)
        {
            if (!name.StartsWith(prefix))
            {
                return "";
            }

            int index = prefix.Length;
            if (index >= name.Length)
            {
                return "";
            }

            char[] nameChars = name.ToCharArray();
            nameChars[index] = char.ToUpper(nameChars[index]);

            return new string(nameChars);
        }
    }
}

