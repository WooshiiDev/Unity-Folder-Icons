using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace FolderIcons
    {
    [InitializeOnLoad]
    internal static class FolderIconsReplacer
        {
        // Initialize
        private readonly static bool isValid;

        // References
        private static Object[] allFolderIcons;
        private static FolderIconSettings folderIcons;

        public static bool showFolder;
        public static bool showOverlay;

        private static Color BackgroundColour = EditorGUIUtility.isProSkin
           ? new Color32 (51, 51, 51, 255)
           : new Color32 (190, 190, 190, 255);

        private static readonly Color selectedColor = new Color (60f/255f, 92f/255f, 148f/255f);

        static FolderIconsReplacer()
            {
            CheckPreferences ();

            isValid = ValidateFiles ();

            if (isValid)
                folderIcons = allFolderIcons[0] as FolderIconSettings;

            EditorApplication.projectWindowItemOnGUI -= ReplaceFolders;
            EditorApplication.projectWindowItemOnGUI += ReplaceFolders;

            }

        private static void ReplaceFolders(string guid, Rect selectionRect)
            {
            if (!isValid)
                return;

            if (!folderIcons.showCustomFolder && !folderIcons.showOverlay)
                return;

            string path = AssetDatabase.GUIDToAssetPath (guid);
            Object folderAsset = AssetDatabase.LoadAssetAtPath (path, typeof (DefaultAsset));

            if (folderAsset == null)
                return;
        
            for (int i = 0; i < folderIcons.icons.Length; i++)
                {
                var icon = folderIcons.icons[i];

                if (icon.folder == null)
                    continue;

                DrawTextures (selectionRect, icon, folderAsset, guid);
                }
            }

        private static void DrawTextures(Rect rect, FolderIconSettings.FolderIcon icon, Object folderAsset, string guid)
            {
            if (icon.folder == folderAsset)
                {
                if (showFolder && icon.folderIcon)
                    {
                    bool isSmall = rect.width > rect.height;
                    bool isTreeSide = rect.x == 44;

                    float xScale = rect.width / FolderIconConstants.MAX_PROJECT_WIDTH;
                    float yScale = rect.height / FolderIconConstants.MAX_PROJECT_HEIGHT;

                    if (isSmall)
                        {
                        rect.width = rect.height = FolderIconConstants.MAX_TREE_HEIGHT;

                        if (!isTreeSide)
                            rect.x += 3f;
                        }
                    else
                        {
                        rect.height -= 14f;
                        }

                    if (icon.folderIcon && showFolder)
                        {
                        Color rectCol = Selection.assetGUIDs.Contains (guid)
                            ? selectedColor
                            : BackgroundColour;

                        Texture2D folderIcon = icon.folderIcon;

                        EditorGUI.DrawRect (rect, rectCol);
                        GUI.DrawTexture (rect, folderIcon, ScaleMode.ScaleAndCrop);
                        }
                    }

                if (icon.overlayIcon && showOverlay)
                    {
                    rect.width *= 0.5f;
                    rect.height *= 0.5f;

                    rect.x += rect.width * 0.5f;
                    rect.y += rect.height * 0.5f;

                    Texture2D overlayIcon = icon.overlayIcon;
                    GUI.DrawTexture (rect, overlayIcon);
                    }
                }
            }

        #region Initialize 

        private static void CheckPreferences()
            {
            string prefFolder = FolderIconConstants.FOLDER_TEXTURE_PATH;
            string prefIcon = FolderIconConstants.ICONS_TEXTURE_PATH;

            if (!EditorPrefs.HasKey (prefFolder))
                EditorPrefs.SetBool (prefFolder, true);

            if (!EditorPrefs.HasKey (prefIcon))
                EditorPrefs.SetBool (prefIcon, true);

            showFolder = EditorPrefs.GetBool (prefFolder);
            showOverlay = EditorPrefs.GetBool (prefIcon);
            }

        private static bool ValidateFiles()
            {
            string folderTexturePath = FolderIconConstants.FOLDER_TEXTURE_PATH;
            string iconTexturePath = FolderIconConstants.ICONS_TEXTURE_PATH;

            bool isValid = FindOrCreateFolder (folderTexturePath, "Folders");

            if (!isValid)
                Debug.LogWarning ("FolderPlus could not create texture folder at " + folderTexturePath);

            isValid &= FindOrCreateFolder (iconTexturePath, "Icons");

            if (!isValid)
                Debug.LogWarning ("FolderPlus could not create texture folder at " + iconTexturePath);

            // Does the folder asset exist at all?
            allFolderIcons = GetAllInstances<FolderIconSettings> ();
            
            isValid &= allFolderIcons.Length > 0;

            if (!isValid)
                {
                Debug.LogWarning ("Cannot find FolderPlus Settings Asset");
                Debug.LogError ("FolderPlus is missing some assets or folders. Please check the Console for more info.");
                }

            return isValid;
            }

        private static bool FindOrCreateFolder(string path, string folderCreateName)
            {
            if (AssetDatabase.IsValidFolder (path))
                return true;

            string parentFolder = path.Substring (0, path.LastIndexOf ('/'));
            return AssetDatabase.CreateFolder(parentFolder, folderCreateName) != "";
            }

        private static T[] GetAllInstances<T>() where T : Object
            {
            string[] guids = AssetDatabase.FindAssets ("t:" + typeof (T).Name); 

            T[] instances = new T[guids.Length];

            //probably could get optimized 
            for (int i = 0; i < guids.Length; i++)
                {
                string path = AssetDatabase.GUIDToAssetPath (guids[i]);
                instances[i] = AssetDatabase.LoadAssetAtPath<T> (path);
                }


            return instances;

            }

        #endregion
        }
    }
