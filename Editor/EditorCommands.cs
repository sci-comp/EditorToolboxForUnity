﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EditorToolbox
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

    /// <summary>
    /// Used to store transient data while performing transform operations.
    /// </summary>
    public struct TransformData
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public TransformData(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    public static class EditorCommands
    {
        #region General commands

        private static bool gizmosHidden = false;

        /// <summary>
        /// Toggle visibility of all Gizmos in the Scene view.
        /// </summary>
        [MenuItem("Editor Toolbox/Commands/Hide scene view gizmos", priority = 100 * (int)LetterAsInteger.C + (int)LetterAsInteger.H)]
        public static void HideSceneViewGizmos()
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
        [MenuItem("Editor Toolbox/Commands/Lock Inspector", priority = 100 * (int)LetterAsInteger.C + (int)LetterAsInteger.L)]
        public static void LockInspector()
        {
            Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
            EditorWindow window = EditorWindow.GetWindow(type);
            PropertyInfo propertyInfo = type.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);
            bool value = (bool)propertyInfo.GetValue(window, null);
            propertyInfo.SetValue(window, !value, null);
        }

        
        [MenuItem("Editor Toolbox/Commands/Screenshot", priority = (100 * (int)LetterAsInteger.C) + (int)LetterAsInteger.S)]
        public static void Screenshot()
        {
            EditorToolboxSettings settings = Resources.Load<EditorToolboxSettings>("EditorToolboxSettings");

            if (settings == null)
            {
                Debug.LogWarning("Could not find EditorToolboxSettings resource file.");
                return;
            }

            if (!Directory.Exists(settings.screenshotFolder))
            {
                Directory.CreateDirectory(settings.screenshotFolder);
            }

            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            string sceneName = activeScene.IsValid() ? activeScene.name : "NoScene";
            string savePath = $"{settings.screenshotFolder}/{sceneName}{DateTime.Now: yyyy-MM-dd_HH-mm-ss}.png";

            ScreenCapture.CaptureScreenshot(savePath);
            Debug.Log("Screenshot saved to: " + savePath);
        }

        #endregion

        #region Revert scene components

        [MenuItem("Editor Toolbox/Scene/Revert selected components/Animator")]
        public static void RevertSelectedAnimatorComponents()
        {
            RevertSelectedComponentsToPrefab<Animator>();
        }

        [MenuItem("Editor Toolbox/Scene/Revert selected components/AudioSource")]
        public static void RevertSelectedAudioSourceComponents()
        {
            RevertSelectedComponentsToPrefab<AudioSource>();
        }

        [MenuItem("Editor Toolbox/Scene/Revert selected components/AudioListener")]
        public static void RevertSelectedAudioListenerComponents()
        {
            RevertSelectedComponentsToPrefab<AudioListener>();
        }

        [MenuItem("Editor Toolbox/Scene/Revert selected components/Camera")]
        public static void RevertSelectedCameraComponents()
        {
            RevertSelectedComponentsToPrefab<Camera>();
        }

        [MenuItem("Editor Toolbox/Scene/Revert selected components/Image")]
        public static void RevertSelectedImageComponents()
        {
            RevertSelectedComponentsToPrefab<Image>();
        }

        [MenuItem("Editor Toolbox/Scene/Revert selected components/Light")]
        public static void RevertSelectedLightComponents()
        {
            RevertSelectedComponentsToPrefab<Light>();
        }

        [MenuItem("Editor Toolbox/Scene/Revert selected components/NavMeshAgent")]
        public static void RevertSelectedNavMeshAgentComponents()
        {
            RevertSelectedComponentsToPrefab<UnityEngine.AI.NavMeshAgent>();
        }

        [MenuItem("Editor Toolbox/Scene/Revert selected components/Rigidbody")]
        public static void RevertSelectedRigidbodyComponents()
        {
            RevertSelectedComponentsToPrefab<Rigidbody>();
        }

        [MenuItem("Editor Toolbox/Scene/Revert selected components/Transform")]
        public static void RevertSelectedTransformComponents()
        {
            RevertSelectedComponentsToPrefab<Transform>();
        }

        [MenuItem("Editor Toolbox/Scene/Revert selected components/TextMeshPro - Text UI")]
        public static void RevertSelectedTMPTextUIComponents()
        {
            RevertSelectedComponentsToPrefab<TMP_Text>();
        }

        [MenuItem("Editor Toolbox/Scene/Revert selected components/Button")]
        public static void RevertSelectedButtonComponents()
        {
            RevertSelectedComponentsToPrefab<Button>();
        }

        #endregion

        #region Scene commands

        
        [MenuItem("Editor Toolbox/Scene/Count all game objects", priority = 100 * (int)LetterAsInteger.C + (int)LetterAsInteger.C)]
        public static void CountActiveInactiveGameObjects()
        {
            GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
            int activeCount = 0;
            int inactiveCount = 0;

            foreach (GameObject go in allGameObjects)
            {
                if (go.scene.isLoaded)
                {
                    if (go.activeSelf)
                    {
                        activeCount++;
                    }
                    else
                    {
                        inactiveCount++;
                    }
                }
            }

            Debug.Log("Active GameObjects: " + activeCount + ", Inactive GameObjects: " + inactiveCount);
        }


        /// <summary>
        /// Reverts the specified type of component to its prefab values on all selected GameObjects in the scene.
        /// If a selected GameObject is not part of a prefab instance, it will be skipped.
        /// </summary>
        /// <typeparam name="T">The type of component to revert. Must derive from Component.</typeparam>
        public static void RevertSelectedComponents<T>() where T : Component
        {
            var selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                Debug.Log("No objects selected, returning.");
                return;
            }

            foreach (var selectedObject in selectedObjects)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(selectedObject))
                {
                    T[] components = selectedObject.GetComponents<T>();
                    foreach (var component in components)
                    {
                        PrefabUtility.RevertObjectOverride(component, InteractionMode.UserAction);
                    }
                }
                else
                {
                    Debug.Log("Object " + selectedObject.name + " is not part of a prefab, skipping.");
                }
            }
        }

        private static void RevertSelectedComponentsToPrefab<T>() where T : Component
        {
            var selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                Debug.Log("No objects selected, returning.");
                return;
            }

            foreach (var selectedObject in selectedObjects)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(selectedObject))
                {
                    T[] components = selectedObject.GetComponents<T>();
                    foreach (var component in components)
                    {
                        PrefabUtility.RevertObjectOverride(component, InteractionMode.UserAction);
                    }
                }
                else
                {
                    Debug.Log("Object " + selectedObject.name + " is not part of a prefab, skipping.");
                }
            }
        }

        /// <summary>
        /// Alphabetize children of each selected parent game object.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Alphabetize children", priority = 100 * (int)LetterAsInteger.S + (int)LetterAsInteger.A)]
        public static void AlphabetizeChildren()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;

            foreach (GameObject parentGameObject in selectedGameObjects)
            {
                if (!parentGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

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
        [MenuItem("Editor Toolbox/Scene/Center to geometry", priority = 100 * (int)LetterAsInteger.S + (int)LetterAsInteger.C)]
        public static void CenterToGeometry()
        {
            GameObject[] selectedObjects = Selection.gameObjects;

            foreach (GameObject parentGameObject in selectedObjects)
            {
                if (!parentGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

                int childCount = parentGameObject.transform.childCount;

                if (childCount == 0)
                {
                    continue;
                }

                Vector3 sumPositions = Vector3.zero;

                for (int i = 0; i < childCount; i++)
                {
                    Transform childTransform = parentGameObject.transform.GetChild(i);
                    sumPositions += childTransform.position;
                }

                Vector3 centerOfMass = sumPositions / childCount;
                Vector3 oldPosition = parentGameObject.transform.position;

                parentGameObject.transform.position = centerOfMass;

                for (int i = 0; i < childCount; i++)
                {
                    Transform childTransform = parentGameObject.transform.GetChild(i);
                    childTransform.position += oldPosition - centerOfMass;
                }
            }
        }


        /// <summary>
        /// Destroy all colliders on selected game objects.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Destroy colliders", priority = (100 * (int)LetterAsInteger.S) + (int)LetterAsInteger.D)]
        public static void DestroyColliders()
        {
            foreach (GameObject selectedGameObject in Selection.gameObjects)
            {
                if (!selectedGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

                if (selectedGameObject.TryGetComponent<Collider>(out var collider))
                {
                    Undo.DestroyObjectImmediate(collider);
                }
            }
        }


        /// <summary>
        /// For each selected parent game object, move colliders on all children to their respective parents.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Move all colliders to parent", priority = (100 * (int)LetterAsInteger.S) + (int)LetterAsInteger.M)]
        public static void MoveAllCollidersToParent()
        {
            foreach (GameObject parentGameObject in Selection.gameObjects)
            {
                if (!parentGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

                Collider[] colliders = parentGameObject.GetComponentsInChildren<Collider>();

                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject == parentGameObject)
                    {
                        continue;
                    }

                    UnityEditorInternal.ComponentUtility.CopyComponent(collider);
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(parentGameObject);
                }

                foreach (Transform child in parentGameObject.transform)
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
        [MenuItem("Editor Toolbox/Scene/Move colliders on top child to parent", priority = (100 * (int)LetterAsInteger.S) + (int)LetterAsInteger.M)]
        public static void MoveCollidersOnTopChildToParent()
        {
            foreach (GameObject parentGameObject in Selection.gameObjects)
            {
                if (!parentGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

                Transform topChild = parentGameObject.transform.GetChild(0);

                if (topChild != null)
                {
                    Collider[] colliders = topChild.GetComponents<Collider>();

                    foreach (Collider collider in colliders)
                    {
                        if (collider.transform.parent == parentGameObject.transform)
                        {
                            Undo.RecordObject(parentGameObject, "Move Collider to Parent");
                            UnityEditorInternal.ComponentUtility.CopyComponent(collider);
                            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(parentGameObject);
                            Undo.DestroyObjectImmediate(collider);
                        }
                    }
                }                
            }
        }


        /// <summary>
        /// Moves all components on the selected game object's top child to the respective parent.
        /// The process is undoable.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Move components on top child to parent", priority = 100 * (int)LetterAsInteger.S + (int)LetterAsInteger.M)]
        public static void MoveComponentsOnTopChildToParent()
        {            
            foreach (GameObject parentGameObject in Selection.gameObjects)
            {
                if (!parentGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

                Transform topChild = parentGameObject.transform.GetChild(0);

                Component[] components = topChild.GetComponents(typeof(Component));

                Debug.Log("Found " + components.Length + " components on " + topChild.name);

                foreach (Component component in components)
                {
                    if (component is Transform)
                    {
                        continue;
                    }

                    UnityEditorInternal.ComponentUtility.CopyComponent(component);
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(parentGameObject);
                    Undo.DestroyObjectImmediate(component);
                }
            }
        }


        /// <summary>
        /// Paste component from clipboard as new onto the actively selected game object.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Paste component as new", priority = 100 * (int)LetterAsInteger.S + (int)LetterAsInteger.P)]
        public static void PasteComponentAsNew()
        {
            if (Selection.activeGameObject != null && Selection.activeGameObject.scene.isLoaded)
            {
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(Selection.activeGameObject);                
            }
            else
            {
                Debug.Log("Scene object not selected, returning.");
            }
        }


        /// <summary>
        /// Rounds the x, y, and z position of selected game objects to the nearest grid value.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Push children to grid", priority = 100 * (int)LetterAsInteger.S + (int)LetterAsInteger.P)]
        public static void PushChildrenToGrid()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;

            EditorToolboxSettings settings = Resources.Load<EditorToolboxSettings>("EditorToolboxSettings");

            if (settings == null)
            {
                Debug.LogWarning("Could not find EditorToolboxSettings resource file.");
                return;
            }

            float gridSize = settings.gridSize;

            foreach (GameObject parentGameObject in selectedGameObjects)
            {
                if (!parentGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

                foreach (Transform child in parentGameObject.transform)
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
        [MenuItem("Editor Toolbox/Scene/Rename to prefab name", priority = 100 * (int)LetterAsInteger.S + (int)LetterAsInteger.R)]
        public static void RenameToPrefabName()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                if (!selectedGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

                if (selectedGameObject != null)
                {
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
        }


        /// <summary>
        /// Resets the position of selected parent game objects while maintaining the global position of their children.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Reset category transform position", priority = 100 * (int)LetterAsInteger.S + (int)LetterAsInteger.R)]
        public static void ResetCategoryTransformPosition()
        {
            var selectedGameObjects = Selection.gameObjects;

            foreach (GameObject parentGameObject in selectedGameObjects)
            {
                if (!parentGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

                var childPositions = new List<KeyValuePair<Transform, Vector3>>();

                for (int i = 0; i < parentGameObject.transform.childCount; i++)
                {
                    Transform child = parentGameObject.transform.GetChild(i);
                    Vector3 position = child.position;
                    childPositions.Add(new KeyValuePair<Transform, Vector3>(child, position));
                }

                parentGameObject.transform.localPosition = Vector3.zero;

                foreach (var childPosition in childPositions)
                {
                    childPosition.Key.position = childPosition.Value;
                }
            }
        }


        /// <summary>
        /// Resets the local rotation of selected GameObjects in the scene to identity
        /// while preserving the global rotation and position of their child GameObjects.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Reset category transform rotation", priority = 100 * (int)LetterAsInteger.S + (int)LetterAsInteger.R)]
        public static void ResetCategoryTransformRotation()
        {
            var selectedGameObjects = Selection.gameObjects;
            foreach (GameObject parentGameObject in selectedGameObjects)
            {
                if (!parentGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

                var childTransforms = new List<TransformData>();

                for (int i = 0; i < parentGameObject.transform.childCount; i++)
                {
                    Transform childTransform = parentGameObject.transform.GetChild(i);
                    childTransforms.Add(new TransformData(childTransform.position, childTransform.rotation));
                }

                parentGameObject.transform.localRotation = Quaternion.identity;

                for (int i = 0; i < childTransforms.Count; i++)
                {
                    Transform childTransform = parentGameObject.transform.GetChild(i);
                    childTransform.SetPositionAndRotation(childTransforms[i].Position, childTransforms[i].Rotation);
                }
            }
        }


        /// <summary>
        /// Sets the local transform position of selected objects to (0, 0, 0).
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Reset local transform position", priority = 100 * (int)LetterAsInteger.S + (int)LetterAsInteger.R)]
        public static void ResetLocalTransformPosition()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;

            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                if (!selectedGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

                Undo.RecordObject(selectedGameObject.transform, "Reset local transform position");
                selectedGameObject.transform.localPosition = Vector3.zero;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }


        /// <summary>
        /// Reverses the sibling order of the children of selected game objects in the hierarchy.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Reverse sibling order", priority = (100 * (int)LetterAsInteger.S) + (int)LetterAsInteger.R)]
        public static void ReverseSiblingOrder()
        {
            foreach (GameObject parentGameObject in Selection.gameObjects)
            {
                if (!parentGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

                int count = parentGameObject.transform.childCount;

                for (int i = 0; i < count / 2; i++)
                {
                    var child = parentGameObject.transform.GetChild(i);
                    child.SetSiblingIndex(count - i - 1);
                }
            }
        }


        /// <summary>
        /// Select children of selected game objects while deselecting the current selection.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Select children", priority = 100 * (int)LetterAsInteger.S + (int)LetterAsInteger.S)]
        public static void SelectChildren()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            List<GameObject> childGameObjects = new();

            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                if (!selectedGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

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
        [MenuItem("Editor Toolbox/Scene/Select parents", priority = 100 * (int)LetterAsInteger.S + (int)LetterAsInteger.S)]
        public static void SelectParents()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            List<GameObject> parentGameObjects = new();

            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                if (!selectedGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Retuning.");
                    return;
                }

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

        /// <summary>
        /// Sets the size of the RectTransform of each selected GameObject to match the dimensions of the Image's sprite.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Set rect to image size", priority = (100 * (int)LetterAsInteger.S) + (int)LetterAsInteger.S)]
        public static void SetRectToImageSize()
        {
            foreach (GameObject selectedGameObject in Selection.gameObjects)
            {
                if (!selectedGameObject.scene.isLoaded)
                {
                    Debug.LogWarning("This method only works for scene objects. Returning.");
                    return;
                }

                Image image = selectedGameObject.GetComponent<Image>();
                RectTransform rectTransform = selectedGameObject.GetComponent<RectTransform>();

                if (image != null && rectTransform != null)
                {
                    Sprite sprite = image.sprite;
                    if (sprite != null)
                    {
                        Vector2 pixelSize = sprite.rect.size;
                        rectTransform.sizeDelta = pixelSize;
                    }
                }
            }
        }

        /// <summary>
        /// Toggles the active status of selected GameObjects in the hierarchy.
        /// </summary>
        [MenuItem("Editor Toolbox/Scene/Toggle active status", priority = 100 * (int)LetterAsInteger.S + (int)LetterAsInteger.T)]
        public static void ToggleActiveStatus()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;

            if (selectedGameObjects.Length > 0)
            {
                if (selectedGameObjects[0].scene.name == null)
                {
                    return;
                }

                bool isActive = selectedGameObjects[0].activeSelf;

                foreach (GameObject selectedGameObject in selectedGameObjects)
                {
                    if (!selectedGameObject.scene.isLoaded)
                    {
                        Debug.LogWarning("This method only works for scene objects. Retuning.");
                        return;
                    }

                    Undo.RecordObject(selectedGameObject, "Toggle Active Status");
                    selectedGameObject.SetActive(!isActive);
                }

                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        #endregion

        #region Asset

        /// <summary>
        /// Remove missing scripts from selected GameObjects and Prefabs within the scene hierarchy or project window.
        /// </summary>
        [MenuItem("Editor Toolbox/Asset/Remove missing scripts", priority = (100 * (int)LetterAsInteger.O) + (int)LetterAsInteger.R)]
        public static void RemoveMissingScripts()
        {
            Object[] collectedDeepHierarchy = EditorUtility.CollectDeepHierarchy(Selection.gameObjects);

            int removedComponentsCounter = 0;
            int gameobjectsAffectedCounter = 0;

            foreach (Object targetObject in collectedDeepHierarchy)
            {
                if (targetObject is GameObject gameObject)
                {
                    int amountOfMissingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);

                    if (amountOfMissingScripts > 0)
                    {
                        Undo.RegisterCompleteObjectUndo(gameObject, "Removing missing scripts");
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                        removedComponentsCounter += amountOfMissingScripts;
                        gameobjectsAffectedCounter++;
                    }
                }
            }

            Debug.Log("Removed " + removedComponentsCounter + " missing scripts from " + gameobjectsAffectedCounter + " game objects");
        }

        #endregion
    }
}

