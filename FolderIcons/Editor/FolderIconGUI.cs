using UnityEditor;
using UnityEngine;

namespace FolderIcons
{
    /// <summary>
    /// GUI Methods for Folder Icons.
    /// </summary>
    public static class FolderIconGUI
    {
        /// <summary>
        /// Draw the folder preview.
        /// </summary>
        /// <param name="rect">Rect to draw preview.</param>
        /// <param name="folder">The folder texture.</param>
        /// <param name="overlay">The overlay texture.</param>
        public static void DrawFolderPreview(Rect rect, Texture folder, Texture overlay)
        {
            if (folder != null)
            {
                GUI.DrawTexture (rect, folder, ScaleMode.ScaleToFit);
            }
       
            if (overlay != null)
            {
                GUI.DrawTexture (GetOverlayRect(rect), overlay, ScaleMode.ScaleToFit);
            }
        }

        public static void DrawCustomFolder(Rect rect, FolderIconSettings settings, FolderIconSettings.FolderIcon icon)
        {
            Texture2D folderIcon = icon.folderIcon;
            Texture2D overlayIcon = icon.overlayIcon;

            rect = GetFolderRect (rect);

            if (settings.showCustomFolder && folderIcon)
            {
                DrawFolderTexture (rect, folderIcon);
            }

            if (settings.showOverlay && overlayIcon)
            {
                DrawOverlayTexture (rect, overlayIcon);
            }
        }

        /// <summary>
        /// Draw the folder texture.
        /// </summary>
        /// <param name="rect">The rect for the folder texture.</param>
        /// <param name="folder">The texture to draw.</param>
        public static void DrawFolderTexture(Rect rect, Texture folder)
        {
            if (folder == null)
            {
                return;
            }

            EditorGUI.DrawRect (rect, FolderIconConstants.BackgroundColour);
            GUI.DrawTexture (rect, folder, ScaleMode.ScaleAndCrop);
        }

        /// <summary>
        /// Draw the folder overlay texture, given the folder rect.
        /// </summary>
        /// <param name="rect">The original rect of the folder.</param>
        /// <param name="overlay">The overlay texture to draw.</param>
        public static void DrawOverlayTexture(Rect rect, Texture overlay)
        {
            if (overlay == null)
            {
                return;
            }

            GUI.DrawTexture (GetOverlayRect(rect), overlay);
        }

        /// <summary>
        /// Check if the given rect is part of the project sidevew.
        /// </summary>
        /// <param name="rect">The rect to check.</param>
        public static bool IsSideView(Rect rect)
        {
#if UNITY_2019_3_OR_NEWER
            return rect.x != 14;
#else
            return rect.x != 13;
#endif
        }

        /// <summary>
        /// Check if the given rect is in a project treeview.
        /// </summary>
        /// <param name="rect">The rect to check.</param>
        public static bool IsTreeView(Rect rect)
        {
            return rect.width > rect.height;
        }

        private static Rect GetOverlayRect(Rect rect)
        {
            //Half size of overlay, and reposition to center
            rect.size *= 0.5f;
            rect.position += rect.size * 0.5f;

            return rect;
        }

        private static Rect GetFolderRect(Rect rect)
        {
            bool isTreeView = rect.width > rect.height;
            bool isSideView = IsSideView (rect);

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

            return rect;
        }
    }
}