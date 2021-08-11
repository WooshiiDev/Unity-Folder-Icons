using UnityEditor;
using UnityEngine;

namespace FolderIcons
{
    /// <summary>
    /// Contains all constant data, values and colours FolderIcon requires
    /// </summary>
    internal static class FolderIconConstants
    {
        public static readonly string ASSET_DEFAULT_PATH = Application.dataPath;

        // Settings
        public const string FOLDER_TEXTURE_PATH = "Assets/ColouredFolders/Folders";

        public const string ICONS_TEXTURE_PATH = "Assets/ColouredFolders/Icons";

        public const string PREF_FOLDER = "FolderPlus_ShowCustomFolders";
        public const string PREF_OVERLAY = "FolderPlus_ShowCustomIcons";

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