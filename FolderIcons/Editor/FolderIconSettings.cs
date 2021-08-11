using System;
using UnityEditor;
using UnityEngine;

namespace FolderIcons
{
    [CreateAssetMenu (fileName = "Folder Icon Manager", menuName = "Scriptables/Others/Folder Manager")]
    public class FolderIconSettings : ScriptableObject
    {
        [Serializable]
        public class FolderIcon
        {
            public DefaultAsset folder;

            public Texture2D folderIcon;
            public Texture2D overlayIcon;
        }

        //Global Settings
        public bool showOverlay = true;

        public bool showCustomFolder = true;

        public FolderIcon[] icons = new FolderIcon[0];
    }
}