using UnityEngine;
using UnityEditor;

namespace EditorToolbox
{
    public class AssignPhysicMaterialToAllColliders : EditorWindow
    {
        [SerializeField] private PhysicMaterial physicMaterial;
        [SerializeField] private bool getComponentsInChildren = false;

        /// <summary>
        /// Used to set a physics material onto all selected GameObjects' colliders.
        /// Optionally, set the physics material to child colliders as well.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Assign physic material to all colliders", priority = (100 * (int)LetterAsInteger.G) + (int)LetterAsInteger.S)]
        public static void ShowWindow()
        {
            GetWindow<AssignPhysicMaterialToAllColliders>("Set Physic Material");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            physicMaterial = (PhysicMaterial)EditorGUILayout.ObjectField("Physic Material", physicMaterial, typeof(PhysicMaterial), false);
            getComponentsInChildren = EditorGUILayout.Toggle("Get Components In Children", getComponentsInChildren);

            if (GUILayout.Button("Go"))
            {
                ApplyPhysicsMaterial();
            }

            EditorGUILayout.EndVertical();
        }

        private void ApplyPhysicsMaterial()
        {
            if (physicMaterial == null)
            {
                Debug.LogWarning("Assign a Physics Material.");
                return;
            }

            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("Select one or more GameObjects in the scene.");
                return;
            }

            int appliedCount = 0;

            foreach (GameObject selectedObject in selectedObjects)
            {
                if (!selectedObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

                Collider[] colliders = getComponentsInChildren ? selectedObject.GetComponentsInChildren<Collider>(true) : selectedObject.GetComponents<Collider>();

                foreach (Collider collider in colliders)
                {
                    collider.sharedMaterial = physicMaterial;
                    appliedCount++;
                }
            }

            Debug.Log($"Applied Physics Material to {appliedCount} collider(s) on selected GameObject(s).");
        }
    }
}
