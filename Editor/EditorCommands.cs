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
    /// <summary>
    /// LetterAsInteger is used to alphabetize menu items.
    /// </summary>
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

        /// <summary>
        /// Toggle visibility of all Gizmos in the Scene view.
        /// </summary>
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


        /// <summary>
        /// Toggle the Lock Inspector feature.
        /// </summary>
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

        /// <summary>
        /// Alphabetize children of each selected parent game object.
        /// </summary>
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


        /// <summary>
        /// Center the parent GameObject to the geometric center of its child objects.
        /// </summary>
        [MenuItem("Paul/GameObject/Center to geometry", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.C)]
        private static void CenterToGeometry()
        {
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


        /// <summary>
        /// Destroy all colliders on selected game objects.
        /// </summary>
        [MenuItem("Paul/GameObject/Destroy colliders", priority = (100 * (int)LetterAsInteger.G) + (int)LetterAsInteger.D)]
        private static void DestroyColliders()
        {
            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                if (selectedObject.TryGetComponent<Collider>(out var collider))
                {
                    Undo.DestroyObjectImmediate(collider);
                }
            }
        }


        /// <summary>
        /// For each selected parent game object, move colliders on all children to their respective parents.
        /// </summary>
        [MenuItem("Paul/GameObject/Move all colliders to parent", priority = (100 * (int)LetterAsInteger.G) + (int)LetterAsInteger.M)]
        private static void MoveAllCollidersToParent()
        {
            foreach (GameObject parentObject in Selection.gameObjects)
            {
                Collider[] colliders = parentObject.GetComponentsInChildren<Collider>();

                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject == parentObject)
                    {
                        continue;
                    }

                    UnityEditorInternal.ComponentUtility.CopyComponent(collider);
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(parentObject);
                }

                foreach (Transform child in parentObject.transform)
                {
                    Collider[] childColliders = child.GetComponents<Collider>();
                    foreach (Collider childCollider in childColliders)
                    {
                        Undo.DestroyObjectImmediate(childCollider);
                    }
                }
            }
        }


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
        [MenuItem("Paul/GameObject/Move colliders on top child to parent", priority = (100 * (int)LetterAsInteger.G) + (int)LetterAsInteger.M)]
        private static void MoveCollidersOnTopChildToParent()
        {
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


        /// <summary>
        /// Moves all components on the selected game object's top child to the respective parent.
        /// The process is undoable.
        /// </summary>
        [MenuItem("Paul/GameObject/Move components on top child to parent", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.M)]
        private static void MoveComponentsOnTopChildToParent()
        {            
            foreach (GameObject parentObject in Selection.gameObjects)
            {
                Transform topChild = parentObject.transform.GetChild(0);

                Component[] components = topChild.GetComponents(typeof(Component));

                Debug.Log("Found " + components.Length + " components on " + topChild.name);

                foreach (Component component in components)
                {
                    if (component is Transform)
                    {
                        continue;
                    }

                    UnityEditorInternal.ComponentUtility.CopyComponent(component);
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(parentObject);
                    Undo.DestroyObjectImmediate(component);
                }
            }
        }


        /// <summary>
        /// Paste component from clipboard as new onto the actively selected game object.
        /// </summary>
        [MenuItem("Paul/GameObject/Paste component as new", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.P)]
        private static void PasteComponentAsNew()
        {
            if (Selection.activeGameObject != null)
            {
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(Selection.activeGameObject);
            }
            else
            {
                Debug.LogWarning("No game object is selected.");
            }
        }


        /// <summary>
        /// Rounds the x, y, and z position of selected game objects to the nearest grid value.
        /// </summary>
        [MenuItem("Paul/GameObject/Push children to grid", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.P)]
        private static void PushChildrenToGrid()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;

            EditorToolboxSettings settings = Resources.Load<EditorToolboxSettings>("EditorToolboxSettings");

            if (settings == null)
            {
                Debug.LogWarning("Could not find EditorToolboxSettings resource file.");
                return;
            }

            float gridSize = settings.gridSize;

            foreach (GameObject parent in selectedGameObjects)
            {
                foreach (Transform child in parent.transform)
                {
                    Vector3 position = child.position;

                    float x = gridSize * Mathf.Round(position.x / gridSize);
                    float y = gridSize * Mathf.Round(position.y / gridSize);
                    float z = gridSize * Mathf.Round(position.z / gridSize);

                    Undo.RecordObject(child, "Push Child to Grid");

                    child.position = new Vector3(x, y, z);
                }
            }
        }


        /// <summary>
        /// Renames selected GameObjects in the scene hierarchy to the name of their corresponding prefab.
        /// It does nothing if the selected GameObject is not a prefab instance or not in the scene.
        /// </summary>
        [MenuItem("Paul/GameObject/Rename to prefab name", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.R)]
        private static void RenameSelectedToPrefabName()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                if (selectedGameObject == null || selectedGameObject.scene.name == null)
                {
                    continue;
                }

                PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(selectedGameObject);
                if (prefabAssetType == PrefabAssetType.Regular || prefabAssetType == PrefabAssetType.Variant)
                {
                    string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selectedGameObject);
                    string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);

                    Undo.RecordObject(selectedGameObject, "Rename to Prefab Name");
                    selectedGameObject.name = prefabName;
                }
            }
        }


        /// <summary>
        /// Sets the local transform position of selected objects to (0, 0, 0).
        /// </summary>
        [MenuItem("Paul/GameObject/Reset local transform position", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.R)]
        private static void ResetLocalTransformPosition()
        {
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


        /// <summary>
        /// Select children of selected game objects while deselecting the current selection.
        /// </summary>
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


        /// <summary>
        /// Select parents of selected game objects while deselecting the current selection.
        /// </summary>
        [MenuItem("Paul/GameObject/Select parents", priority = 100 * (int)LetterAsInteger.G + (int)LetterAsInteger.S)]
        private static void SelectParents()
        {
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

