using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FolderIcons
{
    public class FolderIconSettingsWindow : EditorWindow
    {
        private Vector2 scroll = new Vector2 (100, 0);

        private static FolderIconSettings.FolderIcon iconSettings;

        private static bool Drawn;
        private static bool isRegistered;

        private static List<DefaultAsset> selectedAssets;

        public static FolderIconSettingsWindow window;

        public static void ShowWindow(List<DefaultAsset> selection)
        {
            window = GetWindow<FolderIconSettingsWindow> ("Icon Settings");
            selectedAssets = selection;
            isRegistered = false;
            iconSettings = new FolderIconSettings.FolderIcon
            {
                labelColor = Color.white,
                selectionGradient = new Gradient ()
            };
            foreach (FolderIconSettings.FolderIcon icon in FolderIconsReplacer.GetFolderIconSettings ().icons)
            {
                if (icon.folder == selection[0])
                {
                    iconSettings = icon;
                    isRegistered = true;
                }
            }
            window.Show ();
        }

        private void OnGUI()
        {
            Rect rect = EditorGUILayout.GetControlRect ();
            if (FolderIconsReplacer.GetFolderIconSettings ().showCustomFolder)
            {
                iconSettings.folderIcon = (Texture2D)EditorGUILayout.ObjectField ("Folder Icon ", iconSettings.folderIcon, typeof (Texture2D), true, GUILayout.Height (EditorGUIUtility.singleLineHeight));
            }
            if (FolderIconsReplacer.GetFolderIconSettings ().showOverlay)
            {
                iconSettings.overlayIcon = (Texture2D)EditorGUILayout.ObjectField ("Overlay Icon ", iconSettings.overlayIcon, typeof (Texture2D), true, GUILayout.Height (EditorGUIUtility.singleLineHeight));
            }
            iconSettings.labelColor = EditorGUILayout.ColorField ("Label Color ", iconSettings.labelColor);
            if (!FolderIconsReplacer.GetFolderIconSettings ().useGlobalSelectionColor)
            {
                iconSettings.selectionGradient = EditorGUILayout.GradientField ("Selection Gradient", iconSettings.selectionGradient);
            }

            GUILayout.BeginHorizontal ();
            Rect buttonsRect = EditorGUILayout.GetControlRect ();
            Rect cancelRect = new Rect (buttonsRect.x, buttonsRect.y, buttonsRect.width / 2, buttonsRect.height);
            Rect addRect = new Rect (buttonsRect.x + buttonsRect.width / 2, buttonsRect.y, buttonsRect.width / 2, buttonsRect.height);
            if (GUI.Button (cancelRect, "Cancel"))
            {
                iconSettings = new FolderIconSettings.FolderIcon ();
                window.Close ();
            }
            if (isRegistered ? GUI.Button (addRect, "Replace") : GUI.Button (addRect, "Add"))
            {
                iconSettings.folder = selectedAssets[0];
                if (isRegistered)
                {
                    for (int i = 0; i < FolderIconsReplacer.GetFolderIconSettings ().icons.Length; i++)
                    {
                        FolderIconSettings.FolderIcon icon = FolderIconsReplacer.GetFolderIconSettings ().icons[i];
                        if (icon.folder == iconSettings.folder)
                        {
                            FolderIconsReplacer.folderIcons.icons[i] = iconSettings;
                        }
                    }
                }
                else
                {
                    FolderIconSettings.FolderIcon[] icons = new FolderIconSettings.FolderIcon[1] { iconSettings };
                    FolderIconsReplacer.folderIcons.icons = FolderIconsReplacer.folderIcons.icons.Concat (icons).ToArray ();
                }
                window.Close ();
            }
            GUILayout.EndHorizontal ();

            if (isRegistered)
            {
                if (GUI.Button (EditorGUILayout.GetControlRect (), "Delete"))
                {
                    FolderIconsReplacer.folderIcons.icons = FolderIconsReplacer.folderIcons.icons
                                                        .Where (icon => icon.folder != iconSettings.folder)
                                                        .ToArray ();
                    window.Close ();
                }
            }
        }
    }
}