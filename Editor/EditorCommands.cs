using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Paul
{
    public static class EditorCommands
    {
        #region General commands

        private static bool gizmosHidden = false;

        [MenuItem("Paul/Commands/Hide scene view gizmos", priority = 100 * (int)LetterAsInteger.C + (int)LetterAsInteger.H)]
        private static void HideSceneViewGizmos()
        {
            gizmosHidden = !gizmosHidden;

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.drawGizmos = !gizmosHidden;
                SceneView.lastActiveSceneView.Repaint();
            }
        }

        [MenuItem("Paul/Commands/Lock Inspector", priority = 100 * (int)LetterAsInteger.C + (int)LetterAsInteger.L)]
        private static void LockInspector()
        {
            Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
            EditorWindow window = EditorWindow.GetWindow(type);
            PropertyInfo propertyInfo = type.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);
            bool value = (bool)propertyInfo.GetValue(window, null);
            propertyInfo.SetValue(window, !value, null);
        }

        #endregion

        #region Game object commands

        [MenuItem("Paul/GameObject/Alphabetize children", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.A)]
        private static void AlphabetizeChildren()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;

            foreach (GameObject parentGameObject in selectedGameObjects)
            {
                Transform[] childTransforms = parentGameObject.GetComponentsInChildren<Transform>();
                childTransforms = childTransforms.Where(t => t != parentGameObject.transform).ToArray();

                // Order child GameObjects alphabetically
                Transform[] orderedChildTransforms = childTransforms.OrderBy(t => t.name).ToArray();

                // Reparent each GameObject in the new order
                for (int i = 0; i < orderedChildTransforms.Length; i++)
                {
                    orderedChildTransforms[i].SetSiblingIndex(i);
                }
            }
        }

        [MenuItem("Paul/GameObject/Center to geometry", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.C)]
        private static void CenterToGeometry()
        {
            // Center the parent GameObject to the geometric center of its child objects.

            GameObject[] selectedObjects = Selection.gameObjects;

            foreach (GameObject parentObject in selectedObjects)
            {
                int childCount = parentObject.transform.childCount;

                if (childCount == 0)
                {
                    continue;
                }

                Vector3 sumPositions = Vector3.zero;

                for (int i = 0; i < childCount; i++)
                {
                    Transform childTransform = parentObject.transform.GetChild(i);
                    sumPositions += childTransform.position;
                }

                Vector3 centerOfMass = sumPositions / childCount;
                Vector3 oldPosition = parentObject.transform.position;

                parentObject.transform.position = centerOfMass;

                for (int i = 0; i < childCount; i++)
                {
                    Transform childTransform = parentObject.transform.GetChild(i);
                    childTransform.position += oldPosition - centerOfMass;
                }
            }
        }

        [MenuItem("Paul/GameObject/Destroy colliders", priority = (100 * (int)LetterAsInteger.G) + (int)LetterAsInteger.D)]
        private static void DestroyColliders()
        {
            // Destroy all colliders on selected game objects.

            foreach (GameObject parentObject in Selection.gameObjects)
            {
                Collider[] colliders = parentObject.transform.GetComponentsInChildren<Collider>();

                foreach (Collider collider in colliders)
                {
                    Object.DestroyImmediate(collider);
                }
            }
        }

        [MenuItem("Paul/GameObject/Select children", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.S)]
        private static void SelectChildren()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            List<GameObject> childGameObjects = new();

            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                if (selectedGameObject.transform.childCount > 0)
                {
                    foreach (Transform child in selectedGameObject.transform)
                    {
                        childGameObjects.Add(child.gameObject);
                    }
                }
            }

            // If we found any child objects, select them.
            if (childGameObjects.Count > 0)
            {
                Selection.objects = childGameObjects.ToArray();
            }
            // If no child objects were found, deselect all.
            else
            {
                Selection.objects = new Object[0];
            }
        }

        #endregion
    }
}

