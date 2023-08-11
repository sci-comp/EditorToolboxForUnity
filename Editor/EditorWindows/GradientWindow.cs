using UnityEngine;
using UnityEditor;
using System.IO;

namespace EditorToolbox
{
    /// <summary>
    /// Creates a PNG image from a specified gradient with a given resolution.
    /// Allows for manual configuration of gradient properties and file path settings.
    /// Converts the gradient to a one-dimensional texture and saves it as a PNG file
    /// in the provided path with the given filename.
    /// </summary>
    public class GradientWindow : EditorWindow
    {
        private Gradient gradient = new();
        private int resolution = 256;
        private string filename = "T_Gradient_xyz";
        private string path = "/Sandbox/Gradient/";

        [MenuItem("Editor Toolbox/Utility/Gradient Window", priority = 100 * (int)LetterAsInteger.U + (int)LetterAsInteger.G)]
        public static void ShowWindow()
        {
            GetWindow<GradientWindow>(false, "Gradient to PNG", true);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Gradient to File Converter");

            gradient = EditorGUILayout.GradientField("Gradient", gradient);
            resolution = EditorGUILayout.IntField("Resolution", resolution);
            filename = EditorGUILayout.TextField("Filename", filename);
            path = EditorGUILayout.TextField("Root", path);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Convert to asset", GUILayout.Height(50)))
            {
                ConvertGradient();
            }
        }

        private void ConvertGradient()
        {
            Texture2D tex = new(resolution, 1);
            Color[] texColors = new Color[resolution];

            for (int x = 0; x < resolution; ++x)
            {
                texColors[x] = gradient.Evaluate((float)x / resolution);
            }

            tex.SetPixels(texColors);

            byte[] bytes = tex.EncodeToPNG();
            string filenameWithExtension = filename + ".png";

            try
            {
                string fullPath = Application.dataPath + path + filenameWithExtension;

                var dir = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllBytes(fullPath, bytes);
                AssetDatabase.Refresh();
            }
            catch (IOException e)
            {
                Debug.LogError(e);
            }
        }
    }
}
