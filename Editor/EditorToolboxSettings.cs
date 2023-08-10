using UnityEngine;

namespace EditorToolbox
{
    [CreateAssetMenu(fileName = "EditorToolboxSettings", menuName = "ScriptableObjects/EditorToolbox/Settings", order = 1)]
    public class EditorToolboxSettings : ScriptableObject
    {
        public float gridSize = 0.25f;
    }
}
