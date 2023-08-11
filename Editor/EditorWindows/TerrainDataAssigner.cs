using UnityEngine;
using UnityEditor;

namespace EditorToolbox
{
    /// <summary>
    /// Choose a TerrainData asset and assigns it to the currently selected game object's Terrain component.
    /// This script also makes a call to terrain.terrainData.SyncHeightmap().
    /// </summary>
    public class AssignTerrainDataAndSyncHeightmap : EditorWindow
    {
        private TerrainData terrainDataToAssign;

        [MenuItem("Editor Toolbox/Terrain/Terrain data assigner", priority = (100 * (int)LetterAsInteger.T) + (int)LetterAsInteger.T)]
        public static void ShowWindow()
        {
            GetWindow(typeof(AssignTerrainDataAndSyncHeightmap));
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Assign TerrainData Asset");

            terrainDataToAssign = (TerrainData)EditorGUILayout.ObjectField("TerrainData", terrainDataToAssign, typeof(TerrainData), false);

            if (GUILayout.Button("Assign TerrainData"))
            {
                if (terrainDataToAssign == null)
                {
                    Debug.LogWarning("Select a TerrainData asset to assign.");
                    return;
                }

                if (Selection.activeGameObject == null)
                {
                    Debug.LogWarning("No game object selected.");
                    return;
                }

                if (!Selection.activeGameObject.TryGetComponent<Terrain>(out var terrain))
                {
                    Debug.LogError("Selected game object does not have a Terrain component.");
                    return;
                }

                terrain.terrainData = terrainDataToAssign;
                terrain.terrainData.SyncHeightmap();

                Debug.Log("TerrainData assigned to Terrain, heightmap synchronized.");
            }
        }
    }
}

