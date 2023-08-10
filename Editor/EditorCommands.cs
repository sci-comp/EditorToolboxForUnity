using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Paul
{
    // LetterAsInteger is used to alphabetize menu items
    public enum LetterAsInteger
    {
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
        F = 6,
        G = 7,
        H = 8,
        I = 9,
        J = 10,
        K = 11,
        L = 12,
        M = 13,
        N = 14,
        O = 15,
        P = 16,
        Q = 17,
        R = 18,
        S = 19,
        T = 20,
        U = 21,
        V = 22,
        W = 23,
        X = 24,
        Y = 25,
        Z = 26
    }

    public static class EditorCommands
    {
        #region General commands

        private static bool gizmosHidden = false;

        [MenuItem("Paul/Commands/Hide scene view gizmos", priority = 100 * (int)LetterAsInteger.C + (int)LetterAsInteger.H)]
        private static void HideSceneViewGizmos()
        {
            /// <summary>
            /// Toggle visibility of all Gizmos in the Scene view.
            /// </summary>
            
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
            /// <summary>
            /// Toggle the Lock Inspector feature.
            /// </summary>

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
            /// <summary>
            /// Alphabetize children of each selected parent game object.
            /// </summary>
            
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
            /// <summary>
            /// Center the parent GameObject to the geometric center of its child objects.
            /// </summary>

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
            // <summary>
            // Destroy all colliders on selected game objects.
            // </summary>

            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                if (selectedObject.TryGetComponent<Collider>(out var collider))
                {
                    Undo.DestroyObjectImmediate(collider);
                }
            }
        }

        [MenuItem("Paul/GameObject/Move colliders on top child to parent", priority = (100 * (int)LetterAsInteger.G) + (int)LetterAsInteger.C)]
        private static void MoveCollidersOnTopChildToParent()
        {
            /// <summary>
            /// For each selected parent game object, all colliders on the top-level child will 
            /// be moved to the parent.
            ///
            /// This command was made for the following application in mind,
            /// 
            /// When dealing with prefabs that have LOD groups, often colliders will exist on 
            /// the LOD0 game object rather than the root game object in a prefab.If a designer 
            /// would like to remove LOD components entirely, then this is a way to move colliders 
            /// to the root game objects in bulk.
            /// </summary> 

            foreach (GameObject parentObject in Selection.gameObjects)
            {
                Transform topChild = parentObject.transform.GetChild(0);
                Collider[] colliders = topChild.GetComponents<Collider>();

                foreach (Collider collider in colliders)
                {
                    if (collider.transform.parent == parentObject.transform)
                    {
                        Undo.RecordObject(parentObject, "Move Collider to Parent");
                        UnityEditorInternal.ComponentUtility.CopyComponent(collider);
                        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(parentObject);
                        Undo.DestroyObjectImmediate(collider);
                    }
                }
            }
        }

        [MenuItem("Paul/GameObject/Reset local transform position", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.R)]
        private static void ResetLocalTransformPosition()
        {
            /// <summary>
            /// Sets the local transform position of selected objects to (0, 0, 0).
            /// </summary>

            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No objects selected. Operation not performed.");
                return;
            }

            foreach (GameObject obj in selectedObjects)
            {
                Undo.RecordObject(obj.transform, "Reset local transform position");
                obj.transform.localPosition = Vector3.zero;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Paul/GameObject/Select children", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.S)]
        private static void SelectChildren()
        {
            /// <summary>
            /// Select children of selected game objects while deselecting the current selection.
            /// </summary>

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

        [MenuItem("Paul/GameObject/Select parents", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.S)]
        private static void SelectParents()
        {
            /// <summary>
            /// Select parents of selected game objects while deselecting the current selection.
            /// </summary>

            GameObject[] selectedGameObjects = Selection.gameObjects;
            List<GameObject> parentGameObjects = new();

            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                if (selectedGameObject.transform.parent != null)
                {
                    parentGameObjects.Add(selectedGameObject.transform.parent.gameObject);
                }
            }

            if (parentGameObjects.Count > 0)
            {
                Selection.objects = parentGameObjects.ToArray();
            }
            else
            {
                Selection.objects = new Object[0];
            }
        }

        #endregion
    }
}

