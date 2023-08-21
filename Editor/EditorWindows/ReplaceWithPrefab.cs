using UnityEditor;
using UnityEngine;

namespace EditorToolbox
{
    public class ReplaceWithPrefab : EditorWindow
    {
        [SerializeField] GameObject prefab;
        [SerializeField] bool useLocalCenter;

        [MenuItem("Editor Toolbox/Scene/Replace with Prefab", priority = (100 * (int)LetterAsInteger.S) + (int)LetterAsInteger.R)]
        public static void ShowWindow()
        {
            GetWindow<ReplaceWithPrefab>("Replace with Prefab");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
            useLocalCenter = EditorGUILayout.Toggle("Use Local Center", useLocalCenter);

            if (GUILayout.Button("Replace"))
            {
                ReplaceSelectedObjectsWithPrefab();
            }

            EditorGUILayout.EndVertical();
        }

        private void ReplaceSelectedObjectsWithPrefab()
        {
            if (prefab == null)
            {
                Debug.LogWarning("Assign a Prefab.");
                return;
            }

            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("Select one or more GameObjects in the scene.");
                return;
            }

            foreach (GameObject selectedObject in selectedObjects)
            {
                if (!selectedObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Returning.");
                    return;
                }

                Transform parentTransform = selectedObject.transform.parent;
                Vector3 position = selectedObject.transform.position;
                Quaternion rotation = selectedObject.transform.rotation;
                Vector3 scale = selectedObject.transform.localScale;

                if (useLocalCenter)
                {
                    Bounds bounds = new(selectedObject.transform.position, Vector3.zero);
                    foreach (Renderer renderer in selectedObject.GetComponentsInChildren<Renderer>())
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }

                    position = bounds.center;
                }

                GameObject newObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                if (parentTransform != null)
                {
                    newObject.transform.SetParent(parentTransform, false);
                }

                newObject.transform.SetPositionAndRotation(position, rotation);
                newObject.transform.localScale = scale;

                Undo.DestroyObjectImmediate(selectedObject);
            }
        }
    }
}
