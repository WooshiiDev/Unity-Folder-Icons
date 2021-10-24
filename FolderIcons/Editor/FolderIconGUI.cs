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
        /// Draw the folder preview
        /// </summary>
        /// <param name="rect">Rect to draw preview</param>
        /// <param name="folder">The folder texture</param>
        /// <param name="overlay">The overlay texture</param>
        public static void DrawFolderPreview(Rect rect, Texture folder, Texture overlay)
        {
            if (folder == null && overlay == null)
            {
                return;
            }

            if (folder != null)
            {
                GUI.DrawTexture (rect, folder, ScaleMode.ScaleToFit);
            }

            //Half size of overlay, and reposition to center
            rect.size *= 0.5f;
            rect.position += rect.size * 0.5f;

            if (overlay != null)
            {
                GUI.DrawTexture (rect, overlay, ScaleMode.ScaleToFit);
            }
        }

        /// <summary>
        /// Draw the folder texture and background rect if required
        /// </summary>
        /// <param name="rect">Folder rect</param>
        /// <param name="folder">Folder texture</param>
        /// <param name="guid">The guid of the project fodler</param>
        public static void DrawFolderTexture(Rect rect, Texture folder, string guid)
        {
            if (folder == null)
            {
                return;
            }

            EditorGUI.DrawRect (rect, FolderIconConstants.BackgroundColour);
            GUI.DrawTexture (rect, folder, ScaleMode.ScaleAndCrop);
        }

        /// <summary>
        /// Draw the folder overlay texture, given the folder rect
        /// </summary>
        /// <param name="rect">Original rect of the folder</param>
        /// <param name="overlay">Overlay Texture</param>
        public static void DrawOverlayTexture(Rect rect, Texture overlay)
        {
            if (overlay == null)
            {
                return;
            }

            rect.size *= 0.5f;
            rect.position += rect.size * 0.5f;

            GUI.DrawTexture (rect, overlay);
        }

        /// <summary>
        /// Check if the current rect is the side view of folders
        /// </summary>
        /// <param name="rect">Current rect</param>
        public static bool IsSideView(Rect rect)
        {
#if UNITY_2019_3_OR_NEWER
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
        }
    }
}