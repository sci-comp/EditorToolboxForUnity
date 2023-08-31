using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditorToolbox
{
    public class GenerateX11SwatchLibrary : EditorWindow
    {
        [MenuItem("Editor Toolbox/Utility/Generate X11 swatch library")]
        public static void ShowWindow()
        {
            GetWindow<GenerateX11SwatchLibrary>("Generate X11 swatch library");
        }

        [SerializeField] string swatchLibraryName = "X11SwatchLibrary";

        private void OnGUI()
        {
            swatchLibraryName = EditorGUILayout.TextField("Swatch Name:", swatchLibraryName);
            string filePath = "./Assets/Editor/" + swatchLibraryName + ".colors";

            if (GUILayout.Button("Generate Swatch File"))
            {
                GenerateSwatchFile(filePath, swatchLibraryName);
            }
        }

        private void GenerateSwatchFile(string path, string _swatchLibraryName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("%YAML 1.1");
            sb.AppendLine("%TAG !u! tag:unity3d.com,2011:");
            sb.AppendLine("--- !u!114 &1");
            sb.AppendLine("MonoBehaviour:");
            sb.AppendLine("  m_ObjectHideFlags: 52");
            sb.AppendLine("  m_CorrespondingSourceObject: {fileID: 0}");
            sb.AppendLine("  m_PrefabInstance: {fileID: 0}");
            sb.AppendLine("  m_PrefabAsset: {fileID: 0}");
            sb.AppendLine("  m_GameObject: {fileID: 0}");
            sb.AppendLine("  m_Enabled: 1");
            sb.AppendLine("  m_EditorHideFlags: 0");
            sb.AppendLine("  m_Script: {fileID: 12323, guid: 0000000000000000e000000000000000, type: 0}");
            sb.AppendLine("  m_Name: " + _swatchLibraryName);
            sb.AppendLine("  m_EditorClassIdentifier: ");
            sb.AppendLine("  m_Presets:");

            (List<string> sortedColorsNames, List<Color32> sortedColors) = SortColorsByCategory(Toolbox.Colors.Dictionary);

            for (int i = 0; i < sortedColors.Count; i++)
            {
                sb.AppendLine("  - m_Name: " + sortedColorsNames[i]);
                sb.AppendLine("    m_Color: {r: " + sortedColors[i].r / 255.0 + ", g: " + sortedColors[i].g / 255.0 + ", b: " + sortedColors[i].b / 255.0 + ", a: " + sortedColors[i].a / 255.0 + "}");
            }

            using (System.IO.StreamWriter writer = new(path, false))
            {
                writer.Write(sb.ToString());
            }
        }

        public static (float H, float S, float L) RGBToHSL(Color32 color)
        {
            float r = color.r / 255.0f;
            float g = color.g / 255.0f;
            float b = color.b / 255.0f;

            float min = Math.Min(r, Math.Min(g, b));
            float max = Math.Max(r, Math.Max(g, b));
            float delta = max - min;

            float h = 0;
            float s = 0;
            float l = (max + min) / 2.0f;

            if (delta != 0)
            {
                s = (l < 0.5f) ? (delta / (max + min)) : (delta / (2.0f - max - min));

                if (r == max) h = (g - b) / delta;
                else if (g == max) h = 2 + (b - r) / delta;
                else if (b == max) h = 4 + (r - g) / delta;

                h *= 60;
                if (h < 0) h += 360;
            }

            return (h, s, l);
        }

        public static (List<string>, List<Color32>) SortColorsByCategory(Dictionary<string, Color32> colorDictionary)
        {
            List<(string, Color32)> sortedColors = new List<(string, Color32)>();

            foreach (var (name, color) in colorDictionary)
            {
                sortedColors.Add((name, color));
            }

            sortedColors.Sort((a, b) =>
            {
                var hslA = RGBToHSL(a.Item2);
                var hslB = RGBToHSL(b.Item2);
                int hCompare = hslA.H.CompareTo(hslB.H);
                if (hCompare != 0) return hCompare;

                int sCompare = hslA.S.CompareTo(hslB.S);
                if (sCompare != 0) return sCompare;

                return hslA.L.CompareTo(hslB.L);
            });

            List<string> sortedColorNames = new List<string>();
            List<Color32> sortedColorList = new List<Color32>();

            foreach (var (name, color) in sortedColors)
            {
                sortedColorNames.Add(name);
                sortedColorList.Add(color);
            }

            Debug.Log("Dictionary.Count: " + Toolbox.Colors.Dictionary.Count);
            Debug.Log("sortedColorList.Count: " + sortedColorList.Count);

            return (sortedColorNames, sortedColorList);
        }


    }
}

