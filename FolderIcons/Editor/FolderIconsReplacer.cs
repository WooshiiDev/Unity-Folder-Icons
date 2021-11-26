﻿using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FolderIcons
{
    [InitializeOnLoad]
    internal static class FolderIconsReplacer
    {
        // References
        private static Object[] allFolderIcons;

        public static FolderIconSettings folderIcons;

        public static bool showFolder;
        public static bool showOverlay;

        private static bool selectionChanged;

        private static readonly Color selectedColor = new Color (60f / 255f, 92f / 255f, 148f / 255f);

        static FolderIconsReplacer()
        {
            CheckPreferences ();

            EditorApplication.projectWindowItemOnGUI -= ReplaceFolders;
            EditorApplication.projectWindowItemOnGUI += ReplaceFolders;

            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged()
        {
            selectionChanged = true;
        }

        private static void ReplaceFolders(string guid, Rect selectionRect)
        {
            // Does the folder asset exist at all?
            if (allFolderIcons == null)
            {
                allFolderIcons = GetAllInstances<FolderIconSettings> ();
            }

            if (folderIcons == null)
            {
                if (allFolderIcons.Length > 0)
                {
                    folderIcons = allFolderIcons[0] as FolderIconSettings;
                }
            }

            if (folderIcons == null)
            {
                return;
            }

            if (!folderIcons.showCustomFolder && !folderIcons.showOverlay)
            {
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath (guid);
            Object folderAsset = AssetDatabase.LoadAssetAtPath (path, typeof (DefaultAsset));

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

            if (selectionChanged && IsFolderSelected (out List<DefaultAsset> selectedFolders) && Event.current.alt)
            {
                FolderIconSettingsWindow.ShowWindow (selectedFolders);
            }

            selectionChanged = false;
        }

        private static bool IsFolderSelected(out List<DefaultAsset> selectedFolders)
        {
            selectedFolders = new List<DefaultAsset> ();
            foreach (System.Int32 id in Selection.instanceIDs)
            {
                System.String path = "Assets";
                path = AssetDatabase.GetAssetPath (id);
                if (path.Length > 0)
                {
                    if (Directory.Exists (path) && path != "Assets")
                    {
                        DefaultAsset folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset> (path);
                        selectedFolders.Add (folderAsset);
                    }
                }
            }
            return selectedFolders.Count == 1;
        }

        private static void DrawTextures(Rect rect, FolderIconSettings.FolderIcon icon, Object folderAsset, string guid)
        {
            bool isTreeView = rect.width > rect.height;
            bool isSideView = FolderIconGUI.IsSideView (rect);

            Rect folderRect = rect;

            // Vertical Folder View
            if (isTreeView)
            {
                folderRect.width = folderRect.height = FolderIconConstants.MAX_TREE_HEIGHT;

                //Add small offset for correct placement
                if (!isSideView)
                {
                    folderRect.x += 3f;
                }
            }
            else
            {
                folderRect.height -= 14f;
            }

            FolderLabelGUI.DrawLabel (rect, guid, icon);

            if (showFolder && icon.folderIcon)
            {
                FolderIconGUI.DrawFolderTexture (folderRect, guid, icon);
            }

            if (showOverlay && icon.overlayIcon)
            {
                FolderIconGUI.DrawOverlayTexture (folderRect, icon.overlayIcon, guid);
            }
        }

        #region Initialize

        private static void CheckPreferences()
        {
            string prefFolder = FolderIconConstants.FOLDER_TEXTURE_PATH;
            string prefIcon = FolderIconConstants.ICONS_TEXTURE_PATH;

            if (!EditorPrefs.HasKey (prefFolder))
            {
                EditorPrefs.SetBool (prefFolder, true);
            }

            if (!EditorPrefs.HasKey (prefIcon))
            {
                EditorPrefs.SetBool (prefIcon, true);
            }

            showFolder = EditorPrefs.GetBool (prefFolder);
            showOverlay = EditorPrefs.GetBool (prefIcon);
        }

        private static bool FindOrCreateFolder(string path, string folderCreateName)
        {
            if (AssetDatabase.IsValidFolder (path))
            {
                return true;
            }

            string parentFolder = path.Substring (0, path.LastIndexOf ('/'));
            return AssetDatabase.CreateFolder (parentFolder, folderCreateName) != "";
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

        #endregion Initialize

        #region Getters

        public static FolderIconSettings GetFolderIconSettings()
        {
            return folderIcons;
        }

        #endregion Getters
    }
}