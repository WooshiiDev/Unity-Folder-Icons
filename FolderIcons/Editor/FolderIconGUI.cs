using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;
using System.IO;

namespace FolderIcons
{
    /// <summary>
    /// GUI Methods for Folder Icons.
    /// </summary>
    public static class FolderIconGUI
    {
        /// <summary>
        /// Draw the folder preview
        /// </summary>
        /// <param name="rect">Rect to draw preview</param>
        /// <param name="folder">The folder texture</param>
        /// <param name="overlay">The overlay texture</param>
        public static void DrawFolderPreview(Rect rect, Texture folder, Texture overlay)
        {
            if (folder == null && overlay == null)
                return;

            if (folder != null)
                GUI.DrawTexture(rect, folder, ScaleMode.ScaleToFit);

            //Half size of overlay, and reposition to center
            rect.size *= 0.5f;
            rect.position += rect.size * 0.5f;

            if (overlay != null)
                GUI.DrawTexture(rect, overlay, ScaleMode.ScaleToFit);
        }

        /// <summary>
        /// Draw the folder texture and background rect if required
        /// </summary>
        /// <param name="rect">Folder rect</param>
        /// <param name="folder">Folder texture</param>
        /// <param name="guid">The guid of the project fodler</param>
        public static void DrawFolderTexture(Rect rect, string guid, FolderIconSettings.FolderIcon iconSettings)
        {
            if (iconSettings.folderIcon == null)
                return;

            if (Selection.assetGUIDs.Contains(guid))
            {
                if (FolderIconsReplacer.GetFolderIconSettings().showCustomFolder)
                {
                    GUI.DrawTexture(rect, iconSettings.folderIcon, ScaleMode.ScaleAndCrop, true, 0, FolderIconConstants.SelectedColor, 0, 0);
                }
                else
                {
                    Texture2D defaultTexture = Resources.Load("Folders/Folder_Default") as Texture2D;
                    GUI.DrawTexture(rect, defaultTexture, ScaleMode.ScaleAndCrop, true, 0, FolderIconConstants.SelectedColor, 0, 0);
                }
            }
            else
            {
                if (FolderIconsReplacer.GetFolderIconSettings().showCustomFolder)
                {
                    GUI.DrawTexture(rect, iconSettings.folderIcon, ScaleMode.ScaleAndCrop);
                }
                else
                {
                    Texture2D defaultTexture = Resources.Load("Folders/Folder_Default") as Texture2D;
                    GUI.DrawTexture(rect, defaultTexture, ScaleMode.ScaleAndCrop);
                }
            }
        }

        /// <summary>
        /// Draw the folder overlay texture, given the folder rect
        /// </summary>
        /// <param name="rect">Original rect of the folder</param>
        /// <param name="overlay">Overlay Texture</param>
        public static void DrawOverlayTexture(Rect rect, Texture overlay, string guid)
        {
            if (overlay == null || !FolderIconsReplacer.GetFolderIconSettings().showOverlay)
                return;

            rect.size *= 0.5f;
            rect.position += rect.size * 0.5f;

            if (Selection.assetGUIDs.Contains(guid))
            {
                GUI.DrawTexture(rect, overlay, ScaleMode.ScaleAndCrop, true, 0, FolderIconConstants.SelectedColor, 0, 0);
            }
            else
            {
                GUI.DrawTexture(rect, overlay);
            }
        }

        /// <summary>
        /// Check if the current rect is the side view of folders
        /// </summary>
        /// <param name="rect">Current rect</param>
        public static bool IsSideView(Rect rect)
        {
#if UNITY_2019_3_OR_NEWER
            // return rect.x != 14 && rect.x < 16;
            // return rect.x == 44;
            return rect.x != 14;
#else
            return rect.x != 13;
#endif
        }

        /// <summary>
        /// Check if the current rect is in tree view
        /// </summary>
        /// <param name="rect">Current rect</param>
        public static bool IsTreeView(Rect rect)
        {
            return rect.width > rect.height;
            // return rect.width == 16;
        }
    }
}
