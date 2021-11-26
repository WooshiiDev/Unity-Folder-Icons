using UnityEditor;
using UnityEngine;

namespace FolderIcons
{
    /// <summary>
    /// Contains all constant data, values and colours FolderIcon requires
    /// </summary>
    internal static class FolderIconConstants
    {
        // Settings
        public const string ASSET_DEFAULT_PATH = "Assets/";

        public const string PREF_GUID = "FP_GUID";
        public const string PREF_FOLDER = "FP_SHOW_FOLDERS";
        public const string PREF_OVERLAY = "FP_SHOW_ICONS";

        // GUI

        public const float MAX_TREE_WIDTH = 118f;
        public const float MAX_PROJECT_WIDTH = 96f;

        public const float MAX_TREE_HEIGHT = 16f;
        public const float MAX_PROJECT_HEIGHT = 110f;

        // Colours

        public static readonly Color SelectedColor = new Color (0.235f, 0.360f, 0.580f);

        public static Color BackgroundColour = EditorGUIUtility.isProSkin
          ? new Color32 (51, 51, 51, 255)
          : new Color32 (190, 190, 190, 255);
    }
}