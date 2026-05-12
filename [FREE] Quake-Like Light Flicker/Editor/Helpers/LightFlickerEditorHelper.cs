namespace QuietNoize.QuakeLikeLightFlicker.Editor
{
#if UNITY_EDITOR
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Helper methods for the QuakeLike light flicker editor tools.
    /// </summary>
    public static class LightFlickerEditorHelper
    {
        /// <summary>
        /// Finds the asset folder that contains the LightFlicker resources.
        /// </summary>
        /// <returns>The asset folder path, or null if it is not found.</returns>
        public static string GetAssetFolder()
        {
            string[] allFolders = AssetDatabase.GetAllAssetPaths();

            string folder = allFolders.FirstOrDefault(p =>
                AssetDatabase.IsValidFolder(p) &&
                p.EndsWith("/[FREE] Quake-Like Light Flicker"));

            return folder;
        }

        /// <summary>
        /// Draws a thin separator line with vertical spacing.
        /// </summary>
        /// <param name="spaceSize">Spacing above and below the line.</param>
        public static void DrawSeparatorLine(int spaceSize)
        {
            EditorGUILayout.Space(spaceSize);
            Rect r = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(r, new Color(1f, 1f, 1f, 0.08f));
            EditorGUILayout.Space(spaceSize);
        }
    }
#endif
}