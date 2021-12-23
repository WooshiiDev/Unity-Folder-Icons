using UnityEditor;
using UnityEngine;

namespace FolderIcons
{
    /// <summary>
    /// Contains all constant data and settings required.
    /// </summary>
    internal static class FolderIconConstants
    {
        // Settings

        /// <summary>
        /// The default path for creating <see cref="FolderIconSettings"/>.
        /// </summary>
        public const string ASSET_DEFAULT_PATH = "Assets/";

        /// <summary>
        /// The EditorPref Key for saving the last known GUID of a <see cref="FolderIconSettings"/> instance.
        /// </summary>
        public const string PREF_GUID = "FP_GUID";

        /// <summary>
        /// The EditorPref Key for showing custom folder textures.
        /// </summary>
        public const string PREF_FOLDER = "FP_SHOW_FOLDERS";

        /// <summary>
        /// The EditorPref Key for showing custom folder icons.
        /// </summary>
        public const string PREF_OVERLAY = "FP_SHOW_ICONS";

        // GUI

        /// <summary>
        /// The maximum height of a texture in the project treeview.
        /// </summary>
        public const float MAX_TREE_HEIGHT = 16f;

        /// <summary>
        /// The maximum height of a texture in the standard project view.
        /// </summary>
        public const float MAX_PROJECT_HEIGHT = 110f;

        // Colours

        public static readonly Color SelectedColor = new Color (0.235f, 0.360f, 0.580f);

        public static readonly Color SkinColor = EditorGUIUtility.isProSkin
          ? new Color32 (51, 51, 51, 255)
          : new Color32 (190, 190, 190, 255);

        // Textures

        public const string TEX_FOLDER_CLOSED = "Folder Icon";
        public const string TEX_FOLDER_OPEN = "FolderEmpty Icon";

        // Methods

        public static Color GetBackgroundColor(Object instance)
        {
            if (Selection.Contains(instance))
            {
                return SelectedColor;
            }

            return SkinColor;
        }
    }
}