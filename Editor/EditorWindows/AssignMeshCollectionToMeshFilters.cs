using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace EditorToolbox
{
    /// <summary>
    /// Assigns meshes to corresponding mesh filters by matching prefab and mesh prefixes in Unity and Blender.
    /// This assumes that prefabs in Unity begin with "PF_" and meshes in Blender with "M_". 
    /// For each mesh in an FBX file referenced by a Model Importer,
    ///   If a mesh with name M_MeshObjectName matches a prefab named PF_MeshObjectName,
    ///   Then the mesh is assigned to the mesh filter on that prefab.
    /// Prefabs and meshes must follow the naming conventions, and any deviation results in an error.
    /// </summary>
    public class AssignMeshCollectionToMeshFilters : EditorWindow
    {
        public string meshCollectionPath;
        public List<GameObject> prefabs;

        [MenuItem("Editor Toolbox/Asset/Assign mesh to mesh filters", priority = (100 * (int)LetterAsInteger.A) + (int)LetterAsInteger.A)]
        [MenuItem("GameObject/Editor Toolbox/Asset/Assign mesh to mesh filters", priority = (100 * (int)LetterAsInteger.A) + (int)LetterAsInteger.A)]
        public static void ShowWindow()
        {
            GetWindow<AssignMeshCollectionToMeshFilters>("Assign Mesh To Mesh Filters");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Mesh Collection Path:", EditorStyles.boldLabel);
            meshCollectionPath = EditorGUILayout.TextField(meshCollectionPath);

            EditorGUILayout.LabelField("Prefabs:", EditorStyles.boldLabel);
            SerializedObject serializedObject = new(this);
            SerializedProperty prefabsProperty = serializedObject.FindProperty("prefabs");
            EditorGUILayout.PropertyField(prefabsProperty, true);

            if (GUILayout.Button("Assign"))
            {
                AssignMeshes();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void AssignMeshes()
        {
            ModelImporter modelImporter = AssetImporter.GetAtPath(meshCollectionPath) as ModelImporter;

            if (modelImporter != null)
            {
                Mesh[] meshArray = AssetDatabase.LoadAllAssetsAtPath(meshCollectionPath)
                                                 .OfType<Mesh>()
                                                 .ToArray();

                Dictionary<string, GameObject> objectNameToPrefab = new();

                foreach (GameObject prefab in prefabs)
                {
                    if (prefab.name.StartsWith("PF_"))
                    {
                        objectNameToPrefab[prefab.name[3..]] = prefab;
                    }
                    else
                    {
                        Debug.LogError("Prefab without 'PF_' prefix found: " + prefab.name);
                        return;
                    }
                }

                foreach (Mesh mesh in meshArray)
                {
                    if (!mesh.name.StartsWith("M_"))
                    {
                        Debug.LogError("Mesh without 'M_' prefix found: " + mesh.name);
                        return;
                    }

                    string meshName = mesh.name[2..];

                    if (objectNameToPrefab.TryGetValue(meshName, out GameObject matchingPrefab))
                    {
                        
                        if (matchingPrefab.TryGetComponent<MeshFilter>(out var meshFilter))
                        {
                            meshFilter.sharedMesh = mesh;
                            EditorUtility.SetDirty(matchingPrefab);
                        }
                    }
                    else
                    {
                        Debug.Log("The following mesh was not matched with a prefab: " + meshName);
                    }
                }
            }
            else
            {
                Debug.LogError("Invalid mesh collection path.");
            }
        }
    }
}
