using UnityEditor;
using UnityEngine;

namespace FolderIcons
{
    [InitializeOnLoad]
    internal static class FolderIconsReplacer
    {
        private const string SETTINGS_TYPE_STRING = "FolderIconSettings";
        private const string SETTINGS_NAME_STRING = "Folder Icons";

        // References
        private static Object[] allFolderIcons;
        private static FolderIconSettings folderIcons;

        public static bool showFolder;
        public static bool showOverlay;

        static FolderIconsReplacer()
        {
            // Find scriptable instance or create one
            folderIcons = GetOrCreateSettings ();

            // Setup callback
            EditorApplication.projectWindowItemOnGUI -= OnFolderGUI;
            EditorApplication.projectWindowItemOnGUI += OnFolderGUI;
        }

        private static void OnFolderGUI(string guid, Rect selectionRect)
        {
            if (folderIcons == null)
            {
                folderIcons = GetOrCreateSettings ();
                return;
            }

            if (!folderIcons.showCustomFolder && !folderIcons.showOverlay)
            {
                return;
            }

            DefaultAsset folderAsset = AssetUtility.LoadAssetFromGUID<DefaultAsset> (guid);

            if (folderAsset == null)
            {
                return;
            }

            for (int i = 0; i < folderIcons.icons.Length; i++)
            {
                FolderIconSettings.FolderIcon icon = folderIcons.icons[i];

                if (icon.folder != folderAsset)
                {
                    continue;
                }

                DrawTextures (selectionRect, icon, folderAsset, guid);
            }
        }

        private static void DrawTextures(Rect rect, FolderIconSettings.FolderIcon icon, Object folderAsset, string guid)
        {
            bool isTreeView = rect.width > rect.height;
            bool isSideView = FolderIconGUI.IsSideView (rect);

            // Vertical Folder View
            if (isTreeView)
            {
                rect.width = rect.height = FolderIconConstants.MAX_TREE_HEIGHT;

                //Add small offset for correct placement
                if (!isSideView)
                {
                    rect.x += 3f;
                }
            }
            else
            {
                rect.height -= 14f;
            }

            if (showFolder && icon.folderIcon)
            {
                FolderIconGUI.DrawFolderTexture (rect, icon.folderIcon, guid);
            }

            if (showOverlay && icon.overlayIcon)
            {
                FolderIconGUI.DrawOverlayTexture (rect, icon.overlayIcon);
            }
        }

        #region Initialize

        private static FolderIconSettings GetOrCreateSettings()
        {
            string path = null;

            // Make sure the key is still valid - no assuming that settings just 'exist'
            string guidPref = FolderIconConstants.PREF_GUID;
            if (EditorPrefs.HasKey (guidPref))
            {
                if (AssetUtility.TryGetAsset(EditorPrefs.GetString (guidPref), out path))
                {
                    return AssetDatabase.LoadAssetAtPath<FolderIconSettings> (path);
                }
            }

            FolderIconSettings settings = AssetUtility.FindOrCreateScriptable<FolderIconSettings> (SETTINGS_TYPE_STRING, SETTINGS_NAME_STRING, FolderIconConstants.ASSET_DEFAULT_PATH);

            path = AssetDatabase.GetAssetPath (settings);
            EditorPrefs.SetString (guidPref, AssetDatabase.AssetPathToGUID (path));

            return settings;
        }

        #endregion Initialize
    }
}